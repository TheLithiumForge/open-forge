---
open-forge:
  description: Layer 4 council seed, feature and repair cases, regression guards and richer context
  tags: [Memory, Document, CurrentTruth, Evaluation, NeonPairs, Layer4, Brownfield]
---
# Layer Four
Layer Four is the brownfield case that asks agents to change a council-built Neon Pairs game while keeping its earlier behavior working.

Status: the maintainer froze the reference after a 10 out of 10 judgment. The first three case workers match the runner contract and are locally validated. Product validation is pending.

Every attempt starts from the same audited solution or a registered defect copy of it. Code from a Layer 3 attempt never becomes the seed. For example, Hard mode needs a recorded format change, while a session repair starts with a symptom that doesn't reveal its cause.

## Council seed

Build the seed only after spec version 2 and its clause map are frozen. The council builds a separate solution to the full product contract. It can use earlier code as design context, but an earlier reference isn't compliant until every version 2 clause has evidence.

Use this process in order:

1. Have the council agree on the product clauses, supported runtime combination and architecture before implementation. Record disagreements and their decisions without changing a rule silently.
2. Build the complete game and run the locked install, type check, unit tests, fresh browser build, replay and offline delivery journeys in the verifier container. Keep credentials out and keep network off after install.
3. Audit each of the 401 clauses by an independent executable test, a static check or a verdict from two reviewers. Calculate expected decks and results independently. An unsure or disputed verdict leaves that clause open.
4. Fill every clause's `checks` and `reference` slots in `trace.json`. Link check IDs to exact assertions and evidence. Link reference paths and symbols to the same clause. A two-reviewer verdict records both reasons and the source each reviewer read.
5. Mutate one rule at a time. Include broken moves, best-score scope, mismatch edges, pause, stale callbacks, format validation, keyboard movement, hidden names, face separation, focus clipping and stale builds. Show that its target check fails and that the clean seed passes. Record surviving and equivalent mutations. Resolve a surviving material mutation before freezing.
6. Format the complete seed with Prettier `3.9.6`, pinned with its configuration. Record line counts after formatting for pure game functions, browser session, components and styles, application tests, tooling and configuration. For each tier, give file count, physical lines and nonblank lines. List generated and dependency files as exclusions. Check T06's authored TypeScript ceiling per file. Report the 100-line aim separately from the 200-line limit.
7. Record the decision log, including every spec amendment, review disagreement and mutation outcome that changed the result. If a product rule changes, bump the spec version and rebuild its evidence before using the seed.
8. Freeze source, dependency lock, formatter and configuration, full check set, fixtures, trace map, decisions, line counts and evidence hashes as one seed record. Confirm that every executable clause passes and every review clause has two decided verdicts.

The maintainer accepted the reference after three correction rounds and a 10 out of 10 judgment. That reference is the frozen seed for these cases. The process above governs a later seed rebuild. Runtime and screen reader records still name the versions actually tested.

## First cases

The first cases use named snapshot saves and a snapshot import repair. The Hard mode examples below describe other case shapes. They aren't the first feature delta.

The registered cases are:

| Case | Type | Request | Checks | Starting result |
| --- | --- | --- | --- | --- |
| [F01](../../../../../../src/evals/benchmarks/neon-pairs/cases/l4-f01/README.md) | F | Name a game and keep the name with saved progress | 10 | Nine feature failures and one compatibility pass |
| [R01](../../../../../../src/evals/benchmarks/neon-pairs/cases/l4-r01/README.md) | R | Some copied saved games don't open while local exports work | 10 | Ten repair failures |
| [FR01](../../../../../../src/evals/benchmarks/neon-pairs/cases/l4-fr01/README.md) | FR | Add names and repair copied saved games | 20 | Nineteen failures and one compatibility pass |

F01 and FR01 replace C08.2, D05.1 and D05.2. Named games use snapshot version 2. Unnamed games and version 1 replay output keep version 1. Both versions import with the existing pause, focus, validation and best-score rules. The product rules live in each case's complete replacement Controls and Reproducibility documents. R01 changes no product clause.

Each folder records its request, answer bank, patches, check entry, expected outcomes, exact diff scope and validation results. The starting seed patch stays private. The known solution applies to that starting project. The repair checks use version 1 snapshots and work independently of the feature. The combined feature checks use the normal snapshot member order and work independently of the repair. The feature changes the same import path that carries the defect.

The product runner handles patch application, builds, regressions, diffScope and protected-input checks. Each case has a private worker that checks the built page. A case's validation.json records its current detached worker results. The earlier case-local tooling has been removed.

The initial implementation validation had these results. Each starting project passed 123 public unit tests and all 61 public browser journeys. The requests, product deltas and patches are byte unchanged during the worker conversion:

