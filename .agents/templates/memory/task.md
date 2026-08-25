---
open-forge:
  description: Starting structure for a bounded task with a clear problem, outcome, execution profile, authority, acceptance, and linked context
  tags: [Template, Memory, Working, Task, Contextual, Execution]
execution:
  profile: null
  review-budget: null
  council-budget: null
  correction-budget: null
---

# {Task}

{
Use this Template only when the current request or external task source cannot preserve the task accurately. Keep one mutable task source. This record defines what and why; use a linked Plan only when sequencing, parallel lanes, or verification need more detail. Replace every null execution value with an explicit non-negative budget before invoking an external review, council, or correction owner. Remove unused sections and this guidance.
}

## Task State

- State: {Local state vocabulary.}
- Responsible person or role: {Accountable owner.}
- Task source: {This file or external system.}
- Last updated: {Date or timestamp when freshness matters.}

## Problem And Expected Outcome

- Problem: {Current condition, evidence, and consequence.}
- Known or suspected cause: {Distinguish observed cause, hypothesis, or `Unknown`.}
- Expected outcome: {Observable changed state and value.}
- Execution profile: {Must match `execution.profile`: Direct, Standard, Assured, Derivative, or Batch, with reason.}

## Relationships

| Relationship              | Link                   | Relevance                                         |
| ------------------------- | ---------------------- | ------------------------------------------------- |
| Parent or program         | {Link or `None`}       | {Why this task exists.}                           |
| Child or dependency       | {Link or `None`}       | {Outcome, dependency, or blocking relationship.}  |
| Archetype or golden slice | {Link or `None`}       | {Pattern inherited or established.}               |
| Plan                      | {Link or `Not needed`} | {Execution graph.}                                |
| Checkpoint or handoff     | {Link or `Not needed`} | {Resumption or transfer.}                         |
| Backlog or roadmap        | {Link or `None`}       | {Selection, priority, or follow-up relationship.} |
| External task source      | {Link or `None`}       | {Mutable fields it defines.}                      |

## References And Authority

| Source | Question it answers | Status or authority                                      | May this task change it? |
| ------ | ------------------- | -------------------------------------------------------- | ------------------------ |
| {Link} | {Question}          | {Current, candidate, generated, historical, or external} | {Yes, no, or boundary}   |

## Accepted Architecture And Decisions

- Invariants: {Dependency, safety, compatibility, lifecycle, or behavior invariants.}
- Placement map: {Responsibility to local, nearest-shared, or foundational owner.}
- Accepted decisions: {Links or concise task-local decisions.}
- Decisions needed: {Only material choices that block or change the task, or `None`.}

## Scope And Paths

### Included

{Behaviors and deliverables.}

### Excluded

{Non-goals, deferred work, forbidden effects, and protected systems.}

### Constraints

{Compatibility, safety, time, quality, technology, policy, reversibility, environment, or external-effect constraints.}

| Scope kind                      | Paths or surfaces               | Meaning                                                                          |
| ------------------------------- | ------------------------------- | -------------------------------------------------------------------------------- |
| Expected                        | {Forecast paths}                | Likely changes, not a hard allowlist.                                            |
| Protected                       | {Hard boundaries}               | Must not change without a new decision.                                          |
| Direct integration neighborhood | {Adjacent consumers or support} | May change only when accepted meaning directly requires it and must be reported. |

## Assumptions, Prerequisites, Resources, And Recovery

| ID  | Kind                                                      | Claim, required state, resource, or risk           | Validation, availability, or signal   | Owner or source                 | Response if false or triggered                  |
| --- | --------------------------------------------------------- | -------------------------------------------------- | ------------------------------------- | ------------------------------- | ----------------------------------------------- |
| A1  | {Assumption, prerequisite, dependency, resource, or risk} | {What execution relies on or must protect against} | {How and when to establish the state} | {Person, role, system, or link} | {Replan, recovery, rollback, decision, or stop} |

## Behavior And Acceptance Matrix

| ID  | Behavior or condition                                        | Evidence tier                             | Expected observation | Source or verifier                           |
| --- | ------------------------------------------------------------ | ----------------------------------------- | -------------------- | -------------------------------------------- |
| B1  | {Success, boundary, failure, safety, or regression behavior} | {Direct, integration, public, end-to-end} | {Observable result}  | {Command, artifact, system, person, or role} |

## Execution Capsule

- Current owner: {Primary or delegated owner.}
- Current boundary: {Planning, contract, Red, implementation, review, correction, acceptance, or local vocabulary.}
- Dependencies: {Inputs and predecessor outputs.}
- Focused evidence: {Commands or inspections.}
- Integration or full gate: {Boundary and commands.}
- Review budget: {Must match `execution.review-budget`; name each lens and consumption ID.}
- Council budget: {Must match `execution.council-budget`; name each authorized round.}
- Correction budget: {Must match `execution.correction-budget`; name each consumed cycle.}
- Stop conditions: {Plan gap, protected path, safety, external effect, or invalid assumption.}
- Next action: {Exact next useful action.}

## Findings And Corrections

| ID   | Severity               | Finding                         | Disposition                                                        | Owner   | Recheck evidence   |
| ---- | ---------------------- | ------------------------------- | ------------------------------------------------------------------ | ------- | ------------------ |
| {R1} | {Blocking or material} | {Concise evidence-backed issue} | {Accepted, rejected, duplicate, preference, false positive, fixed} | {Owner} | {Targeted recheck} |

## Progress And Evidence

- Current result: {What changed or was learned.}
- Evidence: {Links or concise results.}
- Blockers: {Current blockers or `None`.}
- Residual risk: {Known unproved boundary or `None`.}

## Completion And Closeout

{Exact completion conditions, durable source updates, integration authority, child-task disposition, residual-risk recording, and whether this Working record should be archived or pruned.}
