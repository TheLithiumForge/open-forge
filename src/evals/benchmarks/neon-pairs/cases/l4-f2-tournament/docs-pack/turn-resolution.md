# Turn resolution

This page traces how `activateCard` in `src/game/game.ts` handles an eligible second reveal and produces a match or mismatch.

The important transition happens when a one-card selection receives another distinct unmatched position. That activation adds one move, compares the two deck values, and either records the pair immediately or creates an 800-millisecond pending mismatch. Browser session code adds feedback and local record handling around that pure result.

For a compact statement of the contract, see [Rules](rules.md). For the three alternatives themselves, see [Selection state](selection-state.md).

## What enters the core

The board supplies a position index to `useGame.activate`, which synchronizes active time before calling `activateSession`. A paused imported session blocks activation there; otherwise the session calls `activateCard` with the game and index.

The core reads neither a DOM node nor an input device. Pointer and keyboard activation share this position-based transition. Face identity comes from the value at that position, as explained in [Pair identity](pair-identity.md).

## Guards before selection branching

The function returns the existing state for a noninteger index, a negative index, or an index at or beyond `deck.length`. It also returns that same state if `matched` already includes the position.

The integer check excludes fractional numbers, `NaN`, and infinities. Invalid activation does not start the game, add a move, or disturb a first selection. The matched-position check applies regardless of which selection alternative is current.

Returning the same object is observable by the session layer. It uses `game === session.game` to avoid treating an ignored operation as a turn result. A structurally identical fresh object would lose that inexpensive distinction.

There is no independent `isWon` guard at the beginning of `activateCard`. In a valid completed game every position is matched, so all in-range activation is already caught by the matched-position guard. This relies on the state invariants established by normal transitions and snapshot validation.

## First reveal

From closed selection, an eligible activation returns a new game with `started: true` and selection `{ kind: one, index }`. Deck, matched positions, moves, and elapsed time are preserved.

Repeated activation of that first position returns the existing state. A matched card is also ignored before this branch, leaving the first card available for another eligible position. No move is charged until that second position is revealed.

## Second reveal: shared work

In the one-card branch, the function reads the first position from `state.selection.index`. Once it has rejected a repeat of that same position, it calculates:

```ts
const moves = Math.min(MOVES_MAX, state.moves + 1);
```

`MOVES_MAX` is 999,999. This calculation is shared by both outcomes. A successful guess and an unsuccessful guess each complete a two-card attempt and cost one move, until the displayed counter reaches its cap.

The pair comparison is exactly:

```ts
state.deck[first] === state.deck[index]
```

It does not compare row numbers, card DOM identifiers, shape labels, or the order of activation. On a valid deck, each face has two occurrences, and the guards ensure the two eligible positions are distinct and unmatched.

The second reveal has no intermediate returned state where two equal cards remain merely selected. The comparison and resulting progress update are one pure call. A render sees the result of that call.

## Match path

For equal face values, the new game contains:

```ts
{
  ...state,
  moves,
  matched: [...state.matched, first, index],
  selection: { kind: SelectionKind.closed },
}
```

Appending the two indices records their positions as permanently revealed progress. Earlier matched positions retain their order in memory. Selection closes immediately, making another turn eligible without a mismatch wait.

On the example deal `np1-4-1`, positions 2 and 6 both contain face 0:

| Action | Selection | Moves | Matched |
| --- | --- | --- | --- |
| Start | Closed | 0 | Empty |
| Reveal 2 | One at 2 | 0 | Empty |
| Reveal 6 | Closed | 1 | 2, 6 |
| Activate 6 again | Unchanged | 1 | 2, 6 |
| Reveal 0 | One at 0 | 1 | 2, 6 |

The last action begins a new turn. The earlier successful pair stays visible because it is matched, not because the old selection remains open.

The same match path handles the final pair. Once the new `matched.length` equals the deck length, `isWon` reports completion. There is no special final-pair comparison or extra move. See [Win conditions](win-conditions.md).

