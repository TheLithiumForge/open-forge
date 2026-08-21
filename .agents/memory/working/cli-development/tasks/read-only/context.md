---
open-forge:
  description: Implement ordered context selection, loading reasons, overlap, size, and token projections
  tags: [Memory, Working, CLI, Task, Context, ReadOnly, Loading, Contextual]
---

# Implement Context

## Task State

- State: Planned after accepted Find and References.
- Parent: [Read-Only Commands](_read-only.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/context/interface.md), [Behavior](../../../../crystallized/documents/cli/contracts/context/behavior.md), and [Technical Design](../../../../crystallized/documents/cli/contracts/context/technical-design.md).

## Expected Outcome

`context` projects exact ordered source sets, loading reasons, overlap, added
context, byte/character size, and accepted token estimates without reading more
bodies than the selected operation requires.

## Architecture

- Keep command models at `Commands/Context/` and local support under
  `Shared/{Selection,Measurement,Rendering}/`.
- Consume shared route loading facts, source catalogue, references, and document
  read capabilities. Do not rebuild Loader traversal or Find.
- Model source identity, inclusion reason, loading event, measurement availability,
  overlap, and ordering as separate immutable facts.
- Keep token estimation explicitly approximate and versioned by one accepted
  algorithm. It is not a model-provider tokenizer.

## Requirements

Implement accepted startup, selected, automatic, later, overlap, exclusion,
ordering, size, unavailable/incomplete, view, JSON, diagnostic, help, status, and
next-action behavior. Preserve exact distinctions among zero, unavailable, and
not applicable.

## Evidence

Unit covers set algebra, reason precedence, ordering, measurements, estimate
rounding, statuses, and projections. Integration uses real loading routes,
overwrites, exclusions, unreadable sources, and unchanged snapshots. Process and
AOT evidence proves public scenarios and stream isolation.

## Stop Conditions

Stop before adding provider-specific tokenization, body caching, semantic ranking,
workspace databases, or a second route-loading model.
