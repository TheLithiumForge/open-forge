# Layer 1 grading
Layer 1 grading is a check pack that measures whether a Neon Pairs attempt meets the Layer 1 request.

The pack finds cards through visible play and compares their screenshots. For example, a 12x12 board has 144 cards even when New Game has a similar size. Card detection groups elements within their board container so it keeps that control out.

The checker version is `L1-v3`. `layers/L1.validation.json` records reference and mutation results with their input hashes.

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

The plan prints expected results without executing games. The second command runs the browser regressions, then builds and checks the reference and four mutated copies in verifier containers. It copies plain output through `copyOutPlain`, validates the report and compares every outcome with the fixture's registered result. A complete match writes `layers/L1.validation.json` with the checked input hashes.

The reference stays unchanged. Fixture edits create separate projects and require each replacement to match exactly once. The mutations cover faces repeating after 32, overwritten best scores, retained moves and unreachable keyboard cards. Their JSON files record every expected result.

## Regressions

The small pages in `regression-fixtures.ts` cover the six faults from the first matrix batch. They use checker markup and scripts written for these tests. Saved attempts are read as data only.

From `src/evals`, run the pages in the existing verifier image:

```sh
node benchmarks/neon-pairs/checks/validate.ts --regressions
```

The tests compare working and broken pages through the checker functions. Pure tests also show which names, face pixels and win phrases the old checker misses. Browser tests run when `PLAYWRIGHT_BROWSERS_PATH` is set, which the container command supplies. They cover pointer and keyboard games, reset after play, dialog controls and every size from 4 through 16.

The focus glow fixture closes a mismatch, then finishes by pointer and by keyboard. Focus stays on the card after the mismatch closes. The inert copy still fails reveal detection in both modes.

The repeated activation fixture turns a card, moves focus away, then activates that card again. Its unfocused face is dim enough to resemble its back. The test repeats a matched card too, checks that both repeats keep the move count unchanged and finishes the game. Pointer and keyboard runs check that the face stays classified as a face. The inert copy still fails reveal detection.

Before activation, the checker captures known backs after focus moves. These images give each card alternate backs for the current game. A card can add an alternate back only before its first activation or revealed face, and only when its coarse signature matches its original back before focus moves. An activation attempt excludes the card even if the game ignores that input. A revealed face can resemble a back, so the card stays excluded after a mismatch closes. Reading the next fresh board resets this record.

The pixel comparison ignores gradual light changes near card edges, where a nearby focus glow can reach a closed card. It allows each pixel the color range within one pixel of the other image, in both directions, because 3D flips can round the same back differently. New strokes still differ from that nearby back, including soft dim strokes. Signatures for comparing faces keep their existing thresholds.

The row fixture groups a 16x16 board into row containers. Card detection needs rows and columns in the chosen container, so one row of sixteen cards doesn't become the whole board. The matched-style fixture changes both faces after a match. Input checks compare both cards in one board screenshot, so the first card's earlier style doesn't create a false mismatch.

## Diagnostics

To save card crops and look comparisons during a recheck, set `EVALS_L1_DIAGNOSTICS=1` before running `npm run evals -- recheck <batch> <runId>` from the repository root. It defaults to off. Set `EVALS_SCRATCH` and npm's cache folder to a scratch drive before running.

The assessment's evidence holds a `shots/diagnostics/` folder. Each numbered JSON file records its page, capture kind, card indexes, back decisions, color distance, shape distance, brightness distance and full pixel comparison. The matching PNG files show the card crops. `backs` starts a fresh board, `face` checks a reveal and `board` reads all cards together. Capture stops after 400 steps and covers boards with at most sixteen cards. Diagnostic files stay apart from the judge's screenshots. They leave the container through `copyOutPlain`, then pass filename, file count, byte, image and JSON checks before saving.

## Judge

`rubrics/L1.json` defines eight questions and three ballots each. Rules, code, visuals and wishes have separate areas. Ballots need valid verdicts and citations to files, lines or image IDs. Invalid model output fails the judge assessment and keeps the check results.

The judge receives filtered source, the Layer 1 prompt, board screenshots, the win screenshot and a face sheet. Setup files, generated output and test reports are excluded. Package metadata and known tooling dependencies are removed from its view. Both providers use the same evidence builder.

The default judge adapter sends images through the assessor container with model tools disabled. An adapter without image support fails before a model call. Fake image support tests the complete ballot path without Docker or model calls.

## Evidence

From `src/evals`, run `npm run typecheck` and `npm test` for product checks. Product tests use fakes for Docker and model calls. The container regression command also runs the browser cases that a host test run skips without `PLAYWRIGHT_BROWSERS_PATH`.

The detector probe in `browser-probe.ts` uses checker markup with scripts disabled. It checks every even size from 4x4 through 16x16, including 144 cards at 12x12. The browser regressions add row containers and the broken layout cases described above.

## Changes

The regression files keep working and broken cases for these corrections:

| Finding | Correction | Broken case |
| --- | --- | --- |
| Name | Accept the document title or the page text with only letters and case ignored | Different title and logo |
| Turned cards | Compare full resolution pixels against each card's backs captured before activation, with the pointer off the cards | Inert cards with focus and hover glows |
| Best score | Read a score followed by its unit under the label and a dash with text as an empty best | Text without a score and unrelated card counts |
| Win message | Read new finishing text after the last pair | No new message and unchanged finishing text |
| Modal win | Try Escape, then a close, dismiss or play again button before actions | Dialog that cancels Escape and has no exit |
| Odd sizes | Check the square layout with one empty cell on odd boards | Unpaired count, extra missing cards and wrong layout |
| Closed 3D backs | Allow one-pixel edge ranges in both directions | New dim strokes and inert glowing cards |
| Matched styles | Compare the two cards in the same screenshot | Inert glowing cards |
| Row containers | Find rows and columns before choosing a board container | Wrong card counts and layouts |

Signatures for comparing faces and the `remember-size` wish keep their previous rules. The `L1-v3` version also updates the layer, clause map, rubric, suite and fixture declarations.
