namespace MyEchoBot.Bots

open FSharp.Control.Tasks
open Microsoft.Bot.Builder

type EchoBot() =
    inherit ActivityHandler()

    override __.OnMessageActivityAsync(turnContext, cancellationToken) =
        let replyText = $"Echo: {turnContext.Activity.Text}" 
        unitTask {
            return! turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken)
        }

    override __.OnMembersAddedAsync(membersAdded, turnContext, cancellationToken) =
        let welcomeText = "Hello and welcome!"
        unitTask {
            for newMember in membersAdded do
                if newMember.Id <> turnContext.Activity.Recipient.Id then
                    let! _ = turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken)
                    ()
        }
