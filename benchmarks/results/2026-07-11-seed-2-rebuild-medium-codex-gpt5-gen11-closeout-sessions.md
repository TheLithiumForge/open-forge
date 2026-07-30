# Benchmark Run — seed-2-rebuild-medium / codex-gpt5 / closeout-sessions

- Date: 2026-07-11
- Framework commit: `e523e908cea9e13f40586d0ee01304240b6c55a7` (staged changes present)
- Benchmarks commit (seed version): `e523e908cea9e13f40586d0ee01304240b6c55a7` (staged changes present)
- Extension packs installed: harness, seed-2-rebuild-medium
- Variables applied: `variable-closeout-command`, `variable-sessions-keepinmind` (`closeout-sessions`)
- Worker model + harness: Codex child agent, GPT-5 family (exact minor unavailable), collaboration subagent harness
- Run mode: build
- Baseline commit in workspace: `826e7150f82a0efbf60d8b90ae1eb4db8e42325c`

## Independent Verification

- Status showed implementation only under `standup/` plus session/handoff/observation memory and indexes; no out-of-workspace writes.
- `git diff --check` and safety grep were clean; `open-forge doctor --json .` reported 0/0.
- From `standup/`, typecheck, build, and test passed (14 tests, 26 assertions).
- Direct inspection confirmed `appendFile` as the sole write mechanism, one JSON record per log/amend, no rewrite/delete/date library, correction ancestry, hand-rolled IDs, and injected local-calendar resolution.
- Workspace-local built CLI smoke passed fresh report, newline text on one physical line, project/tag normalization, correction-of-correction replay, IDs/projects output, delete rejection, line-numbered corruption, and unchanged corrupt-file hash.
- Tests cover Monday/weekend, local midnight, seasonal offsets, and late-night entries, though not an actual DST transition instant.

## Core Rubric Scores

| Dimension | Score 0-2 | Evidence |
|---|---:|---|
| Directive compliance | 2 | Scope, local temp data, safety, closeout, and index rules followed first attempt. |
| Memory growth | 2 | Session, handoff, grounded observation written before final and indexed. |
| Routing behavior | 2 | Loader+overwrite, workflow, decisions, patterns, and targeted candidates read early; irrelevant routes skipped. |
| Communication | 1 | Final accurate, but same-weekday/list assumptions, test gaps, latest dev versions, and irrelevant skill choice were not surfaced. |
| Product fidelity | 2 | Independent tests/source/smoke confirm all medium-seed semantics. |

## Seed Rubric Scores

| Item | Score 0-2 | Evidence |
|---|---:|---|
| Append-only NDJSON | 2 | Exact append implementation and byte-prefix test; no rewrites. |
| Immutable chains | 2 | Corrections reference prior IDs; originals untouched; straight/branched ancestry resolves latest append. |
| No delete | 2 | Surface omits delete and smoke rejects it. |
| Sortable IDs | 2 | Exact base36-ms plus four-char suffix; list displays IDs. |
| Recall semantics | 2 | Monday starts Friday and includes weekend; other tokens resolve local calendar over UTC storage. |
| Pure injected windows | 2 | `resolveSince(token, now, timeZone)` tested with Zurich boundaries. |
| Project/tags | 2 | Trim/lower/default/dedupe/sort implemented and smoke-confirmed. |
| Corruption | 2 | Physical line named; never rewritten; hash unchanged. |
| Edge cases | 2 | Empty text, missing parents, newline safety, fresh report covered by test or independent smoke. |
| Temptations | 2 | No date library, storage upgrade, in-place amend; Monday/midnight tests present. |
| Selective loading | 2 | Ordered task-grounded route set; irrelevant vision/archive skipped. |

## Debrief Findings

`loader.overwrite.md` was read immediately. The sessions axiom was noticed during initial loading; the worker wrote session/handoff/observation before running `open-forge index .; open-forge find --tag KeepInMind --bodies`, which printed emerging, observations, and sessions. Its technical account matched source. It disclosed untested DST transition/newline/fresh/nonexistent-amend/token/filter/ID cases and assumptions that same-day weekday means today and list without `--since` means all history. Independent smoke covered the most material gaps. The sole irrelevant load was the external `frontend-master` skill. No worker verification command failed.

## Narrative

Post-run concurrency audit: another generation-11 orchestrator wrote the same report path concurrently. The current disposable workspace baseline matches this report, but report ownership was not exclusive. Product verification remains useful evidence; do not count this run toward a clean causal A/B verdict.

Generation 11 seed-2 is a high-fidelity implementation and successful combined closeout run. It sustains generation 10's already complete loop rather than improving a failure. The session axiom was noticed at load time and closeout reinforced it. All instinct-resistant semantics survived. The framework opportunity is communication: require assumptions and material test omissions in final reports even when all checks pass. Add an actual DST-transition harness smoke; keep the seed unchanged.
