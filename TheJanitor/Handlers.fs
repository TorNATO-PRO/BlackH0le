namespace TheJanitor

open System
open System.Globalization
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
{e.Message.Content}
"""
        } |> Async.StartAsTask :> Task

    [<RequireBotPermissions(Permissions.ManageMessages)>]
    let messageFuzzer (c: DiscordClient) (e: MessageCreateEventArgs) : Task =
        // TODO - Deal with stickers and images?
        async {
            if Set.contains e.Channel.Id suspectChannels
               && not e.Author.IsBot then
                do!
                    e.Message.DeleteAsync()
                    |> Async.AwaitTask
                    |> Async.Ignore

                let username =
                    if Map.containsKey e.Author.Id pseudonames then
                        Map.find e.Author.Id pseudonames
                    else
                        addPseudoNames e.Author.Id |> ignore
                        Map.find e.Author.Id pseudonames
                                                
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
                        messageBuilder.Content <- $"[%i{username}] %s{e.Message.Content}"
                        
                        do!
                            e.Channel.SendMessageAsync(messageBuilder)
                            |> Async.AwaitTask
                            |> Async.Ignore


                if Seq.isEmpty e.Message.Attachments then
                    let messageBuilder = DiscordMessageBuilder()
                    messageBuilder.Content <- $"[%i{username}] %s{e.Message.Content}"

                    do!
                        e.Channel.SendMessageAsync(messageBuilder)
                        |> Async.AwaitTask
                        |> Async.Ignore
        } |> Async.StartAsTask :> Task
