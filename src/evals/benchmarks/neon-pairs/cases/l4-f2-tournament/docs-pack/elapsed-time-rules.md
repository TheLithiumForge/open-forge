# Elapsed time rules

This page explains active elapsed time, browser clock accounting, and display formatting in Neon Pairs.

`src/game/elapsed-time.ts` contains the formatter. Accumulation belongs to `advanceGame` in `game.ts`, and the browser decides which durations to supply through `session/use-session-clock.ts` and `session/browser-clock.ts`. Reading those three layers separately explains why rendering frequency does not define the measured duration.

## What counts as active

Elapsed time begins with the first valid reveal. Before that, `advanceGame` returns the existing state because `started` is false. Reading the controls or waiting at a fresh hidden board does not accrue game time.

Once started, time counts while the page is visible and the game is unfinished, unless an unfinished imported snapshot is awaiting Resume. It includes time spent looking at one revealed card, considering a choice between turns, and viewing a mismatch. It does not measure only the time when a pointer or keyboard is moving.

The browser running predicate combines four conditions:

```text
page visible
and session not paused
and game started
and game not won
```

The core knows only started and completion. Visibility and imported pause are browser-session concerns. Direct calls to `advanceGame` assume the caller has supplied an active duration.

There is no ordinary manual pause control during a played game. Resume is associated with loaded unfinished progress. Page visibility is based on `document.hidden`; the hook does not use window focus, pointer idleness, or the reproduction panel's open state as additional clock gates.

## Core accumulation

`advanceGame(state, milliseconds)` ignores a duration that is nonfinite or nonpositive. It also ignores it before start or after victory. For a valid active step:

```ts
const elapsedMs = Math.min(ELAPSED_MAX_MS, state.elapsedMs + milliseconds);
```

`ELAPSED_MAX_MS` is 359,999,999. The current implementation retains fractional milliseconds in memory, since monotonic browser observations can include fractions. It does not round each observation down before adding it.

The same call subtracts its delta from a pending mismatch. A large step can close the mismatch while adding the entire active duration to elapsed time. The 800-millisecond mismatch duration is not an upper bound on how much elapsed time that call can add.

Elapsed saturation does not end play. It also does not stop a mismatch countdown, because that countdown uses the original supplied delta. See [Mismatch interval](mismatch-interval.md).

## Observing the browser clock

`browser-clock.ts` stores one observation in an `ActiveClock`: `observedAt`. `createClock(now)` initializes it. `elapsedStep(clock, now, active)` computes the difference from that observation, returning a positive finite difference only when active is true.

Every result includes a new clock observed at `now`, even when no duration was counted. Advancing the observation during inactivity is essential: the next active observation can then exclude the period spent waiting, hidden, or paused.

The browser hook reads `performance.now()` and uses recursive timeouts with a usual 40-millisecond interval. It measures the actual difference between readings. It does not add a fixed 40 merely because a callback ran.

For example, active readings at 0, 0.3, 0.8, 1.1, 3.7, and 10.9 accumulate the same duration, within floating-point precision, as a single active observation from 0 to 10.9. `browser-clock.test.ts` checks this property, along with fractional durations and inactive intervals.

That example concerns logical observation, not a guarantee of a 40-millisecond browser paint. A busy task can delay a callback. When it runs, the measured difference catches up the game to the observed active time.

## Start, actions, and completion boundaries

Before processing a card activation, `useGame.activate` synchronizes the clock. At the first reveal, the game is still unstarted during this synchronization, so earlier waiting is discarded and the observation point moves to the reveal's time. The activation then sets started to true.

Later activations synchronize while the game is active. This captures duration between rendered ticks and ensures an overdue mismatch can close before a new card is considered. At the final activation, synchronization captures time up to that action before the game becomes won.

After victory, the running predicate is false and `advanceGame` also refuses additional duration. The completion time is retained without waiting for the completion banner's visual effect to finish.

Export also synchronizes before encoding a snapshot. Thus exported progress can be ahead of the last ordinary rendered tick, while still corresponding to the current active clock. It does not rely on a status string that was formatted earlier.

