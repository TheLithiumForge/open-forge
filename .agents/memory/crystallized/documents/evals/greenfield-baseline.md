---
open-forge:
  description: The first Open Forge eval, one short prompt for Neon Pairs, a small React memory game, run in an empty folder, in fresh Open Forge installs and with the Collaboration extension, checking that Open Forge is not worse
  tags: [Memory, Document, CurrentTruth, Evaluation, Greenfield, Baseline, Demo]
---

# Greenfield Baseline

Greenfield Baseline is the first Open Forge eval: one short prompt asks for Neon Pairs, a small memory game, and we run it in an empty folder and in fresh Open Forge installs.

Someone who starts a new project with no docs should lose nothing by installing Open Forge first. This eval checks that in the barest case, where the only context is the prompt. For example, the same prompt runs five times in an empty folder and five times in an Essentials install, and we compare whether the games work, how they look, how well they follow the prompt's one rule and what they cost.

Status: proposed on 2026-10-30, then updated the same day with the maintainer's project choice and decisions. The [eval tool](../../../../../src/evals/README.md) runs it, and its hidden checks pass on a reference build of the game. No real model run has happened yet. It's the first step of the [Eval Ladder](../../../emerging/ideas/eval-ladder.md), which isn't accepted yet.

## What we compare

Every setup gets the same prompt, tool, model, effort and limits:

- No setup - an empty folder.
- Minimal - a fresh install of the Minimal preset.
- Essentials - a fresh install of the Essentials preset.
- Full Core - a fresh install of the Full Core preset.
- Helpful - Essentials plus the Guidance route and the Collaboration extension, whose Adaptive Collaboration Guidance helps the agent find out what the user wants. Essentials leaves out Guidance, so the route comes first.

No setup holds project docs. We run every preset because the result speaks for Open Forge as a whole, and a preset that does worse stays in the results.

Helpful asks a second question: does helping the user say what they want give them a better game, and what does it cost them? Adaptive Collaboration is Guidance, so the agent reads it only when it finds it relevant. Context delivered shows whether it did.

## The project

Neon Pairs is a memory matching game with a neon look. The grid goes from 4x4 up to 16x16, and the code draws every card face. A 16x16 board needs 128 different shapes, so the agent has to generate them instead of picking a few by hand.

We chose it because:

- It's fun to look at and to show.
- A turn-based game gives the hidden checks clear actions and results, with no timing to make them flaky.
- It's small, but it has real ways to fail, such as a double click on one card, clicks during the flip-back delay, a best score saved from an unfinished game, an odd grid like 5x5 that pairs can't fill, or 128 shapes that look too alike.
- React, TypeScript and a build step make it a real project, with packages, types and a build that can break.
- The 16x16 board gives values we can chart, such as how fast a tap shows its card among 256 cards.

The maintainer chose it on 2026-10-30 over a reading-list app, which nearly every run would likely build perfectly, and the neon snake, whose timing makes reliable checks hardest.

## The prompt

Every run gets this prompt, word for word:

> Can you build me a neon-style memory matching game called Neon Pairs? I want to pick the grid size, from 4x4 up to 16x16, and the card faces should be shapes drawn by the code, not images or emoji. Add a move counter, a New Game button, and remember my best score for each grid size between visits. It has to work on my phone and with the keyboard. Use React and TypeScript with a small build step that gives me one index.html I can just open. I like locality of behavior, so keep each component's logic, markup and styles together. If you have any questions, ask me.

It reads like a developer's first prompt for a hobby project: the goal, the features, the stack and one coding rule. It asks for no planning, docs, tests or token savings, so it neither rewards nor punishes what Open Forge encourages. The single `index.html` gives the checks one fixed entry point without setting the code's structure.

It ends with an invitation to ask, as most people write. Every setup gets it, so Helpful has to beat agents that were already invited to ask.

The rule is there so every eval, starting with this one, can score how well the agent follows the user's rules. Here it shows whether Open Forge's own instructions get in the way of a rule given in the prompt. Later evals on the Eval Ladder move rules out of the prompt and into files.

## The user's answers

A real user answers questions, so the eval does too. A simulated user, a fixed model given a hidden intent sheet, answers the agent:

- It answers only from the intent sheet. Anything the sheet doesn't cover gets "Your call, pick what you think works best."
- It never adds requirements unasked and never helps with the code.
- The run ends when the agent's message isn't a question, or after three rounds of questions.
- We keep the questions, the answers and their cost in the run record, apart from the agent's cost.

The intent sheet holds what this user wants but didn't say:

- Start on 4x4 the first time, then remember the last grid size picked.
- Even sizes are enough. Odd grids like 5x5 aren't needed.
- Cyan and magenta on a near-black background.
- A short celebration when a game is won.
- No sound.

Every setup gets the same simulated user, so a setup that asks better questions can get closer to what the user wanted. Hidden checks test the start size, the remembered size and the sound. The judge scores the rest.

## Run rules

Each run follows these steps:

