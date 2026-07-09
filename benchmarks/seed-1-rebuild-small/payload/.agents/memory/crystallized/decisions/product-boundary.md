---
open-forge:
  description: Accepted product boundary for the bookmarks CLI seed
  tags: [Extension, Memory, Decision, Product, Scope, CurrentTruth]
---

# Product Boundary

## Decision

The seed project is a local personal bookmarks CLI, not a sync service or full bookmark manager.

## In Scope

- Add, list, remove, search, and help commands.
- Optional comma-separated tags.
- Local JSON persistence.
- Friendly validation and errors.
- Tests and documentation.

## Out of Scope

- Accounts, auth, cloud sync, browser integration, database servers, TUI screens, web UIs, import/export formats, and concurrent multi-process editing guarantees.

## Rationale

The project should stay small enough that OpenForge route quality, agent compliance, and implementation judgment are easy to inspect.
