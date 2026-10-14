---
title: Flows and Scenarios
description: Four Templates for describing user journeys and expected outcomes, and recording what actually happened.
---

# Flows and Scenarios

Describe the experience you want people to have, then record what actually happens when someone tries it. This package installs four [Templates](../glossary.md#template): copy-ready starting files that you copy, adapt, and then maintain on their own. They fit planning, exploration, and verification.

- **[Package ID](../glossary.md#package-id):** `scenarios`
- **Depends on:** Nothing
- **Startup visibility:** This package adds no entries to startup indexes. The Templates root [entrypoint](../glossary.md#entrypoint) stays on demand, so these starters do too.

These counts describe this package's own entries when their parent routes expose them in `Entries` and are read. Required packages add their own entries. Linked files stay on demand unless selected or required.

## Install

Package files follow your installation choices. Essentials leaves out Templates, so this package skips its starters and copies no package files. The CLI still records it as installed and names the excluded category in the result. If you want the starters, restore the Templates category before installing the package. Preview the restoration, review the plan, then apply:

```sh
open-forge route init templates --framework --dry-run
open-forge route init templates --framework
```

From your installed Open Forge workspace, preview the package and its required packages:

```sh
open-forge extension install scenarios --dry-run
```

Review the plan. Then apply it:

```sh
open-forge extension install scenarios
```

The CLI installs required packages from the same [catalogue](../glossary.md#catalogue). See the [Extension guide](/guides/extensions#install-an-extension) for scripts using `--automatic`, updates, removal, or a local source.

## Why it exists

When you write down what should happen before anyone tries it, a wrong result is easy to spot, whether you're designing an experience or checking a build. Keeping expected and observed results in separate records makes it hard for a run to quietly rewrite the requirement to match what happened.

These starters used to ship with Planning. They moved into their own package so experience design and verification can use them without a planning method. The [demos](../demos/index.md) apply the same idea: each demo's checks list an action and its expected result.

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
| `user-flow.md`           | Connect scenarios from a person's starting state to a meaningful goal                     | Who And Goal, Starting Point, Flow, Alternatives And Recovery, Final Result, Verification, Cost      |
| `scenario.md`            | Define one starting situation, the actions a person takes, and the result they should get | Who, Goal, Starting Point, Steps, Expected Result, Supporting Contract, Verification, Open Questions |
| `scenario-collection.md` | Group related scenarios without copying their expectations                                | Purpose, Shared Context, Scenarios, Coverage Boundaries                                              |
| `run-record.md`          | Record what happened during one run                                                       | Basis, Verified Starting State, Actions And Observations, Independent Checks, Verdict, Follow-Up     |

A **[Scenario](../glossary.md#scenario)** can describe something that doesn't exist yet. A **[Run Record](../glossary.md#run-record)** describes an actual attempt. Neither establishes acceptance on its own. A **[Scenario Collection](../glossary.md#scenario-collection)** groups cases but doesn't put them in order. A **[User Flow](../glossary.md#user-flow)** connects scenarios in order, passing each step's resulting state to the next.

## How to use it

> Describe the expected experience as a flow with scenarios. After trying it, record the actual result separately.

> Write a run record for the onboarding flow we just tested.

## Good to know

- These records work for products, services, or other work. They don't need an executable plan, a task tracker, a test runner, or the Planning Extension.
- Copies you made when these starters shipped with Planning stay where they are. Moving the starters to this package doesn't move or rewrite them. An existing installation needs a reviewed transition: see [moving from the earlier package layout](/guides/extensions#moving-from-the-earlier-package-layout).

Next: [Install this package](#install), or [update and remove Extensions](/guides/extensions#update-and-remove).
