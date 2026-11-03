# Respect reduced motion
B3 is a scoped task that makes the game respect reduced motion, a Visuals change.

With reduced motion on, cards change state without a transition and the win banner appears static.

Status: waiting on product validation.

## Case

The seed removes the two `prefers-reduced-motion` style rules. Normal motion still works. The Visuals scope, V17 and V18, defines both the motion and its reduced form.

## Checks

Five clause checks take their results from the full-spec checker's V17 and V18 assertions. V17.2 and V18.3 fail on the starting project. V17.1, V18.1 and V18.2 guard the normal motion, so a fix that removes all motion fails. Every other full-spec check runs as regression. The Layer 2 completion check and the project's unit tests are expected to fail at the start.

## Scope

Game source, game tests, application tests under `tests/` and `README.md` can change. Open Forge cells can change their installed files and work records under the runner's scope rules.

## Validation

From the repository root, run `npm run evals -- validate-case --case l4-b3-reduced-motion`. No models or sign-ins are needed.

Back to [Layer Four](../../../../../../.agents/memory/crystallized/documents/evals/neon-pairs/layer-4.md).
