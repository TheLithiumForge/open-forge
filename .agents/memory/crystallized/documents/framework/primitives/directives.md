---
open-forge:
  description: Current Directive role, routed activation, binding scope, authority composition, conflict handling, and relationship with Framework Axioms
  responsibility: Define why independently routed binding behavior exists and how its selected scope composes with broader authority
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, Core, Directive, Authority, Loading, Scope]
---

# Directives

## Role

A Directive is an independently routed binding instruction. Use one when behavior is mandatory in a reusable scope and no broader authoritative source already expresses that requirement. Its direct sibling file exposes exactly one substantive `## Instructions` section.

Directives let a workspace grow explicit mandatory behavior without placing every rule in the loader or treating contextual advice as binding.

## Relationship With Framework Axioms

Framework `Axioms` define the mechanics and invariants of an already loaded `route`. A Directive uses those mechanics to provide independently selectable binding behavior through `Instructions` rather than declaring a second Axioms source.

A category Axiom belongs with the route whose meaning it defines. A Directive belongs in its own route when the mandatory behavior is independently meaningful, reusable, and selectable. Adding every workspace rule to category entrypoints would make unrelated concerns inseparable and expand baseline context.

## Routing And Activation

Routing establishes a Directive's positive scope before its binding contents become active. A Directive on the selected active route chain applies within that scope. Merely inspecting a Directive as source, history, an example, or an inactive alternative does not activate it.

This separation makes mandatory behavior discoverable without loading every possible instruction. The installed [Directives entrypoint](../../../../../directives/_directives.md) owns the exact loading and direct-sibling-file contract.

## Authority And Composition

Directives are binding within their established scope, but their primitive type does not place them above current user direction, platform constraints, runtime safety, or declared external authoritative sources. The [Framework authority model](../architecture.md#authority) owns that ordering.

Loaded child Directives add to active ancestor Directives. Narrower routing changes where a requirement applies, not its authority, and does not create a silent override.

When active Directives conflict or cannot be followed, the exact sources and scopes are surfaced for an explicit decision or exception. A hidden precedence rule would make binding behavior difficult to inspect and correct.

## Relationships And Boundaries

A Directive may require use of a Pattern, Skill, Template, Workflow, Map route, or another result. The linked primitive retains its own meaning; the Directive owns only the requirement.

Optional advice belongs in Guidance. A reusable inspectable shape belongs in a Pattern. A bounded capability belongs in a Skill. A repeatable goal belongs in a Workflow.

Examples of suitable Directives include a mandatory security check, a prohibited dependency, or a required review boundary. A preferred architectural approach with legitimate contextual exceptions belongs in Guidance. A description of already accepted product state belongs in its current document.

## Related Current Sources

- [Core primitive model](model.md)
- [Routing model](../routing/model.md)
- [Scope and inheritance](../routing/scope.md)
- [Directives entrypoint](../../../../../directives/_directives.md)
- [Directives maintenance contract](../../maintenance/payload/agents/directives.md)

## Decisions And Rationale

- [Loading reliability](../../../decisions/framework/loading-reliability.md)
- [Routing surfaces](../../../decisions/framework/routing-surfaces.md)
