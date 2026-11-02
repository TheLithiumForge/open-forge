---
slug: /evals
title: Evals
---
# Evals
Open Forge Evals is a tool that gives the same coding task to an AI coding agent under different setups, with and without Open Forge, then grades every result the same way.

For example, Layer 2 asks Claude Code or Codex to build Neon Pairs, a memory game, from a short set of player-facing documents. One run gets the documents with a plain pointer, another gets them through Open Forge's Essentials preset. Browser checks and model judges then grade both games against the same requirements.

## Questions

The evals answer these questions, in this order:

1. Does Open Forge leave an agent at least as good as no setup, on the same model?
2. Which preset helps most, and what does it cost in tokens? Time is recorded too, but it mostly reflects the provider's speed, so it only compares setups of one model.
3. How do results change across models and CLI versions?

These are screening results. Every Open Forge CLI stays beta, and nothing here is a claim yet.

## Layers

Each layer gives the agent more to work from and checks more:

| Layer | The agent receives | Setups compared |
| --- | --- | --- |
| 1, an idea | One short prompt | No setup, Minimal, Essentials, Full Core |
| 2, a defined idea | Player-facing documents, a subset of the full spec | Plain docs, Linked AGENTS.md, Spec in prompt, Minimal, Essentials, Full Core |
| 3, a full spec | The full Neon Pairs spec | Plain docs, Linked AGENTS.md, Minimal, Essentials, Full Core |
| 4, brownfield | The reviewed reference game, its spec and a change request | Code only, Plain docs, Linked AGENTS.md, Minimal, Essentials, Full Core |

Layer 4 has three cases: a feature (`l4-f01`), a repair (`l4-r01`) and both together (`l4-fr01`). Each starts from the reference solution to the full spec, never from an agent's Layer 3 game.

## Setups

| Setup | What the agent gets |
| --- | --- |
| No setup | The prompt alone. The control on Layer 1 |
| Plain docs | The documents and a neutral pointer to them. The control on later layers |
| Linked AGENTS.md | A root `AGENTS.md` that links the same documents |
| Spec in prompt | The same requirements pasted into the prompt |
| Code only | Layer 4 only: the code, its tests and the request, without the documents |
| Minimal, Essentials, Full Core | The same documents, installed with that Open Forge preset |

The control is the first setup of each plan. The [Results](results.mdx) page compares every other setup with it.

## How a run works

An implementation run is one agent building one setup once. Grading is the checks and judges that score it, and it can run again later on the saved files.

1. The runner installs the setup into a fresh project with the pinned Open Forge CLI.
2. The agent, Claude Code or Codex CLI, works in its own container with the project and its sign-in, and nothing else from your machine.
3. When the agent asks a question, a simulated user answers from a frozen answer bank in a separate container. It gives no coding advice.
4. The runner saves the agent's files. A verifier container builds them offline, then runs hidden Playwright checks against the built game.
5. Judges read the saved code and screenshots in an assessor container. Each answers fixed yes, no or unsure questions in three ballots, citing files and lines.
6. Everything is saved as versioned records with the CLI, runner, model and grading versions.

Because grading only reads saved files, a fixed check or a new judge can grade old runs without running the agent again.

## Grading

Checks drive the built game in a browser and test behaviour that every correct build must show, such as flipping two cards and matching a pair. Layer 1 checks only what the prompt asks for. Layers 2 and 3 check each requirement the agent was given, and the full spec's board codes make deals repeatable. Layer 4 checks the requested change and that nothing else broke.

Every check returns `pass`, `fail`, `error` or `blocked` with evidence:

- **Coverage** is checks passed among the applicable checks. A blocked check counts as not passed. A grader error is checked again on the saved files, and an unresolved one shows in the error count.
- **Working gate** passes when every working check passes. **Stated success** passes when every check of the stated request passes.
- **Judge yes share** is yes ballots among the judge's ballots for one area: rules, code or wishes. Unsure ballots are counted and shown apart.

Two implementers, Claude Opus and Codex Sol, and two judges, Claude Opus and Codex Sol, give four implementer and judge pairs. Each run is implemented once, and both judges read the same saved files, so the judges can be compared on identical work.

Failed runs and timeouts stay in results. An infrastructure failure gets one replacement, and both runs stay recorded. A run stopped by a usage limit or a rejected sign-in is redone when the batch resumes.

## Pages

- [Run evals](run.md) installs the tool and runs, follows and grades batches on your machine.
- [CLI reference](cli.md) lists every command and flag.
- [Results](results.mdx) shows published, latest, local or downloaded results, with each batch's progress, setup comparisons and every run's evidence.
- [History](history.mdx) compares CLI versions and the reference data point over time.

## Limits

Full plans run three times per setup, so differences are screening signals. They can't establish a benefit or non-inferiority. Different benchmark inputs, implementers, judges and grading versions stay in separate comparison groups. Missing values stay missing. Codex reports no dollar cost, and Claude Code's cost is its own estimate. Implementation usage comes from one run and isn't added again for each judge.
