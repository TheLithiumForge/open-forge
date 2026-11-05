# Clock design

This page traces `browser-clock.ts` and `use-session-clock.ts`, including scheduling, visibility changes, action synchronization and cleanup.

The clock has a small arithmetic part and a browser-lifecycle part. The arithmetic keeps an observation baseline; the hook decides when to observe and when an interval counts as active play. `advanceGame` remains responsible for what a positive amount of logical time does to the game. [Time policy](time-policy.md) states the ownership boundary; this page follows the implementation.

## The arithmetic record

`browser-clock.ts` exports `CLOCK_INTERVAL_MS = 40`, `createClock` and `elapsedStep`. Its private `ActiveClock` interface contains only `observedAt: number`. It does not store accumulated active time, a mismatch deadline, a timer identifier or a pause flag.

`createClock(now)` returns `{ observedAt: now }`. `elapsedStep(clock, now, active)` subtracts the previous observation, accepts the delta only if active, finite and positive, and returns `{ clock: createClock(now), milliseconds }`. Accumulated elapsed time remains in `GameState`; the clock only supplies the next contribution.

An inactive step still updates the baseline. That is the mechanism that discards waiting time without asking the game rules to subtract it later. A backward or nonfinite delta contributes zero, but its supplied observation still becomes the baseline. The function has no hidden memory beyond the record passed by its caller.

## Fractional values survive observation

The arithmetic never floors the delta. For observations at 0, 0.3, 0.8, 1.1, 3.7 and 10.9, adding the active steps gives the same total as one active observation at 10.9, allowing ordinary floating-point tolerance. A timer frequency change therefore does not inherently change elapsed accounting.

This matters particularly for a mismatch opened between integer milliseconds. Its remaining interval is a positive number in memory and decreases by actual observed active time. Export is the separate rounding boundary: elapsed rounds downward and a positive pending remainder rounds upward.

`browser-clock.test.ts` checks fractional steps, hidden intervals, waiting before start, backward observations, NaN and independence from observation frequency. These tests do not schedule anything. Their purpose is to isolate the arithmetic contract from browser event behavior.

## Hook inputs and persistent refs

`useSessionClock(session, current, commit)` receives the rendered session, a ref to the latest session and a function that commits a new value. It creates a ref for the arithmetic clock and another for the stored visibility state, initialized from `!document.hidden`.

The expression supplied to `useRef(createClock(performance.now()))` is evaluated as the hook renders, while React retains the first ref value. Later evaluations do not by themselves reset that retained baseline. Explicit reset updates `clock.current`. This distinction helps when tracing observations in a render-heavy test: evaluating an initializer expression and replacing a ref value are different operations.

The hook derives `won = isWon(session.game)` for effect dependencies. Inside asynchronous work, `running()` uses `current.current` to read pause, start and victory state, along with the visibility ref. It does not decide activity from a session captured several renders earlier.

## Synchronization step by step

`synchronize(generation = current.current.generation)` begins with a generation check. A mismatched captured generation returns before observing or updating the baseline. Otherwise it reads a fresh browser observation and calls `elapsedStep` using the current running predicate.

It always installs the returned clock record. If `milliseconds` is nonzero, it commits `tickSession(current.current, generation, milliseconds)`. The session function performs another generation check and a pause check, then delegates to `advanceGame`.

The second check is data-layer protection. The first check also prevents an old callback from disturbing the new game's observation baseline. A session-level refusal alone would stop an incorrect game advance but would not explain ownership of the clock record.

The hook's returned `reset()` replaces the baseline with a new observation. It does not clear the game elapsed total or selection. Controller actions combine reset with the appropriate game replacement or resume transition, which is where progress semantics are defined.

## One rescheduling chain

The effect captures the rendered generation, sets `active = true` and holds an optional timer identifier. Its `schedule()` function first clears the current identifier. It then returns if the effect is inactive, the generation no longer matches or `running()` is false.

If active, it reads the current selection. A mismatch uses `Math.min(CLOCK_INTERVAL_MS, selection.remainingMs)` as the requested delay; other states use 40. The timeout callback checks `active`, synchronizes with the captured generation and schedules again.

This is a recursive timeout chain rather than a fixed interval. The next delay is chosen from current state after the previous callback. A mismatch near expiry can request its smaller remainder, and an inactive session stops producing periodic work. The nominal delay is never treated as the actual amount elapsed.

## A delayed callback

Suppose active play has a baseline of 100ms and a timeout requested for 40ms runs at 180ms. Synchronization contributes 80ms. It does not contribute one unit of 40ms merely because one callback fired. The rules update elapsed time and any pending mismatch using that measured contribution.

If the remaining mismatch was 60ms, the same 80ms advance closes it. The rules still add the whole active 80ms to elapsed time, subject to the cap. They do not reduce total elapsed to 60ms just because the selection closed partway through the interval.

This is why callbacks are observation opportunities rather than authority over time. A busy event loop can delay visible updates, but the next synchronization catches the game up from the last observation. [Mismatch interval](mismatch-interval.md) covers the visible duration independently of this scheduling mechanism.

## Visibility event ordering

