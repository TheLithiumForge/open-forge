# Open copied saves
L4-R01 is a repair case that repairs copied snapshots that fail while local exports still load.

You can paste a saved game from another tool and recover its progress without losing the current game when the draft is invalid.

Status: all local worker expectations pass for the starting project, solution and alternative solution. Full product validation is pending.

## Case

The seed replaces the snapshot key validator with an order-sensitive comparison. Public tests and app exports still work. The case changes no product clause. Its answer bank gives a failing snapshot when asked for an example and names no cause.

The runner creates one starting commit after applying the seed and product delta. It keeps the clean reference and private patches outside the agent project. The starting change result is seven passes and three repair failures.

## Checks

The manifest declares ten repair checks. Each outcome keeps its own evidence.

Repair probes use published best-score text, card names, ignored activation while paused and matching live-region text. Top-level member-order variants count as one result. Nested selection order, invalid fields, inconsistent progress, pause and completed loads test different behavior. CRLF drafts pass through the native textarea, which changes line endings to LF.

## Scope

The snapshot and saved-format areas, game tests, application tests under `tests/` and `README.md` can change. The scope includes new local modules and different test names. The patterns in `case.json` describe behavior areas. Both solutions exercise them with different module names and structure. The alternative repair moves field validation into a saved-format module.

The starting `.gitignore` controls ignored output. Open Forge cells can change their installed Open Forge files and work records under the runner's scope rules.

## Worker

Run `node checks/worker.ts INPUT_JSON` through the [worker contract](../../../../grading/brownfield/README.md). Each declared ID gets one result. Each check uses a fresh browser context. Missing HTML gives blocked results. Assertion failures give fail results. Checker faults give error results. Workers never import submitted Node source.

## Validation

Local validation builds the starting project, `solution.patch` and `alt-solution.patch` from scratch copies under `src/evals/.tmp/`. Public unit tests pass on all three trees. Both solutions pass every case worker check and all 65 public browser journeys. The product patch parser accepts both solution patches. Direct scope and protected-input checks pass on all three source trees. The case's `validation.json` records the results.

The product typecheck passes. Its test run has 199 passes and 29 external failures. The [packet record](../l4-f01/packet-validation.json) lists every failure and every changed file. The shared requirements loader still expects spec version 2, while the inputs use 2.1. Stale Layer 2 input records also block product tests. Full frozen regression and Docker validation need those shared fixes.

From the repository root, the overseer can validate this case with `npm run evals -- validate-case --case l4-r01`. No models or sign-ins are needed. Docker validation was left to the overseer as requested.

Back to [Layer Four](../../../../../../.agents/memory/crystallized/documents/evals/neon-pairs/layer-4.md).
