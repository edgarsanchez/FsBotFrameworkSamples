namespace TeamsConversationBot.Bots

open System
open System.Xml
open System.Threading.Tasks
open FSharp.Control.Tasks.V2.ContextInsensitive
open Newtonsoft.Json.Linq
open Microsoft.Extensions.Configuration
open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.Teams
open Microsoft.Bot.Schema
open Microsoft.Bot.Connector.Authentication

type TeamsConversationBot (config: IConfiguration) =
    inherit TeamsActivityHandler ()

    let appId = config.["MicrosoftAppId"]
    let appPassword = config.["MicrosoftAppPassword"]

    let mentionActivityAsync (turnContext: ITurnContext<IMessageActivity>) cancellationToken =
        let mention = Mention (
                        mentioned = turnContext.Activity.From,
                        text = sprintf "<at>%s</at>" (XmlConvert.EncodeName turnContext.Activity.From.Name) )

        let replyActivity = MessageFactory.Text (sprintf "Hello %s." mention.Text)
        replyActivity.Entities <- [| mention |]

        task { return! turnContext.SendActivityAsync (replyActivity, cancellationToken) } :> Task

    let getSingleMemberAsync turnContext cancellationToken =
        task {
            try
                let! teamMember = TeamsInfo.GetMemberAsync (turnContext, turnContext.Activity.From.Id, cancellationToken)
                let message = MessageFactory.Text (sprintf "You are: %s" teamMember.Name)
                let! _ = turnContext.SendActivityAsync message
                ()
            with
            | :? ErrorResponseException as ex ->
                if isNull ex.Body.Error then
                    let! _ = turnContext.SendActivityAsync "Couldn't get member data from Teams."
                    ()
                elif ex.Body.Error.Code = "MemberNotFoundInConversation" then
                    let! _ = turnContext.SendActivityAsync "Member not found."
                    ()
                else
                    raise ex
        } :> Task 

    let sendUpdatedCard (turnContext: ITurnContext<IMessageActivity>) (card: HeroCard) cancellationToken =
        card.Title <- "I've been updated"

        let data = JObject.FromObject turnContext.Activity.Value // :?> JObject)
        printfn "****** data: %A" data
        let newCount = data.["count"].Value<int> () + 1
        data.["count"] <- JValue (newCount)
        card.Text <- sprintf "Update count - %d" newCount

        card.Buttons.Add (
            CardAction (
                ``type`` = ActionTypes.MessageBack,
                title = "Update Card",
                text = "UpdateCardAction",
                value = data ) )

        let activity = MessageFactory.Attachment (card.ToAttachment ())
        activity.Id <- turnContext.Activity.ReplyToId
        printfn "****** Id :%s:" activity.Id

        task { return! turnContext.UpdateActivityAsync (activity, cancellationToken) }

    let sendWelcomeCard (turnContext: ITurnContext<IMessageActivity>) (card: HeroCard) cancellationToken =
        let initialValue = JObject (JProperty ("count", 0))
        card.Title <- "Welcome!"
        card.Buttons.Add (
            CardAction (
                ``type`` = ActionTypes.MessageBack,
                title = "Update Card",
                text = "UpdateCardAction",
                value = initialValue ) )

        let activity = MessageFactory.Attachment (card.ToAttachment ())

        task { return! turnContext.SendActivityAsync (activity, cancellationToken) }

    let cardActivityAsync turnContext update cancellationToken =
        let card = HeroCard (
                        buttons = ResizeArray ( [|
                            CardAction (
                                ``type`` = ActionTypes.MessageBack,
                                 title = "Message all members",
                                 text = "MessageAllMembers" )
                            CardAction (
                                ``type`` = ActionTypes.MessageBack,
                                title = "Who am I?",
                                text = "whoami" )
                            CardAction (
                                ``type`` = ActionTypes.MessageBack,
                                title = "Delete card?",
                                text = "Delete" ) |] ) )

        task { return!
            if update then
                sendUpdatedCard turnContext card cancellationToken
            else
                sendWelcomeCard turnContext card cancellationToken 
        } :> Task

    let getPagedMembers turnContext cancellationToken =
        let rec processPage continuationToken members =
            task {
                let! currentPage = TeamsInfo.GetPagedMembersAsync (turnContext, Nullable (100), continuationToken, cancellationToken)
                let moreMembers = List.append (Seq.toList currentPage.Members) members
                match currentPage.ContinuationToken with
                | null  -> return moreMembers
                | token -> return! processPage token moreMembers 
            }

        processPage null []

    // If you encounter permission-related errors when sending this message, see
    // https://aka.ms/BotTrustServiceUrl
    let messageAllMembersAsync (turnContext: ITurnContext<IMessageActivity>) cancellationToken =
        let teamsChannelId = turnContext.Activity.TeamsGetChannelId ()
        let serviceUrl = turnContext.Activity.ServiceUrl
        let credentials = MicrosoftAppCredentials (appId, appPassword)

        task {
            let! members = getPagedMembers turnContext cancellationToken

            for teamMember in members do
                let proactiveMessage = 
                    MessageFactory.Text (
                        sprintf "Hello %s %s. I'm a Teams conversation bot." teamMember.GivenName teamMember.Surname )

                let conversationParameters = 
                    ConversationParameters (
                        isGroup = Nullable (false),
                        bot = turnContext.Activity.Recipient,
                        members = [| teamMember |],
                        tenantId = turnContext.Activity.Conversation.TenantId )

                do! (turnContext.Adapter :?> BotFrameworkAdapter)
                        .CreateConversationAsync (
                            teamsChannelId,
                            serviceUrl,
                            credentials,
                            conversationParameters,
                            BotCallbackHandler (fun t1 _ ->
                                upcast task {
                                    do! (turnContext.Adapter :?> BotFrameworkAdapter)
                                            .ContinueConversationAsync (
                                                appId,
                                                t1.Activity.GetConversationReference (),
                                                BotCallbackHandler (fun t2 c2 ->
                                                    upcast task { return! t2.SendActivityAsync (proactiveMessage, c2) } ),
                                                cancellationToken ) 
                                } ),
                            cancellationToken )

            return! turnContext.SendActivityAsync (MessageFactory.Text "All messages have been sent.", cancellationToken) 
        } :> Task

    let deleteCardActivityAsync (turnContext: ITurnContext<IMessageActivity>) cancellationToken =
        task { return! turnContext.DeleteActivityAsync (turnContext.Activity.ReplyToId, cancellationToken) } :> Task

    override __.OnMessageActivityAsync (turnContext, cancellationToken) =
        turnContext.Activity.RemoveRecipientMention () |> ignore
        let text = turnContext.Activity.Text.Trim().ToLower ()

        upcast task { do!
            if text.Contains "mention" then
                mentionActivityAsync turnContext cancellationToken
            elif text.Contains "who" then
                getSingleMemberAsync turnContext cancellationToken
            elif text.Contains "update" then
                cardActivityAsync turnContext true cancellationToken
            elif text.Contains "message" then
                messageAllMembersAsync turnContext cancellationToken
            elif text.Contains "delete" then
                deleteCardActivityAsync turnContext cancellationToken
            else
                cardActivityAsync turnContext false cancellationToken 
        }

    override __.OnTeamsMembersAddedAsync (membersAdded, _, turnContext, _) =
        upcast task {
            for teamMember in membersAdded do
                let! _ = turnContext.SendActivityAsync (MessageFactory.Text (sprintf "Welcome to the team %s %s." teamMember.GivenName teamMember.Surname)) 
                ()
        }
