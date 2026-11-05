# ADR 049: Keep the constrained zoom layout playable

This record explains the equivalent 200% zoom layout and why scrolling is preferred to shrinking the game.

## Context

A game that fits a desktop at ordinary scale can become unreadable when controls, status and a large board compete for a smaller effective layout viewport. Disabling zoom would hide the problem by taking away a browser tool. Shrinking everything to fit would preserve a screenshot while undermining target size and text readability.

The decision is to keep browser zoom available and support the specified equivalent layout: 640x400 CSS pixels at device scale 2, representing 200% browser zoom from 1280x800. Both a 4x4 and 16x16 board remain playable, with controls, status and completion summary readable and without outer horizontal overflow.

## Layout response

The page uses a maximum content width and padding rather than a fixed desktop canvas. Controls and statistics can wrap. The large board reaches its minimum card size and scrolls inside its viewport instead of forcing the whole document wider. The completion paragraph wraps as text rather than being locked into a one-line overlay.

The entry's viewport metadata sets width and initial scale without a maximum scale restriction. Cards use `touch-action: manipulation`, which retains panning and pinch zoom while avoiding an unnecessary custom gesture layer. These are concrete ways the application leaves zoom available instead of depending on a separate in-game magnifier.

## Focus is part of the floor

At a constrained viewport, a keyboard destination may lie outside the currently visible board area. Nearest scrolling and card scroll margin keep the entire focus outline visible. Merely checking that the focused button's center enters the viewport would miss a clipped white outline at the edge.

The board has horizontal and vertical overflow, and its height is bounded by the dynamic viewport. Controls remain reachable through ordinary page scrolling. The design does not attempt to freeze the whole game into one physical screen. [ADR 035](adr-035-scroll-large-boards.md) explains that separation.

## Evidence and what it represents

`zoom.e2e.test.ts` creates the stated viewport and device-scale combination, checks the observed values and follows play and reproduction operations. It finishes a small board, loads completed and unfinished progress, resumes, restarts, switches to 16x16 and reaches the last card by keyboard. It also checks page width and readable text.

That is evidence for the specified equivalent layout. It should not be described as automation physically operating every browser's zoom menu or as proof for arbitrary scale factors. The dimensions and device scale are explicit so a reviewer can assess what was tested. [Zoom behaviour](zoom-behaviour.md) gives the product account.

## Alternatives and tradeoffs

A fixed board could be scaled with a transform, but its hit targets and text would shrink along with the drawing. A separate compact interface would add another presentation path whose labels and focus behavior could drift. Hiding status or reproduction tools would make the page look simpler while removing part of the supported journey.

The selected layout accepts scrolling and wrapping as ordinary responses to less space. This can make the page taller, but it preserves the same controls and game meaning. Browser tests include readability and non-overlap checks because fitting the outer boxes alone is insufficient if their text overlaps internally.

[ADR 006](adr-006-native-controls.md), [ADR 024](adr-024-roving-focus.md), [Layout grid](layout-grid.md) and [Accessibility audit](accessibility-audit.md) cover the related foundations. The zoom floor is a stated test configuration and usable layout response, not an additional global claim of standards conformance.

## The explicit equivalent-layout context

The browser test declares viewport and device scale directly. Its journey exercises the same built game rather than a special compact-mode renderer.

```ts
{ viewport: { width: 640, height: 400 }, deviceScaleFactor: 2 }
```
