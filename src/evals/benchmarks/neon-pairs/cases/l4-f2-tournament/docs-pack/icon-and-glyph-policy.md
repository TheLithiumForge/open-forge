# Icon and glyph policy
This page explains the game's current use of SVG geometry, a CSS back decoration and native controls instead of an icon font.

The production interface has very little iconography. Card identities are drawn by `Face.tsx`, matched state uses a small SVG check, and hidden cards use a bordered span rotated into a diamond. Action controls retain visible text. This follows the DOM-button and inline-SVG approach in [Visuals](visuals.md) V01 and the face geometry in V10 through V16.

## What is actually drawn

| Visible form | Source | How it is rendered |
| --- | --- | --- |
| Hidden-card diamond | `card.css` | 12px span, 2px border, static rotation |
| Central face shape | `Face.tsx` | Circle, rectangle or explicit SVG path |
| Face marker | `Face.tsx` | Circle with radius 5 and a surface-colored surround |
| Matched check | `Card.tsx` | SVG path in a 12-unit view box |
| Disclosure affordance | Native `details` and `summary` | Browser rendering |
| Select affordance | Native `select` | Browser rendering |

The artwork paths are part of game identity. The matched check is a state mark. The browser's disclosure and select affordances belong to native controls. They may all look like small symbols, but their sources and maintenance needs are different.

There is no font-face declaration or icon-font stylesheet in the source. The application does not render card shapes through character codes or a private-use Unicode range. Its package and component source likewise do not introduce an icon component library.

## Why the faces are geometry

The catalogue distinguishes eight shapes, eight marker directions and two fill modes. It needs those combinations to remain distinct at the smallest card size and in grayscale. An authored view box lets `Face.tsx` give every central shape the same coordinate range and every marker the same distance from the center.

A font glyph would bring the selected font's outlines, side bearings, baseline and hinting into that identity geometry. A fallback glyph could have a different shape or no matching outline variant. The current SVG paths avoid those variables: the chevron, star and plus are the same paths regardless of the system font used for the page title.

The fill distinction is explicit. The SVG group uses either a colored fill with zero stroke width or no fill with a five-unit colored stroke. The marker has its own three-unit surround and remains filled. These operations are direct attributes rather than a reliance on finding solid and hollow Unicode characters that happen to match.

The marker position is similarly independent of text layout. `faceDesign` computes diagonal offsets from `40 / Math.SQRT2`, and the face component places a circle at the result. A combining mark or a second glyph would have to reproduce that geometry through font positioning, which is absent from the current implementation.

[Face catalogue](face-catalogue.md) gives the actual paths, centers and ID arithmetic. Those details are the meaningful visual contract; an abstract description such as “use a star icon” would omit most of what identifies a pair.

## The check is separate from the identity

`Card.tsx` adds the matched mark only when `matched` is true. Its path is `m2 6 3 3 5-6`, with no fill and a two-unit `currentColor` stroke. CSS places it at the top right and limits it to 16% of the card's width and height.

This percentage is constrained by the nearby face marker. The source comment notes that a mark wider than about 17% would reach a top-right marker. A check from a general icon set could have different internal padding or a different view box, so matching its nominal width would not guarantee the same painted footprint.

The existing clearance test measures the check box against the transformed marker circle and central shape, including stroke extents, for all faces on the smallest largest-board cards. That is more specific evidence than a large preview of a check next to an isolated face. [Card artwork](card-artwork.md) describes the selector that overrides the general full-size SVG rule.

The check is not the text character `✓`. Its appearance does not depend on whether the system UI font contains that glyph or how it aligns with surrounding text. The state word lives in the card's accessible name, while the visual check remains hidden from the accessibility tree.

## The back diamond is not a font character

A closed card contains an empty span with a border and a 45-degree transform. The transform is static; it does not spin or animate on reveal. Its border uses the secondary text color, and its size is constant across identities. Every closed card therefore has the same decoration.

Using a CSS border here keeps the back small and neutral without loading a face SVG or looking up a glyph. It also avoids confusing a closed card's diamond decoration with an actual revealed diamond identity. The latter is a central SVG path accompanied by its marker and fill mode.

Because global border-box sizing applies, the span's 12px width includes its border. Rotation increases its axis-aligned visible footprint. A reviewer comparing it with the face diamond needs to compare the actual rendered geometry rather than two nominal dimensions from different coordinate systems.

## Text labels remain text

Top-level actions and reproduction controls render their visible names as ordinary text inside native buttons. The size selector renders textual options. There is no icon-only replacement for New game, Restart, loading or exporting in the game UI. This leaves the action's wording available without interpreting a symbolic convention.

A decorative SVG inside a card uses `aria-hidden="true"` and `focusable="false"`. The button supplies its own accessible name. Giving the internal SVG a separate title would change that accessibility structure; the current renderer avoids duplicated face descriptions by naming the button once through `describeCard`.

The face sheet does show generated names as captions, because it is an inspection tool. Those captions are ordinary system text outside the cards. They do not change the face geometry or imply that every game card should display its full name visually. [Typography](typography.md) describes this separation of page text from card art.

## Native affordances

The reproduction panel is a `details` element with a `summary`. The game leaves its disclosure indicator to the browser. The size picker is a native select, with the browser's menu and indicator. Those controls inherit the game's font and colors where authored styles apply, while their internal rendering can vary by platform.

The root declares a dark color scheme. This is relevant to native control appearance, but it does not turn the browser's disclosure marker into one of the authored card paths. A visual review can expect familiar native behavior without expecting identical pixels across all operating systems.

The distinction also helps when diagnosing missing symbols. A missing face shape points toward SVG or state rendering. A missing native select arrow points toward control appearance or browser styling. A font loading failure is not the first explanation for either in this source, because there is no downloaded icon font in that path.

## Delivery and reuse

Inline SVG travels with the rendered component and uses the same inherited surface variables as the rest of the game. It does not require a separate sprite URL, external image host or font request. The single-file build is compatible with that arrangement. [Offline delivery](offline-delivery.md) covers the broader delivery behavior.

The art sheet imports `Card` itself, not an alternate set of icon samples. Its browser tests compare card computed styles with the game and inspect actual SVG attributes. This makes it possible to review the exact geometry that players see without copying it into a design-only renderer.

An external illustration or icon-font specimen would not be a suitable replacement for that fixture's evidence. It could have correct-looking symbols while omitting the card border, interior size, marker surround and matched overlay. The existing fixture's value is that it brings those constraints into the preview together.

## Reviewing a symbol change

The useful questions are concrete: did the central silhouette change, did the stroke change at the small size, did a diagonal marker lose separation, or did the matched check cover identity artwork? The catalogue and browser tests provide direct observations for these questions. A library's general promise that an icon is “accessible” would not answer them.

A shape-name change also reaches the generated accessible description through the ordered table. A path-only edit can change appearance while leaving names stable. Keeping those two effects visible in review helps detect a card that looks like one shape while being described as another.

The game uses this small set of authored forms. [Visual review checklist](visual-review-checklist.md) points from an artwork change to the existing face-sheet, grayscale, clearance and naming evidence.