| Case | Starting change checks | Solution change checks | Solution unit tests | Solution browser journeys |
| --- | --- | --- | --- | --- |
| F01 | One pass and nine failures | 10 passed | 125 passed | 61 passed |
| R01 | Ten failures | 10 passed | 124 passed | 61 passed |
| FR01 | One pass and nineteen failures | 20 passed | 126 passed | 61 passed |

The combined worker also passes an independence check. Its feature-only solution passes all ten feature checks and fails all ten repair checks. Its repair-only solution passes all ten repair checks while the feature results match the starting project. The original implementation validation also checked a named snapshot with its name before its version. The feature-only solution rejected it through the planted defect. The full solution accepted it. The original scope and protected-input checks passed.

The parent eval type check passes. Its current test run has 182 passes and six pre-existing failures. Five failures concern a stale Layer 2 source fingerprint. One source scan enters installed reference cache and Chromium folders and rejects an oversized file. Those files and checks are outside this packet. The [packet validation record](../../../../../../src/evals/benchmarks/neon-pairs/cases/l4-f01/packet-validation.json) gives the limits.

Every reference game file matches its recorded seed hash. The integrated trace.json differs from the older hash in seed-record.json. Both files are outside this packet and stay unchanged. The overseer needs to refresh that metadata before integration qualification.

## Worker contract

Each case provides `checks/checks.json` and `checks/worker.ts` as the [brownfield worker contract](../../../../../../src/evals/grading/brownfield/README.md) defines. The manifest gives each check a feature or repair kind and keeps its starting and solution expectations. The worker takes one input JSON path and writes one report outcome per declared ID.

The runner accepts check IDs made of word characters and hyphens. The converted IDs run from `F01-01` through `F01-10` and `R01-01` through `R01-10`. Each manifest entry keeps its earlier dotted ID in `previousId`. Product clause IDs and check behavior stay unchanged.

Workers use Node built-ins, Playwright and files inside their own case folder. Each case packages its shared probes because the runner copies one case folder. No worker imports the submitted Node source or another case. Each check gets a fresh browser context. A missing built page returns blocked outcomes for all browser checks. An absent feature still allows unrelated repair checks to run.

Detached local validation built fresh starting and solution pages and invoked each packaged worker as a CLI. The product manifest and report parsers accepted every report. All 80 starting and solution outcomes matched their expectations. Another 40 outcomes were blocked for missing builds. Private checker file faults returned 40 error outcomes. The combined worker also matched all 40 expected outcomes on feature-only and repair-only copies. Four protocol tests cover malformed inputs, group matching and missing-build reports. Full validation through the product stays with the overseer.

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

For diff scope, compare against the case's starting tree after the pinned formatter. Record added, changed and deleted paths with the reason each belongs to the task. Two reviewers decide a disputed scope result with cited hunks. A formatter change can't hide a broad rewrite.

For protected inputs, build as an account that can't read grading files. Keep the verifier and assessor separate from the submitted project. Read copied output only through `copyOutPlain` and validate it as untrusted. Agent test reports never replace independent expected results.

The guards report separate outcomes. A changed expectation without a published delta fails compatibility. An unchanged check failure is a regression even if the new feature works. The [Grading](../grading.md) defines check outcomes and review uncertainty.

## Setups

Use code only, plain docs, linked AGENTS.md and Open Forge. Open Forge has separate Minimal, Essentials and Full Core cells. Give each the identical case seed, request and public tests. Code only provides the source, public tests and request without product documents. Report it as a separate information ablation. The plain, linked and Open Forge forms get identical product facts and differ only in delivery. Don't include code only in an equal-information routing conclusion.

Run each registered case five times per cell, model and tool. Keep F, R and FR as separate case types, not setups. Pin the time limit before the batch. Start with 60 minutes per case in testing, then freeze the measured limit before scored attempts. Helpful and Extension testing wait.

## Richer context

The richer-context variant is still Layer 4. Keep the seed, request, rules, change checks, regression gates, limits and five-run count fixed. Add reviewed project history, architecture explanations and decision records, with realistic extra documents that make finding the relevant rule matter.

Give every delivery form the same extra bodies and descriptions. Open Forge can route them while the linked form uses the same descriptions. Extra context can't add a requirement or reveal a defect. Record a separate context-variant axis and compare it within the same change type, tool and model.

## Not decided yet

- The council members and build allocation for a later seed rebuild.
- The case limits after timed agent testing. Freeze them before scored runs.
- The extra documents for the richer-context variant. Check information equality before freezing them.

Back to [Neon Pairs](_neon-pairs.md).
