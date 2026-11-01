# Visuals
Visuals is a design specification that defines the layout, colors and card artwork for Neon Pairs.

A card face combines a shape, a marker position and a fill. You can identify a pair from those features even when you can't distinguish its color.

## Layout

- V01 - Use native controls and a CSS Grid board of DOM card buttons with inline SVG artwork. Render the game without canvas. This game uses native controls instead of shadcn components.
- V02 - Center the content within a maximum width of 1120px and give it 16px padding. Keep the outer page within the viewport width.
- V03 - Limit the board's scroll area to `70dvh`, capped at 768px tall. Give it 4px padding and allow overflow on both axes inside that area.
- V04 - Make each card square and size it from 44px to 88px according to the available board width. Use a 6px grid gap, a 2px card border and an 8px card radius.
- V05 - Fit the default four-column board within a phone viewport. When a larger board reaches the 44px minimum card size, keep that size and scroll inside the board.

For example, a sixteen-column board needs 794px for its cards and gaps, plus 8px for board padding. On a phone, the board scrolls while the outer page stays within the viewport width.

The 44px minimum is the chosen card target size. It doesn't state that the complete game conforms to an accessibility standard.

## Colors

V06 - Use these colors for the page and controls:

| Token | Color | Use |
| --- | --- | --- |
| Page | `#090B14` | Page background |
| Panel | `#141827` | Panel background |
| Back | `#1B2034` | Hidden card background |
| Face | `#111625` | Revealed card background |
| Text | `#F4F6FF` | Main text |
| Secondary | `#B8BDD2` | Secondary text |
| Border | `#757C9A` | Neutral borders |
| Focus | `#FFFFFF` | Focus outline |

V07 - Use this ordered palette for card artwork:

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

V08 - Use the system sans-serif font stack. Set body text to 16px with a 24px line height, the title to 24px and secondary text to 14px.

V09 - Use spacing values of 4px, 6px, 8px, 12px, 16px, 24px and 32px.

## Card faces

V10 - Generate 128 face identities from integer face IDs `0` through `127`. Calculate each face from these ordered values:

| Part | Calculation | Ordered values |
| --- | --- | --- |
| Shape | `id % 8` | Circle, square, triangle, diamond, star, hexagon, plus, chevron |
| Marker | `floor(id / 8) % 8` | Top, top-right, right, bottom-right, bottom, bottom-left, left, top-left |
| Fill | `floor(id / 64)` | Solid, outline |
| Color | Palette at marker index | The eight palette colors above |

V11 - Treat shape, marker and fill as the face identity. Color repeats the marker distinction and doesn't create a separate identity.

V12 - Draw each face in an SVG with `viewBox="0 0 100 100"`. Center the main shape within `x=20..80` and `y=20..80`. Draw its marker as a circle with radius 4, centered 40 units from `(50, 50)` in its marker direction. Normalize diagonal directions, giving offsets of about 28.284 units on each axis. Use a 5-unit stroke for an outlined shape.

V13 - Name each face with lowercase palette, fill, shape and marker names in the form "{color} {fill} {shape}, marker {position}". For example, face `0` is "cyan solid circle, marker top". Face `64` is "cyan outline circle, marker top".

## Card states

- V14 - Give every hidden card the same back with a neutral diamond. Hidden cards keep their face artwork out of the DOM, as [Input](input.md#card-names) requires.
- V15 - Show the face artwork on revealed cards. Keep it shown on matched cards and add a green border and a check mark. Keep matched cards at full opacity.
- V16 - Show keyboard focus with a 3px white outline offset by 2px. Hover changes only the border.
- V17 - Use 120ms border and opacity transitions. Under `prefers-reduced-motion: reduce`, turn those transitions off.

The gameplay document defines when card states change. See [Rules](rules.md) for turn timing and completion.

## Completion

V18 - Show a win banner when the game finishes. Give it one 600ms pulse, then keep the banner static. Under `prefers-reduced-motion: reduce`, show the static banner immediately. Use neither flashing nor confetti.

The complete game pack starts in [Neon Pairs](README.md).
