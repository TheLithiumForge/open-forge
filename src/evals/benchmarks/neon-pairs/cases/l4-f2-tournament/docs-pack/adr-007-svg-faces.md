# ADR 007: Draw faces as inline SVG

This record explains the generated SVG artwork and why face identity is expressed through geometry.

## Context

The largest board contains 128 pairs, so a small collection of emoji or generic icons would repeat identities. A raster collection with that many faces would need asset management, scaling review and a distribution strategy. The game also needs identities to remain distinguishable without color and at the minimum card size.

The selected approach combines eight shapes, eight marker directions and two fill treatments into 128 identities. Inline SVG expresses those parts directly and scales inside square card buttons. Color reinforces the marker choice rather than creating another independent matching dimension. [Face catalogue](face-catalogue.md) gives the full ordering.

## Decision

`face-design.ts` calculates the description from an integer face ID. `Face.tsx` turns that description into SVG geometry. Every face uses a `0 0 100 100` view box. The principal shape occupies the central area and the circular marker sits 40 units from the center in its assigned direction. Diagonal directions use `40 / Math.SQRT2`, so their distance is consistent with cardinal directions.

The marker has radius 5 and a 3-unit stroke matching the face background. That separation is particularly useful near diagonal corners of a square. Outline faces use a 5-unit shape stroke; solid faces use a fill. These dimensions are not arbitrary drawing preferences: the grayscale face-sheet checks exist because subtle overlap at 44px can make two nominally different designs look alike.

`Face` marks the SVG as hidden from accessibility APIs and nonfocusable. The surrounding button supplies the meaningful name from the same design data. This avoids treating the drawing as an extra focus destination or a second independent description of the card. [ADR 022](adr-022-card-names.md) explains the name model.

## Why inline

A face has a handful of primitives rather than a downloaded image. Inline geometry travels inside the single HTML distribution, inherits the relevant face background variable and needs no asset URL lookup. The face sheet renders the same `Face` component as the board, so its review does not depend on a second drawing implementation that can drift.

Hidden cards do not mount `Face`. They contain the neutral card back instead. This matters for a memory game because merely making an SVG invisible with CSS would leave its identity in the DOM. The chosen rendering path makes revealed information explicit in the component props.

## Alternatives and costs

Emoji would have introduced platform-dependent artwork and an uncontrolled identity catalogue. Canvas could draw crisp symbols but would move semantics and input away from native DOM controls. Raster sprites could be reliable if carefully authored, at the cost of maintaining a sprite sheet and checking resampling at every supported card size.

SVG keeps the geometry readable in code, but that readability is not a visual proof. Browser tests capture color, grayscale and neutralized versions of the actual 44px cards. They check distinct rasterized silhouettes as well as marker dimensions and palette assignment. Unit tests check all calculated identities and invalid face values. [Card artwork](card-artwork.md) covers the drawing details, while [ADR 034](adr-034-face-count.md) explains why this catalogue has its particular size.

## Rendering a computed design

The component separates the central shape treatment from the marker, so the marker keeps its background surround for either fill treatment.

```tsx
export function Face({ face }: { face: number }) {
  const design = faceDesign(face);
  return (
    <svg viewBox="0 0 100 100" aria-hidden="true" focusable="false">
      <g
        fill={design.outline ? 'none' : design.color}
        stroke={design.color}
        strokeWidth={design.outline ? 5 : 0}
        strokeLinejoin="round"
      >
        {shapeElement(design.shape)}
      </g>
      <circle
        cx={design.center[0]}
        cy={design.center[1]}
        r="5"
        fill={design.color}
        stroke="var(--face)"
        strokeWidth="3"
      />
    </svg>
  );
}
```
