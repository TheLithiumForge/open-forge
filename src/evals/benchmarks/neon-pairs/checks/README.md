# Layer 1 grading
Layer 1 grading is a check pack that measures whether a Neon Pairs attempt meets the Layer 1 request.

The pack finds cards through visible play and compares their screenshots. For example, a 12x12 board has 144 cards even when New Game has a similar size. Card detection groups elements within their board container so it keeps that control out.

Status: `L1-v2` candidate. Reference and mutation validation is pending.

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

## Judge

`rubrics/L1.json` defines eight questions and three ballots each. Rules, code, visuals and wishes have separate areas. Ballots need valid verdicts and citations to files, lines or image IDs. Invalid model output fails the judge assessment and keeps the check results.

The judge receives filtered source, the Layer 1 prompt, board screenshots, the win screenshot and a face sheet. Setup files, generated output and test reports are excluded. Package metadata and known tooling dependencies are removed from its view. Both providers use the same evidence builder.

The existing assessor interface accepts text and disables tools. It has no image transport. The default judge adapter therefore records a failed judge assessment before a model call. Completing visual ballots needs image transport in that interface. Fake image support tests the complete ballot path without Docker or model calls.

## Evidence

`npm ci`, `npm run typecheck` and `npm test` pass. The product suite has 85 passing tests and zero skips. Docker and model calls use fakes in these tests.

Chromium was installed inside the worktree. The detector probe passed every even size from 4x4 through 16x16, including 144 cards at 12x12. Run `node benchmarks/neon-pairs/checks/browser-probe.ts` with `PLAYWRIGHT_BROWSERS_PATH` set to your installed browser folder to repeat that check. The probe uses checker markup with scripts disabled.

The reference and mutation matrix wasn't executed because this packet forbids Docker runs. Its registered expectations are still unverified, so the validation record stays pending.
