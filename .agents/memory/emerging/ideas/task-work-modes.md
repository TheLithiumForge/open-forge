---
open-forge:
  description: Explore optional simple and sprint task Extensions that preserve one task authority while interoperating openly with external trackers
  tags: [Memory, Idea, Contextual, Candidate, Task, Backlog, Sprint, Extension, Integration]
---

# Task Work Modes

## Opportunity

Offer useful task organization without making Open Forge a mandatory project
manager or requiring every workspace to adopt the same ceremony.

Tasks should remain optional Extensions or local Memory roles. A workspace may
keep task truth in Open Forge, in an external tracker, or in a deliberately
chosen combination with one declared authority for each piece of state.

## Candidate Modes

The optional [Planning Extension](../../../../src/extensions/planning/README.md)
already supplies the Planning Workflow, Work Records Pattern, and Task, Plan,
Backlog, Checkpoint, Idea, Analysis, and Decision Templates. The open question
here is whether distinct task modes and external-tracker exchange add useful
behavior beyond those shipped starters.

### Simple Tasks

The everyday mode uses a small backlog containing concise items or links to
just-in-time task files.

```text
.agents/memory/working/project/
├── _project.md
├── backlog.md
└── tasks/
    ├── _tasks.md
    ├── cli-foundation.md
    └── status.md
```

Create a task file only when a coherent slice needs durable scope, status,
handoff, or evidence. The backlog remains the selection surface and links to
accepted contracts instead of copying them.

The historical [CLI Foundation](../../archived/cli-v2/implementation-history/cli-foundation.md) and [CLI
Development Workflow Task](../../archived/cli-v2/implementation-history/cli-development-workflow.md) showed
the local trial shape: a backlog selects one just-in-time Task, and that Task is
authoritative for its own progress and evidence. This candidate does not copy
either Task schema or turn one orchestration policy into a universal Task
contract.

### Sprint Tasks

The structured mode provides actual sprint planning and state when a team
benefits from it.

```text
.agents/memory/working/project/
├── _project.md
├── product-backlog.md
└── sprints/
    ├── _sprints.md
    └── sprint-12/
        ├── _sprint-12.md
        ├── backlog.md
        └── tasks/
```

Its Templates may capture a sprint goal, selected work, task state, evidence,
and review outcomes. Those fields must be justified by real use before the
Extension is designed.

The two modes may become separate Extensions so users do not install sprint
ceremony to obtain a useful backlog. Their exact package boundary remains open.

## External Systems

Open Forge should welcome existing trackers rather than compete with them.

- A GitHub, Jira, Linear, GitLab, or other declared tracker may remain the
  authoritative task source.
- Local files may contain only the links and working context needed by agents
  or people in the workspace.
- Users may deliberately pull useful knowledge into Open Forge or push durable
  outcomes back out. They should also remove local material that no longer
  earns its context cost.
- Integrations must preserve the declared authority. They should not create an
  automatic second copy of mutable task truth.
- Import, export, pull, push, and tracker mutation require explicit operations
  and suitable authority. Merely selecting a Workflow never performs them.

This openness is part of the product experience: Open Forge helps route the
context needed for work without gatekeeping how people organize that work.

## Orchestration Boundary

The local [Assured Development recipe](../../../skills/use-workflow/references/open-forge/development/_development.md) and [Task Lifecycle](../../../skills/use-workflow/references/open-forge/development/task-lifecycle.md) define their own implementation ownership, frozen boundaries, evidence, and task-update process. Commit freeze points require authorization and useful isolation, review, or recovery value. Separate Blue and Purple passes are conditional.

Those choices belong to the selected local method. They do not define a universal Task primitive. Another Workflow may define a different safe update policy, and an external tracker may enforce its own permissions.

## Current Trials

The shipped [Task Template](../../../templates/planning/task.md) and
[Plan Template](../../../templates/planning/plan.md) provide optional starting
shapes. The Task defines the accepted outcome and current state. A separate
Plan defines sequence when that detail needs its own source. Copying a starter
does not establish a required Task route or one universal lifecycle.

The historical [CLI Foundation](../../archived/cli-v2/implementation-history/cli-foundation.md) and [CLI
Development Workflow Task](../../archived/cli-v2/implementation-history/cli-development-workflow.md) suggest
two complete historical trials. Task and Plan starters have since shipped.
Separate Simple Tasks or Sprint Tasks packages, and tracker import or export,
remain candidates requiring their own evidence and acceptance.

## Evidence Before Promotion

The accepted CLI Foundation satisfies the first item. The remaining evidence
still governs additional mode packaging and tracker integration.

1. Use the provisional simple shape for at least one complete implementation
   cycle.
2. Identify the smallest fields that materially improve resumption,
   orchestration, and review.
3. Test whether a sprint form adds value beyond links to an existing tracker.
4. Validate authority and synchronization with at least one external tracker
   before promising import or export behavior.
5. Package only the mode boundaries that remain independently useful.