`visibilityChanged()` first checks effect activity and generation. It then calls `synchronize(generation)` before assigning `visible.current = !document.hidden`, and finally calls `schedule()`.

On a visible-to-hidden event, synchronization sees the previously visible state and accounts for active time up to the hide observation. The flag then becomes false, and scheduling clears the pending timeout and stops.

On a hidden-to-visible event, synchronization sees the previously hidden state. It contributes zero while updating the baseline to the return observation. The flag then becomes true and scheduling resumes. This order excludes the hidden interval without relying on browser timer throttling behavior.

Reversing that assignment and synchronization would misclassify the interval ending at the event. The relevant state is the activity during the time since the previous observation, not merely the new visibility value after the event. The current order makes that distinction explicit.

## A visibility timeline

| Observation | Stored visibility used for the step | Contribution |
| --- | --- | --- |
| Start accepted at 100ms | Not started before the activation | Zero for waiting before start |
| Hide event at 350ms | Visible | 250ms active play |
| Show event at 5350ms | Hidden | Zero for the hidden interval |
| Next active observation at 5390ms | Visible | 40ms active play |

The table assumes no intervening observations and an unfinished, unpaused game. It describes the same arithmetic as many short periodic steps. Hidden time is excluded because the baseline advances on return, not because the system clock stopped.

Headless Chromium does not naturally supply the hidden-tab behavior these journeys need, so browser tests override `document.hidden` and dispatch `visibilitychange` at the adapter boundary. This drives the actual handler while keeping the rule functions unchanged.

## Action synchronization is deliberate

The controller synchronizes immediately before activation. Consider a mismatch whose logical interval has expired while its scheduled callback remains queued. A click can land first. Synchronization closes the old mismatch through `advanceGame`, then `activateSession` sees the newly closed selection and accepts the click as the next first reveal.

`fractional-clock.e2e.test.ts` tests this in one browser task: it updates the controlled observation and invokes a button click before any timer can run. That makes the action-boundary behavior observable without depending on machine load.

Export has the same need for current data. The last rendered state can trail the clock by less than a scheduled interval or more under load. `exportSnapshot` synchronizes and encodes the latest ref. It does not serialize the session value captured by the prior render.

## Replacement, pause and Resume

New Game, Restart and valid loads reset the baseline before replacement. `replaceGame` increments generation, causing the prior effect to become obsolete. An earlier callback checks both activity and generation before affecting the replacement.

An unfinished snapshot import installs its saved state but pauses the session. No active scheduled loop runs while paused. Resume resets the baseline before clearing `paused`, so waiting at the Resume control is not charged to the imported game. Its saved elapsed total and mismatch remainder remain intact.

An invalid load only changes notices. It neither replaces the game nor resets its baseline. An export during active play also does not pause. The controller's action semantics therefore determine which intervals count; the reproduction panel's visibility is not itself a clock state.

## Effect dependencies and cleanup

The effect depends on `session.generation`, `session.game.started`, `session.paused` and the derived `won`. Those values describe changes that start, stop or replace the scheduling lifetime. Ordinary elapsed updates do not restart the effect on every render.

Cleanup marks `active` false, clears the timeout and removes the visibility listener. The callback's activity check handles a callback associated with a retired effect. The generation check handles a replaced game. Removing the listener prevents a later visibility event from invoking obsolete session work after unmount.

The timer identifier may be undefined before anything is scheduled. The browser clear operation is used both at the start of `schedule` and at cleanup, keeping one ownership path. There is no independent timeout per mismatch stored in the core state.

## Completion and numeric caps

When the final match is accepted, the controller has already synchronized. `isWon` then becomes true, the effect's victory dependency changes and the running predicate becomes false. Further logical advances return the won game unchanged.

At `ELAPSED_MAX_MS`, however, victory need not be true. The game can continue, including resolving pending mismatches, even though the elapsed total no longer increases. The hook does not treat the cap as a reason to stop scheduling. This is consistent with the core's independent handling of elapsed accumulation and selection remainder.

The move cap has no direct role in this hook. It is part of activation logic. Separating those bounds avoids turning a display/storage limit into an accidental lifecycle stop.

## Evidence that isolates the risks

The arithmetic tests establish numerical behavior. Session tests establish generation and pause gating. `clock-edges.e2e.test.ts` exports at amounts that are not multiples of the 40ms interval, demonstrating action synchronization. Fractional tests preserve submillisecond mismatch origin and export a final quarter-millisecond as a valid pending interval.

`idle-clock.e2e.test.ts` waits through a hidden period without inspecting the game repeatedly, then checks the remaining active interval on return. A separate case imports a paused mismatch and waits before Resume. These cases expose bugs that incidental observations during inactivity could conceal.

`timer-lifecycle.e2e.test.ts` instruments timeouts and visibility listeners in the separate session harness, unmounts the actual game and checks both counts return to zero. It also starts another mismatch in the same deal after an earlier one has closed. That case distinguishes stale work inside one generation from stale work across replacement.

When investigating drift, the useful trace contains observations, active conditions and resulting deltas. Counting callbacks or inspecting the displayed seconds is too coarse to distinguish a rounding defect from a baseline defect. The two-file design makes each of those quantities available to focused tests without adding diagnostic state to the shipped game.
