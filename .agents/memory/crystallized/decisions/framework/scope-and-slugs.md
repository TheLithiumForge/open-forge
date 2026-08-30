---
open-forge:
  description: Accepted universal scoping rules, root boundary, `managed route` relationship, and concrete `slug` behavior
  tags: [Memory, Decision, CurrentTruth, Routing, Scope]
---

# Scope And Slugs

## Context

Earlier wording treated `framework routes`, `scope routes`, and `scoped routes` as separate structural types. That made one recursive mechanism harder to understand and tied ordinary `route` meaning too closely to who originally installed it.

## Decision

Open Forge uses `routes`, `root routes`, concrete `slugs`, and an optional management relationship.

A `root route` exists only where the loader exposes it. The `root routes` are not scopable and do not reappear beneath another `route`.

Every `route` below a `root route` is scopable. Any number of routed `slugs` may appear after the root, before, between, or after deeper `route` segments. Each scope narrows everything that follows it.

Scope is not a separate `route` type. It is the narrowing role performed by a routed `slug` and its `entrypoint`.

A scope contains only the `routes` useful there. It does not need to mirror another scope or the installed defaults.

Scoping preserves the order and meaning of deeper `routes`. For a `managed route`, the manager-declared `route` segments retain their order through every scope. A familiar `slug`, filename, or tag alone creates neither root behavior nor managed status.

A `managed route` is an ordinary `route` whose identified files Open Forge, an Extension, or another declared manager may install, update, restore, or remove. Each manager declares the `route` shapes it recognizes and acts only on files it explicitly owns or safely identifies. Management affects lifecycle, not runtime meaning or authority.

Installed paths use concrete `slugs`. Placeholders remain authoring notation before a `route` exists. Open Forge imposes no fixed projects, domains, teams, or repositories taxonomy.

The accepted Framework-aware Route Init mode expresses scope positions directly
in one desired concrete route. It aligns canonical non-root Framework segments
against the embedded canonical topology and treats inserted segments as scope
labels. Its ID-form input deterministically converts those labels to concrete
slugs; an exact `.agents/...` path is already concrete and is never slugged. This
is command-local authoring assistance, not a universal runtime placeholder or
slug service.

Users may edit, replace, move, or remove installed `routes` and expose additional roots through the loader. Missing defaults remain absent unless a requested lifecycle operation explicitly restores or replaces them.

## Rationale

One recursive rule can express a project inside a Memory state, a Memory lifecycle inside a project scope, or deeper combinations without inventing a `route` type for each arrangement.

Keeping root identity structural prevents miniature copies of root trees from appearing implicitly below other `routes`. Treating scope as a use of ordinary routed `slugs` keeps customization unconstrained below that boundary.

Separating management from runtime meaning lets Open Forge and Extensions reconcile explicit files without making provenance part of agent interpretation. Concrete paths remain self-describing and navigable without a registry.

## Alternatives And Tradeoffs

- Treating scope as a dedicated `route` type would add vocabulary without changing the routing mechanism
- Runtime placeholders would make installed paths depend on interpretation rather than visible folders
- A fixed organizational taxonomy would improve uniformity but exclude equally valid personal, disciplinary, multi-project, and multi-repository structures
- Treating Open Forge provenance as a runtime type would help lifecycle vocabulary but burden every reader with information that does not change `route` selection or meaning
- Allowing a nested familiar `slug` to recreate a `root route` would maximize physical nesting but make loading, native discovery, updates, and validation ambiguous

Recursive freedom requires every scope `slug` to have an `entrypoint` that makes its local meaning clear. Managed lifecycle support also requires each manager to declare what it can safely recognize.

## Consequences

- Any number of nested scopes may appear anywhere below a root
- A scope is placed immediately before the first `route` segment it should narrow
- The segments of a `managed route` retain the order declared by their manager
- The `root routes` compose through links instead of implicit physical nesting
- Tools operate on concrete `route` identities even when help and Templates explain parameterized shapes
- Framework-aware Route Init may help form those concrete identities from scope
  labels only when canonical topology alignment is unique; ambiguity, reordering,
  root recreation, and identity collision block
- Open Forge, Extensions, and future managers may support the same generic scoping rule through their own explicit lifecycle contracts
- Moving or renaming a `managed route` may end automatic reconciliation without changing its readable runtime meaning
- A `route`'s placement, `description`, tags, and inherited content jointly communicate scope

## Authoritative Sources

- [Scope and inheritance contract](../../documents/framework/routing/scope.md)
- [Current routing model](../../documents/framework/routing/model.md)
- [Path contract](../../documents/framework/routing/paths.md)
- [Open Forge loader routing contract](../../../../loader.md#routing)

## Decision Relationships

- [Product direction](../product/product-direction.md)
- [Routing model](routing-model.md)
