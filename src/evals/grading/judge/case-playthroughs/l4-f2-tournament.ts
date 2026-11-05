import { mkdir, writeFile } from 'node:fs/promises';
import { join } from 'node:path';
import { chromium } from 'playwright';
import type { Browser, Page } from 'playwright';
import { PhoneViewport, session, settled } from '../../../benchmarks/neon-pairs/cases/l4-f2-tournament/checks/browser.ts';
import { markMainBoard, playGame } from '../../../benchmarks/neon-pairs/cases/l4-f2-tournament/checks/board.ts';
import type { GamePlan, GameResult } from '../../../benchmarks/neon-pairs/cases/l4-f2-tournament/checks/board.ts';
import { button, championOf, Control, exported, offered, playerField, press, setPlayers, visibleText } from '../../../benchmarks/neon-pairs/cases/l4-f2-tournament/checks/tournament.ts';

/** Plays one built game the same way every time, for judges to read. Nothing here passes or fails. A game this script can't finish simply has a shorter trace. */
const LongName = 'Maximilian Alexander';
const EightPlayers = [LongName, 'Bob', 'Cyd', 'Dee', 'Eve', 'Fay', 'Gus', 'Hal'];
const FourPlayers = ['Ann', 'Bob', 'Cyd', 'Dee'];
const Repeated = ['Ann', 'Bob', 'ann', 'Dee'];
const WideViewport = { width: 1280, height: 800 };
const Pass = { phone: 'phone', wide: 'wide' } as const;
type Pass = (typeof Pass)[keyof typeof Pass];
export const TraceFile = 'trace.json';
const TextLimit = 1000, ExportLimit = 4000, TabLimit = 40, StallMs = 1000, StallLimit = 3, StepLimit = 200;
const MovesTieMatch = 1, FullTieMatch = 2, BracketMatch = 3, FinalOfEight = 6;
const FirstPlayerDelayMs = 1000, SecondPlayerDelayMs = 6000;
const CardLabel = /^Row \d+, column \d+, /;

interface Open { readonly page: Page; readonly errors: readonly string[] }
interface Extra { readonly [key: string]: unknown }

/** What a person, a keyboard and a screen reader have at this moment, as the page shows it. */
const probe = (page: Page) => page.evaluate(({ labels, cardLabel }) => {
  const shown = (element: Element) => element.getClientRects().length > 0;
  const nameOf = (element: Element) => (element.getAttribute('aria-label') ?? element.textContent ?? '').trim().slice(0, 80);
  const active = document.activeElement;
  const live = [...document.querySelectorAll('[aria-live],[role=status],[role=alert],[role=log]')].map((element) => ({ tag: element.tagName.toLowerCase(), role: element.getAttribute('role'), live: element.getAttribute('aria-live'), text: (element.textContent ?? '').trim().slice(0, 240) }));
  const buttons = [...document.querySelectorAll('button')].filter(shown);
  const next = buttons.filter((item) => labels.includes(item.textContent?.trim() ?? '')).map((item) => {
    const box = item.getBoundingClientRect();
    return { name: item.textContent?.trim(), top: Math.round(box.top), bottom: Math.round(box.bottom), inView: box.top >= 0 && box.bottom <= innerHeight };
  });
  const cards = buttons.filter((item) => new RegExp(cardLabel).test(item.getAttribute('aria-label') ?? '') && !item.hasAttribute('data-eval-main-board'));
  const tables = [...document.querySelectorAll('table')].filter(shown).map((table) => ({ caption: table.caption?.textContent?.trim().slice(0, 80) ?? null, width: table.scrollWidth, room: table.parentElement?.clientWidth ?? null }));
  const line = [...document.querySelectorAll('body *')].find((element) => shown(element) && element.children.length === 0 && /^Now playing:/.test(element.textContent?.trim() ?? ''));
  const style = line === undefined ? null : getComputedStyle(line);
  return {
    focus: active === null || active === document.body ? 'page' : `${active.tagName.toLowerCase()}: ${nameOf(active)}`,
    live, next, tables, scrollY: Math.round(scrollY), overflow: document.documentElement.scrollWidth - document.documentElement.clientWidth,
    cards: cards.length, faceUp: cards.filter((item) => !/, closed$/.test(item.getAttribute('aria-label') ?? '')).length,
    turnLine: style === null || style === undefined ? null : { size: style.fontSize, weight: style.fontWeight },
  };
}, { labels: [Control.startTurn, Control.nextTurn, Control.nextMatch] as string[], cardLabel: CardLabel.source });

