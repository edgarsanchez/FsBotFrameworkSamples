namespace MyCoreBot.Dialogs

open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.Dialogs
open Microsoft.Bot.Schema

type CancelAndHelpDialog (id) =
    inherit ComponentDialog (id)

    let helpMsgText = "Show help here"
    let cancelMsgText = "Cancelling..."

    let interruptAsync (innerDc: DialogContext) cancellationToken =
        task {
            match innerDc.Context.Activity.Type with
            | ActivityTypes.Message ->
                match innerDc.Context.Activity.Text.ToLowerInvariant () with
                | "help" 
                | "?" ->
                    let helpMessage = MessageFactory.Text (helpMsgText, helpMsgText, InputHints.ExpectingInput);
                    let! _ = innerDc.Context.SendActivityAsync (helpMessage, cancellationToken)
                    return DialogTurnResult (DialogTurnStatus.Waiting)
                | "cancel"
                | "quit" ->
                    let cancelMessage = MessageFactory.Text (cancelMsgText, cancelMsgText, InputHints.IgnoringInput);
                    let! _ = innerDc.Context.SendActivityAsync (cancelMessage, cancellationToken)
                    return! innerDc.CancelAllDialogsAsync (cancellationToken)
                | _ ->
                    return null
            | _ ->
                return null }

    // Silly trick to access base.OnContinueDialogAsync from its overriden definition below
    member private __.BaseOnContinueDialogAsync dc ct = base.OnContinueDialogAsync (dc, ct)

    override __.OnContinueDialogAsync (innerDc, cancellationToken) =
        task {
            match! interruptAsync innerDc cancellationToken with
            | null ->
                return! __.BaseOnContinueDialogAsync innerDc cancellationToken
            | result ->
                return result }
