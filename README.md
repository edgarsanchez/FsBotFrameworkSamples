# Bot Framework Samples in F#
An F# translation of [Bot Framework](https://dev.botframework.com/) sample templates, so far we've got:

* The EchoBot sample at the src/MyEchoBot folder
* The CoreBot sample at the src/MyCoreBot folder (I haven't thoroughly tested this sample yet, but it seems to be mostly working 🤞🏽🙂)
  * Configuring this bot is basically putting your LUIS API keys in appsetting.json (more detailed instrucions in the C# sample)

All the examples try to follow the original C# templates as closely as possible, including some quirks in the bot, e.g., CoreBot checks cities when using LUIS yet, it doesn't do that when getting cities as a response to a direct question. On the other hand, this allows you to compare the C# code with the F# code vis a vis.

For any particular sample, get into its folder and type:

    dotnet run

This command will compile the project and run a Kestrel web server exposing the bot at http://localhost:3978/api/messages.

I started with this dotnet core template:

    dotnet new webapi -n MyEchoBot -lang F#

Afterward, I manually coded the F# equivalent functions and classes, trying to use functional idioms whenever relevant (but always respecting the general code organization and behavior). I used @rspeele [TaskBuilder.fs](https://github.com/rspeele/TaskBuilder.fs) to emulate quickly and easily C# async methods in F# functions.

Comments and feedback welcomed!
