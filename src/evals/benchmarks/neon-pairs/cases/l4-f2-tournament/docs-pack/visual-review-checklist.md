# Visual review checklist
This page groups the existing visual requirements and test evidence into a practical review of a Neon Pairs change.

The baseline is [Visuals](visuals.md), backed by the current source and browser tests. It is organized by what a reviewer can inspect, with enough source pointers to distinguish an artwork regression from a layout, focus or state problem.

## Start with the affected surface

A change to `game.css` can reach most of the interface. A change to `card.css` reaches the real game and the art sheet. A change to `face-design.ts` can change artwork and accessible descriptions without changing CSS. A change to the board focus hooks can alter where a correctly drawn card appears after navigation.

The useful review scope follows those consumers. There is little value in repeating every unrelated gameplay journey for a small caption edit, while a global root color edit needs more than one closed-card screenshot. The mappings below describe the evidence already present in the project.

| Change area | Existing evidence most directly related |
| --- | --- |
| Root colors, type and page geometry | Style and product-contract browser tests |
| Face tables and paths | Face-design tests and art-sheet browser tests |
| Matched check and card states | Style, matched-mark and art browser tests |
| Grid width or viewport sizing | Layout and visible-scrollbar browser tests |
| Focus scrolling | Layout, keyboard, pointer-focus and focus-bound checks |
| Motion | Normal and reduced-motion browser tests |
| Wrapping text or controls | Zoom, readability and spacing checks |

These are source file groups under `tests` and `src/game/board`. Build and execution details belong in [Browser test guide](browser-test-guide.md) and [Test strategy](test-strategy.md).

## Page and control layout

- The page uses a centered main area capped at 1120px with 16px padding, and does not gain horizontal overflow from the board.
- The default four-column board fits the narrow-phone case; the existing layout test includes 320x640 in addition to the stated visual viewport cases.
- The top control bar wraps, the size group changes arrangement below the existing 599px breakpoint, and native controls retain inherited type.
- Stats groups wrap without labels or values colliding. The completion sentence and dynamic notices remain in normal flow.
- Opening the reproduction panel exposes its full-width textarea, wrapping action rows and notices without widening the outer page.

These observations correspond to V02, V05, V08, V09 and V20 and to the current control CSS. [Layout grid](layout-grid.md) gives the box arithmetic; [Zoom behaviour](zoom-behaviour.md) separates the 640px equivalent layout from the narrower phone breakpoint.

A container fitting within the window is useful but incomplete evidence. `expectReadableText` measures actual text rectangles and can find an overflowing label inside an otherwise well-sized box. The zoom test invokes it after completion and after both completed and unfinished snapshot loads, including the conditional Resume control.

## Board geometry and scrolling

- Cards remain square, within the 44px to 88px sizing range, with a 6px grid gap, 2px border and 8px radius.
- The scroll viewport uses 6px padding and a maximum height of `min(70dvh, 768px)`.
- The 16x16 board contains all 256 card buttons at 360x800 and 1280x800, while excess grid size remains inside its viewport.
- First, last and interior cards can be reached through the existing keyboard path with their full focus outlines visible.
- Touch scrolling reaches the distant end without revealing a card merely because the gesture crossed one.

The taller 1280x1400 layout test also exercises the absolute height cap. The visible-scrollbar tests check cases where tracks should fit beside consuming scrollbars. Those cases complement the phone largest board, where horizontal board scrolling is expected. [Large-board rendering](large-board-rendering.md) explains the distinction.

For focus, the measured object includes the outline's 3px width and 2px offset, not just the button's rectangle. The board client area and window edges both matter. An interior card reached by nearest alignment is a useful check because physical end padding cannot protect it in the same way it protects the outermost row.

## Presses near scroll edges

The pointer-focus test deliberately clicks and taps a partly clipped card at the board's bottom edge. It covers phone, desktop and equivalent zoom layouts, including actions after scrolling and after returning from the reproduction panel. The intended card reveals, then settles fully into view.

