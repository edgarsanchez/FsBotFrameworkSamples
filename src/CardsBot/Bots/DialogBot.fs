namespace CardsBot.Bots

open FSharp.Control.Tasks
open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.Dialogs
open Microsoft.Extensions.Logging

// This IBot implementation can run any type of Dialog. The use of type parameterization is to allows multiple different bots
// to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
// each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
// The ConversationState is used by the Dialog system. The UserState isn't, however, it might have been used in a Dialog implementation,
// and the requirement is that all BotState objects are saved at the end of a turn.
type DialogBot<'T when 'T :> Dialog> (conversationState: ConversationState, userState: UserState, dialog: 'T, logger: ILogger<DialogBot<'T>>) =
    inherit ActivityHandler()

    // Quick trick to be able to call base.OnTurnAsync from the overriden __.OnTurnAsync
    member private __.BaseTurnAsync t c = base.OnTurnAsync(t, c)

    override __.OnTurnAsync (turnContext, cancellationToken) =
        upcast task {
            do! __.BaseTurnAsync turnContext cancellationToken

            // Save any state changes that might have occurred during the turn.
            do! conversationState.SaveChangesAsync(turnContext, false, cancellationToken)
            do! userState.SaveChangesAsync(turnContext, false, cancellationToken)
        }

    override __.OnMessageActivityAsync (turnContext, cancellationToken) =
        logger.LogInformation "Running dialog with Message Activity."

        // Run the Dialog with the new message Activity.
        unitTask { do! dialog.RunAsync(turnContext, conversationState.CreateProperty(nameof DialogState), cancellationToken) }
