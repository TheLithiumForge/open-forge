---
open-forge:
  description: Patterns scoped to the HTTP server package
  tags: [Extension, Pattern, Server, Scope, Index]
---

# Server Package Patterns

Patterns here apply to `ledger/packages/server` and narrow the general workspace patterns for that package.

## Axioms

- Use `Entries` when creating or reviewing server-package code.
- Prefer these over broader workspace patterns inside the server package when both apply.

## Entries

<!-- open-forge:generated-index:start -->
- `handler-shape.md` - One handler per route, wired through a small hand-rolled router - #Extension #Pattern #Server #Http
- `error-responses.md` - All error paths flow through one response helper implementing the error contract - #Extension #Pattern #Server #Error #Contract
<!-- open-forge:generated-index:end -->
