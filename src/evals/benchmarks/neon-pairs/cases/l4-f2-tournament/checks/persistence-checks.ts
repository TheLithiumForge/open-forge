import assert from 'node:assert/strict';
import { settled } from './browser.ts';
import { Control, entryHolding, press, stored, tournament, writtenText } from './tournament.ts';
import type { Page } from 'playwright';
import type { ChangeCheck } from './check.types.ts';

const KeyForm = /^np3:[a-z][a-z-]*s$/;
const Envelope = { version: 3, keys: ['items', 'v'] } as const;
const Blocked = "Progress won't be saved on this device.";
const Kept = 10;
const Broken = ['{"v":3,"items":', '{"v":3,"items":"nothing"}'] as const;
const Complaint = /error|corrupt|invalid|could not|couldn't|failed/i;
const players = (prefix: string) => ['a', 'b', 'c', 'd'].map((letter) => `${prefix}${letter}`);

async function finished(page: Page, prefix: string): Promise<string> {
  const run = await tournament(page, players(prefix));
  assert.ok(run.champion !== null, 'A champion is named.');
  await press(page, Control.back);
  return run.champion;
}
async function setValue(page: Page, key: string, value: string): Promise<void> {
  await page.evaluate(([name, text]) => { if (name !== undefined && text !== undefined) localStorage.setItem(name, text); }, [key, value]);
}

export const persistenceChecks: readonly ChangeCheck[] = [
  { id: 'P01', description: 'P01: new stored data uses a key of the form np3:THING, with THING a lowercase plural noun.', action: async (page) => {
    const [key] = await entryHolding(page, await finished(page, 'Pk'));
    assert.match(key, KeyForm);
  } },
  { id: 'P02', description: 'P02: a stored value is a JSON object with exactly the keys "v", the number 3, and "items", the array of records.', action: async (page) => {
    const [, text] = await entryHolding(page, await finished(page, 'Pv'));
    const value = JSON.parse(text) as Record<string, unknown>;
    assert.deepEqual(Object.keys(value).sort(), Envelope.keys);
    assert.equal(value['v'], Envelope.version);
    assert.ok(Array.isArray(value['items']) && value['items'].length === 1, 'The records are in the array.');
  } },
  { id: 'P03', description: 'P03: a stored list keeps its newest 10 records and drops older ones when a new record is saved.', action: async (page) => {
    const prefix = (round: number) => `R${String(round).padStart(2, '0')}`;
    for (let round = 0; round <= Kept; round++) await finished(page, prefix(round));
    const text = Object.values(await stored(page)).join('\n');
    assert.equal(text.includes(prefix(0)), false, 'The oldest record is dropped.');
    for (let round = 1; round <= Kept; round++) assert.ok(text.includes(prefix(round)), 'The newest records stay.');
  } },
  { id: 'P04', session: { blockedStorage: true }, description: 'P04: with storage blocked the feature works in memory and shows "Progress won\'t be saved on this device." where its records would appear.', action: async (page) => {
    const champion = await finished(page, 'Pb');
    await press(page, Control.open);
    const text = await writtenText(page);
    assert.ok(text.includes(Blocked), 'The notice shows.');
    assert.ok(text.includes(champion), 'The finished tournament is still listed in memory.');
  } },
  { id: 'P05', description: 'P05: a stored value that fails to parse or validate is treated as empty without any error, and stays untouched until the next save replaces it.', action: async (page) => {
    const champion = await finished(page, 'Pc');
    const [key] = await entryHolding(page, champion);
    for (const broken of Broken) {
      await setValue(page, key, broken);
      await page.reload(); await settled(page);
      await press(page, Control.open);
      const text = await writtenText(page);
      assert.equal(text.includes(champion), false, 'Nothing is listed.');
      assert.equal(Complaint.test(text), false, 'No error shows.');
      assert.equal((await stored(page))[key], broken, 'The value stays untouched.');
      await page.reload(); await settled(page);
    }
    const next = await finished(page, 'Pn');
    assert.ok((await stored(page))[key]?.includes(next), 'The next save replaces it.');
    JSON.parse((await stored(page))[key] ?? '');
  } },
];
