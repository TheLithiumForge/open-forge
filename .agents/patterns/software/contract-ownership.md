---
open-forge:
  description: Keep semantic documents, exact implementation contracts, generated reference, and conformance evidence under one source-of-truth policy
  tags: [Pattern, Software, Contract, Documentation, Testing, SourceOfTruth]
---

# Contract Ownership

## Shape

| Contract fact                                               | Required defining source                        | Relationship                                       |
| ----------------------------------------------------------- | ----------------------------------------------- | -------------------------------------------------- |
| Meaning, safety invariant, compatibility promise            | Current-truth contract document                 | Explains semantics independently of implementation |
| Exact field, value, schema, signature, importable interface | Production source or declared external contract | One exact definition                               |
| Executable behavior, cross-boundary compatibility           | Conformance test and stable fixture             | Verifies the definition and promises               |
| Exact reader reference, when needed                         | Reproducible generated output                   | Derived from the exact definition                  |
| Reusable implementation organization                        | Pattern                                         | Shows the continuing shape                         |

```text
semantic document -> exact definition -> generated reference (optional)
        \                 /
         conformance evidence
```

Source roles and derivation are fixed. Concrete filenames, fields and values are illustrative. A labelled schematic contract is optional during design. Public protocol values may appear in reader documentation when users need them.

## Applicability And Sources

For contracts described by prose, implementation, generated reference and evidence.

- [Contract Ownership Directive](../../directives/software/contract-ownership.md) binds the source boundaries.
- [Contract maintenance procedure](../../guidance/adaptive-design-delivery.md#contract-maintenance) supplies the development sequence.
- [TypeScript specialization](../open-forge/typescript/contract-ownership.md) shows the TypeScript mechanics.
