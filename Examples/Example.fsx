#r "../FsharpUnionHelpers/bin/Debug/netstandard2.0/FSharpUnionHelpers.dll"
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
        | RetreivalFailed(id) -> { Code = "E01"; Message = sprintf "No products found for id=%A" id }
        | NotFound(id, label) -> { Code = "E02"; Message = sprintf "No products found for id=%A label=%A" id label }
        | Disabled -> { Code = "E03"; Message = "Disabled" }
    | ValidationError(e) -> 
        match e with
        | IdEmpty -> { Code = "E04"; Message = "Id must not be empty" }
        | LabelEmpty -> { Code = "E05"; Message = "Label must not be empty" }


let results = matchUnionWithFunction generateDefaultTypeParameter toErrorDto
results
    |> List.sortBy (fun x -> x.Code)
    |> List.iter (printfn "%A")


type IEvent = interface end
type AnEvent = {
    Data: string
}
with interface IEvent

type DataDto = {
    Name: string
    Value: int
}

type WithNestedEvent = {
    Data: DataDto
}
with interface IEvent

type InnerClass() = 
    member val Str = "" with get, set
    member val Int = 0 with get, set

type AClass() = 
    member val PublicStr = "" with get, set
    member val InnerClass: InnerClass = Unchecked.defaultof<InnerClass> with get, set

type Events =
| AnEvent of AnEvent
| WithNestedEvent of WithNestedEvent

getTypesPublicSignature typeof<string> |> toSignatureString
getTypesPublicSignature typeof<bool> |> toSignatureString
getTypesPublicSignature typeof<System.DateTime> |> toSignatureString
getTypesPublicSignature typeof<WithNestedEvent> |> toSignatureString

getTypesPublicSignature typeof<InnerClass> |> toSignatureString
getTypesPublicSignature typeof<AClass> |> toSignatureString

getTypesPublicSignature typeof<Events> |> toSignatureString