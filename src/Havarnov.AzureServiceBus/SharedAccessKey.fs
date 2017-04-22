module Havarnov.AzureServiceBus.SharedAccessKey

open System
open System.Security.Cryptography

open Havarnov.AzureServiceBus.Utils
open Havarnov.AzureServiceBus.ConnectionString

let generateSharedAccessKeyHeader connectionString =
    let duration = 60 * 6 // 6 minutes
    let expiry =
        int((DateTime.UtcNow - origin).TotalSeconds) + duration
    let encodedEndpoint = Uri.EscapeDataString(connectionString.Endpoint.ToString())
    let hmacInput =
        sprintf "%s\n%d"
            encodedEndpoint
            expiry

    printfn "uri endpoint: %s" hmacInput

    let hmac = new HMACSHA256(toBytes connectionString.SharedAccessKey)
    let hash = hmac.ComputeHash(toBytes hmacInput)
    let signature = Uri.EscapeDataString(Convert.ToBase64String(hash))

    let header =
        sprintf "sig=%s&se=%d&skn=%s&sr=%s"
            signature
            expiry
            connectionString.SharedAccessKeyName
            encodedEndpoint
    (expiry, header)

type Msg =
    Fetch of AsyncReplyChannel<string>

type SharedAccessKey(connectionString: ConnectionString) =
    let connectionString = connectionString

    let mailbox = MailboxProcessor<Msg>.Start(fun inbox ->
        let rec msgLoop e =
            async {
                let (expiry, header) =
                    match e with
                    | Some(expiry, header) when expiry > (now()) ->
                        expiry, header
                    | _ ->
                        generateSharedAccessKeyHeader
                            connectionString

                let! msg = inbox.Receive()
                match msg with
                | Fetch replyChannel ->
                    replyChannel.Reply(header)

                do! msgLoop (Some (expiry, header))
            }
        msgLoop None)

    member this.Get() =
        async {
            return! mailbox.PostAndAsyncReply(Fetch)
        }