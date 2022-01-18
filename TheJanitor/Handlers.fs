namespace TheJanitor

open System
open System.Threading.Tasks
open DSharpPlus
open DSharpPlus.CommandsNext.Attributes
open DSharpPlus.Entities
open DSharpPlus.EventArgs
open DataStorage

module Handlers =
    let messageLogger (_: DiscordClient) (e: MessageCreateEventArgs) : Task =
        async {
            printfn
                $"""
Author: {e.Author.Username} - {e.Author.Id}
Date: {e.Message.Timestamp.Date.ToLongDateString()} {e.Message.Timestamp.TimeOfDay.ToString()}
Channel: {e.Message.Channel.Name}
{e.Message.Content}
"""
        }
        |> Async.StartAsTask
        :> Task

    [<RequireBotPermissions(Permissions.ManageMessages)>]
    let sendFuzzedMessage (content: string) (e: MessageCreateEventArgs) (channel: DiscordChannel) =
        if Set.contains channel.Id suspectChannels
           && not e.Author.IsBot
           && not channel.IsPrivate then
            e.Message.DeleteAsync()
            |> Async.AwaitTask
            |> Async.Ignore
            |> ignore

            let username = Random().Next()

            let imageAttachments =
                e.Message.Attachments
                |> Seq.filter (fun i -> i.MediaType.Contains("image"))
                |> Seq.map (fun i -> i.Url)
                |> Seq.toList

            // deal with images
            if not (List.isEmpty imageAttachments) then
                for image in imageAttachments do
                    let messageBuilder = DiscordMessageBuilder()
                    let anonMessage = DiscordEmbedBuilder()
                    anonMessage.Color <- DiscordColor.PhthaloBlue
                    anonMessage.ImageUrl <- image
                    messageBuilder.Embed <- anonMessage
                    messageBuilder.Content <- $"[%i{username}] %s{content}"

                    channel.SendMessageAsync(messageBuilder)
                    |> Async.AwaitTask
                    |> Async.Ignore
                    |> ignore

            if Seq.isEmpty e.Message.Attachments then
                let messageBuilder = DiscordMessageBuilder()
                messageBuilder.Content <- $"[%i{username}] %s{content}"

                channel.SendMessageAsync(messageBuilder)
                |> Async.AwaitTask
                |> Async.Ignore
                |> ignore


    [<RequireBotPermissions(Permissions.ManageMessages)>]
    let messageFuzzer (_: DiscordClient) (e: MessageCreateEventArgs) : Task =
        // TODO - Deal with stickers and images?
        async { sendFuzzedMessage e.Message.Content e e.Channel }
        |> Async.StartAsTask
        :> Task

    [<RequireBotPermissions(Permissions.ManageMessages)>]
    let handleDirectMessage (c: DiscordClient) (e: MessageCreateEventArgs) : Task =
        async {
            if e.Channel.IsPrivate && not e.Author.IsCurrent then
                let messagePieces = e.Message.Content.Split("\n")

                if messagePieces.Length > 0
                   && (Array.head messagePieces)[0] = '#' then

                    let channel = (messagePieces |> Seq.head).Substring(1)

                    let messageContent =
                        messagePieces
                        |> Seq.tail
                        |> Seq.fold (fun x y -> x + $"{y}\n") ""

                    let channels =
                        c.Guilds.Values
                        |> Seq.map (fun i -> i.Channels.Values)
                        |> Seq.concat
                        |> Seq.filter
                            (fun i ->
                                i.Name = channel
                                && Seq.contains i.Id suspectChannels)

                    if Seq.isEmpty channels then
                        do!
                            e.Channel.SendMessageAsync("Sorry, that is not a valid channel")
                            |> Async.AwaitTask
                            |> Async.Ignore
                    else
                        let channel = Seq.head channels
                        sendFuzzedMessage messageContent e channel
        }
        |> Async.StartAsTask
        :> Task
