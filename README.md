# FsBotFrameworkEchoBot
An F# translation of the Bot Framework EchoBot sample. Just download the contents of the src/MyEchoBot folder. Get into that downloaded folder and type:
    dotnet run
This will compile a run a Kestrel web server exposing the bot at http://localhost:3978/api/messages.

The solution was started with this dotnet core template:
    dotnet new webapi -n MyEchoBot -lang F#
Then I manually coded the F# equivalent funcions as classes to the MyEchoBot https://dev.botframework.com/ example. I used rspeele's TaskBuilder.fs https://github.com/rspeele/TaskBuilder.fs to quickly and easily emulate C# async methods in F# functions.

Comments and feedback welcomed!
