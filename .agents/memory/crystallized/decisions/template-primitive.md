---
open-forge:
  description: Templates are a distinct Core primitive for copy-ready source artifacts whose ownership transfers to independently maintained results
  tags: [Memory, Decision, CurrentTruth, Core, Template, Primitive]
---

# Templates As A Core Primitive

## Context

Open Forge benefits from useful starting content for documents, Memory records, and other artifacts. Existing Core roles do not express the same lifecycle:

- Patterns continue to guide the shape of related results
- Directives and Axioms bind behavior
- Skills perform specialized work
- Workflows coordinate repeatable goals

Copy-ready content should make creation cheaper without making the source an ongoing authority over every result created from it.

## Decision

Templates are a distinct Core primitive for reusable source artifacts intended to be instantiated into independently owned workspace content.

Instantiation transfers ownership to the destination. The result receives its own accurate metadata, scope, state, authority, and relationships. Later template changes do not update it and the template creates no continuing conformance.

Generic templates are fallbacks. A specialization earns a separate template only when its copy-ready contents differ materially. Users may edit, scope, replace, or remove templates through the ordinary file-native customization model.

The Framework ships the Templates category contract. Concrete generic templates remain repository-local dogfood candidates until each one demonstrates enough cross-workspace value to justify inclusion in the shared payload.

## Rationale

Templates reduce blank-page cost while preserving user ownership and unbounded customization. Their ownership transfer gives them semantics that no previous Core primitive expresses cleanly.

Separating starting content from continuing conformance also makes updates safe to reason about. Updating a template may improve future instances without silently claiming existing independently maintained files.

## Alternatives And Tradeoffs

- Treating Templates as Patterns would make copy-ready convenience imply continuing conformance
- Treating Templates as Skills or Workflows would make static source content depend conceptually on execution
- Shipping examples without an authoritative route would make their lifecycle, authority, and selection ambiguous
- Shipping many specialized templates by default would lower initial creation cost at the price of a larger and more opinionated Framework

The distinct primitive adds another route and maintenance surface, so every generic or specialized template must still earn its value.

## Consequences

- Template descriptions and removable source instructions expose the need and primary question or result they address
- Instantiated results never inherit update authority from their source template
- Continuing shape or behavior is expressed by a linked Pattern, Directive, Axiom, or another matching authoritative source
- The installable Templates route may exist without shipping concrete starter artifacts
- Promoting a dogfood template into the shared payload requires an independent review of generic value, baseline cost, and ownership

## Authoritative Sources

These sources express the accepted result:

- [Current Templates contract](../documents/framework/primitives/templates.md)
- [Framework Architecture](../documents/framework/architecture.md#core-primitives)
- [Current Templates route](../../../templates/_templates.md)
- [Templates category maintenance contract](../documents/maintenance/payload/agents/templates.md)

## Decision Relationships

- [Distinct Core primitive roles](core-primitives.md)
- [Source and packaging](source-and-packaging.md)
- [Typed authoritative source terminology](authoritative-source-terminology.md)
