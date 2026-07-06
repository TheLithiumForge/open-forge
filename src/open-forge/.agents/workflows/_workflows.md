---
open-forge:
    description: Repeatable agent workflows for reaching a defined goal
    tags: [OpenForge, Core, Workflow, Index]
---

# Workflows

Workflows are repeatable agent workflows for reaching a defined goal.

## Axioms

- Use `Entries` when current work matches a repeatable goal that may have an established workflow.
- Load routes whose path, description, or tags match the current work.
- Every workflow defines its goal, starting context, ordered work shape, expected outputs, and completion or handoff condition.
- A workflow may act as an Open Forge root for a given reusable goal, as such it may include #Core routes of it's own that may take precedence when safe annd allowed.
- Workflow-local material applies only while that workflow is active.
- Prefer safe workflow-local material over broader workspace material for that active workflow; report unresolved conflicts.
- Follow each selected child `entrypoint`'s scope and loading axioms recursively.
- Generated `entries` are navigation and reserved load policy only.

## Entries

<!-- open-forge:generated-index:start -->

- none - No entries - #Empty
  <!-- open-forge:generated-index:end -->
