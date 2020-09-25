namespace CardsBot

open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging

module Program =
    let exitCode = 0

    let CreateHostBuilder args =
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults (fun webBuilder ->
                webBuilder
                    .ConfigureLogging(fun logging -> logging.AddDebug().AddConsole () |> ignore)
                    .UseStartup<Startup> () |> ignore )

    [< EntryPoint >]
    let main args =
        CreateHostBuilder(args).Build().Run ()

        exitCode
