---
open-forge:
    description: Repeatable markdown workflow recipes for reaching a defined goal
    tags: [OpenForge, Core, Workflow, Index]
---

# Workflows

Workflows are markdown recipes for reaching a defined goal.

## Axioms

- Before non-trivial work, read `Entries` and load matching workflows.
- Load routes whose path, description, or tags match the current work.
- Open Forge workflows are routed markdown recipes, not runtime orchestration objects.
- Every workflow defines its goal, starting context, required skill packages when it uses skills, ordered steps, loop behavior, expected outputs, and completion or handoff condition.
- When a selected workflow lists `Required Skill Packages`, load every listed package before running `Steps`; report missing routes.
- A workflow may act as a local Open Forge root for its reusable goal by owning workflow-local #Core routes.
- Workflow-local #Core routes apply only while that workflow is active; prefer them over broader routes when safe and allowed, and report unresolved conflicts.
- Follow each selected child `entrypoint`'s scope and loading axioms recursively.
- Generated `entries` are navigation and reserved load policy only.

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
