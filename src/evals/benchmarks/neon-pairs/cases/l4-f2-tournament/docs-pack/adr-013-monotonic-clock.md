# ADR 013: Measure active time with a monotonic clock

This record explains measured browser deltas, fractional milliseconds and the separation from logical game time.

## Decision

Use `performance.now()` in the browser session adapter and pass measured positive deltas to the game. The game itself advances through `advanceGame(state, milliseconds)`. Replay provides its own logical deltas. Browser observation and rule execution therefore share units without sharing a clock API.

The timer is an opportunity to observe time, not a declaration that a fixed amount has elapsed. `CLOCK_INTERVAL_MS` is 40, but callbacks may run late. Adding 40 on every callback would make the game run slowly under load and would couple recorded time to rendering frequency. The adapter instead subtracts successive observations.

## Why monotonic observations

The useful question is how much active time passed between two observations, not what civil time the machine currently reports. A monotonic source is suited to that interval measurement. The browser adapter keeps an `observedAt` value in `ActiveClock`; `elapsedStep` derives the delta and always returns a new observation baseline.

The helper credits time only when activity is enabled and the delta is positive and finite. Waiting before the first reveal, a hidden document and an imported pause can update the baseline without increasing active time. This prevents the next active observation from including the whole preceding inactive interval. [Clock design](clock-design.md) follows the adapter in detail.

## Fractions matter

`performance.now()` can report fractions of milliseconds. The clock retains those fractions in game state instead of rounding each observation. Rounding every 40ms sample would accumulate an error dependent on sample timing. It would also make a mismatch close slightly early or late if the second activation occurred between whole milliseconds.

Whole-millisecond representation belongs at export. The snapshot encoder floors elapsed time and rounds pending mismatch time upward. In memory, the mismatch begins at its actual activation boundary. The browser checks include fractional starts and exact closing edges because an apparently harmless early rounding can change whether another card activation is accepted.

## Synchronization at actions

The hook synchronizes the clock before card activation and before exporting. Rendered state can trail the latest browser observation by a scheduled tick. An action therefore must not assume that the last rendered elapsed value is the current active time. Synchronizing first also lets an interval expire before an action arriving at its closing boundary.

The session timer schedules the smaller of 40ms and a mismatch's remaining time. This improves observation near a closing edge without making scheduling the source of truth. A late timeout still advances by the measured delta. The same distinction is why replay can run without waiting for actual browser time.

## Alternatives and costs

Counting callbacks was simpler but incorrect under throttling. Measuring only on user actions would preserve some intervals but leave the displayed clock and mismatch closure stale during idle viewing. A permanently running animation loop would do unnecessary work before play, during pauses and after completion.

The chosen adapter has lifecycle complexity: it needs visibility handling, cleanup and a generation check. That complexity is contained in the session directory and covered by focused and browser tests. The numeric game cap remains independent of the observation mechanism. [ADR 014](adr-014-pause-on-hidden.md), [ADR 048](adr-048-replay-order.md) and [Elapsed time rules](elapsed-time-rules.md) cover those related boundaries.

## Measured clock steps

The helper has no timer of its own. Its callers supply the observation and activity decision, making frequency independence straightforward to test.

```ts
export const CLOCK_INTERVAL_MS = 40;

interface ActiveClock {
  readonly observedAt: number;
}

export function createClock(now: number): ActiveClock {
  return { observedAt: now };
}

// Fractions stay in the game, so a mismatch runs from its own activation and only exports round.
export function elapsedStep(clock: ActiveClock, now: number, active: boolean) {
  const delta = now - clock.observedAt;
  const milliseconds = active && Number.isFinite(delta) && delta > 0 ? delta : 0;
  return { clock: createClock(now), milliseconds };
}
```