This journey catches an error that a normal centered-card click does not. If a focus handler scrolls the card before release, the press target can move. The current implementation postpones pointer scrolling until the click handler, while keyboard focus takes its own nearest-scroll path. [Focus policy](focus-policy.md) and [Pointer and touch](pointer-and-touch.md) provide the wider input context.

## Artwork at the real size

- Closed cards have the same neutral diamond back and no mounted identity artwork.
- Revealed cards show the shape, marker and fill generated for their ID.
- Matched cards preserve that face, add the green border and check, and retain full opacity.
- The check remains clear of both the marker circle and central shape on the actual smallest cards.
- All 128 identities remain geometrically distinct in the grayscale and neutralized face-sheet checks.

The art fixture imports the production `Card`; it is not a second interpretation of the drawings. Its browser test compares non-placement computed styles with a revealed game card. A change that only looks right in the fixture can therefore expose a real style dependency rather than a harmless fixture difference.

The most crowded cases include diagonal markers beside square corners. Solid and outline variants need comparison at 44px, not just at a generous illustration size. [Face catalogue](face-catalogue.md) provides IDs and paths; [Card artwork](card-artwork.md) explains the approximately 40px inner drawing area inside a 44px bordered card.

The neutral-raster uniqueness test is strong evidence that different IDs do not collapse to identical small silhouettes. Human inspection still answers whether two similar silhouettes are comfortably distinguishable. Those are related observations rather than interchangeable proofs.

## Colors, hover and focus

The style tests compare the declared visual roles against computed RGB values. The product-contract test adds panel backgrounds on reproduction and non-card controls, secondary text roles, and the marker surround. The art test checks palette use for every face. Together they cover CSS surface colors and TypeScript artwork colors.

Hover changes only the card border to the main text color. Matched borders remain green under hover. Keyboard focus adds a white outline rather than replacing the persistent border. These observations follow V15 and V16 and can be checked while one card is closed, another revealed and a pair matched.

[Contrast audit](contrast-audit.md) records the calculated ratios and their limitations. It separates opaque colors from the completion pulse's temporary opacity. A high contrast value does not excuse a clipped outline, and a passing grayscale sheet does not establish text contrast.

## Motion and completion

The ordinary card transition is limited to border color for 120ms. Completion adds one 600ms pulse to the victory paragraph, followed by its static appearance. Reduced motion removes those two authored effects. The normal and reduced-motion tests inspect computed properties instead of inferring their absence from a late screenshot.

A completed import is a useful separate presentation state: it shows the completion paragraph without a new celebration. The product-contract test checks the absence of the celebration class on that path. [Motion policy](motion-policy.md) traces the session flag and CSS behavior.

There is no authored card flip, scaling hover or particle layer in the current game. Those facts help a reviewer recognize an unintended effect introduced by a broad transition declaration. The existing V16 through V18 behavior is the comparison point.

## Spacing and type evidence

The spacing audit examines computed margins, padding and row/column gaps, excluding native option internals. It covers a completed game with the reproduction panel open and an unfinished imported game with its Resume notice. [Spacing scale](spacing-scale.md) lists the source values and explains why borders, radii and font metrics are separate.

The type checks cover the 16px root, 24px root line height, 24px title and 14px secondary roles. Native controls inherit the font. Platform system fonts can vary in width, so the rendering evidence is strongest when text and boxes are both checked. [Typography](typography.md) follows inheritance and the tabular-numeric value styling.

## Making a finding actionable

A visual finding is easier to resolve when it names the current state, board size, CSS viewport and affected element. For artwork, the face ID and fill/marker combination locate the exact geometry. For scrolling, the target position, input method and client versus scroll dimensions distinguish necessary overflow from a regression.

An evidence note can be short: “At 360x800 on size 16, keyboard focus at row 9, column 9 clips the right side of the outline; the card box itself is visible.” That describes the failing existing requirement and points directly toward the focus-bound helper.

The pages linked here supply the detailed source explanations. This checklist is the entry point for selecting the relevant existing observations, keeping the review concrete without treating every visual change as a redesign of the game.
