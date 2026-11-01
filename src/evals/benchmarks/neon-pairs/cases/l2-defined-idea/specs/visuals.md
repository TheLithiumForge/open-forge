# Visuals
Visuals is a specification that defines requirements for Neon Pairs.

## Layout

- V01.1 - Use native controls.
- V01.3 - Use inline SVG artwork.

- V02.1 - Center content at a maximum width of 1120px.
- V02.2 - Use 16px content padding.
- V02.3 - Keep the outer page within viewport width.

- V03.1 - Limit board height to 70dvh capped at 768px.
- V03.2 - Use 6px board padding.
- V03.3 - Allow both axes to scroll inside the board area.

- V04.1 - Keep cards square between 44px and 88px based on available board width.
- V04.2 - Use a 6px grid gap.
- V04.3 - Use a 2px card border.
- V04.4 - Use an 8px card radius.

- V05.1 - Fit the four-column board within a phone viewport.
- V05.2 - Keep 44px cards and scroll inside the board when a larger board reaches that minimum.

## Colors

- V06.1 - Page background is #090B14.
- V06.2 - Use #141827 for the board scroll area, reproduction panel and non-card button, select, input and textarea backgrounds.
- V06.3 - Hidden card background is #1B2034.
- V06.4 - Revealed card background is #111625.
- V06.5 - Main text is #F4F6FF.
- V06.6 - Use #B8BDD2 for secondary text, meaning help text, status labels and optional supporting text.
- V06.7 - Neutral borders are #757C9A.
- V06.8 - Focus outline is #FFFFFF.

- V07.1 - Use the ordered palette Cyan #63E7FF, Magenta #FF71D6, Green #8BEA7D, Yellow #FFE082, Orange #FFB064, Violet #B49AFF, Rose #FF8296 and Ice #D9F3FF.

## Type and spacing

- V08.1 - Use the system sans-serif font stack.
- V08.2 - Body text is 16px with 24px line height.
- V08.3 - Title text is 24px.
- V08.4 - Set secondary text to 14px, meaning help text, status labels and optional supporting text.

- V09.1 - Positive layout gaps, padding and margins you set use 4px, 6px, 8px, 12px, 16px, 24px or 32px. Allow zero resets and automatic centering. Reset browser default margins. Native control internals are not covered.
- V09.2 - Borders, radii, focus, font metrics and SVG coordinates use their own stated values.

## Card faces

- V10.1 - Generate 128 distinguishable face identities.
- V10.3 - Use circle, square, triangle, diamond, star, hexagon, plus and chevron shapes.
- V10.4 - Use top, top-right, right, bottom-right, bottom, bottom-left, left and top-left markers.
- V10.5 - Use solid and outline fills.
- V10.9 - Color is the palette entry at the marker index.

- V11.1 - Shape, marker and fill define identity.
- V11.2 - Color repeats the marker distinction without adding an identity.

- V12.7 - Distinguish all 128 identities at 44px in grayscale, including solid and outline squares with diagonal markers.

- V13.1 - Name faces with lowercase palette, fill, shape and marker names.
- V13.2 - Use the form {color} {fill} {shape}, marker {position}.

## Card states

- V14.1 - Use the same neutral-diamond back for every hidden card.
- V14.2 - Keep hidden face artwork out of the DOM under [I14](input.md#card-names).

- V15.1 - Show face artwork on revealed and matched cards.
- V15.2 - Matched cards add a Green palette (#8BEA7D) border and check mark.
- V15.3 - Keep matched cards at full opacity.

- V16.1 - Keyboard focus has a 3px white outline offset by 2px.
- V16.2 - Hover changes only the border to the Text color (#F4F6FF). Matched cards keep their Green border when hovered.

- V17.1 - Use a 120ms border-color transition.
- V17.2 - Turn that transition off under prefers-reduced-motion: reduce.

## Completion

- V18.1 - Show a win banner on completion.
- V18.2 - Pulse it once for 600ms then keep it static.
- V18.3 - Reduced motion shows the static banner immediately.
- V18.4 - Use neither flashing nor confetti.

## Large boards

- V19.1 - Render all 256 cards at 360x800 and 1280x800 viewports.
- V19.2 - Keep the page within viewport width and controls reachable.
- V19.3 - Keep cards at least 44px square with board scrolling when needed.
- V19.4 - Reach first and last cards through touch scrolling and keyboard.
- V19.5 - Keep the full focused card and outline in view.

## Browser zoom

- V20.1 - At a 640x400 CSS-pixel viewport with device scale 2, equivalent to 200% browser zoom from 1280x800, keep controls, status and the win summary readable without overlap.
- V20.2 - Keep the outer page within viewport width at that equivalent zoom layout.
- V20.3 - At that equivalent zoom layout keep 4x4 and 16x16 playable by pointer and keyboard.
- V20.4 - At that equivalent zoom layout allow board scrolling and show the whole focused card and outline.
- V20.5 - Keep pinch zoom enabled.

