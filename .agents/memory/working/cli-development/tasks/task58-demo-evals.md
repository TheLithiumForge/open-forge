---
open-forge:
  description: Open Task 58 to turn the demos into a repeatable evaluation comparing Open Forge with other setups, agents, and models
  tags: [Memory, Working, Task, Evaluation, Demo, Contextual, Active]
---

# Task 58 — Demo-based evaluations

## Outcome

Recorded at the maintainer's request on 2026-09-25. The demos from
[Task 57](task57-onboarding-and-demos.md) become a repeatable evaluation: the
same fixed inputs, run under different setups, scored the same way, with
results that can be reproduced and published honestly.

**Direction:** the maintainer asked for a task, not implementation. This record
authorizes no harness, no model or service spending, and no published result.

**Questions the evaluation should answer:**

- Does Open Forge change outcomes compared with no framework, on the same agent
  and model?
- How does it compare with other spec-driven setups on the same inputs?
- How do results vary across agents, models, and the four seed levels?
- In brownfield work, do adopted Decisions carry rules that the request never
  mentions?

**In scope:**

- A protocol: clean workspaces, pinned tool and model versions, fixed seeds,
  several runs per setup, and one starting commit per run.
- Executable checks, kept out of the agent's workspace, for each demo's
  `checks.md`: expected results, the quiet failures such as floating point
  money and the payer rounding rule, and whether existing tests still pass.
- Knowledge checks for Open Forge runs: whether the reasons were recorded as
  Decisions and whether anything was accepted without the user.
- Cost and effort measures: tokens, wall time, number of user turns, and diff
  size.
- A results format that states its limits, and a way to rerun it.

**Preserve / out of scope:** the demos stay usable by hand. Claims of measured
superiority wait for results and their limits.

**Done when:**

- [ ] The protocol and scoring rubric are written and accepted.
- [ ] Hidden executable checks exist for both expense splitter demos.
- [ ] A pilot run covers at least two setups, and its results are recorded with
      their limits.
- [ ] The maintainer decides whether and how results are published.

## Plan

1. **Define the protocol and rubric**, including what counts as a pass for each
   seed level.
2. **Build the hidden checks**, starting with the brownfield demo, because its
   quiet failures are the clearest signal.
3. **Automate one setup end to end**, from a clean copy to a scored result.
4. **Pilot** against a baseline without Open Forge, then widen.

## Current State

**Now:** recorded, not started.

**Related:** the Emerging idea on
[documentation comprehension probes](../../../emerging/ideas/documentation-comprehension-probes.md)
proposes known-answer questions for unfamiliar agents, which could share this
harness. [Task 41](task41-beta-journey-scenarios.md) owns the CLI's own user
journeys, which are a different kind of check.
