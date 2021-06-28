namespace MyCoreBot.Dialogs

open FSharp.Control.Tasks
open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.Dialogs
open Microsoft.Bot.Schema
open Microsoft.Recognizers.Text.DataTypes.TimexExpression
open System.Collections.Generic
open System.Threading.Tasks

type DateResolverDialog (?id) as __ =
    inherit CancelAndHelpDialog(match id with Some v -> v | None -> nameof DateResolverDialog)

    let promptMsgText = "When would you like to travel?"
    let repromptMsgText = "I'm sorry, to make your booking please enter a full travel date including Day Month and Year."

    let initialStepAsync (stepContext: WaterfallStepContext) cancellationToken =
        let timex = stepContext.Options :?> string

        let promptMessage = MessageFactory.Text(promptMsgText, promptMsgText, InputHints.ExpectingInput)
        let repromptMessage = MessageFactory.Text(repromptMsgText, repromptMsgText, InputHints.ExpectingInput)

        task { return!
            if isNull timex then
                // We were not given any date at all so prompt the user.
                stepContext.PromptAsync(
                                nameof DateTimePrompt,
                                PromptOptions(Prompt = promptMessage, RetryPrompt = repromptMessage),
                                cancellationToken )
            // We have a Date we just need to check it is unambiguous.
            elif TimexProperty(timex).Types.Contains Constants.TimexTypes.Definite then
                let dateTimeResList = List()
                dateTimeResList.Add(DateTimeResolution(Timex = timex))
                stepContext.NextAsync(dateTimeResList, cancellationToken)
            else
               // This is essentially a "reprompt" of the data we were given up front.
                stepContext.PromptAsync(nameof DateTimePrompt, PromptOptions(Prompt = repromptMessage), cancellationToken )
        }

    let finalStepAsync (stepContext: WaterfallStepContext) cancellationToken =
        let timex = (stepContext.Result :?> IList<DateTimeResolution>).[0].Timex
        task { return! stepContext.EndDialogAsync(timex, cancellationToken) }
        
    let dateTimePromptValidator (promptContext: PromptValidatorContext<IList<DateTimeResolution>>) _ =
        Task.FromResult(
            if promptContext.Recognized.Succeeded then
                // This value will be a TIMEX. And we are only interested in a Date so grab the first result and drop the Time part.
                // TIMEX is a format that represents DateTime expressions that include some ambiguity. e.g. missing a Year.
                let timex = promptContext.Recognized.Value.[0].Timex.Split('T').[0]
                // If this is a definite Date including year, month and day we are good otherwise reprompt.
                // A better solution might be to let the user know what part is actually missing.
                TimexProperty(timex).Types.Contains Constants.TimexTypes.Definite
            else
                false )

    do
        __.AddDialog(DateTimePrompt(nameof DateTimePrompt, PromptValidator(dateTimePromptValidator)))
            .AddDialog(WaterfallDialog(nameof WaterfallDialog, [| WaterfallStep(initialStepAsync); WaterfallStep(finalStepAsync) |]))
        |> ignore

        // The initial child Dialog to run.
        __.InitialDialogId <- nameof WaterfallDialog
