---
open-forge:
  description: Apply TypeScript source mechanics to generic semantic-contract, generated-reference, and conformance ownership
  tags: [Pattern, TypeScript, Contract, API, Documentation, Testing, SourceOfTruth]
---

# TypeScript Contract Ownership

## TypeScript Specialization

Apply the runner- and language-neutral [Contract Ownership](../../software/contract-ownership.md) Pattern first. This specialization owns only the TypeScript mechanics used to define and validate the exact contract.

- Export exact types and named values from focused source modules.
- Derive union types from their owning readonly const objects when that is clearer than an enum.
- Use `satisfies` for structural checking where useful.
- Do not use workspace-owned `as Type` assertions to manufacture contract conformance.
- Do not maintain handwritten declaration mirrors for documentation.
- Keep runtime validation beside the untrusted boundary it protects.

## Review Checks

- The generic Contract Ownership review checks pass.
- TypeScript declarations and named values remain focused production exports.
- TypeScript assertions do not manufacture workspace-owned contract conformance.
- Runtime validation remains beside its untrusted boundary.
