# ADR 039: Keep contrast grounded in actual surfaces

This record explains the contrast rationale and the limits of the palette and artwork evidence.

## Context

The word “neon” can invite glowing decoration and low-contrast outlines against dark backgrounds. That would be a poor foundation for a game whose smallest cards must carry a recognizable shape, marker and fill. The design discussion therefore treated readable text, visible boundaries and distinct face geometry as prerequisites for the appearance.

The decision was to use the specified light text and neutral boundaries on the dark surfaces, with a white keyboard focus outline and a separate matched-card treatment. The specification defines actual colors and geometry. The source has no universal contrast-ratio gate.

## Different observations need different evidence

Text readability, control boundaries, keyboard focus and face identity are related but not interchangeable. Main text uses `#F4F6FF`; secondary text uses `#B8BDD2`. Neutral borders use `#757C9A`. Focus is a 3px white outline offset by 2px. Those choices make the intended relationships concrete enough to inspect in the built page.

Artwork has its own separation mechanism. The circular marker carries a stroke in the face-background color so it does not merge with the central shape. Outline faces use a 5-unit stroke in a 100-unit view box. The grayscale sheet checks that shape, marker position and fill remain distinct without relying on hue. [Card artwork](card-artwork.md) explains those dimensions.

## Why the floor is conceptual here

A single ratio would not capture a clipped focus outline, a marker overlapping a square or matched cards faded until their faces become difficult to read. The current matched state stays at full opacity and adds both a green border and a check mark. Hover changes only the card border, so it does not dim or distort the face being remembered.

The design's “floor” is the refusal to trade away these readable relationships for decoration. [Contrast audit](contrast-audit.md) is the place to inspect the actual combinations and evidence, while [Visuals](visuals.md) defines the product values.

## Alternatives considered

Using bright color alone to distinguish faces would have created an inaccessible dependency on hue recognition. Applying a low-opacity matched state would have made progress look subdued at the cost of hiding information the player can still inspect. A soft glow alone for focus could be attractive but would be less definite than an outlined boundary.

The chosen palette has a restrained hierarchy: page, panel and card backgrounds separate surfaces; text stays light; focus is unambiguous; the artwork palette is bright but structurally redundant. The cost is a less atmospheric visual treatment than a game built around glows and fading cards.

## Review evidence and limits

Browser style tests compare computed colors, dimensions and focus values with the specified vector. Artwork tests capture the actual 44px components in color, grayscale and neutralized form. Focus-bound checks ensure the outline is fully inside the scroll viewport. These checks address different failure modes and are useful together.

They do not certify the complete game against an accessibility standard. The specification explicitly avoids treating the minimum card size as such a claim, and the same caution applies to palette checks. Automated accessibility-tree evidence also does not establish spoken output. [Accessibility audit](accessibility-audit.md), [ADR 022](adr-022-card-names.md) and [ADR 049](adr-049-zoom-floor.md) describe the surrounding evidence boundaries.

## Neutral surfaces and text tokens

These authored values are checked against computed browser colors. They provide specific surfaces to audit rather than an unnamed promise of high contrast.

```css
:root {
  font-family: system-ui, sans-serif;
  font-size: 16px;
  line-height: 24px;
  color: #f4f6ff;
  background: #090b14;
  color-scheme: dark;
  --text: #f4f6ff;
  --secondary: #b8bdd2;
  --panel: #141827;
  --back: #1b2034;
  --face: #111625;
  --border: #757c9a;
  --green: #8bea7d;
  --content-max: 1120px;
  --page-padding: 16px;
}
```
