namespace CardsBot.Dialogs

open FSharp.Control.Tasks
open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.Dialogs
open Microsoft.Bot.Builder.Dialogs.Choices
open Microsoft.Bot.Schema
open Microsoft.Extensions.Logging

type MainDialog (logger: ILogger<MainDialog>) as __ =
    inherit ComponentDialog(nameof MainDialog)

    let getChoices () =
        ResizeArray [|
            Choice(value = "Adaptive Card", Synonyms = ResizeArray [| "adaptive" |])
            Choice(value = "Animation Card", Synonyms = ResizeArray [| "animation" |])
            Choice(value = "Audio Card", Synonyms = ResizeArray [| "audio" |])
            Choice(value = "Hero Card", Synonyms = ResizeArray [| "hero" |])
            Choice(value = "OAuth Card", Synonyms = ResizeArray [| "oauth" |])
            Choice(value = "Receipt Card", Synonyms = ResizeArray [| "receipt" |])
            Choice(value = "Signin Card", Synonyms = ResizeArray [| "signin" |])
            Choice(value = "Thumbnail Card", Synonyms = ResizeArray [| "thumbnail"; "thumb" |])
            Choice(value = "Video Card", Synonyms = ResizeArray [| "video" |])
            Choice(value = "All cards", Synonyms = ResizeArray [| "all" |])
        |]

    // 1. Prompts the user if the user is not in the middle of a dialog.
    // 2. Re-prompts the user when an invalid input is received.
    let choiceCardStepAsync (stepContext: WaterfallStepContext) cancellationToken =
        logger.LogInformation "MainDialog.ChoiceCardStepAsync"

        // Create the PromptOptions which contain the prompt and re-prompt messages.
        // PromptOptions also contains the list of choices available to the user.
        let options = PromptOptions(
                        Prompt = MessageFactory.Text "What card would you like to see? You can click or type the card name",
                        RetryPrompt = MessageFactory.Text "That was not a valid choice, please select a card or number from 1 to 9.",
                        Choices = getChoices() )

        task {
            // Prompt the user with the configured PromptOptions.
            return! stepContext.PromptAsync(nameof ChoicePrompt, options, cancellationToken)
        }

    // Send a Rich Card response to the user based on their choice.
    // This method is only called when a valid prompt response is parsed from the user's response to the ChoicePrompt.
    let showCardStepAsync (stepContext: WaterfallStepContext) cancellationToken =
        logger.LogInformation "MainDialog.ShowCardStepAsync"
        
        // Cards are sent as Attachments in the Bot Framework.
        // So we need to create a list of attachments for the reply activity.
        let attachments = ResizeArray<Attachment>()
        
        // Reply to the activity we received with an activity.
        let reply = MessageFactory.Attachment attachments
        
        // Decide which type of card(s) we are going to show the user
        match (stepContext.Result :?> FoundChoice).Value with
        | "Adaptive Card" ->
            // Display an Adaptive Card
            reply.Attachments.Add(Cards.createAdaptiveCardAttachment())
        | "Animation Card" ->
            // Display an AnimationCard.
            reply.Attachments.Add(Cards.getAnimationCard().ToAttachment())
        | "Audio Card" ->
            // Display an AudioCard
            reply.Attachments.Add(Cards.getAudioCard().ToAttachment())
        | "Hero Card" ->
            // Display a HeroCard.
            reply.Attachments.Add(Cards.getHeroCard().ToAttachment())
        | "OAuth Card" ->
            // Display an OAuthCard
            reply.Attachments.Add(Cards.getOAuthCard().ToAttachment())
        | "Receipt Card" ->
            // Display a ReceiptCard.
            reply.Attachments.Add(Cards.getReceiptCard().ToAttachment())
        | "Signin Card" ->
            // Display a SignInCard.
            reply.Attachments.Add(Cards.getSigninCard().ToAttachment())
        | "Thumbnail Card" ->
            // Display a ThumbnailCard.
            reply.Attachments.Add(Cards.getThumbnailCard().ToAttachment())
        | "Video Card" ->
            // Display a VideoCard
            reply.Attachments.Add(Cards.getVideoCard().ToAttachment())
        | _ ->
            // Display a carousel of all the rich card types.
            reply.AttachmentLayout <- AttachmentLayoutTypes.Carousel
            reply.Attachments.Add(Cards.createAdaptiveCardAttachment())
            reply.Attachments.Add(Cards.getAnimationCard().ToAttachment())
            reply.Attachments.Add(Cards.getAudioCard().ToAttachment())
            reply.Attachments.Add(Cards.getHeroCard().ToAttachment())
            reply.Attachments.Add(Cards.getOAuthCard().ToAttachment())
            reply.Attachments.Add(Cards.getReceiptCard().ToAttachment())
            reply.Attachments.Add(Cards.getSigninCard().ToAttachment())
            reply.Attachments.Add(Cards.getThumbnailCard().ToAttachment())
            reply.Attachments.Add(Cards.getVideoCard().ToAttachment())

        task {
            // Send the card(s) to the user as an attachment to the activity
            let! _ = stepContext.Context.SendActivityAsync(reply, cancellationToken)

            // Give the user instructions about what to do next
            let! _ = stepContext.Context.SendActivityAsync(MessageFactory.Text "Type anything to see another card.", cancellationToken)

            return! stepContext.EndDialogAsync()
        }

    do
        // Define the main dialog and its related components.
        __.AddDialog(ChoicePrompt(nameof ChoicePrompt))
          .AddDialog (WaterfallDialog(
                        nameof WaterfallDialog,
                        [| WaterfallStep(choiceCardStepAsync); WaterfallStep(showCardStepAsync) |] ) )

        |> ignore

        __.InitialDialogId <- nameof WaterfallDialog
