---
open-forge:
    description: Repeatable markdown workflow recipes for reaching a defined goal
    tags: [LoadNow, Core, Workflow]
---

# Workflows

Workflows are markdown recipes for reaching a defined goal that takes more than one step or more than one skill.

## Axioms

- Before non-trivial work, read `Entries` and load matching workflows.
- Open Forge workflows are routed markdown recipes, not runtime orchestration objects.
- Every workflow defines `Goal`, `Required Routes`, `Steps`, `Loop`, `Outputs`, and `Completion`; it adds `Constraints` only when cross-step invariants exist.
- Generated `Entries` list what a workflow contains; `Required Routes` list what it needs from elsewhere.
- Read every `Required Routes` route before Step 1; a route that cannot be read is a blocker to report, not a step to skip. "none" is a valid value.
- A step may invoke a skill, consult guidance, delegate to a subagent, or hand off to another workflow by route; a sub-workflow's `Required Routes` are read at that activation.
- Delegation handoffs name the workflow route and the active step so the worker enters the same contract.
- A workflow may own workflow-local #Core routes; they apply only while it is active and are preferred over broader routes when safe and allowed, with unresolved conflicts reported.

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
