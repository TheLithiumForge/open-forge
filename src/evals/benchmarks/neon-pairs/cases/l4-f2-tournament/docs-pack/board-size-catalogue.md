# Board size catalogue

This page lists the seven supported square boards and explains what size changes in the deck, progress, and visible play.

The supported side lengths come from `GRID_SIZES` in `src/game/deck.ts`: 4, 6, 8, 10, 12, 14, and 16. `Controls.tsx` uses that tuple to build the native Grid size options. The game has one set of turn rules across all seven sizes.

## Counts and face ranges

| Grid option | Positions | Pairs | Face numbers | Fewest moves to finish |
| --- | ---: | ---: | --- | ---: |
| 4x4 | 16 | 8 | 0–7 | 8 |
| 6x6 | 36 | 18 | 0–17 | 18 |
| 8x8 | 64 | 32 | 0–31 | 32 |
| 10x10 | 100 | 50 | 0–49 | 50 |
| 12x12 | 144 | 72 | 0–71 | 72 |
| 14x14 | 196 | 98 | 0–97 | 98 |
| 16x16 | 256 | 128 | 0–127 | 128 |

A face occurs twice. The deck is constructed from consecutive face numbers before being shuffled, so each row above describes the exact range in every seed at that size. It is not a random subset of 128 faces.

The minimum move count follows from one successful two-card attempt per pair. It is a mathematical lower bound and a useful test oracle, not a difficulty rating. The current game does not grade performance against these minima.

## 4x4: the default and the published example

Four rows and four columns are the fallback when no valid saved preferences exist. There are eight faces, all from the first shape group: solid shapes with a top marker. Their labels use the cyan color name. Shape alone distinguishes those eight identities within this size.

This is the size of `np1-4-1`, whose literal deck appears in the reproducibility specification and generation tests. A compact known board makes it practical to reason about a complete sequence without a large lookup table. Positions 2 and 6 match; positions 0 and 1 differ.

A fresh status begins at zero moves, zero of eight pairs, and zero elapsed time. Completion in eight moves is possible when every attempt is a match. Taking longer does not impose a penalty beyond the displayed elapsed time, and a miss costs the same single move as at every other size.

The small board is useful for reproducing state transitions. It is not a special implementation: it passes through the same generator, selection union, clock, and completion check as the larger boards.

## 6x6: the first repeated shapes

Six rows create 36 positions and 18 distinct faces. Face numbers 0 through 7 have top markers; 8 through 15 have top-right markers; 16 and 17 begin the right-marker group. The shape sequence repeats every eight face values.

A player can therefore encounter two cards with the same central shape that are not a pair. For example, face 0 and face 8 are both circles, but the marker positions differ. The artwork also uses different group colors, while identity remains the numeric face value.

This is the first size at which remembering only a shape becomes insufficient across the complete deck. The game does not change its comparison rule to compensate: exact face equality has always been the criterion. [Pair identity](pair-identity.md) explains the dimensions that distinguish faces.

The lowest completed move count is 18. Its best record is stored under key `"6"`, independently of the eight-pair board's record. Selecting 6x6 resets current progress and requests a fresh seed.

## 8x8: four complete marker groups

An 8x8 board contains 64 positions and 32 faces. It uses solid faces from the top, top-right, right, and bottom-right marker groups. Each group contains the eight central shapes.

The board has four times as many positions as 4x4, even though its side length is only doubled. That squared growth is relevant when comparing screenshots or considering how much of a board fits in a viewport. It does not imply that a human game takes exactly four times as long; the code makes no such timing assumption.

The progress denominator is 32 and the minimum move count is 32. The mismatch delay stays at 800 active milliseconds. A larger board does not give a longer memory interval per miss.

A size-8 seed of 1 is a distinct board code from size-4 seed 1. Both start the same generator recurrence, but the initial deck length, bounded destinations, and number of swaps differ. A smaller board is not a cropped part of a larger generated deck.

