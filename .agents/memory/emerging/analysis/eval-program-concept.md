---
open-forge:
  description: The maintainer's November 2026 concept for the next evals, with Layer 4 first, a four-step setup ladder, milestone tasks, additive task chains and one agent against an orchestrator with subagents
  tags: [Memory, Analysis, Contextual, Candidate, Evaluation, Concept, Brownfield, Orchestration, Milestones, Tokens, Scopes, Proposal]
---

# Eval Program Concept

## What should the next evals test, and in what order?

Status: maintainer direction from 2026-11-03 for [Task 58](../../working/cli-development/tasks/task58-demo-evals.md). A planning pass turns it into concrete scenarios. No eval starts until the maintainer approves those scenarios and the improvements are committed.

The current base works. The recent cycle of fixing one thing and breaking another went in circles, so the next set is planned first and then run tactically.

## Order

The starting points, from the top, are an idea, a defined idea where the user knows what they want, a fully specified idea and an existing implementation.

1. **Layer 4, an existing implementation.** It takes the least time and gives the most meaningful results.
2. **Layer 3, the fully specified idea.** The second most important.
3. **Layers 2 and 1, the defined idea and the idea.** Helpful, but secondary.

## Setup ladder

Each step adds one thing to the step before, so each comparison isolates one cause.

1. **Code only.** The absolute baseline.
2. **Code and linked specs.** The spec documents, with links between them.
3. **Open Forge, specs one to one.** The specs moved in unchanged, with `open-forge index` run.
4. **Open Forge, scoped.** The specs routed by scope, so an agent loads only what a task needs.

## Milestones

Agents have limited context windows. Each scenario splits its work into milestones or tasks, implemented one after another with tests between them. The evals can then grade progress along the way and not only the end state.

## Additive tasks

Evals build on each other to spend fewer tokens. On Layer 4, a first variant implements one fix or task. A second variant implements another one on top of it. Later steps reuse the state of earlier ones instead of starting over.

## Orchestration

Four ways of working, each run in isolation:

- One agent.
- One agent with Open Forge.
- An orchestrator with subagents, without Open Forge.
- An orchestrator using Open Forge properly, with subagents.

The orchestrator receives the tasks, plans them, splits the plan into smaller tasks and hands each to a subagent. Open Forge's orchestration extensions could drive this later.

## Measures

- Tokens for the whole system, the orchestrator and its subagents, against one agent.
- How thoroughly the spec was implemented.
- Context composition, such as how much went to documents and how much to exploring code.

## Next steps

1. A planning agent turns this concept into scenarios and plans the cleanup the current data structures and run process need.
2. The maintainer reviews the scenarios.
3. Layer 4 runs first, then Layer 3, then Layers 2 and 1.

Related: [Eval Context And Cost Measures](eval-context-and-cost-measures.md), [Layer 4](../../crystallized/documents/evals/neon-pairs/layer-4.md).
