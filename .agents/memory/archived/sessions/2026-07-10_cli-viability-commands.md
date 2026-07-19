---
open-forge:
  description: Historical session record of shipping the initial CLI viability commands
  tags: [Memory, Archived, Session, Contextual, Historical, CLI, Implementation]
---

# Session: CLI Viability Commands

Status: archived 2026-07-18.  
Original route: `.agents/memory/working/sessions/2026-07-10_cli-viability-commands.md`.  
Archived because: the command pass shipped and later CLI work materially extended its surface.  
Current owner or replacement: `docs/cli.md`, `src/cli/cli.ts`, CLI tests, and `.agents/memory/working/backlog.md`.

Date: 2026-07-10. The CLI's viability core shipped after the rune boundary settled what open-forge must own itself.

## What Happened

1. `find` implemented: deterministic routing-contract lookup with repeatable `--tag` (AND), `--route` plus `--depth` over generated entries, `--follow-required` over Required Routes with missing routes failing as blockers, and entry/paths/bodies/json output. Metadata by default; bodies opt-in. The closeout recheck is now one command: `open-forge find --tag KeepInMind --bodies`.
2. `doctor` implemented: read-only integrity report with CI-friendly exit codes - ambiguous entrypoints, malformed markers, unresolved generated entries and Required Routes as errors; stale regions, retired load-policy tags, orphan overwrites, and unreachable files as warnings. Running it on this repository immediately caught a real parser gap (Required Routes template inside a code fence), fixed by fence-stripping.
3. `create category` implemented: scaffolds the full missing route chain with canonical entrypoints, placement-derived type tags, placeholder descriptions, and a reindex; refuses already-routable paths. `create extension` scaffolds `extension.json`, an authoring README, and an empty payload tree.
4. Earlier `routes`/`context` verb split collapsed into `find`; tiers became documented invocations rather than subcommands.
5. Twelve new tests added (34 total passing); `docs/cli.md` gained find/doctor/create sections; `.agents/memory/archived/ideas/cli-design.md` records the shipped rationale and remaining flags extracted to the current backlog.
6. Backlog item 1 narrowed to validating closeout compliance via a benchmark variable overlay before promoting loader wording.

## Unresolved

- The closeout A/B overlay and the loader wording promotion await the next benchmark generation.
- Remaining flags from the design are unimplemented: `--max-tokens`, `--dry-run` token estimates, doctor startup-tier budget warning.
