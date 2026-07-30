---
open-forge:
  description: Routing goes through small markdown entrypoints; one recognized entrypoint per folder; the loader exposes only direct root routes
  tags: [Memory, Decision, CurrentTruth, Routing]
---

# Routing Model

Accepted decisions extracted from the design sessions and idea notes on 2026-07-06.

- Open Forge routes agents through small markdown `entrypoints`.
- A folder is routable only when it contains exactly one recognized `entrypoint`.
- Open Forge-authored entrypoints use `_{folder-name}.md`.
- Compatibility entrypoints are `_index.md`, `index.md`, `_references.md`, and `references.md`.
- Generated `Entries` list direct sibling markdown files and direct child `entrypoints`.
- Nested routing requires an `entrypoint` at every visible folder level.
- The loader exposes only direct active root routes under `.agents/`.
- Loose markdown files beside `loader.md` are not root routes.
- Universal loading and routing rules live once in the loader; category `entrypoints` stay minimal. (accepted 2026-07-09)
- Axioms of loaded ancestor `entrypoints` apply to all routes below them; a child `entrypoint` adds only scope-specific axioms and does not restate ancestor rules. (accepted 2026-07-09)
