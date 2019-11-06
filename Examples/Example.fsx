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

// examples for getTypesPublicSignature and toSignatureString

type IEvent = interface end
type AnEvent = {
    Data: string
}

type DataDto = {
    Name: string
    Value: int
}

type RecordWithNestedRecord = {
    Data: DataDto
}

type InnerClass() = 
    member val Str = "" with get, set
    member val Int = 0 with get, set

type AClass() = 
    member val PublicStr = "" with get, set
    member val Stamp = DateTime.MinValue with get, set
    member val NullableInt = 0 |> Nullable with get, set
    member val InnerClass: InnerClass = Unchecked.defaultof<InnerClass> with get, set

type Events =
| AnEvent of AnEvent
| WithNestedEvent of RecordWithNestedRecord
| SimpleEvent of string
with interface IEvent

type MyEnum =
    | One = 1
    | Two = 2

let tuple = (1, "hi", InnerClass())

// built-in types
getTypesPublicSignature typeof<bool> |> toSignatureString
getTypesPublicSignature typeof<string> |> toSignatureString
getTypesPublicSignature typeof<System.DateTime> |> toSignatureString
// nullables
getTypesPublicSignature typeof<Nullable<int>> |> toSignatureString

// Collections
getTypesPublicSignature typeof<ResizeArray<int>> |> toSignatureString
getTypesPublicSignature typeof<List<int>> |> toSignatureString

// records
getTypesPublicSignature typeof<ErrorDto> |> toSignatureString
getTypesPublicSignature typeof<RecordWithNestedRecord> |> toSignatureString

// classes
getTypesPublicSignature typeof<InnerClass> |> toSignatureString
getTypesPublicSignature typeof<AClass> |> toSignatureString

// Discriminated Unions
getTypesPublicSignature typeof<Events> |> toSignatureString
getTypesPublicSignature typeof<WrappedId> |> toSignatureString

// enums
getTypesPublicSignature typeof<MyEnum> |> toSignatureString

// tuples
getTypesPublicSignature (tuple.GetType()) |> toSignatureString