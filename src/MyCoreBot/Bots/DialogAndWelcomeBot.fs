namespace MyCoreBot.Bots

open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.Dialogs
open Microsoft.Bot.Schema
open Newtonsoft.Json
open System.IO

type DialogAndWelcomeBot<'T when 'T :> Dialog> (conversationState, userState, dialog, logger) as __ =
    inherit DialogBot<'T> (conversationState, userState, dialog, logger)

    let createAdaptiveCardAttachment () =
        let cardResourcePath = "MyCoreBot.Cards.welcomeCard.json"
        use stream = __.GetType().Assembly.GetManifestResourceStream cardResourcePath
        use reader = new StreamReader (stream)
        let adaptiveCard = reader.ReadToEnd ()
        Attachment (ContentType = "application/vnd.microsoft.card.adaptive", Content = JsonConvert.DeserializeObject adaptiveCard)

    override __.OnMembersAddedAsync (membersAdded, turnContext, cancellationToken) =
        upcast task {
            for member' in membersAdded do
                if member'.Id <> turnContext.Activity.Recipient.Id then
                    let welcomeCard = createAdaptiveCardAttachment ()
                    let response = MessageFactory.Attachment welcomeCard
                    let! _ = turnContext.SendActivityAsync (response, cancellationToken)
                    do! dialog.RunAsync (turnContext, conversationState.CreateProperty "DialogState", cancellationToken) }
