# Mismatch interval

This page explains the 800 active milliseconds during which two different revealed faces remain visible, including pauses and exact expiry.

`MISMATCH_MS` is 800 in `src/game/game.ts`. A miss stores that duration as `selection.remainingMs`. The interval is part of game state and advances through the same logical time operation as elapsed time. It is not a standalone callback that remembers two DOM nodes to close later.

## The pending state

After two eligible positions with different face values are revealed, selection contains `kind: mismatch`, `first`, `second`, and `remainingMs`. The move has already been counted. Matched positions are unchanged.

While selection remains mismatch, `activateCard` returns the same state for every activation. A third card is not revealed or saved for later. Repeated activation of either exposed card does not extend the interval. Focus can move independently of that game-state rejection.

Both faces are included by `faceUpCards`, alongside all previously matched positions. When selection closes, both temporary faces leave that derived set in one state transition. Matched faces remain up.

The start of this interval is the second reveal. Time spent considering the first revealed card contributes to elapsed time but does not consume any of the 800 milliseconds, since there is no mismatch yet.

## Exact core arithmetic

For a started, unfinished game and a finite positive delta, `advanceGame` updates elapsed time and then subtracts the same delta from a pending remainder:

```ts
const remainingMs = state.selection.remainingMs - milliseconds;
const selection =
  remainingMs > 0
    ? { ...state.selection, remainingMs }
    : { kind: SelectionKind.closed };
```

A positive result stays pending. Zero and negative results close selection. A single advance of 800 from a new mismatch closes it; an advance of 799 followed by 1 has the same selection outcome.

The whole delta contributes to elapsed time, subject to the elapsed cap. If a callback arrives 900 milliseconds after a new mismatch, the function closes it and adds 900 elapsed milliseconds. It does not discard the 100 milliseconds after expiry, since the game remains active between turns.

No extra move is added on expiry. The move was charged when the second card was revealed. There is also no new turn-result feedback event on a clock tick; closing is the delayed consequence of the already reported miss.

## A visible-time example

On `np1-4-1`, reveal positions 0 and 1. Their face values differ. From that second activation:

| Active duration since miss | Selection result |
| ---: | --- |
| 0 ms | Mismatch, 800 remaining |
| 250 ms | Mismatch, 550 remaining |
| 799 ms | Mismatch, 1 remaining |
| 800 ms | Closed |

These durations exclude hidden-page intervals and an imported snapshot's pause. They are not promises about when the browser paints a frame. The logical boundary is exact; the visible update happens when the browser runs and renders the relevant work.

A one-card selection has no comparable timeout. Waiting 800 milliseconds with one card revealed leaves that card revealed. [Selection state](selection-state.md) distinguishes the alternatives.

## Browser scheduling

`useSessionClock` in `src/game/session/use-session-clock.ts` uses a recursive timeout. Its usual interval is `CLOCK_INTERVAL_MS`, currently 40. When a mismatch is present, the next delay is the smaller of 40 and its remaining duration.

The timeout is a chance to observe `performance.now`, not a claim that exactly its requested delay passed. `elapsedStep` subtracts the previous observation from the actual reading and passes that positive active delta through `tickSession` to `advanceGame`.

There is no fixed number of ticks defining a mismatch. Twenty nominal 40-millisecond waits and a delayed larger update are different schedules that can produce the same active duration. Basing the interval on measured time prevents slower rendering from stretching the logical memory window by counting callbacks.

The hook reads the latest session through a ref when it schedules and synchronizes. It does not preserve the two indices from the original miss in an independent closing closure. A later time update applies to the current selection.

## Synchronization before a click

`useGame.activate` calls `clock.synchronize()` before `activateSession`. This ordering handles an overdue mismatch even if the timeout has not run.

For example, assume a miss starts at monotonic reading 362.25. Its logical expiry is 1162.25. A click at 1163.25 first advances the pending duration past zero, then can reveal a new eligible first card. The stale rendered appearance immediately before that task does not force the click to be ignored.

A click while positive time still remains is ignored after synchronization. It does not become an action waiting to execute at expiry. The distinction is the measured active duration at the activation, not the location of a timer callback in the task queue.

`tests/fractional-clock.e2e.test.ts` explicitly checks an overdue click in the same task that changes the test clock, leaving no opportunity for an earlier timeout to resolve the selection first.

## Hiding and showing the page

The hook tracks `document.hidden` through a visibility listener. On a visibility change it synchronizes using the previous visibility state, updates its stored visibility, and schedules as appropriate.

On hiding, that final synchronization includes the active portion since the last observation. While hidden, the scheduling predicate is false. On showing, synchronization observes the hidden period as inactive, updates the observation point, and then restarts visible scheduling.

If 200 active milliseconds of a mismatch passed before hiding, it has 600 remaining on return even after a long hidden interval. Another 599 visible milliseconds leave it pending; the next active millisecond closes it.

The elapsed clock uses the same accounting, so the hidden period is excluded from both. A page merely losing focus is not itself the predicate in this hook; the relevant browser signal is page visibility. [Elapsed time rules](elapsed-time-rules.md) covers that distinction.

## Loading and resuming a mismatch

A valid snapshot can carry a mismatch with a remaining whole duration from 1 through 800. `replaceGame` pauses an imported unfinished game, retaining the restored selection. `activateSession` and `tickSession` return early while the session is paused.

Resume resets the browser clock's observation point and clears `paused`. The time spent reading the loaded position is therefore excluded. A restored remainder of 300 needs another 300 active milliseconds after Resume; it does not receive a fresh 800-millisecond allowance.

If the page is hidden when an imported pause is cleared, visibility still prevents active time. Resume removes one gate, while the clock's running predicate also checks page visibility, started, and completion.

Export first synchronizes the active clock. It rounds a fractional positive remainder upward to a whole millisecond. A live remainder of 0.25 exports as 1, preserving a pending selection that the decoder accepts. Export does not restart the interval or pause the live game. See [Snapshot selection notes](snapshot-selection-notes.md).

## Replacement and stale work

Restart during a mismatch discards that selection and creates a hidden game on the same code. New Game and size changes also clear it while choosing another seed. Successful loads replace it with the loaded position.

Each replacement increments `session.generation`. The timer path rejects a generation that differs from the current session. Effect cleanup additionally marks the old effect inactive, clears its timeout, and removes its visibility listener. An old deal cannot consume part of a replacement game's mismatch or close its new first card.

Within one deal, there is no independent old mismatch deadline that can later close another turn. Time advancement reads the current selection and applies only the measured delta since the last observation. `tests/timer-lifecycle.e2e.test.ts` checks a later mismatch after an earlier interval has finished, as well as cleanup on unmount.

## Caps do not disable the interval

Elapsed saturation uses `min(ELAPSED_MAX_MS, elapsedMs + milliseconds)`, but mismatch subtraction still uses the original positive delta. A miss can therefore close even when the elapsed counter has no room to increase. The displayed clock's remaining capacity is not the source of mismatch time.

Move saturation is independent too. At 999,999 moves a miss still creates the normal pending state and closes normally. Neither counter limit allows an extra reveal during the interval or turns a miss into completion.

## Timing evidence

`game.test.ts` checks the exact 799-plus-1 boundary. `tests/idle-clock.e2e.test.ts` checks untouched hidden and paused intervals. Fractional-clock and lifecycle tests add synchronization and replacement cases.

For a report, include the code, two positions, active durations, and visibility or Resume events. Total wall-clock waiting alone can include periods deliberately excluded from the remainder.
