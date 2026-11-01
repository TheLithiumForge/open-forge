import { readFile } from 'node:fs/promises';
import { isJsonObject } from '../src/game/saved-format.ts';
import { SNAPSHOT_PATH } from './game-fixture.ts';

export async function uiSnapshot() {
  const value: unknown = JSON.parse(await readFile(SNAPSHOT_PATH, 'utf8'));
  if (
    !isJsonObject(value) ||
    typeof value.bestNone !== 'string' ||
    typeof value.winBanner !== 'string' ||
    typeof value.mismatch !== 'string' ||
    typeof value.winningResult !== 'string'
  ) {
    throw new Error('Expected the reviewed UI snapshot.');
  }
  return {
    bestNone: value.bestNone,
    winBanner: value.winBanner,
    mismatch: value.mismatch,
    winningResult: value.winningResult,
  };
}
