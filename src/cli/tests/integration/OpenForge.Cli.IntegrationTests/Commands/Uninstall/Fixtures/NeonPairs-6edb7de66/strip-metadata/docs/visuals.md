# Visuals
Visuals is a design specification that defines the layout, colors and card artwork for Neon Pairs.

Status: spec version 2.2.

A card face combines a shape, a marker position and a fill. You can identify a pair from those features even when you can't distinguish its color.

## Layout

- V01 - Use native controls and a CSS Grid board of DOM card buttons with inline SVG artwork. Render the game without canvas. Use no component library.
- V02 - Center the content within a maximum width of 1120px and give it 16px padding. Keep the outer page within the viewport width.
- V03 - Limit the board's scroll area to `70dvh`, capped at 768px tall. Give it 6px padding and allow overflow on both axes inside that area.
- V04 - Make each card square and size it from 44px to 88px according to the available board width. Use a 6px grid gap, a 2px card border and an 8px card radius.
- V05 - Fit the default four-column board within a phone viewport. When a larger board reaches the 44px minimum card size, keep that size and scroll inside the board.

For example, a sixteen-column board needs 794px for its cards and gaps, plus 12px for board padding. On a phone, the board scrolls while the outer page stays within the viewport width.

The 44px minimum is the chosen card target size. It doesn't state that the complete game conforms to an accessibility standard.

## Colors

- V06 - Use these colors for the page and controls:

| Token | Color | Use |
| --- | --- | --- |
| Page | `#090B14` | Page background |
| Panel | `#141827` | Board scroll area, reproduction panel and non-card button, select, input and textarea backgrounds |
| Back | `#1B2034` | Hidden card background |
| Face | `#111625` | Revealed card background |
| Text | `#F4F6FF` | Main text |
| Secondary | `#B8BDD2` | Help text, status labels and optional supporting text |
| Border | `#757C9A` | Neutral borders |
| Focus | `#FFFFFF` | Focus outline |

- V07 - Use this ordered palette for card artwork:

| Index | Name | Color |
| --- | --- | --- |
| 0 | Cyan | `#63E7FF` |
| 1 | Magenta | `#FF71D6` |
| 2 | Green | `#8BEA7D` |
| 3 | Yellow | `#FFE082` |
| 4 | Orange | `#FFB064` |
| 5 | Violet | `#B49AFF` |
| 6 | Rose | `#FF8296` |
| 7 | Ice | `#D9F3FF` |

## Type and spacing

- V08 - Use the system sans-serif font stack. Set body text to 16px with a 24px line height, the title to 24px and secondary text to 14px. Secondary text means help text, status labels and optional supporting text.

- V09 - Use spacing values of 4px, 6px, 8px, 12px, 16px, 24px and 32px for positive layout gaps, padding and margins you set. Allow zero resets and automatic centering. Reset browser default margins. Native control internals aren't covered. Card borders, radii, focus outlines, font metrics and SVG coordinates use their own stated values.

## Card faces

- V10 - Generate 128 face identities from integer face IDs `0` through `127`. Calculate each face from these ordered values:

| Part | Calculation | Ordered values |
| --- | --- | --- |
| Shape | `id % 8` | Circle, square, triangle, diamond, star, hexagon, plus, chevron |
| Marker | `floor(id / 8) % 8` | Top, top-right, right, bottom-right, bottom, bottom-left, left, top-left |
| Fill | `floor(id / 64)` | Solid, outline |
| Color | Palette at marker index | The eight palette colors above |

- V11 - Treat shape, marker and fill as the face identity. Color repeats the marker distinction and doesn't create a separate identity.

- V12 - Draw each face in an SVG with `viewBox="0 0 100 100"`. Center the main shape within `x=25..75` and `y=25..75`. Draw the marker above the shape as a circle with radius 5, centered 40 units from `(50, 50)` in its marker direction. Give the marker a 3-unit stroke in the Face background color, so it stays separate from the shape. Normalize diagonal directions, giving offsets of about 28.284 units on each axis. Use a 5-unit stroke for an outlined shape. At 44px, all 128 identities must be distinguishable in a grayscale face sheet, including solid and outlined squares with diagonal markers.

Use the face color for the solid shape's fill, the outlined shape's stroke and the marker's fill. An outlined shape has no fill. The marker keeps its Face-background stroke.

- V13 - Name each face with lowercase palette, fill, shape and marker names in the form "{color} {fill} {shape}, marker {position}". For example, face `0` is "cyan solid circle, marker top". Face `64` is "cyan outline circle, marker top".

## Card states

- V14 - Give every hidden card the same back with a neutral diamond. Hidden cards keep their face artwork out of the DOM, as [Input](input.md#card-names) requires.
- V15 - Show the face artwork on revealed cards. Keep it shown on matched cards and add a Green palette border and check mark. Keep matched cards at full opacity.
- V16 - Show keyboard focus with a 3px white outline offset by 2px. Hover changes only the border to the Text color. Matched cards keep their Green border when hovered.
- V17 - Use a 120ms border-color transition. Under `prefers-reduced-motion: reduce`, turn that transition off.

The gameplay document defines when card states change. See [Rules](rules.md) for turn timing and completion.

## Completion

- V18 - Show a win banner when the game finishes. Give it one 600ms pulse, then keep the banner static. Under `prefers-reduced-motion: reduce`, show the static banner immediately. Use neither flashing nor confetti.

## Large boards

- V19 - At viewport sizes 360x800 and 1280x800 CSS pixels, render all 256 cards of a 16x16 board. Keep the page within the viewport width and keep controls reachable. Keep each card at least 44px square, with scrolling confined to the board when needed. Reach the first and last card by touch scrolling and by keyboard. Scroll focused cards fully into view without clipping their focus outline.

## Browser zoom

- V20 - At a 640x400 CSS-pixel viewport with device scale 2, the equivalent layout of 200% browser zoom from 1280x800, keep controls, status and the win summary readable without overlap. Keep the page within the viewport width. Keep both a 4x4 board and a 16x16 board playable with pointer and keyboard. Allow board scrolling and keep the focused card and its complete focus outline in view. Don't disable pinch zoom.

The complete game pack starts in [Neon Pairs](README.md).
