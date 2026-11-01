// An independent reading of D03 and D04 in exact BigInt arithmetic, kept apart from the game's deck code.
const MULTIPLIER = 16807n;
const MODULUS = 2147483647n;
const DRAW_RANGE = 2147483646n;

export function specDeal(size: number, seed: number): number[] {
  const deck = Array.from({ length: size * size }, (_, position) => Math.floor(position / 2));
  let state = BigInt(seed);
  for (let index = deck.length - 1; index > 0; index -= 1) {
    const choices = BigInt(index + 1);
    const limit = (DRAW_RANGE / choices) * choices;
    let draw: bigint;
    do {
      state = (state * MULTIPLIER) % MODULUS;
      draw = state - 1n;
    } while (draw >= limit);
    const other = Number(draw % choices);
    const current = deck[index];
    const swapped = deck[other];
    if (current === undefined || swapped === undefined) throw new Error('Deal index out of range.');
    deck[index] = swapped;
    deck[other] = current;
  }
  return deck;
}

export function specPairs(size: number, seed: number): number[][] {
  const pairs = new Map<number, number[]>();
  specDeal(size, seed).forEach((face, index) =>
    pairs.set(face, [...(pairs.get(face) ?? []), index]),
  );
  return [...pairs.values()];
}
