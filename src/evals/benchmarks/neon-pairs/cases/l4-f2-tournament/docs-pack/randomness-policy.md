# Randomness policy

This page explains the boundary between choosing a fresh deal and reproducing a deal in Neon Pairs.

A new board needs an unpredictable seed. A board code, restart, imported snapshot and replay need the opposite property: a particular code always reconstructs the same deck. The implementation keeps those two tasks separate rather than letting a shuffle consult ambient state. [Deck generation](deck-generation.md) covers the deal algorithm as a format; this page concentrates on its source ownership and integration.

## One boundary for fresh input

`src/game/session/fresh-board.ts` exports `freshBoardCode(size, previousSeed?)`. It obtains a seed and returns the normal board-code representation through `boardCode` from `deck.ts`. Only src/game/session/fresh-board.ts may call a random source. Everything else that needs randomness imports it from there. The session controller uses that exported operation when initializing a session or asking for a new game.

The distinction is visible in the imports. `use-game.ts` imports `freshBoardCode` for fresh deals and `createGame` for constructing a game from a code. `game.ts` imports `generateBoard`, which works from supplied data. Nothing in a card component needs to choose a seed, and no deck is shuffled merely because React rendered it.

## Two finite ranges

The fresh-board module uses a one-element `Uint32Array` and `crypto.getRandomValues`. Its possible raw values span the unsigned 32-bit range, with `UINT32_RANGE` set to `2 ** 32`. The allowed seed range is defined in `deck.ts` by `SEED_MAX = 2_147_483_646`. Valid codes use positive seeds, so the transformation adds one after taking a remainder.

The module computes `RANDOM_LIMIT = Math.floor(UINT32_RANGE / SEED_MAX) * SEED_MAX`. With the current values the limit is `4_294_967_292`. A draw below that limit is accepted; one at or above it is retried. Thus each allowed remainder has the same number of accepted raw values. The last four unsigned values do not enter the modulo mapping.

The seed is `(draw % SEED_MAX) + 1`. Its endpoints are therefore 1 and `SEED_MAX`; zero is not produced. The code also checks that `buffer[0]` is defined, which matches the project's checked-index TypeScript setting even though the buffer length is deliberately one. It loops until it can return a valid code.

This is rejection sampling, not rounding a floating-point draw. Applying a plain remainder to all `2 ** 32` possible values would assign a few seeds one more source value than the others. The difference is small numerically, but a reproducible format benefits from an implementation whose range treatment can be described exactly.

## Excluding the immediately previous seed

When `previousSeed` is supplied, a candidate equal to it is discarded. `use-game.ts` passes the current game's seed when creating a new game, including when a different grid size is selected. This means the next seed differs from the immediately preceding seed even across a size change.

That check is narrower than a history of every generated deal. The implementation does not keep a seed registry, does not persist a sequence of fresh codes and does not exclude every seed seen earlier in the session. A code can reappear after intervening games. Nor does the initial call pass an earlier browser session's seed; preferences retain size and best moves, not the last active deal.

The retry loop also does not claim a bounded number of calls. It retries out-of-range draws and an excluded candidate. There is no time-based fallback in this module. A browser crypto failure is not caught here. The storage module's tolerant behavior should not be mistaken for a universal fallback around every browser API.

## Construction after selection

Once the seed is selected, `boardCode(size, seed)` validates the representation. Its format is `np1-SIZE-SEED`, with the supported sizes supplied by `GRID_SIZES`. The parser accepts exactly the seven sizes 4, 6, 8, 10, 12, 14 and 16. It also checks that the seed is a safe integer within the allowed maximum.

`createGame(code)` calls `generateBoard(code)` and adds the initial progress fields. The starting deck is already determined by that point. The game starts with no matched positions, a closed selection, zero moves, zero elapsed milliseconds and `started: false`.

The code-producing function can throw for an invalid size or seed. The session's `newGame` action checks the requested size with `isValidSize` before requesting a fresh code. Its normal UI callers use the native select populated from `GRID_SIZES`. These checks protect the application path without adding a second size list to the form.

## Deterministic state is not ambient entropy

`deck.ts` exports `nextRandomState(state)`, but this function is arithmetic over an explicit argument. It computes `(state * 16_807) % 2_147_483_647`. Repeating a call with the same state returns the same result. The name describes a pseudorandom sequence; it does not imply a browser read.

