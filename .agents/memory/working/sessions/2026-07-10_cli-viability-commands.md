---
open-forge:
  description: Session record of shipping the CLI viability commands - find, doctor, create category, create extension
  tags: [Memory, Session, Contextual, CLI, Implementation]
---

# Session: CLI Viability Commands

Date: 2026-07-10. The CLI's viability core shipped after the rune boundary settled what open-forge must own itself.

## What Happened

1. `find` implemented: deterministic routing-contract lookup with repeatable `--tag` (AND), `--route` plus `--depth` over generated entries, `--follow-required` over Required Routes with missing routes failing as blockers, and entry/paths/bodies/json output. Metadata by default; bodies opt-in. The closeout recheck is now one command: `open-forge find --tag KeepInMind --bodies`.
2. `doctor` implemented: read-only integrity report with CI-friendly exit codes - ambiguous entrypoints, malformed markers, unresolved generated entries and Required Routes as errors; stale regions, retired load-policy tags, orphan overwrites, and unreachable files as warnings. Running it on this repository immediately caught a real parser gap (Required Routes template inside a code fence), fixed by fence-stripping.
3. `create category` implemented: scaffolds the full missing route chain with canonical entrypoints, placement-derived type tags, placeholder descriptions, and a reindex; refuses already-routable paths. `create extension` scaffolds `extension.json`, an authoring README, and an empty payload tree.
4. Earlier `routes`/`context` verb split collapsed into `find`; tiers became documented invocations rather than subcommands.
5. Twelve new tests added (34 total passing); `docs/cli.md` gained find/doctor/create sections; the cli-design idea updated to shipped status with remaining flags (`--max-tokens`, `--dry-run`, doctor token budget).
6. Backlog item 1 narrowed to validating closeout compliance via a benchmark variable overlay before promoting loader wording.

## Unresolved

- The closeout A/B overlay and the loader wording promotion await the next benchmark generation.
- Remaining flags from the design are unimplemented: `--max-tokens`, `--dry-run` token estimates, doctor startup-tier budget warning.
