---
open-forge:
    description: Repeatable markdown workflow recipes for reaching a defined goal
    tags: [LoadNow, Core, Workflow, Index]
---

# Workflows

Workflows are markdown recipes for reaching a defined goal.

## Axioms

- Before non-trivial work, read `Entries` and load matching workflows.
- Open Forge workflows are routed markdown recipes, not runtime orchestration objects.
- Every workflow defines its goal, starting context, required skill packages when it uses skills, ordered steps, loop behavior, expected outputs, and completion or handoff condition.
- When a selected workflow lists `Required Skill Packages`, read every listed package before running `Steps`; report missing routes.
- A workflow may act as a local Open Forge root for its reusable goal by owning workflow-local #Core routes.
- Workflow-local #Core routes apply only while that workflow is active; prefer them over broader routes when safe and allowed, and report unresolved conflicts.

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
