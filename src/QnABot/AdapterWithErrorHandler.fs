namespace QnABot

open System.Runtime.InteropServices
open FSharp.Control.Tasks
open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.Integration.AspNet.Core
open Microsoft.Bot.Builder.TraceExtensions
open Microsoft.Extensions.Logging

type AdapterWithErrorHandler(configuration, logger, [< Optional; DefaultParameterValue (null: ConversationState) >] conversationState: ConversationState) as __ =
    inherit BotFrameworkHttpAdapter(configuration, logger)

    do
        __.OnTurnError <- fun turnContext exn -> 
            // Log any leaked exception from the application.
            // NOTE: In production environment, you should consider logging this to
            // Azure Application Insights. Visit https://aka.ms/bottelemetry to see how
            // to add telemetry capture to your bot.
            logger.LogError(exn, $"[OnTurnError] unhandled error: {exn.Message}")

            unitTask {
                // Send a message to the user
                let! _ = turnContext.SendActivityAsync "The bot encountered an error or bug."
                let! _ = turnContext.SendActivityAsync "To continue to run this bot, please fix the bot source code."

                if not (isNull conversationState) then
                    try
                        // Delete the conversationState for the current conversation to prevent the
                        // bot from getting stuck in a error-loop caused by being in a bad state.
                        // ConversationState should be thought of as similar to "cookie-state" in a Web pages.
                        do! conversationState.DeleteAsync turnContext
                    with
                    | e ->
                        logger.LogError(e, $"Exception caught on attempting to Delete ConversationState : {e.Message}")

                // Send a trace activity, which will be displayed in the Bot Framework Emulator
                return! turnContext.TraceActivityAsync("OnTurnError Trace", exn.Message, "https://www.botframework.com/schemas/error", "TurnError")
            }
