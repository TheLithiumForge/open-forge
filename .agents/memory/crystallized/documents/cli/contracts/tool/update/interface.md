---
open-forge:
  description: Define exact tool pin updates, renewed consent and shared reference retention
  tags: [Memory, Document, CLI, Contract, Tool, Update, Interface, CurrentTruth]
---

# tool update interface
`tool update` is a write command that replaces a registered tool pin with one exact supported pin from the chosen source.

It doesn't poll for releases. A newer program or pin outside this CLI release's allow-list requires an Open Forge release. Downgrades use the same checks.

Status: V1 core contract decided.

## Syntax

Proposed:

```text
open-forge tool update <id> [--source <path>] [--tool-version <exact>] [--allow-download] [--allow-session-daemon] [--allow-path <path>...] [--automatic] [--dry-run] [global flags]
```

Use the selected source's default unless an exact listed version is supplied. Recorded hosts, root and roles stay with this registration. The help purpose is "Update one registered tool to an exact supported pin."

## Changes

New bytes require consent for their exact descriptors. A changed daemon build, cache or process scope requires matching local consent or renewed consent. Stage separately. Publish the verified new reference before releasing the old one. Keep references and usable artifacts needed by other worktrees.

Run the new pin's declared installer with the recorded configuration. Agent-file edits belong to that process. Required extraction-format or version changes produce rebuild instructions. Update doesn't build an index. An equal pin and equal verified configuration is a no-op.

The shared [tool operations](../../../shared-operation-contract.md#tool-operations) define writer data, recovery and exclusions. `role` is null. `pin` is the exact target pin. Missing source, unsupported tuples and incomplete stores keep their defined outcomes.

## Conformance

Required cases are:

- Exact upgrades and downgrades between pins reviewed in the current release's catalogue.
- Old tool and private Node versions kept while another workspace references them.

- Source default, exact upgrade and exact downgrade.
- Equal-pin verified no-op without downloads or runtime state.
- New artifact and changed process-scope consent.
- New reference published before the old reference is released.
- Other worktree references kept during shared-byte deletion checks.
- Excluded declared outputs and unavailable references block unsafe changes.
- Interruption reports verified partial effects and recovery.

The [behavior](behavior.md) defines staging and publication.
