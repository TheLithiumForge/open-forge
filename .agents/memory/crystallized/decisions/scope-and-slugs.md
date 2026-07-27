---
open-forge:
  description: Distinct route and slug terms preserve recursive scope meaning without runtime placeholders or a fixed organizational taxonomy
  tags: [Memory, Decision, CurrentTruth, Routing, Scope]
---

# Scope And Slugs

Open Forge distinguishes a standard Framework route, a local scope route, a Framework route reused inside a scope, and the concrete slug that realizes a path. Collapsing those ideas made it unclear whether a name described standard behavior, local narrowing, recursive composition, or only one installed folder.

Installed paths therefore use concrete slugs rather than runtime placeholders. This keeps every route self-describing and navigable without a registry or proprietary resolver. Placeholders remain useful only when documents, Templates, maintenance, or tools describe a shape before it is instantiated.

Placement carries scope intentionally. A Framework route inside a state scopes content within that state; a state lifecycle inside a local route gives that local subject its own lifecycle. Open Forge does not impose a fixed projects, domains, or teams taxonomy because the same recursive mechanism must support whichever structure the workspace needs.

The [current scope and inheritance contract](../documents/framework/routing/scope.md) expresses the accepted result.
