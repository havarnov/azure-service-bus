module Havarnov.AzureServiceBus.ConnectionString

open System

open Havarnov.AzureServiceBus.Error
open Havarnov.AzureServiceBus.Utils

type ConnectionString = {
    Endpoint: Uri;
    SharedAccessKeyName: string;
    SharedAccessKey: string;
}

let choose<'a, 'b> (i: Result<'a, 'b> list) : Result<'a list, 'b list> =

    let (oks, errors) =
        i
        |> List.fold
            (fun (oks, errors) item ->
                match item with
                | Ok item -> (item :: oks, errors)
                | Error item -> (oks, item :: errors))
            ([], [])

    match (oks, errors) with
    | oks, [] -> Ok oks
    | _, errs -> Error errs

let find key l =
    l
    |> List.find (fun (k, v) -> k = key)

let tryParseUri s =
    try
        Ok (new Uri(s))
    with
        | :? System.UriFormatException as ex ->
            Error (ConnectionStringParseError ex.Message)

let changeToScheme scheme (uri: Uri) =
    let builder = new UriBuilder(uri)

    let hadDefaultPort = builder.Uri.IsDefaultPort;
    builder.Scheme <- scheme
    builder.Port <- if hadDefaultPort then -1 else builder.Port

    builder.Uri

let changeToHttps = changeToScheme "https"

let parse (s: string) : Result<ConnectionString, AzureServiceBusError>  =
    let parts =
        s.Split [|';'|]
        |> Seq.toList
        |> List.map
            (fun i ->
                let part =
                    i.Split([|'='|], 2)
                    |> Seq.toList

                match part with
                | key::value::[] ->
                    Ok (key, Some value)
                | key::[] ->
                    Ok (key, None)
                | _ ->
                    Error (ConnectionStringParseError
                            "not a key value pair or just a key"))
        |> choose

    match parts with
    | Ok parts ->
        let (_, endpoint) = find "Endpoint" parts
        let (_, name) = find "SharedAccessKeyName" parts
        let (_, key) = find "SharedAccessKey" parts

        match (endpoint, name, key) with
        | (Some endpoint, Some name, Some key) ->
            match tryParseUri endpoint with
            | Ok endpoint ->
                Ok ({
                        Endpoint = changeToHttps endpoint;
                        SharedAccessKeyName = name;
                        SharedAccessKey = key;
                    })
            | Error e ->
                Error e
        | _ ->
            Error (ConnectionStringParseError
                    "foobar")
    | Error e ->
        Error (ConnectionStringParseError
                "foobar")