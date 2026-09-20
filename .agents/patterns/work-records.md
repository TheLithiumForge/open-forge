---
open-forge:
  description: "Keep task outcomes, planned steps, and current state predictable without duplicating project knowledge"
  tags: [Extension, Pattern, Planning, Task, Memory]
---

# Work Records

Use the project's declared task source and established record conventions. This Pattern supplies a default where no suitable structure exists. Small work may need only the current conversation.

## Start With One Task

| Section | Owns |
| --- | --- |
| Outcome | The accepted result, scope, exclusions, constraints, and completion evidence |
| Plan | The short sequence, dependencies, and verification |
| Current State | Progress needed for continuation, decisive evidence, blockers, and next action |

These are content responsibilities, not a new mandatory schema. Existing project headings may express them. Keep the three answers together until one needs separate scope, lifetime, or coordination.

## Split Only What Needs Its Own Source

| Record | Use when | Default sections |
| --- | --- | --- |
| Plan | Sequence and dependencies need independent maintenance | Outcome Source; Steps And Dependencies; Verification And Completion |
| Backlog | Candidate work needs a selection and priority view | Scope; Items |
| Checkpoint | Additional live state is needed to resume a workstream | Goal And Sources; Current State; Accepted Direction And Evidence; Open Questions; Next Steps |

[Planning Templates](../templates/planning/_planning.md) provide optional starting files. A separate file should answer a separate maintained question, not repeat the Task in another format.

## Keep Relationships Clear

- A Backlog lists work awaiting selection. Once a Task exists, link to it. A listed idea is not automatically accepted work.
- The Task owns the outcome and completion criteria. A separate Plan owns the sequence only when separated; the Task then links to it.
- A Checkpoint owns additional live continuation state. Link to the Task and Plan rather than repeating their specifications or finished history.
- A Handoff, when required by a real transfer, freezes that boundary. It does not replace the live state source.
- An Idea preserves a possibility; Analysis preserves evidence and reasoning; a Decision preserves an accepted choice and rationale. None is a mandatory predecessor to a Task. A small accepted choice may remain in the existing source when no separate record helps.
- Current project knowledge stays in its defining source, whether local documentation or an external system. Task records carry only the context needed to execute.

Each changing fact has one defining source. Use external task systems for the facts assigned to them; local records may add context those systems do not maintain. Do not introduce a second board, status catalogue, or sequence by copying.

## Complete And Retire

Report the delivered outcome, decisive evidence, limitations, and remaining work. Move still-current knowledge into its defining sources before retaining or archiving the working record under workspace rules.

A status records what happened. It does not accept its own result, authorize new scope, or grant permission to integrate or publish.
