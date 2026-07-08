---
open-forge:
  description: Framework routes, scope routes, scoped framework routes, and slugs are distinct; placeholders are notation only and slug placement changes meaning
  tags: [Memory, Decision, CurrentTruth, Routing, Scope]
---

# Scope And Slugs

Accepted decisions extracted from the design sessions and idea notes on 2026-07-06.

- A `framework route` is an Open Forge core route with stable default meaning.
- A `scope route` is a routed local folder used to narrow meaning or ownership below it.
- A `scoped framework route` is a framework route initialized inside a scope route.
- A `slug` is the concrete folder name used in a route path.
- Route-template placeholders such as `[scope]` are documentation, maintainer, CLI, and extension-author notation only. Installed workspaces receive concrete slug folders.
- Slug placement changes meaning. For example, `memory/crystallized/mobile-app/documents/` scopes documents inside crystallized memory, while `memory/mobile-app/crystallized/documents/` gives `mobile-app` its own memory states.
