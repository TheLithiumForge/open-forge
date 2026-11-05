# Theme tokens
This page inventories the CSS custom properties and related literal values that make up the current Neon Pairs presentation.

There is one implemented dark presentation. Its shared declarations are in `src/game/game.css`, board geometry is declared in `src/game/board/board.css`, and artwork colors are data in `src/game/board/face-design.ts`. The source does not contain a theme selector or a second set of root color overrides. [Color policy](color-policy.md) explains the roles; this page is the reference for finding their declarations and consumers.

## Root declarations

| Custom property | Value | Current consumers |
| --- | --- | --- |
| `--text` | `#f4f6ff` | Control text, card foreground and hover border, victory text |
| `--secondary` | `#b8bdd2` | Eyebrow, intro, help, stat labels, card-back border |
| `--panel` | `#141827` | Board viewport, controls, reproduction panel, pulse midpoint |
| `--back` | `#1b2034` | Closed-card background |
| `--face` | `#111625` | Revealed and matched card background, marker surround |
| `--border` | `#757c9a` | Board, controls, cards, reproduction and victory borders |
| `--green` | `#8bea7d` | Matched-card border and matched-mark color |
| `--content-max` | `1120px` | Main container maximum width |
| `--page-padding` | `16px` | Main container padding |

These are ordinary inherited custom properties declared on `:root`. The stylesheet separately declares `color: #f4f6ff` and `background: #090b14` on the root. The root foreground happens to equal `--text`, but its declaration is a literal rather than `color: var(--text)`. A token-only edit would therefore not automatically change the inherited body foreground at that location.

The page background has no `--page` custom property in the source. The visual specification calls the role Page, but the code expresses it as a root background literal. Similarly, the Focus role is implemented with a white literal in the global focus-visible rule, not a `--focus` variable. Role names in the specification and variable names in the implementation are related without being identical lists.

The root also sets `color-scheme: dark`. This gives the browser information for native controls and browser-rendered surfaces. It is not a palette generator and does not define a light alternative. The explicit panel, text and border declarations remain the authored appearance of `.control` elements.

## Board-local values

| Custom property | Declaration site | Value |
| --- | --- | --- |
| `--card-min` | `.board-viewport` | `44px` |
| `--card-max` | `.board-viewport` | `88px` |
| `--card-gap` | `.board-viewport` | `6px` |
| `--board-padding` | `.board-viewport` | `6px` |
| `--board-border` | `.board-viewport` | `1px` |
| `--grid-size` | Inline style on `.board` | Current game size |

`Board.tsx` defines a `BoardStyle` interface extending `CSSProperties` with a numeric `--grid-size` property. The component supplies that value from the game rather than creating classes for each supported size. `board.css` uses it in both the repeated grid track list and the clamp arithmetic.

The local viewport variables inherit into `.board`. This scope keeps card dimensions and scroll padding together without exposing a separate global spacing system. A face sheet can use `Card` outside a board because the card's own styles do not depend on these board-only variables. Its placement supplies the dimensions instead.

`--board-border` controls the scroll area's border width. It is unrelated to the card's literal 2px border. Confusing the two would change the available content width calculation and the card silhouette in different ways. [Layout grid](layout-grid.md) shows the arithmetic with padding and borders kept separate.

## Values that are not tokens

Several frequently reviewed values remain literal declarations: 8px card and control radii, 12px board and reproduction radii, the white focus outline, its 3px width and 2px offset, the card's 6px scroll margin, the 120ms border transition and the 600ms completion pulse.

The spacing scale is also expressed through repeated CSS lengths, not named `--space-*` properties. For example, controls use a 12px gap and a 16px bottom margin, while the reproduction panel uses a 24px top margin. [Spacing scale](spacing-scale.md) maps those uses to the specification's allowed values.

Typography uses root font declarations and class-specific sizes. There is no `--font-body`, `--font-title` or loaded font asset. The system stack and the three main type sizes are documented in [Typography](typography.md). Searching for a token that does not exist can make a small style change look more architectural than it is.

