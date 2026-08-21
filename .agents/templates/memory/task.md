---
open-forge:
  description: Experimental starting structure for a bounded task with a clear problem, outcome, hierarchy, scope, dependencies, acceptance, and linked context
  tags: [Template, Memory, Working, Task, Contextual, Experimental]
---

# {Task}

{
Template selection:

- Need: One bounded unit of work needs durable meaning, relationships, state, and acceptance.
- Primary question: What problem must this Task solve, what result is expected, and what is outside its boundary?

Use this experimental Template when a backlog item or current request is not enough to preserve the Task accurately. Keep the current request or a declared external tracker as the task source when it already answers the question. Do not create a competing copy of mutable task truth.

Instantiate this file under the appropriate Working route. The Task may stand alone or link to parent, child, and related Tasks. Hierarchy organizes work but creates no status, authority, or completion behavior unless this Task states it.

This Task defines what and why. Put nontrivial execution order, parallel work, resources, and verification sequencing in a linked Plan. Use a Checkpoint only when concise current state must support resumption without rereading the whole Task and Plan.

Keep only sections that help define, execute, review, or close this Task. Replace the frontmatter, title, and placeholders, then remove this braced guidance.
}

## Task State

{Name the source that defines Task state. Use the workspace's own state vocabulary rather than assuming an Agile lifecycle.}

- State: {Proposed, ready, active, blocked, complete, cancelled, or another locally defined state.}
- Responsible person or role: {Who is accountable for moving the Task forward?}
- Task source: {This file or a link to the external system that defines mutable Task state.}
- Last updated: {Date or timestamp when freshness matters.}

## Problem Statement

{Describe the current condition, who or what it affects, the evidence that it exists, and why it is worth addressing. Separate a known cause from a suspected cause.}

## Expected Outcome

{State the observable result and the value it provides. Describe the changed state, not the implementation steps.}

## Relationships And Backlinks

{Link only relationships that help a reader navigate or understand the Task. Add a reciprocal link from a local parent, child, backlog, plan, or document when that backlink materially improves discovery. A link does not copy the destination's authority or lifecycle.}

| Relationship         | Link                   | Relevance                                                                |
| -------------------- | ---------------------- | ------------------------------------------------------------------------ |
| Parent Task          | {Link or `None`}       | {How this Task contributes to the parent.}                               |
| Child Task           | {Link or `None`}       | {Delegated outcome and whether it blocks this Task. Add rows as needed.} |
| Related Task         | {Link or `None`}       | {Dependency, overlap, follow-up, or another relationship.}               |
| Plan                 | {Link or `Not needed`} | {Execution plan for this Task.}                                          |
| Checkpoint           | {Link or `Not needed`} | {Concise resumption state.}                                              |
| Backlog or roadmap   | {Link or `None`}       | {Where this Task is selected or prioritized.}                            |
| External task source | {Link or `None`}       | {Which mutable fields the external system defines.}                      |

## References And Authority

{List the sources needed to understand or perform the Task. State the question each source answers and whether this Task may change it. Include documents, Decisions, Directives, Patterns, code, tests, data, external systems, prior evidence, and generated surfaces only when relevant.}

| Source | Question it answers | Status or authority                                                                                 | May this Task change it?                    |
| ------ | ------------------- | --------------------------------------------------------------------------------------------------- | ------------------------------------------- |
| {Link} | {Relevant question} | {Accepted current source, candidate, historical evidence, generated surface, or external authority} | {Yes, no, or only within a stated boundary} |

## Scope

### Included

{List the behaviors, deliverables, surfaces, people, or systems this Task covers.}

### Excluded

{List non-goals, deferred work, forbidden surfaces, and external effects that are not authorized.}

### Constraints

{State compatibility, safety, time, quality, technology, policy, reversibility, or environment constraints that shape a valid result.}

## Requirements

{State the capabilities, behavior, content, or other properties the result must provide. Keep solution choices out unless they are accepted constraints. Use stable identifiers when individual requirements need traceability.}

- {Requirement and the reason it matters.}

## Acceptance Evidence

{Define how the expected outcome will be judged. Each item should be observable and should name the evidence that can prove it.}

| Acceptance condition   | Evidence                                                       | Source or verifier                       |
| ---------------------- | -------------------------------------------------------------- | ---------------------------------------- |
| {Observable condition} | {Test, review, artifact, measurement, scenario, or inspection} | {Command, file, system, person, or role} |

## Prerequisites And Dependencies

{Separate conditions that must be true before work starts from dependencies needed during execution. Link a dependency instead of copying its mutable state.}

| Kind         | Item                                         | Required state                   | Source             | Current effect                  |
| ------------ | -------------------------------------------- | -------------------------------- | ------------------ | ------------------------------- |
| Prerequisite | {Condition}                                  | {State required before starting} | {Link or evidence} | {Ready, blocks, or unknown}     |
| Dependency   | {Capability, task, person, system, or input} | {State needed during the Task}   | {Link}             | {Impact on sequence or outcome} |

## Decisions And Assumptions

### Accepted Decisions

{Link accepted Decisions or record Task-local choices whose source, scope, and expected lifetime are clear.}

### Assumptions To Validate

| Assumption   | Why it matters                            | Validation                | Result if false                     |
| ------------ | ----------------------------------------- | ------------------------- | ----------------------------------- |
| {Assumption} | {Affected scope, approach, or acceptance} | {How and when to test it} | {Replan, stop, or another response} |

### Decisions Needed

| Decision   | Decision-maker   | Needed by          | Effect if unresolved                |
| ---------- | ---------------- | ------------------ | ----------------------------------- |
| {Question} | {Person or role} | {Boundary or date} | {What cannot proceed or may change} |

## Risks And Safeguards

| Risk   | Signal                          | Prevention or mitigation  | Recovery or stop condition       |
| ------ | ------------------------------- | ------------------------- | -------------------------------- |
| {Risk} | {Evidence that it is occurring} | {Proportionate safeguard} | {How to recover or when to stop} |

## Progress And Evidence

{Keep Task-level state and decisive evidence here only when this file is the declared Task source. Let the Plan define step state and a Checkpoint define concise resumption state. Link detailed logs and artifacts.}

- Current result: {What has changed or been learned.}
- Evidence: {Links to verification, review, or artifacts.}
- Blockers: {Current blockers or `None`.}
- Next Task-level action: {The next meaningful transition.}

## Completion And Closeout

{State the exact conditions that complete the Task. Include required acceptance, integration, linked-source updates, child-Task disposition, durable outcome extraction, and residual-risk recording. State whether this Working record should then be archived or pruned.}
