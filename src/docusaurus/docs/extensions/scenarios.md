---
title: Flows and Scenarios
description: Four Templates for describing user journeys and expected outcomes, and recording what actually happened.
---

# Flows and Scenarios

Describe the experience you want people to have, then keep an honest record of what happens when it's tried. These starters fit planning, exploration, and verification.

- **Package ID:** `scenarios`
- **Depends on:** Nothing
- **Loads at startup:** Nothing. Templates are on demand.

## What it installs

```text
.agents/templates/scenarios/
  _scenarios.md               <- Template category entrypoint and rules
  user-flow.md                <- a journey made of scenarios
  scenario.md                 <- one situation and its expected result
  scenario-collection.md      <- a group of related scenarios
  run-record.md               <- what happened in one actual run
```

## What each file is for

### `_scenarios.md`

**Kind:** Template category entrypoint. It carries four rules that apply to every starter here:

- Specify expected results before recording actual results.
- Keep proposed expectations distinct from accepted requirements and observed behavior.
- Connect flow steps through their real resulting state. Don't silently reset starting conditions between steps.
- Keep missing inputs and unrun checks explicit.

### The Templates

| Template                 | Use it to                                                                                 | Main sections                                                                                        |
| ------------------------ | ----------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| `user-flow.md`           | Connect scenarios from a person's starting state to a meaningful goal                     | Who and goal, Starting point, Flow, Alternatives and recovery, Final result, Verification, Cost      |
| `scenario.md`            | Define one starting situation, the actions a person takes, and the result they should get | Who, Goal, Starting point, Steps, Expected result, Supporting contract, Verification, Open questions |
| `scenario-collection.md` | Group related scenarios without copying their expectations                                | Purpose, Shared context, Scenarios, Coverage boundaries                                              |
| `run-record.md`          | Record what happened during one run                                                       | Basis, Verified Starting State, Actions And Observations, Independent Checks, Verdict, Follow-Up     |

A **Scenario** can describe something that doesn't exist yet. A **Run Record** describes an actual attempt. Neither establishes acceptance on its own. A collection groups cases, but doesn't define an order. That's what a **User Flow** is for.

## How to use it

> Describe the expected experience as a flow with scenarios. After trying it, record the actual result separately.

> Write a run record for the onboarding flow we just tested.

## Good to know

- These records work for products, services, or other work. They don't need an executable plan, a task tracker, a test runner, or the Planning Extension.
- These four starters used to ship with Planning. Existing copies stay where they are, and moving the starters doesn't move or rewrite them.
