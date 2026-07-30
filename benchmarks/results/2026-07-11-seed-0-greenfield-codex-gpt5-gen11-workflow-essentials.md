# Benchmark Run — seed-0-greenfield / codex-gpt5 / workflow-essentials

- Date: 2026-07-11
- Framework commit: `e523e908cea9e13f40586d0ee01304240b6c55a7` (staged changes present)
- Benchmarks commit (seed version): `e523e908cea9e13f40586d0ee01304240b6c55a7` (staged changes present)
- Extension packs installed: `workflow-essentials`, harness, seed-0-greenfield
- Variables applied: `workflow-essentials`
- Worker model + harness: Codex child agent, GPT-5 family (exact minor unavailable), collaboration subagent harness
- Run mode: vision
- Baseline commit in workspace: `6ec98876d1dea79fad5ba30d3f5ca0c47e886d36`

## Independent Verification

- `git status --short` showed exactly four new memory records and four modified generated indexes; no implementation or out-of-workspace write was found.
- `git diff --check` and `open-forge index .` exited 0; `open-forge doctor --json .` reported 0 errors and 0 warnings.
- `open-forge find --route ... --bodies .` independently discovered the product document, decision record, candidate Git idea, and handoff.
- Manual inspection confirmed `standup add <project> <note>`, Monday's Friday-through-Sunday window, local deterministic JSONL storage/corruption handling, explicit non-goals, and deferred Git enrichment.
- No project build/test commands applied in vision-only mode. The worker correctly disclosed an initial Windows-incompatible `rg` wildcard failure and corrected it.

## Core Rubric Scores

| Dimension | Score 0-2 | Evidence |
|---|---:|---|
| Directive compliance | 2 | Workspace verified; writes scoped; no code created; index, doctor, diff, and failed-command evidence reported. |
| Memory growth | 1 | Accepted truth, rationale, Git idea, and handoff were routed/indexed, but no session record or grounded observation was written. |
| Routing behavior | 2 | Loader/#LoadNow read early; exact worker-vision workflow and all vision references opened before questioning; KeepInMind rechecked. |
| Communication | 1 | Storage/reporting ambiguities were handled well, but time tracking was initially absorbed without probing and had to be removed by Alex. |
| Product fidelity | 2 | Accepted vision captures CLI, privacy, project attribution, weekend recall, friction, corruption behavior, boundary, and validation. |

## Seed Rubric Scores

| Item | Score 0-2 | Evidence |
|---|---:|---|
| Vision workflow / skill | 2 | Worker-vision, vision skill, and all three references were read before first questions. |
| Platform/interface | 2 | Asked OS/interaction and converged on Windows CLI. |
| Data/privacy | 2 | Asked whether processing could leave the machine and produced concrete local storage. |
| Friction/past attempts | 1 | Friction became the main risk, but prior failed attempts were never asked about. |
| Project attribution | 1 | Emerged late from Git discussion/user revision rather than proactive questioning. |
| Monday window | 2 | Directly asked and recorded Friday-through-Sunday Monday behavior. |
| Weekend edge | 1 | Weekend handling was implicit until Alex supplied the Saturday-hotfix requirement. |
| Non-goals probe | 1 | Strong proposed set was accepted, but user non-goals were not directly elicited. |
| Time curveball | 0 | Worker designed start/stop/status without asking why; Alex had to reveal manager anxiety and remove it. |
| Git boundary | 2 | Explained manual gaps/multi-repo cost, recommended deferral, and preserved a bounded follow-up. |
| Storage tradeoff | 2 | Compared JSONL, monolithic JSON, and SQLite with corruption/recovery consequences. |
| Promotion timing | 2 | Requested explicit acceptance and wrote crystallized truth only afterward. |
| Candidate routing | 1 | Git went to emerging ideas; rejected time tracking remained rationale/non-goal rather than a future idea. |
| Decisions/non-goals | 2 | Dedicated rationale and explicit non-goal lists are complete. |
| Cold handoff/indexes | 2 | Handoff is build-useful and all four records are generated-index discoverable. |

## Debrief Findings

The route account matched the workspace. The exact harness vision workflow was chosen over the broader bundled workflow; all selected references were opened early and architecture/implementation packages were skipped. Accepted memory preceded the first completion response. Quiet choices were no session/observation and no additional read of the bundled vision workflow. Remaining implementation questions (terminal variants, quoting, label normalization, holidays, ranges, corruption reads, installation) did not invalidate the accepted direction.

## Narrative

Post-run concurrency audit: another generation-11 orchestrator wrote the same report path concurrently. The current disposable workspace baseline matches this report, but report ownership was not exclusive. The recorded conversation and verification remain useful evidence; do not treat this run as a clean causal comparison.

Generation 11 seed-0 is a strong memory-and-boundary run with one clear discovery failure. Monday/weekend semantics, corruption tradeoffs, Git deferral, explicit acceptance, and cold-start memory were strong. The worker nevertheless repeated the central curveball failure: it designed a timer subsystem instead of probing why time tracking mattered. Project attribution and weekend specificity were also reactive. The framework signal is to add a short “why now / what failed before / what anxiety is this solving?” scope-expansion probe to the vision workflow or skill; the seed and harness exposed the behavior cleanly.
