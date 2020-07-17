# Bot Framework Samples in F#
An F# translation of [Bot Framework](https://dev.botframework.com/) sample templates, right now I've only got the EchoBot sample. Just download the contents of the src/MyEchoBot folder. Get into that downloaded folder and type:

    dotnet run

This will compile the project and run a Kestrel web server exposing the bot at http://localhost:3978/api/messages.

I started with this dotnet core template:

    dotnet new webapi -n MyEchoBot -lang F#

Then I manually coded the F# equivalent functions and classes for the MyEchoBot example. I used @rspeele [TaskBuilder.fs](https://github.com/rspeele/TaskBuilder.fs) to quickly and easily emulate C# async methods in F# functions.

Comments and feedback welcomed!
