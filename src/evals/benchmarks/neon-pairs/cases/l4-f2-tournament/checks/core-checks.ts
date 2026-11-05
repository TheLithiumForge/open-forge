import assert from 'node:assert/strict';
import { boardCards, CardState } from './board.ts';
import { settled } from './browser.ts';
import { begin, button, championOf, Control, Eight, exported, Four, loaded, nowPlaying, offered, playOn, press, sameCards, tournament, tournamentStopped, visibleText, winnerOf, writtenText } from './tournament.ts';
import type { Page } from 'playwright';
import type { ChangeCheck } from './check.types.ts';

const ExtraMoves = 2;

async function leave(page: Page): Promise<void> {
  await press(page, Control.leave);
  // The request only asks for a way to stop early. How it confirms is a rule, checked on its own.
  if (await offered(page, Control.confirmLeave)) await press(page, Control.confirmLeave);
}
async function backAtTheGame(page: Page): Promise<void> {
  assert.ok(await offered(page, Control.newGame), 'New Game is back.');
  assert.ok(await offered(page, Control.open), 'Tournament is back.');
  assert.equal(await offered(page, Control.startTurn), false, 'The tournament has stopped.');
}

export const coreChecks: readonly ChangeCheck[] = [
  { id: 'core-01', description: 'A Tournament button next to New Game opens a screen with a Players select of 4 and 8 and the matching Player inputs.', action: async (page) => {
    const near = await button(page, Control.newGame).evaluate((element, name) => [...(element.parentElement?.querySelectorAll('button') ?? [])].some((item) => item.textContent?.trim() === name), Control.open);
    assert.ok(near, 'Tournament sits beside New Game.');
    await press(page, Control.open);
    const select = page.getByLabel(Control.players, { exact: true });
    assert.equal(await select.count(), 1, 'A Players select exists.');
    assert.deepEqual((await select.locator('option').evaluateAll((options) => options.map((option) => (option as HTMLOptionElement).value))).sort(), ['4', '8']);
    await select.selectOption('8'); await settled(page);
    assert.equal(await page.getByLabel('Player 8', { exact: true }).count(), 1, 'Eight players have eight inputs.');
    await select.selectOption('4'); await settled(page);
    assert.equal(await page.getByLabel('Player 4', { exact: true }).count(), 1, 'Four players have four inputs.');
    assert.equal(await page.getByLabel('Player 5', { exact: true }).count(), 0, 'Four players have no fifth input.');
  } },
  { id: 'core-02', description: 'Start tournament names an entered player as "Now playing: NAME", and Start turn begins a game on a closed board.', action: async (page) => {
    await begin(page, Four);
    const name = await nowPlaying(page);
    assert.ok(name !== null && Four.some((player) => player === name), 'An entered player is up.');
    await press(page, Control.startTurn);
    const cards = await boardCards(page);
    assert.ok(cards.length > 0 && cards.every((card) => card.state === CardState.closed), 'The board starts closed.');
  } },
  { id: 'core-03', description: 'A game ending offers Next turn, and a match ending offers Next match.', action: async (page) => {
    const { steps } = await tournament(page, Four);
    assert.deepEqual(steps.slice(0, 4), [Control.startTurn, Control.nextTurn, Control.startTurn, Control.nextMatch]);
  } },
  { id: 'core-04', description: 'Four players play 3 matches to "Champion: NAME", and Back to game returns to the game.', action: async (page) => {
    const run = await tournament(page, Four);
    assert.equal(run.matches.length, 3);
    assert.ok(Four.some((player) => player === run.champion), 'The champion is a player.');
    await press(page, Control.back); await backAtTheGame(page);
  } },
  { id: 'core-05', description: 'Eight players play 7 matches to "Champion: NAME".', action: async (page) => {
    const run = await tournament(page, Eight);
    assert.equal(run.matches.length, 7);
    assert.ok(Eight.some((player) => player === run.champion), 'The champion is a player.');
  } },
  { id: 'core-06', description: 'Both players of a match play the same deal, one after the other.', action: async (page) => {
    const run = await tournament(page, Four);
    for (const match of run.matches) {
      assert.equal(new Set(match.turns.map((turn) => turn.name)).size, 2, 'Two different players take turns.');
      assert.ok(sameCards(match.turns[0]?.layout, match.turns[1]?.layout), 'Both play the same cards.');
    }
  } },
  { id: 'core-07', description: 'Fewer moves wins the match, whichever player went first.', action: async (page) => {
    const run = await tournament(page, Four, { plan: ({ match, first }) => first === undefined ? {} : { known: first.layout, ...(match === 1 ? { moves: first.moves + ExtraMoves } : {}) } });
    assert.equal(winnerOf(run, 0), run.matches[0]?.turns[1]?.name, 'The second player took fewer moves.');
    assert.equal(winnerOf(run, 1), run.matches[1]?.turns[0]?.name, 'The first player took fewer moves.');
    assert.equal(run.champion, run.matches[2]?.turns[1]?.name, 'The second player took fewer moves.');
  } },
  { id: 'core-08', description: 'The bracket shows each player, their moves and the matches at the end.', action: async (page) => {
    const run = await tournament(page, Four);
    const text = await visibleText(page);
    for (const match of run.matches) for (const turn of match.turns) {
      assert.ok(text.includes(turn.name), 'The bracket names each player.');
      assert.ok(text.includes(String(turn.moves)), 'The bracket shows each player\'s moves.');
    }
  } },
  { id: 'core-09', description: 'A finished tournament stays in Past tournaments on the Tournament screen after reload.', action: async (page) => {
    const run = await tournament(page, Four);
    await press(page, Control.back); await page.reload(); await settled(page);
    await press(page, Control.open);
    const text = await writtenText(page);
    assert.ok(text.includes(Control.past), 'The list has its title.');
    assert.ok(run.champion !== null && text.includes(run.champion), 'The list holds the finished tournament.');
  } },
  { id: 'core-10', description: 'Export tournament puts JSON in the box, and Load tournament reads it back after reload, mid-tournament and when finished.', action: async (page) => {
    await begin(page, Four);
    const run = await playOn(page, { stopAtMatch: 1 });
    const name = await nowPlaying(page), text = await exported(page);
    JSON.parse(text);
    await page.reload(); await settled(page); await press(page, Control.open);
    await loaded(page, text);
    assert.equal(await nowPlaying(page), name, 'The same player is up.');
    const finished = await playOn(page, {}, run);
    assert.equal(finished.matches.length, 3, 'The tournament carries on from where it was.');
    const end = await exported(page);
    await page.reload(); await settled(page); await press(page, Control.open);
    await loaded(page, end);
    assert.equal(await championOf(page), finished.champion, 'The champion comes back.');
  } },
  { id: 'core-11', description: 'Leave tournament stops it early, before a turn and during one.', action: async (page) => {
    await begin(page, Four);
    await leave(page); await tournamentStopped(page);
    await begin(page, Four);
    await press(page, Control.startTurn);
    await leave(page); await tournamentStopped(page);
  } },
  { id: 'core-12', description: 'The ordinary game still plays after a tournament.', action: async (page) => {
    await tournament(page, Four);
    await press(page, Control.back);
    const closed = (await boardCards(page)).length;
    assert.ok(closed > 0, 'The game\'s board is back.');
    await press(page, Control.newGame);
    const cards = page.locator('.card');
    await cards.nth(0).click(); await cards.nth(1).click(); await settled(page);
    assert.match((await visibleText(page)).replace(/\s+/g, ' '), /Moves:? 1\b/, 'A move counts.');
  } },
];
