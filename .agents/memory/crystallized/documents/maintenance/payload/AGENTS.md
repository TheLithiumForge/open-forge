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

- The managed block starts with the top-level ATX heading `# Open Forge` and ends with the standalone strong paragraph `**End of Open Forge managed section.**`. Between them it contains only a short explanation that Open Forge provides workspace rules and context, and three instructions: read the loader before a task, select every relevant scope including nested scopes, and follow the loaded rules. These instructions remain ordinary Markdown.
- The exact legacy `<!-- open-forge:start -->` and `<!-- open-forge:end -->` pair remains input-only compatibility. Root Update converts it under the existing ownership, recovery, preview, confirmation, and expected-state rules. New Install output uses the canonical heading and footer, and managed divergence remains Install-blocking.
- The [loader maintenance contract](agents/loader.md) governs the installed loader, which remains authoritative for detailed authority, routing, tag, loading, and conflict behavior.
- The canonical and dogfood managed blocks remain identical.

### Installation

- The source is the canonical entry in the [managed root entry pattern](../../../../../patterns/open-forge/managed-root-entry.md), implemented by the [native Install command](../../../../../../src/cli/operations/OpenForge.Cli.Operations/Commands/Install/InstallOperation.cs).

### Harness Integration

- The [Claude bridge contract](CLAUDE.md) imports this canonical entry as its Open Forge instructions.

## Verification

- [Install integration tests](../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Install/InstallOperationIntegrationTests.cs) verify exact installed AGENTS and CLAUDE hosts with the canonical heading and footer.
- [Managed-host Update tests](../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Update/UpdateManagedHostIntegrationTests.cs) verify legacy conversion, invalid-boundary blocking, preservation of outside bytes, and repeated canonical no-op behavior.
- Check source and dogfood block equality directly when this maintenance contract changes.
