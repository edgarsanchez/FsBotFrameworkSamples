namespace TeamsMessagingExtensionsAction.Bots

open System
open FSharp.Control.Tasks
open Microsoft.Bot.Builder.Teams
open Microsoft.Bot.Schema
open Microsoft.Bot.Schema.Teams
open Newtonsoft.Json.Linq

type CreateCardData = { Title: string; Subtitle: string; Text: string }

type TeamsMessagingExtensionsActionBot () =
    inherit TeamsActivityHandler ()

    let createCardCommand _ (action: MessagingExtensionAction) =
        // The user has chosen to create a card by choosing the 'Create Card' context menu command.
        let createCardData = (action.Data :?> JObject).ToObject<CreateCardData>()
        
        let card = HeroCard(title = createCardData.Title, subtitle = createCardData.Subtitle, text = createCardData.Text)

        let attachments = ResizeArray [|
                            MessagingExtensionAttachment(
                                content = card,
                                contentType = HeroCard.ContentType,
                                preview = card.ToAttachment() ) |]
                        
        MessagingExtensionActionResponse (
            composeExtension = MessagingExtensionResult(attachmentLayout = "list", ``type`` = "result", attachments = attachments) )

    let shareMessageCommand _ (action: MessagingExtensionAction) =
        // The user has chosen to share a message by choosing the 'Share Message' context menu command.
        let displayName =
            if isNull action.MessagePayload.From || isNull action.MessagePayload.From.User then
                "Somebody"
            else
                action.MessagePayload.From.User.DisplayName
        let heroCard = HeroCard(title = $"{displayName} originally sent this message:", text = action.MessagePayload.Body.Content)

        if not (isNull action.MessagePayload.Attachments) && action.MessagePayload.Attachments.Count > 0 then
            // This sample does not add the MessagePayload Attachments.  This is left as an
            // exercise for the user.
            heroCard.Subtitle <- $"({action.MessagePayload.Attachments.Count} Attachments not included)"
        
        // This Messaging Extension example allows the user to check a box to include an image with the
        // shared message.  This demonstrates sending custom parameters along with the message payload.
        match (action.Data :?> JObject).TryGetValue "includeImage" with
        | true, jtoken ->
            if String.Equals(string jtoken, bool.TrueString, StringComparison.OrdinalIgnoreCase) then
                heroCard.Images <- ResizeArray [| CardImage(url = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQtB3AwMUeNoq4gUBGe6Ocj8kyh3bXa9ZbV7u1fVKQoyKFHdkqU") |]
        | _ ->
            () 

        MessagingExtensionActionResponse (
            composeExtension = MessagingExtensionResult (
                ``type`` = "result",
                attachmentLayout = "list",
                attachments = ResizeArray [|
                    MessagingExtensionAttachment(content = heroCard, contentType = HeroCard.ContentType, preview = heroCard.ToAttachment()) |] ) )

    override __.OnTeamsMessagingExtensionSubmitActionAsync (turnContext, action, _) =
        task { return
            match action.CommandId with
            // These commandIds are defined in the Teams App Manifest.
            | "createCard" ->
                    createCardCommand turnContext action
            | "shareMessage" ->
                    shareMessageCommand turnContext action
            | otherCommand ->
                    raise(NotImplementedException($"Invalid CommandId: {otherCommand}"))
        }
