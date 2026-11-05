# ADR 033: Show a miss for 800 active milliseconds

This record explains the mismatch observation interval and why it belongs to logical game time.

## Decision

When the second eligible card has a different face, both remain revealed for 800 active milliseconds. All card activation is ignored during that interval, but focus can move. The cards then close together. The interval pauses while the page is hidden or an imported unfinished snapshot awaits Resume.

The chosen duration gives a miss a readable observation window without requiring a separate dismiss action. It is a fixed game value, `MISMATCH_MS = 800`, rather than a CSS animation duration or a browser timeout that independently owns the selection.

## What happens at the second reveal

`activateCard` increments moves and returns a mismatch selection containing `first`, `second` and `remainingMs`. That object describes both the visible positions and the time left. No closure captures the two button elements and later hides them directly. React derives the visible faces from the current selection.

`advanceGame` subtracts active milliseconds from the remaining interval. A positive result retains the mismatch; zero or a negative result produces a closed selection. A single time step can therefore cross the closing boundary without requiring one special callback exactly at 800ms. [Turn resolution](turn-resolution.md) explains the transition sequence.

## Why active time

A player who hides the page has not received useful viewing time while it was hidden. The same active delta drives both elapsed time and remaining mismatch time. Returning to the page continues the remaining interval rather than starting a fresh one or closing immediately because wall time passed.

Snapshot export preserves the remaining interval as a positive whole number. Import pauses unfinished progress, so copying and inspecting the JSON cannot silently consume a short interval. Replay advances logical time explicitly and resolves the close before an activation at the exact boundary. [ADR 014](adr-014-pause-on-hidden.md), [ADR 037](adr-037-snapshot-pause.md) and [ADR 048](adr-048-replay-order.md) cover those contexts.

## Alternatives considered

An immediate close would make a miss hard to inspect, especially with keyboard or screen-reader use. A click-to-dismiss miss would give unlimited observation time but introduce another action and uncertainty about whether the dismissal also revealed the next card. A much longer automatic interval would make repeated misses feel like enforced waiting.

A CSS flip animation could visually hide the card after a delay, but it would not provide a reliable game state for input blocking, export or replay. It would also make reduced-motion handling interact with a rule that should be unchanged. The implementation instead keeps the observation interval in data and uses presentation only to show the resulting state.

## Boundary evidence

The browser clock schedules at most 40ms between observations and shortens its next delay near a mismatch edge. Actual measured time still determines progress. Tests cover 799ms versus 800ms, fractional activation boundaries, hidden intervals and restarting during a pending miss.

The restart case is particularly important: an old timer must not close a newly revealed card in the restarted deal. Session generations invalidate old work, and the current selection supplies what is visible. A callback does not retain authority over the original pair of DOM nodes.

The duration is not a difficulty setting and does not vary with board size. Larger boards increase the memory task through more positions and identities, while turn resolution stays the same. [Mismatch interval](mismatch-interval.md), [Selection state](selection-state.md) and [ADR 008](adr-008-seven-sizes.md) describe those stable boundaries.

## Closing through logical advancement

Both elapsed time and mismatch remaining time use the supplied delta. Crossing zero closes the selection in the same state transition.

```ts
export function advanceGame(state: GameState, milliseconds: number): GameState {
  if (!Number.isFinite(milliseconds) || milliseconds <= 0) return state;
  if (!state.started || isWon(state)) return state;
  const elapsedMs = Math.min(ELAPSED_MAX_MS, state.elapsedMs + milliseconds);
  if (state.selection.kind !== SelectionKind.mismatch) return { ...state, elapsedMs };
  const remainingMs = state.selection.remainingMs - milliseconds;
  const selection =
    remainingMs > 0 ? { ...state.selection, remainingMs } : { kind: SelectionKind.closed };
  return { ...state, elapsedMs, selection };
}
```
