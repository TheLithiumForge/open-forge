# Game state model

This page describes `GameState` in `src/game/game.ts`, the invariants established by its transitions, and the browser data that deliberately lives outside it.

A game state combines a reproducible board with progress on that board. The board code determines the deck; selection and matched positions determine which faces are visible; moves and active elapsed milliseconds describe the attempt so far. Completion and pair count are derived.

## The declaration

`GameState` extends the `Board` interface from `deck.ts`:

```ts
export interface GameState extends Board {
  readonly matched: readonly number[];
  readonly selection: Selection;
  readonly moves: number;
  readonly elapsedMs: number;
  readonly started: boolean;
}
```

The inherited fields are `boardCode`, `size`, `seed`, and `deck`. All fields are readonly at the type level. The transition functions return a replacement state when something changes and return the original state for a rejected operation.

| Field | Interpretation |
| --- | --- |
| `boardCode` | Valid generation version, side length, and seed |
| `size` | Number of rows and columns |
| `seed` | Initial deterministic generator state |
| `deck` | Face number at each zero-based row-order position |
| `matched` | Positions belonging to found pairs |
| `selection` | Temporary unmatched reveal state |
| `moves` | Completed two-card attempts, capped |
| `elapsedMs` | Accumulated active duration, capped |
| `started` | Whether a valid first reveal has occurred |

There is no `score`, `pairs`, `faceUp`, or `won` field. Each can be obtained from existing data, avoiding another stored value that could disagree after a transition or import.

## Construction

`createGame(boardCode)` first calls `generateBoard`. Invalid codes therefore fail before any usable game is returned. For valid input, it spreads the generated board and supplies:

```ts
matched: []
selection: { kind: SelectionKind.closed }
moves: 0
elapsedMs: 0
started: false
```

Generating the same code creates equivalent starting progress and deck contents. It does not reuse the current matched list or inspect an earlier game. Restart uses this property to recreate the same deal with an empty attempt.

The board and its deck are explicitly frozen in generation. Spreading the board into a game creates a new outer object; it does not carry the outer object's freeze status with it. The deck reference remains frozen. The game, matched arrays, and selection objects rely on readonly typing and immutable update code rather than a recursive runtime freeze.

## Progress is positional

`matched` contains indices, not face values. For the example deal `np1-4-1`, `[2, 6]` means the two positions containing face 0 have been found. `[0]` would mean a single position, not the face whose number is zero, and would not be a valid complete matched pair.

Normal matching appends the two selected positions. Since matched positions are ineligible for activation, they cannot be appended again through normal play. Matched length stays even and grows by two at a time.

Selection indices also refer to positions and are absent from matched. A one-card selection contains one eligible position; a mismatch contains two distinct eligible positions with different face values. Matching temporary cards are moved to matched immediately instead of being retained as a mismatch.

The full union is covered in [Selection state](selection-state.md). The numeric identity carried by the deck is covered in [Pair identity](pair-identity.md).

## Three public state-changing operations

The core's main entry points are deliberately direct:

- `createGame(code)` creates the hidden starting state.
- `activateCard(state, index)` applies one position activation.
- `advanceGame(state, milliseconds)` applies one positive active-time step.

Activation checks its index and matched eligibility first, then branches on selection. From closed it reveals one card and sets started. From one it ignores a repeated position or resolves the second distinct reveal. From mismatch it returns the original state.

Time advancement rejects nonfinite or nonpositive durations. It also rejects advances before start or after completion. Otherwise it saturates elapsed time at `ELAPSED_MAX_MS` and, when a mismatch exists, subtracts the duration from its remainder. Expiry replaces selection with closed.

The operations do not mutate their arguments or call the browser. A retained earlier state can be compared with the result after another operation. The functions receive the time and activation explicitly, which is also how replay can use them without React.

## Bounds and logical relationships

`MOVES_MAX` is 999,999, `ELAPSED_MAX_MS` is 359,999,999, and `MISMATCH_MS` is 800. Activation and advancement enforce these where values change.

An unstarted game has zero counters, no matches, and closed selection. Started remains true after its first reveal, even while hidden, imported and paused, or complete. It is not a clock-running flag.

A started zero-move game has one card revealed. Moves are at least matched pairs plus one for a pending mismatch; earlier misses explain larger counts. No positive time per move is required. Live timing may be fractional, with whole-millisecond conversion at serialization.

## Derived views

`isWon` compares matched length with deck length. In a valid game that means every position is part of a found pair. It does not scan the deck to reconstruct a historical sequence of attempts.

`faceUpCards` creates a set from matched positions and adds the temporary selection positions. It returns a `ReadonlySet<number>`. It is a derived collection used for rendering, not a field persisted in progress.

`GameStatus` divides matched length and deck length by two for the pair display. It formats elapsed time through `formatElapsed` and receives the best score from preferences as a separate prop. The best is not part of `GameState`.

The board component memoizes face-up and matched membership collections. This is useful because an ordinary elapsed tick leaves matched and selection references unchanged. The game object can change its elapsed value without requiring new card identities or matched membership to be calculated from scratch.

## Object identity as a no-op signal

Ignored activation and rejected time advancement return the same state reference. `activateSession` uses this to return the same session for ignored activation. `tickSession` likewise preserves its session when the game did not change. `useGame.commit` checks whether the next session is already current before setting React state.

A successful state update can preserve most nested references. For example, a time advance outside mismatch returns `{ ...state, elapsedMs }` and retains both the deck and matched array. A first reveal replaces selection but retains matched. A match replaces both matched and selection.

This is not a deep-equality comparison of all data. It is a consequence of the known transition paths. External mutation would invalidate those assumptions, which is why the update code consistently constructs replacements.

## External progress is validated first

Snapshots store version, board code, matched positions, selection, moves, elapsed time, and started. Size, seed, and deck are regenerated from the code; completion and visible faces are derived.

`decodeSnapshot` validates unique in-range matched positions containing whole face pairs, available selection indices, counter bounds, and progress relationships before replacement. It checks a consistent position without proving the history of earlier guesses. See [Snapshot format](snapshot-format.md).

Export sorts matched positions, floors elapsed milliseconds, and rounds a positive mismatch remainder upward. These external representation choices do not replace the live state.

## The containing browser session

`Session` in `src/game/session/session.ts` wraps the game with `paused`, `generation`, `feedback`, `loadNotice`, `announcement`, `celebration`, and `preferences`.

Pause is for unfinished imported progress. Generation changes on replacement and invalidates old callbacks. Feedback and announcement represent turn-result communication. Celebration distinguishes a newly played win from an already complete import. Preferences hold selected size and per-size best records.

None of these is needed to compare two card faces. Keeping them outside `GameState` lets replay and core tests apply the same rules without a storage object or a DOM. The clock hook also keeps its observation timestamp and page visibility outside game state.

A hidden page is therefore not represented by a new game selection or a changed started flag. The browser simply stops supplying active duration. Resume resets the clock observation point and clears the session pause without replacing the restored game.

## Inspection order

Check code and deck length, then matched positions and selection, then moves and started. If time or input remains frozen, inspect session pause and visibility. This follows the data dependencies: a different board explains different faces, while a paused session can explain blocked input without a faulty core transition.
