---
open-forge:
  description: Implement deterministic CommonMark-aware source discovery and accepted Find projections
  tags: [Memory, Working, CLI, Task, Find, ReadOnly, Markdown, Contextual]
---

# Implement Find

## Task State

- State: Planned after route inspect.
- Parent: [Read-Only Commands](_read-only.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/find/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/find/behavior.md).

## Expected Outcome

`find` discovers accepted source matches deterministically across the selected
source universe without fuzzy semantics, hidden indexing, network fetches, or
workspace mutation.

## Architecture

- Keep definitions, binding, request, operation, result, and renderers at
  `Commands/Find/`.
- Create `Commands/Find/Shared/Query/` for command grammar and matching semantics.
- Activate Markdig 1.3.2 through one fixed CommonMark pipeline under
  `Framework/Documents/Markdown/` because Find is the first real body consumer.
- Put source enumeration in accepted shared source-catalogue capabilities. Do not
  make Find own another route inventory.
- Separate parsed document facts, match facts, result ordering, and projection.

## Requirements

Map the complete contract first. Preserve exact query semantics, accepted source
universe filters, external-source no-fetch facts, frontmatter/body boundaries,
deterministic ordering, views, JSON, diagnostics, statuses, and next actions.
Unknown or unreadable sources remain typed findings; cancellation retains known
safe matches and incomplete coverage.

## Evidence

Unit covers query grammar, fixed CommonMark semantics, matching, ordering, status,
and renderers. Integration uses real routed Markdown, frontmatter, Unicode,
excluded sources, invalid encoding, links, and cancellation. EndToEnd proves
public queries, filters, streams, exits, no writes, and published AOT.

## Stop Conditions

Stop before adding fuzzy search, semantic ranking, a persistent index, another
Markdown pipeline, dynamic Markdig configuration, or network access.

## Completion

Complete with full contract coverage, route-discovery regressions, package/AOT
proof for Markdig, and shared document facts ready for References and Context.
