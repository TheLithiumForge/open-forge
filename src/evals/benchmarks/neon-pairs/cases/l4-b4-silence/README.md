# Silence the game again
B4 is a scoped task that removes a sound the game shouldn't make, a Sound change.

The game stays silent through every action, as the spec requires.

Status: waiting on product validation.

## Case

The seed adds a short Web Audio tone on each match and a higher one on the win. The Sound scope, S01 and S02, requires silence with no audio files, audio API calls or sound toggle. The answer bank refuses a toggle or volume setting.

## Checks

Four clause checks take their results from the full-spec checker's S01 and S02 assertions. The audio checks fail on the starting project. The audio file and toggle checks guard against a fix that swaps the tone for a file or adds a mute switch. Every other full-spec check runs as regression. The project's unit tests are expected to fail at the start, because one of them forbids audio in the shipped source.

## Scope

Game source, game tests, application tests under `tests/` and `README.md` can change. Open Forge cells can change their installed files and work records under the runner's scope rules.

## Validation

From the repository root, run `npm run evals -- validate-case --case l4-b4-silence`. No models or sign-ins are needed.

Back to [Layer Four](../../../../../../.agents/memory/crystallized/documents/evals/neon-pairs/layer-4.md).
