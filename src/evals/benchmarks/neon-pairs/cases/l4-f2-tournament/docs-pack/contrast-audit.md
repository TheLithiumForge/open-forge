# Contrast audit
This page records calculated contrast ratios for the current Neon Pairs colors and explains what those measurements do and do not establish.

The inputs come from `src/game/game.css`, `src/game/board/face-design.ts` and the visual specification. Ratios below were calculated from their sRGB hex values, not sampled from screenshots. That makes the arithmetic reproducible without confusing antialiasing, active animation or a platform's native-control rendering with an authored color. [Color policy](color-policy.md) describes the roles behind the numbers.

## Method

Each 8-bit channel is divided by 255. A normalized channel `c` is converted to linear light as `c / 12.92` when `c <= 0.04045`, otherwise as `((c + 0.055) / 1.055) ** 2.4`. Relative luminance is the weighted sum of those linear channels with coefficients 0.2126, 0.7152 and 0.0722 for red, green and blue.

For two luminances, the ratio is `(lighter + 0.05) / (darker + 0.05)`. The tables round the result to two decimal places. Calculations retain the underlying precision before rounding. These are ratios between flat, opaque colors; the special completion-opacity case is considered separately below.

A small reproduction function, using only the supplied hex values, is:

```python
def luminance(hex_color):
    value = hex_color.lstrip('#')
    channels = [int(value[i:i + 2], 16) / 255 for i in (0, 2, 4)]
    linear = [c / 12.92 if c <= 0.04045
              else ((c + 0.055) / 1.055) ** 2.4 for c in channels]
    return sum(c * w for c, w in zip(linear, (0.2126, 0.7152, 0.0722)))

def contrast(first, second):
    a, b = sorted((luminance(first), luminance(second)))
    return (b + 0.05) / (a + 0.05)

print(f"{contrast('#B8BDD2', '#090B14'):.2f}:1")
# 10.52:1
```

The visual requirements fix the colors and specify geometry, non-color face identity and focus visibility. A ratio alone does not establish the complete game's accessibility.

## Interface text and borders

| Foreground | Page `#090B14` | Panel `#141827` | Back `#1B2034` | Face `#111625` |
| --- | --- | --- | --- | --- |
| Text `#F4F6FF` | 18.21:1 | 16.37:1 | 14.95:1 | 16.71:1 |
| Secondary `#B8BDD2` | 10.52:1 | 9.45:1 | 8.63:1 | 9.65:1 |
| Border `#757C9A` | 4.77:1 | 4.29:1 | 3.92:1 | 4.38:1 |
| Focus `#FFFFFF` | 19.63:1 | 17.65:1 | 16.12:1 | 18.02:1 |
| Matched green `#8BEA7D` | 13.24:1 | 11.90:1 | 10.87:1 | 12.15:1 |

Not every cell is a current text pairing. The matrix gives the available surface comparisons so a reviewer can identify the one an element actually uses. For example, secondary help text is on the page; the closed-card back diamond uses secondary color on Back; native control text uses Text on Panel.

The board and reproduction borders meet Page on their outside and Panel on their inside. A neutral closed-card border meets Panel outside and Back inside. A revealed card has Face inside. This explains why one border token has several relevant ratios. Describing it only as “4.77:1” would omit the less contrasted card-interior adjacency.

The keyboard outline is white and offset from the border. In the board gap or edge padding its nearby dark surface is Panel. Its high color ratio does not demonstrate that all four sides are visible: scroll clipping is a geometric question. `tests/focus-bounds.ts` measures the outline's reach separately from its color.

The hover border uses Text. A matched card retains green on hover, so its state border remains the Matched green row rather than the Text row. The style test waits for the border to reach its declared target before asserting that color. During the 120ms interpolation, a sampled border can have an intermediate ratio.

## Surface separation

| Pair of surfaces | Ratio |
| --- | --- |
| Page and Panel | 1.11:1 |
| Page and Back | 1.22:1 |
| Page and Face | 1.09:1 |
| Panel and Back | 1.10:1 |
| Panel and Face | 1.02:1 |
| Back and Face | 1.12:1 |

