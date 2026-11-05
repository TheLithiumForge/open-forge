# CSS organization

This page maps the Neon Pairs stylesheets to their components and explains the global cascade, layout formulas and visual-test seams.

Styles sit beside the behavior they present and are imported by components. They remain ordinary global CSS. File adjacency makes ownership discoverable, while selector choice and the cascade determine actual reach. There is no CSS module transformation, CSS-in-JS runtime or component-library theme layer in this source.

## Stylesheet ownership

| Stylesheet | Importing component or entry | Main responsibility |
| --- | --- | --- |
| `src/game/game.css` | `Game.tsx`; face-sheet entry also imports it | Root theme, page geometry, typography, focus outline and result space |
| `board/board.css` | `Board.tsx` | Viewport scrolling and grid dimensions |
| `board/card.css` | `Card.tsx` | Card states, back, face sizing and matched mark |
| `controls/control.css` | Deal and reproduction components | Shared native-control surface |
| `controls/controls.css` | `Controls.tsx` | Size selector and deal-button arrangement |
| `controls/reproduction.css` | Reproduction components | Disclosure, forms, textarea and load notice |
| `status/status.css` | `GameStatus.tsx` | Statistics and completion presentation |
| `art/face-sheet.css` | Face-sheet entry | Diagnostic placement of real cards |

A component import brings its styles into the application graph, and the build embeds the resulting CSS in the finished HTML. The source HTML has no separate stylesheet link to distribute. [Build pipeline](build-pipeline.md) describes that assembly.

## Global foundation

`game.css` sets `system-ui, sans-serif`, a 16px root font size, 24px line height, light text on a dark background and `color-scheme: dark`. It defines the shared variables `--text`, `--secondary`, `--panel`, `--back`, `--face`, `--border` and `--green`.

The current values are `#f4f6ff` for text, `#b8bdd2` for secondary text, `#141827` for panels, `#1b2034` for card backs, `#111625` for face surfaces, `#757c9a` for borders and `#8bea7d` for matched green. The page background is `#090b14`.

The root also defines `--content-max: 1120px` and `--page-padding: 16px`. A global border-box reset applies to every element, body margin is zero and main uses full width up to that maximum with centered margins and page padding. These foundational rules affect diagnostics that deliberately import `game.css` as well as the main game.

Headings, paragraphs and help text have a small set of explicit margins and font sizes. Secondary text is generally 14px. The global `:focus-visible` outline is 3px white with a 2px offset. Because it is global, its interaction with scroll-container padding matters for cards as well as ordinary controls.

## The board owns its geometry

`board.css` defines board-specific custom properties on `.board-viewport`: card minimum 44px, card maximum 88px, gap 6px, padding 6px and border 1px. The viewport uses `overflow: auto`, contained overscroll, a rounded panel and `max-height: min(70dvh, 768px)`.

The viewport has `width: fit-content` and `max-width: 100%`. The inner board is a grid with a number of columns supplied by `--grid-size`, set from the current game size by `Board.tsx`. Track widths range from the card minimum to maximum.

Its width uses a clamp with these conceptual bounds:

```text
minimum = size * card-min + (size - 1) * gap
preferred = 100% of the available content width
maximum = size * card-max + (size - 1) * gap
```

For size 16, the inner minimum is 794px and maximum is 1498px. The viewport's own padding, border and any scrollbar are separate geometry. A narrow screen can therefore scroll the board rather than shrinking every track without limit or making the whole page wider than the viewport.

The width formula uses the scroll area's content box, which already accounts for occupied scrollbar space. `scrollbars.e2e.test.ts` runs with visible scrollbars at several viewport/size combinations to catch a layout that only works when Chromium hides them.

## Card states come from data

`Card.tsx` supplies `card` plus a state class returned by `describeCard`. `.card` is a square grid-placed native button with full available width, zero padding, a 2px border and an 8px radius. It uses the back surface until revealed or matched, when the face surface applies.

Hover changes the border to the text color. The matched selector appears after the hover selector and gives matched cards the green border. This ordering matters when a matched card remains hovered: its matched treatment continues to win. The class is derived from game data, not a CSS pseudo-state pretending to own the match.

The concealed back is a small rotated bordered span. Visible faces are SVG elements sized to the card. The matched check is a separate absolutely positioned SVG in the top-right corner with width and height at 16% of the card. The source comment explains that a mark much wider than roughly 17% would reach the face's top-right marker.

The card has `scroll-margin: 6px`, which works with nearest scrolling and the global focus outline. Its touch-action and user-selection properties support native pointer behavior. These rules are adjacent to the card because the face sheet also renders the same component outside the normal board layout.

