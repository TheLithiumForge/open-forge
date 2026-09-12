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

- The bridge follows the [managed root entry pattern](../../../../../patterns/open-forge/managed-root-entry.md), implemented by the [native Install command](../../../../../../src/cli/core/OpenForge.Cli.Core/Commands/Install/InstallOperation.cs).

### External Contract

- [Claude Code](https://code.claude.com/docs/en/memory#import-additional-files) loads `CLAUDE.md`, expands both imports, and resolves their relative paths from the bridge file.

## Verification

- [Install integration tests](../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Install/InstallOperationIntegrationTests.cs) verify exact installed AGENTS and CLAUDE blocks and one matching marker pair.
- [Managed-host Update tests](../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Update/UpdateManagedHostIntegrationTests.cs) verify block updates, preservation of outside bytes and repeated no-op behavior.
- Check source and dogfood block equality directly when this maintenance contract changes.
- Recheck the linked Claude Code documentation when bridge syntax or loading behavior changes.