The dark surfaces are close in luminance. In particular, Panel and Face almost coincide numerically. Their difference is not sufficient evidence that a card is visibly bounded or has changed state. The border, face artwork, neutral back and matched check supply additional structure.

This is consistent with the renderer. A reveal replaces a back span with actual identity geometry, and a match preserves that geometry while adding a mark. The current UI does not ask the player to detect a reveal only by comparing two nearly equal dark backgrounds. [Card artwork](card-artwork.md) follows those changes in the DOM.

Rounded corners, spacing and border widths also affect perceived separation. The ratio table does not measure the visual weight of a two-pixel border or the distance between adjacent cards. Those details belong to the actual 44px card and the six-pixel grid gaps, not to the color function.

## Face colors on their actual surface

| Marker direction | Color | Relative luminance | Ratio on Face |
| --- | --- | --- | --- |
| top | Cyan `#63E7FF` | 0.670245 | 12.36:1 |
| top-right | Magenta `#FF71D6` | 0.379253 | 7.37:1 |
| right | Green `#8BEA7D` | 0.658153 | 12.15:1 |
| bottom-right | Yellow `#FFE082` | 0.761830 | 13.93:1 |
| bottom | Orange `#FFB064` | 0.532308 | 9.99:1 |
| bottom-left | Violet `#B49AFF` | 0.400345 | 7.73:1 |
| left | Rose `#FF8296` | 0.394273 | 7.62:1 |
| top-left | Ice `#D9F3FF` | 0.860729 | 15.63:1 |

These ratios apply to the solid fill, outline stroke and marker fill against the face background. The marker surround is itself Face-colored. Against adjacent artwork it therefore has the same ratio in the reverse direction; against the underlying Face surface it intentionally blends into that surface.

Magenta has the lowest face-to-surface ratio and Ice the highest. That ordering does not rank identity recognition. A small outlined square with a diagonal marker can pose a geometry problem even when its color ratio is high. A large solid shape can remain recognizable with fewer bright pixels than another combination if its silhouette is distinctive.

Color-to-color separation is not the face identity test. Color follows marker position, and solid/outline variants can have exactly the same color. The art browser test removes hue and then gives all geometry one neutral light value before comparing 44px raster silhouettes. [Face catalogue](face-catalogue.md) describes that evidence and its human-review limits.

## Completion during the pulse

The ordinary victory paragraph has Text color, a Border outline and no permanent background declaration. It sits on the page. At the pulse midpoint, its keyframes supply Panel background and opacity 0.75. Element opacity composites the whole paragraph, including its text, over the underlying page.

For a simple sRGB channel blend over the opaque Page color, the midpoint text is approximately `#B9BBC4` and the midpoint panel is approximately `#111522`, after rounding channels to 8-bit values. Applying the same contrast formula to these estimated composite colors gives about 9.50:1. This is a compositing estimate for the specified midpoint, not a screenshot measurement or an exact result for every antialiased edge pixel.

The permanent opaque Text-on-Panel ratio of 16.37:1 therefore should not be quoted as the midpoint text ratio. Conversely, the lower midpoint estimate is not the final static appearance. After its single iteration, the banner returns to the underlying static styles. Reduced motion disables the animation altogether. See [Motion policy](motion-policy.md).

## Evidence boundaries

The browser style test compares computed RGB values with fixed expectations for the main roles. The art test compares SVG attributes with an independent ordered palette and checks the marker's resolved surround. Those checks establish which authored colors reach representative elements; the calculations here then describe those colors' flat-pair contrast.

These calculations do not sample operating-system select popups, forced-color substitutions, display calibration or grayscale screenshots. The face-sheet grayscale test addresses geometry after filtering, which is a different measurement from this opaque-token calculation.

For a regression, separating the declared value, computed value and sampled pixel is useful. A wrong declaration is a palette issue; a wrong computed value can be cascade or inheritance; an intermediate screenshot pixel may come from antialiasing, opacity or an unfinished transition. [Theme tokens](theme-tokens.md) identifies the declaration sites, and [Visual review checklist](visual-review-checklist.md) connects them to the existing appearance checks.
