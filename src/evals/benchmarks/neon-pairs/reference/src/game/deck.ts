import type { Board } from './game.types.ts';

export const GRID_SIZES = [4, 6, 8, 10, 12, 14, 16] as const;
const BOARD_VERSION = 'np1';
export const SEED_MAX = 2_147_483_646;
const PRNG_MODULUS = SEED_MAX + 1;
const PRNG_MULTIPLIER = 16_807;

export function isValidSize(value: number): boolean {
  return GRID_SIZES.some((size) => size === value);
}

export function nextRandomState(state: number): number {
  return (state * PRNG_MULTIPLIER) % PRNG_MODULUS;
}

export function parseBoardCode(code: string): { size: number; seed: number } | null {
  const parts = new RegExp(`^${BOARD_VERSION}-([1-9]\\d*)-([1-9]\\d*)$`).exec(code);
  if (!parts) return null;
  const size = Number(parts[1]);
  const seed = Number(parts[2]);
  return isValidSize(size) && Number.isSafeInteger(seed) && seed <= SEED_MAX
    ? { size, seed }
    : null;
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
    if (currentFace === undefined || otherFace === undefined)
      throw new Error('Deck index out of range.');
    deck[index] = otherFace;
    deck[other] = currentFace;
  }
  return Object.freeze({ boardCode: code, ...parsed, deck: Object.freeze(deck) });
}
