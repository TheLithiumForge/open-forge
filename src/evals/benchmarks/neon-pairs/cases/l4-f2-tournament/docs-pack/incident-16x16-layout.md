# Incident review: the sixteen-column board widened the page

This is an incident review about a large-board layout that made the outer page wider than the viewport and hid the useful part of keyboard focus.

The board now has its own constrained scroll area.

## The screenshot looked like a successful large board

The initial report showed all 256 cards on a desktop-sized image. Nothing appeared missing. The failure became visible on a narrow phone: choosing 16x16 widened the document, controls no longer fitted the visible page, and reaching the rightmost column moved the whole screen sideways. Returning to New Game required finding the left edge of the page again.

There was a second symptom for keyboard use. The active element reached the final card, but its white outline was partly clipped against the edge of the board window. A check that asserted only the focused index would pass. A player could still lose confidence about which card was selected.

The two symptoms shared a mistaken view of success: the prototype had rendered the right number of cards but had not defined the viewport around them. The [large-board release notes](release-notes-1-2.md) explain the broader design choice; this page follows the debugging details.

## The minimum width was real

At the current 44px card minimum and 6px gap, sixteen columns need 794px for tracks and gaps. A phone cannot show that entire row while keeping those values. The review did not find a magic responsive formula that removed this constraint. It identified which surface should own the unavoidable overflow.

Shrinking targets until the row fitted made the failure disappear from a width assertion while creating another problem: the faces and controls became too small for the intended design. Allowing the document itself to grow preserved cards but damaged everything outside the board. The chosen result was a stable document with an internally scrollable board.

This is why the current `.board-viewport` has `overflow: auto`, `max-width: 100%`, and a bounded height. The grid can retain its minimum content width while its containing window remains within the page. Cards preserve row-order identity whether currently visible or outside that window.

## The first width fix was only half a fix

An early patch added horizontal overflow to the board but left its parent participating in minimum-content sizing. The parent still claimed the grid's wide intrinsic size, so the document remained wide. Another patch set a width based directly on the browser's inner width, forgetting the page padding and the board's own padding and border.

The current layout derives available width through the containing boxes. `main` is full width with a maximum of 1120px and 16px padding, under universal `border-box` sizing. The board viewport uses fit-content width constrained to its parent. The grid's width is clamped between the width of minimum tracks and maximum tracks, with the available content width between them.

That shape gives a small board room to grow up to its maximum card size without stretching indefinitely, while a large board keeps a larger scrollable content width when necessary. It also avoids making a global viewport calculation responsible for a local content-box question.

## Scrollbars changed the available space

The next failure appeared only with classic scrollbars. A width formula that looked right with overlaid scrollbars left a horizontal excess when a vertical scrollbar consumed actual space. The reviewer initially suspected a fractional pixel rounding issue because the visible difference was small.

The important observation was that the scroll area's content box already reflected scrollbar space. Subtracting another guessed width could make the board unnecessarily small; ignoring the content box and using an outer measurement could make it too large. The current `board.css` comment calls out this exact responsibility.

The repository has `tests/scrollbars.e2e.test.ts` as well as general layout checks. This is useful evidence because “desktop browser” is not one scrollbar geometry. A review that only uses the author's default environment may never exercise space-consuming scrollbars.

The historical repair did not hard-code a scrollbar width. It arranged containment so the browser's box calculations supplied the available space. The [layout grid page](layout-grid.md) gives the current CSS explanation without the incident narrative.

## Height created a second navigation surface

The board's current maximum height is `min(70dvh, 768px)`. A sixteen-row grid can therefore scroll vertically as well as horizontally. Controls and summaries remain in normal page flow, while the board window holds the larger spatial task.

During the review, a full-viewport board seemed attractive because it maximized visible cards. It also crowded out status and made the route back to controls less obvious. A bounded area preserved context around the board. The use of dynamic viewport height responds to the available browser space rather than assuming a permanently visible phone chrome configuration.

The current `overscroll-behavior: contain` keeps the board's scrolling surface more self-contained. That does not make the entire document fixed or prevent ordinary page movement elsewhere. It simply reflects the design that the grid's overflow belongs inside its own region.

## Focus needed room beyond the card rectangle

The clipped outline survived the width correction because a button's visible bounds do not include its outline. Current focus styling adds a 3px outline with a 2px offset. A card brought flush to a scroll edge could therefore have its border visible while much of its focus treatment remained outside the clipped area.

Cards now carry a 6px scroll margin, and the viewport has 6px padding. The margin gives nearest scrolling a target area larger than the card itself. Browser focus-bound helpers inspect gaps around the focused card, including mid-board positions that approach an edge from within the grid.

The distinction matters for Tab entry as well as arrow movement. The current nearest-scroll helper counters browser centering from the remembered board position, then applies nearest alignment. A player returning to a far card should see the relevant edge and full outline without having the board jump to an arbitrary centered view.

## Pointer focus could not move the pressed target

The investigation briefly introduced a regression by scrolling whenever any card received focus. A pointer press can focus a partly visible card before the click completes. If that focus handler moves the board, the release may occur over a different place and the expected click can be lost.

The current `cardFocused` path scrolls immediately when the card matches `:focus-visible`. Pointer interaction can complete before the click callback scrolls with nearest alignment. The same core activation remains shared, but the timing of visual movement respects the input method's event sequence.

This was a useful reminder that a keyboard fix can have pointer consequences without changing a single game transition. The [pointer and touch page](pointer-and-touch.md) and [focus policy](focus-policy.md) cover the current distinction. Touch scrolling has its own browser journey because dragging the board is not a completed tap.

## What counted as resolution

The closeout used first and last cards, not only a centered screenshot. At 360x800 and 1280x800 CSS pixels, the board had all 256 buttons, the outer page stayed within the viewport width, and the controls remained reachable. Touch could reach the far end, and keyboard movement could reach it with a complete outline visible.

The equivalent zoom layout added another pressure case: 640x400 CSS pixels at device scale 2. Readable controls and summaries mattered there as much as card geometry. The test suite contains a journey for that specific configuration; it is not evidence of testing every physical phone or every browser zoom implementation.

The incident's lasting change was the definition of layout success. Rendering all positions was necessary, but reachability, containment, and visible focus made those positions usable. The [phone research](research-phone-usage.md) returns to that lesson from a player's point of view.
