import assert from 'node:assert/strict';
import { Control, Four, nowPlaying, press, setPlayers, tournament, visibleText, writtenText } from './tournament.ts';
import type { Page } from 'playwright';
import type { ChangeCheck } from './check.types.ts';

const Message = { length: 'Use 2 to 20 characters.', duplicate: 'Each name must be different.' } as const;
const [, Second = '', Third = '', Fourth = ''] = Four;
const Spaced = { typed: '  Ana \t  Maria  ', stored: 'Ana Maria' } as const;
const Decomposed = { typed: 'Café Bar', stored: 'Café Bar' } as const;
const CodePoints = { fine: '\u{1F600}'.repeat(20), over: 'x'.repeat(21), under: 'x', spacedUnder: '  x  ' } as const;

async function attempt(page: Page, first: string, second: string = Second): Promise<void> {
  await setPlayers(page, [first, second, Third, Fourth]);
  await press(page, Control.start);
}
async function blockedWith(page: Page, message: string, first: string, second?: string): Promise<void> {
  await attempt(page, first, second);
  assert.ok((await visibleText(page)).includes(message), `"${first}" shows the message.`);
  assert.equal(await nowPlaying(page), null, `"${first}" starts nothing.`);
}

export const nameChecks: readonly ChangeCheck[] = [
  { id: 'N01', description: 'N01: names are stored and shown trimmed, with inner whitespace collapsed and normalized to NFC.', action: async (page) => {
    await tournament(page, [Spaced.typed, Decomposed.typed, Third, Fourth]);
    const text = await writtenText(page);
    assert.ok(text.includes(Spaced.stored), 'Inner whitespace collapses.');
    assert.equal(text.includes('Ana  '), false, 'No run of spaces remains.');
    assert.ok(text.includes(Decomposed.stored), 'Text is normalized to NFC.');
    assert.equal(text.includes(Decomposed.typed), false, 'No decomposed text remains.');
  } },
  { id: 'N02', description: 'N02: a name has 2 to 20 code points after normalization, or "Use 2 to 20 characters." blocks the action.', action: async (page) => {
    await press(page, Control.open);
    await blockedWith(page, Message.length, CodePoints.under);
    await blockedWith(page, Message.length, CodePoints.spacedUnder);
    await blockedWith(page, Message.length, CodePoints.over);
    await attempt(page, CodePoints.fine);
    assert.ok(await nowPlaying(page) !== null, '20 code points are allowed, whatever their UTF-16 length.');
  } },
  { id: 'N03', description: 'N03: names compare without case after normalization, or "Each name must be different." blocks the action.', action: async (page) => {
    await press(page, Control.open);
    await blockedWith(page, Message.duplicate, 'Ann', 'aNN');
    await blockedWith(page, Message.duplicate, 'Zoë', 'ZOË');
  } },
];
