---
open-forge:
  description: "Keep task outcomes, planned steps, and current state predictable without duplicating project knowledge"
  tags: [Extension, Pattern, Planning, Task, Memory]
---

# Work Records

## Shape

| Task content  | Defines                                                                 | Status                         |
| ------------- | ----------------------------------------------------------------------- | ------------------------------ |
| Outcome       | Accepted result, scope, exclusions, constraints and completion evidence | Default content responsibility |
| Plan          | Short sequence, dependencies and verification                           | Default content responsibility |
| Current State | Continuation progress, decisive evidence, blockers and next action      | Default content responsibility |

These are content responsibilities, not a mandatory schema. Existing project headings may express them. Names and order are illustrative.

```text
declared task source
  Task: outcome + plan + current state
    -> separate Plan, when sequence needs independent maintenance
    -> Checkpoint, when additional live continuation state is needed
Backlog -> selected Task
Handoff -> sealed actual-transfer boundary, when required
current project knowledge -> its defining local document or external system
```

| Optional separate record | Use when                                               | Default sections                                                                             |
| ------------------------ | ------------------------------------------------------ | -------------------------------------------------------------------------------------------- |
| Plan                     | Sequence and dependencies need independent maintenance | Outcome Source, Steps And Dependencies, Verification And Completion                          |
| Backlog                  | Candidate work needs selection and priority view       | Scope, Items                                                                                 |
| Checkpoint               | Additional live state is needed to resume a workstream | Goal And Sources, Current State, Accepted Direction And Evidence, Open Questions, Next Steps |

Links express fixed relationships when those optional records exist. Repeated Tasks and steps are allowed. A separate file should answer a separately maintained question instead of repeating the Task in another format.

## Applicability And Sources

The project's declared task source and established conventions come first. This is a default where no suitable structure exists. Small work may need only the current conversation. Ideas, Analysis and Decisions are optional, with no mandatory predecessor chain.

- [Work Record Relationships](../directives/planning/work-records.md) binds source relationships and completion behavior without making this default schema mandatory.
- [Planning workflow](../skills/use-workflow/references/planning/planning.md) supplies the procedure.
- [Planning Templates](../templates/planning/_planning.md) provide optional starting files.
