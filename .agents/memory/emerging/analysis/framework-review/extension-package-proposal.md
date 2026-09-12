---
open-forge:
  description: Proposed focused Extension packages and planning starters for Task 28 discussion
  tags: [Memory, Analysis, Contextual, Candidate, Framework, Extension, Planning]
---

# Extension Package Proposal

## Question And Status

Which independently useful capabilities should be installable separately, while keeping the catalogue small enough to understand and maintain?

The user accepted this reduced split and Experience Design removal. The [accepted decision](../../../crystallized/decisions/extensions/focused-extension-packages.md) records the outcome; the [catalogue](../../../../../src/extensions/README.md) defines the resulting packages. This analysis preserves the proposal and planning rationale.

## Current Recommendation

| Package | Content |
| --- | --- |
| Project Documents | Vision and Architecture Workflows; Vision, Principles, Architecture, and Maintenance Contract Templates; Document Templates entrypoint |
| Memory Starters | Analysis, Decision, Handoff, Idea, and Observation Templates; Memory Templates entrypoint |
| Planning | Planning Workflow, Work Records Pattern, and Task, Plan, Backlog, and Checkpoint Templates under one planning Template entrypoint |
| Development | Development, Debugging, and Review Workflows |
| Orchestration | Managed Delivery Workflow, with dependencies on Planning and Development |

Retain `development-toolkit` as an optional bundle of the first four packages. Orchestration remains separately selectable.

This reduces the initial seven-capability proposal by keeping Review with Development and omitting Experience Design. Review remains an independently selectable Workflow after installation. The [Experience Design assessment](experience-design-value.md) explains the recommendation and its evidence limits. Removal was subsequently authorized by the user and is recorded in the accepted decision.

Each package has a coherent selection purpose. Existing installed paths can remain unchanged. Each Template subtree has one package responsible for its entrypoint; Planning receives its own subtree. Shared files have one package source and are supplied through declared dependencies.

## Evidence And Content Boundaries

The [branch review](../../../working/framework-review/reviews/branch-logic-review.md#extension-closure-and-workflow-review) checked all 23 existing package content files at `65e3b636`. Both packages at that baseline resolved through their declared dependencies and the base Framework. The reason for a split is more useful selection, rather than a demonstrated broken dependency.

The [E02 discussion](../../../working/framework-review/review-followup-instructions.md#e02--compact-task-and-plan-starters) accepted useful planning starters without requiring a fixed file collection or a separate package. The generic-content pass builds on that outcome. It uses project scopes for actual facts, conventions, tools, requirements, and accepted decisions.

## Planning Starters

The Workflow supplies the process, the Pattern supplies inspectable record shapes, and the Templates supply removable prompts for project content.

| Starter | Responsibility |
| --- | --- |
| Task | Outcome, boundaries, and completion evidence, with a compact plan and current state for small work |
| Plan | A separately maintained sequence and dependencies, linked to the Task's outcome |
| Backlog | Work awaiting selection and its priority, linked to a Task once one exists |
| Checkpoint | Current workstream state and next actions needed to resume |

A separate Plan or Checkpoint replaces the corresponding mutable detail in the Task with a link. Each changing fact has one defining source. Existing external task systems and established project formats remain valid. Ordinary work does not need four records. The existing Handoff Template retains its distinct role as a sealed transfer snapshot.

## Disposition

The accepted source split preserves installed paths and payload meaning. [Task 28](../../../working/cli-development/tasks/source-framework-review.md#extension-reassessment) records delivery and verification. CLI implementation and executable qualification remain with the separate catalogue task. This proposal adds no Framework primitive, fixed agent hierarchy, mandatory APM package, or new integration permission.
