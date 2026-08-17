---
open-forge:
  description: Keep semantic documents, exact implementation contracts, generated reference, and conformance evidence under one source-of-truth policy
  tags: [Pattern, Software, Contract, Documentation, Testing, SourceOfTruth]
---

# Contract Ownership

## Shape

Give each contract fact one authoritative owner:

| Concern | Owner |
| --- | --- |
| Meaning, safety invariant, and compatibility promise | Current-truth contract document |
| Exact field, value, schema, signature, or importable interface | Production source or declared external contract |
| Executable behavior and cross-boundary compatibility | Conformance test and stable fixture |
| Reader reference requiring exact source structure | Generated output from the authoritative implementation definition |
| Reusable implementation organization | Pattern |

During design, a document or Pattern may show a clearly labelled schematic contract. Once an exact implementation definition exists, remove copied declarations or replace them with generated reference. Do not manually synchronize exact source contracts into explanatory Markdown.

## Development Flow

1. Change or accept the semantic current truth.
2. Change the exact implementation contract at its narrowest shared scope.
3. Update focused conformance evidence and stable fixtures.
4. Regenerate exact reader reference when one is required.

An internal refactor that preserves semantics changes implementation and evidence without rewriting the semantic document. Public protocol values may appear in reader-facing documentation when users must know them, while the authoritative definition and conformance evidence prevent drift.

## Review Checks

- Every contract fact has one clear authoritative owner.
- Exact implementation declarations are not copied into maintained explanatory documents.
- Semantic promises remain legible without reading implementation.
- Public structured contracts have executable conformance evidence.
- Generated reference is reproducible from the exact source.