class Recorder {
  private readonly steps: string[] = [];
  private readonly folder: string;
  constructor(folder: string) { this.folder = folder; }
  async step(pass: Pass, at: string, open: Open, extra: Extra = {}): Promise<void> {
    await settled(open.page);
    const text = (await visibleText(open.page)).slice(0, TextLimit);
    this.steps.push(JSON.stringify({ step: this.steps.length + 1, pass, at, ...extra, text, ...await probe(open.page), pageErrors: open.errors.length }));
  }
  /** A viewport image, since a page image would show what a player has to scroll to. */
  async image(name: string, page: Page): Promise<void> { await page.screenshot({ path: join(this.folder, `${name}.png`) }); }
  async stopped(pass: Pass, error: unknown, open: Open | undefined): Promise<void> {
    const reason = error instanceof Error ? error.message.split('\n')[0]?.slice(0, 240) : String(error);
    this.steps.push(JSON.stringify({ step: this.steps.length + 1, pass, stopped: reason }));
    if (open !== undefined) await this.image(`${pass}-stopped`, open.page).catch(() => undefined);
  }
  async save(): Promise<void> { await writeFile(join(this.folder, TraceFile), `[\n${this.steps.join(',\n')}\n]\n`); }
}

/** First player wins, except the matches that show how a tie is decided. */
function secondPlan(match: number, first: GameResult): GamePlan {
  if (match === MovesTieMatch) return { known: first.layout, moves: first.moves, delayMs: SecondPlayerDelayMs };
  if (match === FullTieMatch) return { known: first.layout, moves: first.moves };
  return { known: first.layout, moves: first.moves + 1 };
}
const firstPlan = (match: number): GamePlan => match === MovesTieMatch ? { delayMs: FirstPlayerDelayMs } : {};

interface Moments {
  /** Called at each moment of the game, with its match and turn, counted from 0. */
  readonly at: (moment: string, match: number, turn: number) => Promise<void>;
}
/** The driver's playOn, with a place to look at each moment. Whatever this page offers next is what gets pressed. */
async function playThrough(open: Open, moments: Moments): Promise<void> {
  const { page } = open;
  let match = -1, first: GameResult | undefined, stalls = 0;
  for (let step = 0; step < StepLimit; step++) {
    await settled(page);
    if (await offered(page, Control.startTurn)) {
      stalls = 0;
      const turn = first === undefined ? 0 : 1;
      if (turn === 0) match++;
      await moments.at('ready', match, turn);
      if (await offered(page, Control.startTurn)) await press(page, Control.startTurn);
      await moments.at('turn-start', match, turn);
      const result = await playGame(page, first === undefined ? firstPlan(match) : secondPlan(match, first));
      await settled(page);
      await moments.at(turn === 0 ? 'turn-end' : 'match-end', match, turn);
      first = first === undefined ? result : undefined;
    } else if (await offered(page, Control.nextTurn)) { stalls = 0; await press(page, Control.nextTurn); }
    else if (await offered(page, Control.nextMatch)) { stalls = 0; await press(page, Control.nextMatch); }
    else if (await championOf(page) !== null) { await moments.at('champion', match, 1); return; }
    else {
      await page.clock.runFor(StallMs); stalls++;
      if (stalls >= StallLimit) throw new Error('The page offers no next step and no champion.');
    }
  }
  throw new Error('The tournament did not come to an end.');
}

async function bracketImage(recorder: Recorder, page: Page): Promise<void> {
  const before = await page.evaluate(() => scrollY);
  await page.evaluate(() => [...document.querySelectorAll('table')].find((table) => table.getClientRects().length > 0)?.scrollIntoView());
  await recorder.image('phone-bracket', page);
  await page.evaluate((top) => scrollTo(0, top), before);
}

async function phonePass(browser: Browser, html: string, recorder: Recorder): Promise<void> {
  const setup = await session(browser, html, { viewport: PhoneViewport });
  try {
    if (await offered(setup.page, Control.open)) await press(setup.page, Control.open);
    await setPlayers(setup.page, Repeated);
    if (await offered(setup.page, Control.start)) await press(setup.page, Control.start);
    await recorder.step(Pass.phone, 'setup-with-repeated-name', setup);
    await recorder.image('phone-setup-error', setup.page);
  } catch (error) { await recorder.stopped(Pass.phone, error, setup); } finally { await setup.close(); }

  const open = await session(browser, html, { viewport: PhoneViewport });
  try {
    await markMainBoard(open.page);
    if (await offered(open.page, Control.open)) await press(open.page, Control.open);
    await setPlayers(open.page, EightPlayers);
    await press(open.page, Control.start);
    let readyShown = false, turnEndShown = false, bracketShown = false;
    await playThrough(open, { at: async (moment, match, turn) => {
      await recorder.step(Pass.phone, moment, open, { match: match + 1, turn: turn + 1 });
      if (moment === 'ready' && !readyShown) { readyShown = true; await recorder.image('phone-ready', open.page); }
      if (moment === 'ready' && match === BracketMatch && !bracketShown) { bracketShown = true; await bracketImage(recorder, open.page); }
      if (moment === 'turn-start' && match === FinalOfEight && turn === 0) await recorder.image('phone-final', open.page);
      if (moment === 'turn-end' && !turnEndShown) { turnEndShown = true; await recorder.image('phone-turn-end', open.page); }
      if (moment === 'match-end' && match === FullTieMatch) await recorder.image('phone-tied-end', open.page);
      if (moment === 'champion') await recorder.image('phone-champion', open.page);
    } });
    await open.page.reload(); await settled(open.page);
    if (await offered(open.page, Control.open)) await press(open.page, Control.open);
    await recorder.step(Pass.phone, 'after-reload', open);
    await recorder.image('phone-past-list', open.page);
  } catch (error) { await recorder.stopped(Pass.phone, error, open); } finally { await open.close(); }
}

