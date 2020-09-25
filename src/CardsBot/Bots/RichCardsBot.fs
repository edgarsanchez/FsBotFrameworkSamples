namespace CardsBot.Bots

open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.Bot.Builder
open CardsBot.Dialogs

type RichCardsBot (conversationState, userState, dialog, logger) =
    inherit DialogBot<MainDialog> (conversationState, userState, dialog, logger)

    override __.OnMembersAddedAsync (membersAdded, turnContext, cancellationToken) =
        let reply = MessageFactory.Text "Welcome to CardBot. \
                        This bot will show you different types of Rich Cards. \
                        Please type anything to get started."

        upcast task {
            for member' in membersAdded do
                // Greet anyone that was not the target (recipient) of this message.
                // To learn more about Adaptive Cards, see https://aka.ms/msbot-adaptivecards for more details.
                if member'.Id <> turnContext.Activity.Recipient.Id then
                    let! _ = turnContext.SendActivityAsync (reply, cancellationToken)
                    ()
        }
