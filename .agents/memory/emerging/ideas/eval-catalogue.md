---
open-forge:
  description: The overseer's earlier twelve-eval plan from two Sol council rounds on 2026-10-30, kept to compare with the maintainer's Eval Ladder
  tags: [Memory, Idea, Contextual, Candidate, Evaluation, Greenfield, Brownfield, Demo, Council]
---

# Twelve-Eval Catalogue

The Twelve-Eval Catalogue is the overseer's earlier plan for Open Forge evals. It grew out of two council rounds with seven or eight Sol 6.1 xhigh members on 2026-10-30.

The maintainer set it aside the same day and asked us to keep it, so we can compare it with the [Eval Ladder](eval-ladder.md). It isn't accepted. [Task 58](../../working/cli-development/tasks/task58-demo-evals.md) tracks the work.

## Claims

The plan tests three claims in order:

1. Open Forge is better than nothing, even for a beginner with no setup.
2. It's better than a good hand-made setup with the same facts.
3. It's better than other tools, such as GitHub Spec Kit or BMAD Method. This is the main point, and it only convinces anyone after the first two hold.

## Building blocks

A run is one task, one setup, and one tool with one model:

- Tasks - a beginner build with a simulated user, a snake build from one prompt, ten changes to a finished snake, a change to an existing project, past changes in a large repository, tiny changes, and questions with known answers.
- Setups - nothing, a good hand-written `AGENTS.md` with docs, another tool's own files, Open Forge, and Open Forge variants (light install, team workflow, one part turned off).
- Tools - Claude Code and Codex first, any model.

## Fair comparison

Two runs are comparable only when everything except the setup is the same:

- The task, prompt, starting code, checks and budget.
- The tool, model and effort.
- The information. Either neither side has project facts, or both have the same facts, written once.
- The number of runs, paired one to one.

The result is the edge: Open Forge's result minus the other setup's result, with its range. We compare edges, never raw scores across tasks or models. Rerunning on new models shows whether the edge lasts.

## Evals

The catalogue had twelve evals:

1. First steps - a beginner's vague request and four follow-ups, empty folder against a default install.
2. Same request - one prompt, one agent, against a pasted spec, one `AGENTS.md` and custom docs.
3. How much you explain - three prompt levels, from one line to the full spec.
4. Other tools - evals 2 and 5 with other tools' own workflows.
5. Ten changes later - ten changes in fresh sessions, old rules kept.
6. Team build - the same build with two and with five agents.
7. Large repository - past changes in Open Forge's own repository.
8. Small existing project - the expense splitter's hidden rounding rule, with light and full adoption.
9. Real existing project - an outside project, adoption cost and break-even.
10. Find and explain - known-answer questions, no building.
11. Mechanism checks - one Open Forge part turned off at a time.
12. Small-task tax - tiny changes, where Open Forge should cost more.

## Data

Each run gives one record:

- Who - task, setup, tool, model, effort, version, date and run number.
- Result - checks passed in three groups (basic, project rules, old rules kept), whether it works, and what broke.
- Cost - input, cached and output tokens, dollars and time.
- User effort - the user's words and turns.
- Context - input before the first edit, and the largest single request.
- Why - which rule files the agent read, and the reason for each missed rule.

Each setup gets one record of its files, size and writing effort, counted once.

## Honesty rules

The council asked for these rules:

- Write down, before any run, which result would show each claim is wrong.
- Let an agent that never saw Open Forge write the hand-made setup and the project's unusual rules.
- Start every run with a fresh tool config. The council's own runs showed user-level instructions leaking in.
- Publish every run, and show ties and losses as clearly as wins.

## Cost

The council estimated about $1,650 of model spend for a full pilot on one tool and one model, and about $300 for its first phase. These were spending caps per run, not measured prices.
