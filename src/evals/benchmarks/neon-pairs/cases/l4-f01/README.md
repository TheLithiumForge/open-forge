# Name a saved deal
L4-F01 is a feature case that adds a Deal name field and keeps the name with exported progress.

You can name a deal, play a mismatch, export it and load that text after New Game to restore its name and progress.

Status: all local worker expectations pass for the starting project, solution and alternative solution. Full product validation is pending.

## Case

The version 3 delta replaces the overview, Architecture, Controls and Reproducibility documents. C15.1 through C15.5 follow C14. Named snapshots use version 2. Unnamed snapshots, board codes and replay output keep version 1.

The runner creates one starting commit after applying the seed and product delta. It keeps the clean reference and private patches outside the agent project. The starting change result is one pass and nine feature failures.

## Checks

The manifest declares ten feature checks. Each outcome keeps its own evidence.

The feature probes cover the named-save journey, UTF-16 input limits, name clearing, paused and completed editing, reload, preferences and invalid version 2 states.

## Scope

The game source under `src/game/`, application tests under `tests/` and `README.md` can change. The patterns in `case.json` describe behavior areas. Both solutions exercise them with different module names and structure. The alternative name field enforces its limit in its handler.

The starting `.gitignore` controls ignored output. Open Forge cells can change their installed Open Forge files and work records under the runner's scope rules.

## Worker

Run `node checks/worker.ts INPUT_JSON` through the [worker contract](../../../../grading/brownfield/README.md). Each declared ID gets one result. Each check uses a fresh browser context. Missing HTML gives blocked results. Assertion failures give fail results. Checker faults give error results. Workers never import submitted Node source.

## Validation

Local validation builds the starting project, `solution.patch` and `alt-solution.patch` from scratch copies under `src/evals/.tmp/`. Public unit tests pass on all three trees. Both solutions pass every case worker check and all 65 public browser journeys. The product patch parser accepts both solution patches. Direct scope and protected-input checks pass on all three source trees. The case's `validation.json` records the results.

The product typecheck passes. Its test run has 199 passes and 29 external failures. The [packet record](../l4-f01/packet-validation.json) lists every failure and every changed file. The shared requirements loader still expects spec version 2, while the inputs use 2.1. Stale Layer 2 input records also block product tests. Full frozen regression and Docker validation need those shared fixes.

From the repository root, the overseer can validate this case with `npm run evals -- validate-case --case l4-f01`. No models or sign-ins are needed. Docker validation was left to the overseer as requested.

Back to [Layer Four](../../../../../../.agents/memory/crystallized/documents/evals/neon-pairs/layer-4.md).
