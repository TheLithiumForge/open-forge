# Neon Pairs 1.3: board codes

These release notes explain the introduction of reproducible Neon Pairs deals and the distinction between a board's identity and a player's progress.

The historical trigger was a familiar debugging exchange: someone reported that two cards behaved unexpectedly, but starting the game again produced a different arrangement. A screenshot showed the result without supplying the hidden starting deck. A verbal description such as “the third card in the second row” identified a position only within a deal nobody else had. Board codes made the starting arrangement something both people could load.

The code syntax, generator, controls, and examples are the current version 1 behavior, retained by spec 2.2.

## A deal acquired a compact name

The current form is `np1-N-seed`. `N` is one of the supported side lengths, and the seed is a decimal integer from 1 through 2147483646. Leading zeros are rejected. The control trims surrounding whitespace before parsing; the core `parseBoardCode` function expects the canonical code itself.

For example, `np1-4-1` names a particular four-column deal. It does not say which cards have been revealed, how many moves have been used, or whether the board has been finished. This distinction kept a short code useful in a bug report. A player could paste it into Board code and select Load board to begin from its hidden starting state.

The release review considered encoding the complete shuffled deck instead. That would have made a deal independent of the generator implementation, but the text would have been much longer and would need to describe the same pair invariants already known to the game. Fixing an algorithm under a versioned prefix gave the project a compact format and a clear compatibility boundary. The [board-code decision](adr-009-board-codes.md) and [format description](board-code-format.md) contain the lasting details.

## The version includes the shuffle

It was tempting to think that recording the seed was enough. It is enough only when every consumer uses the same generator, initial deck ordering, draw mapping, and swap order. The current implementation begins with duplicated face IDs in ascending order, then shuffles backward from the final position.

Each Park-Miller step multiplies the state by 16807 and takes the remainder modulo 2147483647. To select among a bounded number of positions, the generator maps `state - 1` through a rejection interval before applying the remainder. The rejection step avoids assigning some destinations more generator values than others. Replacing it with a simpler remainder operation would change some decks, even if ordinary examples happened to look plausible.

The release's enduring lesson was that deterministic behavior includes the small arithmetic choices. A code's meaning cannot depend on which other feature happened to consume a random draw first. In the current source, `generateBoard` receives only the board code. Browser seed creation lives separately in `session/fresh-board.ts`, while the deck generator advances its own local state. See [deck generation](deck-generation.md) for the algorithm's full account.

## An example became an independent anchor

The known deck for `np1-4-1` is:

```text
6 7 0 5
2 2 0 5
3 7 1 6
4 4 1 3
```

Positions use zero-based row order in the core. Face 0 therefore occupies positions 2 and 6; face 6 occupies 0 and 11. This compact example gives reviewers a way to reason about a match or miss without relying on a hidden test-only copy of game state. The literal array appears in `src/game/game.fixture.ts`, alongside example pairs and initial generator states.

The fixture is useful because a test that generates its own expected deck with the production generator can agree with the same bug. A literal vector adds an external anchor within the project. The current repository also has dedicated rejection-path tests, so the less common branch of bounded draw selection is not left to chance.

A single example does not test every seed, but this memorable vector became part of debugging language. “Load the four-column seed-one board and open positions zero and eleven” is much more actionable than “try until you get the same cards.”

## Controls respected the current attempt

Board code entry belongs inside the collapsed Reproduce a game details element. The current field shows the active code after replacement, limits entry to 32 characters, and allows an edited draft to remain while ordinary card actions update the board. Merely typing a valid code does not replace the game.

On Load board, validation happens before replacement. An invalid entry leaves both the attempt and the draft available and displays `Invalid board code.` A successful load creates that deal's initial state and updates the selected size. Focus remains associated with the loading control rather than jumping to a card. The board's next entry position resets to its first card.

This was a deliberate usability boundary in the review. If every keystroke could replace the board, correcting a typo would become destructive to the current attempt. If an invalid load erased the draft, diagnosis would require retyping the same evidence. The current `BoardCodeForm.tsx` keeps an uncontrolled input keyed by session generation, and the session replaces a game only after `parseBoardCode` succeeds.

## Restart became easier to explain

With a named deal, the difference between New Game and Restart became concrete. New Game chooses a fresh seed at the current size. Restart uses the current code again. Both clear progress, but only one changes the starting arrangement. A page reload starts a fresh deal rather than secretly resuming the previous attempt.

The distinction also made incident reproduction safer. A reporter could reset the same problem board without assuming that the Restart button meant “give me something else.” The [new-game guide](new-game-and-restart.md) covers current behavior, and [session design](session-module.md) explains the replacement boundary that invalidates old timer work.

## What the code did not carry

A board code was never a proof of a result. Anyone can load it and play it, and local data is not authenticated. Nor did the code preserve a mismatch that was halfway through closing. Those are different questions: one about trust, the other about progress. The release kept the format small by making it answer only which cards begin where.

That constraint led naturally to the [snapshot work in 1.4](release-notes-1-4.md). A saved position could refer to the same board code and add state rather than inventing another deal algorithm. Later replay support could also begin with that code and apply actions. This gave the project three useful levels of evidence: starting arrangement, current position, and timed action sequence. The [reproducibility overview](reproducibility.md) is the present reference for their relationship.

The release's practical improvement was less time spent reconstructing the setup of a report. It also made compatibility visible. Product releases can refine rendering, input, or timing without changing what existing version 1 codes mean. The [2.0 notes](release-notes-2-0.md) return to that distinction between a product specification version and an exchange-format version.
