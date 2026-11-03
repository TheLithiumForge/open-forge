---
open-forge:
  description: Council record for trimming the Neon Pairs reference's own tests, with the ranking, the four gates, a verdict per test file, the dissent, the experiment and the cost, approved and applied
  responsibility: Preserve the November 2026 reference test audit council's ranking, chair decisions, dissent and open decisions without changing the reference
  tags: [Memory, Analysis, Contextual, Candidate, Evaluation, NeonPairs, Tests, Brownfield, Proposal]
---

# Reference Test Audit Council

Status: approved by the maintainer on 2026-11-03 and applied in c4bb3f6c0, with the changes under Applied. It replaces the proposal in [eval-reference-test-audit.md](../eval-reference-test-audit.md), whose findings stand except where this record corrects them.

## Question

The maintainer asked for only useful and essential tests in the game the Layer 4 agents get, regression tests included, without checking everything or duplicating tests, and asked for a council to make sure.

## Method

Five independent read-only members got one brief and different personas: Prosecutor (argues against cuts), Minimalist (smallest suite), Spec Lawyer (maps tests to the spec's sentences), Eval Designer (what tests do to the measurement) and Flake Hunter (load and timing). Each wrote a verdict for every test file, a coverage proof, the expected effect, risks and an experiment. Then each ranked the other four, shuffled under letters with personas removed, against six weighted goals: journeys covered, planted-bug failures kept, agent time, load sensitivity, the grader's regression signal and migration cost. The chair wrote this record. [proposals.md](proposals.md) and [reviews.md](reviews.md) hold the evidence.

## Ranking

Average position across the four members who ranked each proposal. Lower is better.

| Rank | Proposal | Average | First places | Central idea |
| --- | --- | --- | --- | --- |
| 1 | A, Eval Designer | 1.0 | 4 | Keep every test that fails on a planted bug, drop only the rest |
| 2 | C, Flake Hunter | 2.25 | 0 | Make load-sensitive tests deterministic before cutting |
| 3 | D, Prosecutor | 2.5 | 1 | Keep every test that is the only killer of a recorded mutant |
| 4 | B, Spec Lawyer | 2.75 | 0 | One test per spec sentence, and a gap no test covers |
| 5 | E, Minimalist | 4.0 | 0 | The smallest suite that covers each journey once |

## What the council found

- **The audit's cut would change the tasks.** The shipped tests that fail on a planted bug are how the agent finds it: 2 unit and 7 browser tests for B1, 13 browser tests for B2, one for B3 and two for B4. One browser test per journey would cut B1's and B2's sets and make those tasks harder without anyone noticing.
- **The audit's cut would lose proven coverage.** `reference-audit/mutations.fixture.json` records mutants that only one browser test kills: M22 (scrollbars at 900x600), M23 (clock-edges' export test), M26 and M27 (idle-clock), M36 (pointer-focus at 640x400) and M40 (saved-bests). Only layout's 1280x1400 view checks V03's 768px cap. The unit oracle checks deals only at 4x4, so completion.test.ts can't catch a wrong deal at other sizes.
- **A spec journey has no test today.** "Reveal one card twice, then reveal a different card" is only half done by `first reveal starts play without a move and ignores invalid activation`.
- **Load, not count, is the main cost.** Each browser file starts npm once to read its version. Playwright's 5s expect timeout is unconfigured while B1's failing tests took 5.3 to 6.2s. The overdue-mismatch test can pass under load without reaching the overdue path. The suite must stay well under the grader's 600s command limit.
- **The audit's counts were wrong.** The source holds 93 browser tests in 37 files and 20 unit files. The run reported 87 distinct titles.

## Decision

### Gates

1. Every acceptance journey keeps a test, and `game.test.ts` gains the different-card reveal.
2. Every shipped test that fails on a planted bug still fails, and `keyboard.e2e.test.ts` and `keyboard.test.ts` stay byte for byte, since the B2 and P1 patches delete and restore them.
3. Every recorded mutant the current suite kills is still killed.
4. Under load the suite stays well under 600s, and no test runs near the 30s limit without an explicit timeout.

