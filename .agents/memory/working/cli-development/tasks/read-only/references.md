---
open-forge:
  description: Implement direct incoming and outgoing reference facts with shared source-universe filters
  tags: [Memory, Working, CLI, Task, References, ReadOnly, Links, Contextual]
---

# Implement References

## Task State

- State: Planned after route inspect; may run in parallel with Find after shared
  source contracts freeze.
- Parent: [Read-Only Commands](_read-only.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/references/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/references/behavior.md).

## Expected Outcome

`references` reports accepted direct incoming or outgoing references for one
source with exact source identity, deterministic provenance, and shared
source-universe filters.

## Architecture

- Keep command models and projections at `Commands/References/`.
- Create `Commands/References/Shared/Extraction/` for command-local reference
  extraction until another consumer proves identical link semantics.
- Consume shared source references, route facts, and Markdown document facts.
- Promote only the smallest identical parsed link fact needed by Find, References,
  or Index. Keep direction, filter validity, rows, findings, and status local.

## Requirements

Implement direct directions, exact and ID subjects, collisions, supported local
reference forms, unchecked external URL facts, include/exclude behavior, filter
validity by direction, unreadable and malformed source handling, deterministic
ordering, views, JSON, diagnostics, help, and no-write behavior.

Do not recurse reference graphs, compute semantic impact, fetch external URLs, or
diagnose broken links beyond the command contract.

## Evidence

Unit covers extraction and normalization rules, direction/filter matrix, ordering,
status, and renderers. Integration uses real Markdown paths, anchors, spaces,
Unicode, external URLs, missing targets, ambiguous IDs, compatibility sources,
and unchanged snapshots. EndToEnd and AOT prove public streams and exits.

## Stop Conditions

Stop if reference extraction requires a competing Markdown parser, if a shared
filter changes meaning by direction, or if a recursive graph is introduced to
answer a direct-reference contract.
