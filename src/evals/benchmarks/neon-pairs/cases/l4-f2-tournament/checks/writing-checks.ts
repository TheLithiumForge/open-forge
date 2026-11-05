import assert from 'node:assert/strict';
import { Control, Four, playOn, press, setPlayers, tournament, visibleText } from './tournament.ts';
import type { Page } from 'playwright';
import type { ChangeCheck } from './check.types.ts';

const Slower = { first: 65_000, second: 7000 } as const;

async function interfaceText(page: Page): Promise<string> {
  return page.evaluate(() => [document.body.innerText, ...[...document.querySelectorAll('[aria-label], [title], [placeholder]')].flatMap((element) => [element.getAttribute('aria-label') ?? '', element.getAttribute('title') ?? '', element.getAttribute('placeholder') ?? ''])].join('\n'));
}
function clock(milliseconds: number): string {
  const seconds = Math.floor(milliseconds / 1000);
  return `${Math.floor(seconds / 60)}:${String(seconds % 60).padStart(2, '0')}`;
}

export const writingChecks: readonly ChangeCheck[] = [
  { id: 'C01', description: 'C01: interface text has no exclamation marks, on any Tournament screen or message.', action: async (page) => {
    const seen: string[] = [];
    const look = async () => { seen.push(await interfaceText(page)); };
    await press(page, Control.open); await look();
    await setPlayers(page, ['x', 'x', 'x', 'x']); await press(page, Control.start); await look();
    await setPlayers(page, Four); await press(page, Control.start);
    await playOn(page, { visit: look });
    await press(page, Control.back); await press(page, Control.open); await look();
    assert.equal(seen.join('\n').includes('!'), false);
  } },
  { id: 'C02', description: 'C02: counts of moves read "2 moves", and durations read M:SS, in the bracket.', action: async (page) => {
    const run = await tournament(page, Four, { plan: ({ match, first }) => {
      if (match !== 0) return first === undefined ? {} : { known: first.layout };
      return first === undefined ? { delayMs: Slower.first } : { known: first.layout, delayMs: Slower.second };
    } });
    const text = (await visibleText(page)).replace(/\s+/g, ' ');
    for (const match of run.matches) for (const turn of match.turns) assert.ok(text.includes(`${turn.moves} moves`), `"${turn.moves} moves" shows.`);
    for (const turn of run.matches[0]?.turns ?? []) assert.ok(text.includes(clock(turn.elapsedMs)), `${clock(turn.elapsedMs)} shows.`);
  } },
];
