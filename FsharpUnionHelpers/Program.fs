module Program
open test
open Core
 
let normaliseName (n: string) = n.TrimStart('_')
 
let getUnionNameAndResult<'a> fn =
    let allDUCases = getAllDUCases generateParameter typeof<'a>
    let matchedCaseResults =
        allDUCases
        |> List.map (fun c -> c :?> 'a)
        |> List.map (fun case -> case.GetType().Name, case, fn case)
    matchedCaseResults

[<EntryPoint>]
let main argv =
    let matchedCaseResults = getUnionNameAndResult toErrorDto
    printfn "-----------------"
    printfn "returned results:"
    matchedCaseResults
        |> List.iter (fun (name, case, result) -> printfn "Type.Name: %s # DomainError: %A # returns: %A" (name |> normaliseName) case result)
    0 // return an integer exit code
