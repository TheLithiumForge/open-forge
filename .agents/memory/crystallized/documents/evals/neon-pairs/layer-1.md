---
open-forge:
  description: Layer 1 idea prompt, current setups, answer bank and L1-v2 checks
  tags: [Memory, Document, CurrentTruth, Evaluation, NeonPairs, Layer1]
---
# Layer One
Layer One is the Neon Pairs case that compares what agents build from one short idea prompt.

Status: approved case definition on 2026-11-01. Real end-to-end testing of the combined product is still needed.

An agent gets the requested game and one coding rule, with no product documents. For example, remembered grid size is a private preference here. Layer 2 states that rule before work starts.

## Prompt

Send the existing case prompt word for word:

> Can you build me a neon-style memory matching game called Neon Pairs? I want to pick the grid size, from 4x4 up to 16x16, and the card faces should be shapes drawn by the code, not images or emoji. Add a move counter, a New Game button, and remember my best score for each grid size between visits. It has to work on my phone and with the keyboard. Use React and TypeScript with a small build step that gives me one index.html I can just open. I like locality of behavior, so keep each component's logic, markup and styles together. If you have any questions, ask me.

The prompt lives in `src/evals/benchmarks/neon-pairs/cases/greenfield-baseline/prompt.md`. It asks for no plan, tests, documents or token savings. Its component locality rule stays part of the stated request. The product specs aren't given to this layer.

## Setups

Use the same prompt, tool, pinned model, effort, resources and limits in these setups:

| Setup | Starting context |
| --- | --- |
| No setup | Empty project with no project documents |
| Minimal | Fresh Minimal preset |
| Essentials | Fresh Essentials preset |
| Full Core | Fresh Full Core preset |

Helpful waits with Extension testing. The earlier proposal put it on Essentials with the Guidance route and Collaboration extension. That later comparison measures useful questions and planning against its parent preset. It isn't part of the current batch.

Run each case five times per setup, model and tool in balanced random order. Four current setups give 20 attempts for one model and tool. Keep every failure and timeout. These five-run cells screen differences and support no non-inferiority conclusion. Don't add another batch because a result nearly passes.

## Answer bank

Use exactly the five preferences in the case's `intent.json`:

| Intent ID | Answer |
| --- | --- |
| `start-4x4` | Start on 4x4 the first time, then remember the last grid size I picked. |
| `even-sizes` | Even sizes are enough. Odd grids like 5x5 aren't needed. |
| `colors` | Cyan and magenta on a near-black background. |
| `celebration` | A short celebration when a game is won. |
| `no-sound` | No sound. |

The simulated user knows the prompt and this bank. It gives the relevant answer only when asked. It volunteers no preference and gives no coding advice. A choice outside the bank gets "Your call, within the supplied requirements." The shared [Protocol](../protocol.md#simulated-user) defines the three question rounds and failure handling.

Starting size, remembered size and silence stay in the wishes result. Even sizes, colors and celebration also stay outside stated correctness when the prompt didn't state them. Asking belongs to collaboration. A question earns credit only when the answer is used.

## Run

Restore a frozen setup in a fresh container and tool home before starting the clock. Keep user instructions, native memory, plugins, hooks, MCP servers and web search out. Allow the npm registry for dependencies. Use the same entry path in every container. Keep setup manifests and grading outside it.

Stop at a completed turn or 30 minutes, with the shared question protocol in between. A final plan without a game is still a finished attempt. Stop child processes and freeze output before verification. The [Running Evals](../running-evals.md) guide covers the product commands and recovery.

The game is small enough to build but still fails in useful ways. A second click can pair a card with itself, a stale timer can change New Game and unfinished progress can become a best score. At 16x16, 256 cards need 128 distinguishable faces. React, TypeScript and an offline build add real delivery checks. This turn-based game also avoids the continuous timing demands of a snake game.

## Checks

Use grading version `L1-v2`. Keep historical G1 results under their own recorded grading version. The six working checks require offline opening, pointer completion, phone-tap completion, keyboard completion, a shown win and New Game clearing board and moves. Every journey needs no uncaught error.

The 16 stated checks cover delivery, React and TypeScript, offered and finishable sizes, code-drawn distinguishable faces, matching and mismatch behavior, input, moves, New Game, completion and per-size best scores. The three wishes cover first size, remembered size and silence. The exact frozen checks and separate judge results follow [Grading](../grading.md#layer-checks).

Build the frozen game offline, then open the HTML from disk in the pinned browser with outside requests blocked. Find faces through screenshots and play. Use generous mismatch waits because this prompt specifies no interval. Don't require board codes, snapshots, replay, a solver hook, exact labels or a particular scoring formula. A better best-score journey has both fewer moves and less time, so either sensible score can rank it first.

## Results

Record the shared data points, including functionality, requirement coverage, locality, wishes, blind desktop and phone picks, shipped size, 16x16 response time, tokens, cost, time, reads, tool calls, peak context and compactions. Keep question and judge usage apart from coding usage. The [Data Points](../data-points.md) defines each measure.

Compare setups within one model and tool. Publish every attempt and inconclusive results under [Results](../results.md). A small build with no compactions says little about context loss. If all setups pass, report that the task doesn't separate them before designing a later case.

The [approval](../../../../emerging/analysis/eval-product-proposal.md#approved) replaces the earlier baseline's five-setup batches, result-dependent extra runs and proposed margins. It keeps the CLI beta and moves delivery layer by layer. The [Baseline Lessons](../../../../emerging/ideas/baseline-lessons.md) keeps the first batch's findings without changing this prompt.

Back to [Neon Pairs](_neon-pairs.md).
