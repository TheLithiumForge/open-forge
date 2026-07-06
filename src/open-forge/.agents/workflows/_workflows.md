---
open-forge:
  description: Repeatable goal-oriented agent workflows 
  tags: [OpenForge, Core, Workflow, Index]
---

# Workflows

Workflows are larger goal-oriented agent workflows.

## Axioms

- Use `Entries` when current work matches a repeatable goal that may have an established workflow.
- Load routes whose path, description, or tags match the current work.
- Every workflow defines its goal, starting context, ordered work shape, expected outputs, and completion or handoff condition.
- A workflow may own essential local #Core categories that can scope behavior like: `directives/`, `patterns/`, `guidance/`, and `skills/` categories under its workflow folder.
- Workflow-local material applies only while that workflow is active.
- Prefer safe workflow-local material over broader workspace material for that active workflow; report unresolved conflicts.
- Do not nest another Open Forge root, loader, workspace route, or independent install under a workflow route.
- Follow each selected child `entrypoint`'s scope and loading axioms recursively.
- Generated `entries` are navigation and reserved load policy only.

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
