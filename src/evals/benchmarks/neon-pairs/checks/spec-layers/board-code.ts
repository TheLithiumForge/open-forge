import { Rules, type BoardCode } from "./rules.ts";

export function parseBoardCode(text: unknown): BoardCode | null {
  if (typeof text !== "string") return null;
  const match = /^np1-([1-9]\d*)-([1-9]\d*)$/.exec(text);
  if (!match || match[0] !== text) return null;
  const size = Number(match[1]), seed = Number(match[2]);
  return Rules.sizes.some((supported) => supported === size) && Number.isSafeInteger(seed) && seed <= Rules.seedMaximum ? { size, seed } : null;
}

export function generator(seed: number): () => number {
  if (!Number.isSafeInteger(seed) || seed < 1 || seed > Rules.seedMaximum) throw new Error("Invalid oracle seed.");
  let state = BigInt(seed);
  return () => { state = state * BigInt(Rules.multiplier) % BigInt(Rules.modulus); return Number(state); };
}

export function boundedDraw(next: () => number, possibilities: number): number {
  if (!Number.isSafeInteger(possibilities) || possibilities < 1 || possibilities > Rules.seedMaximum) throw new Error("Invalid bounded draw size.");
  const limit = Math.floor(Rules.seedMaximum / possibilities) * possibilities;
  for (;;) {
    const value = next() - 1;
    if (value < 0 || value >= Rules.seedMaximum) throw new Error("Generator value is outside its range.");
    if (value < limit) return value % possibilities;
  }
}

export function expectedDeck(code: string): number[] {
  const parsed = parseBoardCode(code);
  if (!parsed) throw new Error("Invalid board code in oracle input.");
  const deck = Array.from({ length: parsed.size ** 2 }, (_, index) => Math.floor(index / 2));
  const next = generator(parsed.seed);
  for (let index = deck.length - 1; index > 0; index -= 1) {
    const other = boundedDraw(next, index + 1);
    const first = deck[index], second = deck[other];
    if (first === undefined || second === undefined) throw new Error("Oracle shuffle index is missing.");
    deck[index] = second; deck[other] = first;
  }
  return deck;
}
