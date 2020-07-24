namespace MyCoreBot

open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.AI.Luis
open Microsoft.Extensions.Configuration
type LuisV3PredictionOptions = AI.LuisV3.LuisPredictionOptions  // LuisRecognizerOptionsV3 uses this specific class

type FlightBookingRecognizer (configuration: IConfiguration) =
    let _recognizer =
        let luisAppId = configuration.["LuisAppId"]
        let luisAPIKey = configuration.["LuisAPIKey"]
        let luisAPIHostName = configuration.["LuisAPIHostName"]
        let nullOrEmpty s = isNull s || String.length s <= 0

        if nullOrEmpty luisAppId || nullOrEmpty luisAPIKey || nullOrEmpty luisAPIHostName then
            None
        else
            let luisApplication = LuisApplication (luisAppId, luisAPIKey, "https://" + luisAPIHostName)

            // Set the recognizer options depending on which endpoint version you want to use.
            // More details can be found in https://docs.microsoft.com/en-gb/azure/cognitive-services/luis/luis-migration-api-v3
            let recognizerOptions = LuisRecognizerOptionsV3 (
                                        luisApplication, 
                                        PredictionOptions = LuisV3PredictionOptions (IncludeInstanceData = true) )
            Some (LuisRecognizer (recognizerOptions))

    // Returns true if luis is configured in the appsettings.json and initialized.
    member __.IsConfigured = _recognizer.IsSome

    interface IRecognizer with
        member __.RecognizeAsync (turnContext, cancellationToken) =
            _recognizer.Value.RecognizeAsync (turnContext, cancellationToken)

        member __.RecognizeAsync<'T when 'T :> IRecognizerConvert and 'T: (new: unit -> 'T)> 
                                (turnContext, cancellationToken) =
            _recognizer.Value.RecognizeAsync<'T> (turnContext, cancellationToken)
