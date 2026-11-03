# Keep the best score right
L4-R02 is a scoped task that repairs the best score, a Gameplay change.

A worse game no longer replaces your best score, and a size you haven't finished shows `Best: none`.

Status: waiting on product validation.

## Case

The seed lets every finished game replace the saved best and shows 0 for a size without a record. The rules in the Gameplay scope, R10, define the right behavior. The request describes the symptoms and names no cause. The case changes no product clause.

## Checks

Seven clause checks take their results from the full-spec checker's R10 assertions, so the case needs no worker of its own. All seven fail on the starting project. Every other full-spec check runs as regression. The best-score check, the project's unit tests and the capped-best persistence check are expected to fail at the start.

## Scope

Game source, game tests, application tests under `tests/` and `README.md` can change. Open Forge cells can change their installed files and work records under the runner's scope rules.

## Validation

From the repository root, run `npm run evals -- validate-case --case l4-r02`. It builds the starting project and the solution in the verifier and compares every check with its expected result. No models or sign-ins are needed.

Back to [Layer Four](../../../../../../.agents/memory/crystallized/documents/evals/neon-pairs/layer-4.md).
