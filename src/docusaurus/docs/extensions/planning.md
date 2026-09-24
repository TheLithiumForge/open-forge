---
title: Planning
description: A planning workflow, the Work Records Pattern, four Memory categories, and seven Templates for tasks, plans, and decisions.
---

# Planning

Turn an outcome into work that can be understood, carried out, and resumed. Keep your existing boards and task systems, and create only the records that add useful context.

- **Package ID:** `planning`
- **Depends on:** [Workflow Support](workflows.md)
- **Loads at startup:** Only the one-line entries for its Memory categories and Pattern, through their already-loaded parents. The contents stay on demand.

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

It defines the default shape. Start with **one Task** that holds three answers together:

| Section       | Defines                                                                      |
| ------------- | ---------------------------------------------------------------------------- |
| Outcome       | The accepted result, scope, exclusions, constraints, and completion evidence |
| Plan          | The short sequence, dependencies, and verification                           |
| Current State | Progress, decisive evidence, blockers, and the next action                   |

Split into a separate **Plan**, **Backlog**, or **Checkpoint** only when that answer needs its own maintenance. Each changing fact keeps one defining source, and external task systems keep the facts assigned to them.

### The Memory categories

| Entrypoint                    | State        | Holds                                                         | Key rules                                                                                                                                                  |
| ----------------------------- | ------------ | ------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `checkpoints/_checkpoints.md` | Working      | Current state, step, and next steps for one active workstream | One per workstream, only when durable sources aren't enough to resume. Tag it `#Active` and `#KeepInMind` while active, and remove those tags at closeout. |
| `ideas/_ideas.md`             | Emerging     | Possibilities, experiments, open questions                    | Record without treating it as accepted. Keep the motivation clear enough to revisit.                                                                       |
| `analysis/_analysis.md`       | Emerging     | Structured reasoning, investigation, comparison               | State the question, evidence, assumptions, limits, and current conclusion. Check the assumptions still hold before relying on it.                          |
| `decisions/_decisions.md`     | Crystallized | Accepted choices and why                                      | Record what was chosen, why, and who accepted it. Keep proposals elsewhere until accepted. One choice per Decision.                                        |

Each arrives empty. Records appear as you create them.

### The Templates

| Template        | Starts a...    | Main sections                                                                                  |
| --------------- | -------------- | ---------------------------------------------------------------------------------------------- |
| `task.md`       | Task           | Outcome (with a "Done when" checklist), Plan, Current State                                    |
| `plan.md`       | Separate plan  | Outcome Source, Steps And Dependencies (table), Verification And Completion                    |
| `backlog.md`    | Selection view | Scope, Items (work, priority, why now)                                                         |
| `checkpoint.md` | Checkpoint     | Goal And Sources, Current State, Accepted Direction And Evidence, Open Questions, Next Steps   |
| `idea.md`       | Idea           | Opportunity, Proposed Direction, What We Know, Smallest Useful Experiment                      |
| `analysis.md`   | Analysis       | Question, Current Conclusion, Evidence And Reasoning, Alternatives And Assumptions, Next Check |
| `decision.md`   | Decision       | Decision (with "Accepted by"), Context And Rationale, Alternatives And Tradeoffs, Consequences |

## How to use it

> Use the planning workflow for this change. Keep the plan and state in the existing task, and preserve separate reasoning only when it will help later.

> Record this as a Decision: we're keeping Postgres, and here's why.

## Good to know

- Ideas, Analysis, and Decisions are independent conventions, **not a pipeline**. A clear request can go straight to work, and a Decision isn't a mandatory approval meeting.
- The Memory categories keep their meaning wherever they're used. Shipping them in Planning doesn't narrow Decisions to planning.
- The Templates require no status system, ID scheme, estimate, hierarchy, or approval stage you haven't chosen.
- User Flow, Scenario, and Run Record starters live in the separate [Flows and Scenarios](scenarios.md) package.
