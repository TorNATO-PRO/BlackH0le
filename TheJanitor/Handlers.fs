namespace TheJanitor

open System.Threading.Tasks
open DSharpPlus
open DSharpPlus.CommandsNext.Attributes
open DSharpPlus.Entities
open DSharpPlus.EventArgs
open DataStorage

module Handlers =
    let messageLogger (_: DiscordClient) (e: MessageCreateEventArgs) : Task =
        task {
            printfn
                $"""
Author: {e.Author.Username} - {e.Author.Id}
Date: {e.Message.Timestamp.Date.ToLongDateString()} {e.Message.Timestamp.TimeOfDay.ToString()}
{e.Message.Content}
"""
        }

    [<RequireBotPermissions(Permissions.ManageMessages)>]
    let messageFuzzer (c: DiscordClient) (e: MessageCreateEventArgs) : Task =
        // TODO - Deal with stickers and images?
        task {
            if Set.contains e.Channel.Id suspectChannels
               && not e.Author.IsBot then
                do!
                    e.Message.DeleteAsync()
                    |> Async.AwaitTask
                    |> Async.Ignore

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
                        messageBuilder.Content <- e.Message.Content

                        do!
                            e.Channel.SendMessageAsync(messageBuilder)
                            |> Async.AwaitTask
                            |> Async.Ignore


                if Seq.isEmpty e.Message.Attachments then
                    let messageBuilder = DiscordMessageBuilder()
                    messageBuilder.Content <- e.Message.Content

                    do!
                        e.Channel.SendMessageAsync(messageBuilder)
                        |> Async.AwaitTask
                        |> Async.Ignore
        }
