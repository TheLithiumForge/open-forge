---
title: Highlights
description: The features that do the most work for particular kinds of users, starting with Decisions, and when each one is worth your attention.
---

# Highlights

Open Forge is small, but a few of its features carry a lot of weight. Not everyone needs all of them. Find the line that sounds like you.

| If you...                                              | Look at                                                                          | Because                                                    |
| ------------------------------------------------------ | -------------------------------------------------------------------------------- | ---------------------------------------------------------- |
| Work on an existing codebase, or with other people     | [Decisions](#decisions-keep-the-why)                                             | The reason behind a change survives the person who made it |
| Keep docs elsewhere, in a wiki or ADRs                 | [Maps](#maps-point-dont-copy)                                                    | The agent finds them without you copying anything          |
| Have a large repository or a monorepo                  | [Scopes](#scopes-rules-only-where-they-apply)                                    | Specialized rules load only for the work that needs them   |
| Change the shipped files to suit your team             | [Overwrite companions](#overwrite-companions-keep-your-tweaks)                   | Your changes survive every update                          |
| Switch between sessions, agents, or people             | [Checkpoints and Handoffs](#checkpoints-and-handoffs-resume-from-the-real-state) | Work resumes from where it actually stands                 |
| Want reviews and debugging done the same way each time | [Development workflows](#development-workflows-evidence-first)                   | Findings come with evidence, and fixes follow causes       |
| Care about user journeys                               | [Flows and Scenarios](#flows-and-scenarios-expected-versus-observed)             | What should happen stays apart from what did               |

## Decisions: keep the why

Early feedback named Decisions as one of the most useful parts of Open Forge, and they matter most once a codebase outlives the people who started it. Code tells you what a system does. It rarely tells you why, and the why is exactly what gets lost when a person leaves, a chat scrolls away, or an agent "cleans up" something that looked odd.

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

A Decision isn't where the current specification lives. Accepting one updates the documents it affects, such as the Architecture or the README section for the feature, and the two link to each other: the document to the Decision for the reason, the Decision to the document for the result. [The document flow](extensions/document-flow.md) shows the whole picture.

At startup an agent sees that a Decisions category exists, because Crystallized Memory lists it. The Decisions themselves load only when a task touches a past choice, so a long history costs nothing until it's needed. The [brownfield demo](demos/brownfield.md) shows why this matters: the app's rules live only in the author's notes, and the rounding choice for its new feature is exactly the kind of reason that gets lost.

**When to write one:** when you choose between real alternatives, or when you change behavior on purpose. In an existing codebase, start with your next change. You can't decide what was already decided before you arrived, and a Map can point to wherever older reasons live.

**How:** install [Planning](extensions/planning.md), then say "Record this as a Decision". The agent proposes it, and it becomes current once you accept it.

## Maps: point, don't copy

A Map lists important sources, such as your docs folder, ADRs, API specs, or runbooks, and says when to read each one. The sources stay where they are and keep their own authority. It's the fastest way to make an existing project's knowledge reachable. See [Maps](concepts/core-categories.md#maps).

## Scopes: rules only where they apply

Put frontend rules in a frontend scope and database rules in a database scope. A task selects the scopes it needs, and the rest cost one line of context each. In a monorepo, give each package its own scope. See [Scopes](concepts/scopes.md).

## Overwrite companions: keep your tweaks

Want the shipped Memory rules slightly different? Add `_memory.overwrite.md` next to `_memory.md`. It loads right after the base file, wins where the two disagree, and updates leave it alone. See [Customizing](concepts/customizing.md#overwrite-companions).

## Checkpoints and Handoffs: resume from the real state

A Checkpoint holds where a workstream stands and what comes next, and it's re-read when work resumes. A Handoff seals a snapshot when work actually changes hands. Together they make "pick this up tomorrow" or "hand this to another agent" reliable. See [Planning](extensions/planning.md) and [Observations and Handoffs](extensions/observations-and-handoffs.md).

## Development workflows: evidence first

The Review workflow returns prioritized findings with evidence and stays read-only unless you ask for fixes. The Debugging workflow separates hypotheses with the cheapest check before changing anything, and treats "inconclusive" as an honest result. See [Development](extensions/development.md).

## Flows and Scenarios: expected versus observed

Write what should happen before anyone tries it, then record what actually happened in a separate Run Record. The demos use the same idea for their checks. See [Flows and Scenarios](extensions/scenarios.md).

## Also worth knowing

- **Everything previews first.** Commands that change files accept `--dry-run`, and the plan shows exactly what will be written.
- **Removed stays removed.** Delete a default with `open-forge remove`, and later updates respect it.
