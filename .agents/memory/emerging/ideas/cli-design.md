---
open-forge:
  description: CLI design - shipped find/doctor/create surface, remaining flags, rune boundary, and deferred capabilities
  tags: [Memory, Idea, Contextual, Candidate, CLI, Extension, Tooling]
---

# CLI Design

Revised 2026-07-10. The viability core shipped this date: `find`, `doctor`, `create category`, and `create extension` landed in `src/cli/cli.ts` with tests; `docs/cli.md` is the behavior truth. This file keeps the rationale and what remains.

## Shipped Surface

- `install`, `extend`, `index` - unchanged lifecycle commands.
- `find` - deterministic routing-contract lookup: `--tag` (repeatable, AND), `--route` plus `--depth` over generated `entries`, `--follow-required` over `Required Routes` (a missing required route fails as a blocker), output as entry lines, `--paths`, `--bodies`, or `--json`. Metadata by default, bodies only on request, so exploring cannot accidentally dump a workspace.
- `doctor` - read-only integrity report, nonzero exit on errors, `--json` for CI: ambiguous entrypoints, malformed markers, unresolved generated entries, unresolved Required Routes, stale regions, retired load-policy tags, orphan overwrites, unreachable files. `index` remains the repair tool; no `--fix-index`.
- `create category <route-path>` - scaffolds the whole missing chain with canonical entrypoints, placement-derived type tags, placeholder descriptions, and a reindex. Refuses already-routable paths.
- `create extension <id>` - scaffolds `extension.json`, an authoring README, and an empty `payload/.agents/`.

Design principles that held: behavior keys to contract features (generated `Entries`, `Required Routes`, load-policy tags), never to route types, so new primitives need no new CLI surface. One earlier idea merged away: the separate `routes`/`context` verbs collapsed into `find` with body emission opt-in, and the named tiers became documented invocations - closeout is `find --tag KeepInMind --bodies`, a workflow bundle is `find --route <workflow> --follow-required --bodies`.

## Remaining Flags To Add

- `--max-tokens <n>` on `find --bodies`: warn or fail, never silently truncate - silent truncation would corrupt the reliability model.
- `--dry-run` companion output with approximate token counts per file.
- doctor token-budget warning when the #LoadNow startup tier grows past a threshold, keeping token cost flat as workspaces grow.

## Rune Boundary

Rune is the maintainer's standalone companion tool for project knowledge - markdown memories with hybrid semantic search, lifecycle, and agent integration; glyph is its LSP code-intelligence sibling. The index generator already accepts `rune:` scoped frontmatter. Full boundary analysis and the bridge-extension design: `rune-glyph-integration.md`.

- open-forge owns contract operations: install, extend, index, find, doctor, create. Deterministic, spec-bound, no ranking, no guessing.
- rune owns relevance: free-text and semantic search, relatedness, recency, memory lifecycle tooling, briefing-style summaries. This is why free-text query stays out of open-forge permanently; `find --tag` stays because tag matching is deterministic and the closeout recheck needs it.
- Integration ships as an optional first-party bridge extension, never as core coupling; rune indexes the workspace's markdown as-is.
- Tree rendering and exploration UX are rune territory.

The rule that resolves every overlap: if the output depends on relevance judgment, it is rune; if it depends only on the routing contract, it is open-forge.

## Deferred Capabilities

- Scaffold concrete routed paths from user intent; preview trees before writing.
- Detect collisions and ask before reusing paths (ancestor entrypoint generation shipped in create category).
- Route templates with named slug parameters.
- Extension manifests, provenance, compatibility, aliases, migrations, preview; install, update, remove, list, and local testing; see `extension-skill-sharing.md` for dependency handling.
- Let users select individual workflows while the CLI auto-selects required shared skills.
- Test extension installs through real OS temp directories instead of mocked filesystem operations.
- Extension-template authoring for maintainers.
- Forceful versus softer upgrade modes: forceful overwrites and re-adds all framework-owned files; softer updates existing framework-owned files without re-adding intentionally deleted defaults.
- The benchmark `variable-dump-tool` overlay stays as an A/B variable; `find` supersedes it as the product feature.
