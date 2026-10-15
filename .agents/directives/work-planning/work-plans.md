---
open-forge:
  description: Keep pre-specified slice execution plans verifiable, live and within accepted authority
  tags: [LoadNow, Directive, Planning, Task, Execution, Delegation, Evidence]
---

# Work Plans

## Instructions

- For a strict executor or a slice that must be reviewable ahead of work, write one plan file per slice before implementation. Do not use this execution shape for exploratory work whose next step depends on the previous finding.
- The [Work Plan Pattern](../../patterns/work-plan.md) illustrates the default record shape. In that record, the Goal is one externally observable sentence. Depends on / Blocks contains slice IDs only, without prose. References supplies exact `path:line` for every change site and the authorizing decision or contract. A reference requiring executor search is a defect.
- Make Preconditions verifiable before starting and include the verifying command. Number Steps, naming the exact file, exact change and proving command. Each step is one reviewable edit plus verification. Expected result gives observable before and after, including the exact output shape when output changes. Acceptance lists every gate required for completion. Rollback explains how to undo an abandoned partial slice.
- Treat checkboxes as live execution state and tick them as execution proceeds. The plan is not immutable.
- Leave no judgement about scope, ownership, naming or safety in Steps. Resolve it in the plan before execution or the slice is not ready to delegate. Every step carries verification.
- Keep Divergences observed empty until implementation, then record every divergence: planned state, actual finding, change and whether a durable record needs updating. These entries are mandatory and preserve plan-to-reality evidence for architecture reconstruction.
- A plan executes an already accepted decision and creates no authority. Link to that decision instead of restating its reasoning. If a step would change an uncovered public contract, dependency or safety boundary, stop and record it under Divergences.
