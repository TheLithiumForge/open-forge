---
open-forge:
  description: "Historical record: Review of Task 58 Demo-based evaluations, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation"
  tags: [Memory, Analysis, TaskReview, Contextual, Archived, Historical]
---

# Task 58 Demo-Based Evaluations Review

## Archive Status

Archived on 2026-10-04 from `.agents/memory/emerging/analysis/open-task-review/task58-demo-evals.md` after the maintainer selected Memory cleanup. Superseded analytical baseline; preserve original bodies and current residue owners.

This record preserves historical evidence. The [current CLI development route](../../../../working/cli-development/_cli-development.md) and its open Tasks define current work. Local qualification in this record does not establish later integration or publication.

## Question

Is Task 58 still worth doing, what exactly remains, and when should it run?

**Task record:** [Task 58](../../../../working/cli-development/tasks/task58-demo-evals.md)

## Current Conclusion

**Recommendation:** Do after 1.0.

**Size:** Large, because a fair comparison needs a harness, hidden checks for two demos, several setups, and repeated paid model runs. A first pilot is Medium.

The Task is worth doing. Honest Reliability in the [Principles](../../../../crystallized/documents/principles.md) says reliability claims require evidence, and the demos already hold the fixed inputs. It doesn't change what a 1.0 user sees, though, and it needs a spending decision first. Run a narrow brownfield pilot after 1.0. Run it earlier only if the maintainer wants evidence for a 1.0 decision in Tasks 53, 60, or 62.

## What It Implies

Users would get reproducible results with stated limits, in place of comparing runs by hand. The Framework gains evidence about which parts change outcomes, such as Maps, Decisions, and startup loading. Maintenance gains a harness and hidden checks that must track every demo change, plus model costs whenever results are refreshed.

## State Today

The record is accurate: nothing has started. Verified in the repository:

- The [demos](../../../../../../demos/README.md) exist under `demos/expense-splitter/`, with greenfield and brownfield variants, four seed levels, and a manual `checks.md` for each. Brownfield also ships `app/`, runnable TypeScript with `node --test`, and a `reference/` folder.
- No harness, script, or hidden check exists. A search for evaluation files found only this Task and an older flow observation.
- The public docs make no measured-outcome claim. The site's demo page calls the evaluation planned work.
- The brownfield checks are already a command table with exact outputs, so they automate easily.
- Greenfield fixes no command syntax at any seed level. Level 3 gives expected results, not an interface. Executable greenfield checks therefore need a per-run adapter or a grader.

## Dependencies

- **Blocked by:** a maintainer decision on budget, models, agents, and whether results may be published. Nothing technical blocks it.
- **Blocks:** nothing formally. Any future claim that Open Forge improves outcomes should wait for it.
- **Overlaps with:** the [documentation comprehension probes](../../../../emerging/ideas/documentation-comprehension-probes.md) idea, which could share the harness. [Task 62](../../../../working/cli-development/tasks/task62-glob-scoped-loading.md) names these demos as the way to test whether agents follow glob triggers. Task 60's loader question needs evidence about how harnesses activate Skills. Task 41 covers CLI journeys, a different kind of check.

## Remaining Work

1. Write the protocol and rubric: setups, pinned tool and model versions, runs per cell, and what counts as a pass at each seed level.
2. Build the brownfield hidden checks as a script outside the agent's workspace. It runs the `checks.md` command table against a fresh `EXPENSES_FILE`, runs `npm test`, and searches new code for `parseFloat` and `toFixed`.
3. Decide how greenfield is scored. Either add a fixed command contract to seed levels 2 and 3, which changes the demo, or accept a graded adapter.
4. Write the knowledge rubric for Open Forge runs: a Decision recorded with reasons, still proposed until the user accepts it, the README updated, and a Map present.
5. Automate one setup end to end: clean copy, starting commit, install, seed, transcript, tokens, wall time, and diff size.
6. Pilot brownfield levels 1 and 2 on one agent and model, with and without Open Forge, over several runs. Record the limits.
7. The maintainer decides publication.

## Pros And Cons

| Pros                                                                    | Cons                                                                  |
| ----------------------------------------------------------------------- | --------------------------------------------------------------------- |
| Turns the product promise into evidence, as Honest Reliability requires | Paid model runs, repeated for every refresh                           |
| The inputs already exist, and brownfield checks are nearly executable   | Small samples from nondeterministic agents support only weak claims   |
| Can settle Task 62's assumption and Task 60's loader question with data | Fair comparison with other spec-driven setups needs expertise in each |
| Reusable for comprehension probes and later demos                       | A harness is one more thing to maintain                               |

## Risks And Open Questions

- **Contamination:** the demos, checks, and reference records are public in this repository. A web-enabled agent could find the answers. The hidden checks and any new seeds need to live elsewhere.
- **Level 4 isn't neutral:** its seeds are Open Forge records, so a no-framework run at level 4 measures something different.
- **Scripted setup steps:** the brownfield Map step is a manual prompt today. It must be identical in every run.
- **Knowledge grading:** knowledge checks need a human or model grader with a fixed rubric. That grader is itself a source of variance.
- The maintainer must choose the setups to compare and whether negative results are published too.

## Next Check

**Action:** write the brownfield hidden-check script and run it against the untouched app. Existing rows should pass and the percentage rows should fail, which validates the checks before any model spending.

**Would change the conclusion:** the maintainer wanting evidence before 1.0 for the Task 53, 60, or 62 decisions, which moves the pilot earlier.

**Acceptance needed:** the maintainer, for the protocol, spending, and publication.
