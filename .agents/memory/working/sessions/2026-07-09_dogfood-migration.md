---
open-forge:
  description: Session record of migrating the temporary .agents notes into an installed Open Forge workspace
  tags: [Memory, Session, Contextual, Dogfood, Migration]
---

# Session: Dogfood Migration

Date: 2026-07-09. Open Forge was installed into its own repository and the temporary `.agents/` cleanup memory was migrated into proper routes.

## What Happened

1. Renamed the temporary `.agents/` to `.agents-temp/`.
2. Ran `bun run src/cli/cli.ts install .`, which installed 19 managed payload files and patched `AGENTS.md`.
3. Moved the archive: `archive/ideas/` and `archive/sessions/` to `.agents/memory/archived/`, with `archive-map.md` beside them; folded `archive/README.md` into the map; removed the reserved #OpenForge tag from archived file metadata so archived material stays on-demand.
4. Split `decisions.md` into per-topic files under `.agents/memory/crystallized/decisions/`.
5. Split `backlog.md`: actionable items to `.agents/memory/working/backlog.md`; deferred designs to `.agents/memory/emerging/ideas/cli-design.md` and `deferred-product-ideas.md`.
6. Dissolved `current-state.md`: the source-of-truth map became `.agents/workspace/sources-of-truth.md`; the WIP snapshot became `.agents/memory/working/handoffs/2026-07-06_cleanup-pass.md`; duplicated CLI and product descriptions were dropped in favor of `docs/cli.md`, `README.md`, and the decision routes.
7. Added new decisions (`loading-reliability.md`, `routing-surfaces.md`), new ideas (`workflow-redesign.md`, `observations-rework.md`, `extension-skill-sharing.md`), an observation on tag load compliance, and a documents route to the dogfood reports.
8. Regenerated indexes and ran the test suite.

## Follow-Ups Landed In This Session

- The tag rework (#LoadNow, #KeepInMind replacing #OpenForge, #LoadWithParentEntrypoint, #LoadForPostWorkReview) was applied across payload, workspace, docs, and tests, then staged for review.
- The dogfood report files were moved into `.agents/memory/crystallized/documents/dogfood-reports/` so the accepted evidence records are routed memory, not loose root files.
- A full per-file wording and deduplication pass followed; see `.agents/memory/emerging/analysis/2026-07-09_post-rework-review.md`.

## Sources

Old content came from the temporary `.agents/memory/` and `.agents/archive/` (deleted in this migration; recoverable through git history) plus the framework assessment conversation of 2026-07-08.
