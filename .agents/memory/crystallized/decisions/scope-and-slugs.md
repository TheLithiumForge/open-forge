---
open-forge:
  description: Distinct route and slug terms preserve recursive scope meaning without runtime placeholders or a fixed organizational taxonomy
  tags: [Memory, Decision, CurrentTruth, Routing, Scope]
---

# Scope And Slugs

## Context

Earlier wording collapsed standard Framework roles, local subject scopes, reused Framework routes, and concrete folder names into the same idea. Readers could not tell whether a term described semantic behavior, local narrowing, recursive composition, or one example path.

## Decision

Open Forge distinguishes framework routes, scope routes, scoped framework routes, and concrete slugs.

Placement narrows meaning without changing the underlying route role. Scope routes and Framework routes may compose recursively in whichever order expresses the workspace. Installed paths use concrete slugs; placeholders remain authoring notation for documents, Templates, Maintenance contracts, or tools before a route exists.

Open Forge does not impose a fixed projects, domains, teams, or repositories taxonomy.

## Rationale

Distinct terms let one recursive mechanism express a project inside Memory, Memory inside a project, or deeper combinations without inventing special route types for each organization.

Separating subject-bearing scope chains from reusable Framework role entrypoints keeps standard role wording stable at any depth and avoids hidden semantic metadata.

Concrete installed paths remain self-describing and navigable without a registry. Avoiding a fixed taxonomy preserves the grow-your-own Framework promise.

## Alternatives And Tradeoffs

- Calling every nested folder a scope would hide whether a standard Framework role is being reused
- Runtime placeholders would make installed paths depend on interpretation rather than visible folders
- A fixed organizational taxonomy would improve uniformity but exclude equally valid personal, disciplinary, multi-project, and multi-repository structures

Recursive freedom requires descriptions and ancestor entrypoints to make local meaning clear.

## Consequences

- The same standard route may be initialized at any useful scope
- A scope may contain only the Framework routes it needs
- Tools operate on concrete route identities even when help and Templates explain parameterized shapes
- Route placement, description, tags, and inherited content jointly communicate scope

## Authoritative Sources

- [Scope and inheritance contract](../documents/framework/routing/scope.md)
- [Current routing model](../documents/framework/routing/model.md)
- [Path contract](../documents/framework/routing/paths.md)
- [Open Forge loader terms](../../../loader.md#terms)

## Decision Relationships

- [Product direction](product-direction.md)
- [Routing model](routing-model.md)
