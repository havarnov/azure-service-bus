// Learn more about F# at http://fsharp.org

open System

open Havarnov.AzureServiceBus
open Havarnov.AzureServiceBus.ConnectionString
open Havarnov.AzureServiceBus.Utils

type Msg =
    | ExampleMsg of string

[<EntryPoint>]
let main argv =
    match argv with
    | [|connectionString|] ->
        let res =
            asyncResult {
                // parse connection string
                let! connectionString = async { return parse connectionString }

                // create client
                let client = QueueClient<Msg>(connectionString, "testqueue1")

                // post new msg
                // let! _ = client.Post(ExampleMsg "foobar1")

                // receive msg (non destructive)
                let! msg = client.Receive()

                // receive msg
                // let! msg = client.DestructiveReceive()

                let lockToken = msg.Properties.LockToken
                let seqNum = msg.Properties.SequenceNumber
                let! s = client.DeleteMsg lockToken seqNum
                printfn "done"
            } |> Async.RunSynchronously

        match res with
        | Ok _ -> printfn "everything should be ok."
        | Error e -> printfn "something went wrong: %A" e

    | _ ->
        printfn "usage: dotnet run <connection string>"

    0 // return an integer exit code
