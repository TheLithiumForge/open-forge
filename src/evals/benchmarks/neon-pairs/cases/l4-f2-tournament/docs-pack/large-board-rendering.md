# Large-board rendering
This page explains how the 16x16 board renders all 256 cards while keeping the page narrow and distant cards reachable.

The largest board uses the same components as the default board. It has more DOM buttons and a larger scrollable grid, not a separate canvas renderer or a paginated board. V19 in [Visuals](visuals.md) defines the phone and desktop appearance and interaction checks; [Layout grid](layout-grid.md) explains the shared sizing rules.

## All positions exist

`Board.tsx` maps the complete `deck` array to `Card` components. A size of 16 means 256 positions, and every position is rendered. There is no viewport filter, row window or virtualized subset in that map. A card outside the visible scroll area still exists as a button in the grid.

Hidden cards receive `face={null}` and contain their neutral back span. Only revealed or matched positions receive the actual face ID and mount `Face`. This limits identity artwork in the initial DOM without removing positions. A completely matched largest board contains the face artwork and check mark for every position.

The distinction matters to counts. The largest-board browser test expects 256 `.card` elements and initially 256 backs. After activating the first position, it expects one fewer back. It is checking the full board state, not asking the visible window to display all cards simultaneously.

The generated largest deck uses 128 identities, twice each. It consequently exercises the entire face catalogue, including outline variants that smaller boards do not reach. This makes a completed largest board a useful overlay-clearance case as well as a layout stress case. [Face catalogue](face-catalogue.md) describes the identity ordering.

## Minimum geometry and overflow

At the 44px card minimum, sixteen columns take `16 * 44 + 15 * 6 = 794px`. Viewport padding adds 12px and its border adds another 2px to an unscrolled outer-width estimate. At maximum card size, the grid content is 1498px wide. These are grid calculations, not claims about a particular browser's client width.

On a 360px page, 16px page padding on each side leaves 328px before any consuming page scrollbar. The board viewport stays within that width, while the minimum-size grid remains wider inside it. Horizontal scrolling is necessary here. Shrinking cards below the minimum would change the specified geometry rather than solve the intended overflow case.

At 1280px, the centered main area is capped at 1120px. Its inner width is 1088px before scrollbar effects. This is enough for sixteen tracks larger than the minimum, so horizontal board scrolling is not inherently necessary. The full square grid is still taller than the board's 560px cap at an 800px dynamic height, so vertical scrolling remains necessary.

Visible scrollbars can consume both page and board space. The grid's preferred width comes from the board viewport's actual content area, allowing its tracks to fit the remaining width. The scrollbar-specific tests select cases that can fit horizontally and check that the implementation does not introduce an accidental extra horizontal scroll range.

## A bounded scroll window

`.board-viewport` has `max-height: min(70dvh, 768px)`, `overflow: auto`, 6px padding, a 1px border and `max-width: 100%`. It has a panel background and 12px radius. The clamp is a maximum, so smaller boards do not acquire a large empty viewport just because the largest board needs one.

At 360x800 and 1280x800 the proportional height ceiling is 560px. At 1280x1400, the 768px ceiling is lower than 70% of the height. `tests/layout.e2e.test.ts` includes that taller window to exercise the absolute cap as well as the proportional one.

The outer page can still scroll vertically. Headers and controls above the board and reproduction content below it remain part of normal flow. “Board scrolling” here means the excess grid is contained within the board window; it does not mean the entire application is fixed to the viewport or that page scrolling is disabled.

## Keyboard movement and remembered focus

`useBoardFocus` owns a remembered index initialized to zero. Each card receives tab index 0 only when it is that remembered position; other cards receive -1. The keyboard navigation helper calculates a target from the current index, size, key and modifiers. The focus hook moves focus without revealing a card.

For explicit moves, it calls `focus({ preventScroll: true })`. This prevents the browser's default focus scrolling from choosing an unwanted alignment. The focus event then asks the nearest-scroll helper to place the card inside the visible region. The card's own 6px scroll margin supplies clearance for its outline.

The nearest-scroll helper remembers the board's last reported scroll position through a passive scroll listener. Chromium can center a Tab-focused card before its focus handlers run, while the scroll event reporting that change arrives later. Restoring the remembered board position first lets nearest alignment operate from the view the player had been using. The code leaves page scrolling to the browser.

