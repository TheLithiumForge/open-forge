export const GRID_SIZES = [4, 6, 8, 10, 12, 14, 16] as const;
export const DEFAULT_SIZE = GRID_SIZES[0];
export const MISMATCH_MS = 800;
export const SNAPSHOT_VERSION = 1;
export const SNAPSHOT_MAX_BYTES = 65_536;
export const REPLAY_VERSION = 1;
export const REPLAY_MAX_BYTES = 1_048_576;
export const REPLAY_MAX_ACTIONS = 10_000;
export const BOARD_VERSION = 'np1';
export const SEED_MAX = 2_147_483_646;
export const PRNG_MODULUS = SEED_MAX + 1;
export const PRNG_MULTIPLIER = 16_807;
export const SelectionKind = { closed: 'closed', one: 'one', mismatch: 'mismatch' } as const;

export function validSize(value: number): boolean {
  return GRID_SIZES.some((size) => size === value);
}

export function safeNonnegative(value: unknown): value is number {
  return typeof value === 'number' && Number.isSafeInteger(value) && value >= 0;
}
