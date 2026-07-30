---
open-forge:
  description: Historical CLI design rationale for shipped commands, remaining flags, the Rune boundary, and deferred lifecycle options
  tags: [Memory, Archived, Idea, Contextual, Historical, CLI, Extension, Tooling]
---

# CLI Design

Status: archived 2026-07-18.  
Original route: `.agents/memory/emerging/ideas/cli-design.md`.  
Archived because: shipped behavior now belongs to CLI truth and its unresolved options were extracted to the live backlog.  
Current owner or replacement: `docs/cli.md`, crystallized CLI and extension decisions, CLI tests, and `.agents/memory/working/backlog.md`.

Revised 2026-07-17. The viability core now includes `find`, `chain`, `doctor`, `create category`, `create extension`, dependency-aware extension selection, and Git-checkpointed install flows in `src/cli/cli.ts` with command-level tests; `docs/cli.md` is the behavior truth. This file keeps the rationale and what remains.

## Shipped Surface

- `install`, `extend`, `index` - Core-first, target-scoped Git review checkpoints by default; read-only list/dry-run stay ungated and `--pro` bypasses only lifecycle guards.
- `find` - deterministic routing-contract lookup: `--tag` (repeatable, AND), `--route` plus `--depth` over generated `entries`, `--follow-required` over `Required Routes` (a missing required route fails as a blocker), output as entry lines, `--paths`, `--bodies`, or `--json`. Metadata by default, bodies only on request, so exploring cannot accidentally dump a workspace.
- `chain` - deterministic loader/ancestor/skill/target/overwrite inspection for any Markdown heading, including explicit inherited/none/empty status and target-containment checks.
- `doctor` - read-only integrity report, nonzero exit on errors, `--json` for CI: ambiguous entrypoints, malformed markers, unresolved generated entries, unresolved Required Routes, stale regions, retired load-policy tags, orphan overwrites, unreachable files. `index` remains the repair tool; no `--fix-index`.
- `create category <route-path>` - scaffolds the whole missing chain with canonical entrypoints, placement-derived type tags, placeholder descriptions, and a reindex. Refuses already-routable paths.
- `create extension <id>` - scaffolds `extension.json`, an authoring README, and an empty `payload/.agents/`.
- `extend` catalogue - derives payload contents, supports manifest-only dependency packs, and visibly auto-selects and locks transitive extension dependencies while keeping skill, workflow, directive, mixed, and pack payloads under one install contract.

Design principles that held: behavior keys to contract features (generated `Entries`, `Required Routes`, load-policy tags), never to route types, so new primitives need no new CLI surface. One earlier idea merged away: the separate `routes`/`context` verbs collapsed into `find` with body emission opt-in, and the named tiers became documented invocations - closeout is `find --tag KeepInMind --bodies`, a workflow bundle is `find --route <workflow> --follow-required --bodies`.

## Remaining Flags To Add

- `--max-tokens <n>` on `find --bodies`: warn or fail, never silently truncate - silent truncation would corrupt the reliability model.
- `--dry-run` companion output with approximate token counts per file.
- doctor token-budget warning when the #LoadNow startup tier grows past a threshold, keeping token cost flat as workspaces grow.

## Rune Boundary

Rune is the maintainer's standalone companion tool for project knowledge - markdown memories with hybrid semantic search, lifecycle, and agent integration; glyph is its LSP code-intelligence sibling. The index generator already accepts `rune:` scoped frontmatter. Full boundary analysis and the bridge-extension design: `.agents/memory/archived/ideas/rune-glyph-integration.md`.

- open-forge owns contract operations: install, extend, index, find, doctor, create. Deterministic, spec-bound, no ranking, no guessing.
- rune owns relevance: free-text and semantic search, relatedness, recency, memory lifecycle tooling, briefing-style summaries. This is why free-text query stays out of open-forge permanently; `find --tag` stays because tag matching is deterministic and the closeout recheck needs it.
- Integration ships as an optional first-party bridge extension, never as core coupling; rune indexes the workspace's markdown as-is.
- Tree rendering and exploration UX are rune territory.

The rule that resolves every overlap: if the output depends on relevance judgment, it is rune; if it depends only on the routing contract, it is open-forge.

## Deferred Capabilities

- Scaffold concrete routed paths from user intent; preview trees before writing.
- Detect collisions and ask before reusing paths (ancestor entrypoint generation shipped in create category).
- Route templates with named slug parameters.
- Persistent ownership/provenance receipts, compatibility and version solving, aliases, migrations, update/remove, remote provenance, and richer local authoring tests; install/list/dry-run and dependency composition have shipped.
- Extension-template authoring for maintainers.
- Forceful versus softer upgrade modes: forceful overwrites and re-adds all framework-owned files; softer updates existing framework-owned files without re-adding intentionally deleted defaults.
- The benchmark `variable-dump-tool` overlay stays as an A/B variable; `find` supersedes it as the product feature.
