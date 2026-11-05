# ADR 034: Provide 128 distinct face identities

This record explains how the artwork catalogue covers the largest board without relying on color alone.

## Context

The supported maximum board is 16x16, or 256 positions. With every face appearing twice, that requires 128 pair identities. Reusing the same identity for several pairs would change matching from finding a specific partner to choosing among several equivalent cards. The chosen catalogue therefore has one identity per possible pair on the largest board.

`FACE_COUNT` is 128. Valid IDs are the integers zero through 127. `faceDesign` rejects values outside that range, fractions and nonintegers rather than attempting to wrap them into a design. The deck for a smaller board uses IDs up to its pair count minus one.

## A structured catalogue

Eight shapes combine with eight marker positions and two fill treatments. Shape is `id % 8`; marker is `floor(id / 8) % 8`; fill changes at 64. The marker chooses the palette color as well, so hue reinforces an existing distinction. The identity remains the combination of shape, marker and fill.

This structure gives the catalogue a predictable order and makes it possible to generate both drawing and name. It also means the largest board includes the full cross-product, including combinations that can be visually demanding at the minimum size. The design is not complete merely because 128 different strings can be generated.

## Why not more

A larger catalogue would be unused by the current size list and would increase art review without enabling a required board. Randomly selecting identities from a larger pool would also make a seed's meaning depend on another sampling step. The present generator uses a simple initial pair sequence and shuffles positions.

A smaller catalogue was incompatible with one partner per face at the maximum size. Increasing the number of colors could have produced more nominal combinations, but would have made color carry an independent part of identity. The geometric cross-product was chosen because it can remain distinguishable in grayscale. [ADR 007](adr-007-svg-faces.md) explains the rendering medium.

## Geometry has to survive rasterization

The central shape sits inside a 100-unit SVG view box, with markers on a 40-unit radius. Diagonal offsets are normalized rather than using the same raw offset on each axis. The marker's background-colored stroke separates it from an adjacent shape edge. Outline shapes use a visible stroke rather than an almost-filled interior.

The awkward cases include solid and outlined squares with diagonal markers. A marker that overlaps the central square too much can make two distinct combinations look similar. The browser face sheet therefore renders the actual components at 44px rather than reviewing only enlarged drawings.

## Evidence

Focused tests check all identities by shape, marker center and fill, and compare names and geometry with the ordered tables. They also test invalid IDs. Browser tests capture color, grayscale and neutralized sheets and compare rasterized silhouettes. The sheet uses the same `Card` and `Face` components as the game, preventing a parallel art renderer from concealing a component-specific issue.

The evidence does not turn visual recognition into a purely numerical problem. Screenshots remain useful for review even when masks are unique. A unique pixel pattern can still be hard for a person to remember. [Face catalogue](face-catalogue.md), [Card artwork](card-artwork.md), [Contrast audit](contrast-audit.md) and [ADR 008](adr-008-seven-sizes.md) explain the catalogue's related constraints.

## Face lookup and label derivation

The design lookup validates its input and derives geometry, palette and label from the same index arithmetic.

```ts
export function faceDesign(face: number) {
  if (!Number.isInteger(face) || face < 0 || face >= FACE_COUNT) throw new Error('Invalid face.');
  const shape = SHAPES[face % SHAPES.length];
  const markerIndex = Math.floor(face / SHAPES.length) % MARKERS.length;
  const marker = MARKERS[markerIndex];
  const center = MARKER_CENTERS[markerIndex];
  const color = FACE_COLORS[markerIndex];
  const colorName = COLOR_NAMES[markerIndex];
  const outline = Math.floor(face / 64) === 1;
  if (!shape || !marker || !center || !color || !colorName) throw new Error('Invalid face.');
  return {
    shape,
    center,
    color,
    outline,
    label: `${colorName} ${outline ? 'outline' : 'solid'} ${shape}, marker ${marker}`,
  };
}
```
