---
slug: /evals
title: Evals
---
# Evals
Open Forge Evals is a tool that gives an AI coding agent the same task under different setups, with and without Open Forge, then grades every result the same way.

For example, from a short brief, Claude Code builds Neon Pairs, a memory game, once with the documents behind a plain pointer and once with the same documents routed through Open Forge's Essentials preset. Checks and model judges then grade both games against the same requirements.

## Questions

The evals answer these questions, in this order:

1. Does Open Forge leave an agent at least as good as no setup, on the same model?
2. Which preset helps most, and what does it cost in tokens?
3. How do results change across models and CLI versions?

These are screening results. Every Open Forge CLI stays beta, and nothing here shows yet that Open Forge improves results.

## Starting points

A starting point is how much the agent is told before it starts: an idea, a short brief, a full spec or an existing app to change. The existing app has three larger cases, a feature, a repair and both together, and four small repairs that each need one part of the spec. [Starting points](starting-points.mdx) describes each one and the setups it compares, and [Scenarios](scenarios.mdx) lists what runs next.

## Setups

A setup is the project context the agent starts with. Six setups are rungs on the [setup ladder](../demos/index.md#setup-ladder), from No setup at rung 0 to Open Forge's Full Core preset at rung 5. Spec in prompt, Code only and Plain docs are delivery variants that move, remove or unlink the documents. Overloaded docs and Full Core, scoped, with memory add project notes beside the spec.

Each starting point measures its setups against a control: No setup for an idea and Organized docs for the others. [Setups](setups.mdx) defines every setup and how the runner builds it.

## Measures

Every run records check coverage, judge score, tokens and time. Coverage and judge score measure quality, and tokens are the main cost. Time mostly reflects the provider's speed, so it only compares setups of one model. [Measures](measures.mdx) defines each one and how checks and judges grade a run.

## How a run works

A run is one agent building or changing the game once under one setup. Grading is the checks and judges that score it, and it can run again later on the saved files.

1. The runner builds the setup in a fresh project with the pinned Open Forge CLI.
2. The agent, Claude Code or Codex CLI, works in its own container with the project and its sign-in, and nothing else from your machine.
3. When the agent asks a question, a simulated user answers from a frozen answer bank in a separate container. It gives no coding advice.
4. The runner saves the agent's files. A verifier container builds them offline, then hidden checks test the built game, most of them in a browser.
5. Judges read the saved code and screenshots in their own container. Each answers fixed yes, no or unsure questions in three ballots, citing files and lines.
6. Everything is saved as versioned records with the CLI, runner, model and grading versions.

Because grading only reads saved files, a fixed check or a new judge can grade old runs without running the agent again.

Each saved plan names one provider for its implementers, judges and simulated user. Claude implementation runs are paused for now, so the scoped Layer 4 plans name Codex, with Sol 6.1 implementing and Luna playing the simulated user. Older plans name Claude, and role flags such as `--implementers codex` replace them. `--implementers both --judges both` gives all four implementer and judge pairs. Each run is implemented once, and both judges read the same saved files, so the judges can be compared on identical work.

Failed runs and timeouts stay in results. An infrastructure failure gets one replacement, and both runs stay recorded. A run stopped by a usage limit or a rejected sign-in is redone when the batch resumes.

## Pages

- [Run evals](run.md) installs the tool and runs, follows and grades batches on your machine.
- [CLI reference](cli.md) lists every command and flag.
- [Results](results.mdx) shows published, latest, local or downloaded results, with each batch's progress, setup comparisons and every run's evidence.
- [History](history.mdx) compares CLI versions and the reference data point over time.

## Limits

Full plans run each setup three times, so differences are screening signals. They can't show a benefit or that a setup is no worse than its control. Different benchmark inputs, implementers, judges and grading versions stay in separate comparison groups. Missing values stay missing. Codex reports no dollar cost, and Claude Code's cost is its own estimate. Implementation usage comes from one run and isn't added again for each judge.
