---
open-forge:
  description: Starting structure for an executable plan with a compact capsule, dependencies, parallel lanes, ownership, verification, and continuity
  tags: [Template, Memory, Working, Plan, Planning, Contextual, Execution]
---

# {Task} Plan

{
Use this Template when an accepted outcome needs enough sequencing, ownership, parallelism, or verification that an inline plan would be ambiguous. The linked Task remains the source for what must be accomplished. Keep one live execution-state source. Remove unused sections and this guidance.
}

## Planning Boundary

- Task: {Link or current request.}
- Plan state: {Draft, ready, active, blocked, complete, superseded, or local vocabulary.}
- Execution profile: {Match the linked Task Markdown Execution Capsule: Direct, Standard, Assured, Derivative, or Batch.}
- Planning authority: {Who may change sequence or allocation?}
- Current step or lane: {ID or `Not started`.}
- Last updated: {Date or timestamp when freshness matters.}

## Planning Basis

- Outcome: {Linked summary.}
- Architecture and invariants: {Linked summary.}
- Acceptance: {Decisive evidence.}
- Constraints and non-goals: {Boundaries.}
- Assumptions: {Only claims that change sequence, resources, safety, or verification.}
- Archetype or golden slice: {Link or `None`.}

### References And Authority

| Source | Question it answers | Status or authority                                      | Use in this Plan                             |
| ------ | ------------------- | -------------------------------------------------------- | -------------------------------------------- |
| {Link} | {Question}          | {Current, candidate, generated, historical, or external} | {Input, constraint, output, or verification} |

## Execution Capsule

- Expected paths: {Forecast paths.}
- Protected paths: {Hard boundaries.}
- Direct integration neighborhood: {Adjacent paths allowed only when directly required and reported.}
- Dependencies: {Prerequisites and predecessor outputs.}
- Focused evidence: {Per-step checks.}
- Integration or full gates: {Task, archetype, or batch boundaries.}
- Review budget: {Maximum and consumed IDs from the linked Task Execution Capsule.}
- Council budget: {Maximum and consumed rounds from the linked Task Execution Capsule.}
- Correction budget: {Maximum and consumed cycles from the linked Task Execution Capsule.}
- Stop conditions: {Evidence that requires replan or decision.}
- Resumption path: {Minimum sources and exact next action.}

## Prerequisites, Assumptions, Resources, And Recovery

| ID  | Kind                                                      | Required state, claim, resource, or risk       | Owner or source                 | Validation, availability, or signal   | Blocks or response                                     |
| --- | --------------------------------------------------------- | ---------------------------------------------- | ------------------------------- | ------------------------------------- | ------------------------------------------------------ |
| P1  | {Prerequisite, assumption, dependency, resource, or risk} | {What execution needs or must protect against} | {Person, role, system, or link} | {How and when to establish the state} | {Steps blocked, recovery, rollback, decision, or stop} |

## Work Graph

| ID  | State     | Owner       | Action and observable result | Depends on      | Parallel lane          | Verification |
| --- | --------- | ----------- | ---------------------------- | --------------- | ---------------------- | ------------ |
| S1  | {Pending} | {One owner} | {Action and result}          | {IDs or `None`} | {Lane or `Sequential`} | {Evidence}   |

## Parallel Lanes

{Remove when all work is sequential. Mutation ownership must not overlap unless an explicit coordination protocol exists.}

| Lane     | Steps | May start when | Owned mutation surfaces     | Shared read-only inputs | Integration point |
| -------- | ----- | -------------- | --------------------------- | ----------------------- | ----------------- |
| {Lane A} | {IDs} | {Prerequisite} | {Paths or responsibilities} | {Sources or contracts}  | {Gate or step}    |

## Step Details

### {S1: Step Name}

- Purpose: {Why this step exists.}
- Inputs: {Sources, decisions, and predecessor outputs.}
- Actions: {Concrete work.}
- Expected paths: {Forecast.}
- Protected boundaries: {Hard limits.}
- Direct integration neighborhood: {Permitted adjacent scope.}
- Output: {Observable result.}
- Verification: {Focused and integration evidence.}
- Stop condition: {Plan gap, invalid assumption, safety, or authority boundary.}
- Handoff or integration: {Recipient or gate.}

## Decision Points And Risks

| ID  | Decision or risk   | Signal               | Owner                     | Branch, recovery, rollback, or stop response |
| --- | ------------------ | -------------------- | ------------------------- | -------------------------------------------- |
| D1  | {Question or risk} | {Observable trigger} | {Decision-maker or owner} | {Branch, recovery, or stop}                  |

## Review And Correction

- Review triggers: {Named risks only.}
- Finding ledger: {Link or location for stable IDs and disposition.}
- Correction owner: {Prefer original implementation owner.}
- Recheck rule: {Changed finding IDs and affected context, not automatic full review.}

## Coordination And Continuity

- Child tasks or dependencies: {Links and contributed outcomes, or `None`.}
- Checkpoint: {Link or `Not needed`.}
- Handoffs: {Sealed transfer links or `None`.}
- Update points: {Steps, decisions, or gates that refresh this Plan and any active Checkpoint.}

## Completion

{State when the Plan is complete, including integration of lanes, required evidence, task-state update, durable source reconciliation, residual-risk disposition, and archive or prune behavior.}
