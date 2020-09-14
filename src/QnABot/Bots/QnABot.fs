namespace QnABot.Bots

open System.Net.Http
open System.Threading.Tasks
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.AI.QnA
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging

type QnABot (configuration: IConfiguration, logger: ILogger<QnABot>, httpClientFactory: IHttpClientFactory) =
    inherit ActivityHandler ()

    override __.OnMessageActivityAsync (turnContext, cancellationToken) =
        let httpClient = httpClientFactory.CreateClient ()

        let qnaMaker = QnAMaker ( 
                        QnAMakerEndpoint (
                            KnowledgeBaseId = configuration.["QnAKnowledgebaseId"],
                            EndpointKey = configuration.["QnAEndpointKey"],
                            Host = configuration.["QnAEndpointHostName"] ),
                        null,
                        httpClient )

        logger.LogInformation "Calling QnA Maker"

        let options = QnAMakerOptions (Top = 1)

        task {
            // The actual call to the QnA Maker service.
            let! response =  qnaMaker.GetAnswersAsync (turnContext, options)
            let message = MessageFactory.Text (
                            if isNull response || Array.isEmpty response then
                                "No QnA Maker answers were found."
                            else
                                (Array.head response).Answer )
            return! turnContext.SendActivityAsync (message, cancellationToken)
        } :> Task
