# FSharp Union Helpers
F#  Discriminative Union helpers

## Features
### getAllDUCases
Returns all Discriminative Union cases for a type

### matchUnionWithFunction
Calls the provided function with all Discriminative Union cases for a type

### getTypesPublicSignature (Work in progress - not fit for usage)
Returns a types public signature for classes, records etc.

Note: known issues
- union cases not useful and are unstable
- types with single case private unions return an empty type signature as a simple type sig
- nullable & others not handled
- no guards or safety nested types

### Helpers with bindingflags for
- getUnionCases
- makeUnion
- isUnion

## Examples
See /Examples