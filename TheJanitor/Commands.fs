namespace TheJanitor

open System
open System.Threading.Tasks
open DSharpPlus
open DSharpPlus.CommandsNext
open DSharpPlus.CommandsNext.Attributes
open DSharpPlus.Entities
open DataStorage

type Commands() =

    // inherit from the base command module
    inherit BaseCommandModule()

    member private self.Flex(ctx: CommandContext) : Async<unit> =
        async {
            // trigger a typing indicator
            do! ctx.TriggerTypingAsync() |> Async.AwaitTask

            // create an emoji to make the message a bit more colourful
            let emoji =
                DiscordEmoji.FromName(ctx.Client, ":muscle:")

            // finally respond with the ping
            do!
                ctx.RespondAsync $"%s{emoji.ToString()} F# is awesome!"
                |> Async.AwaitTask
                |> Async.Ignore
        }

    [<Command("fsharp"); Description("Flexes the F# language for all to see.")>]
    member public self.FlexAsync(ctx: CommandContext) : Task =
        self.Flex(ctx) |> Async.StartAsTask :> Task


    member private self.Trash(ctx: CommandContext, count: int) : Async<unit> =
        async {
            do! ctx.TriggerTypingAsync() |> Async.AwaitTask

            if count <= 0 then
                let frownEmoji =
                    DiscordEmoji.FromName(ctx.Client, ":frowning:")

                do!
                    ctx.RespondAsync $"You must specify an integer > 0, not %i{count} %s{frownEmoji.ToString()}"
                    |> Async.AwaitTask
                    |> Async.Ignore
            else
                // get all of the messages to delete
                let messages =
                    ctx.Channel.GetMessagesAsync(count + 1)
                    |> Async.AwaitTask
                    |> Async.RunSynchronously
                    // verify 14 days old or less
                    |> Seq.filter
                        (fun x ->
                            (DateTimeOffset.UtcNow - x.Timestamp).TotalDays
                            <= 14)
                    |> List.ofSeq

                // get the true count of the messages to be deleted
                let trueCount = messages |> List.length

                if trueCount = 0 then
                    let frownEmoji =
                        DiscordEmoji.FromName(ctx.Client, ":frowning:")

                    do!
                        ctx.RespondAsync $"No messages the can be deleted found %s{frownEmoji.ToString()}"
                        |> Async.AwaitTask
                        |> Async.Ignore

                else
                    let maxTimeBack =
                        messages
                        |> List.fold (fun x y -> max x (DateTimeOffset.UtcNow - y.Timestamp).TotalDays) 0

                    do!
                        ctx.Channel.DeleteMessagesAsync(messages)
                        |> Async.AwaitTask
                        |> Async.Ignore

                    printfn
                        $"{DateTime.UtcNow.ToLongDateString()} - Successfully deleted %i{trueCount} in %s{ctx.Guild.Name} - %s{ctx.Channel.Name} from the past %.2f{maxTimeBack}"

                    let smileEmoji =
                        DiscordEmoji.FromName(ctx.Client, ":smile:")

                    do!
                        ctx.RespondAsync
                            $"%s{ctx.User.Mention} - Successfully trashed %i{trueCount} messages from the past %.2f{maxTimeBack} days! %s{smileEmoji.ToString()}"
                        |> Async.AwaitTask
                        |> Async.Ignore
        }

    [<Command("trash"); Description("Trashes a specified number of messages."); Aliases("clean", "delete")>]
    [<RequireUserPermissions(Permissions.ManageMessages)>]
    [<RequireBotPermissions(Permissions.ManageMessages)>]
    member public self.TrashAsync(ctx: CommandContext, count: int) : Task =
        self.Trash(ctx, count) |> Async.StartAsTask :> Task


    member private self.TrashAll(ctx: CommandContext) : Async<unit> =
        async {
            do! ctx.TriggerTypingAsync() |> Async.AwaitTask

            // get all of the messages to delete (maxes out at 10000)
            let messages =
                ctx.Channel.GetMessagesAsync(10000)
                |> Async.AwaitTask
                |> Async.RunSynchronously
                // verify 14 days old or less
                |> Seq.filter
                    (fun x ->
                        (DateTimeOffset.UtcNow - x.Timestamp).TotalDays
                        <= 14)
                |> List.ofSeq

            // get the true count of the messages to be deleted
            let trueCount = messages |> List.length

            if trueCount = 0 then
                let frownEmoji =
                    DiscordEmoji.FromName(ctx.Client, ":frowning:")

                do!
                    ctx.RespondAsync $"No messages the can be deleted found %s{frownEmoji.ToString()}"
                    |> Async.AwaitTask
                    |> Async.Ignore

            else
                let maxTimeBack =
                    messages
                    |> List.fold (fun x y -> max x (DateTimeOffset.UtcNow - y.Timestamp).TotalDays) 0

                do!
                    ctx.Channel.DeleteMessagesAsync(messages)
                    |> Async.AwaitTask
                    |> Async.Ignore

                printfn
                    $"{DateTime.UtcNow.ToLongDateString()} - Successfully deleted %i{trueCount} in %s{ctx.Guild.Name} - %s{ctx.Channel.Name} from the past %.2f{maxTimeBack}"

                let smileEmoji =
                    DiscordEmoji.FromName(ctx.Client, ":smile:")

                do!
                    ctx.RespondAsync
                        $"%s{ctx.User.Mention} - Successfully trashed %i{trueCount} messages from the past %.2f{maxTimeBack} days! %s{smileEmoji.ToString()}"
                    |> Async.AwaitTask
                    |> Async.Ignore
        }

    [<Command("trashall"); Description("Trashes a specified number of messages."); Aliases("deleteall")>]
    [<RequireUserPermissions(Permissions.ManageMessages)>]
    [<RequireBotPermissions(Permissions.ManageMessages)>]
    member public self.TrashAllAsync(ctx: CommandContext) : Task =
        self.TrashAll(ctx) |> Async.StartAsTask :> Task
        
    
    member private self.SetSuspect(ctx: CommandContext) : Async<unit> =
        async {
            addSuspectChannel ctx.Channel.Id |> ignore    

            do!
                ctx.RespondAsync $"Added {ctx.Channel.Name} to the set of sus channels!"
                    |> Async.AwaitTask
                    |> Async.Ignore                
        }
        
    [<Command("susify"); Description("Sets a channel as suspect, anonymizes messages."); Aliases("setsus")>]
    [<RequireUserPermissions(Permissions.ManageMessages)>]
    [<RequireBotPermissions(Permissions.ManageMessages)>]
    member public self.SetSuspectAsync(ctx: CommandContext) : Task =
        self.SetSuspect(ctx) |> Async.StartAsTask :> Task

    member private self.SetTrust(ctx: CommandContext) : Async<unit> =
        async {
            removeSuspectChannel ctx.Channel.Id |> ignore    

            do!
                ctx.RespondAsync $"Removed {ctx.Channel.Name} from the set of sus channels!"
                    |> Async.AwaitTask
                    |> Async.Ignore                
        }
        
    [<Command("trust"); Description("Removes a channel as suspect channel."); Aliases("unsus")>]
    [<RequireUserPermissions(Permissions.ManageMessages)>]
    [<RequireBotPermissions(Permissions.ManageMessages)>]
    member public self.SetTrustAsync(ctx: CommandContext) : Task =
        self.SetTrust(ctx) |> Async.StartAsTask :> Task