# ADR 010: Save progress as explicit JSON

This record explains the JSON snapshot format, its validation boundary and its relationship to a board code.

## Decision

Export progress as readable version 1 JSON. The snapshot contains `version`, `boardCode`, `matched`, `selection`, `moves`, `elapsedMs` and `started`. It derives the deck from the code and derives completion from matched positions. There is no second serialized deck, pair total or win flag to reconcile with the underlying data.

JSON was selected because both browser and Node tools can read it, a person can inspect it during a bug report, and copying it from a textarea needs no additional file or clipboard API. The export uses two-space indentation. Formatting makes a report easier to read, while validation relies on values rather than indentation.

## Coherent state, not arbitrary fields

`decodeSnapshot` treats parsed data as unknown. It checks the complete shape, version, code, bounded integer counters, matched positions and selection. Extra keys are rejected. Matched positions must be unique, in range and form whole face pairs. A pending mismatch needs two different unmatched positions whose faces differ, with remaining time from 1 through 800 whole milliseconds.

The decoder also checks relationships. Moves must cover the matched pairs and a pending mismatch. An unstarted game has no progress. A started game with zero moves needs its first revealed card. These checks prevent combinations that contradict the current state rules without requiring an exhaustive reconstruction of earlier play. [Snapshot selection notes](snapshot-selection-notes.md) explains the selection cases.

## Representation details

Positions are zero-based row-order indices. Export sorts matched positions so equivalent sets have a stable readable ordering. Active time can retain fractions in memory; export floors elapsed milliseconds and rounds pending mismatch time upward. Rounding the remaining interval upward keeps a still-open mismatch from becoming a zero-duration serialized state.

The UTF-8 snapshot limit is 65536 bytes. The textarea also has a character limit, but the parser's byte check is the format boundary. This matters because a character count and a UTF-8 byte count are not interchangeable in general. `TextEncoder` supplies the byte measurement in the shared format helper.

## Import as a session action

Validation completes before the controller replaces the current game. Invalid text leaves the active game and draft available. Valid unfinished progress enters a paused session until Resume. That gives the player time to inspect what loaded instead of consuming a short remaining mismatch interval during the act of importing. Completed progress follows its completed presentation path. [ADR 037](adr-037-snapshot-pause.md) explains the pause choice.

Snapshot state does not include the board's remembered focus, the open state of the reproduction disclosure or a textarea draft. Those are presentation concerns. The reconstructed board starts with its ordinary focus memory, and the session controls retain their documented focus behavior.

## Alternatives and consequences

Serializing the entire internal session object would have exported browser bookkeeping such as the generation counter and announcement sequence. It would also have made implementation refactors into accidental format changes. Saving only a board code would have been smaller but could not preserve moves or a half-finished turn.

A binary format could reduce size at the expense of inspectability and implementation complexity. For this bounded state, explicit JSON is already compact enough and easier to diagnose. The shared encoder is also the replay CLI's output, giving that tool a familiar final-state format. See [Snapshot format](snapshot-format.md), [ADR 048](adr-048-replay-order.md) and [Data exchange policy](data-exchange-policy.md).

## The exported representation

The encoder names its public fields explicitly instead of stringifying the entire GameState object.

```ts
export const SNAPSHOT_VERSION = 1;
export const SNAPSHOT_MAX_BYTES = 65_536;
export const SNAPSHOT_KEYS = [
  'version',
  'boardCode',
  'matched',
  'selection',
  'moves',
  'elapsedMs',
  'started',
] as const;

export function encodeSnapshot(state: GameState): string {
  return JSON.stringify(
    {
      version: SNAPSHOT_VERSION,
      boardCode: state.boardCode,
      matched: [...state.matched].sort((left, right) => left - right),
      // Active time keeps its fractions in play. Export writes whole milliseconds, and rounding
      // pending time up keeps a mismatch at 1ms or more.
      selection:
        state.selection.kind === SelectionKind.mismatch
          ? { ...state.selection, remainingMs: Math.ceil(state.selection.remainingMs) }
          : state.selection,
      moves: state.moves,
      elapsedMs: Math.floor(state.elapsedMs),
      started: state.started,
    },
    null,
    2,
  );
}
```
