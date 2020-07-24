namespace MyCoreBot.Dialogs

open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.Dialogs
open Microsoft.Bot.Schema
open Microsoft.Recognizers.Text.DataTypes.TimexExpression
open MyCoreBot

type BookingDialog () as __ =
    inherit CancelAndHelpDialog (nameof BookingDialog)

    let destinationStepMsgText = "Where would you like to travel to?"
    let originStepMsgText = "Where are you traveling from?"

    let destinationStepAsync (stepContext: WaterfallStepContext) cancellationToken =
        let bookingDetails = stepContext.Options :?> BookingDetails
        task { return!
            if isNull bookingDetails.Destination then
                let promptMessage = MessageFactory.Text (destinationStepMsgText, destinationStepMsgText, InputHints.ExpectingInput)
                stepContext.PromptAsync (nameof TextPrompt, PromptOptions (Prompt = promptMessage), cancellationToken)
            else
                stepContext.NextAsync (bookingDetails.Destination, cancellationToken) }

    let originStepAsync (stepContext: WaterfallStepContext) cancellationToken =
        let bookingDetails = stepContext.Options :?> BookingDetails
        bookingDetails.Destination <- stepContext.Result :?> string
        task {  return!
            if isNull bookingDetails.Origin then
                let promptMessage = MessageFactory.Text (originStepMsgText, originStepMsgText, InputHints.ExpectingInput)
                stepContext.PromptAsync (nameof TextPrompt, PromptOptions (Prompt = promptMessage), cancellationToken)
            else
                stepContext.NextAsync (bookingDetails.Origin, cancellationToken) }

    let isAmbiguous timex = not (TimexProperty(timex).Types.Contains Constants.TimexTypes.Definite)

    let travelDateStepAsync (stepContext: WaterfallStepContext) cancellationToken =
        let bookingDetails = stepContext.Options :?> BookingDetails
        bookingDetails.Origin <- stepContext.Result :?> string
        task { return!
            if isNull bookingDetails.TravelDate || isAmbiguous bookingDetails.TravelDate then
                stepContext.BeginDialogAsync (nameof DateResolverDialog, bookingDetails.TravelDate, cancellationToken)
            else
                stepContext.NextAsync (bookingDetails.TravelDate, cancellationToken) }

    let confirmStepAsync (stepContext: WaterfallStepContext) cancellationToken =
        let bookingDetails = stepContext.Options :?> BookingDetails
        bookingDetails.TravelDate <- stepContext.Result :?> string
        let messageText = sprintf "Please confirm, I have you traveling to: %s from: %s on: %s. Is this correct?"
                                                bookingDetails.Destination bookingDetails.Origin bookingDetails.TravelDate
        let promptMessage = MessageFactory.Text (messageText, messageText, InputHints.ExpectingInput)
        task { return! stepContext.PromptAsync (nameof ConfirmPrompt, PromptOptions (Prompt = promptMessage), cancellationToken) }

    let finalStepAsync (stepContext: WaterfallStepContext) cancellationToken =
        let result = if stepContext.Result :?> bool then stepContext.Options else null
        task { return! stepContext.EndDialogAsync (result, cancellationToken) }

    do
        __.AddDialog(TextPrompt (nameof TextPrompt))
            .AddDialog(ConfirmPrompt (nameof ConfirmPrompt))
            .AddDialog(DateResolverDialog ())
            .AddDialog(WaterfallDialog (nameof WaterfallDialog,
                                         [| WaterfallStep (destinationStepAsync)
                                            WaterfallStep (originStepAsync)
                                            WaterfallStep (travelDateStepAsync)
                                            WaterfallStep (confirmStepAsync)
                                            WaterfallStep (finalStepAsync) |] )) |> ignore

        // The initial child Dialog to run.
        __.InitialDialogId <- nameof WaterfallDialog