## Visibility boundaries

The visibility listener synchronizes before changing its stored visibility flag. On the transition to hidden, it counts the active interval up to that observation. On the transition back to visible, it observes the intervening duration as inactive, then resumes scheduling.

Suppose the game starts, runs visibly for 300 milliseconds, then a miss runs visibly for another 200. Hiding for five seconds adds nothing. On return, another 600 active milliseconds finish the mismatch. The total elapsed duration is 1,100 milliseconds: 300 before the miss plus the miss's full 800 active milliseconds.

Counting wall-clock time between first reveal and completion would include the hidden five seconds and disagree with the game's definition. A reproduction of a clock issue needs the visibility changes as well as the overall wait.

The hook clears its scheduled timeout while inactive. The implementation also handles a synchronization request made during inactivity by updating the observation without counting that period.

## Imported pause and resets

An unfinished import retains its elapsed value and waits paused. Resume resets browser observation before clearing that pause, excluding the time spent inspecting the position. If started is false, the clock still waits for a valid first reveal. Completed imports remain stopped by victory.

Successful replacements also reset observation. New Game and Restart receive zero game time from `createGame`; snapshots retain their imported value. Invalid loads do neither reset nor replacement. [New Game and Restart](new-game-and-restart.md) compares these operations.

## Display conversion

`formatElapsed` first floors milliseconds divided by 1,000. It then divides that whole-second count into minutes and a seconds remainder, padding the seconds to two digits:

```ts
const seconds = Math.floor(milliseconds / 1000);
const minutes = Math.floor(seconds / 60);
return `${minutes}:${String(seconds % 60).padStart(2, '0')}`;
```

| Stored milliseconds | Formatted value |
| ---: | --- |
| 0 | `0:00` |
| 999.9 | `0:00` |
| 1,000 | `0:01` |
| 59,999 | `0:59` |
| 60,000 | `1:00` |
| 3,600,000 | `60:00` |
| 359,999,999 | `5999:59` |

There is no separate hour field. Minutes continue past 59. `GameStatus` places the `Time:` label alongside this formatted value, and completion feedback uses the same formatter.

The formatter does not enforce the game's domain or cap its argument. It assumes a valid duration supplied by state. The cap is enforced by advancement and snapshot validation, so a direct unsupported call to the formatter is not an alternative game input path.

## Snapshot precision

`encodeSnapshot` floors elapsed milliseconds to a whole number. It rounds a pending mismatch remainder upward. These opposite rounding directions preserve the external constraints: elapsed is a whole nonnegative duration, and a still-pending mismatch has at least one millisecond remaining.

For an in-memory elapsed value of 11.75 and a mismatch remainder of 799.25, export writes elapsed 11 and remaining 800. Import restores those integer values. The live game keeps its fractions; export does not replace it with the rounded representation.

A round trip can therefore change submillisecond precision in the restored copy. That is expected at the format boundary. It does not justify rounding every browser tick, which would repeatedly discard fractions and make accumulated time depend on observation frequency.

The fractional browser test uses quarter milliseconds so its test values are exactly representable in binary. It checks both elapsed export and a final quarter-millisecond mismatch remainder exporting as 1.

## Replay uses explicit active time

Replay advances to each action's logical timestamp before activation, then to its end time. Waiting before the first valid reveal is ignored, and completion stops later advances. Hidden intervals and import pauses are omitted from this active timeline. Compare active durations when comparing replay and browser results; see [Replay format](replay-format.md).

## Source evidence

`elapsed-time.test.ts` covers the cap, exported capped progress, and rejection above the cap. `browser-clock.test.ts` checks fractional accumulation and inactive observations. `tests/clock-edges.e2e.test.ts` exports between scheduled ticks. `tests/idle-clock.e2e.test.ts` checks untouched hidden and paused intervals.

These are distinct checks: a formatter example verifies text, a core advance verifies arithmetic, and a browser journey verifies which intervals reach that arithmetic. A correct-looking status string alone cannot prove that a hidden period was excluded or that the final click was synchronized.
