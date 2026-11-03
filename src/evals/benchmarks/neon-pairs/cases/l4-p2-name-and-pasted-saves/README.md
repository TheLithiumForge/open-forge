# Name and reopen saves
P2 is a combined feature and repair case that adds names and repairs copied snapshots in the same import path.

You can name progress and import a reordered named snapshot after the repair. The separate compatibility result checks that interaction.

Status: local validation passed on reference 2.2. Docker validation through the product and the reference's final 10 out of 10 judgment are pending.

## Case

The request joins F01's sentences followed by R01's unchanged. The version 3 product delta is identical to F01's. The private seed is identical to R01's. The compatibility result has ID `FR01-01` and uses the feature group under the runner contract. R01's own-export check is already covered by the feature journeys.

The runner creates one starting commit after applying the seed and product delta. It keeps the clean reference and private patches outside the agent project. The starting change result is seven passes and thirteen failures.

## Checks

The manifest declares ten feature checks, nine repair checks and one compatibility check. Each outcome keeps its own evidence.

The feature probes cover the named-save journey, UTF-16 input limits, name clearing, paused and completed editing, reload, preferences and invalid version 2 states.

Repair probes use published best-score text, card names, ignored activation while paused and matching live-region text. Top-level member-order variants count as one result. Nested selection order, invalid fields, inconsistent progress, pause and completed loads test different behavior. CRLF drafts pass through the native textarea, which changes line endings to LF.

## Scope

The game source under `src/game/`, application tests under `tests/` and `README.md` can change. The patterns in `case.json` describe behavior areas. Both solutions exercise them with different module names and structure. The alternative name field enforces its limit in its handler. The alternative repair moves field validation into a saved-format module.

The starting `.gitignore` controls ignored output. Open Forge cells can change their installed Open Forge files and work records under the runner's scope rules.

## Worker

Run `node checks/worker.ts INPUT_JSON` through the [worker contract](../../../../grading/brownfield/README.md). Each declared ID gets one result. Each check uses a fresh browser context. Missing HTML gives blocked results. Assertion failures give fail results. Checker faults give error results. Workers never import submitted Node source.

## Validation

Local validation applies every patch with `git apply --check` to fresh reference copies under `src/evals/.tmp/`. The starting project, `solution.patch` and `alt-solution.patch` build, pass their unit suites and pass all 94 public browser journeys. Every local worker outcome matches its registered expectation. Direct scope and protected-input checks pass on all three source trees. The case's `validation.json` records hashes, counts and evidence paths.

From the repository root, the overseer can validate this case with `npm run evals -- validate-case --case l4-p2-name-and-pasted-saves`. No models or sign-ins are needed. Docker validation was left to the overseer as requested.

Back to [Layer Four](../../../../../../.agents/memory/crystallized/documents/evals/neon-pairs/layer-4.md).
