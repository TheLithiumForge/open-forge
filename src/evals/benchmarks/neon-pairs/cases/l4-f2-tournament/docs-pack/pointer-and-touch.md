# Pointer and touch

This page describes click and tap activation, pointer focus and scrolling through boards that exceed their viewport.

Cards use ordinary button clicks for activation. The implementation does not reveal cards on pointer down, hover or touch start. That choice lets a finger begin on a card and then scroll the board without committing a turn before the browser has decided whether the gesture was a tap.

## The activation path

`Card.tsx` attaches `onClick` to a native `button` with `type="button"`. The handler first calls `activate(index)` and then `clicked(event.currentTarget)`. Board supplies `activateCard`, a stable callback that forwards to the latest session activation handler, and `cardClicked`, which scrolls the clicked element into view with nearest alignment on both axes.

There is no face-specific pointer handler. Whether the click reveals a card is decided in the session and game functions. Matched cards, the already revealed first card, out-of-range indices and activation during a mismatch have no game effect. Imported paused games are blocked at the session boundary. A click handler can therefore run without a counter or selection changing.

The native button path also accepts Enter and Space. A player can tap the first card and use arrows and Enter to select the second; the game receives the same index actions. [Board keyboard navigation](board-keyboard-navigation.md) covers movement, while [Turn resolution](turn-resolution.md) explains the shared rules behind the result.

## What is intentionally absent

Card has no `onPointerDown`, `onTouchStart`, `onMouseEnter` or context-menu cancellation handler. The game does not reveal the face merely because a pointer enters its bounds. Hover changes a border through CSS. Pointer cancellation has no game action to undo because a press alone did not choose the card.

The browser's context menu remains available for right-click and long-press. The source comment records the behavior relied on in Chromium: it does not send a click after a context menu, long press or touch scroll. Thus the component can treat its received clicks as activations instead of keeping a second gesture classifier.

There is no application constant for gesture distance or press duration. Adding one would change the interaction model and would need evidence beyond a visual review.

## Focus before and after a press

A pointer press can focus a card before its release. `use-board-focus.ts` remembers that position, but it scrolls immediately on focus only when the card matches `:focus-visible`. The source explains why: moving a card beneath a held pointer could make the release land somewhere else and lose the click.

After a click completes, Board's `cardClicked` calls `scrollIntoView` with `{ block: 'nearest', inline: 'nearest' }`. At that point the pressed card can safely move fully into view. This ordering is especially noticeable when only a sliver of a card is visible at the bottom edge of the board.

The result is a two-stage behavior. Focus memory may update during the press, without moving the target under the pointer. Completed activation is followed by nearest scrolling. A refactor that always scrolls in `onFocus` could pass centered-card click checks and still fail edge clicks. A refactor that removes the click scroll could preserve activation but leave the chosen card clipped.

Keyboard focus uses the board's scroll helper, which also deals with native Tab centering. Click scrolling is a more direct `scrollIntoView` call after completion. They share an alignment goal but have different timing constraints. [Focus policy](focus-policy.md) explains the related action focus behavior.

## Board dimensions and overflow

The viewport is the `.board-viewport` element in `board.css`. It uses `overflow: auto`, `max-height: min(70dvh, 768px)`, `6px` padding and a `1px` border. `overscroll-behavior: contain` keeps scroll chaining contained at the board boundary. Its width is content sized with `max-width: 100%`, so an oversized deck does not widen the outer page.

The inner `.board` is a CSS grid. Columns repeat `--grid-size` times, with minimum and maximum track sizes of `44px` and `88px`. Its width is clamped between the space needed for all minimum tracks and all maximum tracks, using `100%` as the preferred width. The gap is `6px`.

For sixteen columns, the minimum card-and-gap content width is `16 × 44 + 15 × 6 = 794px`. Padding and border surround that content. On a phone, horizontal scrolling is expected. Shrinking the cards below their minimum is not how the current layout makes a large board fit.

The default four-column board fits a narrow phone without an internal horizontal overflow in the layout test. The sixteen-column board intentionally needs more space and can scroll both ways. These are different expected results, so checking only whether any horizontal scrollbar exists is not a useful universal pass condition.

## Touch CSS and browser behavior

`.card` sets `touch-action: manipulation`, `user-select: none` and the WebKit-prefixed selection property. The touch-action value retains browser panning and pinch interaction while avoiding a separate application gesture system. The neutral back and face artwork are children of the button; there is no transparent overlay catching touches above them.

The textarea, input and native select are outside this card-specific selection styling. A user can still select exported JSON for copying. The interface does not apply a page-wide rule that blocks all text selection or consumes every touch movement. [Reproduction panel](reproduction-panel.md) covers the text editing surface.

Cards have a square aspect ratio and zero padding, with an `8px` border radius. Artwork fills the card, while the matched check mark occupies a small corner. All of these visual pieces live inside the same button target. A tap on a shape, marker, neutral back or check mark reaches that button's click path.

## Reaching a large board's end

The touch-scroll browser check creates a 16x16 game and dispatches actual touch sequences through Chromium's debugging protocol. It moves diagonally across the viewport in repeated gestures, waits for the scroll position to settle and checks that the last card is inside the board bounds. It also checks that the gestures revealed no cards.

That last assertion matters. A test could reach the bottom-right corner successfully while accidentally choosing a card at the beginning of each drag. The interface would be scrollable but would have changed a turn merely because the player moved the view. The current test checks both travel and the absence of activation.

`tests/pointer-focus.e2e.test.ts` addresses a different risk. It clicks or taps a partially clipped card, after wheel scrolling, and after a jump from the reproduction panel. It checks that the pressed card reveals and ends in view. These sequences cover focus and scroll timing that a simple centered tap does not expose.

The viewport matrix includes phones, a desktop-sized layout and an equivalent zoom layout. The source uses settled frames when waiting for wheel or touch motion rather than assuming that the first event means scrolling has finished. That makes a click-coordinate failure easier to distinguish from a game-rule failure.

## Mixed input and blocked turns

Pointer and keyboard use can alternate within one turn. Focus on the clicked card becomes the remembered board location. Pressing a plain arrow next moves from that coordinate, with a visible keyboard outline. The focus-entry tests check this switch in all four directions.

During a mismatch, clicking a third card does not reveal it. The click may still affect focus or scroll position, because the button is present and focusable. The game guard is what holds the two revealed cards steady for the remaining active interval. During a paused import, the same distinction applies at the session layer.

Matched cards retain their full-opacity face and green border when hovered. Clicking one does not add a move. Their continued presence as buttons allows a pointer user to revisit a remembered position and a keyboard user to traverse a consistent grid. [Disabled state guide](disabled-state-guide.md) discusses why semantic unavailability is not implemented as removal from focus order.

## Practical investigation notes

When a report says a tap failed, it helps to establish whether a click event was delivered, whether focus moved, whether the card was eligible and whether a scroll moved the view after activation. A tap on the first revealed card is expected to have no game effect. A tap during the mismatch interval is also expected to have no game effect. A release that stopped being a tap belongs to the native gesture path.

A reproduction can use a fixed code such as `np1-4-1` for turn eligibility and a 16x16 code for clipping. Record the index under the pointer, not just the artwork, because each face has a partner. Record the viewport and the board's own scroll offsets separately from the outer page's position. That makes a wrong-target report concrete.

The existing evidence does not imply that every possible mobile browser version was exercised. The repository README records Chromium and Playwright versions and distinguishes tree checks from spoken acceptance. [Browser support](browser-support.md) is the appropriate place for broader support claims; this page describes the interaction implemented and the specific failure modes covered by source tests.
