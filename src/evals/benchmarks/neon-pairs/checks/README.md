# Layer 1 grading
Layer 1 grading is a check pack that measures whether a Neon Pairs attempt meets the Layer 1 request.

The pack finds cards through visible play and compares their screenshots. For example, a 12x12 board has 144 cards even when New Game has a similar size. Card detection groups elements within their board container so it keeps that control out.

The checker version is `L1-v3`. Reference and mutation validation is pending for this version.

## Check sets

`layers/L1.json` declares 16 stated checks, 6 working checks and 3 wishes. The working gate covers offline opening, pointer play, phone tap, keyboard play, the win and New Game resetting cards and moves. Uncaught app errors fail the affected checks. Starting size, remembered size and silence are wishes. Asking belongs to collaboration.

Each check returns `pass`, `fail`, `error` or `blocked` with evidence. A game failure and a checker failure keep different statuses. Missing results caused by a stopped group become blocked. Missing independent results become errors. The first result for an ID is kept.

`layers/L1.requirements.json` maps the prompt clauses to checks and source review. The layer, clauses and rubric hashes feed both grading fingerprints and assessment versions.

## Validate

From `src/evals`, run:

```sh
node benchmarks/neon-pairs/checks/validate.ts --plan
node benchmarks/neon-pairs/checks/validate.ts
```

The plan prints expected results without executing games. The second command builds and checks the reference and four mutated copies in verifier containers. It copies plain output through `copyOutPlain`, validates the report and compares every outcome with the fixture's registered result. A complete match writes `layers/L1.validation.json` with the checked input hashes.

The reference stays unchanged. Fixture edits create separate projects and require each replacement to match exactly once. The mutations cover faces repeating after 32, overwritten best scores, retained moves and unreachable keyboard cards. Their JSON files record every expected result.

## Regressions

The small pages in `regression-fixtures.ts` cover the six faults from the first matrix batch. They use checker markup and scripts written for these tests. Saved attempts are read as data only.

From `src/evals`, install Chromium in the worktree, then run the pages:

```powershell
$env:PLAYWRIGHT_BROWSERS_PATH = Join-Path (Get-Location) '.playwright-browsers'
npm exec -- playwright install chromium
node --test benchmarks/neon-pairs/checks/regressions.test.ts
```

The tests compare working and broken pages through the checker functions. Pure tests also show which names, face pixels and win phrases the old checker misses. Browser tests run when `PLAYWRIGHT_BROWSERS_PATH` is set. They cover pointer and keyboard games, reset after play, dialog controls and every size from 4 through 16.

## Judge

`rubrics/L1.json` defines eight questions and three ballots each. Rules, code, visuals and wishes have separate areas. Ballots need valid verdicts and citations to files, lines or image IDs. Invalid model output fails the judge assessment and keeps the check results.

The judge receives filtered source, the Layer 1 prompt, board screenshots, the win screenshot and a face sheet. Setup files, generated output and test reports are excluded. Package metadata and known tooling dependencies are removed from its view. Both providers use the same evidence builder.

The default judge adapter sends images through the assessor container with model tools disabled. An adapter without image support fails before a model call. Fake image support tests the complete ballot path without Docker or model calls.

## Evidence

Dependencies were installed with `npm ci --cache .npm-cache`. Product tests use fakes for Docker and model calls. The regression pages use the installed Chromium.

Chromium was installed inside the worktree. The detector probe passed every even size from 4x4 through 16x16, including 144 cards at 12x12. Run `node benchmarks/neon-pairs/checks/browser-probe.ts` with `PLAYWRIGHT_BROWSERS_PATH` set to your installed browser folder to repeat that check. The probe uses checker markup with scripts disabled.

The Docker validation matrix wasn't run in this packet. The previous validation record is cleared so `L1-v3` can't reuse results from the old checker.

## Changes

Each correction keeps a broken case in `regressions.test.ts`:

| Finding | Correction | Broken case |
| --- | --- | --- |
| Name | Accept the document title or the page text with only letters and case ignored | Different title and logo |
| Turned cards | Compare full resolution pixels against each card's back with the pointer off the cards | Inert cards with a hover tint |
| Best score | Read a score followed by its unit under the label and a dash with text as an empty best | Text without a score and unrelated card counts |
| Win message | Read new finishing text after the last pair | No new message and unchanged finishing text |
| Modal win | Try Escape, then a close, dismiss or play again button before actions | Dialog that cancels Escape and has no exit |
| Odd sizes | Check the square layout with one empty cell on odd boards | Unpaired count, extra missing cards and wrong layout |

Signatures for comparing faces and the `remember-size` wish keep their previous rules. The `L1-v3` version also updates the layer, clause map, rubric, suite and fixture declarations.
