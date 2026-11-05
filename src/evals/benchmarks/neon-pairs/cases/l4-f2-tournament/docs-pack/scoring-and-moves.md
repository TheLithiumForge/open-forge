# Scoring and moves

This page explains the pair score, the move counter, and the saturating move limit in `src/game/game.ts`.

Neon Pairs gives one point of progress per matched pair. Moves count completed two-card attempts. These quantities answer different questions: pairs show how much of the board is finished, and moves show how many attempts were used. Elapsed time is displayed alongside them but does not alter either value or decide the saved best.

The short contract is in [Rules](rules.md). Record comparison and persistence are covered separately in [Best score records](best-score-records.md).

## Where the numbers come from

`GameState` stores `moves` and an array of matched position indices. It does not store a separate numeric pair score. `GameStatus.tsx` derives the progress display as:

```text
matched pairs = game.matched.length / 2
total pairs   = game.deck.length / 2
```

A successful comparison appends two positions, so the displayed pair count increases by one. A miss appends none. A first reveal also appends none. The denominator is fixed by the board size for the life of that game.

A size-4 board contains 16 cards and 8 pairs. After matching positions 2 and 6 of `np1-4-1`, the status shows `Moves: 1` and `Pairs: 1/8`. A later miss raises moves to 2 while pairs remain 1/8.

No bonus is attached to a fast guess, consecutive matches, a large board, a first try, or a particular face. The pair count is enough to express progress. A saved best is the fewest completed attempts for that size, not the pair total, which is identical for every completed game of that size.

## Exactly when a move is charged

In `activateCard`, the increment appears in the one-card branch after the repeated-index check:

```ts
const first = state.selection.index;
if (first === index) return state;
const moves = Math.min(MOVES_MAX, state.moves + 1);
```

The function has already excluded invalid indices and matched positions. Reaching this calculation therefore means a second distinct eligible card has been revealed. The match and mismatch branches both use the same calculated value.

This placement makes counting independent of the comparison outcome. It also means the player is charged at the reveal, not when a mismatch later closes. During the 800-millisecond mismatch interval, the attempt is already included in moves.

| Operation | Move effect |
| --- | --- |
| First valid reveal from closed selection | None |
| Activate the same first position again | None |
| Reveal a different unmatched position | Add one, subject to cap |
| Activate a matched position | None |
| Activate during a pending mismatch | None |
| Advance time until a mismatch closes | None |
| Move keyboard focus | None |
| Start or restart a game | Reset the game counter to zero |

An imported unfinished session also blocks activation before the core is called. It does not charge moves for attempts to activate cards while paused. Resume itself changes the pause state, with no move increment.

## A worked sequence

Use the fixed deck `np1-4-1` to distinguish attempts from individual reveals:

```text
reveal 2          one card, 0 moves, 0 pairs
reveal 2 again    unchanged
reveal 6          match, 1 move, 1 pair
activate 6        unchanged, because 6 is matched
reveal 0          one card, still 1 move
reveal 1          miss, 2 moves, still 1 pair
activate 3        ignored while mismatch remains
advance 800 ms    mismatch closes, still 2 moves
reveal 0          one card, still 2 moves
reveal 11         match, 3 moves, 2 pairs
```

The two completed matches and one miss total three attempts. There are more activation calls because some were first reveals or no-ops. Counting event handlers, clicks, or key presses would not reproduce this number.

An ignored browser activation can still synchronize elapsed time first. That does not make it a move. Time advances and card activation are separate operations in the controller; [Turn resolution](turn-resolution.md) explains their ordering.

## Theoretical minimum

Every completed pair needs one successful second reveal. The lowest possible move count is therefore the number of pairs:

| Size | Minimum completed moves |
| --- | ---: |
| 4x4 | 8 |
| 6x6 | 18 |
| 8x8 | 32 |
| 10x10 | 50 |
| 12x12 | 72 |
| 14x14 | 98 |
| 16x16 | 128 |

The completion test groups positions by face and reveals each known pair consecutively. It reaches exactly these minima for every supported size. This is an oracle-driven rules check.

There is no par, grade, percentage, or efficiency multiplier in the current state or status component. A maintainer can calculate excess attempts as moves minus pairs for an ordinary unsaturated game, but the application does not keep that as another score field.

Even that calculation has a boundary: after the move counter saturates, it no longer reports the full number of attempts. The cap is a limit on the stored counter, not on how many turns the player can take.

## Saturation at 999,999

`MOVES_MAX` is `999_999`. A second reveal at 999,998 produces 999,999. A later second reveal keeps the value 999,999. The implementation uses `Math.min`, so ordinary progression does not wrap to zero or grow beyond the limit.

Reaching the cap does not end a game. Matching, mismatching, the pending interval, elapsed time, snapshot export, and eventual victory continue to work. A capped game can still have a first selection, a mismatch, or a closed selection between attempts.

For a match at the cap, both positions are appended to `matched` exactly as usual. For a miss, selection gets the same 800-millisecond initial remainder. Neither path substitutes an error or disables further input because of the move count.

A completed capped game can establish a record for a size that has none. It cannot beat a lower existing record. Equality retains the previous record. The stored score is the capped count; there is no hidden larger attempt counter used for comparison.

`src/game/move-cap.test.ts` checks reaching the cap with a match, finishing the remaining pairs, exporting and restoring completion, and resolving a capped mismatch. These tests address continued play directly rather than merely asserting the constant's value.

## Time and moves are independent

`advanceGame` never changes moves. `activateCard` does not add elapsed duration. A logical replay can match several pairs at the same timestamp, while a visible player can spend a long time with one card revealed and still have zero moves.

The two caps are independent. A saturated move counter does not stop active time, and saturated elapsed time does not stop second reveals from being counted. Completion depends on found pairs. See [Elapsed time rules](elapsed-time-rules.md).

## Imported counts and consistency

`decodeSnapshot` accepts a nonnegative safe integer move count no greater than `MOVES_MAX`. It also checks a lower bound based on the imported progress:

```text
minimum = matched positions / 2
        + 1 if a mismatch is pending
```

A pending mismatch has already consumed its second reveal, which explains the additional move. A one-card selection needs no extra completed attempt. A snapshot with one matched pair and a mismatch therefore needs at least two moves.

The decoder does not reconstruct every earlier guess. A large valid move count with little current progress can represent repeated misses. That is legitimate within the format's state checks. Validation establishes consistency, not an independently verified play history.

An unstarted snapshot requires zero moves, zero elapsed time, no matches, and closed selection. A started zero-move state cannot be closed; it has one revealed card under the other consistency checks. [Game state model](game-state-model.md) explains these cross-field relationships.

Saved best records use the same maximum. They also have a size-specific minimum equal to the total pair count, because a record represents a completed game rather than partial progress. A value of 1 for a size-4 best is invalid, even though 1 is a valid move count during that board's play.

## Reading a report about a wrong score

Identify the affected value first. Moves are stored by turn transitions; pairs are derived from matched positions; best comes from preferences. An uncounted activation may be a repeated position, matched position, blocked mismatch input, or saturated count. A pair display increasing by two suggests matched positions were shown without division by two.

For best lookup, `Game.tsx` uses `bestMoves[String(game.size)]`. A size change can select another record without creating a new score.
