# L4-R06 Fix Four Player Reports

L4-R06 plants the four scoped repairs at once, for scenario L4-3, which compares one agent with an orchestrator that hands each task to a subagent.

Status: passes case validation.

## Case

The seed is the four scoped tasks' seeds together: [Best score](../l4-r02/README.md), [Arrow keys](../l4-r03/README.md), [Reduced motion](../l4-r04/README.md) and [Silence](../l4-r05/README.md). They touch separate files, so they combine without overlap. The request lists the four symptoms and asks for them one at a time, with the project's tests run after each one. Both ways of working get the same request, and an orchestrated setup adds one registered line telling the orchestrator to delegate each task.

## Checks

Twenty-three clause checks, the four tasks' own, keep their IDs, so each task's coverage reads from its prefix: R02, R03, R04 and R05. Every other full-spec check runs as regression, and the regressions the four tasks fail at the start are expected to fail here too.

## Scope

Game source, game tests, application tests under `tests/`, the face sheet under `art/` and `README.md` can change. Open Forge cells can change their installed files and work records under the runner's scope rules.

## Validation

From the repository root, run `npm run evals -- validate-case --case l4-r06`. No models or sign-ins are needed.
