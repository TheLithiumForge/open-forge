# Incident review: the clock counted callbacks

This is an incident review about elapsed-time drift and mismatch intervals that followed browser scheduling rather than active play time.

The current implementation separates the game core, browser clock, and session hook, with timing tests covering their boundaries.

## Two symptoms arrived as one report

In the report, a player left a large board open while the browser was busy. The displayed time advanced more slowly than the time they had actually spent looking at the cards. In a second attempt, they hid the page during a miss and returned to find both cards already closed. The first symptom undercounted active time; the second counted time that should have been excluded.

Both came from treating the browser's task schedule as the game's timeline. A periodic callback added a fixed increment to elapsed time, while an independent timeout closed a mismatch after wall time passed. Those mechanisms agreed during a quiet foreground demonstration and disagreed under throttling or visibility changes.

The impact was more than an inaccurate label. A miss is a chance to remember two identities. Closing it while the page is hidden consumes information the player never had an opportunity to observe. A delayed close can cause the opposite problem: the interval is logically over, but input stays blocked until the callback finally runs.

## The misleading first repair

The first patch shortened the callback interval. More frequent callbacks made the drift less visible on a lightly loaded machine but did not correct its cause. If the browser postpones work, ten expected callbacks are not ten observations. No choice of a smaller scheduled delay guarantees that callbacks run on time.

Another attempt updated the visible clock from a start timestamp while retaining the standalone mismatch timeout. This fixed one symptom and left two different notions of active time in the game. Hiding the page could pause the displayed total while the pending close continued, and importing a partly elapsed miss required yet another timer adjustment.

The eventual model used one supplied amount of logical active time for both elapsed total and mismatch remainder. It moved the question “how much time counts?” to the session adapter and left the question “what does this amount change?” in the core. The [clock design](clock-design.md) and [elapsed-time rules](elapsed-time-rules.md) describe that current split.

## What the current core does

`advanceGame(state, milliseconds)` rejects non-finite or non-positive advances and leaves an unstarted or completed game unchanged. Otherwise it adds the supplied amount to `elapsedMs`, clamped to `ELAPSED_MAX_MS`. If selection is a mismatch, it also subtracts that same amount from `remainingMs` and closes the selection when the remainder is no longer positive.

This means a single delayed observation can advance the correct amount. A pending interval does not need to hear every hypothetical intermediate tick. It also means reaching the elapsed cap does not freeze a miss: the accumulated total can stay capped while remaining time continues to decrease.

The core does not ask whether the browser is hidden. It receives active milliseconds from its caller. A replay can therefore supply a logical timeline directly, while the browser can exclude time when play is paused. Keeping visibility out of the core avoids creating a second path through the turn rules.

## What the current browser adapter observes

`browser-clock.ts` stores `observedAt`. `elapsedStep` subtracts it from a new reading and returns a positive finite delta only when active. It always moves the observation anchor to the new reading. `useSessionClock` supplies `performance.now()` and determines activity from visibility, imported pause, whether play started, and whether the game is won.

The normal scheduling interval is 40ms. During a pending miss, the next delay is the smaller of 40ms and the remaining interval. These delays make observation timely, but their numeric values are not added blindly to the game. The actual monotonic readings determine what elapsed.

On a visibility change, the adapter synchronizes using the previous active state before updating its visibility flag. Hiding therefore accounts for the last foreground segment. Showing accounts for no hidden interval and establishes the new basis for active observation. Scheduling then follows the updated state.

An imported unfinished position blocks ticking through the session pause. Resume resets the browser observation, so the time spent reading the imported board is not added when play restarts. Restart and fresh deals likewise reset the observation while replacing the game.

## The fractional edge

The follow-up report involved a much smaller discrepancy. A mismatch started between whole milliseconds, and an implementation rounded every delta down before subtracting it. Repeated truncation could lose fractions and move the close boundary. Rounding the shared elapsed total instead created a different problem: part of the first reveal's fraction could influence a later mismatch's starting interval.

The current live state retains fractions. The browser test uses exact quarter-millisecond readings: a first reveal at 351.25, a second at 362.25, and an export at 363. The mismatch starts from its own second activation, so only 0.75ms of its 800ms interval has passed at export. Near the close boundary, a remaining quarter millisecond still exists in live state.

Serialization handles the whole-number format separately. `encodeSnapshot` floors elapsed time and rounds the pending remainder upward. That last quarter millisecond becomes `remainingMs: 1`, avoiding an exported mismatch with zero time. The [snapshot-selection notes](snapshot-selection-notes.md) describe this boundary in more detail.

## Input can arrive before a late callback

Another edge occurs when a click lands after the logical interval but before the scheduled callback. The current `useGame.activate` calls `clock.synchronize()` before applying `activateSession`. If the interval expired, synchronization closes the old miss first. The same click can then reveal an eligible card instead of being ignored because the last render still showed two faces.

Replay expresses the same ordering directly: advance to the action timestamp, then activate its index. A timeout ending exactly at that timestamp resolves before the action. This is why the correct repair is not merely “make the callback faster.” The rule needs a consistent ordering even when scheduling cannot provide one.

`tests/fractional-clock.e2e.test.ts` places an overdue reading and a card click in the same task, preventing a timer from conveniently running between them. That tests the semantic boundary rather than the speed of the machine. `replay-order.test.ts` covers the logical counterpart.

## Old work needed an identity

The investigation also reproduced starting a new game during a miss. An older timer could otherwise close a selection on the replacement board. The current session increments `generation` whenever `replaceGame` runs. `tickSession` ignores a mismatched generation, and the clock hook checks generation before synchronization and scheduling. Its effect cleanup also clears the timeout and removes the visibility listener.

Cleanup and identity serve different moments. Cleanup prevents unnecessary work; generation checks protect the state boundary when work is no longer relevant. The combination keeps an earlier deal from changing a later one without asking the game core to know about browser callbacks.

## Outcome and evidence limits

In the incident, the final repair replaced callback counting with observed active deltas, unified mismatch and elapsed advancement, and synchronized before actions. Focused core tests cover supplied logical time; browser tests cover visibility, lifecycle, idle behavior, caps, and fractional observations.

The README records successful checks on its named runtime combination. The durable result is a coherent definition of active time that survives delayed tasks and pauses, with rounding confined to display and exchange boundaries. The [current release notes](release-notes-2-2.md) summarize where that behavior is now visible to players.
