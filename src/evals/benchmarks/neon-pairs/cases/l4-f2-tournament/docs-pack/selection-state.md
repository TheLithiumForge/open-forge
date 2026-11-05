# Selection state

This page describes the `Selection` union in `src/game/selection.ts` and how it represents temporarily revealed unmatched cards.

Selection is a small part of `GameState`, with three alternatives: `closed`, `one`, and `mismatch`. It does not contain matched cards, an independent pause flag, or a separate winning state. Those distinctions explain several behaviors that can otherwise look surprising in a rendered board.

[Game state model](game-state-model.md) covers the containing object. [Turn resolution](turn-resolution.md) follows the activation function line by line.

## The union as written

`SelectionKind` exports string constants for the three discriminants. The type uses the values of those constants:

```ts
export type Selection =
  | { readonly kind: typeof SelectionKind.closed }
  | { readonly kind: typeof SelectionKind.one; readonly index: number }
  | {
      readonly kind: typeof SelectionKind.mismatch;
      readonly first: number;
      readonly second: number;
      readonly remainingMs: number;
    };
```

The discriminant determines which other fields exist. A closed selection has no card index. A one-card selection has one index. A mismatch has two indices and a remaining duration. The type does not represent a generic array of selected positions with a separate count.

This shape lets `activateCard`, `faceUpCards`, and the snapshot reader branch on one field. In the activation function, the default branch calls `unreachable` with a `never` parameter. Exhaustiveness is tied to the union rather than to an informal assumption that there are only three strings.

The fields are readonly in TypeScript. State transitions construct replacement objects. They do not decrement `remainingMs` or rewrite `kind` on an existing selection. Keeping an earlier state reference therefore gives a stable account of that earlier turn when the public transition functions are used.

## Closed means no temporary reveal

`createGame` begins with `{ kind: SelectionKind.closed }`. A fresh game also has no matched positions, so every card is face down.

The same selection shape is used after a successful match. At that point some cards remain face up, but they are in `matched`, not in selection. A closed selection also follows the expiry of a mismatch. It can therefore describe three visibly different situations:

| Situation | Selection | Matched positions |
| --- | --- | --- |
| Fresh deal | `closed` | Empty |
| Waiting after one or more matches | `closed` | Previously found pairs |
| Completed game | `closed` | Every position |

It is incorrect to treat `closed` as equivalent to a fresh board or to an idle clock. A started, unfinished game keeps accruing active time while the selection is closed. A completed game stops accruing it because `isWon` is true.

There is no deadline attached to closed selection. Waiting before a first reveal leaves the game unstarted; waiting between turns of a started game adds elapsed time. The `started` flag, completion result, and browser session determine that distinction.

## One card stays revealed

An eligible activation from closed selection returns a new game with `started: true` and `{ kind: one, index }`. No move is added yet. The index identifies a position in the current deck.

Activating that same index again returns the original state. Activating a matched position is ignored before the switch is entered, so it does not act as the second card. Out-of-range or noninteger indices are ignored too. These operations leave the one-card selection available for the next eligible activation.

For the published `np1-4-1` deal:

```text
activate 2  -> one(index=2), moves=0
activate 2  -> same state
activate 6  -> closed, matched=[2,6], moves=1
```

The deck values at 2 and 6 are both 0. If position 0 is chosen instead of 6, the values differ and selection becomes a mismatch with first position 2 and second position 0.

A one-card selection does not expire on its own. `advanceGame` updates elapsed time but leaves this selection in place. There is no automatic close after the mismatch duration, because that duration belongs only to the mismatch alternative. A player can study one revealed face while searching for its mate, with active time continuing to accumulate.

## A mismatch contains the pending work

A second eligible position with a different face produces a mismatch holding `first`, `second`, and `remainingMs: MISMATCH_MS`. The constant is 800. The order records the activation sequence; both positions remain equally visible.

While this alternative is current, activation is ignored. It cannot replace an index, extend the interval, or charge another move. `advanceGame` subtracts active duration, retaining mismatch only while the remainder is positive. At zero or below, selection becomes closed. No normal result stores a zero-duration mismatch.

Closing adds no move. [Mismatch interval](mismatch-interval.md) covers the exact arithmetic, pauses, and browser ordering.

## A successful match has no fourth alternative

Equal deck values move both positions into `matched` and close selection immediately. There is no temporary matching alternative or 800-millisecond wait before the next turn.

The final match uses that same transition. Completion is derived from full matched coverage rather than another selection kind; see [Win conditions](win-conditions.md).

## Selection and visible cards

`faceUpCards` constructs a set beginning with the matched positions. It then adds the one-card index or both mismatch indices, depending on the current alternative. Closed adds nothing. This gives the board one derived collection for visible faces.

For a game with `matched: [2, 6]`:

| Selection | Visible positions |
| --- | --- |
| `closed` | 2, 6 |
| `one`, index 0 | 0, 2, 6 |
| `mismatch`, first 0, second 1 | 0, 1, 2, 6 |

The set is derived; it is not another progress field that has to be kept in sync. `Board.tsx` memoizes the result using `matched` and `selection` as dependencies. A clock advance outside a mismatch retains both references, so it can reuse the visibility set. During a mismatch, remaining time changes selection even though the same two faces remain exposed.

`Card.tsx` receives a face number only if that position is in the face-up set. Otherwise it receives `null`. Selection is therefore part of deciding what identity reaches the card's rendered markup, as well as which CSS state appears. [Pair identity](pair-identity.md) discusses that boundary.

## Pause is outside this type

An unfinished imported snapshot retains its selection while the containing `Session` sets `paused`. `activateSession` and `tickSession` return early while paused. Resume clears that gate without reconstructing the selected positions. Page visibility is another clock gate outside selection.

Thus a loaded mismatch with 300 milliseconds remaining still needs those 300 active milliseconds after Resume. A hidden page preserves its remainder too. The core applies whatever active delta it receives; [Mismatch interval](mismatch-interval.md) explains the browser's accounting.

## Decoding progress

Parsed JSON needs runtime checks. `readSelection` in `snapshot-selection.ts` checks each alternative's exact fields. A one-card index must be a nonnegative safe integer in range and absent from matched. Mismatch requires two such distinct indices with different face values and an integer remainder from 1 through 800. Closed has only its discriminant.

These checks prevent a matched position from also being temporary selection, or a matching pair from masquerading as a miss. Additional moves/started relationships are validated by the containing decoder; the union alone does not express them. See [Snapshot selection notes](snapshot-selection-notes.md).

## Fractions and exported durations

Live `remainingMs` can be fractional. `encodeSnapshot` rounds a positive remainder upward, so 0.25 milliseconds exports as 1 and remains a pending mismatch. Decode accepts the whole-millisecond representation. This boundary conversion does not round the live selection after every tick.

## Reading a revealed-card report

Inspect selection alongside matched positions. One has no expiry; mismatch needs active duration to close; closed can still show matched faces. `game.test.ts` checks all three visibility cases and the exact mismatch boundary, while snapshot-selection tests check malformed external alternatives.
