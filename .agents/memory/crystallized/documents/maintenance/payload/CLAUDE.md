---
open-forge:
  description: Current maintenance contract for the Claude Code bridge to the canonical AGENTS.md entry and Open Forge loader
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Bridge, ClaudeCode]
---

# Claude Code Bridge Maintenance Contract

## Source

[`src/open-forge/CLAUDE.md`](../../../../../../src/open-forge/CLAUDE.md) is the canonical installed bridge for Claude Code. The repository [`CLAUDE.md`](../../../../../../CLAUDE.md) dogfoods the same managed block.

The [source and packaging decision](../../../decisions/framework/source-and-packaging.md) is authoritative for keeping [`AGENTS.md`](AGENTS.md) canonical while provider-native imports make required baseline loading cheaper and more reliable.

## Contract

- The managed block contains only its boundary markers, the exact `@AGENTS.md` import, and the exact `@.agents/loader.md` import.
- The [AGENTS entry contract](AGENTS.md) remains the canonical root instruction contract.
- The [loader maintenance contract](agents/loader.md) governs the detailed Open Forge contract imported directly by the bridge.
- Direct loader import removes an agent-decided read step without copying either canonical source into `CLAUDE.md`.
- The canonical and dogfood managed blocks remain identical.

### Installation

- The bridge follows the [managed root entry pattern](../../../../../patterns/open-forge/managed-root-entry.md), currently implemented by the [frozen MVP CLI](../../../../../../src/cli-mvp/cli.ts) until the Framework installation slice is ported.

### External Contract

- [Claude Code](https://code.claude.com/docs/en/memory#import-additional-files) loads `CLAUDE.md`, expands both imports, and resolves their relative paths from the bridge file.

## Verification

- The `patches canonical and bridged root entries without replacing workspace instructions` case in [`src/cli-mvp/cli.closure.test.ts`](../../../../../../src/cli-mvp/cli.closure.test.ts) verifies both exact imports, source/dogfood managed-block equality, workspace-content preservation, and idempotence.
- The `rejects malformed or duplicate managed root entry markers before mutation` case verifies valid marker topology and failure atomicity.
- Recheck the linked Claude Code documentation when bridge syntax or loading behavior changes.
