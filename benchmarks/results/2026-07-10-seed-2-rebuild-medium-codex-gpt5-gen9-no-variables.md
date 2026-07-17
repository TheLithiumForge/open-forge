# Benchmark Run - seed-2-rebuild-medium / codex-gpt5 / no variables

- Date: 2026-07-10
- Framework commit: e523e908cea9e13f40586d0ee01304240b6c55a7
- Benchmarks commit (seed version): e523e908cea9e13f40586d0ee01304240b6c55a7 with existing uncommitted benchmark edits in the source repo
- Extension packs installed: harness, seed-2-rebuild-medium
- Variables applied: none
- Worker model + harness: Codex GPT-5 subagent, Open Forge benchmark harness
- Run mode: build
- Baseline commit in workspace: 61315844e6e4c629b69299c37b90ceeb2845987e

## Independent Verification

- `git status --short`: implementation was under `standup/`; memory changes were one session, one handoff, one observation, and generated index updates.
- `bun run verify`: passed. This ran typecheck, build, and 11 tests.
- Source safety grep over `standup/src`, `standup/test`, and `.agents`: no eval, dynamic function, suppressions, `as any`, `: any`, date libraries, SQLite, or source-level delete command.
- Storage inspection: `appendRecord` uses `appendFile`; `readRecords` parses NDJSON line by line and reports line numbers.
- Date/window inspection: `resolveSinceWindow(token, now, timeZone)` is pure and tests inject both `now` and `Europe/Zurich`.
- Manual smoke: `log` plus `amend` produced two NDJSON lines; `list --since 2026-07-01` displayed the original id with corrected text and normalized `web`, `#api`, `#bug`.
- Direct date smoke: `resolveSinceWindow("laststandup", 2026-07-06T07:00:00.000Z, "Europe/Zurich")` returned `2026-07-02T22:00:00.000Z`, i.e. Friday local midnight for a Monday standup.

## Core Rubric Scores

| Dimension | Score | Evidence |
|---|---:|---|
| Directive Compliance | 2 | Worker stayed in workspace, used workspace-local test artifacts, avoided banned code patterns, and reported verification plus ignored local artifacts. |
| Memory Growth | 2 | Session, handoff, candidate observation, and generated indexes were written before final response. |
| Routing Behavior | 2 | Debrief gave a selective route order: loader/root routes, directives, implementation workflow, product target, current truth, date/edge observations, and relevant patterns. |
| Communication | 2 | Final status and debrief clearly listed implemented commands, assumptions, verification, artifacts, and open user questions. |
| Product Fidelity | 2 | Append-only storage, immutable amendments, ids, date windows, normalization, corrupt-line errors, and edge cases all passed inspection or tests. |

## Seed Rubric Scores

- NDJSON append-only storage: 2 - `log` and `amend` append one JSON line; no whole-file rewrite path exists.
- Immutable amendments: 2 - corrections reference target ids, originals stay on disk, and chained corrections resolve to latest text.
- No delete command: 2 - CLI supports `log`, `list`, `report`, `amend`, `projects`, and `help`; no delete/remove path.
- Sortable ids displayed by list: 2 - ids are timestamp base36 plus random suffix, sorted through created time/id and shown by `list`.
- Recall windows and UTC/local semantics: 2 - storage uses `Date.toISOString()`, resolution uses local calendar math, Monday `laststandup` starts Friday local midnight and includes weekend work.
- Pure window resolution with injected now/zone: 2 - `resolveSinceWindow` takes token, `now`, and zone; tests inject these values.
- Project/tag normalization: 2 - projects trim/lowercase/default to `general`; tags trim/lowercase/deduplicate/sort.
- Unparseable NDJSON handling: 2 - parse errors name the line number and read paths do not rewrite the file.
- Edge cases: 2 - empty message rejected, missing parent directories created, newline text is JSON-safe and flattened for display, fresh reports say no entries.
- Temptation checks: 2 - no date library, no storage upgrade, no in-place amendment, and Monday/midnight boundaries are tested.
- Selective loading: 2 - debrief explained why each route family was read and did not claim recursive bulk reading.

## Debrief Findings

The worker's self-report was specific and consistent with the workspace: it named product/current-truth routes, date failure analysis, MVP edge observations, and only the implementation workflow. It reported no late required-route discovery. The only open assumption it identified was user-facing amendment targeting: it allowed targeting correction ids internally while continuing to display the original id.

## Narrative

Generation 9 seed-2 is a strong medium rebuild. The implementation preserved the non-obvious seeded opinions: append-only NDJSON, immutable corrections, no delete command, hand-rolled sortable ids, and local calendar windows over UTC timestamps. Compared with seed-1, closeout memory was stronger: session, handoff, observation, and indexes were all present. This run suggests the current framework carries medium-complexity semantics well when the worker follows routed current truth and uses selective loading deliberately.
