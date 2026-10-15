---
open-forge:
  description: Pre-specified execution plan for one slice, written ahead of time so a strict executor needs no judgement and the record survives forgotten architecture updates
  tags: [Pattern, Planning, Task, Execution, Delegation, Evidence]
---

# Work Plan

## Shape

Schematic Markdown record. Angle-bracket placeholders stand in for the actual slice. Headings and order show the default shape. The content described in the linked rules is required when using this execution plan, including the Divergences section. IDs, name, paths, commands and evidence are illustrative. Preconditions, Steps, Acceptance and Divergences entries repeat as needed. Divergences is initially empty. Additional sections are optional.

```markdown
---
open-forge:
  description: <slice id> execution plan
  tags: [Memory, Working, Task, Plan, Contextual]
---

# <Slice ID>: <short name>

## Goal

<one externally observable outcome sentence>

## Depends on / Blocks

<predecessor slice IDs> / <blocked slice IDs>

## References

<exact path:line for each change site> -> <authorizing decision or contract link>

## Preconditions

- [ ] <starting condition> -> <verifying command>

## Steps

1. <exact file>: <one reviewable edit> -> <verifying command>

## Expected result

<observable before> -> <observable after, exact output shape when changed>

## Acceptance

- [ ] <completion gate>

## Divergences observed

<!-- Initially empty. When observed: planned / found / changed / durable update. -->

## Rollback

<undo path for an abandoned partial slice>
```

## Applicability And Sources

For one slice, planned before implementation, when the executor is strict or the work needs advance review. Exploratory work needs a different shape.

- [Work Plans Directive](../directives/work-planning/work-plans.md) binds readiness, verification, live state, divergence recording and the authority boundary.
- [Work Records](work-records.md) shows the enclosing task's outcome and current state.
