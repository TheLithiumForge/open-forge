---
open-forge:
  description: Pre-specified execution plan for one slice, written ahead of time so a strict executor needs no judgement and the record survives forgotten architecture updates
  tags: [Pattern, Planning, Task, Execution, Delegation, Evidence]
---

# Work Plan

## When to use

One file per slice, written **before** implementation, when the executing agent
is strict rather than deliberative, or when the slice must be reviewable ahead of
the work. Do not use it for exploratory work where the next step depends on what
the last one found.

## Shape

```markdown
---
open-forge:
  description: <slice id> execution plan
  tags: [Memory, Working, Task, Plan, Contextual]
---

# <Slice ID> — <short name>

## Goal

One sentence. Observable from outside the code.

## Depends on / Blocks

Slice IDs only. No prose.

## References

Exact `path:line` for every site to change, plus the decision or contract that
authorizes it. A reference the executor must search for is a defect in the plan.

## Preconditions

- [ ] Verifiable before starting, with the command that verifies it.

## Steps

Numbered. Each step names the exact file, the exact change, and the command that
proves it. One step is one reviewable edit plus its verification.

## Expected result

Observable before and after. Include the exact output shape when output changes.

## Acceptance

- [ ] Gates that must all hold before the slice is done.

## Divergences observed

Empty until implementation. Every entry: what the plan said, what was actually
found, what changed, and whether a durable record needs updating.

## Rollback

How to undo the slice if it is abandoned midway.
```

## Rules

- **Checkboxes are live state.** The executing agent ticks them as it goes. The
  plan file is execution state, not an immutable spec.
- **No judgement left in Steps.** If a step needs the executor to decide scope,
  ownership, naming, or whether something is safe, that decision belongs in the
  plan before execution, or the slice is not ready to delegate.
- **Every step carries its verification.** A step whose result cannot be checked
  is a step that will be reported done without being done.
- **Divergences are mandatory, not optional.** The section exists because plans
  are wrong in small ways and the difference between plan and reality is the most
  perishable knowledge in the work. Recording it is what lets architecture be
  rebuilt from the plans when someone forgets to update it directly.
- **A plan is not authority.** It executes an already accepted decision. If a
  step would change a public contract, dependency, or safety boundary that no
  decision covers, stop and record it under Divergences.

## Boundaries

This shape is for execution, not for deciding. The decision it executes lives in
its own accepted record; the plan links to that record rather than restating its
reasoning.

Related: [Work Records](work-records.md) covers task outcome and state. This
covers one slice of the doing.
