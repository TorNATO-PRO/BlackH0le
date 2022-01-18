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

    let mutable pseudonames: Map<uint64, uint64> =
        if not (File.Exists(usernames)) then
            File.Create(usernames) |> ignore
            Map.empty
        else
            File.ReadLines(usernames)
            |> Seq.map (fun i -> (i.Split(",")) |> Seq.map uint64 |> Seq.toList)
            |> Seq.filter (fun i -> List.length i = 2)
            |> Seq.map (fun i -> (i[0], i[1]))
            |> Map.ofSeq
            
    let addPseudoNames (userid: uint64) =
        let randomID = uint64 ((Random()).Next())
        
        pseudonames <- Map.add userid randomID pseudonames

        if not (File.Exists(usernames)) then
            File.Create(usernames) |> ignore

        let tokenizedPseudonames =
            pseudonames
            |> Map.toList
            |> List.map (fun (a, b) -> $"%i{a},%i{b}")

        File.WriteAllLines(usernames, tokenizedPseudonames)
        Map.containsKey userid pseudonames

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
