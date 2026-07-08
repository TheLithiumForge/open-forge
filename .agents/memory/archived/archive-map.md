---
open-forge:
  description: Where the pre-dogfood design notes were archived, what was extracted from them, and how to treat them
  tags: [Memory, Archived, Map, Contextual, Historical]
---

# Archive Map

The old `.agents/sessions/` and `.agents/ideas/` design notes were archived during cleanup on 2026-07-06 and moved into this memory route during the dogfood migration on 2026-07-09.

## Archived Locations

- `ideas/` - old design exploration notes that were useful during framework shaping
- `sessions/` - raw session summaries, handoffs, and the original prompt material

## Extracted Current Material

The useful current material was extracted into:

- `.agents/memory/crystallized/decisions/` - accepted design decisions
- `.agents/memory/working/backlog.md` - pending priorities still worth carrying forward
- `.agents/memory/emerging/ideas/` - deferred designs and product ideas
- `.agents/workspace/sources-of-truth.md` - source-of-truth file map for this repository

## How To Use The Archive

- Treat archived files as historical context, not active truth.
- Prefer the extracted memory routes for continuation.
- Load archived sessions only when reconstructing original intent or old rationale.
- Load archived ideas only when a deferred design needs its detailed reasoning.
- If archived material conflicts with current source files or extracted decisions, the current files win unless the user restores or revises history.

## Archived Because

- The old session files mixed current direction, old structures, superseded paths, and raw discussion history.
- The old idea files were useful while designing, but current decisions now live in maintained framework docs, installable payload files, and crystallized decision routes.
- Keeping the raw files active made agents pay context cost for historical baggage.
