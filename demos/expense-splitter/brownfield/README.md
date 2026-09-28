# Brownfield demo: add a feature to a half-built app

A friend started an expense splitter and handed it to you. It works: it adds expenses, splits them evenly, and shows who owes whom. It has tests. It also has two important rules: money stays in whole cents, and the payer absorbs the leftover cent. The code follows both, but only the author's scratch notes explain why. Your job is to add percentage splits without breaking them.

This is what most real work looks like. The code is the best evidence of how things behave today, but the reasons behind it live somewhere else, if they're written down at all.

## What's inside

| Path                           | What it is                                                                                                   |
| ------------------------------ | ------------------------------------------------------------------------------------------------------------ |
| [`app/`](app/)                 | The half-built project: TypeScript that Node.js 22.18+ runs directly, with tests and no runtime dependencies |
| [`app/NOTES.md`](app/NOTES.md) | The author's scratch notes, where the important reasons live                                                 |
| [`seeds/`](seeds/)             | The feature request at four levels of detail                                                                 |
| [`reference/`](reference/)     | Records a careful run might produce, for comparison afterwards                                               |
| [`checks.md`](checks.md)       | How to tell whether the result is right                                                                      |

## Run it

1. Copy `app/` somewhere outside this repository, so your agent sees only the demo, then commit it as a starting point:

   ```sh
   cp -R app ~/expense-splitter && cd ~/expense-splitter
   git init && git add -A && git commit -m "Starting point"
   npm test
   ```

2. Install Open Forge and the Development Toolkit, which brings Planning (for Decisions), Project Documents, Flows and Scenarios, and the Development workflows:

   ```sh
   open-forge install
   open-forge extension install development-toolkit
   git add -A && git commit -m "Added Open Forge"
   ```

3. **Map what's already there.** Ask your agent:

   > Add a Map of this project's important files, such as the README, the author's notes, and the tests, with a line on when to read each one.

   A Map points to sources and copies nothing. It's part of the base Framework. The Maps entrypoint is read at startup, so every later task sees the Map's one-line entry. The Map itself opens on demand, when a task needs the project's sources, and says where the notes are and when they matter.

   The rules the app already had get no Decisions. Decisions record choices from the moment you adopt Open Forge, for changes made from then on. For the older rules, the Map points to where their reasons live: `NOTES.md`.

4. **Pick a seed level** from [`seeds/`](seeds/) and give your agent that request. Start with level 1 or 2. Neither mentions rounding, so they show whether the agent finds the rules on its own, in the code or through the Map to the author's notes. A good run keeps both rules, and records its own choice, how percentage splits round, as a Decision that stays proposed until you accept it.

5. **Check the result** against [`checks.md`](checks.md), then compare your records with [`reference/`](reference/).

## Try it both ways

Run the same seed on a second copy of the app without Open Forge, and compare. The failures to look for are quiet ones: shares computed with floating point, or leftover cents given to whoever was typed first instead of to the payer. The tests that exist today don't catch either.
