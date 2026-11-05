import assert from 'node:assert/strict';
import { SettleTasks, settled } from './browser.ts';
import type { Page } from 'playwright';

export const CardState = { closed: 'closed', revealed: 'revealed', matched: 'matched' } as const;
export const MismatchMs = 800;
const CardName = /^Row \d+, column \d+, (closed|revealed|matched)(?:, (.+))?$/;
const AttemptsPerCard = 6;

export interface GamePlan {
  /** A layout already seen, so the game can be played without reading it again. */
  readonly known?: readonly string[];
  /** The number of moves to end on, reached with deliberate mismatches. */
  readonly moves?: number;
  /** Active time to spend, after the first card. */
  readonly delayMs?: number;
}
export interface GameResult { readonly moves: number; readonly elapsedMs: number; readonly layout: readonly string[] }
interface Card { readonly state: string; readonly face: string | null }

function parsed(names: readonly string[]): Card[] {
  return names.map((name) => {
    const match = CardName.exec(name);
    return { state: match?.[1] ?? '', face: match?.[2] ?? null };
  });
}
export async function boardCards(page: Page): Promise<Card[]> {
  return parsed(await page.evaluate((pattern) => [...document.querySelectorAll('button')].map((button) => button.getAttribute('aria-label') ?? '').filter((name) => new RegExp(pattern).test(name)), CardName.source));
}
async function reveal(page: Page, index: number): Promise<Card[]> {
  return parsed(await page.evaluate(async ({ index, pattern, tasks }) => {
    const cards = () => [...document.querySelectorAll('button')].filter((button) => new RegExp(pattern).test(button.getAttribute('aria-label') ?? ''));
    cards()[index]?.click();
    for (let task = 0; task < tasks; task++) await new Promise<void>((done) => { const channel = new MessageChannel(); channel.port1.onmessage = () => { channel.port1.close(); done(); }; channel.port2.postMessage(null); });
    return cards().map((button) => button.getAttribute('aria-label') ?? '');
  }, { index, pattern: CardName.source, tasks: SettleTasks }));
}
function closedIndexes(cards: readonly Card[]): number[] {
  return cards.flatMap((card, index) => card.state === CardState.closed ? [index] : []);
}
function knownPair(closed: readonly number[], memory: ReadonlyMap<number, string>): readonly [number, number] | null {
  const seen = new Map<string, number>();
  for (const index of closed) {
    const face = memory.get(index);
    if (face === undefined) continue;
    const other = seen.get(face);
    if (other !== undefined) return [other, index];
    seen.set(face, index);
  }
  return null;
}
function knownMismatch(closed: readonly number[], memory: ReadonlyMap<number, string>): readonly [number, number] | null {
  const known = closed.filter((index) => memory.has(index));
  for (const first of known) for (const second of known) if (memory.get(first) !== memory.get(second)) return [first, second];
  return null;
}
function partnerOf(index: number, closed: readonly number[], memory: ReadonlyMap<number, string>): number | undefined {
  return closed.find((other) => other !== index && memory.get(other) === memory.get(index));
}

/** Plays the board on screen to its end, remembering what it reveals as a careful player would. A page may take the board away as soon as the last pair matches. */
export async function playGame(page: Page, plan: GamePlan = {}): Promise<GameResult> {
  const memory = new Map<number, string>((plan.known ?? []).map((face, index) => [index, face]));
  let cards = await boardCards(page);
  const total = cards.length;
  assert.ok(total > 0, 'A board is on screen.');
  let moves = 0, elapsedMs = 0, delayed = false;
  const open = async (index: number): Promise<string | null> => {
    cards = await reveal(page, index);
    if (cards.length === 0) return null;
    const face = cards[index]?.face;
    assert.ok(cards[index]?.state !== CardState.closed && face !== null && face !== undefined, 'A revealed card shows its face.');
    memory.set(index, face);
    if (!delayed && plan.delayMs !== undefined) { await page.clock.runFor(plan.delayMs); elapsedMs += plan.delayMs; delayed = true; }
    return face;
  };
  for (let attempt = 0; closedIndexes(cards).length > 0; attempt++) {
    assert.ok(attempt < total * AttemptsPerCard, 'The game reached its end.');
    const closed = closedIndexes(cards);
    const spare = plan.moves === undefined ? false : plan.moves - moves > closed.length / 2;
    const chosen = spare ? knownMismatch(closed, memory) : knownPair(closed, memory);
    const first = chosen?.[0] ?? closed.find((index) => !memory.has(index)) ?? closed[0];
    assert.ok(first !== undefined);
    const firstFace = await open(first);
    const second = chosen?.[1] ?? partnerOf(first, closed, memory) ?? closed.find((index) => index !== first && !memory.has(index)) ?? closed.find((index) => index !== first);
    assert.ok(firstFace !== null && second !== undefined, 'A second closed card remains.');
    const secondFace = await open(second);
    moves++;
    if (secondFace === null) { memory.set(second, firstFace); break; }
    if (firstFace !== secondFace) { await page.clock.runFor(MismatchMs); elapsedMs += MismatchMs; await settled(page); cards = await boardCards(page); }
  }
  return { moves, elapsedMs, layout: Array.from({ length: total }, (_, index) => memory.get(index) ?? '') };
}
