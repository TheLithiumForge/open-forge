---
open-forge:
  description: Apply TypeScript source mechanics to generic semantic-contract, generated-reference, and conformance ownership
  tags: [Pattern, TypeScript, Contract, API, Documentation, Testing, SourceOfTruth]
---

# TypeScript Contract Ownership

## Shape

```text
semantic contract document
  -> focused TypeScript contract module
       named runtime values -> derived union type
       exact exported types
  -> generated exact reader reference, when required
  <- conformance tests + stable fixtures

untrusted input -> adjacent runtime validation -> typed contract value
```

| Part                                                          | Status                             |
| ------------------------------------------------------------- | ---------------------------------- |
| Focused production exports, no handwritten declaration mirror | Required                           |
| Boundary-local runtime validation                             | Required                           |
| Union derived from readonly const values                      | Optional when clearer than an enum |
| `satisfies` for structural checking                           | Optional where useful              |
| Workspace-owned `as Type` to manufacture conformance          | Invalid                            |

Names and filenames are illustrative. Relationships specialize the generic shape.

## Applicability And Sources

For exact TypeScript contracts described by semantic documents, generated reference and conformance evidence.

- [Contract Ownership](../../software/contract-ownership.md) supplies the generic shape.
- [Contract Ownership Directive](../../../directives/software/contract-ownership.md) binds generic source boundaries.
- [TypeScript Source Structure](../../../directives/open-forge/typescript/typescript-source-structure.md#exact-contracts-and-named-values) binds TypeScript mechanics.