## Mismatch path

For different values, matched positions remain unchanged and selection becomes:

```ts
{
  kind: SelectionKind.mismatch,
  first,
  second: index,
  remainingMs: MISMATCH_MS,
}
```

`MISMATCH_MS` is 800. Both positions are face up while that selection is current. On the example deal, positions 0 and 1 contain faces 6 and 7, so revealing them from a fresh board gives one move and this mismatch.

Every subsequent activation returns the same state until active-time advancement closes the selection. A third-card activation is ignored rather than queued. Holding a position in a pending input list is not part of this implementation.

`advanceGame` subtracts the supplied positive active duration from `remainingMs`. At the exact boundary it replaces selection with closed. Closing does not add a move and does not record a pair. The next eligible activation begins another one-card selection.

For the example miss:

```text
reveal 0, reveal 1 -> mismatch, moves 1, remaining 800
advance 799       -> mismatch, moves 1, remaining 1
activate 2        -> unchanged
advance 1         -> closed, moves 1
activate 2        -> one at 2, moves 1
```

[Elapsed time rules](elapsed-time-rules.md) explains which browser intervals count as active, and [Mismatch interval](mismatch-interval.md) covers the precise closing boundary.

## Ordering time before activation

The browser synchronizes its clock before processing each activation. This can close an overdue mismatch immediately before the clicked position is evaluated. The result depends on the current logical time, rather than on whether a scheduled render callback happened to run first.

Suppose a mismatch started at monotonic time 362.25 milliseconds. At 1162.00 it still has 0.25 milliseconds remaining, so an activation at that moment is ignored. At 1162.25 or later, synchronization can close it first, after which a valid position begins the next turn.

This also explains an apparent state change during an otherwise ignored click. The click can cause clock synchronization to commit elapsed time or finish a mismatch even when the ensuing card activation is itself ignored. Move accounting remains tied to the core second-reveal rule. An ignored activation adds no move, while time continues to pass in an active game.

The replay path uses analogous ordering. It advances to each action's logical timestamp before activating the index. An interval ending exactly at that timestamp has therefore closed before the action. Equal-time actions retain their array order. The format is covered in [Replay format](replay-format.md).

## What the session adds

`activateSession` recognizes a first reveal because the returned selection is one. It updates the game without adding turn feedback. A completed two-card attempt follows a different path: it determines whether the result is a win, a mismatch, or an ordinary match, and updates the feedback text and announcement counter.

For a nonfinal successful match, feedback includes the number of pairs matched. The first uses singular wording and later results use plural wording. Mismatch feedback says the cards close after a short pause. Completion feedback includes final moves and formatted elapsed time.

A successful final activation also calls `rememberBest` and sets `celebration` to true. Those are session effects derived from a core transition. `activateCard` on its own produces no saved preference update, sound, message, or celebration flag.

A clock tick closing a mismatch goes through `tickSession`, which updates the game but does not create another turn announcement. The feedback can therefore still describe the most recent miss after its two cards have closed. It is the result of the last attempt, not a continuously rewritten description of selection.

## Replacement during an attempt

Game replacement discards the current selection and increments session generation, so an old clock callback cannot close a replacement game's new card. Restart recreates the same hidden code; New Game requests a fresh seed. [New Game and Restart](new-game-and-restart.md) covers the shared replacement path.

## Practical evidence

`game.test.ts` checks known matching positions, repeated-first-card activation, invalid indices, blocking, and exact expiry. `move-cap.test.ts` checks continued turns at the saturated count. `session.test.ts` adds pause, feedback, and completion records.

For browser ordering, `tests/fractional-clock.e2e.test.ts` checks a mismatch begun between whole milliseconds and an overdue click before a timer can run. A known code plus ordered indices and active-time steps is enough to distinguish comparison, counting, and timing defects.
