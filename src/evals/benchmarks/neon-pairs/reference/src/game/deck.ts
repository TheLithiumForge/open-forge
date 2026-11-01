import { BOARD_VERSION, PRNG_MODULUS, PRNG_MULTIPLIER, SEED_MAX, validSize } from './rules.ts';
import type { Board } from './game.types.ts';

export function nextRandomState(state: number): number {
  return (state * PRNG_MULTIPLIER) % PRNG_MODULUS;
}

export function parseBoardCode(code: string): { size: number; seed: number } | null {
  const parts = /^np1-([1-9]\d*)-([1-9]\d*)$/.exec(code);
  if (!parts) return null;
  const size = Number(parts[1]);
  const seed = Number(parts[2]);
  return validSize(size) && Number.isSafeInteger(seed) && seed <= SEED_MAX ? { size, seed } : null;
}

export function boardCode(size: number, seed: number): string {
  const code = `${BOARD_VERSION}-${size}-${seed}`;
  if (!parseBoardCode(code)) throw new Error('Invalid board code.');
  return code;
}

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
    if (currentFace === undefined || otherFace === undefined) throw new Error('Deck index out of range.');
    deck[index] = otherFace;
    deck[other] = currentFace;
  }
  return { boardCode: code, ...parsed, deck };
}
