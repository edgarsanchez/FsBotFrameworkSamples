namespace TeamsConversationBot

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.Integration.AspNet.Core
open TeamsConversationBot.Bots

type Startup (configuration: IConfiguration) =

    member val Configuration = configuration with get

    // This method gets called by the runtime. Use this method to add services to the container.
    member __.ConfigureServices(services: IServiceCollection) =
        services.AddControllers().AddNewtonsoftJson () |> ignore

        // Create the Bot Framework Adapter with error handling enabled.
        services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>()
                // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
                .AddTransient<IBot, TeamsConversationBot> () |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member __.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        if env.IsDevelopment () then
            app.UseDeveloperExceptionPage () |> ignore

        app.UseDefaultFiles()
            .UseStaticFiles()
            .UseRouting()
            .UseAuthorization()
            .UseEndpoints (fun endpoints -> endpoints.MapControllers () |> ignore)
        |> ignore
