---
open-forge:
  description: Starting structure for an executable plan with a compact capsule, dependencies, parallel lanes, ownership, verification, and continuity
  tags: [Template, Memory, Working, Plan, Planning, Contextual, Execution]
---

# {Task} Plan

{
Use when sequence, ownership, parallelism, or verification would make an inline plan ambiguous. The linked Task defines the outcome. Keep one live execution-state source, a short Work Graph, and step detail only where execution needs it. Replace prompts and remove this guidance and unused optional sections without dropping required controls or authority relationships.
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
- Dependencies: {Links to prerequisites and predecessor outputs defined below.}
- Focused evidence: {Per-step checks.}
- Integration or full gates: {Task, archetype, or batch boundaries.}
- Review budget: {Link to the Task Execution Capsule's maximum and consumed IDs.}
- Council budget: {Link to the Task Execution Capsule's maximum and consumed rounds.}
- Correction budget: {Link to the Task Execution Capsule's maximum and consumed cycles.}
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

{OPTIONAL: Add only detail needed beyond the Work Graph. Link applicable Task constraints and evidence instead of copying them. Keep required execution controls in their defining sources.}

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
