import assert from 'node:assert/strict';
import { settled } from './browser.ts';
import { begin, Control, Eight, Four, playOn, playersOf, press, PreferencesKey, sameCards, stored, tournament, visibleText, winnerOf } from './tournament.ts';
import type { Page } from 'playwright';
import type { ChangeCheck } from './check.types.ts';

const Rounds = { quarter: 'Quarterfinal', semi: 'Semifinal', final: 'Final' } as const;
const Sides = { quarter: 4, semi: 6, final: 8 } as const;
const SavedSize = 6;
const SlowerMs = 5000;
const seed = (names: readonly string[], number: number): string => names[number - 1] ?? '';
const unordered = (names: readonly string[]) => [...names].sort();

function pairingIs(actual: readonly string[], names: readonly string[], seeds: readonly number[]): void {
  assert.deepEqual(unordered(actual), unordered(seeds.map((number) => seed(names, number))), `Seeds ${seeds.join(' and ')} meet.`);
}
async function savedSize(page: Page): Promise<string> { return page.getByLabel('Grid size', { exact: true }).inputValue(); }

export const competitionChecks: readonly ChangeCheck[] = [
  { id: 'T01', description: 'T01: seeds follow entry order. Four meet 1v4 then 2v3, and eight meet 1v8, 4v5, 3v6, then 2v7.', action: async (page) => {
    await begin(page, Four);
    const four = await playOn(page, { stopAtMatch: 2 });
    pairingIs(playersOf(four, 0), Four, [1, 4]);
    pairingIs(playersOf(four, 1), Four, [2, 3]);
    await page.reload(); await settled(page);
    await begin(page, Eight);
    const eight = await playOn(page, { stopAtMatch: 4 });
    for (const [match, seeds] of [[1, 8], [4, 5], [3, 6], [2, 7]].entries()) pairingIs(playersOf(eight, match), Eight, seeds);
  } },
  { id: 'T02', description: 'T02: each round shows as "Round: NAME", and a bracket of four has no Quarterfinal.', action: async (page) => {
    const run = await tournament(page, Eight);
    const expected = [Rounds.quarter, Rounds.quarter, Rounds.quarter, Rounds.quarter, Rounds.semi, Rounds.semi, Rounds.final];
    assert.deepEqual(run.matches.map((match) => match.round), expected);
    await page.reload(); await settled(page);
    await begin(page, Four);
    assert.match(await visibleText(page), new RegExp(`Round: *${Rounds.semi}`));
  } },
  { id: 'T03', description: 'T03: quarterfinals are played on 4x4, semifinals on 6x6 and finals on 8x8.', action: async (page) => {
    const run = await tournament(page, Eight);
    const sides = [Sides.quarter, Sides.quarter, Sides.quarter, Sides.quarter, Sides.semi, Sides.semi, Sides.final];
    assert.deepEqual(run.matches.map((match) => Math.sqrt(match.turns[0]?.layout.length ?? 0)), sides);
  } },
  { id: 'T04', description: 'T04: both players of a match share one board code, and every match in a tournament has a different one.', action: async (page) => {
    const run = await tournament(page, Eight);
    for (const match of run.matches) assert.ok(sameCards(match.turns[0]?.layout, match.turns[1]?.layout), 'Both players share a deal.');
    assert.equal(new Set(run.matches.map((match) => match.turns[0]?.layout.join('|'))).size, run.matches.length, 'No two matches share a deal.');
  } },
  { id: 'T05', description: 'T05: equal moves go to the lower active time, and if that is equal too, to the higher seed.', action: async (page) => {
    const run = await tournament(page, Four, { plan: ({ match, turn, first }) => {
      if (match === 0 && turn === 0) return { delayMs: SlowerMs };
      if (match === 1 && turn === 1 && first !== undefined) return { known: first.layout, moves: first.moves, delayMs: SlowerMs };
      return first === undefined ? {} : { known: first.layout, moves: first.moves };
    } });
    assert.equal(winnerOf(run, 0), run.matches[0]?.turns[1]?.name, 'The first player spent more active time.');
    assert.equal(winnerOf(run, 1), run.matches[1]?.turns[0]?.name, 'The second player spent more active time.');
    const finalists = playersOf(run, 2);
    assert.equal(run.champion, Four.find((name) => finalists.includes(name)), 'The higher seed wins a complete tie.');
  } },
  { id: 'T06', description: 'T06: tournament games never change best scores or the saved board size.', action: async (page) => {
    await page.getByLabel('Grid size', { exact: true }).selectOption(String(SavedSize)); await settled(page);
    const before = (await stored(page))[PreferencesKey];
    await tournament(page, Four);
    assert.equal((await stored(page))[PreferencesKey], before, 'Preferences stay as they were during the tournament.');
    await press(page, Control.back);
    assert.equal((await stored(page))[PreferencesKey], before, 'Preferences stay as they were afterwards.');
    assert.equal(await savedSize(page), String(SavedSize), 'The game keeps its board size.');
    assert.match((await visibleText(page)).replace(/\s+/g, ' '), /Best:? none\b/, 'No best score appeared.');
  } },
];
