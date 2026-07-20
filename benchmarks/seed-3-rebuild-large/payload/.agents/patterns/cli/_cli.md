---
open-forge:
  description: Patterns scoped to the CLI client package
  tags: [Extension, Pattern, Cli, Scope]
---

# CLI Package Patterns

Patterns here apply to `ledger/packages/cli` and narrow the general workspace patterns for that package.

## Axioms

- Use `Entries` when creating or reviewing CLI-package code.
- Prefer these over broader workspace patterns inside the CLI package when both apply.

## Entries

<!-- open-forge:generated-index:start -->
- [The CLI renders and relays; it never computes business results](thin-client.md) - #Extension #Pattern #Cli #Boundary
<!-- open-forge:generated-index:end -->