### Verdicts

| Verdict | Files |
| --- | --- |
| Drop | elapsed-cap, move-cap |
| Merge | controls into control-contract, preferences into persistence-contract, focus-clearance into layout, card-render into art |
| Trim | all-sizes to size 16 with a 90s timeout, its example deck and a deal check for every size moving into `deck.test.ts`. art drops `records the default board and focused revealed state for visual review` and crops the 128 faces from one screenshot. clock-edges drops `a capped imported clock keeps matching, closing and exporting valid progress` and runs its overdue test on the stubbed clock. matched-mark checks 16x16 at 360x800. pointer-focus drops 1280x720 and waits for several equal frames. saved-bests keeps `a completed import in 10 moves leaves a saved best of 8 alone`. scrollbars drops 1440x800. input-contract's hover test moves into style. zoom drops its two screenshots, which assert nothing. `scripts/replay.test.ts` calls node directly for the error cases, keeps one npm call for the caller folder and takes a 60s timeout. |
| Keep | accessibility with a 60s timeout, announcements, browser-environment, build-output, control-contract, fractional-clock, focus-entry, idle-clock, keyboard, layout's three views, lifecycle, motion, persistence-contract, play, product-contract, snapshots, spacing, status, status-controls, style, timer-lifecycle, touch-scroll, every unit file, `scripts/source-audit.test.ts` and `scripts/source-conventions.test.ts` |
| Harness | Read npm's version from `npm_config_user_agent`, raise Playwright's expect timeout once in the shared harness, and give accessibility, layout, zoom and all-sizes explicit timeouts |

Expected effect: about 70 browser tests in about 29 files instead of 93 in 37, about 500s instead of 664s at the load of 2026-11-03, and about 130s on an idle verifier. Planted-bug failures stay the same for all four tasks. Most of the time comes back from the load fixes and from running fewer agents at once, not from the cuts.

## Dissent

- **E, Minimalist:** about 33 tests in 270s. Any suite near 500s risks the 600s limit under heavy load, and folding files lengthens each test toward 30s. The chair keeps files apart and adds explicit timeouts instead.
- **B, Spec Lawyer:** three of the four source-convention tests state no spec rule and can fail an agent's honest edits under T07.4. The chair keeps them because they cost milliseconds and hold the project's own rules, so this stays an open decision.

## Experiment

Run `reference-audit/mutations.ts` with the full unit and browser suites for each mutant, on the current and the trimmed reference, adding three mutants: `max-height: min(70dvh, 768px)` changed to `70dvh`, a `Date.now` session clock and a deal change above 4x4. Then apply each task's seed and solution, five to ten runs each under the load of 2026-11-03. The trim holds if every seed fails the same shipped tests, every mutant the current suite kills is still killed, every solution passes every time without a T07.4 or T07.5 failure, and every run finishes well under 600s.

## Cost

Changing the reference changes every task's recorded hashes, so every task and packet is validated again and the reference is graded again. The seeds touch only source files and the keyboard files, which stay byte for byte, so no patch is regenerated. Runs before the change stay comparable only with each other.

## Applied

The maintainer approved the trim, asking that the browser still start a 16x16 game without playing it through. So all-sizes holds one test that starts the largest board with every card ready and takes one reveal, and `deck.test.ts` checks every size's deal against the independent oracle.

Two verdicts changed while applying it:

- **controls stays its own file.** Merging it into control-contract passed the project's 200-line limit for a source file.
- **art keeps its element screenshots.** Cropping all 128 faces from one screenshot needs a PNG decoder, and the project pins its packages. The screenshot-only test still went.

The keyboard tests stayed byte for byte, so no task patch changed. Mutants M19 and M25 now name layout as their killer, and M39 the session unit test, which was checked to kill it. `reference-audit/mutation-results.json` still records the old run, since the mutation script needs the named Chromium on the machine that runs it.

## Open decisions

1. Keep or trim the three source-convention tests that no spec rule states.
2. Run the mutation experiment above once the named Chromium is set up for the mutation script.
