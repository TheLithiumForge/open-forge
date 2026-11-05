# Pair identity

This page explains the numeric face identities in a deck, their visual design, and why a face number is different from a card position.

A pair is two distinct unmatched positions whose deck values are equal. The core comparison is `state.deck[first] === state.deck[index]`. Neither location nor visual resemblance substitutes for that equality.

## Two coordinate systems

The deck is an array in row order. Its index names a position and its value names a face. For `np1-4-1`:

```text
position:  0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15
face:      6  7  0  5  2  2  0  5  3  7  1  6  4  4  1  3
```

Positions 2 and 6 form the pair for face 0. Positions 0 and 11 form the pair for face 6. Treating an activation of index 6 as an activation of face 6 would select the wrong identity in this example.

Progress fields contain positions. `matched: [2, 6]` records the two locations of a found pair. A one-card selection with index 0 identifies the first board position, whose face happens to be 6. The selection does not store a copied face value; it reads identity from the unchanged deck.

Human-readable positions are one-based rows and columns. `describeCard` computes row as `floor(index / size) + 1` and column as `index % size + 1`. On a four-column board, index 6 is row 2, column 3. This conversion changes the description, not the underlying index.

## How identities enter a deck

`generateBoard` creates two copies of each consecutive integer from zero through `size * size / 2 - 1`, then shuffles positions. The shuffle exchanges existing values, so it preserves exactly two occurrences of each face.

Smaller sizes use an initial segment of the face range. A new seed moves those identities but does not select a different assortment of faces for the same size. A larger size extends the range. The full 16x16 deck includes face numbers 0 through 127.

The two copies do not have separate copy identifiers. Once the deck is made, their positions distinguish which card was activated. Comparing a position with itself is excluded before face comparison, so a single occurrence cannot match itself even though its value equals itself.

[Deck generation](deck-generation.md) gives the shuffle arithmetic. [Board size catalogue](board-size-catalogue.md) lists how far the face range extends at each size.

## From a number to a face

`src/game/board/face-design.ts` exports `FACE_COUNT = 128` and `faceDesign(face)`. The function rejects a noninteger, a negative number, or a value at or beyond 128 with `Error('Invalid face.')`.

The central shape comes from the face number modulo eight:

| Remainder | Shape |
| ---: | --- |
| 0 | Circle |
| 1 | Square |
| 2 | Triangle |
| 3 | Diamond |
| 4 | Star |
| 5 | Hexagon |
| 6 | Plus |
| 7 | Chevron |

The marker group is `floor(face / 8) % 8`. Its directions run top, top-right, right, bottom-right, bottom, bottom-left, left, and top-left. Fill treatment is outline when `floor(face / 64) === 1`, and solid for the lower half of the valid range.

Together the eight shapes, eight marker directions, and two fill treatments give 128 distinct combinations. Color follows the marker group: cyan, magenta, green, yellow, orange, violet, rose, and ice. It is not an extra independently shuffled identity dimension.

This mapping is deterministic and independent of board seed. Face 72 has the same design on every board that includes it. The seed determines where it appears, not how it is drawn.

## Similarity is not equality

Consider four valid face numbers:

| Face | Shape | Marker | Fill |
| ---: | --- | --- | --- |
| 0 | Circle | Top | Solid |
| 8 | Circle | Top-right | Solid |
| 64 | Circle | Top | Outline |
| 72 | Circle | Top-right | Outline |

All four have the same central shape. Face 0 and 64 even share the marker direction and color. They remain different identities because the fill differs. Only another occurrence of the exact face number matches.

At 4x4, the deck only includes faces 0 through 7, so central shape distinguishes every pair in that board. From 6x6 onward, shapes repeat across marker groups. From 12x12 onward, both fill treatments can appear. This is a property of the included range, not a change in rules between sizes.

The introductory UI text mentions matching shape and marker. On boards that include outlines, the complete design also distinguishes fill. Revealed accessible labels include the fill word, so a full identity can be described without relying on color alone.

## Rendering a revealed identity

`Face.tsx` obtains the design and draws an SVG central shape and a circular marker. Solid shapes use the design color as fill; outline shapes use no fill and a colored stroke. The marker sits on one of eight radial positions and has a contrasting stroke.

The SVG is hidden from the accessibility tree. `describeCard` adds the design's text label to the card button's accessible name when the face is revealed. For example, face 0 supplies `cyan solid circle, marker top` after the row, column, and card state.

The matched check mark is a separate SVG added when a position is matched. It communicates found progress and does not create a different face identity. A revealed face and its later matched rendering are the same face.

The detailed artwork reference is [Card artwork](card-artwork.md), with the complete collection in [Face catalogue](face-catalogue.md).

## Hidden identity stays in game data

`Board.tsx` iterates the deck, but passes a face number to `Card` only when the derived face-up set includes that position. Hidden positions receive `face: null`. `Card` renders a card back for that case and does not mount `Face`.

The accessible name of a hidden card includes its position and closed state, without the design label. A developer should not expect to recover the hidden face from a card attribute in the current rendered interface. The deck is still present in the in-memory game state because the core needs it to resolve guesses.

The board uses position indices as React keys. That preserves the relationship between each rendered card and its position during ordinary progress. The session generation keys the whole Board component on replacement, so another deal starts a new board instance even when the size is unchanged.

## Identity survives progress changes

Revealing a card, matching a pair, waiting through a mismatch, and advancing time do not rearrange the deck. A pair does not move together when found. The `matched` array is separate, and its order is not a new spatial ordering of cards.

Restart regenerates the existing board code, preserving the entire position-to-face mapping. New Game requests another seed and makes another mapping. Loading a snapshot regenerates the deck from its board code, then overlays matched positions and temporary selection.

That last step explains why snapshot validation needs the actual generated deck. Two indices that look like a pair syntactically are only a complete matched pair when their deck values agree. A pending mismatch likewise needs different face values. The external data cannot redefine the deck by assigning its own face labels.

## Tests and useful reproduction details

`deck.test.ts` checks two copies of every included face. `face-design.test.ts` exercises the design catalogue, and `card-state.test.ts` covers state descriptions. Core game tests use known position pairs from the published deck to verify equality-based matching.

A useful identity report includes the board code, both position indices or full row-and-column locations, and the complete revealed labels. “Two stars did not match” is ambiguous once marker groups repeat. “The two top-marker solid stars on this code” narrows the question to an identity that can be checked against deck values.

If the generated values differ, the miss is expected even if part of the art looks similar. If the values agree but the core reports a mismatch, the defect is in state or activation handling. If the values agree and the labels or shapes differ, investigate the face-design and rendering path. Keeping the position and face concepts distinct makes those cases separable.
