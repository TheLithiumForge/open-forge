---
title: "Brownfield: add a feature to a half-built app"
sidebar_label: Brownfield
description: Adopt Open Forge in a working but half-finished expense splitter, then add percentage splits without breaking rules whose reasons live only in the author's notes.
---

# Brownfield: add a feature to a half-built app

A friend started an expense splitter and handed it to you. It works and it has tests. It also relies on two rules: money stays in whole cents, and the payer absorbs the leftover cent. The code follows both, but only the author's scratch notes explain why. Your job is to add percentage splits without breaking either. The whole demo is in [`demos/expense-splitter/brownfield`](../../../../demos/expense-splitter/brownfield/).

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
2. Install Open Forge and the [Development Toolkit](../extensions/development-toolkit.md). The Map in the next step is part of the base Framework. Decisions come from Planning, which the Toolkit installs.
3. **Map what's already there.** Ask your agent:

   > Add a Map of this project's important files, such as the README, the author's notes, and the tests, with a line on when to read each one.

   A [Map](../concepts/core-categories.md#maps) points to sources and copies nothing. The Maps entrypoint is read at startup, so every later task sees the Map's one-line entry. The Map itself opens on demand, when a task needs the project's sources, and says where the notes are and when they matter.

   The rules the app already had get no Decisions. The advice here is to record Decisions from the moment you adopt Open Forge, for changes made from then on. For the older rules, the Map points to where their reasons live.

4. Give your agent a seed request. Start with level 1 or 2. Neither mentions rounding, so the agent has to find the rules itself, in the code or through the Map to the author's notes. A good run keeps both rules and records its own choice, how percentage splits round, as a [Decision](../highlights.md#decisions-keep-the-why) that stays proposed until you accept it.
5. Check the result against [the checklist](../../../../demos/expense-splitter/brownfield/checks.md), and compare your records with the [reference records](../../../../demos/expense-splitter/brownfield/reference/).

## The quiet failures

The existing tests don't catch either of these failures:

- **Floating point returns** through "just the percentage", and shares stop being whole cents.
- **The extra cent goes to whoever was typed first** instead of to the payer. The result looks correct, but it contradicts how even splits already work.

Run the same request on a second copy without Open Forge and compare. Look less at whether the feature works and more at whether the rules survived.
