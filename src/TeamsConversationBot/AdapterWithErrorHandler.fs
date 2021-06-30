namespace TeamsConversationBot

open FSharp.Control.Tasks
open Microsoft.Bot.Builder.Integration.AspNet.Core
open Microsoft.Bot.Builder.TraceExtensions
open Microsoft.Extensions.Logging

type AdapterWithErrorHandler(configuration, logger) as __ =
    inherit BotFrameworkHttpAdapter(configuration, logger)

    do
        __.OnTurnError <- fun turnContext exn -> 
            // Log any leaked exception from the application.
            // NOTE: In production environment, you should consider logging this to
            // Azure Application Insights. Visit https://aka.ms/bottelemetry to see how
            // to add telemetry capture to your bot.
            logger.LogError(exn, $"Exception caught : {exn.Message}")

            unitTask {
                // Send a catch-all apology to the user.
                let! _ = turnContext.SendActivityAsync "Sorry, it looks like something went wrong."

                // Send a trace activity, which will be displayed in the Bot Framework Emulator
                return! turnContext.TraceActivityAsync("OnTurnError Trace", exn.Message, "https://www.botframework.com/schemas/error", "TurnError")
            }
