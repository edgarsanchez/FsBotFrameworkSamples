namespace MyCoreBot.Controllers

open FSharp.Control.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.Bot.Builder.Integration.AspNet.Core

// This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
// implementation at runtime. Multiple different IBot implementations running at different endpoints can be
// achieved by specifying a more specific type for the bot constructor argument.
[<Route "api/messages"; ApiController>]
type BotController (adapter: IBotFrameworkHttpAdapter, bot) =
    inherit ControllerBase()

    [<HttpGet; HttpPost>]
    member __.PostAsync() =
        // Delegate the processing of the HTTP POST to the adapter.
        // The adapter will invoke the bot.
        unitTask { return! adapter.ProcessAsync(__.Request, __.Response, bot) }
