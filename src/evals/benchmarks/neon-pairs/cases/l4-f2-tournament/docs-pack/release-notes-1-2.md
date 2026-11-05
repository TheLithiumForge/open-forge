# Neon Pairs 1.2: large boards

These release notes describe the expansion to large Neon Pairs boards and the layout work needed to keep their positions reachable.

A bigger board changed more than the number of cards. The [layout incident](incident-16x16-layout.md) follows one failure in more detail.

## Seven sizes became one coherent range

The current size catalogue is `4`, `6`, `8`, `10`, `12`, `14`, and `16`, held in `GRID_SIZES` in `src/game/deck.ts`. Each size is a square, so card counts range from sixteen to 256 and pair counts from eight to 128. Even side lengths ensure an even number of positions, and each chosen face appears twice.

The historical large-board work made these values a visible progression in the native Grid size selector. It did not introduce a second game mode. The same first reveal, second reveal, immediate match, and delayed mismatch rules apply throughout. The increased difficulty comes from remembering more identities and positions rather than changing the scoring formula.

This was useful for review because a large board could expose a layout weakness without changing the underlying play rules. If a six-column board behaved differently after a miss, the problem belonged to interaction or state handling rather than a special difficulty rule. The [board-size catalogue](board-size-catalogue.md) carries the present values and their pair totals.

## The question was where overflow belonged

The first large-board sketch tried to keep all cards visible by shrinking them. It made the screenshot look tidy and made the cards harder to distinguish. Another sketch allowed the entire document to become wider than the phone. That kept targets larger but forced the controls, explanatory text, and page itself into a horizontal canvas. A player could lose the controls while looking for a card at the right edge.

The surviving layout puts overflow inside `.board-viewport`. The page content has a maximum width of 1120px and 16px padding. The board viewport has its own overflow in both directions and a height limit of `min(70dvh, 768px)`. Cards stay square, ranging from 44px to 88px according to available width. A 6px gap keeps neighboring targets and their focus outlines distinct.

This means a large board can be larger than its visible window without making the document itself wider. It also means there are two useful coordinate systems: position in the full board, and position in the visible portion. A memory game benefits from stable board coordinates. Scrolling moves the window, not the cards' row and column identities.

## The arithmetic behind the smallest large board view

A sixteen-column row at the minimum card size uses `16 * 44 + 15 * 6`, or 794px. Board padding adds 12px, and the viewport's own border is separate. On a narrow phone screen this cannot fit in one horizontal view without breaking the chosen card minimum. The current grid therefore keeps its minimum content width while the containing viewport stays constrained by the page.

The CSS expresses this with a clamped width and repeated `minmax` tracks. One subtle part of the release was learning that width calculations had to respect the scroll area's content box. Subtracting guessed scrollbar widths or using the outer browser width directly made the board look correct under one scrollbar configuration and wrong under another. The current comment in `board.css` explicitly notes that the scroll area's content box already leaves room for page and board scrollbars.

The [large-board rendering guide](large-board-rendering.md) explains the resulting layout. The historical lesson was narrower: “all cards render” is weaker than “all cards can be reached while the rest of the page remains usable.”

## Artwork had to scale in variety too

The largest board needs 128 distinct pairs. The current face generator provides IDs `0` through `127`, combining eight shapes, eight marker positions, and two fills. Marker position also chooses the palette color, so color reinforces a structural difference rather than adding an independent identity axis.

The large-board review made near-neighbor comparison important. A circle and triangle with the same top marker were easy to tell apart; a solid and outline square with a diagonal marker required a more careful drawing. Today's SVG keeps the main shape within the central portion of a `100 100` view box and draws a marker above it with a separating background stroke. The diagnostic face sheet renders all identities at the minimum card size.

The release did not solve variety by loading a large image atlas. Inline SVG preserves the same geometry at every supported card size and works within the offline document. Hidden cards still mount only the neutral back. This keeps the face catalogue a visual vocabulary of revealed information, not a collection of identity-bearing elements concealed by CSS.

## Keyboard reach became a layout check

A keyboard destination can be mathematically valid and visually unusable. Moving to the last position of a sixteen-column board must also bring that position into view. Current focus handling uses nearest alignment and card scroll margins so a focus outline can be seen without unnecessarily centering the whole board.

The release discussion distinguished this from smooth camera movement. The goal was finding the focused card, not making travel across a large surface theatrical. In the current source, `useBoardFocus` focuses with `preventScroll` and applies the board's scrolling behavior deliberately. `useNearestScroll` accounts for the browser's tendency to center a remembered card when Tab enters a scroll area. That refinement is especially visible when a player leaves the board, scrolls elsewhere, and returns.

Pointer and touch supplied a different constraint. Moving the board while a pointer is pressed can shift the release target away from the original card. The current focus handler avoids scrolling a pointer-focused card before the click has completed; the click callback can then bring it into view. This is one reason that keyboard and pointer tests need distinct journeys even though they share activation rules.

## What grew, and what stayed simple

All 256 cards remain DOM buttons in the current largest board. The release considered a windowed render in the design review, but a board this size did not justify making off-screen positions disappear from the DOM and complicating focus. Current rendering instead avoids unnecessary card work: `Card` is memoized, face-up and matched sets are memoized, and a stable callback invokes the latest activation handler.

A normal elapsed-time tick that changes neither selection nor matches therefore need not rerender every card. This is a targeted optimization around observed structure. It does not introduce a separate rendering model for each size or a canvas implementation that would need its own input semantics.

The browser suite includes all-size, layout, scrollbars, touch-scroll, focus-entry, and zoom journeys. The specification calls out 360x800 and 1280x800 CSS-pixel views for the largest board, plus an equivalent zoom layout. These are concrete inspection contexts, not a claim that every device has been tested. [Phone research](research-phone-usage.md) explains why reaching both extremes of the board became more useful than a screenshot of its upper-left corner.

The release left a practical outcome: choosing a larger grid increases the memory task while keeping the same controls, stable positions, and route out of the board. [Board codes in 1.3](release-notes-1-3.md) later made a particular difficult layout easy to discuss without trying to recreate a random deal from memory.
