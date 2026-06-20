---
open-forge:
  description: Open Forge workspace category contract
  tags: [OpenForge, Workspace, Contract]
---

# Open Forge Workspace

This file is the Open Forge managed contract for the workspace route category.

Workspace route files tell agents where important workspace paths live and what those paths mean.

Load `_workspace.md` before choosing workspace route files.

## Axioms

- Workspace routes are navigation.
- A route entry tells an agent where to look.
- The target file or folder owns the detailed truth.
- User route files use clear names that match the local workspace shape.

## Installed Routes

- {forgePath}/ - Agent-owned Open Forge files - #AgentOwned #OpenForge
- {forgePath}/constants.md - Root path constants for Open Forge files - #AgentOwned #Constant
- {forgePath}/loader.md - Agent entrypoint for reading and using this workspace - #AgentOwned #Loader
- {forgePath}/workspace/_workspace.md - Generated index of workspace route files - #AgentOwned #Workspace #Index
- {forgePath}/workspace/_workspace-open-forge.md - Open Forge workspace category contract - #AgentOwned #Workspace
- {docsPath}/ - Human-facing documentation root - #HumanOwned #Docs
- {docsPath}/directives/ - Human-reviewed local rules and local authority - #HumanOwned #Directive #ActiveTruth
- {docsPath}/guides/ - Human-facing explanations and guidance - #HumanOwned #Guide
