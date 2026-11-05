# ADR 048: Advance time before each replay action

This record explains stable equal-time actions and timeout resolution at an exact replay boundary.

## Decision

Replay actions are processed in their array order, with logical time advanced to each action's `atMs` before activating its card. Times must be nondecreasing. Equal timestamps are valid and do not trigger sorting or deduplication. After the final action, time advances to `endMs`.

This defines the boundary between a mismatch closing and a new reveal. If a mismatch expires exactly at an action time, the close happens first. The action then sees a closed selection and can reveal a card. Reversing the order would cause that activation to be ignored as if it had arrived during the mismatch.

## The loop as the contract

`readReplay` starts from `createGame(boardCode)` with `previousTime = 0`. For each validated action, it computes the difference from the previous timestamp, calls `advanceGame` and passes the resulting state to `activateCard`. The nesting in the source makes the order visible in one expression.

A zero delta leaves time unchanged but does not suppress the action. Two equal-time indices can reveal a matching pair in a single timestamp. Three equal-time indices still execute sequentially; the third observes whatever selection the first two produced. If that is a mismatch, the normal ignored-activation rule applies.

## Timeline versus elapsed time

Replay timestamps are logical positions in a trace. The game's elapsed clock starts at its first valid reveal, so waiting before that action does not become elapsed play. After completion, later timeline advancement does not extend the completed game's time. The replay can therefore have a large `endMs` without implying equal elapsed milliseconds in the final snapshot.

Times use nonnegative safe integers. The game's elapsed value has its separate cap of 359999999ms. The parser does not shorten the accepted timestamp range to that cap. This distinction is important when diagnosing a replay with a long pre-start interval or long post-completion tail. [Replay format](replay-format.md) explains the fields.

## Validation before execution

The parser rejects timestamps that go backward or exceed `endMs`, malformed action objects and out-of-range indices. An in-range activation can still be ignored under ordinary game rules; that is valid replay behavior rather than malformed input. Validation establishes a valid instruction sequence, not that every instruction changes state.

Exact object fields and bounded action count keep the accepted format explicit. The CLI reports per-action failures with a one-based position for a person reading the file, while the internal parser tracks the zero-based array position. [ADR 015](adr-015-replay-cli.md) covers the wrapper.

## Alternatives

Sorting actions by time would conceal invalid order and could change equal-time semantics. Resolving time after activation would create an off-by-one event ordering at mismatch boundaries. Treating simultaneous actions as an unordered set would leave first and second reveal undefined. The selected loop avoids all three ambiguities.

Focused replay tests cover boundary times, equal-time order, ignored actions, caps and final advancement. Independent examples let another implementation verify the same semantics without a browser. Browser clock-edge tests then check that its adapter synchronizes before real activation as well. [ADR 013](adr-013-monotonic-clock.md), [ADR 021](adr-021-seeded-decks.md) and [Turn resolution](turn-resolution.md) explain those complementary contracts.

## The sequential replay execution

The action loop preserves the submitted order. Advance-before-activate is explicit, and final advancement uses the same rule function.

```ts
let state = createGame(value.boardCode);
  let previousTime = 0;
  for (const [position, action] of value.actions.entries()) {
    if (
      !isJsonObject(action) ||
      !hasExactKeys(action, ACTION_KEYS) ||
      !isNonnegativeSafeInteger(action.atMs) ||
      !isNonnegativeSafeInteger(action.index)
    )
      return { problem: ReplayProblem.action, action: position };
    if (action.atMs < previousTime || action.atMs > value.endMs)
      return { problem: ReplayProblem.order, action: position };
    if (action.index >= state.deck.length)
      return { problem: ReplayProblem.index, action: position };
    state = activateCard(advanceGame(state, action.atMs - previousTime), action.index);
    previousTime = action.atMs;
  }
  return { state: advanceGame(state, value.endMs - previousTime) };
```
