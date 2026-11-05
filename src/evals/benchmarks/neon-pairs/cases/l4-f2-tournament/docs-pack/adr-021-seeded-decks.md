# ADR 021: Generate a deck from a specified seed

This record explains the seeded shuffle, exact draw mapping and independent generation evidence.

## Decision

A deal is generated deterministically from its board code. Version `np1` fixes the generator and shuffle, so every supported runtime can reconstruct the same face IDs in the same positions. The browser's fresh-seed selection is a separate operation from expanding that seed into a deck.

The initial deck is the sequence of pairs `[0, 0, 1, 1, ...]` through the face count for the chosen size. The shuffle walks backward from the final position to index 1. For each position it chooses another index from zero through that position and swaps. The output board and deck are frozen.

## Generator choice

The generator uses the Park-Miller recurrence: multiply the state by 16807 and take the remainder modulo 2147483647. Allowed seeds run from 1 through 2147483646. The multiplication stays within exact integer representation for these values in JavaScript, so a floating-point approximation is not needed to reproduce the specified recurrence.

Seed 1 begins with generator states 16807, 282475249, 1622650073, 984943658 and 1144108930. Those numbers are useful checkpoints when an independent implementation agrees on the code syntax but produces a different deck. [Deck generation](deck-generation.md) gives the complete procedure.

## Mapping draws to bounds

At a shuffle step with `bound` possible destinations, the code computes the greatest multiple of that bound no larger than the generator's usable range. It subtracts one from the generator state, rejects values at or above that limit, and maps accepted values with the remainder operation. Rejection can consume more than one generator advance for a single swap.

That detail has two consequences. Each destination has the same number of accepted source values, avoiding a simple modulo imbalance. More immediately for reproducibility, skipping the rejection step changes which generator state later swaps consume. An implementation that looks like the same Fisher-Yates shuffle can still disagree with the public code contract.

## Freshness is elsewhere

`fresh-board.ts` uses browser cryptographic draws to select a valid seed and avoids the previous seed for a new deal. Once selected, the seed is just input to pure generation. Rendering frequency, animation and clock observations do not draw from the deck generator. Restart passes the existing code back through construction rather than drawing again.

This separation makes investigations repeatable. A tester can specify `np1-4-1`, know the sixteen literal face IDs and perform actions through the real UI. There is no need for a private endpoint exposing the hidden deck. [ADR 009](adr-009-board-codes.md) explains the identifier and [Randomness policy](randomness-policy.md) describes the existing boundaries.

## Alternatives and evidence

An unspecified shuffle would produce playable pairs but could not be reconstructed consistently across tools. Storing the entire shuffled deck would avoid an algorithm contract at the cost of a longer, more redundant representation. A seed with an unspecified platform PRNG would only move the ambiguity into another layer.

The tests therefore include literal vectors and rejection cases, not merely permutation checks. Pair counts and uniqueness are necessary but would not detect a changed draw order. Browser fixtures also use an independent oracle. The algorithm is small enough to inspect, yet its exact sequencing deserves strong evidence because it is part of the versioned format. See [ADR 048](adr-048-replay-order.md) for the corresponding action-order decision.

## The complete shuffle loop

This excerpt shows initialization, rejection, swap order and freezing together. All of those choices contribute to the deal associated with a code.

```ts
export function generateBoard(code: string): Board {
  const parsed = parseBoardCode(code);
  if (!parsed) throw new Error('Invalid board code.');
  const deck = Array.from({ length: parsed.size ** 2 }, (_, index) => Math.floor(index / 2));
  let state = parsed.seed;
  for (let index = deck.length - 1; index > 0; index -= 1) {
    const bound = index + 1;
    const limit = Math.floor(SEED_MAX / bound) * bound;
    let draw: number;
    do {
      state = nextRandomState(state);
      draw = state - 1;
    } while (draw >= limit);
    const other = draw % bound;
    const currentFace = deck[index];
    const otherFace = deck[other];
    if (currentFace === undefined || otherFace === undefined)
      throw new Error('Deck index out of range.');
    deck[index] = otherFace;
    deck[other] = currentFace;
  }
  return Object.freeze({ boardCode: code, ...parsed, deck: Object.freeze(deck) });
}
```
