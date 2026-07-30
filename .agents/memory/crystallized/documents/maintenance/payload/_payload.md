---
open-forge:
  description: Current maintenance boundary and contracts for reviewed Open Forge installation files
  responsibility: Define the source, repository counterpart, related contracts, and proportionate verification for each reviewed installable file without becoming runtime context
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload]
---

# Payload Maintenance

This route contains repository Maintenance contracts for reviewed files under [`src/open-forge/`](../../../../../../src/open-forge/) and follows the installable source structure as those files are reviewed and maintained.

Users do not receive this Maintenance route. The [Framework distribution contract](../../framework/architecture.md#distribution-and-dogfood) defines what must remain complete in installed files, while the [routed Markdown contract](../../framework/markdown/routes.md) defines generated Entries and their authored sources.

## Axioms

- Every reviewed payload file has a Maintenance contract that identifies its canonical installed source, any repository counterpart, related current contracts, and verification matched to change risk and reach
- The installed source is authoritative for exact runtime wording; Maintenance defines repository obligations without becoming required runtime context
- Review each installed file as the final human-readable and agent-facing product; verify that every operational rule named by Maintenance appears in the source or in an installed route the source directs the reader to load
- Keep shared authored content aligned between the installed source and repository counterpart while allowing generated regions and explicitly local content to differ
- When a Maintenance change affects installed behavior, update the canonical source, any repository counterpart, affected current documents, and behavior tests together
- Replace or define repository-only vocabulary before it enters installed source

## Entries

<!-- open-forge:generated-index:start -->
- [Current maintenance contracts for reviewed files under the installable Open Forge .agents runtime](agents/_agents.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Framework
- [Current maintenance contract for the canonical installed AGENTS.md entry block](AGENTS.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Entry
- [Current maintenance contract for the Claude Code bridge to the canonical AGENTS.md entry](CLAUDE.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Bridge #ClaudeCode
<!-- open-forge:generated-index:end -->
