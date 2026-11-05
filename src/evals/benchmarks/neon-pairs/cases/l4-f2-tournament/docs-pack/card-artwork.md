# Card artwork
This page traces how `Card.tsx` turns card props into a button, a back or face, and an optional matched mark.

A card is a native button in the board's CSS Grid. Its identity artwork is inline SVG supplied by `Face.tsx`; its closed back is a small bordered span. The renderer does not use a canvas, a bitmap sprite or an external image request. [Face catalogue](face-catalogue.md) explains the shape system, while this page concentrates on the card that contains it.

## The rendering boundary

`src/game/board/Board.tsx` owns the full deck for rendering. It computes face-up positions from `matched` and `selection`, builds a set of matched positions, and maps every deck position to a `Card`. A hidden position receives `face={null}`. A visible position receives its actual numeric face ID.

That prop is the important boundary. `Card.tsx` does not receive the concealed identity and then hide an SVG with CSS. Its conditional expression is `face === null ? <span ... /> : <Face face={face} />`. When the face is null, the face component is not mounted. Hidden identity therefore does not appear in SVG attributes or an invisible face node waiting behind the back.

The numeric `index` is a position, not a face ID. `Board` uses it as the React key and passes it to activation and focus handlers. Two positions can show the same face and remain different buttons. Reusing a face as the key would collapse that distinction and would be particularly inappropriate for a pairs game.

A board replacement changes the `Board` key through `session.generation` in `Game.tsx`. The new board consequently starts a fresh local focus memory. That is independent of whether some numeric positions happen to have the same face as the earlier deal. See [New game and restart](new-game-and-restart.md) for the session behavior.

## Props and responsibilities

| Prop | What the card uses it for |
| --- | --- |
| `index` | Position passed to activation, focus and navigation |
| `size` | Row and column in the generated accessible name |
| `face` | Null for the back, numeric ID for visible artwork |
| `matched` | Matched state class, accessibility state and check mark |
| `tabIndex` | Board-managed tab stop |
| `activate` | Shared gameplay action on a click |
| `focus` | Remembering the focused position and scrolling keyboard focus |
| `clicked` | Bringing a completed pointer action fully into view |
| `navigate` | Board navigation from a key event |

The component does not decide whether a turn is legal, compare two faces, advance elapsed time or update a score. Those are upstream concerns. It presents the supplied state and sends input back through callbacks. This makes it possible for the art sheet to render the same component with no-op callbacks and no live game session.

## The button and its name

The outer element has `type="button"`, `className` built from `card` and the derived state, an `aria-label`, `aria-disabled={matched}`, and the supplied tab index. It remains a native button even when matched. The code uses an accessibility disabled state rather than a native `disabled` attribute, allowing board navigation to retain positions across matched cards.

`describeCard` in `card-state.ts` gives matched state priority, then distinguishes a closed card from a revealed card through whether `face` is null. It calculates one-based row and column from the index and board size. A visible face appends `faceDesign(face).label` to the position and state. A hidden card's name contains no face description.

For a four-column board, index 6 is row 2, column 3. If it shows face 0, its name is `Row 2, column 3, revealed, cyan solid circle, marker top`. This example is derived directly from the helper, not from reading SVG geometry. The face and matched mark are both `aria-hidden`, so they do not duplicate that button name.

The visual artwork and accessible naming share `faceDesign`. This reduces the chance of showing one marker while naming another, but does not remove the need for independent expected-value tests. `face-design.test.ts` verifies all ordered tables, and `card-state.test.ts` exercises representative closed, revealed and matched descriptions.

## Drawing the closed back

Every closed card contains a `span.card-back` with `aria-hidden="true"`. In `card.css`, the span is 12px by 12px with a 2px secondary-color border, rotated 45 degrees. Global border-box sizing applies, so those dimensions describe the unrotated border box. Its visible rotated footprint is larger than 12px, roughly 17px across.

