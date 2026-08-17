---
open-forge:
  description: Current maintenance contract for the canonical installed AGENTS.md entry block
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Entry]
---

# AGENTS Entry Maintenance Contract

## Source

[`src/open-forge/AGENTS.md`](../../../../../../src/open-forge/AGENTS.md) is the canonical installed entry from an agent runtime into Open Forge. The repository [`AGENTS.md`](../../../../../../AGENTS.md) dogfoods the same managed block.

The [source and packaging decision](../../../decisions/framework/source-and-packaging.md) is authoritative for the rationale behind this canonical entry and its harness bridges.

## Contract

- The managed block contains only its boundary markers, title, a short explanation that Open Forge provides workspace rules and context, and three instructions: read the loader before a task, select every relevant scope including nested scopes, and follow the loaded rules.
- The [loader maintenance contract](agents/loader.md) governs the installed loader, which remains authoritative for detailed authority, routing, tag, loading, and conflict behavior.
- The canonical and dogfood managed blocks remain identical.

### Installation

- The source is the canonical entry in the [managed root entry pattern](../../../../../patterns/open-forge/managed-root-entry.md), currently implemented by the [frozen MVP CLI](../../../../../../src/cli-mvp/cli.ts) until the Framework installation slice is ported.

### Harness Integration

- The [Claude bridge contract](CLAUDE.md) imports this canonical entry as its Open Forge instructions.

## Verification

- The `patches canonical and bridged root entries without replacing workspace instructions` case in [`src/cli-mvp/cli.closure.test.ts`](../../../../../../src/cli-mvp/cli.closure.test.ts) verifies managed-block replacement, preservation of workspace text, bridge installation, and idempotence.
- The same case compares the canonical and dogfood managed blocks directly.