## 10x10: fifty pairs, still solid faces

Ten rows and ten columns use face numbers 0 through 49. There are six complete marker groups through bottom-left, then the first two shapes of the left-marker group. All 50 faces remain solid.

At this size, a pair's positions can be widely separated in both row and column. The comparison function does not care about that distance. Adjacent pairs and opposite-corner pairs resolve identically once their positions are activated.

The progress denominator and theoretical minimum are both 50. The status component derives them from the deck and matched positions, rather than containing a separate label table for a hundred-card board.

As with every option, rows and columns remain fixed by the selected size. Scrolling changes the portion in view; it does not re-deal or rotate the board. The board's positions keep their row-order indices throughout play.

## 12x12: the outline range begins

A 12x12 deck has 144 positions and 72 faces. Faces 0 through 63 cover all eight marker groups as solid shapes. Faces 64 through 71 add the eight top-marker shapes in outline form.

This is the first supported size with a solid and outline version of the same shape and marker. Face 0 and face 64 are both circles with a top marker and the same group color, but the fill treatment differs. They are distinct faces and do not match each other.

That detail is easy to miss if a reproduction note says only “the cyan circle.” The full revealed label includes solid or outline and marker direction, giving an unambiguous description. The source comparison still uses face numbers; descriptive labels help humans discuss the identity.

The minimum result is 72 moves, saved separately for size 12. There is no unlock step or dependency on completing a smaller board before this option can be chosen.

## 14x14: outline groups extend

A 14x14 deck uses 98 faces across 196 positions. It contains every solid face, outline groups for top, top-right, right, and bottom-right markers, and the first two outline shapes in the bottom-marker group.

The included face range is a prefix of the full face set, so it ends partway through a group. The generator does not balance the count of shapes or marker groups after choosing the size. What is balanced is the multiplicity of each included face: exactly two copies.

The distinction matters for artwork inspection. Seeing more occurrences of one central shape than another across the entire deck can be expected when the face range stops in the middle of an eight-shape group. It does not imply a missing mate or a broken shuffle.

Completion requires 98 pairs. All selection and clock limits remain unchanged. In particular, time and moves are not scaled by the size, and neither cap acts as a loss condition.

## 16x16: the full face set

The largest board has 256 cards and 128 distinct faces. It uses every face supported by `FACE_COUNT` in `src/game/board/face-design.ts`: eight shapes, eight marker directions, and two fill treatments.

Its progress denominator is 128. `completion.test.ts` completes a generated game at this size in 128 known successful attempts, checking that the same core handles the full range. The browser all-sizes test, despite its file name, specifically starts the largest board and checks one reveal; it does not itself finish all seven browser boards.

The full board can exceed the visible scroll area. `board.css` uses a scrollable viewport and a grid that keeps the selected column count. The viewport's maximum height is `min(70dvh, 768px)`. Cards are sized between the CSS bounds of 44 and 88 pixels with a 6-pixel grid gap. Full layout behavior belongs to [Large board rendering](large-board-rendering.md).

Scrolling and focus movement do not alter the face distribution, moves, or pair count. Remembering a position remains meaningful when that position is temporarily offscreen.

## Choosing, loading, and remembering size

Option values are numeric side lengths, such as `"10"`, with visible text `10x10`. The change handler converts the value and calls `newGame`; unsupported sizes are rejected.

Successful code and snapshot loads select their game's size and update preferences. Failed loads retain the existing size. Reload uses the saved supported size but starts fresh progress, and each size selects its own best entry.

Switching options replaces the current game rather than keeping suspended games per size. [New Game and Restart](new-game-and-restart.md) compares these paths.

## Shared behavior

Every option uses the same two-card attempts, 800 active millisecond mismatch interval, first-reveal clock start, and matched-coverage completion. Scrolling preserves row-order positions. The seven values form the supported catalogue; arbitrary even side lengths are not accepted by the parser or size guard.
