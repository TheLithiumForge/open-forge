---
open-forge:
  description: Completed migration from the eight-section phase-aware Workflow model to a smaller goal, steps, completion, and optional dependency contract
  tags: [Memory, Archived, Session, Contextual, Historical, Workflow, Migration, CLI, Extension]
---

# Workflow Migration

Status: completed on 2026-07-29.

## Accepted Result

Open Forge retains Workflows as an optional Core primitive for repeatable Markdown recipes.

Every complete Workflow now contains one non-empty level-2 `Goal`, `Steps`, and `Completion` section in that order. `Required Routes` appears between `Goal` and `Steps` only when unconditional routed dependencies exist. Recipe-specific headings may clarify one recipe without expanding the Framework schema.

Descriptions and topical tags replace the former fixed phase vocabulary. Steps express boundaries, branching, iteration, delegation, capability use, and stopping behavior only where the recipe needs them. Direct execution remains valid, and Workflow-specific commentary, active-step, primary-Workflow, and closeout ceremony is no longer mandatory.

## Preserved Distinctions

- Generated `Entries` express containment.
- `Required Routes` express unconditional routed dependencies and remain deterministically followable.
- Missing required dependencies remain blockers.
- Organizational Workflow `entrypoints` may omit recipe sections.
- Other Core `root routes` compose through explicit links instead of becoming miniature trees beneath Workflows.
- Ordinary Handoffs preserve Workflow or step position when that information materially helps resumption.

## Removed Universal Machinery

- `Mode`
- Mandatory `Constraints`
- Mandatory `Loop`
- Mandatory `Outputs`
- Fixed phase tags and phase-aware validation
- `none` as a required dependency sentinel
- Workflow-first selection for every non-trivial task
- Mandatory active Workflow naming

The first-party recipes retain useful boundaries, iteration, results, and evidence inside their `Steps` and `Completion` sections.

## Inputs And Current Sources

- [Workflow overhaul inputs](../ideas/2026-07-29_workflow-overhaul-inputs.md)
- [Earlier Workflow redesign](../ideas/workflow-redesign.md)
- [Workflow shape decision](../../crystallized/decisions/workflow-shape.md)
- [Current Workflow contract](../../crystallized/documents/framework/primitives/workflows.md)
- [Workflows Maintenance contract](../../crystallized/documents/maintenance/payload/agents/workflows.md)
- [Installed Workflows entrypoint](../../../workflows/_workflows.md)

## Verification

- The complete test suite passed with 25 unit tests and 163 closure tests.
- Root and installable-source `doctor` passed.
- Generated indexes were rebuilt.
- The build and package dry-run passed with 118 packaged files.
- The source Markdown punctuation and retired Workflow vocabulary scans passed.
- `git diff --check` passed.
