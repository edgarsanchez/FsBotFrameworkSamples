namespace MyEchoBot

open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging

module Program =

    let createHostBuilder args =
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(fun webBuilder ->
                webBuilder
                    .ConfigureLogging(fun logging -> logging.AddDebug().AddConsole () |> ignore)
                    .UseStartup<Startup>() |> ignore )

    [<EntryPoint>]
    let main args =
        createHostBuilder(args).Build().Run()

        0   // exit code
