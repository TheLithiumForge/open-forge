---
open-forge:
  description: Audit of the Neon Pairs reference's own tests from 2026-11-03 run timings, with overlap groups, the false regressions they cause and a proposal to keep one browser test per acceptance journey, waiting for the maintainer's approval
  tags: [Memory, Analysis, Contextual, Candidate, Evaluation, NeonPairs, Tests, Brownfield, Proposal]
---

# Reference Test Audit

## Which of the reference's own tests are essential, and which only cost agent time?

The maintainer asked on 2026-11-03 for only useful and essential tests, regression tests included, without checking everything or duplicating tests. Layer 4 agents get the reference's tests with the game, and [Technology](../../../../src/evals/benchmarks/neon-pairs/specs/technology.md) has them run the full commands, so every test costs every run.

## What the suite costs

The reference ships 20 unit test files and 37 browser test files with 93 browser tests. One best-score run's full browser suite on 2026-11-03 showed 87 distinct titles and took 664 seconds back to back, with 34 runs sharing 12 cores. Agents ran it two or more times per run, so tests took about half of each run's time.

The project's unit-test command is also a regression check (T07.4). Its two replay CLI tests start npm in a child process and time out at vitest's 5000ms default when the verifier is busy. On 2026-11-03 that failed `regression-l2-build` and `regression-l3-t07-build` for every Open Forge cell of B4 silence, with byte-identical tests and `package.json` in every cell, so those regressions are timing, not the agents' work.

## Overlap groups

Seconds are from that run, summed over each group's browser tests.

| Group | Browser tests | Seconds | Overlap |
| --- | --- | --- | --- |
| Full games | all-sizes per size, play by keyboard and by taps, the accessibility journey | 120 | `completion.test.ts` already completes every size in unit tests |
| Best scores | preferences, persistence-contract, saved-bests, move-cap, part of snapshots | 52 | `storage.test.ts` and `session.test.ts` cover the same rules |
| Clocks | clock-edges, fractional-clock, idle-clock, elapsed-cap, timer-lifecycle, the hidden-page pause | 79 | `browser-clock.test.ts`, `elapsed-time.test.ts` and `game.test.ts` cover the arithmetic |
| Focus and keyboard | keyboard, focus-entry, focus-clearance, pointer-focus at three viewports, play edges, the lifecycle tab stop | 90 | Several tests walk the same remembered-focus rules |
| Visual contract | layout and spacing per viewport, matched-mark per size and width, zoom, style, product-contract, input-contract, art, card-render | 135 | Parameterized viewports repeat one layout check |

## Proposal

The [test audit council](test-audit-council/record.md) replaced this proposal. One browser test per journey would have changed how hard B1 and B2 are and lost coverage that recorded mutants prove, so the council keeps every test that fails on a planted bug or is a mutant's only killer. The first proposal follows for the record.

1. Keep one browser test per journey in [Acceptance](../../../../src/evals/benchmarks/neon-pairs/specs/acceptance.md), which stays unchanged. Logic variants such as every size, clock edges and saved-best rules move to the unit tests or go where a unit test already covers them.
2. Parameterized viewports check the two viewport sizes the spec names, not three.
3. The replay CLI tests call `node scripts/replay.ts` directly, with one npm call kept for the caller-folder rule, and take a longer timeout.
4. The static source audits stay. They run in milliseconds and hold the project's own rules.

Expected effect: about 25 to 30 browser tests instead of 87, and the browser suite from about 660 to about 300 seconds under the same load, with fewer timing failures in grading.

## Cost

The reference is itself a graded data point, so it's regraded after the change. Every task's seed and solution are regenerated where they touch tests, since B2's seed deletes the keyboard tests, and every task and packet is validated again. Old runs keep the tests they had and stay comparable only with each other.
