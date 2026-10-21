---
open-forge:
  description: Current maintenance contracts for reviewed Core and Extension installation files under the Open Forge runtime
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Framework]
---

# Agents Runtime Maintenance

This route contains maintainer contracts for reviewed Core and Extension installation files in the Open Forge agent runtime.

## Axioms

- The canonical routed Markdown and generated-region contracts apply unless a component links a more specific representation
- Each category catalogue keeps its question as a level-2 heading immediately under its title, followed by a short paragraph on what its folder holds.
- A shipped catalogue describes its own folder and never names another route.
- A newly authored category entrypoint with no local Axioms uses one `inherited` sentinel. Omission or an empty local section remains accepted compatibility input with the same inherited meaning. It never combines `inherited` with substantive local Axioms, and it never uses `none` for Axioms.
- A direct sibling Directive file keeps its instructions under a substantive `Instructions` heading. Maintenance leaves retain `Source`, `Contract`, and `Verification` instead of Axioms.
- Keep maintenance Entries aligned with the contracts present in this folder. For contract-only changes, edit the lines by hand and check links. Verify payload navigation or lifecycle changes in isolated fixtures, following the task's command boundary.

## Entries

- [Current maintenance contract for the installable Directives Core category entrypoint](directives.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Core #Directive
- [Current maintenance contract for the installable Guidance Core category and its optional Collaboration Extension](guidance.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Core #Guidance
- [Current maintenance contract for the installable Open Forge loader and its dogfood counterpart](loader.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Framework #Loader #Routing
- [Current maintenance contract for the installable Maps Core category entrypoint](map.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Core #Map
- [Current maintenance contracts for the Core Memory root and its standard state routes](memory/_memory.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #MemoryModel
- [Current maintenance contract for the installable Patterns Core category entrypoint](patterns.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Core #Pattern
- [Current maintenance contract for the installed project guide](README.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Guide
- [Current maintenance contract for the installable Skills Core category entrypoint](skills.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Core #Skill
- [Current maintenance contract for the installable Templates Core category entrypoint](templates.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Core #Template
- [Current maintenance contract for the optional Workflow Support Skill and its recipe catalogue](workflows.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Extension #Workflow
