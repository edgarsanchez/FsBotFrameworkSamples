namespace MyCoreBot

open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.AI.Luis
open Newtonsoft.Json
open System.Collections.Generic
open System.Linq

// Intent is an enum (hence the int values) instead of discriminated unions
// So that Newtonsoft.Json can properly deserialize it
type Intent = BookFlight = 0 | Cancel = 1 | GetWeather = 2 | None = 3

// Composites
type _InstanceFrom = { Airport: InstanceData array }

type FromClass = { Airport: string array array; [<JsonProperty("$instance")>] _instance: _InstanceFrom }

type _InstanceTo = { Airport: InstanceData array }

type ToClass = { Airport: string array array; [<JsonProperty("$instance")>] _instance: _InstanceTo }

// Instance
type _Instance = { 
    // Built-in entities
    datetime: InstanceData array
    // Lists
    Airport: InstanceData array
    From: InstanceData array
    To: InstanceData array }

type _Entities = {
    datetime : DateTimeSpec array
    Airport: string array array
    From: FromClass array
    To: ToClass array
    [<JsonProperty("$instance")>]
    _instance: _Instance }

type FlightBooking () =
    [<DefaultValue>]
    val mutable Text: string
    [<DefaultValue>]
    val mutable AlteredText: string
    [<DefaultValue>]
    val mutable Intents: Dictionary<Intent, IntentScore>
    [<DefaultValue>]
    val mutable Entities: _Entities

    [<JsonExtensionData(ReadData = true, WriteData = true)>]
    member val Properties: IDictionary<string, obj> = null with get, set

    // TODO: Convert the for loop into a functional max or fold 
    member __.TopIntent () =
        __.Intents.ToArray()
        |> Array.fold
            (fun (maxSoFar: {| intent: Intent; score: float |}) entry ->
                if entry.Value.Score.HasValue && entry.Value.Score.Value > maxSoFar.score then
                    {| intent = entry.Key; score = entry.Value.Score.Value |}
                else
                    maxSoFar )  
            {| intent = Intent.None; score = 0.0 |}
                
    interface IRecognizerConvert with
        member __.Convert result =
            let app = JsonConvert.DeserializeObject<FlightBooking>(
                        JsonConvert.SerializeObject (
                            result, 
                            JsonSerializerSettings(NullValueHandling = NullValueHandling.Ignore) ) )
            __.Text <- app.Text
            __.AlteredText <- app.AlteredText
            __.Intents <- app.Intents
            __.Entities <- app.Entities
            __.Properties <- app.Properties

    // Extends the FlightBooking class with methods and properties that simplify accessing entities in the luis results

    let nullOrFirst (extract: 'T -> string) (data: IEnumerable<'T>) =
        if isNull data then
            null
        else
            match data.FirstOrDefault() with
            | null -> null
            | first -> extract first

    member __.FromEntities =
        let fromValue =
            if isNull (box __.Entities) || isNull (box __.Entities._instance) then
                null
            else
                __.Entities._instance.From |> nullOrFirst (fun x -> x.Text)  
        let fromAirportValue =
            if isNull (box __.Entities) || isNull __.Entities.From then
                null
            else
                let first = __.Entities.From.FirstOrDefault ()
                if isNull (box first) then
                    null
                else
                    first.Airport |> nullOrFirst (fun a -> a.FirstOrDefault ())
        {| From = fromValue; Airport = fromAirportValue |}

    member __.ToEntities =
        let toValue =
            if isNull (box __.Entities) || isNull (box __.Entities._instance) then
                null
            else
                __.Entities._instance.To |> nullOrFirst (fun x -> x.Text)
        let toAirportValue =
            if isNull (box __.Entities) || isNull __.Entities.To then
                null
            else
                let first = __.Entities.To.FirstOrDefault ()
                if isNull (box first) then
                    null
                else
                    first.Airport |> nullOrFirst (fun a -> a.FirstOrDefault ())
        {| To = toValue; Airport = toAirportValue |}

    // This value will be a TIMEX. And we are only interested in a Date so grab the first result and drop the Time part.
    // TIMEX is a format that represents DateTime expressions that include some ambiguity. e.g. missing a Year.
    member __.TravelDate =
        if isNull (box __.Entities) then
            null
        else
            __.Entities.datetime 
            |> nullOrFirst (fun first -> first.Expressions |> nullOrFirst (fun exp -> exp.Split('T').[0]))
