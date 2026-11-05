# Time policy

This page describes how Neon Pairs accounts for active play time and keeps browser timing separate from logical game transitions.

There are three related values to distinguish: a browser observation, the delta between observations and the elapsed milliseconds stored in a game. The browser adapter observes; the clock arithmetic determines a delta; the rules apply that delta. The displayed statistic is derived from stored elapsed time rather than from a second independent stopwatch.

## Where observations enter

`use-session-clock.ts` owns browser observation and scheduling. Its neighbor `browser-clock.ts` accepts observations as arguments and computes a numerical step. Game code reads time only through the session clock. Only files in src/game/session/ may call performance.now, and no game code may use Date. Rule functions therefore receive elapsed amounts directly and can be exercised without a wall clock.

This boundary does not mean every file in the session directory performs an observation. `session.ts` transforms data and delegates logical advancement to `advanceGame`. `browser-clock.ts` creates an observation record from a supplied number. Their placement keeps time-related behavior together while the browser hook contains the actual calls.

The tests have their own environment control. Browser fixtures can install a Playwright clock and test scripts can set the observation that the application sees. Those are verification mechanisms outside the game source, not additional application clocks.

## Conditions for active time

The hook's `running()` predicate reads the latest session ref. It returns true only when the document is considered visible, the session is not paused, the game has started and the game is not won. The rules independently refuse to advance a game that has not started or has already been won.

A first accepted card activation sets `started` to true. The synchronization before that activation treats the preceding wait as inactive. The next active observation can therefore measure from the activation boundary rather than from page load. Repeated activation of the same revealed card is ignored by the rules but synchronization still accounts for time that passed before the attempted action.

An unfinished imported snapshot is paused by `replaceGame`. Its saved elapsed time and mismatch remainder remain game data, while the session prevents further active advancement until Resume. Resume resets the observation baseline and clears pause-related notices. It does not restart the imported game or give a pending mismatch a new full interval.

Completed games stop logical advancement. A completed imported game is already won, so it does not acquire an active timer. Completion through a final match is synchronized before that match, which makes the final stored elapsed time reflect play up to the finishing activation.

## Delta arithmetic

`ActiveClock` stores `observedAt`. `createClock(now)` creates that record. `elapsedStep(clock, now, active)` computes `delta = now - clock.observedAt`; it returns the delta only when active, finite and positive. Otherwise it returns zero. In both cases it returns a new clock whose baseline is `now`.

Advancing the baseline during an inactive observation is essential. Suppose the last active observation was 100ms, the page becomes hidden at 150ms and becomes visible at 5150ms. The hidden-to-visible event synchronizes while the stored visibility flag is still false. That step contributes zero and moves the baseline to 5150ms, so the following active observation excludes the hidden interval.

The arithmetic keeps fractional milliseconds. There is no per-observation integer rounding or tick counter. Several submillisecond positive deltas can add up just as one longer delta would. The tests compare totals with a floating-point tolerance where necessary.

A backward observation or a nonfinite delta produces no advancement. This is defensive handling of supplied values, not a claim that such a sequence is a normal browser clock behavior. The clock still records the supplied observation, so later arithmetic is based on that new baseline. [Clock design](clock-design.md) discusses this small function and the hook separately.

## Scheduling is an opportunity to observe

`CLOCK_INTERVAL_MS` is 40. The hook uses a timeout that reschedules itself. For a mismatch it asks for the smaller of 40 and `remainingMs`; otherwise it uses 40. A scheduled delay is not added directly to the game. When the callback runs, the hook obtains a new observation and computes the actual delta.

This difference handles delayed callbacks. If the browser runs a 40ms timeout after 80ms, the measured active interval is 80ms. The rules are not told that exactly one nominal tick occurred. Conversely, receiving more callbacks does not make time pass faster, because each callback advances from the previous observation.

