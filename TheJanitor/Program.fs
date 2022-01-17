// Author: Nathan Waltz
// TheJanitor is meant to provide functionality for the
// BlackH0le bot, which is to keep channels clean.

open System.Threading.Tasks
open TheJanitor.Core

// main method ...
[<EntryPoint>]
let main (argv: string []) : int =
    printfn "Starting BlackH0le - a bot for keeping channels clean"

    // connect to the Discord client
    client.ConnectAsync()
    |> Async.AwaitTask
    |> Async.RunSynchronously

    // run indefinitely - until being forced to stop
    Task.Delay(-1)
    |> Async.AwaitTask
    |> Async.RunSynchronously

    0
