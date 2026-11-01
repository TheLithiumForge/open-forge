---
open-forge:
  description: Layer 4 council seed, feature and repair cases, regression guards and richer context
  tags: [Memory, Document, CurrentTruth, Evaluation, NeonPairs, Layer4, Brownfield]
---
# Layer Four
Layer Four is the brownfield case that asks agents to change a council-built Neon Pairs game while keeping its earlier behavior working.

Status: plan ready. The spec version 2 seed, its audit and the frozen cases aren't built yet.

Every attempt starts from the same audited solution or a registered defect copy of it. Code from a Layer 3 attempt never becomes the seed. For example, Hard mode needs a recorded format change, while a session repair starts with a symptom that doesn't reveal its cause.

## Council seed

Build the seed only after spec version 2 and its clause map are frozen. The council builds a separate solution to the full product contract. It can use earlier code as design context, but an earlier reference isn't compliant until every version 2 clause has evidence.

Use this process in order:

1. Have the council agree on the product clauses, supported runtime combination and architecture before implementation. Record disagreements and their decisions without changing a rule silently.
2. Build the complete game and run the locked install, type check, unit tests, fresh browser build, replay and offline delivery journeys in the verifier container. Keep credentials out and keep network off after install.
3. Audit each of the 391 clauses by an independent executable test, a static check or a verdict from two reviewers. Calculate expected decks and results independently. An unsure or disputed verdict leaves that clause open.
4. Fill every clause's `checks` and `reference` slots in `trace.json`. Link check IDs to exact assertions and evidence. Link reference paths and symbols to the same clause. A two-reviewer verdict records both reasons and the source each reviewer read.
5. Mutate one rule at a time. Include broken moves, best-score scope, mismatch edges, pause, stale callbacks, format validation, keyboard movement, hidden names, face separation, focus clipping and stale builds. Show that its target check fails and that the clean seed passes. Record surviving and equivalent mutations. Resolve a surviving material mutation before freezing.
6. Format the complete seed with Prettier `3.9.6`, pinned with its configuration. Record line counts after formatting for pure game functions, browser session, components and styles, application tests, tooling and configuration. For each tier, give file count, physical lines and nonblank lines. List generated and dependency files as exclusions. Check T06's authored TypeScript ceiling per file. Report the 100-line aim separately from the 200-line limit.
7. Record the decision log, including every spec amendment, review disagreement and mutation outcome that changed the result. If a product rule changes, bump the spec version and rebuild its evidence before using the seed.
8. Freeze source, dependency lock, formatter and configuration, full check set, fixtures, trace map, decisions, line counts and evidence hashes as one seed record. Confirm that every executable clause passes and every review clause has two decided verdicts.

The council process is future seed work, not a claim that this document packet has audited the application. The runtime and screen reader records must include the versions actually tested. Complete the missing version 2 record before declaring the seed eligible.

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

- The council members and build allocation. Choose them when seed work starts.
- The exact private mutations, added check sets and case limits after testing. Freeze them before scored runs.
- The extra documents for the richer-context variant. Check information equality before freezing them.

Back to [Neon Pairs](_neon-pairs.md).
