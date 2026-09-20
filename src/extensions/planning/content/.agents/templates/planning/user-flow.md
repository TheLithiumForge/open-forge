---
open-forge:
  description: Connect scenarios from a person's starting state to a meaningful goal
  tags: [Extension, Template, Planning, UserFlow]
---

# {User flow name}

{Copy and adapt. Remove this prompt and irrelevant sections. The result is independently maintained.}

## Who and goal

{Who is acting and what they want to achieve.}

## Starting point

{The initial reproducible state and inputs.}

## Flow

| Step | Action | Scenario | Resulting state passed forward |
| --- | --- | --- | --- |
| {1} | {Action or choice} | {Scenario link} | {What actually exists or is known before the next step} |

## Alternatives and recovery

{Meaningful choices, failure branches and safe continuations. Link scenarios instead
of repeating their expected output. Do not reset the workspace silently between steps.}

## Final result

{The accomplished goal and content that must remain intact.}

## Verification

{How to observe the whole flow, including effects left by an interrupted or failed
step. Keep actual runs and their evidence separate from this specification.}

## Cost

{Optional. For frequently repeated work, record the meaningful time, steps, or effort the person spends. Distinguish measured cost from a target.}
