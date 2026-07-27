---
open-forge:
  description: Current Crystallized Memory purpose, accepted authority, consolidation, supersession, and shipped Decisions and Documents roles
  responsibility: Define what makes accepted durable Memory valid without claiming that every authoritative result belongs in Memory
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, MemoryModel, Crystallized, Authority, Consolidation]
---

# Crystallized Memory

## State Contract

[Crystallized Memory](../../../../crystallized/_crystallized.md) contains accepted durable state within its stated scope.

It keeps one coherent current representation for each distinct question and scope. When accepted state changes, agents update, split, merge, or reshape the existing authoritative source rather than creating a competing current copy.

Other accepted results may live in matching #Core routes, source code, declared external systems, or scoped Crystallized routes. The [Memory authority boundary](model.md#authority-boundary) determines the appropriate destination.

## Rationale And Current Records

The shipped routes provide two distinct accepted roles:

- [Decisions](../../../../crystallized/decisions/_decisions.md) preserve accepted rationale for important choices
- [Documents](../../../../crystallized/documents/_documents.md) integrate coherent current records or route to systems that hold them

## Scoped Decisions And Documents

Decisions and Documents may be initialized inside any Crystallized scope that needs them, not only directly under the root Crystallized route.

For example, a `mobile-app` scope inside Crystallized may use:

```text
memory/crystallized/mobile-app/decisions/
memory/crystallized/mobile-app/documents/
```

A broader `mobile-app` scope may instead contain its own complete Memory lifecycle:

```text
memory/mobile-app/crystallized/decisions/
memory/mobile-app/crystallized/documents/
```

Both shapes use ordinary recursive routing. Every folder in the selected path has an entrypoint, and each scope initializes only the roles it needs. Placement narrows the subject to which a Decision or Document applies without changing what that role means.

Overlapping Decisions are consolidated, reshaped, or linked when their accepted rationale is compatible. Material divergence or competing accepted rationale is discussed instead of being merged silently. Superseded rationale is archived or linked when it remains useful.

Documents own accepted current content. Copy-ready creation sources belong in [Templates](../primitives/templates.md), not in the Documents route.

Decisions and Documents are useful customizable child roles, not additional Memory states.

## Related Current Sources

- [Memory model](model.md)
- [Memory transitions](transitions.md)
- [Route scope and inheritance](../routing/scope.md)
- [Crystallized runtime maintenance](../../maintenance/payload/agents/memory/crystallized/_crystallized.md)
- [Accepted state and synchronization](../truth.md)
