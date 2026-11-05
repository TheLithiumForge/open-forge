# ADR 046: Leave printing outside the play experience

This record explains why the game has no dedicated print presentation and what existing export tools provide instead.

## Context

Neon Pairs is an interactive memory task. The player reveals information over time, and the game blocks new activation during a mismatch. A printed page cannot reproduce that interaction. The design discussion therefore did not treat a paper board as an alternate delivery mode.

The decision was to omit dedicated print styles. The current CSS contains screen layout, focus, hover and reduced-motion behavior, but no print-specific game rendering. This does not prevent a browser from offering Print; it means the project does not provide a separately designed paper experience.

## Why a print view was not a small addition

Printing a closed board would show identical backs and little useful state. Printing every face would reveal the deal rather than reproduce play. Printing current progress might help a report, but a static page would lose the remaining active interval and the meaning of a first or second reveal. Choosing among those outputs would require a new feature definition.

The large board also has an intentionally bounded scroll viewport. A useful paper layout would need to decide whether to expand that area, scale cards, paginate rows or omit controls. None of those choices follows automatically from the screen design. [ADR 035](adr-035-scroll-large-boards.md) explains why the screen layout uses scrolling.

## Existing tools answer clearer questions

A board code reproduces the starting deck. A snapshot preserves structured progress. A replay describes timed actions for engineering. Those formats retain information that a screenshot or printout cannot, including exact indices and mismatch time remaining. They are explicit supported routes rather than accidental byproducts of browser printing.

A screenshot can still be useful as visual evidence, especially for a clipped focus outline or artwork problem. Browser tests generate such artifacts where appropriate. That is different from promising a polished print stylesheet for the player. [ADR 051](adr-051-test-artifacts.md) describes diagnostic outputs.

## Alternatives considered

A simple `@media print` rule hiding controls would make a page look cleaner but would not resolve what card information should be printed. Expanding all faces would create a convenient answer sheet with no stated product journey behind it. A separate printable rules page would be documentation work, not a print mode of the game itself.

The choice to do none of those keeps the interactive artifact focused. It also avoids a second layout matrix that would need review at every supported size. That cost matters because the largest board already stretches the screen layout in ways that have dedicated acceptance evidence.

## Consequences and limits

Browser-default printing may clip, paginate or restyle the page according to the browser and user's settings. Those defaults are outside the supported game workflows. The accepted evidence concerns the playable screen artifact, offline opening and documented reproduction formats.

The absence of print styling is a scope boundary rather than a technical limitation of React or SVG. SVG could print crisply, and CSS could define paper layouts. The missing part is a product reason and specified behavior, not an inability to draw shapes on paper.

[ADR 003](adr-003-single-file.md), [ADR 010](adr-010-snapshot-json.md), [Reproducibility](reproducibility.md) and [Visual review checklist](visual-review-checklist.md) are the useful companion pages. They explain how the current game is delivered, preserved and inspected without treating a static printout as a continuation of play.

## The bounded screen viewport

This is a screen-oriented scroll container. There is no alternate paper layout in this rule set; a print feature would need to define its own intended information and geometry.

```css
.board-viewport {
  --card-min: 44px;
  --card-max: 88px;
  --card-gap: 6px;
  --board-padding: 6px;
  --board-border: 1px;
  max-height: min(70dvh, 768px);
  overflow: auto;
  overscroll-behavior: contain;
  padding: var(--board-padding);
  border: var(--board-border) solid var(--border);
  border-radius: 12px;
  background: var(--panel);
  width: fit-content;
  max-width: 100%;
}
```
