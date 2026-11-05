import assert from 'node:assert/strict';
import { settled } from './browser.ts';
import { markMainBoard, playGame } from './board.ts';
import type { Locator, Page } from 'playwright';
import type { GamePlan, GameResult } from './board.ts';

/** The labels the request gives every scenario. The driver uses nothing else, so one rule's check doesn't hang on another rule. */
export const Control = { open: 'Tournament', newGame: 'New Game', players: 'Players', start: 'Start tournament', startTurn: 'Start turn', nextTurn: 'Next turn', nextMatch: 'Next match', leave: 'Leave tournament', confirmLeave: 'Leave', keepPlaying: 'Keep playing', back: 'Back to game', export: 'Export tournament', json: 'Tournament JSON', load: 'Load tournament', past: 'Past tournaments' } as const;
export const Four = ['Ann', 'Bob', 'Cyd', 'Dee'] as const;
export const Eight = [...Four, 'Eve', 'Fay', 'Gus', 'Hal'] as const;
export const PreferencesKey = 'neon-pairs.preferences.v1';
const NowPlaying = /Now playing:\s*([^\n]+)/, ChampionText = /Champion:\s*([^\n]+)/, RoundText = /Round:\s*([^\n]+)/;
const StallLimit = 3, StallMs = 1000, StepLimit = 400;

export interface Turn extends GameResult { readonly name: string }
export interface Match { readonly turns: Turn[]; readonly round: string | null }
export interface Run { readonly matches: Match[]; readonly champion: string | null; readonly steps: string[] }
export const Stage = { turn: 'turn', played: 'played', end: 'end' } as const;
export type Stage = (typeof Stage)[keyof typeof Stage];
export interface PlanContext { readonly match: number; readonly turn: number; readonly name: string; readonly first: Turn | undefined }
export interface RunOptions {
  /** How the player about to go should play. The second player of a match replays the known deal by default. */
  readonly plan?: (context: PlanContext) => GamePlan;
  /** Returns as this match is about to begin, leaving its first player on screen. */
  readonly stopAtMatch?: number;
  readonly visit?: (stage: Stage) => Promise<void>;
}

export const button = (page: Page, name: string): Locator => page.getByRole('button', { name, exact: true });
export const offered = async (page: Page, name: string): Promise<boolean> => await button(page, name).count() === 1 && await button(page, name).isVisible();
export const visibleText = (page: Page): Promise<string> => page.evaluate(() => document.body.innerText);
/** Every text node as written, so whitespace the browser collapses on screen still shows, and text in closed disclosures too. */
export const writtenText = (page: Page): Promise<string> => page.evaluate(() => document.body.textContent ?? '');
export const playersOf = (run: Run, match: number): string[] => run.matches[match]?.turns.map((turn) => turn.name) ?? [];
export const sameCards = (left: readonly string[] | undefined, right: readonly string[] | undefined): boolean => left !== undefined && right !== undefined && left.join('|') === right.join('|');

export async function press(page: Page, name: string): Promise<void> {
  assert.ok(await offered(page, name), `${name} is offered once.`);
  await button(page, name).click(); await settled(page);
}
/** By role, since another element may share a control's label or the control may be named some other way. */
export const playersSelect = (page: Page): Locator => page.getByRole('combobox', { name: Control.players, exact: true });
export const playerField = (page: Page, number: number): Locator => page.getByRole('textbox', { name: `Player ${number}`, exact: true });
const jsonBox = (page: Page): Locator => page.getByRole('textbox', { name: Control.json, exact: true });

