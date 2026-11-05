# Deck generation

This page explains how `src/game/deck.ts` turns a board code into a repeatable deck, and how the browser chooses a seed for a fresh deal.

The deal is determined by its code. Generation does not depend on the order in which cards are revealed, the current move count, or the browser's rendering schedule. `generateBoard` receives a string and returns a board description with a frozen deck. The browser's seed selection happens separately in `src/game/session/fresh-board.ts`.

For the player-facing rules, see [Rules](rules.md). The complete external representation is documented in [Board code format](board-code-format.md); this page follows the implementation of generation.

## Inputs and the returned board

`GRID_SIZES` is the tuple `[4, 6, 8, 10, 12, 14, 16]`. A size is the side length of a square, so size 6 means 36 positions. `isValidSize` checks membership in this tuple rather than accepting every positive even number. The relationship between size and face count is described in [Board size catalogue](board-size-catalogue.md).

The `Board` interface contains four readonly fields:

| Field | Meaning |
| --- | --- |
| `boardCode` | The exact valid code used to generate the board |
| `size` | Supported side length |
| `seed` | Initial generator state |
| `deck` | Position-ordered face numbers |

`parseBoardCode` accepts the `np1-N-seed` structure. Both number components start with a nonzero decimal digit and continue with decimal digits. The size has to be supported. The seed has to be a safe integer no greater than `SEED_MAX`, which is `2_147_483_646`. The syntax rules also exclude zero and negative seeds.

The pure parser receives the code exactly as supplied. It does not trim spaces. The browser's `loadBoard` trims the text before calling the parser. This explains why surrounding spaces in a control draft can be accepted even though passing the same untrimmed text directly to `parseBoardCode` returns `null`.

`boardCode(size, seed)` constructs a string and validates it with that same parser. Invalid arguments produce `Error('Invalid board code.')`. `generateBoard` also throws that error if parsing fails. Callers that handle untrusted text first use parsing or decoding to avoid replacing a working game with invalid input.

## Build the pairs before shuffling

Generation begins with:

```ts
Array.from({ length: parsed.size ** 2 }, (_, index) => Math.floor(index / 2))
```

For size 4, this creates:

```text
0 0 1 1 2 2 3 3 4 4 5 5 6 6 7 7
```

Every face is present twice before any generator draw occurs. Shuffling exchanges positions; it cannot add or remove a face. This construction accounts for the complete-pair property without a later repair pass.

The largest board has 256 positions and face values 0 through 127. The generator does not randomly sample from an artwork catalogue. Smaller boards always use a prefix of the face-number range, then shuffle that prefix's two copies. As a result, changing the seed changes locations, while changing the size changes the available face range as well. [Pair identity](pair-identity.md) explains how those numbers become artwork.

An index into `deck` names a position in row order. On a size-4 board, indices 0 through 3 are the first row, and 4 through 7 are the second. The value at an index identifies the face at that position. These are different numbers with different jobs: index 6 need not contain face 6.

## Generator state

The generator uses the Park–Miller recurrence:

```text
modulus    = 2147483647
multiplier = 16807
next       = (state * multiplier) % modulus
```

The code defines `PRNG_MODULUS` as `SEED_MAX + 1` and keeps the multiplier in `PRNG_MULTIPLIER`. `nextRandomState` implements the recurrence directly. `generateBoard` starts its local `state` at the parsed seed and updates it on every draw, including draws later rejected by the bounded sampler.

Starting with seed 1, the first five states are:

```text
16807
282475249
1622650073
984943658
1144108930
```

These are generator states, not five positions in the final deck. The shuffle converts each state into a bounded position and makes a swap; the bounded conversion can consume additional states.

JavaScript's number representation can express every integer product used here exactly. Even the largest valid state multiplied by 16807 is below the safe-integer ceiling. The multiplication and remainder therefore retain the integer behavior required by the specification. This matters when comparing the implementation to an independent exact-arithmetic deal oracle.

`nextRandomState` itself is a small arithmetic function, not an input validator. The supported seed domain is established by parsing before `generateBoard` starts. Passing arbitrary numbers to the exported recurrence does not expand the set of accepted board codes.

## Descending shuffle and bounded draws

The loop visits the last deck index first and stops after index 1. At index `i`, it chooses a position from 0 through `i`, inclusive, then exchanges that position with `i`. Already processed positions above `i` are left alone by later iterations.