/** Tabs to the named control, or to the first card without a name, and lists every stop on the way. */
async function tabTo(page: Page, name: string | null): Promise<{ readonly stops: readonly string[]; readonly reached: boolean }> {
  const stops: string[] = [];
  for (let key = 0; key < TabLimit; key++) {
    await page.keyboard.press('Tab');
    const focused = await page.evaluate(() => {
      const active = document.activeElement;
      return active === null ? '' : (active.getAttribute('aria-label') ?? active.textContent ?? active.tagName).trim().slice(0, 60);
    });
    stops.push(focused);
    if (name === null ? CardLabel.test(focused) : focused === name) return { stops, reached: true };
  }
  return { stops, reached: false };
}
/** The keys from a finished turn to the next player's first card, without revealing it. */
async function keyboardMeasure(open: Open, recorder: Recorder): Promise<void> {
  const route: { readonly to: string; readonly stops: readonly string[]; readonly reached: boolean }[] = [];
  for (const name of [Control.nextTurn, Control.startTurn]) {
    if (!await offered(open.page, name)) continue;
    const path = await tabTo(open.page, name);
    route.push({ to: name, ...path });
    if (path.reached) await open.page.keyboard.press('Enter'); else await button(open.page, name).click();
    await settled(open.page);
  }
  const card = await tabTo(open.page, null);
  route.push({ to: 'first card', ...card });
  await recorder.step(Pass.wide, 'keyboard-to-first-card', open, { keys: route.reduce((sum, item) => sum + item.stops.length, 0), route });
}

async function widePass(browser: Browser, html: string, recorder: Recorder): Promise<void> {
  const open = await session(browser, html, { viewport: WideViewport });
  try {
    await markMainBoard(open.page);
    if (await offered(open.page, Control.open)) await press(open.page, Control.open);
    await recorder.step(Pass.wide, 'setup', open);
    await recorder.image('wide-setup', open.page);
    if (await playerField(open.page, 1).count() === 0) throw new Error('No Player 1 input to fill.');
    await setPlayers(open.page, FourPlayers);
    await press(open.page, Control.start);
    let exportTaken = false, turnEndShown = false, matchEndShown = false;
    await playThrough(open, { at: async (moment, match, turn) => {
      await recorder.step(Pass.wide, moment, open, { match: match + 1, turn: turn + 1 });
      if (moment === 'turn-start' && match === 1 && turn === 0 && !exportTaken && await offered(open.page, Control.export)) {
        exportTaken = true;
        await recorder.step(Pass.wide, 'export-mid-turn', open, { exported: (await exported(open.page)).slice(0, ExportLimit) });
      }
      if (moment === 'turn-end' && !turnEndShown) { turnEndShown = true; await recorder.image('wide-turn-end', open.page); }
      if (moment === 'match-end' && !matchEndShown) { matchEndShown = true; await recorder.image('wide-match-end', open.page); }
      if (moment === 'champion') await recorder.image('wide-champion', open.page);
    } });
  } catch (error) { await recorder.stopped(Pass.wide, error, open); } finally { await open.close(); }
}

/** Its own session, because pressing on by keyboard takes the game past where the other passes look. */
async function keyboardPass(browser: Browser, html: string, recorder: Recorder): Promise<void> {
  const open = await session(browser, html, { viewport: WideViewport });
  try {
    await markMainBoard(open.page);
    if (await offered(open.page, Control.open)) await press(open.page, Control.open);
    await setPlayers(open.page, FourPlayers);
    await press(open.page, Control.start);
    if (await offered(open.page, Control.startTurn)) await press(open.page, Control.startTurn);
    await playGame(open.page);
    await keyboardMeasure(open, recorder);
  } catch (error) { await recorder.stopped(Pass.wide, error, undefined); } finally { await open.close(); }
}

async function main(): Promise<void> {
  const [html, folder] = process.argv.slice(2);
  if (html === undefined || folder === undefined) throw new Error('Name the built page and the folder to write to.');
  await mkdir(folder, { recursive: true });
  const recorder = new Recorder(folder);
  const browser = await chromium.launch({ headless: true });
  try {
    await phonePass(browser, html, recorder);
    await widePass(browser, html, recorder);
    await keyboardPass(browser, html, recorder);
  } finally {
    await browser.close();
    await recorder.save();
  }
}

await main();