export async function setPlayers(page: Page, names: readonly string[]): Promise<void> {
  const select = playersSelect(page);
  assert.equal(await select.count(), 1, 'A Players select exists.');
  await select.selectOption(String(names.length)); await settled(page);
  for (const [index, name] of names.entries()) {
    const field = playerField(page, index + 1);
    assert.equal(await field.count(), 1, `A Player ${index + 1} input exists.`);
    await field.fill(name);
  }
  await settled(page);
}
/** Leaving may return to the Tournament screen or to the game, so a new tournament starts from whichever is showing. */
export async function begin(page: Page, names: readonly string[]): Promise<void> {
  await markMainBoard(page);
  if (await offered(page, Control.open)) await press(page, Control.open);
  await setPlayers(page, names); await press(page, Control.start);
}
/** The request says leaving stops a tournament, not where it goes next, so this asks only that nothing of it is still going. */
export async function tournamentStopped(page: Page): Promise<void> {
  assert.equal(await offered(page, Control.startTurn), false, 'No turn is offered any more.');
  assert.equal(await offered(page, Control.leave), false, 'There is nothing left to leave.');
}
/** The name after a label, without the full stop a page may end its sentence with. */
const labelled = (pattern: RegExp, text: string): string | null => pattern.exec(text)?.[1]?.trim().replace(/\.$/, '') ?? null;
export async function nowPlaying(page: Page): Promise<string | null> { return labelled(NowPlaying, await visibleText(page)); }
export async function championOf(page: Page): Promise<string | null> { return labelled(ChampionText, await visibleText(page)); }

/** Plays from the current screen until the champion shows, going through whichever next step the page offers. */
export async function playOn(page: Page, options: RunOptions = {}, soFar: Run = { matches: [], champion: null, steps: [] }): Promise<Run> {
  const { matches, steps } = soFar;
  let stalls = 0;
  for (let step = 0; step < StepLimit; step++) {
    await settled(page);
    if (await offered(page, Control.startTurn)) {
      stalls = 0;
      const waiting = await nowPlaying(page);
      let match = matches.at(-1);
      if (match === undefined || match.turns.length === 2) {
        if (options.stopAtMatch === matches.length) return { matches, champion: null, steps };
        match = { turns: [], round: RoundText.exec(await visibleText(page))?.[1]?.trim() ?? null };
        matches.push(match);
      }
      await options.visit?.(Stage.turn);
      await press(page, Control.startTurn); steps.push(Control.startTurn);
      // A page may name the player only once the turn starts.
      const name = waiting ?? await nowPlaying(page);
      assert.ok(name !== null, 'The page says whose turn it is.');
      const first = match.turns[0];
      const plan = options.plan?.({ match: matches.length - 1, turn: match.turns.length, name, first }) ?? (first === undefined ? {} : { known: first.layout });
      match.turns.push({ name, ...await playGame(page, plan) });
      await settled(page);
      await options.visit?.(Stage.played);
    } else if (await offered(page, Control.nextTurn)) { stalls = 0; await press(page, Control.nextTurn); steps.push(Control.nextTurn); }
    else if (await offered(page, Control.nextMatch)) { stalls = 0; await press(page, Control.nextMatch); steps.push(Control.nextMatch); }
    else {
      const champion = await championOf(page);
      if (champion !== null) { await options.visit?.(Stage.end); return { matches, champion, steps }; }
      // A page may hold its next step behind a short timer.
      await page.clock.runFor(StallMs); stalls++;
      assert.ok(stalls < StallLimit, 'The page offers a next step.');
    }
  }
  assert.fail('The tournament came to an end.');
}
export async function tournament(page: Page, names: readonly string[], options: RunOptions = {}): Promise<Run> {
  await begin(page, names);
  return playOn(page, options);
}
/** A finished match's winner, known by who plays on or who ends as champion. */
export function winnerOf(run: Run, match: number): string | undefined {
  const later = new Set(run.matches.slice(match + 1).flatMap((item) => item.turns.map((turn) => turn.name)));
  return playersOf(run, match).find((name) => later.has(name) || name === run.champion);
}
export async function stored(page: Page): Promise<Record<string, string>> { return page.evaluate(() => ({ ...localStorage })); }
/** The stored entry that holds this text, apart from the game's own preferences. */
export async function entryHolding(page: Page, text: string): Promise<readonly [string, string]> {
  const entry = Object.entries(await stored(page)).find(([key, value]) => key !== PreferencesKey && value.includes(text));
  assert.ok(entry !== undefined, 'The finished tournament is in browser storage.');
  return entry;
}
export async function exported(page: Page): Promise<string> {
  await press(page, Control.export);
  const box = jsonBox(page);
  assert.equal(await box.count(), 1, 'A Tournament JSON box exists.');
  return box.inputValue();
}
export async function loaded(page: Page, text: string): Promise<void> {
  await markMainBoard(page);
  const box = jsonBox(page);
  assert.equal(await box.count(), 1, 'A Tournament JSON box exists.');
  await box.fill(text); await press(page, Control.load);
}
