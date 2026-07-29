---
open-forge:
  description: Current Workflow role, minimal recipe contract, optional dependencies, composition, and `route` boundaries
  responsibility: Define how routed Markdown Workflows pursue goals and use other Core primitives without becoming a mandatory lifecycle or provider-specific orchestration runtime
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, Core, Workflow, Goal, Composition]
---

# Workflows

## Role

A Workflow is a repeatable Markdown recipe for reaching a defined goal through steps that are useful beyond one isolated task.

Workflows are optional routed recipes. They are not provider-specific orchestration objects and do not impose one development lifecycle. Work may begin wherever enough current truth exists, skip unnecessary preparation, repeat, move backward when evidence changes, or proceed directly when no installed Workflow adds value.

## Selection

A visible `description`, tags, `route` meaning, and current user direction select a candidate Workflow before its body is loaded. After loading, `Goal` confirms whether the recipe materially helps. If it does not, return to the visible `Entries` instead of executing it by inertia.

Direct execution remains valid when no installed recipe adds value. An explicit choice or opt-out governs optional use.

A Workflow earns its own `route` only when repeating the recipe materially changes execution, preserves a deliberate user methodology, or improves reliability beyond ordinary capable-agent behavior. Generic planning, implementation, testing, review, or brainstorming advice does not earn a Workflow merely by being written as steps.

## Recipe Contract

Every complete Workflow contains exactly one non-empty level-2 `Goal`, `Steps`, and `Completion` section in that order.

- `Goal` states the intended result clearly enough to confirm whether the loaded recipe fits.
- `Steps` state how to pursue the goal. They may include boundaries, branching, iteration, delegation, capabilities, verification, and stopping conditions where those matter.
- `Completion` states the evidence or result that makes the Workflow complete.

This is a minimum contract, not the complete allowed heading vocabulary. A recipe may add level-2 headings that clarify its own subject without making those headings part of every Workflow.

An `entrypoint` may organize descendant Workflows without becoming a recipe. If an `entrypoint` declares `Goal`, `Steps`, `Completion`, or `Required Routes`, it declares the complete applicable recipe contract. Every Workflow file that is not an `entrypoint` is a complete recipe.

## Route Dependencies

Generated `Entries` express containment. `Required Routes` express unconditional cross-tree dependencies.

`Required Routes` is optional. Include it between `Goal` and `Steps` only when the recipe cannot proceed correctly without routed context that is not already active. If the section is absent, the Workflow declares no unconditional routed dependency and needs no sentinel.

Before Step 1, read every linked `route` and report an unreadable dependency as a blocker. Each dependency uses the canonical linked `entry` shape, resolves relative to the Workflow file, and includes useful tags with at least the target primitive type.

Prefer `entrypoint`-level dependencies and keep the list short. Stable routed files are allowed when the Workflow requires one exact source. Direct Directive files do not appear because binding behavior enters through active Directive `routes`.

## Composition

A Step may invoke a Skill, consult Guidance, apply a Pattern, instantiate a Template, follow a Workspace `route`, delegate bounded work, or hand off to another Workflow. Selecting another Workflow also activates its own `Required Routes` when present.

Composition remains explicit in Steps and handoffs. A Workflow does not silently absorb another Workflow's `Axioms`, convert an optional suggestion into a prerequisite, or create an implicit execution graph.

## Workflow Scopes

The loader's universal scoping rules apply below the Workflows `root route`. Routed scopes group Workflow recipes and inherit the Workflow role.

Other `root routes` do not reinitialize beneath Workflows. A routed folder named `directives`, `patterns`, or `skills` remains within Workflows. Its familiar name or tags do not grant another primitive's loading, authority, validation, update, or runtime contract.

Workflows use other primitives through explicit relationships:

- Active [Directives](directives.md) remain binding through their own selected `routes`
- Skills remain under the standard [Skills](skills.md) root for native discovery and enter a recipe through `Required Routes` or Steps
- Guidance, Patterns, Templates, and Workspace `routes` remain under their own roots and are linked where needed

This keeps each Workflow a readable recipe instead of a miniature mixed Core installation.

## Runtime And Validation

The installed [Workflows entrypoint](../../../../../workflows/_workflows.md) contains the compact runtime and manual authoring contract. The [Maintenance contract](../../maintenance/payload/agents/workflows.md) owns source alignment and deterministic verification. Validation preserves the authored recipe contract without becoming authoritative for its meaning.

## Related Current Sources

- [Core primitive model](model.md)
- [Routing model](../routing/model.md)
- [Scope and inheritance](../routing/scope.md)
- [Routed Markdown representation](../markdown/routes.md)
- [Workflows maintenance contract](../../maintenance/payload/agents/workflows.md)

## Decisions And Rationale

- [Workflow shape](../../../decisions/workflow-shape.md)
- [Distinct Core primitive roles](../../../decisions/core-primitives.md)
- [Routing surfaces](../../../decisions/routing-surfaces.md)
