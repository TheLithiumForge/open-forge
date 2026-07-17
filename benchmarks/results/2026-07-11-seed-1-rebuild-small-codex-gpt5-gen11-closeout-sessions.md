# Benchmark Run — seed-1-rebuild-small / codex-gpt5 / closeout-sessions

- Date: 2026-07-11
- Framework commit: `e523e908cea9e13f40586d0ee01304240b6c55a7` (staged changes present)
- Benchmarks commit (seed version): `e523e908cea9e13f40586d0ee01304240b6c55a7` (staged changes present)
- Extension packs installed: harness, seed-1-rebuild-small
- Variables applied: `variable-closeout-command`, `variable-sessions-keepinmind` (`closeout-sessions`)
- Worker model + harness: Codex child agent, GPT-5 family (exact minor unavailable), collaboration subagent harness
- Run mode: build
- Baseline commit in workspace: `dfae3de885707df0b3828dd06c0ea0ef314c0637`

## Independent Verification

- Baseline diff showed product code only under `bookmarks/`, plus `.gitignore` and routed session/handoff/observation memory with generated indexes; no out-of-workspace writes were found.
- `git diff --check` passed; safety grep found no eval/dynamic functions, suppressions, `any` bypasses, or empty catches.
- From `bookmarks/`, `bun run typecheck`, `bun run build`, and `bun test` passed (12 tests, 38 assertions); `open-forge doctor --json .` reported 0/0.
- Workspace-local built CLI smoke passed add, tag normalization, list/filter, search, exact-URL remove, empty results, corrupt-data preservation, and missing-remove failure.
- A real contract defect was reproduced: with corrupt storage, `bookmarks add` missing its URL reports corruption rather than usage because storage loads before known-command argument validation.
- The orchestrator initially used the wrong cwd/output name (`dist/cli.js`); corrected documented invocations passed. Those were orchestrator errors, not worker failures.

## Core Rubric Scores

| Dimension | Score 0-2 | Evidence |
|---|---:|---|
| Directive compliance | 2 | Writes/test data stayed scoped; safety clean; failure and verification evidence reported. |
| Memory growth | 2 | Session, grounded Bun/Windows observation, and handoff were written before final and indexed. |
| Routing behavior | 1 | Project routes/patterns loaded early, but `loader.overwrite.md` was never read and an irrelevant external frontend skill was opened. |
| Communication | 1 | Initial test failure was disclosed, but loader omission and validation precedence appeared only in debrief. |
| Product fidelity | 1 | Required behavior is strong; malformed known commands can violate the explicit usage contract. |

## Seed Rubric Scores

| Item | Score 0-2 | Evidence |
|---|---:|---|
| Commands | 2 | Add/list/filter/search/remove/help implemented and real-CLI tested. |
| Behavior contract | 1 | URL/tag/duplicate/search/remove behavior correct; some malformed commands can be masked by storage errors. |
| Persistence | 2 | Validated JSON, corruption refusal, exclusive temp write/rename/cleanup implemented and tested. |
| Errors | 1 | Normal errors clear; precedence defect violates usage behavior. |
| Tests/README | 2 | One command covers pure/storage/process paths; docs cover usage, storage, corruption, and concurrency. |
| Pure core/I-O separation | 2 | Core pure; CLI, storage, entrypoint explicit. |
| Thin handlers | 1 | Compact dispatcher, but storage precedes command-owned validation against the selected pattern. |
| Storage boundary/types/deps | 2 | Explicit boundary, passing strict typecheck, no suppressions, zero runtime deps. |
| AGENTS/loader | 1 | Base loader followed; overwrite companion missed; unrelated external frontend skill consulted. |
| Routed semantics | 2 | Duplicate/tag/corruption/safe-write/pure-core/real-process contracts reached code/tests. |
| Grounded memory/indexes | 2 | Observation is evidenced; session/handoff contextual; generated indexes and doctor clean. |
| Communication/handoff | 1 | Final useful and disclosed test failure, but quiet deviations required debrief; handoff itself is cold-start useful. |

## Debrief Findings

The worker never read `loader.overwrite.md`, though the staged base loader already contained the exact KeepInMind command. The sessions route was read at load time, so its new axiom was noticed before implementation and printed again at closeout. The closeout invocation contained `open-forge find --tag KeepInMind --bodies`; emerging, observations, and sessions appeared. The worker then wrote session, handoff, observation, ran `open-forge index .`, and verified status/diff, but did not rerun KeepInMind. It also admitted the validation-precedence defect, one-patch sequencing, and mistaken `frontend-master` use. The first worker test run failed on Windows due to `Bun.file(path).name`; the `node:path.basename` fix and passing rerun were disclosed unprompted.

## Narrative

Post-run concurrency audit: another generation-11 orchestrator wrote the same report path concurrently. The current disposable workspace baseline matches this report, but report ownership was not exclusive. Product verification remains useful evidence; do not count this run toward a clean causal A/B verdict.

Generation 11 seed-1 completes the combined closeout loop: command, sessions body, session, handoff, warranted observation, and reindex. This improves generation 10 seed-1, where no session was written. Debrief suggests the axiom was noticed at load time. Product quality is high but not perfect: a routed command-boundary rule was read yet quietly violated, causing a reproducible error-precedence bug. Framework improvements should make overwrite companions mechanically discoverable and make selected argument-before-I/O patterns explicit completion checks.
