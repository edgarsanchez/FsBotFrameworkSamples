namespace QnABot

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.BotFramework
open Microsoft.Bot.Builder.Integration.AspNet.Core
open Microsoft.Bot.Connector.Authentication
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open QnABot.Bots

type Startup () =

    // This method gets called by the runtime. Use this method to add services to the container.
    member __.ConfigureServices (services: IServiceCollection) =
        // Add framework services.
        services.AddControllers().AddNewtonsoftJson () |> ignore

        // Add the HttpClientFactory to be used for the QnAMaker calls.
        services.AddHttpClient () |> ignore

        // Create the credential provider to be used with the Bot Framework Adapter.
        services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>()
                // Create the Bot Framework Adapter with error handling enabled. 
                .AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>()
                // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
                .AddTransient<IBot, QnABot> () |> ignore

   // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member __.Configure (app: IApplicationBuilder, env: IWebHostEnvironment) =
        if env.IsDevelopment () then
            app.UseDeveloperExceptionPage () |> ignore

        app.UseDefaultFiles()
            .UseStaticFiles()
            .UseRouting()
            .UseAuthorization()
            .UseEndpoints (fun endpoints -> endpoints.MapControllers () |> ignore)
        |> ignore
