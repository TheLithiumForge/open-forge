---
open-forge:
  description: Connect scenarios from a person's starting state to a meaningful goal
  tags: [Extension, Template, UserFlow]
---

# {User Flow Name}

{Copy and adapt. Keep the goal, state transitions, and whole-flow checks clear. Replace metadata and prompts, rebase links, and remove this guidance and unused optional sections.}

## Who And Goal

{Who is acting and what they want to achieve.}

## Starting Point

{The initial reproducible state and inputs.}

## Flow

| Step | Action             | Scenario        | Resulting state passed forward                          |
| ---- | ------------------ | --------------- | ------------------------------------------------------- |
| {1}  | {Action or choice} | {Scenario link} | {What actually exists or is known before the next step} |

## Alternatives And Recovery

{OPTIONAL: Meaningful choices, failure branches, and safe continuations. Link scenarios instead of repeating their expected output.}

## Final Result

{The accomplished goal and content that must remain intact.}

## Verification

{How to observe the whole flow, including effects left by an interrupted or failed
step. Keep actual runs and their evidence separate from this specification.}

## Cost

{OPTIONAL: For frequently repeated work, record meaningful time, steps, or effort. Distinguish measured cost from a target.}
