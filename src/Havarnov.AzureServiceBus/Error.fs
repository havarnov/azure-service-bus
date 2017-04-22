module Havarnov.AzureServiceBus.Error

type AzureServiceBusError =
    | UnknownError
    | BadRequest
    | AuthorizationFailure
    | QuotaExceededOrMessageTooLarge
    | QueueOrTopicDoesNotExists of string
    | InternalAzureError
    | NoMessageAvailableInQueue
    | NoMessageFound of (string * string)
    | ConnectionStringParseError of string