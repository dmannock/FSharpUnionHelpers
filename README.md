# FSharp Union Helpers
F# Discriminated Union Helpers

## Features
### getAllDUCases
Returns all Discriminated Union cases for a type

```csharp
(Type -> Obj) -> Type -> CreatedUnion list
```

Where
- *(Type -> obj)* is a function to call on non-union types which creates a value for that type. For an example see the generateDefaultTypeParameter function.
- *Type* is the union type
- *CreatedUnion* list is a list of union case objects created from the provided union type

### matchUnionWithFunction
Calls the provided function with all Discriminative Union cases for a type

### generateDefaultTypeParameter
Creates a value of the type passed in.

```csharp
Type -> Obj
```

### getTypesPublicSignature
Returns a types public signature for classes, records, enums, tuples, unions, etc.

```csharp
Type -> PublicTypeSignature
```

Note: known issues
- not all types tested (move examples to Unit Tests)
- cases in union type signature could do with a more explicit domain model
- no guards or safety for nested types (recursion)

### toSignatureString
Converts a types public signature from above to a printable string.
```csharp
PublicTypeSignature -> string
```

### Helpers with bindingflags for
- getUnionCases 
```csharp
Type -> UnionCaseInfo[]
```
- makeUnion 
```csharp
UnionCaseInfo -> obj List -> obj
```
- isUnion 
```csharp
Type -> bool
```

## Examples
See [/Examples](/examples/Example.fsx)