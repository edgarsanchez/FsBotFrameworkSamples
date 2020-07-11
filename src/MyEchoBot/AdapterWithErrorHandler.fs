namespace MyEchoBot

open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.Bot.Builder.Integration.AspNet.Core
open Microsoft.Bot.Builder.TraceExtensions
open Microsoft.Extensions.Logging

type AdapterWithErrorHandler(configuration, logger) as __ =
    inherit BotFrameworkHttpAdapter(configuration, logger)

    do
        __.OnTurnError <- fun turnContext exn -> 
            // Log any leaked exception from the application.
            logger.LogError(exn, sprintf "[OnTurnError] unhandled error: %s" exn.Message)

            upcast task {
                // Send a message to the user
                let! _ = turnContext.SendActivityAsync "The bot encountered an error or bug."
                let! _ = turnContext.SendActivityAsync "To continue to run this bot, please fix the bot source code."


                // Send a trace activity, which will be displayed in the Bot Framework Emulator
                return! turnContext.TraceActivityAsync("OnTurnError Trace", exn.Message, "https://www.botframework.com/schemas/error", "TurnError")
            }
