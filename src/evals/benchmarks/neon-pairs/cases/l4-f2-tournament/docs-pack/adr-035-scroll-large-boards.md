# ADR 035: Scroll the board after targets reach their minimum

This record explains why large boards scroll internally instead of shrinking cards to fit every viewport.

## Context

Sixteen columns cannot fit a phone width while keeping useful card targets and legible shape/marker combinations. Scaling the entire board down would technically expose every position but would undermine both input and face recognition. The selected layout keeps a minimum card size and lets excess content scroll inside the board.

The board viewport has overflow in both axes, 6px padding and a height limited to the smaller of `70dvh` and 768px. Its outer width stays within the page. The grid uses square cards between 44px and 88px with 6px gaps. Large content belongs to that scroll area rather than widening the whole document.

## Decision in CSS

`board.css` expresses a minimum grid width from the number of columns, minimum card size and gaps. A clamp lets the grid expand with available width up to its maximum card size. When available width is less than the minimum grid width, overflow becomes meaningful instead of forcing smaller cards.

For sixteen columns, cards and gaps need 794px before padding. A phone therefore shows a portion of the board and can reach the rest by scrolling. The default four-column board fits the phone layout without requiring that horizontal movement. [Layout grid](layout-grid.md) gives the calculation and [Large board rendering](large-board-rendering.md) describes the resulting behavior.

## Focus is part of scrolling

A scrollable board is only usable by keyboard if moving focus also reveals the destination. The focus hook uses nearest alignment and each card has a 6px scroll margin. That margin accommodates the white outline and its offset, including when a mid-board card is near the viewport edge.

Native Tab entry can cause Chromium to center a focused descendant before the focus handler runs. `useNearestScroll` retains the last reported board offsets, restores them and then scrolls the card with nearest alignment. The page's own scroll remains with the browser. This is more precise than calling `scrollIntoView` and assuming its default behavior matches the intended board movement.

## Touch and pointer use

Touch panning stays available inside the board. A card activates on completed click rather than pointer down, so a scroll gesture can remain a scroll. `overscroll-behavior: contain` keeps board scrolling from unnecessarily propagating at its edges. Pointer-focused cards are adjusted after a completed click to avoid moving the target during a press.

The distinction between page and board scrolling is important. Controls can still be reached by ordinary page movement, while the large board has its own bounded exploration area. Preventing outer horizontal overflow does not mean locking the entire document against vertical scrolling.

## Alternatives and costs

Shrinking every card to fit would make the largest board visually complete but operationally poor. Paginating rows would preserve target size while changing the spatial memory task. A fullscreen canvas would require custom input and accessibility work without resolving the fundamental width constraint.

Internal scrolling asks players to remember positions outside the current view. That cost was accepted to retain stable card geometry and one continuous board. Browser checks cover 16x16 at 360x800 and 1280x800, first and last positions, visible scrollbars and complete focus outlines. The equivalent zoom layout adds another constrained viewport. [ADR 024](adr-024-roving-focus.md), [ADR 047](adr-047-touch-scroll.md) and [ADR 049](adr-049-zoom-floor.md) cover those complementary decisions.

## The board viewport and grid

The minimum content width is explicit in the clamp. The viewport bounds the visible area while the grid retains card dimensions.

```css
.board-viewport {
  --card-min: 44px;
  --card-max: 88px;
  --card-gap: 6px;
  --board-padding: 6px;
  --board-border: 1px;
  max-height: min(70dvh, 768px);
  overflow: auto;
  overscroll-behavior: contain;
  padding: var(--board-padding);
  border: var(--board-border) solid var(--border);
  border-radius: 12px;
  background: var(--panel);
  width: fit-content;
  max-width: 100%;
}
.board {
  display: grid;
  gap: var(--card-gap);
  grid-template-columns: repeat(var(--grid-size), minmax(var(--card-min), var(--card-max)));
  /* The scroll area's content box already leaves room for page and board scrollbars. */
  width: clamp(
    calc(var(--grid-size) * var(--card-min) + (var(--grid-size) - 1) * var(--card-gap)),
    100%,
    calc(var(--grid-size) * var(--card-max) + (var(--grid-size) - 1) * var(--card-gap))
  );
}
```
