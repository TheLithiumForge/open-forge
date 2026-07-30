---
open-forge:
  description: Target layout for the ledger workspace's three packages
  tags: [Extension, Workspace, Route, Project]
---

# Project Target

The implementation lives under `ledger/` at the workspace root, as three packages:

- `ledger/packages/core` — pure domain library: money, categories, validation, summaries. No I/O.
- `ledger/packages/server` — HTTP JSON API over the core, `node:http` only, file-backed storage.
- `ledger/packages/cli` — client for the server via `fetch`; no direct storage access.

Shared workspace tooling (root package manifest, test scripts) also lives under `ledger/`. Do not place implementation source directly in the workspace root.

Package-specific patterns are routed per package under `.agents/patterns/`; prefer the narrower package scope over general patterns when both apply.
