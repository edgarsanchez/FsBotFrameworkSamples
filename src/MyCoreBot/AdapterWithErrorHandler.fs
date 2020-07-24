namespace MyCoreBot

open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.Integration.AspNet.Core
open Microsoft.Bot.Builder.TraceExtensions
open Microsoft.Bot.Schema
open Microsoft.Extensions.Logging

type AdapterWithErrorHandler(configuration, logger, conversationState: ConversationState) as __ =
    inherit BotFrameworkHttpAdapter(configuration, logger)

    do
        __.OnTurnError <- fun turnContext exn -> 
            // Log any leaked exception from the application.
            logger.LogError(exn, sprintf "[OnTurnError] unhandled error: %s" exn.Message)

            let errorMessageText = "The bot encountered an error or bug."
            let errorMessage = MessageFactory.Text(errorMessageText, errorMessageText, InputHints.ExpectingInput)

            let errorMessageText' = "To continue to run this bot, please fix the bot source code."
            let errorMessage' = MessageFactory.Text(errorMessageText', errorMessageText', InputHints.ExpectingInput)

            upcast task {
                // Send a message to the user
                let! _ = turnContext.SendActivityAsync errorMessage
                let! _ = turnContext.SendActivityAsync errorMessage'

                if not (isNull conversationState) then
                    try
                        // Delete the conversationState for the current conversation to prevent the
                        // bot from getting stuck in a error-loop caused by being in a bad state.
                        // ConversationState should be thought of as similar to "cookie-state" in a Web pages.
                        do! conversationState.DeleteAsync turnContext
                    with ex ->
                        logger.LogError (ex, sprintf "Exception caught on attempting to Delete ConversationState: %s" ex.Message)

                // Send a trace activity, which will be displayed in the Bot Framework Emulator
                return! turnContext.TraceActivityAsync("OnTurnError Trace", exn.Message, "https://www.botframework.com/schemas/error", "TurnError")
            }
