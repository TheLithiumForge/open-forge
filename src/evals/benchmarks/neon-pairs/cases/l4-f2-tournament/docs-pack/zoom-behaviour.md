# Zoom behaviour
This page describes how the current layout responds to browser zoom, smaller layout viewports and text reflow.

The visual specification defines an equivalent zoom layout at 640x400 CSS pixels with device scale 2, representing 200% browser zoom from 1280x800. It calls for readable controls, status and completion text, a page that stays within the viewport width, and playable four- and sixteen-column boards. [Visuals](visuals.md) V20 is the source of that scope.

## What the equivalent layout means

CSS layout is evaluated in CSS pixels. Increasing device scale alone does not halve a page's CSS width; the test deliberately combines a 640x400 viewport with a scale factor of 2. `tests/zoom.e2e.test.ts` first verifies `innerWidth === 640` and `devicePixelRatio === 2` before checking the game. This avoids mistaking a high-density 1280px layout for the intended smaller layout.

The browser test is evidence for that equivalent geometry. It does not operate a browser's zoom menu, inspect mobile pinch scale or claim that every engine exposes identical zoom behavior. Actual browser zoom remains a useful manual observation, especially for native select menus and the visual viewport. The existing acceptance target is precise about its representative layout.

`index.html` declares `width=device-width, initial-scale=1`. There is no restrictive maximum scale or user-scalable setting in that declaration. The game's styles do not use a whole-page transform to simulate zoom. Text and controls participate in normal layout at the available CSS width.

## The page can grow vertically

`main` has width 100%, a maximum of 1120px and 16px padding. Body margin is zero. At 640px with no consuming page scrollbar, the main content width is 608px. Content stays in normal document flow and the page can scroll vertically. The layout does not attempt to preserve a desktop screen's total height inside a 400px window.

This matters because the board's own height cap is 70 dynamic viewport height units, limited to 768px. At 400px dynamic height the proportional value is 280px. The board may therefore have a small visible window even while controls, help and result text occupy the space above it. The expected interaction includes scrolling the page to the board and scrolling inside the board to reach distant cards.

A readable page at this size is not necessarily a page whose every control is visible at the same instant. The tests ask that text stays within the width, does not overlap, and remains reachable. Clipping the entire document to 400px would fail that intent even if an initial screenshot looked tidy.

## Control wrapping

The top controls are a wrapping flex row with a 12px gap. The size label and select form a smaller flex group. Buttons inherit the system font rather than forcing a separate compact type size. This combination lets text and native controls determine their own useful width.

The narrow-layout media query applies at `max-width: 599px`. The equivalent zoom viewport is 640px, so this particular test runs above that breakpoint. It still benefits from the base `flex-wrap: wrap`. A review that attributes all reflow at 640px to the phone breakpoint is reading the wrong mechanism.

Below the breakpoint, the size group takes a full row with its label and select spaced apart, and top-level action buttons use `flex: 1`. Reproduction action rows have a corresponding button-flex rule. The grid's card-size clamp does not change at that breakpoint; available width and the minimum track size determine whether it scrolls.

[Control bar](control-bar.md) and [Form layout guide](form-layout-guide.md) describe the control behaviors. The visual concern here is that the wrapper can accommodate them without producing horizontal page overflow or overlapping the status underneath.

## Status and completion text

The stats container uses flex wrapping with 12px row and 24px column gaps. Each stat group has `min-width: 8ch`, and its label and value are inline. The elapsed and numeric values use tabular figures where the system font supports them. The result is a group that can move as a unit to a later row instead of placing values over neighboring labels.

The completion paragraph is a normal block with 12px by 16px padding and a border. It has no fixed height or no-wrap declaration. Its sentence can use multiple lines. `GameStatus.tsx` renders the sentence above the keyboard help and board, so a longer line increases document height rather than covering a row of cards.

The game-result area uses a minimum height of 56px. A minimum reserves a useful slot for feedback but still permits growth. The load-status row likewise has a minimum height and wrapping flex layout. Its paragraph has `flex: 1` and `min-width: 0`, making space for the conditional Resume button. These are the dynamic text areas most likely to reveal a reflow problem after the initial screen looks correct.

`tests/readability.ts` examines actual text-node range rectangles for controls, stats, victory, feedback, load notices and the Resume button. It counts text rectangles outside the window width and rectangles that overlap different text sources. This supplements checking the outer boxes, which could fit while their contents overflow.

## The default board

The four-column grid has a 194px minimum and 370px maximum content width. At the representative zoom width, it can keep its maximum card size within the available main content width. Its square cards are still native buttons, with their actual face geometry and no scaled screenshot substitute.

The zoom journey first finishes this board so the completion text is present. It then reloads completed and unfinished snapshots through the reproduction panel, checks readability in those states, resumes the unfinished one and restarts. This deliberately exposes more than the short initial control labels.

After restart the test focuses the first card and activates it with Space. It expects one revealed card. This demonstrates keyboard play within the equivalent layout, not just a visual inspection of a solved board. The game state remains the ordinary state used at other viewport sizes.

## The largest board

Sixteen minimum-size tracks need 794px before viewport padding and border. That cannot fit in the 608px main content width at this layout. The board viewport stays constrained, and the grid scrolls inside it. The outer page remains within the window width.

The same zoom journey selects size 16, verifies 256 rendered cards, activates the first card with a pointer, then uses keyboard navigation to reach the last card. It checks the complete focus outline at both ends and activates the last with Enter. The test consequently exercises a mixed-input path through a scrollable board.

A board screenshot can show fewer than sixteen columns without implying missing cards. All buttons remain in the DOM. The viewport is a window onto them. [Large-board rendering](large-board-rendering.md) gives the rendering and scroll details; [Layout grid](layout-grid.md) provides the minimum-width arithmetic.

## Focus visibility at zoom

The global focus ring is 3px wide with a 2px offset. Its full reach is therefore 5px beyond the card box. Card scroll margin and board padding are each 6px. The geometry does not scale those CSS lengths down to make the ring fit; it scrolls the target into a position with clearance.

`expectFocusInside` measures the ring against both the board client area and the window. A card's border box being visible is insufficient. The helper includes outline width and offset, then checks every edge with a small subpixel tolerance. This is valuable at zoom because short viewports and fractional card widths make clipping easier to overlook.

Keyboard focus uses nearest alignment with browser centering suppressed for explicit focus calls. Pointer focus does not immediately reposition the card under a pressed pointer; the click handler adjusts scrolling after activation. The clipped-pointer tests repeat their journeys at 640x400 and scale 2, covering a distinct failure mode from keyboard navigation.

## Native controls and text limits of the evidence

The game uses the system sans-serif stack and native form elements. Their detailed glyph metrics, popup surfaces and rendering can vary by platform. The stylesheet sets font inheritance for `.control`, but it does not replace the select with a custom menu. A browser screenshot from one environment is therefore not a complete catalogue of native appearance.

The equivalent zoom test checks the existing English strings and representative numeric values. It is not evidence for a translation system or every possible user font setting. Conversely, a wider native control is a real layout input even if the project's main screenshot came from a different system font. The flex and minimum-width declarations are the source to inspect first.

Useful reports state the CSS viewport, device scale, board size, whether the reproduction panel is open and which notice is visible. For an actual browser-zoom report, including the zoom percentage and window dimensions helps relate it to the equivalent case. [Visual review checklist](visual-review-checklist.md) keeps the existing width, readability and focus checks together.
