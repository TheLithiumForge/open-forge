---
open-forge:
  description: Idea to turn the Adaptive Collaboration Guidance into a plan-ahead Skill if the Task 58 evals show the Guidance doesn't change how agents plan or ask
  tags: [Memory, Idea, Contextual, Candidate, Extension, Collaboration, Guidance, Skill, Evaluation]
---

# Plan-Ahead Skill

The Plan-Ahead Skill idea turns the Collaboration Extension's Adaptive Collaboration Guidance into a Skill an agent invokes before it starts building.

The Guidance is meant to make an agent plan ahead and ask about choices that matter. In the first Task 58 Baseline batch, no agent asked a question in 30 runs, the five Helpful runs included, even though the prompt said "If you have any questions, ask me". Guidance loads only when an agent judges it relevant, so on a short build request it may never shape the plan. A Skill is invoked as a step, so it could make planning happen instead of hoping it does.

Recorded on 2026-11-01 at the maintainer's request. It depends on eval evidence and isn't accepted. The same day the maintainer moved Extension testing, the Helpful setup included, to a later part of Task 58, so this waits for those evals.

## When to act

The maintainer set the rule: when the Helpful setup is installed and the agent asks nothing and states no assumptions, that counts against Open Forge. If the Helpful setup keeps scoring no better than Essentials on asking, on stated assumptions and on hidden wishes, convert the Guidance:

- A Skill such as `plan-ahead` that lists the open choices, recommends defaults, asks only about choices that change the result, and states the rest as assumptions.
- The Guidance keeps the reasoning, and the Skill links to it.

## Not decided yet

- Whether the Skill belongs in the Collaboration Extension or in Core.
- Which eval result counts as "no better", once the Task 58 grading is decided.

Related: [Task 58](../../working/cli-development/tasks/task58-demo-evals.md) and [Task 90](../../working/cli-development/tasks/task90-extensions-install-their-routes.md).
