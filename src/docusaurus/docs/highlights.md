---
title: Highlights
description: The features that do the most work for particular kinds of users, starting with Decisions, and when each one is worth your attention.
---

# Highlights

Open Forge is small, but a few of its features do most of the work for particular kinds of users. Not everyone needs all of them. Find the row that matches your situation.

| If you...                                              | Look at                                                                          | Because                                                               |
| ------------------------------------------------------ | -------------------------------------------------------------------------------- | --------------------------------------------------------------------- |
| Work on an existing codebase, or with other people     | [Decisions](#decisions-keep-the-why)                                             | The reason behind a change survives the person who made it            |
| Keep docs elsewhere, in a wiki or ADRs                 | [Maps](#maps-point-dont-copy)                                                    | The agent is pointed to them without you copying anything             |
| Have a large repository or a monorepo                  | [Scopes](#scopes-rules-only-where-they-apply)                                    | Specialized rules load only for the work that needs them              |
| Change the shipped files to suit your team             | [Overwrite companions](#overwrite-companions-keep-your-tweaks)                   | Your changes survive every update                                     |
| Switch between sessions, agents, or people             | [Checkpoints and Handoffs](#checkpoints-and-handoffs-resume-from-the-real-state) | Work can resume from its recorded state                               |
| Want reviews and debugging done the same way each time | [Development workflows](#development-workflows-evidence-first)                   | The methods ask for evidence behind findings and a cause before a fix |
| Care about user journeys                               | [Flows and Scenarios](#flows-and-scenarios-expected-versus-observed)             | What should happen stays apart from what did                          |

Maps, scopes, and overwrite companions are part of the base Framework. The others come from optional Extensions: Decisions and Checkpoints from [Planning](extensions/planning.md), Handoffs from [Observations and Handoffs](extensions/observations-and-handoffs.md), the workflows from [Development](extensions/development.md), and Flows and Scenarios from the [package of the same name](extensions/scenarios.md).

## Decisions: keep the why

Decisions come from the Planning Extension, and early feedback named them as one of its most useful parts. They matter most once a codebase outlives the people who started it. Code tells you what a system does. It rarely tells you why, and the why is exactly what gets lost when a person leaves, a chat session ends, or an agent "cleans up" something that looked odd.

A Decision records one choice and why it was made. You write one when the choice is made, so its reasons are still fresh:

```md title=".agents/memory/crystallized/decisions/percentage-split-rounding.md (shortened)"
# Percentage splits keep whole cents and payer-first leftovers

## Decision

Each share is its percentage of the total, rounded down to a whole cent. Leftover cents go to the payer first.

**Accepted by:** the maintainer, when percentage splits were added.

## Context And Rationale

Even splits already give the leftover cent to the payer. Rounding percentage splits differently would split the same expense two ways.

## Alternatives And Tradeoffs

| Alternative                              | Why it was not selected                             |
| ---------------------------------------- | --------------------------------------------------- |
| Give leftover cents to the largest share | Fair in its own way, but contradicts the payer rule |
```

A Decision isn't where the current specification lives. When you accept one, the rules ask for the documents it affects to be updated too, such as the Architecture or the README section for the feature, and the two link to each other: the document to the Decision for the reason, the Decision to the document for the result. [The document flow](extensions/document-flow.md) shows the whole picture.

At startup an agent sees that a Decisions category exists, because the Crystallized Memory entrypoint lists it in one line. The Decisions entrypoint and the records it lists are on demand. They open only when a task touches a past choice, so a long history adds nothing to startup beyond that line. The [brownfield demo](demos/brownfield.md) shows why Decisions matter: the app's code follows two rules, but only the author's notes explain why, and the rounding choice for its new feature is the kind of reason that gets lost.

**When to write one:** when you choose between real alternatives, or when you change behavior on purpose. In an existing codebase, start with your next change. You can't decide what was already decided before you arrived, and a Map can point to wherever older reasons live.

**How:** install [Planning](extensions/planning.md), then say "Record this as a Decision". A Decision records an accepted choice, so the Planning rules ask the agent to keep a choice you haven't accepted as a proposal until you do.

## Maps: point, don't copy

Maps are a Core category, so they're part of the base. A Map lists important sources, such as your docs folder, ADRs, API specs, or runbooks, and says when to read each one. The sources stay where they are and keep their own authority. That makes an existing project's knowledge reachable without moving or copying it. See [Maps](concepts/core-categories.md#maps).

## Scopes: rules only where they apply

Put frontend rules in a frontend scope and database rules in a database scope. A task selects the scopes it needs. Each unselected scope costs one entry line in its parent entrypoint. In a monorepo, give each package its own scope. See [Scopes](concepts/scopes.md).

## Overwrite companions: keep your tweaks

Want the shipped Memory rules slightly different? Add `_memory.overwrite.md` next to `_memory.md`. It loads right after the base file, wins where the two answer the same question differently, and updates leave it alone. See [Customizing](concepts/customizing.md#overwrite-companions).

## Checkpoints and Handoffs: resume from the real state

A Checkpoint, from Planning, holds where a workstream stands and what comes next. While it's active it's tagged `KeepInMind`, so once a task opens the Checkpoints category, the rules have the agent re-read it at each refresh point, including when work resumes. A Handoff, from Observations and Handoffs, seals a snapshot when work actually changes hands. Together they let work resume after a pause, or pass to another agent, from its recorded state. See [Planning](extensions/planning.md) and [Observations and Handoffs](extensions/observations-and-handoffs.md).

## Development workflows: evidence first

The Development Extension adds these workflows, which run through the `use-workflow` Skill from Workflow Support. The Review workflow returns prioritized findings with evidence and stays read-only unless you ask for fixes. The Debugging workflow separates hypotheses with the cheapest check before changing anything, and treats an inconclusive diagnosis as a valid result. See [Development](extensions/development.md).

## Flows and Scenarios: expected versus observed

The Flows and Scenarios Extension separates expectation from result. Write what should happen before anyone tries it, then record what actually happened in a separate Run Record. The demos use the same idea for their checks. See [Flows and Scenarios](extensions/scenarios.md).

## Also worth knowing

- **You can preview every change.** CLI commands that change files accept `--dry-run`, and the plan lists what would change. Add `--detail standard` to see every file.
- **Removed stays removed.** Delete a default with `open-forge remove`, and later updates respect it.
