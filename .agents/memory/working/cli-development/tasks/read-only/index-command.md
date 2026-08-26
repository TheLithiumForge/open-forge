---
open-forge:
  description: Implement deterministic generated Entries projection and idempotent index application
  tags: [Memory, Working, CLI, Task, Index, Generated, Mutation, Contextual]
---

# Implement Index

## Task State

- State: Split for safe execution. [Routed Authored
  Metadata](routed-authored-metadata-foundation.md) is Complete at `5924698`, and
  the pure [Generated Navigation
  foundation](index-generated-navigation-foundation.md) is Complete and
  squash-integrated at `21e5200`. Public Index binding, selection, application,
  result, and presentation remain blocked because the accepted design requires
  workspace locking, post-lock revalidation, atomic replacement, Git policy,
  verification, and recovery while the current Plan places their shared Mutation
  Foundation after Index.
- Parent: [Read-Only Commands](_read-only.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/index-candidate/interface.md), [Behavior](../../../../crystallized/documents/cli/contracts/index-candidate/behavior.md), and [Technical Design](../../../../crystallized/documents/cli/contracts/index-candidate/technical-design.md).

## Expected Outcome

`index` computes deterministic generated `Entries` projections and, when
authorized, applies only those manager-owned regions with preservation,
idempotence, dry-run, and exact failure evidence.

## Architecture

- Keep command source at `Commands/Index/` with local
  `Shared/{Projection,Rendering}/` support.
- Promote reusable generated-navigation parsing and region application to
  `Framework/GeneratedNavigation/` because later mutations, Doctor, Repair, and
  Cleanup consume identical projection identity.
- Separate observation, projection, diff/plan, application, verification, and
  result. Dry run stops before application.
- Index is the first explicit write command but precedes the general lifecycle
  mutation foundation only if its contract-specific bounded apply path is fully
  isolated. Otherwise implement shared lock/atomic primitives first through a
  Plan dependency update.

## Requirements

Preserve authored content outside generated markers, accepted source ordering,
descriptions/tags, overwrite and route semantics, unchanged files, line endings
where contracted, malformed-region blocking, dry-run attention, write policy,
atomic application, post-write verification, human/JSON/diagnostics/help, and
idempotence.

## Child Boundary

- [Routed Authored Metadata](routed-authored-metadata-foundation.md) and the
  [Generated Navigation foundation](index-generated-navigation-foundation.md)
  are Complete and integrated. Extension Inspect implementation remains
  independent.
- Public Index implementation does not start until the maintainer accepts a
  dependency-order correction or another accepted architecture source resolves
  the lock/recovery contradiction. Do not implement an Index-local substitute.

## Evidence

Unit proves projection and region transforms. Integration uses owned real files
for preservation, malformed markers, concurrent-state revalidation, line endings,
no-op, dry run, apply, and second-run idempotence. EndToEnd and AOT prove public
effects, streams, exits, and unchanged unrelated bytes.

## Stop Conditions

Stop before treating generated Entries as authority, rewriting whole files,
creating a persistent index, applying without revalidation, or hiding a needed
shared mutation dependency.
