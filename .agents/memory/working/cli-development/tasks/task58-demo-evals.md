---
open-forge:
  description: Open Task 58 to turn the demos into a repeatable evaluation comparing Open Forge with other setups, agents and models, including a multi-agent neon snake demo
  tags: [Memory, Working, Task, Evaluation, Demo, Contextual, Active]
---

# Task 58 — Demo-based evaluations

**Reviewed on 2026-09-28:** [review](../../../archived/cli-development/analysis/open-task-review/task58-demo-evals.md). Recommendation:
Do after 1.0. The review names any details in this record that are out of date.

## Outcome

Recorded at the maintainer's request on 2026-09-25. The demos from
[Task 57](../../../archived/cli-development/tasks/task57-onboarding-and-demos.md) become a repeatable evaluation: the
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

## Expansion, 2026-10-29

The maintainer asked to widen this Task and hand it to another agent. The goal is a cheap, repeatable evaluation whose results hold up to scrutiny: charts and tables that show what Open Forge changes, and a baseline to track models over time. This record still authorizes no model spending or published result until the maintainer approves the pilot.

### Neon snake demo

A new demo: a small neon snake browser game, built by several agents in parallel. It's small enough to finish cheaply, and it has enough parts that agents must agree on shared interfaces. Each part has one agent role:

- VFX - glow, trails and particles on a canvas.
- Sound design - Web Audio synthesis for moves, food and game over.
- Gameplay interactions - input, collision and food placement.
- Gameplay loop - fixed tick, game states, scoring and levels.
- UX - menus, HUD, pause, settings and accessibility.

Game logic must run headlessly in Node from a seed and an input script. Hidden checks can then replay fixed inputs and compare the score, length and end state exactly. VFX and UX can be checked with Playwright screenshots and DOM assertions.

### Setups

Every setup gets the same request and the same model:

1. Vanilla - the prompt only, no workspace context.
2. One `AGENTS.md` with workspace instructions.
3. Open Forge with a Vision, Architecture, module contracts, Directives and scenarios. Subagents are allowed.
4. Setup 3 plus orchestration through Task Coordination and the `use-workflow` Skill.
5. The same documents as setup 3, dropped into a plain `docs/` folder with no routing or links.
6. Proposed: setup 5 plus a carefully written hand-made `AGENTS.md` that links every document. This is the "good custom solution" baseline.

Setups 1 to 3 show how much context matters. Setups 3, 5 and 6 hold the content fixed and change only structure, so any difference comes from routing, loading and the Framework rules, not from extra information.

### Brownfield variant

The same comparison also runs on an existing build. Each setup starts from one finished, tested codebase and gets one new feature request:

1. Vanilla - the codebase and the request, with no workspace context.
2. Custom solution - the codebase with a hand-written `AGENTS.md` and its existing docs, as a team would set it up without Open Forge.
3. Open Forge - the codebase with its docs routed, its rules as Directives, its reasons as Decisions and a Map to its sources.
4. Hybrid - the custom solution plus a partial Open Forge adoption: the Minimal preset with its rules as Directives and accepted facts in Memory, existing docs added with `route include`, and the CLI for maintenance, without full specs or Decisions.

The hybrid setup shows whether a light adoption already gives most of the gain, which is how many teams will start. The codebase can be the finished neon snake or the expense splitter brownfield app. The new request touches rules that only the docs explain, so the hidden checks show whether each setup keeps them. This variant measures what most real work looks like: adding to a project instead of starting one.

### Measures

We record these per run:

- Hidden check pass rate, including the quiet failures.
- Contract violations between the agents' parts, such as mismatched event names or tick assumptions.
- Tokens in and out, cost, wall time, user turns and rework commits.
- Instruction adherence, from probes for rules that only the Directives state.
- Hallucination, from references to files, APIs or decisions that don't exist.
- Long-session drift: a session of sequential tasks where late tasks depend on early Decisions, scored for contradictions and forgotten rules.

### Rigor

These rules make the results hard to dispute:

- Write the protocol and rubric down, and get them accepted, before the first scored run.
- Pin agent, model and tool versions. Use fixed seeds and one starting commit per run.
- Use at least five runs per setup and model cell. Report the mean with a confidence interval, never one best run.
- Score with scripts kept outside the agent's workspace. Where people judge, they don't know the setup.
- Publish raw logs, prompts and the scoring code with the results, and state the limits.

### Order

1. Accept the protocol, setups and rubric.
2. Build hidden checks for the two expense splitter demos.
3. Build the neon snake specs and its headless checks.
4. Pilot setups 1, 3 and 5 on one model, then widen.
5. Run the brownfield variant on the expense splitter app, then on the finished neon snake.

## Current State

**Now:** recorded and expanded on 2026-10-29, not started. Ready to hand to an agent once the maintainer selects it. Tracked publicly in [issue #12](https://github.com/TheLithiumForge/open-forge/issues/12).

**Related:** the Emerging idea on
[documentation comprehension probes](../../../emerging/ideas/documentation-comprehension-probes.md)
proposes known-answer questions for unfamiliar agents, which could share this
harness. [Task 41](../../../archived/cli-development/tasks/task41-beta-journey-scenarios.md) owns the CLI's own user
journeys, which are a different kind of check.
