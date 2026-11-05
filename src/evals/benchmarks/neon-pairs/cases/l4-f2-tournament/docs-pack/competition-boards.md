# Boards used in competition

This page describes board dimensions across competition stages and explains their implications for the existing renderer and game core.

A board's side length affects the amount of information a player remembers, the physical area occupied by the cards and the range of face designs visible in that game. These are related consequences of one numeric size. They are separate from the actual arrangement of those faces, which is discussed in [competition deals](competition-deals.md).

## Dimensions by stage

Competition uses a subset of the sizes already supported by the solo game. Quarterfinals are played on 4x4, semifinals on 6x6 and finals on 8x8. There is no new board geometry involved: each is an existing square grid handled by the current deck and board components.

| Stage | Side length | Cards | Face pairs |
| --- | --- | --- | --- |
| Quarterfinal | 4 | 16 | 8 |
| Semifinal | 6 | 36 | 18 |
| Final | 8 | 64 | 32 |

The number of cards is the square of the side length, and each face occurs twice. Moving from the smallest board in this table to the largest multiplies the card count by four. This is a meaningful increase in the amount of visible space and remembered information even though the input model remains familiar.

The stage vocabulary itself belongs to [competition rounds](competition-rounds.md). The board currently loaded in the solo application has no stage. A person choosing a size from the ordinary select is choosing a board dimension; there is no current competition context attached to that action.

## Existing support in the core

`GRID_SIZES` in `src/game/deck.ts` is `[4, 6, 8, 10, 12, 14, 16]`. The three dimensions used here are members of that existing list. `isValidSize` checks membership in this list, and `parseBoardCode` uses that check when validating a code.

`generateBoard` creates `size ** 2` positions and initializes their face values with `Math.floor(index / 2)` before shuffling. Consequently the boards above contain face ranges 0–7, 0–17 and 0–31 respectively. A larger board expands the face set rather than merely repeating the small board's faces more often.

`createGame` spreads the generated board into `GameState`. `isWon` compares the number of matched positions with the length of the deck. Neither function branches on an event stage. The game core can therefore represent every board in the table without any competition-specific completion calculation.

`src/game/completion.test.ts` exercises completion at every supported side length. It builds the face pairs from the generated deck and activates both positions of each pair. The core's completion behavior already covers these sizes; the stage-to-size connection is a separate competition rule.

## What the larger face set actually changes

The visual identity comes from `src/game/board/face-design.ts`. The shape is selected by face number modulo eight. Marker direction is selected from the next group of eight, and the outline variant begins at face 64. The game has a total of 128 face designs.

All face numbers used by the boards in this page are below 64, so they use the solid variant. The smallest board contains the eight shapes with the top marker. The middle board includes additional marker directions, and the largest reaches through four marker groups. This gives reviewers a more precise explanation of increasing visual complexity than saying only that there are more cards.

For instance, the circle shape can appear with different marker directions on a larger board. Recognizing a familiar outline of a shape is therefore less informative than recognizing the full face. The player-facing introduction in `Game.tsx` mentions shape and marker for this reason. The deck still compares face numbers; it does not award a partial match for two similar shapes.

[Face catalogue](face-catalogue.md), [pair identity](pair-identity.md) and [card artwork](card-artwork.md) cover those distinctions in depth. They are useful when a playtest report says that two cards looked alike and an engineer needs to determine whether the report concerns face design, remembered position or a misunderstood pair.

## Board dimensions in the layout

`src/game/board/Board.tsx` sets the CSS custom property `--grid-size` from `game.size`. The grid uses that value in its column definition. It does not choose a separate component for a larger stage, and its card keys are position indices.

In `board.css`, `--card-min` is 44px, `--card-max` is 88px and `--card-gap` is 6px. At the minimum card width, the grid content widths for side lengths 4, 6 and 8 are 194px, 294px and 394px. These figures exclude the scroll container's padding and border. They are arithmetic from the current CSS, not additional size targets.

The viewport has `overflow: auto`, `max-width: 100%` and a maximum height of `min(70dvh, 768px)`. Its grid width is clamped between the minimum and maximum card-based widths. This means the presence of scrolling depends on the available space as well as the selected side length. A desktop screenshot cannot establish what the same board feels like on a held phone.

This matters in a shared setting because the device changes orientation and viewing position as it is passed around. A board that fits comfortably on a laptop may involve local scrolling on a narrower device. The existing layout accounts for overflow; the competition stage mapping does not supply a replacement layout. See [large board rendering](large-board-rendering.md), [layout grid](layout-grid.md) and [zoom behaviour](zoom-behaviour.md).

## Positions remain positions

The core uses zero-based row-order indices. The card's accessible name uses a one-based row and column, computed in `src/game/board/card-state.ts`. At size 6, index 17 is row 3, column 6. At size 8, the same numeric index is row 3, column 2. A position number has meaning in the context of its board size.

This is an easy source of misleading manual notes. A researcher can write down “card 17” while watching one board and later open a different size. Without the board context, the note no longer identifies the same visible location. Board codes carry the side length; a loose position number does not.

Keyboard movement also uses the size directly. `navigationTarget` calculates row boundaries and vertical destinations from the current side length, stopping at edges. The number of columns changes the distance represented by an up or down move, while the underlying arrow-key interaction remains the same. [Board keyboard navigation](board-keyboard-navigation.md) covers the existing behavior.

## Review notes and unresolved presentation choices

The important existing distinction is between a size being technically supported and a stage selecting that size. The ordinary `Controls` component renders all seven values from `GRID_SIZES` and delegates changes to `newGame`. It contains no awareness of the stage mapping on this page. A prototype that simply surrounds those controls with competition text has not made that mapping part of its controller.

How that context is presented during a group session remains a separate design subject. The board dimensions for the stages use the existing size support in the source.

One useful comparison in a visual review is the same board early and late in play, at the same viewport width. That isolates the change in exposed artwork from the change in grid dimensions. A second comparison across the three dimensions then makes the actual growth visible. The two comparisons answer different questions and avoid attributing every difficulty on a larger board to its card count alone.
