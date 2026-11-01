import { readFile } from 'node:fs/promises';
import { record } from '../src/game/saved-format.ts';

function strings(value: unknown): value is readonly string[] {
  return Array.isArray(value) && value.every((item) => typeof item === 'string');
}

export async function accessibilityFixture() {
  const value: unknown = JSON.parse(
    await readFile(new URL('./accessibility.fixture.json', import.meta.url), 'utf8'),
  );
  if (
    !record(value) ||
    typeof value.size !== 'number' ||
    !record(value.controls) ||
    typeof value.controls.size !== 'string' ||
    typeof value.controls.newGame !== 'string' ||
    typeof value.controls.restart !== 'string' ||
    typeof value.controls.summary !== 'string' ||
    !strings(value.closed) ||
    !strings(value.revealed) ||
    !strings(value.matched) ||
    !strings(value.turns) ||
    typeof value.win !== 'string' ||
    typeof value.newGame !== 'string' ||
    typeof value.readmeRecord !== 'string'
  )
    throw new Error('Expected the reviewed accessibility journey fixture.');
  return {
    size: value.size,
    controls: {
      size: value.controls.size,
      newGame: value.controls.newGame,
      restart: value.controls.restart,
      summary: value.controls.summary,
    },
    closed: value.closed,
    revealed: value.revealed,
    matched: value.matched,
    turns: value.turns,
    win: value.win,
    newGame: value.newGame,
    readmeRecord: value.readmeRecord,
  };
}

export async function playwrightVersion(): Promise<string> {
  const value: unknown = JSON.parse(
    await readFile(new URL(import.meta.resolve('@playwright/test/package.json')), 'utf8'),
  );
  if (!record(value) || typeof value.version !== 'string')
    throw new Error('Missing Playwright version.');
  return value.version;
}
