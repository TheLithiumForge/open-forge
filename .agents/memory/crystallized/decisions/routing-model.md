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
- Missing, empty, `inherited`, and `none` local Axioms declarations all add no local axioms and never disable loaded ancestor axioms. A sentinel cannot be mixed with substantive local axioms. (accepted 2026-07-17)
- `open-forge chain <route> [--heading <title>]` may expose loader, ancestor entrypoints, skill boundary, target, and user-owned overwrites in load order. It automates inspection without defining runtime truth. (revised 2026-07-19)
- #LoadNow is relative to an already-loaded parent. Direct directive files carry it explicitly, so direct files under the loaded root directive route bind workspace-wide, while loading a positively described child directive route first establishes its narrower scope and then reads its direct files through the same generic rule. Directive bodies add no second applicability gate, and narrower scope does not silently override loaded ancestors. (revised 2026-07-19)
- #KeepInMind is the catalogue-wide continuity exception to parent-chain loading. Agents read or recheck the complete effective set at task start or resume, after actual context restoration, before handoff, and before closeout; they also refresh it when its follow-ups may have changed. A user-owned `.overwrite.md` loads immediately after its base. (revised 2026-07-19)
- `open-forge load --bodies` is optional batched traversal of the loader, transitive visible #LoadNow closure in generated order, and complete #KeepInMind catalogue. It does not traverse an on-demand parent merely because a hidden descendant carries #LoadNow, and plain-file traversal remains complete. (revised 2026-07-19)
- Workflow descriptions and tags are the pre-load selection surface, while phase tags are non-waterfall wayfinding. After selection, an optional Goal `- helpful before: ...` item may prompt one available earlier-workflow recommendation, but never blocks the selected workflow; skipped or unavailable prior work becomes explicit assumptions. (revised 2026-07-19)
- Generated `Entries` and authored `Required Routes` use `- [Description](relative/path.md) - #Tags`. Link targets resolve relative to the Markdown file containing them; loader links therefore resolve from `.agents/loader.md`. CLI `--route` arguments and reported route identities remain workspace-relative. The CLI may read legacy backtick entries during migration with their former workspace-root-relative resolution but emits canonical links. (revised 2026-07-20)