## CSS and SVG share one surface value

The face renderer uses a viewBox of 0 0 100 100. Its main geometry takes the design color, with either a solid fill or outline stroke. A marker circle uses the same color, and its surround stroke is `var(--face)`.

That variable ties SVG geometry to the card's face background. Replacing the card surface without considering the marker surround could make the marker appear to have a mismatched halo. This is a direct dependency between the stylesheet and the inline SVG, even though face identity remains a data mapping in `face-design.ts`.

The matched mark is decorative and uses currentColor from CSS. It is separate from the identity marker, so its size and placement are checked for overlap at the smallest rendered card. [Card artwork](card-artwork.md) and [Face catalogue](face-catalogue.md) cover identity design rather than stylesheet placement.

## Shared controls are deliberately narrow

`control.css` defines `.control`: minimum height 44px, padding 8px 12px, a 1px border, 8px radius, panel background, text color and inherited font. Only button and select controls receive the pointer cursor rule. Inputs and textareas have `min-width: 0` so they can shrink within flex/grid layouts.

This class is applied explicitly to the game's form controls. The card component uses its own class and styling. A broad rule for every button would accidentally affect card rendering and diagnostic comparisons, which is why the current selectors distinguish those surfaces.

`controls.css` uses wrapping flex layout with a 12px gap and 16px bottom margin. The size control aligns label and select with a 12px gap. At widths up to 599px, the size row takes full width with spaced ends, while the direct deal buttons share available space.

These rules handle arrangement without replacing native select and button behavior. The CSS does not define a synthetic menu implementation or a separate touch activation model.

## Reproduction layout

The disclosure panel has 24px top margin, panel background, a 1px border and 12px radius. Its summary has a 44px minimum height and 12px by 16px padding. The open body is a grid with 12px gaps and its own inner padding.

The board-code form is a small grid; `.control-row` is a wrapping flex row with a 12px gap. The code input has a 16ch preferred width and flexible growth. The textarea fills its width and can resize vertically. On narrow screens the row buttons share space.

The load-status row sits outside the details element. It reserves at least 56px, wraps its children and gives the notice paragraph flexible width with `min-width: 0`. The conditional Resume button can therefore remain available even when the disclosure is closed. That structure is defined in JSX; the CSS provides room for the state it renders.

The separate `.game-result` area also reserves 56px and vertically centers its content. These reserved areas reduce layout movement when feedback changes. They do not add game-state transitions or determine which message is shown.

## Statistics and completion

`status.css` lays out the definition list with wrapping flex, a 12px row gap and a 24px column gap. Each statistic group has an 8ch minimum width. `dt` and `dd` are inline; labels use secondary text, and values use tabular numerals with reset margins.

Those `dt` and `dd` selectors are global, not scoped under `.stats`. That is harmless for the current single statistic list but is relevant when reading the cascade. File adjacency should not be mistaken for selector encapsulation.

The completion paragraph has padding, border, radius and bottom margin. The `celebrate` class applies a 600ms one-iteration pulse, whose midpoint changes background and opacity. Victory is still a game-derived paragraph after the pulse. A completed import renders the paragraph without that class because the session's celebration flag is false.

## Existing motion rules

The card transitions only border color for 120ms. A reduced-motion query removes that transition. The status sheet has a reduced-motion query that removes the completion animation. These are two local rules beside the elements they affect.

`motion.e2e.test.ts` reads computed transition properties and animation duration/iteration count, then runs with reduced motion and verifies the removals. This checks the browser result of the cascade rather than merely finding a media query in source. The broader motion rationale belongs to [Motion policy](motion-policy.md).

## Visual evidence and diagnostic independence

`style.e2e.test.ts` measures root colors, typography, page padding, content maximum, board gap and card geometry. It also compares all computed styles before and after hover to ensure the observed change stays within border colors and checks that hovering does not alter exported progress.

`spacing.e2e.test.ts` inspects margins, padding and gaps in completed and paused-import states at phone and desktop widths. It excludes native option internals. The source's spacing values therefore have runtime evidence across more than the initial closed-panel view.

`art.e2e.test.ts` compares computed styles for a revealed game card and a face-sheet card after excluding placement-dependent properties. This checks that the card does not secretly rely on control styles loaded only by the game. It also measures actual geometry at the diagnostic card size.

For a CSS regression, the relevant chain is selector, inherited variable, rendered state and containing geometry. [Large board rendering](large-board-rendering.md), [Focus policy](focus-policy.md) and [Visual review checklist](visual-review-checklist.md) cover the user-facing outcomes that this organization supports.
