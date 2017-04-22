module Havarnov.AzureServiceBus.Utils

open System
open System.Text

let origin = DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

let now () = int((DateTime.UtcNow - origin).TotalSeconds)

let toBytes (s: string) =
    Encoding.UTF8.GetBytes(s)

module Result =
    let map f res =
        match res with
        | Ok v -> Ok (f v)
        | Error e -> Error e

module Async =
    let map f a =
        async {
            let! a = a
            return (f a)
        }

type ResultBuilder() =

    member this.Bind(v, f) =
        match v with
        | Ok v -> f v
        | Error e -> Error e

    member this.Return value = Ok value

    member this.Zero() =
        Ok ()

    member this.Delay f = f

let result = ResultBuilder()

type AsyncResultBuilder () =
    member this.Bind(value, func) =
        async {
            let! value = value
            return!
                match value with
                | Ok value -> func value
                | Error e -> async { return Error e }
        }

    member this.Return value =
        Ok value

    member this.Zero() =
        async {
            return Ok ()
        }

let asyncResult = AsyncResultBuilder()