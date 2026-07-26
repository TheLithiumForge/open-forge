---
open-forge:
  description: Framework routes, scope routes, scoped framework routes, and slugs are distinct; placeholders are notation only and slug placement changes meaning
  tags: [Memory, Decision, CurrentTruth, Routing, Scope]
---

# Scope And Slugs

Accepted decisions extracted from the design sessions and idea notes on 2026-07-06 and aligned with the accepted Framework Architecture on 2026-07-26.

- A `framework route` is a standard Core or Memory route shipped by Open Forge.
- A `scope route` is a local routed subtree used to narrow authority or meaning below it.
- A `scoped framework route` is a framework route initialized inside a scope route.
- A `slug` is the concrete folder name used in a route path.
- Route-template placeholders such as `[scope]` are documentation, maintainer, CLI, and extension-author notation only. Installed workspaces receive concrete slug folders.
- Slug placement changes meaning. For example, `memory/crystallized/mobile-app/documents/` scopes documents inside crystallized memory, while `memory/mobile-app/crystallized/documents/` gives `mobile-app` its own memory states.

The [current scope and inheritance contract](../documents/framework/routing/scope.md) expresses the accepted result.
