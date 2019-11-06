# FSharp Union Helpers
F#  Discriminative Union helpers

## Features
### getAllDUCases
Returns all Discriminative Union cases for a type

```(Type -> Obj) -> Type -> CreatedUnion list```

Where
- (Type -> obj) is a function to call on non-union types
- Type is the union type
- CreatedUnion list is a list of union case objects created from the provided union type

### matchUnionWithFunction

Calls the provided function with all Discriminative Union cases for a type

### getTypesPublicSignature
```Type -> PublicTypeSignature```

Returns a types public signature for classes, records, enums, tuples, unions, etc.

Note: known issues
- not all types tested (move examples to Unit Tests)
- cases in union type signature could do with a more explicit domain model
- no guards or safety for nested types (recursion)

### toSignatureString
```PublicTypeSignature -> string```

Converts a types public signature from above to a printable string.

### Helpers with bindingflags for
- getUnionCases ```Type -> UnionCaseInfo[]```
- makeUnion ```UnionCaseInfo -> obj List -> obj```
- isUnion ```Type -> bool```

## Examples
See /Examples