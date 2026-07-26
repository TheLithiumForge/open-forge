---
open-forge:
  description: Current maintenance contract for the canonical installed AGENTS.md entry block
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Entry]
---

# AGENTS Entry Maintenance Contract

## Source

[`src/open-forge/AGENTS.md`](../../../../../../src/open-forge/AGENTS.md) is the canonical installed entry from an agent runtime into Open Forge. The repository [`AGENTS.md`](../../../../../../AGENTS.md) dogfoods the same managed block.

The [source and packaging decision](../../../decisions/source-and-packaging.md) owns the rationale for this canonical entry and its harness bridges.

## Contract

- The managed block contains only its boundary markers, title, identification of Open Forge as the workspace operating contract, and a mandatory instruction to read the loader before any task and follow applicable Open Forge rules and conventions throughout that task.
- The [installed loader](../../../../../../src/open-forge/.agents/loader.md) is the sole owner of detailed authority, routing, tag, loading, and conflict behavior.
- The canonical and dogfood managed blocks remain identical.

### Installation

- The source is the canonical entry in the [managed root entry pattern](../../../../../patterns/open-forge/managed-root-entry.md), implemented by the [CLI](../../../../../../src/cli/cli.ts).

### Harness Integration

- The [Claude bridge contract](CLAUDE.md) imports this canonical entry as its Open Forge contract.

## Verification

- The `patches canonical and bridged root entries without replacing workspace instructions` case in [`src/cli/cli.closure.test.ts`](../../../../../../src/cli/cli.closure.test.ts) verifies managed-block replacement, preservation of workspace text, bridge installation, and idempotence.
- The same case compares the canonical and dogfood managed blocks directly.
