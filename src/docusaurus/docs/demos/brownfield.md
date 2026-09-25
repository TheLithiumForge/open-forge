---
title: "Brownfield: add a feature to a half-built app"
sidebar_label: Brownfield
description: Adopt Open Forge in a working but half-finished expense splitter, then add percentage splits without breaking rules that live only in the author's notes.
---

# Brownfield: add a feature to a half-built app

A friend started an expense splitter and handed it to you. It works and it has tests. It also relies on two rules that are explained only in the author's scratch notes: money stays in whole cents, and the payer absorbs the leftover cent. Your job is to add percentage splits without breaking either. The whole demo is in [`demos/expense-splitter/brownfield`](../../../../demos/expense-splitter/brownfield/).

## What's inside

| Path                                                                         | What it is                                                                    |
| ---------------------------------------------------------------------------- | ----------------------------------------------------------------------------- |
| [`app/`](../../../../demos/expense-splitter/brownfield/app/)                 | The half-built project: TypeScript run directly by Node.js 22.18+, with tests |
| [`app/NOTES.md`](../../../../demos/expense-splitter/brownfield/app/NOTES.md) | The author's scratch notes, where the reasons live                            |
| [`seeds/`](../../../../demos/expense-splitter/brownfield/seeds/)             | The feature request at four levels of detail                                  |
| [`reference/`](../../../../demos/expense-splitter/brownfield/reference/)     | Records a careful run might produce, for comparison afterwards                |
| [`checks.md`](../../../../demos/expense-splitter/brownfield/checks.md)       | How to tell whether the result is right                                       |

## Run it

1. Copy `app/` outside this repository, commit it, and run `npm test`.
2. Install Open Forge and the [Development Toolkit](../extensions/development-toolkit.md).
3. **Map what's already there.** Ask your agent:

   > Add a Map of this project's important files, such as the README, the author's notes, and the tests, with a line on when to read each one.

   The Map copies nothing. It makes sure every later task knows the notes exist and when they matter. The old rules stay the author's: nobody decides them after the fact.

4. Give your agent a seed request. Levels 1 and 2 are the interesting ones: they never mention rounding, so the agent has to find the rule through the Map. A good run keeps both rules and records its own choice, how percentage splits round, as a [Decision](../highlights.md#decisions-keep-the-why).
5. Check the result against [the checklist](../../../../demos/expense-splitter/brownfield/checks.md), and compare your records with the [reference records](../../../../demos/expense-splitter/brownfield/reference/).

## The quiet failures

The existing tests don't catch these, which is what makes them good demonstrations:

- **Floating point sneaks back in** through "just the percentage", and shares stop being whole cents.
- **The extra cent goes to whoever was typed first** instead of to the payer. It looks correct, and it silently contradicts how even splits already work.

Run the same request on a second copy without Open Forge and compare. The difference usually isn't whether the feature works. It's whether the rules survived.
