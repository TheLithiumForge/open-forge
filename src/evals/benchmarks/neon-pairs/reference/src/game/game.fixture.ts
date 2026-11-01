export const EXAMPLE_CODE = 'np1-4-1';
export const EXAMPLE_DECK = [6, 7, 0, 5, 2, 2, 0, 5, 3, 7, 1, 6, 4, 4, 1, 3];
export const EXAMPLE_PAIRS = [
  [2, 6],
  [10, 14],
  [4, 5],
  [8, 15],
  [12, 13],
  [3, 7],
  [0, 11],
  [1, 9],
] as const;
export const RANDOM_STATES = [16_807, 282_475_249, 1_622_650_073, 984_943_658, 1_144_108_930];
export const SPEC_VECTOR = {
  mismatchMs: 800,
  snapshotVersion: 1,
  snapshotBytes: 65_536,
  replayVersion: 1,
  replayBytes: 1_048_576,
  replayActions: 10_000,
  sizes: [4, 6, 8, 10, 12, 14, 16],
  seedMax: 2_147_483_646,
} as const;
