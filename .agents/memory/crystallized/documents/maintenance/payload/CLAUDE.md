---
open-forge:
  description: Current maintenance contract for the Claude Code bridge to the canonical AGENTS.md entry
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Bridge, ClaudeCode]
---

# Claude Code Bridge Maintenance Contract

## Source

[`src/open-forge/CLAUDE.md`](../../../../../../src/open-forge/CLAUDE.md) is the canonical installed bridge for Claude Code. The repository [`CLAUDE.md`](../../../../../../CLAUDE.md) dogfoods the same managed block.

The [source and packaging decision](../../../decisions/source-and-packaging.md) owns the rationale for keeping [`AGENTS.md`](AGENTS.md) canonical across harnesses.

## Contract

- The managed block contains only its boundary markers and the exact `@AGENTS.md` import.
- The [AGENTS entry contract](AGENTS.md) remains the sole owner of Open Forge instructions exposed through this bridge.
- The canonical and dogfood managed blocks remain identical.

### Installation

- The bridge follows the [managed root entry pattern](../../../../../patterns/open-forge/managed-root-entry.md), implemented by the [CLI](../../../../../../src/cli/cli.ts).

### External Contract

- [Claude Code](https://code.claude.com/docs/en/memory#agentsmd) loads `CLAUDE.md`, expands `@AGENTS.md`, and resolves that relative import from the bridge file.

## Verification

- The `patches canonical and bridged root entries without replacing workspace instructions` case in [`src/cli/cli.closure.test.ts`](../../../../../../src/cli/cli.closure.test.ts) verifies the exact source block, source/dogfood managed-block equality, workspace-content preservation, and idempotence.
- The `rejects malformed or duplicate managed root entry markers before mutation` case verifies valid marker topology and failure atomicity.
- Recheck the linked Claude Code documentation when bridge syntax or loading behavior changes.
