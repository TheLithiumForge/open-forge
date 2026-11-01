# Open copied saved games
L4-R01 is a Layer 4 repair case that checks Neon Pairs changes against the frozen reference.

The starting project and known solution have different expected change results. The worker checks each result independently, so missing feature work doesn't stop repair evidence.

Status: worker contract validated locally. Product validation is pending.

## Checks

`checks/checks.json` declares 10 repair checks and keeps each starting and solution expectation. IDs use hyphens to match the runner parser. The previous dotted ID is kept in `previousId` for comparison with earlier records.

The private worker and all its relative imports are inside this case. Runtime dependencies are Node built-ins and Playwright. Shared probes are packaged in each case because the runner copies one case folder.

## Worker

The grader invokes `node checks/worker.ts INPUT_JSON`. The input names the project, built HTML, report path, declared checks and change type. See the [worker contract](../../../../grading/brownfield/README.md) for the complete input and report formats.

Each declared ID produces one outcome with nonempty evidence. A missing built page gives blocked outcomes. Assertion failures give fail outcomes. Checker faults give error outcomes. Every check gets a fresh browser context.

Patch application, building, regression grading, scope checks and protected-input checks use the product runner. This folder contains no local replacement for those operations.

## Validation

`validation.json` records fresh builds and detached worker CLI runs against the starting project and solution. Reports passed the product report parser. Missing-build runs returned blocked for every declared check. Docker validation through the product stays with the overseer.

The parent type check passes. Product tests have 182 passes and 6 existing failures outside these cases. Five concern stale Layer 2 fingerprints. One concerns reference-cache source scanning.

## Files

The case contains these files:

- `README.md`
- `answers.json`
- `case.json`
- `checks/browser.ts`
- `checks/check-state.ts`
- `checks/check.types.ts`
- `checks/checks.json`
- `checks/game.ts`
- `checks/repair.ts`
- `checks/worker-input.ts`
- `checks/worker-runtime.ts`
- `checks/worker.ts`
- `request.md`
- `seed.patch`
- `solution.patch`
- `validation.json`

Back to [Layer Four](../../../../../../.agents/memory/crystallized/documents/evals/neon-pairs/layer-4.md).
