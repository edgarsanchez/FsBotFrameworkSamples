namespace MyCoreBot.Dialogs

open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.Dialogs
open Microsoft.Bot.Schema
open Microsoft.Extensions.Logging
open Microsoft.Recognizers.Text.DataTypes.TimexExpression
open MyCoreBot
open System
open System.Threading.Tasks

type SysString = System.String

type MainDialog (luisRecognizer: FlightBookingRecognizer, bookingDialog: BookingDialog, logger: ILogger<MainDialog>) as __ =
    inherit ComponentDialog (nameof MainDialog)

    let showWarningForUnsupportedCities (context: ITurnContext) (luisResult: FlightBooking) cancellationToken =
        let unsupportedToCities =
            let toEntities = luisResult.ToEntities
            if not (SysString.IsNullOrEmpty toEntities.To) && SysString.IsNullOrEmpty toEntities.Airport then
                [ toEntities.To ]
            else
                [ ]
        let unsupportedCities =
            let fromEntities = luisResult.FromEntities
            if not (SysString.IsNullOrEmpty fromEntities.From) && SysString.IsNullOrEmpty fromEntities.Airport then
                fromEntities.From :: unsupportedToCities
            else
                unsupportedToCities

        if unsupportedCities.IsEmpty then
            Task.CompletedTask
        else
            let messageText = sprintf "Sorry but the following airports are not supported: %s" (SysString.Join (',', unsupportedCities))
            let message = MessageFactory.Text (messageText, messageText, InputHints.IgnoringInput)
            upcast task { return! context.SendActivityAsync (message, cancellationToken) }

    let introStepAsync (stepContext: WaterfallStepContext) cancellationToken =
        task {
            if luisRecognizer.IsConfigured then
                // Use the text provided in FinalStepAsync or the default if it is the first time.
                let messageText =
                    match stepContext.Options with
                    | null ->
                        "What can I help you with today?\nSay something like \"Book a flight from Paris to Berlin on March 22, 2020\""
                    | options ->
                        options.ToString () 
                let promptMessage = MessageFactory.Text (messageText, messageText, InputHints.ExpectingInput)
                return! stepContext.PromptAsync (nameof TextPrompt, PromptOptions (Prompt = promptMessage), cancellationToken)
            else
                let! _ = stepContext.Context.SendActivityAsync (
                            MessageFactory.Text ("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint = InputHints.IgnoringInput),
                            cancellationToken )
                return! stepContext.NextAsync (null, cancellationToken) }

    let actStepAsync (stepContext: WaterfallStepContext) cancellationToken =
        task {
            if luisRecognizer.IsConfigured then
                // Call LUIS and gather any potential booking details. (Note the TurnContext has the response to the prompt.)
                let! luisResult = (luisRecognizer :> IRecognizer).RecognizeAsync<FlightBooking> (stepContext.Context, cancellationToken)
                match luisResult.TopIntent().intent with
                | (*FlightBooking.*)Intent.BookFlight ->
                    do! showWarningForUnsupportedCities stepContext.Context luisResult cancellationToken

                    // Initialize BookingDetails with any entities we may have found in the response.
                    let bookingDetails = BookingDetails (
                                            // Get destination and origin from the composite entities arrays.
                                            Destination = luisResult.ToEntities.Airport,
                                            Origin = luisResult.FromEntities.Airport,
                                            TravelDate = luisResult.TravelDate )

                    // Run the BookingDialog giving it whatever details we have from the LUIS call, it will fill out the remainder.
                    return! stepContext.BeginDialogAsync (nameof BookingDialog, bookingDetails, cancellationToken)
                | (*FlightBooking.*)Intent.GetWeather ->
                    // We haven't implemented the GetWeatherDialog so we just display a TODO message.
                    let getWeatherMessageText = "TODO: get weather flow here"
                    let getWeatherMessage = MessageFactory.Text (getWeatherMessageText, getWeatherMessageText, InputHints.IgnoringInput)
                    let! _ = stepContext.Context.SendActivityAsync (getWeatherMessage, cancellationToken)
                    return! stepContext.NextAsync (null, cancellationToken)
                | _ ->
                    // Catch all for unhandled intents
                    let didntUnderstandMessageText = sprintf "Sorry, I didn't get that. Please try asking in a different way (intent was %O)" (luisResult.TopIntent().intent)
                    let didntUnderstandMessage = MessageFactory.Text (didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput)
                    let! _ = stepContext.Context.SendActivityAsync (didntUnderstandMessage, cancellationToken)
                    return! stepContext.NextAsync (null, cancellationToken)
            else
                // LUIS is not configured, we just run the BookingDialog path with an empty BookingDetailsInstance.
                return! stepContext.BeginDialogAsync (nameof BookingDialog, BookingDetails(), cancellationToken) }

    let finalStepAsync (stepContext: WaterfallStepContext) cancellationToken =
        task {
            // If the child dialog ("BookingDialog") was cancelled, the user failed to confirm or if the intent wasn't BookFlight
            // the Result here will be null.
            match stepContext.Result with
            | :? BookingDetails as result -> 
                // Now we have all the booking details call the booking service.

                // If the call to the booking service was successful tell the user.

                let timeProperty = TimexProperty (result.TravelDate)
                let travelDateMsg = timeProperty.ToNaturalLanguage DateTime.Now
                let messageText = sprintf "I have you booked to %s from %s on %s" result.Destination result.Origin travelDateMsg
                let message = MessageFactory.Text (messageText, messageText, InputHints.IgnoringInput)
                let! _ = stepContext.Context.SendActivityAsync (message, cancellationToken)
                ()
            | _ ->
                ()
            
            // Restart the main dialog with a different message the second time around
            let promptMessage = "What else can I do for you?"
            return! stepContext.ReplaceDialogAsync (__.InitialDialogId, promptMessage, cancellationToken) }

    do
        __.AddDialog(TextPrompt (nameof TextPrompt))
            .AddDialog(bookingDialog)
            .AddDialog(WaterfallDialog(nameof WaterfallDialog, 
                                         [| WaterfallStep (introStepAsync)
                                            WaterfallStep (actStepAsync)
                                            WaterfallStep (finalStepAsync) |] )) |> ignore
        __.InitialDialogId <- nameof WaterfallDialog
