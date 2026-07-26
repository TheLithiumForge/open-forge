---
open-forge:
  description: Routing goes through small markdown entrypoints; one recognized entrypoint per folder; the loader exposes only direct root routes
  tags: [Memory, Decision, CurrentTruth, Routing]
---

# Routing Model

Accepted decisions extracted from the design sessions and idea notes on 2026-07-06, then narrowed to structural routing rationale on 2026-07-27.

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
- Missing, empty, `inherited`, and `none` local Axioms declarations all add no local axioms and never disable loaded ancestor axioms. A sentinel cannot be mixed with substantive local axioms. (accepted 2026-07-17)
- Generated `Entries` and authored `Required Routes` use `- [Description](relative/path.md) - #Tags`. Link targets resolve relative to the Markdown file containing them; loader links therefore resolve from `.agents/loader.md`. CLI `--route` arguments and reported route identities remain workspace-relative. The CLI may read legacy backtick entries during migration with their former workspace-root-relative resolution but emits canonical links. (revised 2026-07-20)

The accepted current result is expressed by the [routing model](../documents/framework/routing/model.md), [scope contract](../documents/framework/routing/scope.md), and [path contract](../documents/framework/routing/paths.md).

Specialized rationale remains in:

- [Loading reliability](loading-reliability.md)
- [Routing surfaces](routing-surfaces.md)
- [Scope and slugs](scope-and-slugs.md)
- [Workflow shape](workflow-shape.md)
- [Canonical Markdown authoring](canonical-markdown.md)
