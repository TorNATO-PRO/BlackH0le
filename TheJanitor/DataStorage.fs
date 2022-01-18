namespace TheJanitor

open System
open System.IO

module DataStorage =

    let channels =
        Path.Join(Directory.GetCurrentDirectory(), "data", "channels.txt")

    let usernames =
        Path.Join(Directory.GetCurrentDirectory(), "data", "usernames.txt")


    // a set of "suspect" channels
    let mutable suspectChannels: Set<uint64> =
        if not (File.Exists(channels)) then
            File.Create(channels) |> ignore
            Set.empty
        else
            File.ReadLines(channels)
            |> Seq.map uint64
            |> Set.ofSeq

    let addSuspectChannel (element: uint64) =
        suspectChannels <- Set.add element suspectChannels

        if not (File.Exists(channels)) then
            File.Create(channels) |> ignore

        let tokenizedChannels =
            suspectChannels
            |> Set.toList
            |> List.map (fun i -> $"%i{i}")

        File.WriteAllLines(channels, tokenizedChannels)
        Set.contains element suspectChannels

    let removeSuspectChannel (element: uint64) =
        suspectChannels <- Set.remove element suspectChannels

        if not (File.Exists(channels)) then
            File.Create(channels) |> ignore

        let tokenizedChannels =
            suspectChannels
            |> Set.toList
            |> List.map (fun i -> $"%i{i}")

        File.WriteAllLines(channels, tokenizedChannels)
        not (Set.contains element suspectChannels)
