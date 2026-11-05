# Color policy
This page explains the current color roles in Neon Pairs and how color works with geometry, text and state indicators.

The palette is a dark page with pale text, neutral borders and eight bright artwork colors. The source has a shared set of CSS custom properties in `src/game/game.css` and an ordered artwork palette in `src/game/board/face-design.ts`. These two locations serve different purposes: one styles surfaces and interface states, while the other derives a face from its numeric identity. [Theme tokens](theme-tokens.md) inventories the declarations.

## Color roles in the interface

The page background is `#090B14`. Panels and native form controls use `#141827`; closed cards use `#1B2034`; face-up cards use `#111625`. These dark surfaces are intentionally close in value. A reader does not have to distinguish the board's panel from the page background to find its boundary because the scroll area also has a neutral border.

The main text color is `#F4F6FF` and secondary text is `#B8BDD2`. The secondary role appears in the eyebrow, intro, keyboard help and status labels. It is a text role with substantial contrast on the page, rather than a disabled treatment. The move and pair values retain the main text color. [Typography](typography.md) describes how size and placement reinforce this hierarchy.

`#757C9A` supplies neutral borders, and the keyboard focus outline is white. The focus outline is a separate visual layer from the card border: it can be seen while the card remains green after a match. There is no need to replace the persistent state border with a focus color. The shape of the outline and its offset also distinguish focus from a border change.

## State information

A matched card retains its face, gains a green border and displays a check mark in the upper-right corner. State is never shown by color alone, so a winner is marked with the text "(winner)" after the name. The card implementation similarly includes a non-color mark alongside its green state border. Its accessible name also carries its state, independently of the SVG appearance.

In the current game, `Card.tsx` draws the matched check as an inline SVG, and `describeCard` supplies the words `closed`, `revealed` or `matched` in the button name. The renderer does not infer those words by reading computed colors. That makes the state understandable in the DOM even when colors are unavailable or customized by the browser.

A revealed card also differs from a closed card through its artwork. The closed card contains a neutral diamond back; the face-up card contains its actual shape and marker. The slight background difference is supporting information. It is not the sole indication that a card has been revealed.

Completion appears as a sentence above the board, not as a board-wide recoloring. The text includes the result and the recorded performance. The short completion pulse described in [Motion policy](motion-policy.md) is supplementary: the same sentence persists after it ends and is immediately static in the reduced-motion presentation.

## The artwork palette is ordered data

The ordered colors are Cyan `#63E7FF`, Magenta `#FF71D6`, Green `#8BEA7D`, Yellow `#FFE082`, Orange `#FFB064`, Violet `#B49AFF`, Rose `#FF8296` and Ice `#D9F3FF`. These names appear in lowercase in generated face descriptions. They are not arbitrary aliases that can be reordered without changing the meaning of an ID.

`faceDesign` calculates the marker index as `floor(face / 8) % 8` and uses that index for both marker position and color. Top markers are cyan, top-right markers magenta, and so on around the shape. The color repeats a positional distinction that is already present in the geometry. It does not multiply the number of identities.

For example, IDs 0 and 64 are both cyan circles with top markers; one is solid and the other outlined. IDs 0 and 8 share a solid circle but use different marker positions and therefore different colors. A color-only view would collapse some distinct identities, while a geometry-only view preserves the shape, marker and fill combination. See [Face catalogue](face-catalogue.md) for the complete indexing scheme.

The solid shape's fill, the outlined shape's stroke and the marker's fill all use the same face color. The marker has an additional stroke in `var(--face)`, separating it from nearby geometry. That surround follows the face surface token rather than using black or the page background. Changing the face surface without checking the marker surround can alter the most crowded diagonal combinations.

## Neutral and semantic green

Green has two current roles. `--green` supplies matched-card borders and check marks. The Green entry in `FACE_COLORS` supplies faces whose marker points right. Both currently have the value `#8BEA7D`, but they are declared separately in CSS and TypeScript.

A green face therefore does not imply a matched card. It can be a newly revealed, unmatched identity just like a cyan or violet face. The check mark and border are state presentation; the shape, marker and fill are pair identity. This separation is easy to miss when evaluating a screenshot containing only green artwork.

The same issue appears in automated selectors. The current tests find matched cards by `.card.matched` and inspect their border and mark. They do not locate matches by scanning for green pixels. Pixel measurements are useful for contrast and geometry, but game meaning already has an explicit source in props and class names.

## Hover and focus

The neutral card border becomes the main text color on hover. V16 limits hover to that border change. The matched rule appears after the hover rule and has equal specificity, so the green matched border remains in place while hovered. The artwork itself does not brighten or become dimmer.

Keyboard focus uses `:focus-visible` globally, with a 3px white outline offset 2px from the element. The board gives cards a 6px scroll margin and its viewport 6px padding so the ring can remain visible at edges. Focus readability consequently depends on layout as well as the white value. A high numerical contrast ratio cannot compensate for a clipped outline.

The current `.control` class supplies the same text, panel and neutral-border colors to buttons, selects, inputs and the textarea. Their internal browser affordances still depend on the native control renderer. `color-scheme: dark` is declared at the root; it is not a separately implemented light theme. [Native controls guide](native-controls-guide.md) covers that boundary.

## How the palette is checked

`tests/style.e2e.test.ts` compares computed RGB values for the page, panel, card back, face, text, secondary text, border and focus outline. It also checks that hovering changes only border colors and that a matched card stays fully opaque. These checks make a useful distinction between a token being declared and actually reaching its intended element.

The art tests inspect SVG fill and stroke attributes across all 128 faces. They compare each to an independent palette array indexed from the face ID. A wrong color table entry cannot be hidden by the general CSS token tests because those tests exercise a different source of color.

The face sheet is also rendered in grayscale, then with artwork recolored to one neutral light value. The latter gives the test a stronger way to ask whether geometry alone remains distinct. It compares raster silhouettes of the actual small cards, so a passing color table is not treated as proof that the tiny faces remain distinguishable.

[Contrast audit](contrast-audit.md) gives calculated ratios for the opaque colors. Its figures describe the supplied sRGB values and their actual surface pairings, with rounding stated there. They do not claim that an arbitrary screenshot, display brightness setting or substituted native control has those same sampled pixel values.

## Review implications

A color edit can affect more than the element a reviewer first notices. Changing `--face` alters the revealed background and the marker surround. Changing `--green` affects state marks but leaves the artwork array alone. Changing the artwork array affects SVG appearance and names if its corresponding name table also changes. These are separate edits with separate evidence.

The least ambiguous review records the role, its source declaration and the visible state used to inspect it. A report such as “the top-right marker merges with the square at the small size” is actionable because it identifies geometry and size. “The purple is wrong” leaves open whether the issue is the palette, a face name, a filter, an active transition or a display setting.

The existing dark presentation is documented in [Visuals](visuals.md). The current colors remain useful when their hue distinctions cannot carry the whole interaction.
