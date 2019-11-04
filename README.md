# FSharp Union Helpers
F#  Discriminative Union helpers

## Features
### getAllDUCases
Returns all Discriminative Union cases for a type

### matchUnionWithFunction
Calls the provided function with all Discriminative Union cases for a type

### getTypesPublicSignature (Work in progress)
Returns a types public signature for classes, records, enums, tuples, unions, etc.

Note: known issues
- not all types tests (move examples to Unit Tests)
- cases in union type signature could do with a more explicit domain model
- no guards or safety for nested types (recursion)

### toSignatureString
Converts a types public signature from above to a printable string.

### Helpers with bindingflags for
- getUnionCases
- makeUnion
- isUnion

## Examples
See /Examples