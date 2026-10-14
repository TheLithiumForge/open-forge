---
title: Planning
description: A planning workflow, the Work Records Pattern, four Memory categories, and seven Templates for tasks, plans, and decisions.
---

# Planning

Turn an outcome into work that can be understood, carried out, and resumed. Keep your existing boards and task systems, and create only the records that add useful context.

- **[Package ID](../glossary.md#package-id):** `planning`
- **Depends on:** [Workflow Support](workflows.md)
- **Startup visibility:** This package adds five entry lines: four Memory categories and the Work Records Pattern. Checkpoints appears in Working, Ideas and Analysis in Emerging, Decisions in Crystallized, and Work Records in Patterns. Workflow Support also adds the `use-workflow` entry in Skills. Linked files open when selected or required. Recipes and Templates stay on demand.

These counts describe this package's own entries when their parent routes expose them in `Entries` and are read. Required packages add their own entries. Linked files stay on demand unless selected or required.

## Install

Package files follow your installation choices. Essentials leaves out Templates, so Planning skips its starters and installs only its other enabled content. If you want the starters, restore the Templates category first. Preview the restoration, review the plan, then apply:

```sh
open-forge route init templates --framework --dry-run
open-forge route init templates --framework
```

From your installed Open Forge workspace, preview the package and its required packages:

```sh
open-forge extension install planning --dry-run
```

Review the plan. Then apply it:

```sh
open-forge extension install planning
```

The CLI installs required packages from the same [catalogue](../glossary.md#catalogue). See the [Extension guide](/guides/extensions#install-an-extension) for scripts using `--automatic`, updates, removal, or a local source.

If Planning is already installed without its starters, follow the result's guidance to restore Templates, then preview and apply the [package update](index.md#if-your-setup-leaves-out-templates). Plans and receipts name the skipped starters for Planning and Workflow Support.

:::tip[Start with Decisions]

Decisions preserve why something was chosen, which matters most when a codebase outlives the people and conversations behind it. See [Highlights](../highlights.md#decisions-keep-the-why).

:::

## Why it exists

When work spans days, people, or agents, two things are easy to lose: where things stand, and why they are the way they are. Checkpoints keep the first and Decisions keep the second. Ideas and Analysis hold possibilities and reasoning without presenting them as commitments. A Decision records the change, and the sources it affects, such as Documents from [Project Documents](project-documents.md), are meant to be updated to say what's true now, as [the document flow](document-flow.md) explains.

The Work Records Pattern gives each fact one home, which helps keep a task, a plan, and a checkpoint from drifting into three competing versions of the truth. None of it replaces your issue tracker. When you already have a task source, Planning points the agent to it.

## What it installs

```text
.agents/
  patterns/
    work-records.md                          <- Pattern: how task records relate
  memory/
    working/checkpoints/_checkpoints.md      <- Memory category: live resumption state
    emerging/ideas/_ideas.md                 <- Memory category: possibilities
    emerging/analysis/_analysis.md           <- Memory category: reasoning and evidence
    crystallized/decisions/_decisions.md     <- Memory category: accepted choices
  skills/use-workflow/references/planning/
    _planning.md                             <- recipe scope entrypoint
    planning.md                              <- workflow recipe
  templates/planning/
    _planning.md                             <- Template category entrypoint
    task.md  plan.md  backlog.md  checkpoint.md
    idea.md  analysis.md  decision.md        <- seven starters
```

## What each file is for

### The workflow: `planning.md`

**Kind:** workflow recipe. **Used when:** you ask the agent to plan a change.

**Goal:** an executable plan with a clear outcome, sequence, and current state, so another capable agent can proceed without guessing intent. The steps: identify the task source (yours, or the current request), establish the outcome, inspect the actual system before sequencing, expose dependencies and decisions, order coherent steps each with an observable result, and keep one answer per question instead of copying.

It completes when the plan covers the outcome, boundaries, dependencies, ordered steps, decision points, and completion condition, without creating an unauthorized task or a competing status source.

### The Pattern: `work-records.md`

**Kind:** Pattern. **Used when:** work needs durable records and the project has no suitable structure.

It defines the default shape. Start with **one [Task](../glossary.md#task)** that holds three answers together:

| Section       | Defines                                                                      |
| ------------- | ---------------------------------------------------------------------------- |
| Outcome       | The accepted result, scope, exclusions, constraints, and completion evidence |
| Plan          | The short sequence, dependencies, and verification                           |
| Current State | Progress, decisive evidence, blockers, and the next action                   |

Split into a separate **[Plan](../glossary.md#plan)**, **[Backlog](../glossary.md#backlog)**, or **[Checkpoint](../glossary.md#checkpoint)** only when that answer needs its own maintenance. Each changing fact keeps one defining source, and external task systems keep the facts assigned to them.

### The Memory categories

| Entrypoint                    | State        | Holds                                                         | Key rules                                                                                                                                                                                            |
| ----------------------------- | ------------ | ------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `checkpoints/_checkpoints.md` | Working      | Current state, step, and next steps for one active workstream | One per workstream, only when durable sources aren't enough to resume. Tag it `#Active` and `#KeepInMind` while it's active. When the need ends, remove those tags, then archive or prune it.        |
| `ideas/_ideas.md`             | Emerging     | Possibilities, experiments, open questions                    | Record requested ideas and explorations here. Keep the motivation clear enough to revisit.                                                                                                           |
| `analysis/_analysis.md`       | Emerging     | Structured reasoning, investigation, comparison               | State the question, evidence, assumptions, limits, and current conclusion. Check the assumptions still hold before relying on it.                                                                    |
| `decisions/_decisions.md`     | Crystallized | Accepted choices and why                                      | Record what was chosen, why, and who accepted it. Keep proposals elsewhere until accepted. Link to the source that defines the current result. One choice, or a tightly related group, per Decision. |

Each arrives empty. Records appear as you create them.

### The Templates

| Template        | Starts a...    | Main sections                                                                                                   |
| --------------- | -------------- | --------------------------------------------------------------------------------------------------------------- |
| `task.md`       | Task           | Outcome (with a "Done when" checklist), Plan, Current State, Result                                             |
| `plan.md`       | Separate plan  | Outcome Source, Steps And Dependencies (table), Verification And Completion                                     |
| `backlog.md`    | Selection view | Scope, Items (work, priority, why now)                                                                          |
| `checkpoint.md` | Checkpoint     | Goal And Sources, Current State, Accepted Direction And Evidence, Open Questions, Next Steps                    |
| `idea.md`       | Idea           | Opportunity, Proposed Direction, What We Know, Smallest Useful Experiment                                       |
| `analysis.md`   | Analysis       | Question, Current Conclusion, Evidence And Reasoning, Alternatives And Assumptions, Next Check                  |
| `decision.md`   | Decision       | Decision (with "Accepted by"), Context And Rationale, Alternatives And Tradeoffs, Consequences, Current Sources |

## How to use it

> Use the planning workflow for this change. Keep the plan and state in the existing task, and preserve separate reasoning only when it will help later.

> Record this as a Decision: we're keeping Postgres, and here's why.

## Good to know

- Ideas, Analysis, and Decisions are independent conventions, **not a pipeline**. A clear request can go straight to work, and a Decision isn't a mandatory approval meeting.
- The Memory categories keep their meaning wherever they're used. Shipping them in Planning doesn't narrow Decisions to planning.
- The Templates require no status system, ID scheme, estimate, hierarchy, or approval stage you haven't chosen.
- User Flow, Scenario, and Run Record starters live in the separate [Flows and Scenarios](scenarios.md) package.

Next: [Install this package](#install), or [update and remove Extensions](/guides/extensions#update-and-remove).
