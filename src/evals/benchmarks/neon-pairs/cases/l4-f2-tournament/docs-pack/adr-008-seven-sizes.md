# ADR 008: Offer seven even square sizes

This record explains the supported board sizes and their effect on rules, artwork and layout.

## Decision

The supported side lengths are 4, 6, 8, 10, 12, 14 and 16. The first is the default when no usable saved preference supplies another size. `GRID_SIZES` in `deck.ts` is the list used by the selector and size validation. The game does not accept arbitrary dimensions or rectangular boards.

Even square sides make every board contain an even number of positions. Each face can occur twice without a spare card, and row-order indices can use one side length for both navigation axes. This does not make other shapes impossible; it keeps the current product's rules and controls compact.

## Range and progression

The sizes cover 16 through 256 cards, or 8 through 128 pairs. The intermediate choices provide a meaningful increase in memory load without requiring a new mode. A 6x6 board has 18 pairs; an 8x8 has 32; 10x10 has 50; 12x12 has 72; 14x14 has 98. The same reveal, match and mismatch behavior applies throughout.

The number of faces follows the largest size. Face IDs run from zero through the board's pair count minus one, and the full design catalogue covers the 128 identities needed for 16x16. Small boards use a prefix of that catalogue. They do not sample a separately curated selection of easier or harder symbols. [ADR 034](adr-034-face-count.md) explains the connection to artwork.

## Size is part of reproducibility

A board code includes its side length, so reproducing a deal also reproduces its dimensions. `parseBoardCode` rejects a size outside the supported list. Snapshots rebuild the board from that code rather than accepting a separate size field that could disagree with it. The saved-preference parser also validates supported sizes.

Selecting a new size asks the controller for a fresh deal at that size. A successful board-code or snapshot load updates the selected size to match the loaded game. A failed load leaves the active deal in place. This creates a consistent meaning for the selector: it describes the board currently being played, not merely a preference for the next visit. See [Board size catalogue](board-size-catalogue.md) and [New game and restart](new-game-and-restart.md).

## Layout consequence

Larger boards are allowed to exceed the available board width after cards reach their minimum size. They scroll inside the board viewport instead of shrinking every target until 16 columns fit a phone. The same DOM board and rules serve all sizes, so size support includes keyboard movement and focused-card visibility, not just producing a deck of the right length.

A free-form size input was considered unnecessary for this range. It would have invited unsupported face counts, additional validation copy and dimensions with no acceptance evidence. A much smaller fixed menu would have made the maximum-board work irrelevant to most choices. Seven supported values gave a bounded catalogue with room to move between short and long sessions.

## Evidence and tradeoff

Tests exercise deck composition across the list and browser completion across supported sizes. Layout journeys give special attention to 16x16 at phone and desktop viewports. Best moves are scoped to each size because the number of pairs changes substantially; [ADR 032](adr-032-best-score-scope.md) records that choice.

The fixed list is intentionally visible in formats and validation as well as in the UI. Changing that list would therefore be more than adding an option label. It would affect deck capacity, preference acceptance and the meaning of reproducible codes. The current decision keeps those surfaces aligned around the same seven values.

## Size validation at the code boundary

The catalogue and generator limits live with deck behavior rather than being copied out of the select element.

```ts
export const GRID_SIZES = [4, 6, 8, 10, 12, 14, 16] as const;
const BOARD_VERSION = 'np1';
export const SEED_MAX = 2_147_483_646;
const PRNG_MODULUS = SEED_MAX + 1;
const PRNG_MULTIPLIER = 16_807;
```

## Membership check

Consumers ask the deck module whether a numeric side length is supported.

```ts
export function isValidSize(value: number): boolean {
  return GRID_SIZES.some((size) => size === value);
}
```
