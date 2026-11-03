# Move between cards with the arrow keys
B2 is a scoped task that brings back arrow-key movement, a Controls and input change.

You can move focus between cards with the arrow keys again, and movement stops at the board edges.

Status: waiting on product validation.

## Case

The seed removes arrow-key movement with its module, its unit tests and its browser test, and drops the card's movement handler from the face sheet under `art/`. The board keeps its remembered tab stop, so Tab still reaches one card. The input rules in the Controls and input scope, I06 to I08, define movement, edges, modifiers and the tab order. Other browser tests that press arrow keys stay and fail until movement returns.

## Checks

Seven clause checks take their results from the full-spec checker's I06, I07 and I08 assertions. I06.1 to I06.4 fail on the starting project. I06.9, I07.1 and I08.2 guard against a fix that opens cards, wraps at the edges or adds tab stops. Every other full-spec check runs as regression, and the checks that press arrow keys are expected to fail at the start.

## Scope

Game source, game tests, application tests under `tests/`, the face sheet under `art/` and `README.md` can change. Open Forge cells can change their installed files and work records under the runner's scope rules.

## Validation

From the repository root, run `npm run evals -- validate-case --case l4-b2-arrow-keys`. No models or sign-ins are needed.

Back to [Layer Four](../../../../../../.agents/memory/crystallized/documents/evals/neon-pairs/layer-4.md).
