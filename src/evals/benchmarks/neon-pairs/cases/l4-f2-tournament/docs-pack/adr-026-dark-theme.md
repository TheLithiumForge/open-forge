# ADR 026: Ship one dark appearance

This record explains the single dark theme and its relationship to artwork, native controls and review scope.

## Decision

The current game has one dark appearance. The page background is `#090B14`, panels use `#141827`, closed cards use `#1B2034` and face-up cards use `#111625`. Main and supporting text have separate light colors, borders are neutral and focus is white. The root declares `color-scheme: dark` so native controls participate in the intended appearance.

This was a scope choice as well as a visual preference. A second theme would have needed reviewed foreground/background combinations, card states, form controls and artwork separation. The project chose a consistent dark environment for this version rather than shipping an unreviewed light variation.

## Why the palette is structured

The background layers distinguish the page, containers and card states without replacing the face identity system. A hidden card has the same neutral diamond back regardless of its future face. A revealed face has a slightly different surface, while matched cards add the green border and check mark. The palette therefore supports state recognition instead of becoming another matching rule.

Artwork colors are a separate ordered palette. Shape, marker and fill distinguish identities; color reinforces the marker direction. That makes the faces usable when hue distinctions are unavailable. [Color policy](color-policy.md) and [ADR 007](adr-007-svg-faces.md) explain the relationship.

## Tokens and ownership

The root variables in `game.css` provide common page and control colors. Card styles remain in `board/card.css`, and status styles remain beside the status component. `Face` uses the face-background variable for the marker surround, so the intended contrast between marker and shape is preserved in the current appearance.

There is no theme picker in the controls and no theme field in `Preferences`. This keeps the saved model focused on the selected board size and best moves. A browser preference for reduced motion is handled separately because it concerns effects rather than selecting a second palette.

## Alternatives

Following light/dark system preference would have made the initial appearance more familiar on some devices. It would also have doubled the combinations needing evidence and required a meaningful light card-face background for the marker surround. A cosmetic variable swap would not by itself establish that every outline face and focus ring remained clear.

Allowing arbitrary colors was an even larger departure. It would have made the specified artwork palette and contrast review contingent on user combinations. That was unnecessary for the memory task and inconsistent with having a reviewed fixed face catalogue.

## Costs and evidence

Some players prefer a light interface, and this version does not supply it. The choice keeps the review surface bounded; it is not a claim that dark presentation is universally more accessible. The visual specification fixes actual colors, and browser style tests compare computed values against those product values.

The artwork review includes grayscale and neutralized faces at the minimum size, so “neon” does not mean that color alone carries identity. Focus and matched states have their own visible treatment. [ADR 039](adr-039-contrast-floor.md) explains the limits of contrast evidence without treating the palette as a blanket accessibility certification.

A future appearance discussion would need to consider all those relationships rather than only the page background. For the present code, [Theme tokens](theme-tokens.md), [Card artwork](card-artwork.md) and [Visuals](visuals.md) are the useful references when locating a color and understanding why it belongs to that surface.

## The shared appearance values

The root couples the browser color scheme with the authored tokens. Components consume these values while retaining their own layout rules.

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
* {
  box-sizing: border-box;
}
body {
  margin: 0;
}
```
