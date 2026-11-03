---
open-forge:
  description: Layer 4 council seed, feature and repair cases, regression guards and richer context
  tags: [Memory, Document, CurrentTruth, Evaluation, NeonPairs, Layer4, Brownfield]
---
# Layer Four
Layer Four is the brownfield case that asks agents to change a council-built Neon Pairs game while keeping its earlier behavior working.

Every attempt starts from the same audited solution or a registered defect copy of it. The one exception is a chained milestone, approved on 2026-11-03: it starts from its own lineage's previous result with the next registered defect applied, and its first milestone still starts from the registered copy. The [Eval Program Plan](../../../../emerging/analysis/eval-program-plan.md#incremental-design) describes how. Code from a Layer 3 attempt never becomes the seed. For example, Hard mode needs a recorded format change, while a session repair starts with a symptom that doesn't reveal its cause.

Status: the seed is the spec 2.2 reference, which needs 10 out of 10 from both a Claude Opus and a Codex Sol judge before this layer freezes. The three cases pass local starting, solution and alternative solution validation on it. Docker validation through the product is next.

## Contents

- [Council seed](#council-seed)
- [First cases](#first-cases)
- [Scoped tasks](#scoped-tasks)
- [Starting project](#starting-project)
- [Worker contract](#worker-contract)
- [Change cases](#change-cases)
- [Gates](#gates)
- [Setups](#setups)
- [Richer context](#richer-context)
- [Not decided yet](#not-decided-yet)

## Council seed

Build the seed only after spec version 2.2 and its clause map are frozen. The council builds a separate solution to the full product contract. It can use earlier code as design context, but an earlier reference isn't compliant until every version 2.2 clause has evidence.

Use this process in order:

1. Have the council agree on the product clauses, supported runtime combination and architecture before implementation. Record disagreements and their decisions without changing a rule silently.
2. Build the complete game and run the locked install, type check, unit tests, fresh browser build, replay and offline delivery journeys in the verifier container. Keep credentials out and keep network off after install.
3. Audit each of the 412 clauses by an independent executable test, a static check or a verdict from two reviewers. Calculate expected decks and results independently. An unsure or disputed verdict leaves that clause open.
4. Fill every clause's `checks` and `reference` slots in `trace.json`. Link check IDs to exact assertions and evidence. Link reference paths and symbols to the same clause. A two-reviewer verdict records both reasons and the source each reviewer read.
5. Mutate one rule at a time. Include broken moves, best-score scope, mismatch edges, pause, stale callbacks, format validation, keyboard movement, hidden names, face separation, focus clipping and stale builds. Show that its target check fails and that the clean seed passes. Record surviving and equivalent mutations. Resolve a surviving material mutation before freezing.
6. Format the complete seed with Prettier `3.9.6`, pinned with its configuration. Record line counts after formatting for pure game functions, browser session, components and styles, application tests, tooling and configuration. For each tier, give file count, physical lines and nonblank lines. List generated and dependency files as exclusions. Check T06's authored TypeScript ceiling per file. Report the 100-line aim separately from the 200-line limit.
7. Record the decision log, including every spec amendment, review disagreement and mutation outcome that changed the result. If a product rule changes, bump the spec version and rebuild its evidence before using the seed.
8. Freeze source, dependency lock, formatter and configuration, full check set, fixtures, trace map, decisions, line counts and evidence hashes as one seed record. Confirm that every executable clause passes and every review clause has two decided verdicts.

The maintainer accepted the reference after three correction rounds and a 10 out of 10 judgment. That reference is the frozen seed for these cases. The process above governs a later seed rebuild. Runtime and screen reader records still name the versions actually tested.

## First cases

The first cases add named snapshots and repair copied snapshots. The Hard mode examples below describe other case shapes.

The registered cases and their local starting results are:

| Case | Type | Request | Checks | Starting result |
| --- | --- | --- | --- | --- |
| [F01](../../../../../../src/evals/benchmarks/neon-pairs/cases/l4-f01/README.md) | F | Name a game and keep the name with saved progress | 10 | One pass and nine feature failures |
| [R01](../../../../../../src/evals/benchmarks/neon-pairs/cases/l4-r01/README.md) | R | Some copied saved games don't open while local exports work | 10 | Seven passes and three repair failures |
| [FR01](../../../../../../src/evals/benchmarks/neon-pairs/cases/l4-fr01/README.md) | FR | Add names and repair copied saved games | 20 | Seven passes and thirteen failures |

F01 and FR01 replace C08.2, D05.1 and D05.2 under spec version 3. Their complete Controls, Reproducibility, Architecture and overview replacements keep the unchanged version 2.1 rules. C15.1 through C15.5 follow C14. Named games use snapshot version 2. Unnamed games, board codes and replay output keep version 1. Both snapshot versions keep the existing validation, pause, focus and best-score rules. Completed imports stay completed without Resume. R01 changes no product clause.

The feature checks name a deal, play a mismatch, wait 800ms, export it, start New Game and reload the app's text. They also cover editing paused and completed progress, clearing names, browser reload, preferences, UTF-16 limits and the version 1 invalid-state vectors inside version 2 snapshots.

The repair checks cover top-level order variants as one result. Separate results cover nested selection order, invalid fields and progress, paused input and completed imports. CRLF drafts pass through the native textarea. The repair probes use published best-score text, card names and live-region text. They don't need the name control. FR01 uses ten feature results, nine repair results and one separate compatibility result for reordered named version 2 snapshots. The compatibility result is in the feature group under the worker contract. R01's own-export check is already covered by FR01's feature journeys.

FR01's request joins F01's sentences and R01's sentences unchanged. R01 and FR01 can answer a request for a failing example with one copied snapshot. The bank describes the symptom without naming its cause.

## Scoped tasks

Scoped tasks are small repairs that each need one scope of the spec, added on 2026-11-03 to test scoped Open Forge setups. Each change check is a clause check: it takes its result from the full-spec checker's assertion for one clause, so a task needs no worker. Those clauses leave the regression set. A case graded only by clause checks needs at least three of them.

| Case | Scope | Request | Checks |
| --- | --- | --- | --- |
| [R02](../../../../../../src/evals/benchmarks/neon-pairs/cases/l4-r02/README.md) | Gameplay | The best score takes worse games and shows 0 without a record | 7 |
| [R03](../../../../../../src/evals/benchmarks/neon-pairs/cases/l4-r03/README.md) | Controls and input | Arrow keys no longer move between cards | 7 |
| [R04](../../../../../../src/evals/benchmarks/neon-pairs/cases/l4-r04/README.md) | Visuals | Cards and the win banner animate under reduced motion | 5 |
| [R05](../../../../../../src/evals/benchmarks/neon-pairs/cases/l4-r05/README.md) | Sound | The game beeps on a match | 4 |

Scenario L4-1 runs them on the four context steps with Plain docs as the control, and leaves judging for later. Its plans are `neon-pairs-l4-1-smoke.json` and `neon-pairs-l4-1-full.json`. [Eval Context And Cost Measures](../../../../emerging/analysis/eval-context-and-cost-measures.md#scoped-tasks-built-2026-11-03) explains why they replaced partial greenfield builds.

## Starting project

Materialize the reference, private seed patch and applicable product delta before creating the single starting commit. The agent receives that tree and its one commit. The clean reference and private patch stay outside the project, so the Git parent and commit diff can't reveal the plant.

Each folder keeps its request, bank, check manifest, seed where needed, known solution and alternative solution. The alternative uses different local modules and tests. Every patch is regenerated with minimal Git hunks from scratch copies under `src/evals/.tmp/` against the current reference or the resulting starting tree.

The runner applies patches, builds, checks frozen regressions and enforces diff scope and protected inputs. F01 and FR01 allow the game source area, application tests and README. R01 allows the snapshot and saved-format areas, their tests, application tests and README. These areas allow different implementations. They don't prescribe the known solution's file choices.

## Worker contract

Each case provides `checks/checks.json` and `checks/worker.ts` as the [brownfield worker contract](../../../../../../src/evals/grading/brownfield/README.md) defines. Each declared ID has one outcome and a feature or repair group. IDs use word characters and hyphens. The combined compatibility result uses `FR01-01`.

Workers use Node built-ins, Playwright and files inside their case folder. Each case packages its probes because the runner copies one folder. Workers never import submitted Node source. Each check gets a fresh browser context. Missing built HTML blocks browser checks. An absent feature still allows repair checks to run.

Local validation builds each starting project, known solution and alternative solution. The public unit tests pass on all nine trees. The case workers match all 120 starting and solution expectations. Both solutions pass each case's change checks and all 65 public browser journeys. The product patch parser accepts all six solution patches. Direct scope and protected-input checks pass on all nine source trees.

Each case's `validation.json` records local evidence. The [packet validation record](../../../../../../src/evals/benchmarks/neon-pairs/cases/l4-f01/packet-validation.json) lists product-test failures, the changed files and validation limits. Typechecking passes. Full frozen regression and Docker validation need the shared spec-version loader and stale Layer 2 input records fixed outside this packet.

## Change cases

F adds a feature to the clean seed. R repairs a private planted defect. FR combines an addition and a repair. Give all setups the same request and product rules. The request sounds like the user, while the complete behavior lives in the project documents. Never put a file name, function signature, formula, defect location or repair recipe in the request.

### Feature

Example request:

> Could you add a Hard mode? I want a tougher game, and I want saved progress to keep the mode I was playing. Please follow the project rules.

Publish the feature rules in the case's product documents before the run:

| Subject | Required change |
| --- | --- |
| Mode | Offer Normal and Hard. Keep Normal as the default. Changing mode starts a fresh game at the current size and clears progress. New Game, Restart and size changes keep the current mode. |
| Timing | Normal keeps the 800ms mismatch interval. Hard uses 400 active milliseconds with the same pause and ignored-input rules. |
| Bests | Keep best moves separately by size and mode. Existing saved bests become Normal records. Show the current size and mode's best. |
| Snapshot | Export version 2 with the original fields plus mode, whose values are normal and hard. Import version 1 as Normal. Import version 2 only with the exact declared fields. For a pending mismatch, require remainingMs from 1 through 400 in Hard and 1 through 800 in Normal. Import stays paused until Resume. |
| Replay | Export and read version 2 with the original fields plus mode. Keep version 1 replay as Normal. Apply the mode's interval at exact action-time edges. |
| Deal | Keep np1 board codes and deck generation unchanged. A board code describes cards, while mode describes play. Loading a board code keeps the current mode. |
| Accessibility | Name the mode control and expose its current value. Keep keyboard, announcements, touch and zoom working. |

The feature delta must enumerate the changed clauses under R04, R09, R10, C08, C09, D05 through D10 and the added mode control. Keep every untouched clause frozen. The case publishes the exact replacement expectations and migration rules. The feature cannot keep exporting version 1 snapshots because that format has no mode field.

The F starting code passes the entire seed suite. Feature checks fail before work because Hard mode is absent, then pass on the council's known solution.

### Repair

Example request:

> Sometimes after I start New Game while two different cards are up, a card in the next game closes on its own. Can you fix that? Everything else should keep working.

Plant the defect only in a private copy of the audited seed. The public source tests still pass. A private lifecycle journey reproduces the reported symptom. Keep the plant, affected code, expected repair and check details outside the agent's workspace and answer bank.

The rule is already in R12 and A04, so this case changes no product expectation. The simulated user can repeat only the symptom and the project rules. It doesn't know which callback, file or module is wrong.

Before eligibility, show that the clean seed passes, the defect copy fails the target repair checks and the known repair passes the full frozen suite. Record any unrelated failures as a bad case, not as agent performance.

### Combined change

Example request:

> Could you add Hard mode and keep the mode when I save my progress? Also, a card sometimes closes on its own after I start New Game during a miss. Please fix that and keep the rest working.

Use the same published feature delta as F and the same symptom as R. The starting copy has the planted defect and no Hard mode. Feature checks use fresh stable sessions without the defect trigger. Repair checks use Normal mode and don't need the new control or version 2 imports. Run both groups independently, with shared frozen regressions afterward. A missing feature mustn't block the repair result, and an unrepaired timer mustn't block feature checks that don't trigger it.

## Gates

Confirm seed eligibility before every case starts. A repair case also needs its defect to fail the target checks. After the agent finishes, run these three guards as well as the change checks:

| Gate | Passing condition |
| --- | --- |
| Frozen regressions | Every unchanged Layer 3 check passes. A published feature delta can replace only its named expectations. Old-format import and replay fixtures still pass under their migration rules. |
| Diff scope | The diff changes only behavior, tests, styles, docs and format updates needed by the request. Keep unrelated rewrites, broad reformatting, dependency churn and removed tests out. Register the allowed behavior areas before the batch. |
| Protected inputs | The frozen specifications, requirement and trace maps, check definitions, expected fixtures, mutation patches, rubrics, case records and input hashes stay unchanged outside the agent workspace. Compare their hashes before and after verification. |

For diff scope, compare against the case's starting tree after the pinned formatter. Ignore files according to the starting `.gitignore`, so later ignore edits can't hide changes. In Open Forge cells, allow and report edits to installed Open Forge files, routes and `.agents/` work records. Other cells get no Open Forge exemption. Record added, changed and deleted paths with the reason each belongs to the task. Two reviewers decide a disputed scope result with cited hunks. A formatter change can't hide a broad rewrite.

For protected inputs, build as an account that can't read grading files. Keep the verifier and assessor separate from the submitted project. Read copied output only through `copyOutPlain` and validate it as untrusted. Agent test reports never replace independent expected results.

The guards report separate outcomes. A changed expectation without a published delta fails compatibility. An unchanged check failure is a regression even if the new feature works. The [Grading](../grading.md) defines check outcomes and review uncertainty.

## Setups

Use code only, plain docs, linked AGENTS.md and Open Forge. Open Forge has separate Minimal, Essentials and Full Core cells. Give each the identical case seed, request and public tests. Code only provides the source, public tests and request without product documents. Report it as a separate information ablation. The plain, linked and Open Forge forms get identical product facts and differ only in delivery. Don't include code only in an equal-information routing conclusion.

The four context steps each add one thing to the step before: Code only, Plain docs with its linked specs, Full Core with every document up front, and Full Core, scoped. Plain docs stays the control. The published setup ladder keeps its own name for the six setups numbered 0 to 5.

Run each registered case at most five times per cell, model and tool. The full eval runs three times per cell. The check eval runs once. Keep F, R and FR as separate case types, not setups. Pin the time limit before the batch. Start with 60 minutes per case in testing, then freeze the measured limit before scored attempts. Helpful and Extension testing wait.

## Richer context

The richer-context variant is still Layer 4. Keep the seed, request, rules, change checks, regression gates, limits and registered run count fixed. Add reviewed project history, architecture explanations and decision records, with realistic extra documents that make finding the relevant rule matter.

Give every delivery form the same extra bodies and descriptions. Open Forge can route them while the linked form uses the same descriptions. Extra context can't add a requirement or reveal a defect. Record a separate context-variant axis and compare it within the same change type, tool and model.

## Not decided yet

- The council members and build allocation for a later seed rebuild.
- The case limits after timed agent testing. Freeze them before scored runs.
- The extra documents for the richer-context variant. Check information equality before freezing them.

Back to [Neon Pairs](_neon-pairs.md).
