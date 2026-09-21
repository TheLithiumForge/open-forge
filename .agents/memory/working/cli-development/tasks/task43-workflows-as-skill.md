---
open-forge:
  description: Open Task 43 to narrow Workflows and express them through the skill mechanism, so they stop overlapping with Skills and can be selected when they become relevant
  tags: [Memory, Working, CLI, Task, Workflows, Skills, Framework, Beta, Contextual, Active]
---

# Task 43 — Workflows as a skill

## Task state

- State: **Native Skill extraction and documentation aligned; historical upgrade evidence remains unqualified.** Raised by the maintainer on 2026-09-17.
- Owner: Root.

The 2026-09-21 [documentation packet](beta-follow-ups/task42-43-documentation.md) maps the shipped native Skill, catalogue and template and specifies stale-contract repairs. The earlier problem statement below is historical context. No additional routing behavior is approved by this update.

## The problem

Workflows and Skills overlap, and the overlap confuses rather than helps. As
they stand, Workflows are close to manually invoked commands. The only real
difference from a Skill is that **a Skill can be selected when it becomes
relevant, while a Workflow has to be deliberately reached for.**

That is a loading difference, not a difference in kind. Two primitives are
carrying one idea.

## The accepted direction

Express Workflows **through** the skill mechanism rather than beside it: one
Workflows skill, inside which a user defines their own workflows as references.
An agent then learns that workflows exist and decides whether any apply, instead
of a workflow staying invisible until someone remembers it.

This keeps what is good about a Workflow, a strict repeatable recipe, and gives
it the one thing it lacked, which is being noticed at the right moment.

Narrowing the scope of a Workflow is part of this. They are currently broad
enough to read as general instructions. A Workflow should be a defined goal with
a repeatable path to it.

## Open questions

- What shape does a user-authored workflow take inside the skill: a reference, a
  file, an entry? The answer decides how a user adds one.
- Does the `workflows` root route remain, become an Extension route, or go? This
  interacts directly with [Task 42](task42-minimal-core.md), so settle them
  together rather than in sequence.
- What happens to workflows a workspace already has.

## Acceptance

- A reader can say in one sentence when to write a Skill and when to write a
  Workflow, and the two no longer overlap.
- A user can add their own workflow without editing anything the tool manages.
- The loader's account of both primitives is updated, and the shipped payload
  matches it.
