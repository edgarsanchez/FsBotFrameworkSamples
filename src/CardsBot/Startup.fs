namespace CardsBot

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.Integration.AspNet.Core
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open CardsBot.Dialogs
open CardsBot.Bots

type Startup () =

    // This method gets called by the runtime. Use this method to add services to the container.
    member __.ConfigureServices (services: IServiceCollection) =
        // Add framework services.
        services.AddControllers().AddNewtonsoftJson() |> ignore

        // Create the Bot Framework Adapter with error handling enabled.
        services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>()
        // Create the storage we'll be open for User and Conversation state. (Memory is great for testing purposes.)
                .AddSingleton<IStorage, MemoryStorage>()
        // Create the User state. (Used in this bot's Dialog implementation.)
                .AddSingleton<UserState>()
        // Create the Conversation state. (Used by the Dialog system itself.)
                .AddSingleton<ConversationState>()
        // The Dialog that will be run by the bot.
                .AddSingleton<MainDialog>()
        // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
                .AddTransient<IBot, RichCardsBot>() |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member __.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        if env.IsDevelopment() then
            app.UseDeveloperExceptionPage() |> ignore

        app.UseDefaultFiles()
            .UseStaticFiles()
            .UseWebSockets()
            .UseRouting()
            .UseAuthorization()
            .UseEndpoints(fun endpoints -> endpoints.MapControllers() |> ignore)
        |> ignore
