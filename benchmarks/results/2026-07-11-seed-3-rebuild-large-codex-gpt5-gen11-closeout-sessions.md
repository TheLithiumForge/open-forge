# Benchmark Run — seed-3-rebuild-large / codex-gpt5 / closeout-sessions

- Date: 2026-07-11
- Framework commit: `e523e908cea9e13f40586d0ee01304240b6c55a7` (staged changes present)
- Benchmarks commit (seed version): `e523e908cea9e13f40586d0ee01304240b6c55a7` (staged changes present)
- Extension packs installed: harness, seed-3-rebuild-large
- Variables applied: `variable-closeout-command`, `variable-sessions-keepinmind` (`closeout-sessions`)
- Worker model + harness: Codex child agent, GPT-5 family (exact minor unavailable), collaboration subagent harness
- Run mode: build
- Baseline commit in workspace: `f9d51eb2b1dc66fdb740f94c5c4c352f79616652`

## Independent Verification

- Status showed implementation only under `ledger/` plus routed session/handoff/observation and indexes; no out-of-workspace writes.
- `git diff --check` and safety grep were clean; `open-forge doctor --json .` reported 0/0.
- From `ledger/`, typecheck, build, and test passed (12 tests, 32 assertions). Expected catch-all testing logged the real corrupt error server-side while returning only uniform internal JSON.
- Live workspace-local server smoke bound `127.0.0.1`; identical client ID returned 201 then 200; 1+2 minor units summed exactly to 3; June filtering used transaction date despite July created-at; bad CSV physical row 3 returned 422 without count change; corrupt storage returned exact 500 internal and unchanged hash.
- Closed-port CLI printed the friendly message and exited 1. Source inspection confirmed hand-written `node:http`, append-only reversal, server-side filtering/summary, zero runtime deps, and core/server/CLI packages.
- The orchestrator initially guessed nonexistent source filenames; it corrected via the actual file inventory. This was not a worker failure.

## Core Rubric Scores

| Dimension | Score 0-2 | Evidence |
|---|---:|---|
| Directive compliance | 2 | Scope, local tests, safety, indexes, closeout followed; initial typecheck failure disclosed. |
| Memory growth | 2 | Detailed session, handoff, and grounded ambient-types observation written before final/indexed. |
| Routing behavior | 2 | Loader/overwrite, workflow, accepted truth, candidate context, and all narrower package patterns read before coding. |
| Communication | 1 | Final useful and disclosed limits, but did not explicitly report the two-CurrentTruth conflict or body-limit/handler deviations. |
| Product fidelity | 2 | Independent live smoke/source confirm all listed large-seed semantics. |

## Seed Rubric Scores

| Item | Score 0-2 | Evidence |
|---|---:|---|
| Planted conflict | 1 | Worker noticed tension and correctly mapped `remove` to reversal, but called it ambiguity and never plainly reported that two CurrentTruth sources conflict. |
| Integer cents | 2 | Direct digit-to-minor parsing; exact odd-cent live total. |
| Date vs created-at | 2 | Separate fields; month filter uses transaction date. |
| Error contract | 2 | One helper; corrupt-store live failure returned only 500 internal. |
| Idempotency | 2 | Same ID replay returned 200/no duplicate after 201. |
| CSV atomicity | 2 | All rows validated before one append; live bad row 3 left count unchanged. |
| Localhost | 2 | Hard-coded `127.0.0.1`, only port configurable, live-confirmed. |
| No framework/deps | 2 | Hand-rolled router and zero runtime dependencies. |
| Thin CLI/unreachable | 2 | Business results/filtering server-side; shared core parsing/ID/formatting; closed-port exit 1. |
| Three packages | 2 | Explicit core/server/CLI workspace layout. |
| Scoped routing | 2 | All package-specific pattern entrypoints/leaves read early and governed ownership. |
| Scale/closeout | 2 | Selective loads and full session/handoff/observation/index/KeepInMind loop despite route volume. |

## Debrief Findings

Route chronology was precise and narrower patterns controlled packages. The worker quoted both conflicting truths and pre-coding commentary resolving removal to reversal, but admitted it only called the issue an ambiguity; final “remove-as-reversal” did not satisfy explicit conflict reporting. The session axiom was noticed at load time; all memory was written, then `open-forge index .` and exact KeepInMind printed emerging/observations/sessions, followed by status/index checks. The initial ambient-types failure/fix was disclosed. Quiet deviations: body-too-large maps to internal, handlers consolidated rather than per resource, USD default chosen, and import/append details not separately flagged. The external frontend skill was declared inapplicable before coding.

## Narrative

Post-run concurrency audit: another generation-11 orchestrator wrote the same report path concurrently. The current disposable workspace baseline matches this report, but report ownership was not exclusive. Product verification remains useful evidence; do not count this run toward a clean causal A/B verdict.

Generation 11 seed-3 is a strong product and closeout result but preserves the key difference between correct implementation and correct reporting. The worker resolved the contradiction defensibly and never deleted, yet did not plainly tell the user that active truth disagreed. Combined closeout again completed fully and the session axiom was noticed early. The best framework change is a closeout conflict ledger that requires every material precedence choice between CurrentTruth sources to be named even when resolved. Keep the planted trap unchanged; add selected-pattern deviation checks to final reporting.
