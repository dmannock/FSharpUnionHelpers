﻿namespace FSharpUnionHelpers
[<AutoOpen>]
module Core =

    open System
    open Microsoft.FSharp.Reflection
    open System.Reflection

    // TEMP switching globally for initial play about
    [<Literal>]
    let VerbosePrinting = false

    let printDepth depth msg =
        if VerbosePrinting then printfn "%s%s" (String.replicate (depth *4) " ") msg
 
    type CreatedUnion = obj
    let getUnionCases t = FSharpType.GetUnionCases(t, BindingFlags.NonPublic ||| BindingFlags.Public)
    let makeUnion uc args = FSharpValue.MakeUnion(uc,args |> Array.ofList, BindingFlags.NonPublic ||| BindingFlags.Instance ||| BindingFlags.Public)
    let isUnion t = FSharpType.IsUnion(t, BindingFlags.NonPublic ||| BindingFlags.Instance)

    // I basically did nothing, non-public Unions now work. inspired by: https://stackoverflow.com/questions/4470366/using-all-cases-of-union-type-f
    let getAllDUCases fNonUnionArg t : CreatedUnion list =
        let rec loop depth t =
            let printDepth = printDepth depth
            // taken from http://stackoverflow.com/questions/6497058/lazy-cartesian-product-of-multiple-sequences-sequence-of-sequences
            let cartesianProduct2 sequences = 
                let step acc sequence = seq {
                    for x in acc do
                    for y in sequence do
                    yield seq { yield! x; yield y}}
                Seq.fold step (Seq.singleton Seq.empty) sequences
            let makeCaseTypes (fUnion:Type-> obj list) (fNonUnionArg:Type -> obj) (uc: UnionCaseInfo) : UnionCaseInfo*(obj list list) =
                let constructorArgs = 
                    let fields = uc.GetFields() 
                    fields
                    |> Seq.map (fun f -> 
                        if isUnion f.PropertyType then 
                            let childTypes = fUnion f.PropertyType 
                            if childTypes |> Seq.exists (fun ct -> ct.GetType() |> isUnion |> not) then
                                failwithf "fUnion returned a bad type in list %A" childTypes
                            printDepth (sprintf "- type: %s has union args: %A" f.PropertyType.Name childTypes)
                            childTypes
                        else
                            printDepth (sprintf "- type: %s non union args: %A" f.PropertyType.Name  (fNonUnionArg f.PropertyType))
                            [ fNonUnionArg f.PropertyType ])
                    |> List.ofSeq
                let allCombinationsOfFieldPossibles = 
                    cartesianProduct2 constructorArgs
                    |> Seq.map List.ofSeq
                    |> List.ofSeq
                uc, allCombinationsOfFieldPossibles
            // with help from http://stackoverflow.com/a/4470670/57883
            let unionCases = getUnionCases t
            let caseTypes =
                unionCases
                |> Seq.map (makeCaseTypes (loop (depth + 1)) fNonUnionArg)
                |> List.ofSeq
            let result: CreatedUnion list = 
                caseTypes
                |> Seq.map (fun (uc,allFieldComboCases) -> 
                    allFieldComboCases 
                    |> Seq.map (makeUnion uc)
                    |> Seq.map (fun instance -> 
                        printDepth (sprintf "- name: %s # for instance of: %A" uc.Name instance)
                        instance
                    )
                )
                |> Seq.collect id
                |> List.ofSeq
            result
        loop 0 t

    let matchUnionWithFunction<'a ,'b> (generateTypeParameterFn: Type -> obj) (matchFn: 'a -> 'b) : ('b list)  =
        getAllDUCases generateTypeParameterFn typeof<'a>
            |> List.map (fun case -> case :?> 'a |> matchFn)

    let generateDefaultTypeParameter = 
        function
        | t when t = typeof<string> -> "{VALUE}" :> obj
        | t when t = typeof<decimal> -> Unchecked.defaultof<decimal> :> obj
        | t when t = typeof<double> -> Unchecked.defaultof<double> :> obj
        | t when t = typeof<int32> -> Unchecked.defaultof<int32> :> obj
        | t when t = typeof<int64> -> Unchecked.defaultof<int64> :> obj
        | _ -> null

    type PublicTypeSignature =
        | SimpleTypeSig of TypeName: string
        | ClassTypeSig of TypeName: string * Fields: Fields list
        | RecordTypeSig of TypeName: string * Fields: Fields list
        | UnionTypeSig of TypeName: string * Unions: Fields list
        | TupleTypeSig of Fields: PublicTypeSignature list
        | EnumTypeSig of TypeName: string * FieldTypeName: string * Fields: string list
    and Fields = {
        Identifier: string
        TypeSignature: PublicTypeSignature
    }
    let createTypeSigFields id tsig = { Identifier = id; TypeSignature = tsig }

    let rec getTypesPublicSignature (t: Type) = 
        let publicBindingFlags = BindingFlags.Public ||| BindingFlags.Instance
        let propertiesToPublicSignature = 
            Seq.map (fun (pi: PropertyInfo) -> createTypeSigFields pi.Name (getTypesPublicSignature pi.PropertyType))
            >> Seq.sortBy (fun x -> x.Identifier)
            >> List.ofSeq
        if FSharpType.IsRecord t then 
            RecordTypeSig(t.Name, (FSharpType.GetRecordFields t |> propertiesToPublicSignature)) 
        else if FSharpType.IsTuple t then 
            TupleTypeSig(FSharpType.GetTupleElements t |> Array.map getTypesPublicSignature |> List.ofArray)        
        else if t.IsEnum then 
            EnumTypeSig(t.Name, Enum.GetUnderlyingType(t).ToString(), Enum.GetNames(t) |> List.ofArray)        
        else if isUnion t && not (typeof<Collections.IEnumerable>.IsAssignableFrom(t)) then 
            let ucInfo (uc: UnionCaseInfo) = 
                uc.GetFields() 
                |> Seq.map (fun i -> createTypeSigFields uc.Name (getTypesPublicSignature i.PropertyType))
            let unions = 
                getUnionCases t
                |> Seq.sortBy (fun uc -> uc.Name)
                |> Seq.map (ucInfo)
                |> Seq.collect id
                |> List.ofSeq
            UnionTypeSig(t.Name, unions) 
        else if t.IsClass && t <> typeof<String> && not (typeof<Collections.IEnumerable>.IsAssignableFrom(t)) then
            let properties = t.GetProperties(publicBindingFlags) |> propertiesToPublicSignature
            let fields = 
                t.GetFields(publicBindingFlags)
                |> Seq.map (fun fi -> createTypeSigFields fi.Name (getTypesPublicSignature fi.FieldType))
                |> Seq.sortBy (fun x -> x.Identifier)
                |> List.ofSeq
            ClassTypeSig(t.ToString(), properties@fields)  
        else   
            SimpleTypeSig(t.ToString())  

    let rec toSignatureString signature =
        let fieldToString { Identifier = ident; TypeSignature = typeSig } = sprintf "%s:%s" ident (toSignatureString typeSig)
        match signature with
        | SimpleTypeSig(typeName) -> typeName
        | ClassTypeSig(typeName, fields) -> sprintf "%s={%s}" typeName (String.Join("#", fields |> List.map fieldToString))
        | RecordTypeSig(typeName, fields) -> sprintf "%s={%s}" typeName (String.Join(";", fields |> List.map fieldToString))                     
        | UnionTypeSig(typeName, unions) -> sprintf "%s=%s" typeName (String.Join("", unions |> List.map (fieldToString >> sprintf "|%s")))
        | TupleTypeSig(fields) -> sprintf "(%s)" (String.Join(",", fields |> List.map toSignatureString))
        | EnumTypeSig(typeName, fieldTypeName, fields) -> sprintf "%s:%s={%s}" typeName fieldTypeName (String.Join(",", fields))      
