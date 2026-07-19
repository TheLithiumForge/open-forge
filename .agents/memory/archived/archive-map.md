---
open-forge:
  description: Where Open Forge historical records are archived, what replaced them, and how to treat them
  tags: [Memory, Archived, Map, Contextual, Historical]
---

# Archive Map

The old `.agents/sessions/` and `.agents/ideas/` design notes were archived during cleanup on 2026-07-06 and moved into this memory route during the dogfood migration on 2026-07-09. A second scoped pass on 2026-07-18 moved completed working records, applied candidates, resolved observations, and dated planning into explicit archive categories after current material was extracted.

## Archived Locations

- `analysis/` - dated investigations and assessments whose accepted conclusions or open follow-ups moved elsewhere
- `handoffs/` - completed or superseded transfer notes
- `ideas/` - applied or superseded design exploration plus dated snapshots; unresolved candidates remain in emerging memory
- `observations/` - grounded findings after resolution, promotion, or supersession
- `planning/` - dated backlog and planning snapshots replaced by current planning
- `sessions/` - completed raw work sessions plus the original design-session and prompt material
- `benchmarks/harness/orchestrator/archive/` - frozen pre-P0 generation 10 and 11 run plans outside the Memory tree; raw reports and accepted syntheses remain in their evidence routes

## Extracted Current Material

The useful current material was extracted into:

- `.agents/memory/crystallized/decisions/` - accepted design decisions
- `.agents/memory/working/backlog.md` - pending priorities still worth carrying forward
- `.agents/memory/emerging/ideas/` - deferred designs and product ideas
- `.agents/memory/emerging/observations/` - unresolved evidence and decisions still requiring attention
- `.agents/memory/crystallized/documents/evaluations/` - accepted benchmark syntheses; raw reports stay under `benchmarks/results/`
- `.agents/workspace/sources-of-truth.md` - source-of-truth file map for this repository

## How To Use The Archive

- Treat archived files as historical context, not active truth.
- Prefer the extracted memory routes for continuation.
- Load archived sessions only when reconstructing original intent or old rationale.
- Load archived ideas only when a deferred design needs its detailed reasoning.
- Load archived handoffs, analysis, observations, or planning only to reconstruct a completed pass, source evidence, or prior trade-off.
- If archived material conflicts with current source files or extracted decisions, the current files win unless the user restores or revises history.

## Archived Because

- The old session files mixed current direction, old structures, superseded paths, and raw discussion history.
- The old idea files were useful while designing, but current decisions now live in maintained framework docs, installable payload files, and crystallized decision routes.
- Keeping the raw files active made agents pay context cost for historical baggage.