When the running predicate is false, scheduling stops. The visibility listener remains available to re-evaluate the state on return. Changes to generation, start state, pause state and victory also affect the hook's effect lifecycle. React rerenders alone do not supply elapsed milliseconds.

## Synchronize at action boundaries

The controller calls `clock.synchronize()` before `activateSession`. That ordering can close an overdue mismatch before the new activation is evaluated. A busy browser can delay the scheduled callback, but it does not make a logically expired interval block the next click indefinitely.

The controller also synchronizes before exporting a snapshot. Rendered state can trail the current clock by part of a scheduled interval. Serializing that rendered state directly would make exports depend on when the last render occurred. The controller instead encodes `current.current.game` after synchronization.

Fresh deals, restart, valid board loads and valid snapshot loads reset the observation baseline before replacing the game. Resume resets it before unpausing. Invalid load attempts publish a notice but do not replace the game or reset its baseline. The eventual valid synchronization still accounts for the active interval during that interaction.

## Mismatch time and elapsed time

`advanceGame` ignores a nonfinite, zero or negative amount. For a started unfinished game it adds a positive amount to `elapsedMs`, capped at `ELAPSED_MAX_MS = 359_999_999`. If the selection is a mismatch, it also subtracts the amount from `remainingMs`.

A positive remainder keeps the mismatch with its new remainder. A zero or negative remainder closes the selection. The function does not carry a queue of delayed closing events, and it does not truncate elapsed advancement to the mismatch remainder. A 900ms advance against an 800ms mismatch closes it and adds 900ms of active elapsed time.

The mismatch remainder and elapsed total serve different purposes. The total can reach its cap while a new mismatch still has time to resolve. `advanceGame` continues processing the selection even after the total is capped. Similarly, the move cap does not stop matching, time or victory. These limits protect stored/displayed numbers rather than ending play.

The 800ms mismatch interval is defined beside turn resolution as `MISMATCH_MS`. Browser scheduling reads the current remainder; snapshot validation imports the same bound. [Mismatch interval](mismatch-interval.md) covers the player-visible rule, while this page explains how the interval survives browser lifecycle events.

## Serialization has a rounding boundary

Active play retains fractions. `encodeSnapshot` floors `elapsedMs` and rounds a mismatch's `remainingMs` upward. A positive remainder smaller than one millisecond therefore exports as 1, preserving a valid pending interval. Rounding the remainder downward could produce a mismatch object with a zero value that the decoder rejects.

The decoder accepts only nonnegative safe integer elapsed values within the cap and a mismatch remainder from 1 through 800. This means the serialized representation is intentionally coarser than the in-memory state. The rounding happens at export, not after every callback.

The display uses `formatElapsed`, which floors to whole seconds and derives minutes and a two-digit seconds component. That formatter is presentation; it does not determine when cards close or when a game finishes. [Elapsed time rules](elapsed-time-rules.md) covers the visible statistic and limit.

## Deal generations and stale work

Every `replaceGame` call increments `generation`. A scheduled callback captures the generation from its effect. The hook checks that captured generation before observation, and `tickSession` checks it again before applying a delta. An earlier deal's callback cannot spend time in the replacement deal through the normal path.

The effect also has an `active` flag. Cleanup marks it false, clears the timeout and removes the visibility listener. The flag handles callbacks associated with a retired effect; the generation handles replacement ownership. These are related protections with different scopes.

Restarting the same board still changes the generation. Reusing the same code does not mean reusing the old progress or pending browser work. `Game.tsx` also keys `Board` by generation, which resets local board focus state on replacement. This shares a lifecycle marker without putting focus inside the time model.

## Test evidence

Arithmetic, session transitions and browser integration have separate checks. The clock suite covers fractions, inactivity and observation frequency; the session suite covers pause and generations; browser journeys cover between-tick export, overdue clicks, hidden intervals and teardown. [Clock design](clock-design.md) follows those cases in detail, and [Test policy](test-policy.md) explains the runners.