There are `deck.length - 1` accepted bounded draws. There can be more generator advances because rejection consumes a state without performing a swap. The accepted result is allowed to equal `i`; exchanging a position with itself is part of the shuffle.

The bounded conversion is:

```text
bound = i + 1
limit = floor(2147483646 / bound) * bound
repeat:
    state = nextRandomState(state)
    draw = state - 1
until draw < limit
other = draw % bound
```

The subtraction maps the nonzero generator states into a zero-based range with 2,147,483,646 possible values. The limit keeps a prefix whose length is divisible by the number of possible destinations. Inside that prefix, every remainder occurs equally often. Values at or above the limit are skipped.

For the first swap on a 16x16 board, `bound` is 256. The limit is 2,147,483,392. A draw equal to that value is rejected, just as a larger draw is. Replacing the comparison with a strict greater-than comparison would change the boundary case and the resulting deal for some seeds.

For the first swap of the published size-4, seed-1 deal, `bound` is 16. The first state is 16807, giving `draw = 16806` and destination 6. The generator exchanges positions 15 and 6 of the initial paired sequence. Further swaps continue down the deck; that one exchange is not enough to infer the final position of either face.

The implementation reads both face values before assigning either position. It checks for `undefined` and throws `Error('Deck index out of range.')` if the expected values are absent. With a generated deck and bounded destination, the normal loop stays within range.

## Published deal

The checked example code is `np1-4-1`. Its deck is:

```text
[6, 7, 0, 5, 2, 2, 0, 5, 3, 7, 1, 6, 4, 4, 1, 3]
```

Shown in board rows:

```text
6 7 0 5
2 2 0 5
3 7 1 6
4 4 1 3
```

Face 0 appears at positions 2 and 6. Face 6 appears at positions 0 and 11. Revealing positions 0 and 1 produces a mismatch because their values are 6 and 7. These facts support examples throughout [Turn resolution](turn-resolution.md).

The literal deck is useful evidence. A test that generates the same board twice establishes repeatability, but two identical calls can repeat the same incorrect algorithm. Comparing against the published vector checks the actual ordering. Comparing against the separate oracle across all sizes also catches a defect that happens only on a longer shuffle.

## Freezing and game creation

The result is built with `Object.freeze`, and the deck array is frozen separately. Freezing the enclosing object alone would not freeze its array. The returned `Board` therefore protects both its description fields and the deck contents at runtime.

`createGame` in `src/game/game.ts` spreads this board description into a new game object and adds progress fields. The game continues to reference the frozen deck. It does not shuffle again on each activation or copy faces around when a pair is found. Matched positions and selection track progress separately.

The outer game object is not itself frozen by `createGame`. Readonly TypeScript declarations and immutable transition functions express its update model; the explicit runtime freezes in this path apply to generation's board and deck. See [Game state model](game-state-model.md) for that distinction.

## Where a fresh seed comes from

`freshBoardCode` uses `crypto.getRandomValues` with a one-element `Uint32Array`. It rejects draws outside a divisible prefix of the 32-bit range, maps the accepted draw to `1..SEED_MAX`, and returns a validated code. This bounded sampling is separate from the shuffle's generator.

When supplied with `previousSeed`, it also rejects that seed. `useGame.newGame` supplies the current game's seed, including when a size change calls the same function. The immediate next deal therefore has a different seed. There is no history of all seeds previously used in the tab, and there is no comparison of the resulting decks. A distinct seed is the actual guarantee in this path.

At startup, the hook calls `freshBoardCode(preferences.size)` without a previous seed. The stored preferences contain the size and best move records, not the last deal's seed. Restart follows a different path: it calls `createGame` with the existing code and does not request a fresh seed. See [New Game and Restart](new-game-and-restart.md).

## Evidence

`src/game/deck.test.ts` compares the generator states and published deck, checks the maximum seed and invalid forms, and verifies complete frozen pairs at every size. It also compares all sizes with an independent deal oracle. `deck-rejection.test.ts` targets a high rejected draw and a draw exactly at the rejection boundary.

For a wrong-deal report, compare the exact code and the first differing position. A correct code with a wrong deck points to generation; a correct deck with unexpected artwork points to [Pair identity](pair-identity.md).
