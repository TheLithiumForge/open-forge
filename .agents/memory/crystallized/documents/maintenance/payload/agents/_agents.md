---
open-forge:
  description: Current maintenance contracts for reviewed Core and Extension installation files under the Open Forge runtime
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Framework]
---

# Agents Runtime Maintenance

This route contains maintainer contracts for reviewed Core and Extension installation files in the Open Forge agent runtime.

## Axioms

- The canonical routed Markdown and generated-region contracts apply unless a component links a more specific representation
- A newly authored category entrypoint with no local Axioms uses one `inherited` sentinel. Omission or an empty local section remains accepted compatibility input with the same inherited meaning. It never combines `inherited` with substantive local Axioms, and it never uses `none` for Axioms.
- A direct sibling Directive file exposes exactly one substantive `Instructions` section. Maintenance leaves retain `Source`, `Contract`, and `Verification` instead of Axioms.
- After changing route structure or generated route metadata, rebuild indexes and run `open-forge doctor` in each affected tree, including both the repository and `src/open-forge/` when shared routing changed

## Entries

- [Current maintenance contract for the installable Directives Core category entrypoint](directives.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Core #Directive
- [Current maintenance contract for the installable Guidance Core category and its optional Collaboration Extension](guidance.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Core #Guidance
- [Current maintenance contract for the installable Open Forge loader and its dogfood counterpart](loader.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Framework #Loader #Routing
- [Current maintenance contract for the installable Maps Core category entrypoint](map.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Core #Map
- [Current maintenance contracts for the Core Memory root and its standard state routes](memory/_memory.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #MemoryModel
- [Current maintenance contract for the installable Patterns Core category entrypoint](patterns.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Core #Pattern
- [Current maintenance contract for the installable Skills Core category entrypoint](skills.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Core #Skill
- [Current maintenance contract for the installable Templates Core category entrypoint](templates.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Core #Template
- [Current maintenance contract for the optional Workflow Support Skill and its recipe catalogue](workflows.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Extension #Workflow
