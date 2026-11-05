# Retired idea: theme packs

This is a design note about cosmetic theme packs considered for Neon Pairs and retired before they became a product feature.

The game has one authored dark visual treatment and generated geometric faces. It has no theme selector, downloaded artwork pack, stored theme preference, or theme identifier in a board code. The proposed alternatives raised questions about identity and compatibility.

## The appealing first sketch

The proposal began with a familiar idea: let a player choose a different look without changing the rules. A light page, a softer palette, or a set of illustrated objects could make the game feel more personal. The first sketches put all of these under the same word, “theme.”

They were not actually the same scale of change. A page-background variation can leave face identity intact. Replacing the face catalogue with pictures changes what the player must recognize. Altering marker positions or removing outline fills can collapse distinctions between pairs. The review separated those consequences before trying to estimate implementation effort.

The current visual system is compact because its palette and geometry have specific roles. Shape, marker direction, and fill define identity. Color follows marker direction and reinforces that distinction. A theme that changes one of these relationships can change difficulty even if the deck still contains the same numeric IDs.

## Surface colors were the smaller proposal

One option changed only page, panel, text, border, and focus colors. In principle, the existing CSS custom properties make many of those values easy to identify. That is an implementation convenience, not evidence that every combination is equally readable.

The current palette uses a dark page and panel, distinct backs and faces, light primary text, secondary text, neutral borders, and a white focus outline. The visual review includes contrast and state distinctions. A light treatment would need its own examination of hidden versus revealed surfaces, hover borders, matched marks, and focused targets.

The review kept the existing colors as the current contract. [Theme tokens](theme-tokens.md), [color policy](color-policy.md), and [contrast audit](contrast-audit.md) describe the authored treatment and its evidence.

## Artwork packs were a different game surface

Another option replaced geometric faces with animals, food, or abstract illustrations. The sketches were attractive at large sizes. At the minimum card size, details that separated two drawings could disappear, and styles varied in how much visual attention each identity demanded.

The current generator provides exactly the 128 identities needed by the largest board. It calculates the same shape and marker structure wherever a face is drawn. An artwork pack would need a complete comparable set, or a definition of which sizes it supports. Mixing partial packs would raise another set of identity and naming questions.

External image files would also change distribution work. The current offline HTML carries inline SVG artwork and does not fetch assets during play. A pack system could be embedded, but the size and maintenance cost would be real. Fetching packs later would move away from the present no-service delivery model. The [offline delivery page](offline-delivery.md) explains the current artifact.

Maximum pack size and packaging format remained unresolved when the idea was retired.

## Names would have to remain meaningful

Current accessible face names correspond to palette, fill, shape, and marker position. If an illustration replaced a face but kept its old geometric name, a player using names would encounter information unrelated to the picture. If the name changed, a shared board could be remembered through different vocabularies depending on the selected pack.

That is not automatically impossible. It does mean a pack needs an information model, not just a folder of images. A decorative background behind an unchanged face has different consequences from replacing the identity itself. The review found that the original mockups blurred this boundary.

The current hidden-card treatment would also need to remain coherent. Hidden positions share a neutral back and do not mount a face SVG. A pack should not accidentally disclose identity through an image filename or a label while pretending the card is closed. [Card artwork](card-artwork.md) explains the implemented boundary.

## Board codes made the compatibility question visible

A version 1 board code maps to a fixed deck of numeric face IDs. If a theme merely changes decorative surfaces, the deal remains the same. If it remaps face IDs to a different recognition task, the same code can describe equivalent pair positions but a different perceptual experience.

The discussion considered adding the theme to the code. That would change the compact format and its compatibility story. Another option stored it only as a preference, making the same shared code look different on two devices. Neither was adopted, and the current code syntax remains independent of theme because no theme system exists.

Snapshots likewise contain a board code and progress, without visual settings. A pack change halfway through an attempt could replace the identities a player had just memorized with another vocabulary. Treating the choice as purely cosmetic would not remove that practical effect.

The [format-versioning guide](format-versioning-guide.md) and [board-code decision](adr-009-board-codes.md) explain current compatibility. A future visual format extension remains undecided.

## Random appearance was also set aside

One sketch varied colors or decorations each time a deal was generated. This created variety cheaply, but it weakened the value of a code when investigating a visual report. Two people could agree on pair positions and still see different problematic details.

The current face design is a deterministic mapping from ID to geometry and palette. Reproduction therefore includes the visible identity scheme as well as the shuffled positions. The review preferred that inspectability to incidental novelty for the present game.

No random theme choice or per-session style seed was implemented. The game does not reserve a hidden preference for it. The code's existing local seed creation and deck generator remain concerned with creating and reproducing deals, not with picking cosmetic packs.

## Why retirement was useful

The team retired theme packs because the proposals combined three different projects: alternate page colors, alternate face identities, and an asset distribution system. Keeping one broad backlog label made the work look smaller while obscuring its actual decisions.

The current design can still be reviewed and improved within its authored palette and geometric catalogue. Marker separation, outline distinction, readable controls, and complete focus outlines are concrete quality work. They do not need a theme preference to be worthwhile.

If visual variety returns to discussion, these distinctions remain useful questions rather than inherited answers. [Roadmap notes](roadmap-notes.md) treat that as exploratory. The present game continues to use the dark treatment described in [Visuals](visuals.md), with no hidden promise that a theme chooser is waiting behind the existing token names.
