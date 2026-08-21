---
open-forge:
  description: Experimental starting structure for an executable plan with dependencies, parallel work, resources, verification, decisions, risks, and continuity links
  tags: [Template, Memory, Working, Plan, Planning, Contextual, Experimental]
---

# {Task} Plan

{
Template selection:

- Need: An accepted outcome requires enough sequencing, coordination, or verification that an inline list would be ambiguous.
- Primary question: How will the Task reach its outcome, including dependencies, parallel work, resources, integration, and proof?

Instantiate this file under the appropriate Working route and link the Task that defines the problem, outcome, scope, and acceptance. The Task remains the source for what must be accomplished. This Plan defines how to accomplish it and may track the state of its own steps.

Use one source for execution state. If an external system or another plan defines the live sequence, link it and keep only the local context that system does not provide. Create child Tasks only for coherent outcomes that need independent scope, state, evidence, or coordination.

Keep the Plan current while it is in use. A Checkpoint may link here and preserve only the current position, next action, blockers, and minimum resumption context. A Handoff remains a separate sealed transfer snapshot.

Keep only sections and fields that reduce execution or resumption risk. Replace the frontmatter, title, and placeholders, then remove this braced guidance.
}

## Task And Planning Boundary

- Task: {Link to the Task or declared external task source.}
- Plan state: {Draft, ready, active, blocked, complete, superseded, or another locally defined state.}
- Planning authority: {Who may change the sequence, scope allocation, or decision points?}
- Last updated: {Date or timestamp when freshness matters.}
- Current step or lane: {Step ID, parallel lane, or `Not started`.}

{State which questions this Plan answers and which remain with the Task, an accepted document, a Decision, an external system, or the user.}

## Planning Basis

{Summarize only the Task meaning needed to understand the Plan, then link to the source for full detail.}

- Outcome: {Linked summary of the expected result.}
- Acceptance: {Linked summary of the decisive evidence.}
- Constraints and non-goals: {Linked summary of the boundaries that shape execution.}
- Assumptions: {Only assumptions that change sequencing, resources, or verification.}

### References And Authority

| Source | Question it answers | Status or authority                                                                                 | Use in this Plan                                            |
| ------ | ------------------- | --------------------------------------------------------------------------------------------------- | ----------------------------------------------------------- |
| {Link} | {Relevant question} | {Accepted current source, candidate, historical evidence, generated surface, or external authority} | {Input, constraint, output, verification, or update target} |

## Approach

{Explain the execution strategy, why the ordering is appropriate, and the important invariants to preserve. Name alternatives only when they explain a consequential choice or a replan trigger.}

## Prerequisites

| ID  | Prerequisite | Required state and evidence | Responsible source or role      | Blocks                     |
| --- | ------------ | --------------------------- | ------------------------------- | -------------------------- |
| P1  | {Condition}  | {What proves readiness}     | {Link, person, role, or system} | {Step IDs or `Plan start`} |

## Resources

{Include only resources the Plan must acquire, reserve, prepare, or coordinate. Examples include people, roles, tools, environments, data, credentials, budgets, hardware, external services, and review capacity. Do not place secrets in the Plan.}

| Resource   | Purpose            | Availability or source                          | Needed by  | Responsible person or role   |
| ---------- | ------------------ | ----------------------------------------------- | ---------- | ---------------------------- |
| {Resource} | {Why it is needed} | {Ready, missing, constrained, or linked source} | {Step IDs} | {Who secures or supplies it} |

## Work Graph

{Give every step a stable ID. `Depends on` names prerequisites that must complete first. A parallel lane groups work that can proceed concurrently without conflicting ownership. Use a linked child Task when a step needs independent scope or state.}

| ID  | State     | Action and observable result | Depends on                | Parallel lane            | Linked Task      | Verification        |
| --- | --------- | ---------------------------- | ------------------------- | ------------------------ | ---------------- | ------------------- |
| S1  | {Pending} | {Action and result}          | {P1, step IDs, or `None`} | {Lane A or `Sequential`} | {Link or `None`} | {Evidence produced} |

### Parallel Lanes

{Remove this section when all work is sequential. Give concurrent work non-overlapping mutation boundaries or an explicit coordination rule.}

| Lane     | Steps      | May start when         | Owned or allowed surfaces                        | Shared dependency                                 | Integration point                             |
| -------- | ---------- | ---------------------- | ------------------------------------------------ | ------------------------------------------------- | --------------------------------------------- |
| {Lane A} | {Step IDs} | {Prerequisite or step} | {Files, systems, artifacts, or responsibilities} | {Read-only source, interface, fixture, or `None`} | {Step or evidence gate where results combine} |

## Step Details

{Use one subsection for each step that needs more detail than the Work Graph. Omit it for self-explanatory steps.}

### {S1: Step Name}

- Purpose: {Why this step exists.}
- Inputs: {Required sources, artifacts, decisions, and predecessor outputs.}
- Actions: {Concrete work to perform.}
- Allowed changes: {Files, systems, or effects this step may change.}
- Protected boundaries: {What this step must not change or decide.}
- Output: {Observable result or artifact.}
- Verification: {How to establish that the output is correct.}
- Handoff or integration: {Recipient, linked Task, or next step.}
- Replan or stop condition: {Evidence that invalidates the step or requires a decision.}

## Decision Points

| ID  | Decision   | Options or recommendation                       | Decision-maker   | Needed before       | Resulting branch       |
| --- | ---------- | ----------------------------------------------- | ---------------- | ------------------- | ---------------------- |
| D1  | {Question} | {Options, tradeoff, and current recommendation} | {Person or role} | {Step ID or effect} | {How the Plan changes} |

## Risks, Recovery, And Stop Conditions

| Risk or trigger              | Affected steps | Safeguard                       | Recovery, rollback, or stop response    |
| ---------------------------- | -------------- | ------------------------------- | --------------------------------------- |
| {Risk or observable trigger} | {Step IDs}     | {Prevention or early detection} | {Bounded recovery or decision boundary} |

## Verification And Integration

{Describe how step evidence combines into Task acceptance. Include focused checks, cross-step integration, end-to-end or public scenarios, review, generated or packaged projections, and final authority updates only when they apply.}

| Gate   | Inputs         | Verification                               | Pass condition         | Resulting update or next step                               |
| ------ | -------------- | ------------------------------------------ | ---------------------- | ----------------------------------------------------------- |
| {Gate} | {Step outputs} | {Command, inspection, review, or scenario} | {Observable threshold} | {Task state, source update, integration step, or next gate} |

## Coordination And Continuity

- Child Tasks: {Links and the outcome each contributes, or `None`.}
- Checkpoint: {Link to concise current resumption state, or `Not needed`.}
- Handoffs: {Links to sealed transfer snapshots, or `None`.}
- Related plans, backlogs, or roadmaps: {Links or `None`.}
- Update points: {Steps, decisions, integrations, or evidence gates after which this Plan and any active Checkpoint must be refreshed.}
- Resumption path: {Minimum sources to read and the exact next action after interruption.}

## Completion

{State when execution of this Plan is complete. Include Task acceptance, integration of parallel lanes and child Tasks, required verification, current-source and backlink updates, unresolved-risk disposition, and the Task-state update at its declared source. State whether this Working Plan should then be archived or pruned.}