## Artwork data versus CSS

`FACE_COLORS` contains eight hex strings in the order cyan, magenta, green, yellow, orange, violet, rose and ice. `COLOR_NAMES` is a parallel ordered table used in labels. Neither array is a collection of CSS custom-property references. `Face.tsx` supplies the selected string directly as an SVG fill or stroke attribute.

The marker's surround is the exception: its stroke attribute is `var(--face)`, so it resolves the inherited CSS surface token inside the SVG. The artwork color is fixed by the face table, while the separating surround follows the card surface. This is why a root token edit can affect face readability without changing `FACE_COLORS`.

The green palette entry and `--green` have the same current hex value but separate declaration sites. An edit to the CSS state color alone does not recolor right-marker artwork. An edit to the TypeScript entry alone does not change matched borders. A reviewer can decide whether both changes were intended by examining their roles, rather than assuming the duplicate literal is accidental.

`COLOR_NAMES` contributes visible-to-assistive-technology descriptions through the card name. Changing a hex string without its name can leave the label describing the old hue. Changing a name without a color change affects descriptions even if screenshots remain identical. [Face catalogue](face-catalogue.md) covers the ordered identity tables in detail.

## Cascade paths worth tracing

For a closed card, background resolves from `.card` to `--back`; its border starts at `--border`. Revealing it selects `.card.revealed` and changes background to `--face`. Matching it selects `.card.matched`, which retains the face surface and changes border color to `--green`. The check mark also uses `--green` through its own `color` and the path's `currentColor` stroke.

Hover selects `--text` as the card border color. The matched rule follows the hover rule with equal specificity, so a matched card keeps its state border. Keyboard focus instead adds the global white outline. Because border and outline are different properties, both the persistent state and focus indicator can remain visible at once.

A `.control` gets panel background, main text, a neutral border and inherited font in `control.css`. The root `color-scheme` leaves the native renderer aware of the dark environment. There is no card selector that makes all buttons adopt card art, and no generic SVG rule in the controls file that sizes the face. This separation is exercised by the art-sheet style comparison.

The victory paragraph is transparent in its ordinary stylesheet rule. It inherits the page behind it while explicitly setting main text and a neutral border. During the pulse, its background becomes panel-colored at the midpoint and the element is partially opaque. Looking for a permanent victory-panel background in the base rule would misread the keyframes.

## Metadata outside the stylesheet

`index.html` uses `#090B14` in its theme-color metadata, matching the current page role. This is a separate literal from the CSS background. A page-background edit that leaves metadata unchanged can produce different browser chrome and page colors even if the game itself renders as intended.

The art fixture imports `game.css` to receive the same shared values. Its own stylesheet gives figures a face-colored background and a neutral border, and captions secondary text. The fixture's layout is for inspection; its 124px columns do not describe the production board's card tracks.

The Vite configuration inlines the built application through the single-file plugin. These properties are therefore part of the shipped inline styles rather than fetched from a theme service. Delivery details are in [Offline delivery](offline-delivery.md); the visual consequence is that appearance does not depend on a runtime palette request.

## Verification and change review

The style browser test checks computed colors, type sizes, main padding and maximum width, grid gap, card border and radius. The product-contract test additionally checks named panel targets including the reproduction section and non-card native controls. Those assertions find a token that is declared correctly but overridden at a consumer.

The art browser test checks all selected SVG colors and the resolved marker surround. Contrast calculations need the resulting foreground/background pairing, not only a list of declared variables. [Contrast audit](contrast-audit.md) documents the current ratios and the separate pulse compositing case.

A practical change description identifies whether it alters a shared declaration, a literal consumer, an artwork table or native-control behavior. That gives the reviewer a small, relevant set of evidence: a control screenshot, a face sheet, a focus edge or a measured color pair. The current source is small enough that tracing these direct relationships is clearer than assuming a broader theme framework.
