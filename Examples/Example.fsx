#r "../FsharpUnionHelpers/bin/Debug/netcoreapp2.2/FSharpUnionHelpers.dll"
open FSharpUnionHelpers
open System

type WrappedId = private Id of Guid

type WrappedLabel = private Label of string

type DomainError =
    | ValidationError of ValidationError
    | ProductError of ProductError
and ValidationError =
    | IdEmpty
    | LabelEmpty
and ProductError =
    | RetreivalFailed of id: string
    | NotFound of id: WrappedId * label: WrappedLabel
    | Disabled

type ErrorDto = {
    Code: string
    Message: string
}

let toErrorDto =
    function
    | ProductError(e) ->
        match e with
        | RetreivalFailed(dealershipId) -> { Code = "E01"; Message = sprintf "No products found for id=%A" dealershipId }
        | NotFound(dealershipId, channel) -> { Code = "E02"; Message = sprintf "No products found for id=%A label=%A" dealershipId channel }
        | Disabled -> { Code = "E03"; Message = "Disabled" }
    | ValidationError(e) -> 
        match e with
        | IdEmpty -> { Code = "E04"; Message = "Id must not be empty" }
        | LabelEmpty -> { Code = "E05"; Message = "Label must not be empty" }


let results = matchUnionWithFunction generateDefaultTypeParameter toErrorDto
results
    |> List.sortBy (fun x -> x.Code)
    |> List.iter (printfn "%A")