For a valid seed, the generator advances that sequence inside a descending Fisher–Yates shuffle. The initial deck contains two copies of every integer face from zero to `size ** 2 / 2 - 1`, arranged as adjacent duplicates before shuffling. For a 4 by 4 board, it starts as `0, 0, 1, 1` and so on through face 7.

At shuffle position `index`, the number of choices is `index + 1`. The implementation computes a divisible acceptance limit from `SEED_MAX`, advances its local generator state, subtracts one to form the draw, rejects values outside the limit and uses the accepted remainder as the swap position. Both the raw seed selection and the shuffle use rejection, but they operate on different ranges and have different owners.

The maximum multiplication in the sequence is below JavaScript's safe integer limit. The independent browser-test oracle uses BigInt arithmetic, providing a separate expression of the same documented deal rather than calling the production generator to predict itself. [Purity guide](purity-guide.md) explains why explicit state makes these checks possible.

## A concrete stable example

The fixture code `np1-4-1` produces this row-major deck:

```text
6 7 0 5
2 2 0 5
3 7 1 6
4 4 1 3
```

Positions are zero based in rules and formats. Positions 2 and 6 are a pair; positions 0 and 11 are another. The fixture's `EXAMPLE_DECK` stores the whole literal sequence, while `EXAMPLE_PAIRS` lists the matching positions. Those are test facts, not attributes added to hidden cards in the shipped DOM.

Changing the shuffle traversal, recurrence, rejection condition or starting deck would change this example and potentially every shared code. Such a change is therefore observable through board codes, snapshots and replays, even if the interface still looks like a memory game. The generator's determinism connects implementation detail to a public format.

## Every action that chooses or reuses a deal

| Path | Seed behavior in the current implementation |
| --- | --- |
| Initial `useGame` state | Fresh seed using the saved valid size or default size |
| New Game | Fresh seed excluding the current seed |
| Grid-size selection | Fresh seed for the selected size, excluding the current seed |
| Restart | Existing board code passed to `createGame` |
| Load board | Submitted code trimmed, parsed and reconstructed |
| Load snapshot | Decoder validates its code and reconstructs its deck |
| Replay | Reader validates the supplied code and starts from it |

This table is useful when a bug report says that “a new board changed.” A new game is expected to change the seed. A restart or imported code is expected to repeat the existing deal. A size selection is a fresh deal rather than a geometric rearrangement of the old cards.

`newGame` also resets the clock and replaces the session game. Seed choice alone does not clear progress or focus; session replacement and the generation key supply those lifetimes. [Session module](session-module.md) covers that process.

## Evidence and blind spots

`deck.test.ts` covers board-code parsing, generated structure and known vectors. `deck-rejection.test.ts` reads independent BigInt vectors for an over-limit draw and a draw exactly on the shuffle limit. The equality boundary matters because accepting it would break the uniform mapping and alter the deterministic result for that seed.

The browser suite loads documented codes through actual controls and uses `specDeal` from `tests/deal-oracle.ts` when it needs an independent deal. `lifecycle.e2e.test.ts` checks that New Game changes the current code and that a size change chooses a different seed from the previous deal. A complete all-size journey checks that the resulting decks remain playable.

The fresh-board module has no adjacent unit test. Deterministic deck tests establish format behavior and known sequences, rather than statistical properties of the browser entropy source. The raw seed boundary is short enough to inspect directly, but inspection is a different kind of evidence.

The source audit's core list rejects browser entropy references in the named core files. It does not implement a whole-program information-flow proof or a general scanner for every possible source of variation. [Source audit notes](source-audit-notes.md) describes precisely what it scans.

## Diagnosing an unexpected deck

The most useful reproduction is the board code, not the time the game started. Parsing that code reveals both size and seed. Calling `generateBoard` with it establishes the expected deck without involving React or browser state. An imported snapshot reconstructs that same deck and then validates progress against it.

If the code differs, investigate which action replaced the session. If the code agrees but the deck differs, compare the shuffle against the literal vector and independent oracle. If the deck agrees but the wrong art appears, investigate `faceDesign`, reveal state and card props. These are distinct failures with different evidence.

A hidden card deliberately receives `null` as its face prop. Looking for all face identities in its DOM is not a valid debugging strategy for this application boundary. Tests already have the code and can derive a deck independently. [React boundaries](react-boundaries.md) describes that presentation decision in context.
