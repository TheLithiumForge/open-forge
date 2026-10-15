---
open-forge:
  description: Keep semantic meaning, exact definitions, generated reference and conformance in their defining sources
  tags: [LoadNow, Directive, Software, Contract, Documentation, Testing, SourceOfTruth]
---

# Contract Ownership

## Instructions

- Give each contract fact one authoritative source. Current-truth contract documents define meaning, safety invariants and compatibility promises. Production source or a declared external contract defines exact fields, values, schemas, signatures and importable interfaces. Conformance tests and stable fixtures prove executable behavior and cross-boundary compatibility. Exact reader reference is generated from the authoritative implementation definition. Patterns define reusable implementation organization.
- During design, a document or Pattern may show a clearly labelled schematic contract. Once an exact implementation definition exists, remove copied declarations or replace them with generated reference. Do not manually synchronize exact source contracts into explanatory Markdown.
- Semantic promises remain legible without reading implementation. Public structured contracts have executable conformance evidence. Generated reference is reproducible from the exact source.
- An internal refactor that preserves semantics changes implementation and evidence without rewriting the semantic document. Public protocol values may appear in reader-facing documentation when users must know them, while the authoritative definition and conformance evidence prevent drift.

The [Contract Ownership Pattern](../../patterns/software/contract-ownership.md) shows the source relationships. The [maintenance procedure](../../guidance/adaptive-design-delivery.md#contract-maintenance) describes the development sequence.
