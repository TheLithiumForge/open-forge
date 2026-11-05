# Layout grid
This page explains the page container, board scroll area and CSS Grid sizing used by Neon Pairs.

The layout has two width decisions. `main` keeps the document within the available page width and centers its content area. `.board-viewport` bounds the board's visible area, while `.board` can have a larger scrollable grid inside it. This arrangement lets a small board fit naturally and a large board keep useful card sizes. The related requirements are V02 through V05 and V19 in [Visuals](visuals.md).

## The page container

`src/game/game.css` sets global `box-sizing: border-box`, removes the body margin and gives `main` a width of 100%, a maximum width of `var(--content-max)`, automatic horizontal margins and `var(--page-padding)` padding. Those properties currently resolve to 1120px and 16px. The maximum includes the padding because of border-box sizing.

At a 1280px layout width without a space-consuming page scrollbar, `main` is 1120px wide and has an 1088px inner content width. Its left outer edge is 80px from the window; ordinary children begin 96px from that edge of the window. On a 360px phone layout, the available content width is 328px. These examples exclude page scrollbars so the box arithmetic remains visible.

The header, controls, status, help, result slot, board and reproduction section stay in document flow. The board's internal scrolling does not pin the controls to the window. Reaching controls can still involve ordinary vertical page scrolling. There is no outer horizontal board rail that expands the document to the full grid width.

## The scroll area

`board.css` gives `.board-viewport` these local variables:

| Property | Current value | Purpose |
| --- | --- | --- |
| `--card-min` | 44px | Lower bound for a grid track |
| `--card-max` | 88px | Upper bound for a grid track |
| `--card-gap` | 6px | Gap between rows and columns |
| `--board-padding` | 6px | Interior clearance at the scroll boundary |
| `--board-border` | 1px | Border around the scroll area |

The viewport has `width: fit-content` and `max-width: 100%`. It uses a 1px neutral border, 12px radius and panel background. Both axes use `overflow: auto`. The maximum height is `min(70dvh, 768px)`, so a large square board cannot force its entire height into the surrounding page flow.

The height is a maximum, not a forced height. A short board can produce a shorter viewport. At an 800px dynamic viewport height, the cap is 560px. At 1400px, the proportional value would be 980px and the absolute 768px cap takes over. Scrollbar space and borders affect the client area inside that outer size.

`overscroll-behavior: contain` belongs to the board scroll area. It helps keep overscroll from chaining through its boundary. It does not make the surrounding page immovable or turn all page content into board content. The reproduction panel remains below the board and can still be reached through normal page navigation.

## Grid width calculation

`Board.tsx` sets an inline custom property `--grid-size` from `game.size`. The grid uses `repeat(var(--grid-size), minmax(var(--card-min), var(--card-max)))` for its columns and `gap: var(--card-gap)`. The explicit grid width is a clamp between the minimum and maximum total track widths, with 100% as the preferred width.

For `n` columns and gap `g`, the grid content width is bounded by `n * 44 + (n - 1) * g` and `n * 88 + (n - 1) * g`. There are `n - 1` gaps, not `n`. The viewport's padding and border sit outside these grid-content figures.

| Columns | Minimum grid width | Maximum grid width | Minimum with padding | Minimum outer border box |
| --- | --- | --- | --- | --- |
| 4 | 194px | 370px | 206px | 208px |
| 6 | 294px | 558px | 306px | 308px |
| 8 | 394px | 746px | 406px | 408px |
| 10 | 494px | 934px | 506px | 508px |
| 12 | 594px | 1122px | 606px | 608px |
| 14 | 694px | 1310px | 706px | 708px |
| 16 | 794px | 1498px | 806px | 808px |

The last column still excludes any scrollbar that consumes layout width. The example in the visual specification gives 794px plus 12px padding for sixteen minimum-size columns. That is consistent with the table: it is not the full bordered width of the scroll element.

## Card size between the bounds

When the available grid width is between its limits, equal tracks divide the width left after gaps. A useful calculation is `(gridWidth - (n - 1) * 6) / n`. Fractional CSS-pixel results are normal. The card takes `width: 100%` of its track and `aspect-ratio: 1`, with its border included in the square dimensions.

At a 360px width with no consuming scrollbars, a viewport filling the 328px content area would leave 314px after its padding and border. Four columns would be `(314 - 18) / 4 = 74px` each. Sixteen columns cannot compress into that width because their minimum total is 794px. The grid therefore remains wider than the visible scroll area and scrolls horizontally.

At 1280px, an 1088px main content area could leave 1074px inside a full-width board viewport before scrollbar deductions. Sixteen cards would then be `(1074 - 90) / 16 = 61.5px`. This illustrates why a desktop 16x16 board can fit horizontally while still scrolling vertically. It does not predict a universal measured width: visible page and board scrollbars can reduce the available area.

The four-column board's maximum grid width is only 370px. `fit-content` allows its outer viewport to stay compact on a wide page instead of spreading four 88px cards across the entire main container. The board is in normal left-aligned flow within the centered main area; centering the page container does not independently center every smaller child.

## Why the content box matters

The comment beside the clamp notes that the scroll area's content box already leaves room for page and board scrollbars. The preferred width of 100% resolves from that available content area. Hard-coding a width from the window alone would miss a classic desktop scrollbar and could produce a small, unwanted horizontal board overflow.

`tests/scrollbars.e2e.test.ts` exists for this case. It runs with visible scrollbars at 1280x800 for size 16, 900x600 for size 10, 1000x600 for size 12, and 360x640 for size 6. Each selected case has enough real width for the minimum grid. The assertions confirm a consuming page scrollbar, no page horizontal overflow, card widths above the minimum and no horizontal board overflow.

Those cases do not say that every board fits every width. A sixteen-column grid on a phone is expected to have horizontal board overflow. The distinction is between necessary scrolling at the minimum size and accidental overflow from calculating with the wrong box. [Large-board rendering](large-board-rendering.md) describes the first case.

## Reflow around the board

The control bar and stats use wrapping flex layouts rather than fixed columns tied to board width. At a maximum viewport width of 599px, `.size-control` becomes full width and places its label and select at opposite ends; the action buttons can share the following row. The board grid itself has no separate phone breakpoint.

The status list wraps with 12px row gaps and 24px column gaps. Reproduction rows also wrap, with input shrink support through `min-width: 0`. These details prevent peripheral controls from defeating the outer page width even when the board itself is correctly constrained. [Zoom behaviour](zoom-behaviour.md) follows the same mechanisms at the smaller equivalent zoom viewport.

The SVG geometry does not participate in grid track calculations. Hidden and visible cards share the same width, aspect ratio and border. Matching a pair adds an absolutely positioned check rather than increasing its intrinsic content size. This keeps gameplay state changes from reshaping the grid.

## Existing layout evidence

`tests/layout.e2e.test.ts` checks all 256 cards at phone, 1280x800 and 1280x1400 layouts, the height cap, page width, card square dimensions and first/last focus clearance. It also checks the default board at 320x640. The interior focus case reaches row 9, column 9, where nearest scrolling tests more than just the outer corners.

A geometry report is most useful with the viewport dimensions, board size, scrollbar mode, viewport client size, grid scroll size and one measured card box. That information separates an undersized track from an over-wide page or a clipped focus ring. [Spacing scale](spacing-scale.md) covers the supporting clearances, and [Visual review checklist](visual-review-checklist.md) groups the existing checks by symptom.
