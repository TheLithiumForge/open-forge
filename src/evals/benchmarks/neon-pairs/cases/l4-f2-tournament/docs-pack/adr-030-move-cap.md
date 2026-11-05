# ADR 030: Saturate the move counter

This record explains the move limit and why reaching it does not stop matching or completion.

## Context

Moves increase on a second eligible reveal. In normal play the count stays small, but imported progress and long-lived sessions make the numeric boundary part of the product. Leaving it unbounded would make storage validation and display behavior less explicit. Treating a high count as game over would add a failure mode unrelated to the goal of finding every pair.

The decision is a saturating counter with `MOVES_MAX = 999_999`. Once the count reaches that value, later completed turns keep the same count. Matches, mismatches and victory continue normally. The limit is a representation boundary, not a turn-eligibility rule.

## Where the count changes

`activateCard` increments moves only in the one-card selection branch when a different unmatched position is selected. It computes the bounded next count before deciding whether the faces match. Both a match and a miss therefore cost one move, while the first reveal, a repeated first-card activation and blocked input cost none.

At the cap, that same code still creates a mismatch or appends a matched pair. There is no branch that returns early merely because the count cannot increase. This is the important distinction to preserve when reviewing an optimization around numeric limits. [Scoring and moves](scoring-and-moves.md) explains the ordinary turn accounting.

## Format and storage boundaries

Snapshot decoding accepts nonnegative safe-integer moves through the cap and rejects larger values. It also checks that moves are at least the number needed for recorded pairs and a pending mismatch. A bounded counter alone would not make a snapshot coherent.

Saved best counts use the same upper limit. The preference parser also checks each score against the minimum pair count for its size. These checks keep imported and stored values within the same numeric range that the game can display and produce. Replay naturally uses the rule function, so it reaches the same saturation behavior without a separate replay counter implementation.

## Why this limit

The chosen six-digit maximum is far beyond an ordinary memory-game result and is explicitly present in the rules and formats. The limit is applied as saturation instead of an error during play.

An unbounded integer would eventually run into JavaScript's safe-integer constraints, far beyond practical interaction but still awkward for untrusted serialized input. Throwing when the counter reached a limit would make an otherwise valid match fail. Wrapping to zero would produce a misleadingly good-looking result and break monotonic progress.

## Evidence

`move-cap.test.ts` and completion tests exercise behavior at the boundary. The significant assertions are not merely that arithmetic returns 999999, but that pairs can still be found and a game can still complete. Snapshot and preference tests cover acceptance at the cap and rejection above it.

The elapsed clock has its own cap and continues to allow play as well. That is a neighboring numerical decision, not a reason to share a generic “maximum score” concept: elapsed milliseconds and turn count have different meanings and validators. [Elapsed time rules](elapsed-time-rules.md) and [ADR 032](adr-032-best-score-scope.md) cover those relationships.

The visible best remains a per-size move record. Reaching a display boundary does not add points, create a new scoring mode or remove the need to finish all pairs. [Win conditions](win-conditions.md) remains the definition of completion.

## The bounded second reveal

The bounded value flows into both match and mismatch results. No cap check suppresses the pair comparison or the resulting transition.

```ts
case SelectionKind.one: {
      const first = state.selection.index;
      if (first === index) return state;
      const moves = Math.min(MOVES_MAX, state.moves + 1);
      if (state.deck[first] === state.deck[index]) {
        return {
          ...state,
          moves,
          matched: [...state.matched, first, index],
          selection: { kind: SelectionKind.closed },
        };
      }
      return {
        ...state,
        moves,
        selection: { kind: SelectionKind.mismatch, first, second: index, remainingMs: MISMATCH_MS },
      };
    }
```
