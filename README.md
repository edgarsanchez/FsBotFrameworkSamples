# Bot Framework Samples in F#
An F# translation of [Bot Framework](https://dev.botframework.com/) sample templates and also some of the examples in the [BotBuilder-Samples .NET Core repo](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore), so far we've got:

* The EchoBot template at the src/MyEchoBot folder
* The CoreBot template at the src/MyCoreBot folder (I haven't thoroughly tested this sample yet, but it seems to be mostly working 🤞🏽🙂)
  * Configuring this bot consists basically of putting your LUIS API keys in appsetting.json (more detailed instructions in the C# sample)
* The **QnAMaker** bot sample at the src/QnABot folder. The C# original is [here](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore/11.qnamaker)
  * Configuring this bot requires to create a [QnA Maker Service](https://www.qnamaker.ai/), you will find detailed instructions in the README.md file
* The **Teams** Conversation bot sample at the src/TeamsConversationBot folder. The C# original is [here](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore/57.teams-conversation-bot)
  * Warning: I have tested this bot only slightly! (but it seems to be working)
  * Configuring this bot requires a good understanding of how bots work inside Teams. This demands working in several places, like so:
    *  Register a bot in Azure
    *  Modify appsettings.json with the registered bot Azure App Id and password
    *  Publish the bot in Azure or via ngrok
    *  Create a Teams app with the bot's manifest.zip file
    *  You will find the detailed steps on the README.md file of the [original C# example](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore/57.teams-conversation-bot)
 * The **Teams** Messaging Extensions Action Bot sample at the src/TeamsMessagingExtensionsAction folder. The C# original is [here](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore/51.teams-messaging-extensions-action)
   * Warning: I have tested this bot only slightly! (but it seems to be working)
    * Configuring this bot requires a good understanding of how bots work inside Teams. This demands working in several places, like so:
      *  Register a bot in Azure
      *  Modify appsettings.json with the registered bot Azure App Id and password
      *  Publish the bot in Azure or via ngrok
      *  Create a Teams app with the bot's manifest.zip file
      *  You will find the detailed steps on the README.md file of the [original C# example](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore/51.teams-messaging-extensions-action)
  
The sample template translations try to follow the original C# templates as closely as possible, including some quirks in the bot, e.g., CoreBot checks cities when using LUIS yet, but it doesn't do that when getting towns as a response to a direct question. On the other hand, this allows you to compare the C# code with the F# code vis a vis.

For any of the template samples, get into its folder and type:

    dotnet run

This command will compile the project and run a Kestrel web server exposing the bot at http://localhost:3978/api/messages.

I started with this dotnet core template:

    dotnet new webapi -n MyEchoBot -lang F#

Afterward, I manually coded the F# equivalent functions and classes, trying to use functional idioms whenever relevant (but always respecting the general code organization and behavior).

The sample bots in the BotBuilder repo tend to require specific configurations, please check the README.md file in its respective folder.

I used @rspeele [TaskBuilder.fs](https://github.com/rspeele/TaskBuilder.fs) to emulate quickly and easily C# async methods in F# functions.

Your comments and feedback are welcomed!
