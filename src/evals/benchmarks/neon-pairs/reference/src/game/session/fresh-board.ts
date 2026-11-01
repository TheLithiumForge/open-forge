import { boardCode } from '../deck.ts';
import { SEED_MAX } from '../rules.ts';

const UINT32_RANGE = 2 ** 32;
const RANDOM_LIMIT = Math.floor(UINT32_RANGE / SEED_MAX) * SEED_MAX;

export function freshBoardCode(size: number, previousSeed?: number): string {
  const buffer = new Uint32Array(1);
  for (;;) {
    crypto.getRandomValues(buffer);
    const draw = buffer[0];
    if (draw === undefined || draw >= RANDOM_LIMIT) continue;
    const seed = draw % SEED_MAX + 1;
    if (seed !== previousSeed) return boardCode(size, seed);
  }
}