The span has no face-dependent class, style or content. The card background is `var(--back)`. This gives every closed card the same neutral diamond regardless of what will later appear there. The diamond is a back decoration rather than an identity shape: it does not indicate that a diamond face is hidden below it.

Rotation is a static transform, not a motion effect. There are no back-flip keyframes and no transition on the span. On reveal the span is replaced by the face SVG. [Motion policy](motion-policy.md) describes the limited border interpolation that can happen at the same time.

## The face surface

`.card.revealed` and `.card.matched` both switch the background to `var(--face)`. The direct child SVG receives `display: block`, `width: 100%` and `height: 100%`. `Face.tsx` uses a square 100-unit view box, so the artwork scales with the square card interior.

The card has a 2px border and zero padding. At the minimum 44px outer size, the inner drawing area is approximately 40px square. A five-unit SVG stroke is consequently about 2px in that interior, while the five-unit marker radius is about 2px. These figures explain why a face should be reviewed in its actual card rather than as a 100px illustration on a white page.

The main geometry lives in the middle of the view box and the marker sits farther out. The marker is drawn after the main shape, with a face-colored surround. It remains legible where it approaches a diagonal corner of a square. The fill/outline distinction is preserved without changing the shape's position.

## Adding the matched mark

A matched card keeps its face and adds a second SVG after it. This SVG has class `matched-mark`, `viewBox="0 0 12 12"`, and a check path `m2 6 3 3 5-6`. Its path has no fill, uses `currentColor`, and has stroke width 2.

The check is absolutely positioned at top 0 and right 0 inside the card. The selector `.card > .matched-mark` overrides the general full-size SVG rule with 16% width and height. It takes `var(--green)` as its color. The more specific selector is important: without it, the check would occupy the full card and cover the identity.

The source comment explains that a corner mark wider than about 17% would reach the top-right marker. The chosen 16% is therefore part of the face-clearance geometry, not a decorative scale picked independently of the artwork. `tests/matched-mark.e2e.test.ts` checks every identity in a completed small-card board, including the marker circle's stroke and the main shape's outline extent.

Matched cards also receive a green border. Their opacity remains 1. A faded completed pair would reduce the visible information available for remembering the board and would diverge from V15. The check, persistent face and state name work together; the border is only one cue.

## Interaction styling

The card is relatively positioned, takes the width of its grid track, and uses `aspect-ratio: 1`. Grid centering places the back in the middle. Border radius is 8px. `user-select: none` and its WebKit form prevent selection of its decorative contents; `touch-action: manipulation` leaves normal browser touch behavior available.

Hover changes the border to the text color, with the matched border rule taking precedence. The global focus-visible rule adds a white outline outside the border. A 6px scroll margin supports visibility of that outline as the board scrolls. The card has no clipping rule that intentionally cuts its own SVG or focus ring into a rounded image mask.

On click, the handler calls `activate(index)` first and `clicked(event.currentTarget)` second. The latter lets the board adjust scrolling after the press has completed. On focus, the card passes both its index and element to the board hook. This ordering matters when pressing a partly visible card at a scroll edge; [Large-board rendering](large-board-rendering.md) follows the interaction in detail.

## Reuse and verification

`Card` is wrapped in `memo`. `Board` stabilizes its activation callback with a ref to the latest handler and memoizes its face-up and matched sets. A clock update that preserves the selection and matched arrays can therefore update status without rendering every card again. This is a rendering optimization rather than a second source of card state.

`art/face-sheet.tsx` imports the real `Card` and root styles, then places each face in a 44px swatch. The browser art test compares computed styles of a revealed game card and a sheet card, excluding placement-dependent properties. This catches accidental dependence on control styles or a surrounding game layout. The sheet is an inspection fixture, not a separate card renderer.

For artwork work, the useful evidence is the real small card, the grayscale and neutral face sheets, and matched-mark clearance. For an interaction change, the keyboard and clipped-pointer journeys matter too. [Visual review checklist](visual-review-checklist.md) collects those entry points without making the artwork component responsible for gameplay rules.