1. Start in a fresh, isolated folder at the same path, with the setup's files restored from a frozen snapshot. The install runs before the clock starts. [Running Evals](running-evals.md) has the details.
2. Use a clean tool config: no user-level instructions, no native memory, no plugins, hooks or MCP servers, and no web search. The npm registry stays reachable, because the project needs packages. Open Forge's own files stay.
3. End when the agent's turn ends without a question, after three rounds of questions, or after 30 minutes. A final answer that only holds a plan also ends the run.
4. Freeze the files and stop every agent process before any check runs. Check results never reach the agent.

We run Claude Code and Codex, each with one pinned model. Results compare setups within one tool and model, never across them.

## Checks

The hidden checks first run the project's `build` script again in the frozen folder, with no network. Then they open the `index.html` that the build writes in a pinned browser that blocks every outside request, and play the game through its controls.

The board is shuffled, so the checks learn it as they play: they turn cards, compare the faces they see, and count a pair when the game keeps it face up. They never read the game's private state, and they wait generously for the flip-back delay instead of assuming its length.

| Area       | Check                                                                                                                  |
| ---------- | ---------------------------------------------------------------------------------------------------------------------- |
| Delivery   | The build works offline, and its one `index.html` opens straight from disk with no other files and no outside requests |
| Stack      | React and TypeScript, as asked, and the type check passes                                                              |
| Grid sizes | 4x4 and 16x16 are offered, and every size the game offers can be finished                                              |
| Faces      | No images or emoji. Both cards of a pair look the same, and every pair looks different from the others, also at 16x16  |
| Matching   | A matching pair stays face up. A mismatch shows both cards, then hides them                                            |
| Input      | Clicking one card twice doesn't make a pair. Extra clicks during the delay don't break the turn                        |
| Moves      | The counter goes up during play, and a game with many misses shows more moves than one with none                       |
| New Game   | Clears the board and the moves, also during the flip-back delay                                                        |
| Win        | Solving every pair shows that the game is won                                                                          |
| Best score | Kept per grid size. Only finished games count, a clearly better game replaces it, a worse one doesn't, and it survives a reload |
| Phone      | A full 4x4 game by tapping at a phone size                                                                             |
| Keyboard   | A full 4x4 game with the keyboard alone                                                                                |

The best score check doesn't assume what the score counts. Its better game has fewer moves and less time than the worse one, so any sensible score ranks them the same way.

The checks read each card's face from screenshots: its colors, and its shape with half-pixel offsets allowed. At 16x16, every card's closest look-alike has to choose it back, and no second card can pass as the same face. On the reference build, the checks take about six minutes a run.

## Data points

Each run records every [Eval Data Point](data-points.md) that applies. These matter most here:

- Functional success and requirement coverage.
- Rules followed, for the locality of behavior rule.
- Intent coverage, for the intent sheet.
- Blind picks between games, at desktop and phone width.
- Build, shipped size and performance at 16x16.
- Tokens, dollars and time, with Open Forge's own reading counted apart.
- Peak context, context delivered and compactions.
- Questions, above all for Helpful.

A build this size will likely have no compactions, so it tells us little about context degradation. Later evals test that.

## Not worse

We decide before any run what "not worse" means. Each Open Forge setup has to stay within every margin against No setup:

| Outcome            | Margin                                 |
| ------------------ | -------------------------------------- |
| Functional success | At most 5 points lower                 |
| Picked             | Wins at least 40% of blind picks against No setup |
| Rules followed     | At most 10 points fewer yes answers    |
| Dollars per run    | At most 10% higher                     |
| Time per run       | At most 10% higher                     |

Each outcome passes on its own, so a better look can't make up for a higher cost. Each setup ends with one of three results:

- Not worse - every interval stays inside its margin.
- Worse - an interval sits fully past a margin.
- Inconclusive - an interval crosses a margin. That isn't evidence of harm.

With five or ten runs per setup, the intervals are wide, so most results will read inconclusive. A large gap still reads worse, and we report every run, so a few runs still catch big differences and real harm.

Helpful also reports intent coverage and the questions it cost the user, against Essentials, with no margin.

## Runs and cost

We start small, keep every run isolated, and repeat to see whether a result holds:

1. Dry run - one run per setup on one tool, to test the harness and the checks. We never report it.
2. First batch - five runs per setup for each tool and model, with the setups in random order.
3. Second batch - five more runs for each setup whose first batch differs from No setup, to see whether the difference holds.

Showing a small difference in pass or fail would take about a thousand runs per setup, and we don't plan that.

The first batch is 25 runs per tool and model, 50 for both tools. At roughly $1 to $5 a run, it costs about $50 to $250. These are rough allowances, not measured prices.

If every setup passes every check in the dry run and the first batch, the task is too easy. We report that and change the task, without adding hidden requirements.

## Results

The results follow the [reporting rules](data-points.md#reporting):

- One table per tool and model, with one row per setup.
- A chart of each setup's difference from No setup, with the margins drawn in.
- A chart of tokens against requirement coverage, one dot per run.

## Not decided yet

- The margins.
- The model for each tool, and whether a cheaper model buys more runs.
- The model behind the simulated user, and the full intent sheet.
- Whether the locality of behavior rule stays in this prompt or moves to the next eval.
- Whether Helpful builds on Essentials or on another preset.

Back to [Evals](_evals.md).
