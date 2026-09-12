---
open-forge:
  description: Keep task outcomes, planned steps, and current state predictable without duplicating project knowledge
  tags: [Extension, Pattern, Planning, Task, Memory]
---

# Work Records

Use this shape when work needs a plan or a durable record for resumption. Use the project's declared task source and established record conventions. These shapes fill gaps where the project has no suitable structure.

Keep a small task in one record. Split records only when they need separate scope, lifetime, or coordination. The current conversation remains enough when durable state would add no value.

## Task Shape

| Section | Contents |
| --- | --- |
| Outcome | Accepted result, scope, non-goals, constraints, and completion evidence |
| Plan | Ordered steps, dependencies, and verification appropriate to each result |
| Current State | Completed work and evidence, work in progress, blockers, unresolved decisions, and next action |

These are default content sections, not required metadata fields. A workspace's established task format may carry the same information under its own headings. If a separate record defines the plan or current state, the corresponding Task section links to it.

## Separate Records

| Record | When it helps | Default sections |
| --- | --- | --- |
| Plan | The sequence and dependencies need independent maintenance | Outcome Source; Steps And Dependencies; Verification And Completion |
| Backlog | Work awaiting selection needs a maintained view of priorities | Scope; Items |
| Checkpoint | Durable sources alone do not provide enough context to resume an active workstream | Goal And Sources; Current State; Accepted Direction And Evidence; Open Questions; Next Steps |

[Planning Templates](../templates/planning/_planning.md) provide optional starting content for these shapes. A separate file is useful only when its question needs its own maintained answer.

## Relationships

A backlog lists work awaiting selection and its priority. Once an item has a task record, the backlog links to it rather than copying its plan or current state. A queued idea is not accepted work merely because it appears in a backlog.

The task defines its outcome and completion condition. A separate plan defines the sequence only when that sequence needs independent maintenance. A Checkpoint keeps the additional current state needed for resumption and links to the task and plan. It does not repeat their complete contents or accumulate a second history of finished work.

Each changing fact has one defining source. Other records link to it. When an external task system already defines a fact, these records refer to that system. A local record may hold useful context that the external system does not maintain.

Accepted decisions record what was accepted and why. Project documents and reusable instructions define their respective subjects. Task records link to them and carry only the context needed for the task.

## Completion And Retention

Record the delivered outcome, decisive evidence, limitations, and any remaining work. Move knowledge that is still current into the sources that should maintain it. Retain or archive task material according to the workspace's lifecycle rules.

Task status records what happened. It does not authorize new scope, accept its own result, or grant permission to merge or publish.
