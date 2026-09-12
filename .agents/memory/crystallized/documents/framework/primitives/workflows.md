---
open-forge:
  description: Current Workflow role, minimal recipe contract, explicit relationships, composition, and `route` boundaries
  responsibility: Define how routed Markdown Workflows pursue goals and use other Core primitives without becoming a mandatory lifecycle or provider-specific orchestration runtime
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, Core, Workflow, Goal, Composition]
---

# Workflows

## Role

A Workflow is a repeatable Markdown recipe for reaching a defined goal through steps that are useful beyond one isolated task.

Workflows are optional routed recipes. They are not provider-specific orchestration objects and do not impose one development lifecycle. Work may begin wherever enough current truth exists, skip unnecessary preparation, repeat, move backward when evidence changes, or proceed directly when no installed Workflow adds value.

## Selection

A visible `description`, tags, `route` meaning, and current user direction select a candidate Workflow before its body is loaded. After loading, `Goal` confirms whether the recipe materially helps. If it does not, return to the visible `Entries` instead of executing it by inertia.

Select the smallest Workflow that resolves a material missing decision or execution risk. Installed Workflows are not mandatory stages, and users do not need to know or name them.

When a Workflow materially changes the interaction, explain the useful approach and why it applies in one natural sentence. Internal `route` details remain optional.

Direct execution remains valid when no installed recipe adds value. An explicit choice or opt-out governs optional use.

A Workflow earns its own `route` only when repeating the recipe materially changes execution, preserves a deliberate user methodology, or improves reliability beyond ordinary capable-agent behavior. Generic planning, implementation, testing, review, or brainstorming advice does not earn a Workflow merely by being written as steps.

## Recipe Contract

Every complete Workflow contains exactly one non-empty level-2 `Goal`, `Steps`, and `Completion` section in that order.

- `Goal` states the intended result clearly enough to confirm whether the loaded recipe fits.
- `Steps` state how to pursue the goal. They may include boundaries, branching, iteration, delegation, capabilities, verification, and stopping conditions where those matter.
- `Completion` states the evidence or result that makes the Workflow complete.

This is a minimum contract, not the complete allowed heading vocabulary. A recipe may add level-2 headings that clarify its own subject without making those headings part of every Workflow.

An `entrypoint` may organize descendant Workflows without becoming a recipe. If an `entrypoint` declares `Goal`, `Steps`, or `Completion`, it declares the complete applicable recipe contract. Every Workflow file that is not an `entrypoint` is a complete recipe.

## Relationships

Use ordinary Markdown links where another source helps selection or execution. State in Steps when a source must be read or a capability invoked at a particular point. A link makes the relationship visible without creating another preload rule or Workflow-specific dependency graph.

## Composition

A Step may invoke a Skill, consult Guidance, apply a Pattern, instantiate a Template, follow a Map `route`, delegate bounded work, or hand off to another Workflow.

Composition remains explicit in Steps and handoffs. A Workflow does not silently absorb another Workflow's `Axioms`, convert an optional suggestion into a prerequisite, or create an implicit execution graph.

## Workflow Scopes

The loader's universal scoping rules apply below the Workflows `root route`. Routed scopes group Workflow recipes and inherit the Workflow role.

Other `root routes` do not reinitialize beneath Workflows. A routed folder named `directives`, `patterns`, or `skills` remains within Workflows. Its familiar name or tags do not grant another primitive's loading, authority, validation, update, or runtime contract.

Workflows use other primitives through explicit relationships:

- Active [Directives](directives.md) remain binding through their own selected `routes`
- Skills remain under the standard [Skills](skills.md) root for native discovery and enter a recipe through Steps or ordinary links
- Guidance, Patterns, Templates, and Map `routes` remain under their own roots and are linked where needed

This keeps each Workflow a readable recipe instead of a miniature mixed Core installation.

## Reusable And Local Methods

Keep portable recipes focused on the decisions, sequence, evidence, and completion they add. A specialized local profile may add explicit roles, budgets, or protected phases when accepted for that workspace. Those choices do not become requirements for every installation.

A planning recipe may use a Pattern for record shape and a Template for starting content. The Workflow defines how work proceeds; the Pattern defines what the records look like. Keep the method in one source when a Skill or another Workflow uses it.

Evaluate a Workflow by how its method helps achieve its stated goal. Any collaboration it uses should contribute to that result.

## Runtime And Validation

The installed [Workflows entrypoint](../../../../../workflows/_workflows.md) contains the compact runtime and manual authoring contract. The [Maintenance contract](../../maintenance/payload/agents/workflows.md) owns source alignment and deterministic verification. Validation preserves the authored recipe contract without becoming authoritative for its meaning.

## Related Current Sources

- [Core primitive model](model.md)
- [Routing model](../routing/model.md)
- [Scope and inheritance](../routing/scope.md)
- [Routed Markdown representation](../markdown/routes.md)
- [Workflows maintenance contract](../../maintenance/payload/agents/workflows.md)

## Decisions And Rationale

- [Workflow shape](../../../decisions/framework/workflow-shape.md)
- [Distinct Core primitive roles](../../../decisions/framework/core-primitives.md)
- [Routing surfaces](../../../decisions/framework/routing-surfaces.md)