This is a specific implementation response to focus behavior, not a general scroll manager for the entire page. It records only the board's left and top offsets. [Board keyboard navigation](board-keyboard-navigation.md) and [Focus policy](focus-policy.md) explain the navigation contract and focus destinations.

## The complete focus outline

The global focus-visible outline is 3px wide with a 2px offset, reaching 5px beyond the card border box. Six-pixel viewport padding gives outer cards clearance; six-pixel scroll margin makes nearest alignment preserve space around interior targets. Both dimensions matter.

`tests/focus-bounds.ts` measures the card, its nearest board viewport, the computed outline and the window. It subtracts outline width and offset before checking all edges. Merely finding a visible card rectangle would miss a white ring clipped by the scroll area or window boundary.

The layout test checks the first and last card, then has a separate case for row 9, column 9. An interior target can land immediately beside a scroll edge without benefiting from the physical padding at the end of the grid. This is why a successful first-card screenshot does not demonstrate the full focus path.

Subpixel widths are expected when the grid divides an available width among sixteen columns. The helper allows a small tolerance in its comparisons. A one-pixel visual report still deserves measurement of the ring and client area; it should not be dismissed merely because fractional track sizing exists.

## Pointer presses at clipped edges

A partly visible card can receive a press near the board edge. Repositioning it on pointer focus would move the button between press and release and could lose or redirect the click. `cardFocused` therefore initiates scrolling only for a card matching `:focus-visible`.

The click handler runs the activation and then calls the board's `cardClicked` helper. That helper uses `scrollIntoView` with nearest block and inline alignment after the press has completed. This preserves the clicked target and then brings it fully into view. It also works after jumping from the reproduction panel or scrolling the page.

`tests/pointer-focus.e2e.test.ts` deliberately finds a card cut by the bottom edge and clicks a point only 4px inside that edge. It repeats mouse and touch journeys at 360x800, 1280x800 and the 640x400 scale-2 layout. The assertions check that the intended card reveals and ends inside the visible area with its scroll margin.

A screenshot made before release may therefore show a partly clipped card that will be repositioned once the action completes. The timing of the capture is material. Moving the scroll call earlier would make the screenshot look cleaner during press while risking the actual interaction that the test protects.

## Touch scrolling without a reveal

Cards use `touch-action: manipulation`; the viewport uses ordinary overflow scrolling. There is no custom drag-to-pan handler synthesizing moves. `Card.tsx` relies on the browser click behavior following a completed tap. Its comment records that Chromium does not send a click after a context menu, long press or touch scroll.

The touch-scroll browser test dispatches gestures across the board at phone and desktop sizes, waits for scrolling to settle, and checks that the last card is in view. It verifies that a horizontal offset changed when horizontal overflow exists, that vertical movement occurred, and that no card became revealed during the gestures.

This is complementary to a touch tap test. A tap proves activation; a drag proves scrolling can happen without activation. Both matter on the phone largest board because horizontal and vertical movement are needed to inspect its full extent. [Pointer and touch](pointer-and-touch.md) carries the wider input discussion.

## Rendering cost and source-level optimizations

All positions render, but repeated clock updates do not need to redraw their artwork. `Card` is memoized. `Board` memoizes the face-up set from selection and matched arrays, memoizes its matched set, and keeps a stable activation callback that dispatches to the latest handler through a ref.

When a logical clock update preserves those arrays and the handler props remain equal, the card components can be skipped. Focus changes update tab indices for the relevant positions; reveals and matches update their visible props. These source mechanisms explain why status updates need not force 256 face renderings.

DOM count, conditional artwork and memoization are directly observable in the source; frame-rate and timing measurements require a measured environment. [Performance budget](performance-budget.md) is the appropriate related page for project performance evidence.

## Evidence to keep together

The existing tests cover full card count, minimum square dimensions, bounded page width, capped viewport height, first/last/interior focus, touch reachability, clipped pointer activation and visible-scrollbar fitting. No single screenshot or assertion covers all of these. A board can have the right count while its final row is unreachable, or have correct width while clipping a focus ring.

For a report, board size and CSS viewport dimensions establish the expected overflow. Client and scroll sizes show where overflow actually occurs. Input method and the focused or pressed position explain how the view reached its current offset. [Visual review checklist](visual-review-checklist.md) organizes those existing checks into a review journey.
