---
open-forge:
  description: Current maintenance contracts for reviewed files under the installable Open Forge .agents runtime
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Framework]
---

# Agents Runtime Maintenance

This route contains maintainer contracts for reviewed files below `src/open-forge/.agents/`.

## Axioms

- The canonical routed Markdown and generated-region contracts apply unless a component links a more specific representation
- A category entrypoint may omit local Axioms, leave the section empty, or use one `inherited` sentinel. It never combines `inherited` with substantive local Axioms, and it never uses `none` for Axioms.
- After changing route structure or generated route metadata, rebuild indexes and run `open-forge doctor` in each affected tree, including both the repository and `src/open-forge/` when shared routing changed

## Entries

<!-- open-forge:generated-index:start -->
- [Current maintenance contract for the installable Directives Core category entrypoint](directives.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Core #Directive
- [Current maintenance contract for the installable Guidance Core category entrypoint](guidance.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Core #Guidance
- [Current maintenance contract for the installable Open Forge loader and its dogfood counterpart](loader.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Framework #Loader #Routing
- [Current maintenance contracts for the installable Memory root and its Working, Emerging, Crystallized, and Archived `routes`](memory/_memory.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #MemoryModel
- [Current maintenance contract for the installable Patterns Core category entrypoint](patterns.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Core #Pattern
- [Current maintenance contract for the installable Skills Core category entrypoint](skills.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Core #Skill
- [Current maintenance contract for the installable Templates Core category entrypoint](templates.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Core #Template
- [Current maintenance contract for the installable Workflows Core category `entrypoint` and validated Markdown recipe schema](workflows.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Core #Workflow
- [Current maintenance contract for the installable Workspace Core category entrypoint](workspace.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Core #Workspace
<!-- open-forge:generated-index:end -->
