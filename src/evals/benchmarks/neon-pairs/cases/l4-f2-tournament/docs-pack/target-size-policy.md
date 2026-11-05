# Target size policy

This page records the control target policy and explains the size constraints already present in the interface styles.

Target size is about the area a person can activate, not the size of a glyph drawn inside it. A card's marker is small, but the native button enclosing the whole card is the target. The same distinction applies to a summary's disclosure marker and the much larger summary row.

## Policy and present coverage

The existing code already gives cards a minimum track size and shared controls a minimum height. Extending the interface calls for checking the actual rendered target in the phone layout, where flex wrapping and available width matter. Every new control is at least 44 by 44 CSS pixels at a 390 pixel wide viewport. This policy covers the rendered control area rather than enlarging only a decorative child or a focus ring.

The current styles are useful building blocks, but they are not proof of every dimension for an arbitrary new element. In particular, `.control` declares minimum height without a general minimum width. A new short-label action could be tall enough while still being too narrow. The policy check and the existing class definition therefore answer related but different questions.

This is a control policy, not a claim of complete accessibility conformance. The visuals specification makes the same distinction for its chosen card minimum. Names, keyboard operation, contrast, visible focus and error behavior still have their own evidence. [Interface review checklist](interface-review-checklist.md) gathers those areas without turning a size measurement into a blanket result.

## Size sources in the current code

| Element | Source | Existing size behavior |
| --- | --- | --- |
| Card button | `board.css`, `card.css` | Grid tracks from 44px to 88px, square aspect ratio |
| Button, select, input, textarea using `.control` | `control.css` | Minimum height 44px, padding 8px 12px |
| Reproduction summary | `reproduction.css` | Minimum height 44px, padding 12px 16px |
| Deal action row | `controls.css` | Wrapped flex layout, phone buttons grow |
| Reproduction action row | `reproduction.css` | Wrapped flex layout, phone buttons grow |

`game.css` applies `box-sizing: border-box` globally. The declared height constraints therefore include padding and border. Looking only at text line height would underestimate the target, while adding padding to the measured border box would overestimate it.

Cards have `width: 100%`, `aspect-ratio: 1`, zero padding and a `2px` border. Their width comes from the grid track. They do not establish their minimum using the SVG's coordinates or intrinsic artwork size. The current artwork can change shape while the card target remains the same square.

## Why large boards scroll

The inner board calculates a minimum content width from side length, `--card-min` and `--card-gap`. On a sixteen-column board that is 794px before viewport padding and border. A phone cannot display that whole width at once while keeping the current card minimum.

The implementation chooses an internal scroll area. Its `overflow: auto` and `max-width: 100%` allow the board to extend inside a bounded viewport. The outer page stays within the window. This is the reason a sixteen-column board does not respond like a shrunken thumbnail of a four-column board.

A target-size review can therefore encounter two valid observations at once: every card has its intended dimensions, and some cards are outside the current visible portion of the board. Reachability is supplied by scrolling and keyboard movement. [Pointer and touch](pointer-and-touch.md) explains the touch path to the last card; [Board keyboard navigation](board-keyboard-navigation.md) explains arrow access.

A partially clipped card also has a full underlying button box. Clicking the visible part can activate it, after which the current implementation scrolls it fully into view. That behavior does not mean the clipping itself should be measured as the card's intended dimensions. It is an overflow case with a separate focus and pointer timing concern.

## Phone layout affects widths

At `max-width: 599px`, the deal size group fills the row and uses space-between alignment. New Game and Restart receive `flex: 1`. This gives the actions room without hard-coding two button widths. The text and browser control still contribute their intrinsic size constraints.

The reproduction action rows also wrap, with buttons growing on the phone breakpoint. The Board code input has `flex: 1`, a preferred width of `16ch` and `min-width: 0`. The textarea fills its parent and can resize vertically. These choices keep the text tools within the available width rather than making the outer page horizontally scroll.

A visual review at a wide desktop window can miss the consequences of a new wrapper or label. Selectors such as `.controls > button` and `.control-row > button` apply to direct children. If a wrapper changes which element grows, measuring the wrapper rather than the actual button can hide a target-width problem.

The overall content has `16px` page padding and a maximum width of `1120px`. A viewport width is therefore not the same thing as a control's available row width. Panel padding, gaps and borders consume space too. Reading the size policy as a viewport measurement alone would skip the layout where the target actually lives.

## Visible content versus activation area

A native button may contain text, a shape or multiple SVG pieces. In Card, the face SVG and matched check mark are descendants of one button, and the click callback is attached to that button. Their separate geometry does not produce separate tiny controls.

The neutral back is a `12px` span with a rotated border and `aria-hidden="true"`. It is deliberately smaller than the card. Counting its bounds as the input target would describe the drawing rather than the event-handling element. Similarly, the matched check is a state mark, not a separate action with its own click listener.

Focus outlines sit outside the control border and do not add activation area. Hover border changes likewise do not expand the target. Measurements used to assess the policy concern the actual element box; measurements used to assess focus visibility include outline width and offset. The project already has separate helpers for that distinction in `tests/focus-bounds.ts`.

The reproduction summary is another useful case. Its text and disclosure marker occupy only part of a padded native summary element. The element supplies the disclosure interaction. Replacing it with a visually similar small icon could change both target size and semantics; the current source does neither.

## Zoom, density and CSS units

The policy uses CSS pixels. Device pixel ratio affects the mapping to physical pixels, not the DOM geometry reported by `getBoundingClientRect`. A test at device scale 2 can still report a 44px CSS box. Multiplying the bounding-box result by device scale would answer a different question.

The repository's equivalent zoom layout uses a `640x400` viewport with device scale 2. It checks that controls, statistics and the completion summary remain readable and that both small and large boards remain playable. This is useful complementary evidence for a layout that wraps and scrolls rather than simply shrinking every element.

The game does not disable pinch zoom. Card touch handling uses `touch-action: manipulation`, and index.html does not impose a maximum scale. A target remains part of a browser page whose visible scale can change. [Zoom behaviour](zoom-behaviour.md) covers the visual contract in more detail.

## Existing evidence and what it establishes

`tests/layout.e2e.test.ts` checks the default board and control dimensions at `320x640`. It measures `.controls button`, `.controls select` and `.card` boxes and checks minimum width and height. The same file checks large boards at phone and desktop viewports, where the card floor, viewport height and complete focus ring are important.

Those checks establish the existing tested elements at their stated layouts. They do not automatically cover a future control merely because it shares a color or appears next to a tested button. The source selectors and actual bounding box identify the scope of evidence.

For a new feature review, a concise measurement record can identify the viewport, the control's role and name, its width and height and whether it is fully reachable. The record is most useful alongside the rendered layout, because it can reveal that a formally large button is covered by another element or forced beyond the page edge. This is observation of the target in context, not a second numerical policy.

The current game makes many of these checks straightforward by keeping interaction on native DOM elements. There is no canvas hit-map to reconcile with a drawn target. [Native controls guide](native-controls-guide.md) explains that architectural advantage, while [Contrast audit](contrast-audit.md) and [Focus policy](focus-policy.md) cover other properties that a dimension alone cannot establish.
