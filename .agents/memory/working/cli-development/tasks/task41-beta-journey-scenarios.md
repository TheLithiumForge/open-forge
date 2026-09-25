---
open-forge:
  description: Open Task 41 to define the user journeys a beta must survive, run them by hand or by agent, and turn the findings into the polish list before release
  tags: [Memory, Working, CLI, Task, Scenario, Beta, Release, Contextual, Active]
---

# Task 41 — Beta journey scenarios

## Task state

- State: **Open. Scenarios written, not yet run.** Raised by the maintainer on
  2026-09-17 as the highest beta priority.
- Owner: Root.
- Trigger: the beta bar is that everything a user touches is polished and there
  are no stupid bugs. Nothing currently describes what a user actually *does*,
  so nothing tells us whether that bar is met.

## Why this exists

The end-to-end suite is green and gives almost no confidence, for two reasons
the maintainer named directly: there is **no observability** into what it
actually exercises, and **no one ever wrote down the scenarios** it was supposed
to protect. A passing suite that nobody can read is not evidence.

So the scenarios come first, as prose a person can judge. They are deliberately
written as *things a user does*, not as commands to run, because the beta
question is whether a person understands what happened — not whether a command
exited zero.

## Current Follow-Up

The maintainer's 2026-09-19 request is being carried by [Task 49](../../../archived/beta-preparation/extension-experience-review.md). Its [reviewed flow and scenario collection](../../../crystallized/documents/cli/experience/_experience.md) consolidates the supplied experience pack under the forgiving CLI direction. The older journey lists below remain prior backlog and provenance, not a second current specification. Validation of the new flow list precedes new test implementation.

## The scenarios

### Stage 5 execution instruction

On 2026-09-20 the maintainer said: “You may commit and start stage 5 after you
tell me at each jurney entails in simple human words like, user installs then ...
Then ... Expects ... Ty”. Astra explained F01–F26 as user actions and expected
outcomes, then started Task45 S5.01. The [plain-language list](../../../archived/cli-development/tasks/task45/journeys-in-plain-language.md)
and [source-bound roster](../../../archived/cli-development/tasks/task45/_task45.md) preserve that scope.
This opens preparation of the explained journeys; it does not automatically
adopt every source-pack case or settle missing exact expectations. Deferred
cases stay deferred. Executable packets must bind their exact cases, desired
outcomes and fixtures before dispatch. Stage6 has not been authorized by this
instruction.

### Stage 5 and 6 approval

The subsequent maintainer instruction is explicit: “All scenarios seem
appropriate, we might need even more but we can devise them later on. So for
now let's proceed with step 5 and 6. Ty”. It approves the current explained
F01–F26 set and its selected linked scenarios/alternatives for execution and
necessary behavior changes. [The approval manifest](../../../archived/cli-development/tasks/task45/_task45.md)
binds exact flow source hashes and scenario IDs. Earlier stage6-withheld
statements above are historical and superseded. Additional invented scenarios
and previously deferred source-pack permutations remain outside this wave.
Exact expectations and ownership still freeze before dispatch; G5 acceptance
precedes stage6 product changes.

### Prior journey lists

- [Journeys A–C](task41/journeys-a-c.md) — arriving, Extensions, authoring and indexing
- [Journeys D–F](task41/journeys-d-f.md) — links and repair, Libraries, everyday health
- [Journeys G–H](task41/journeys-g-h.md) — failure and recovery, output quality everywhere

## How this is meant to be run

Two passes, deliberately in this order.

**Now: run them wide, by agent.** Each scenario is independent and read-only
with respect to this repository, so many agents can run them in parallel in
scratch workspaces. The point of using agents rather than assertions is that an
agent can answer *"would a first-time reader understand this?"*, which an
assertion cannot. Their reports go through a reasoning pass to separate real
defects from taste.

**Later: turn the survivors into end-to-end tests.** Once a scenario's expected
behaviour is settled, it becomes reproducible. That is
[Task 45](../../../archived/cli-development/tasks/task45-end-to-end-observability.md), and it depends on this task
having defined the scenarios first.

The scenarios stay after the tests exist. They are the readable statement of
what the product promises; the tests are only the enforcement.

## Boundary

- **Observation only.** A runner does not fix anything, so a second run after
  the fixes can be compared against the first.
- Scratch workspaces only. **Never run `doctor` against this repository** — it
  produces about 258 MB of output here.
- A scenario that passes but reads badly is a **fail**. That is the whole point.

## Acceptance

- Every scenario has a verdict with its literal output recorded.
- Findings are sorted into: defects to fix before beta, wording to route to
  [Task 37](task37-wording-review-against-proposals.md), and accepted behaviour.
- The beta blocking list is short, specific, and each item names its scenario.
