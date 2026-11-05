import assert from 'node:assert/strict';
import { PhoneViewport } from './browser.ts';
import { begin, button, Control, Four, nowPlaying, playOn, press, setPlayers, tournament, tournamentStopped, visibleText, winnerOf } from './tournament.ts';
import type { Page } from 'playwright';
import type { ChangeCheck } from './check.types.ts';

const LiveRegions = '[aria-live]:not([aria-live="off"]), [role="status"], [role="alert"], [role="log"], output';
const MinimumTarget = 44;
const Controls = 'button, select, input, textarea, summary, a[href]';
const Winner = '(winner)';

async function liveRegions(page: Page): Promise<string[]> {
  return page.locator(LiveRegions).evaluateAll((regions) => regions.map((region) => region.textContent ?? ''));
}
async function focusIsOnHeading(page: Page): Promise<void> {
  const focused = await page.evaluate(() => ({ tag: document.activeElement?.tagName, tabIndex: document.activeElement?.getAttribute('tabindex') }));
  assert.deepEqual(focused, { tag: 'H2', tabIndex: '-1' }, 'Focus is on the screen\'s heading.');
}
async function smallControls(page: Page): Promise<string[]> {
  return page.evaluate(({ selector, minimum }) => [...document.querySelectorAll(selector)].flatMap((element) => {
    if (/^Row \d+, column \d+, /.test(element.getAttribute('aria-label') ?? '')) return [];
    const box = element.getBoundingClientRect();
    if (box.width === 0 || box.height === 0) return [];
    return box.width < minimum || box.height < minimum ? [`${element.textContent?.trim() || element.getAttribute('aria-label') || element.id || element.tagName} ${Math.round(box.width)}x${Math.round(box.height)}`] : [];
  }), { selector: Controls, minimum: MinimumTarget });
}
async function moving(page: Page): Promise<string[]> {
  return page.evaluate(() => {
    const seconds = (text: string) => Math.max(...text.split(',').map((part) => part.trim().endsWith('ms') ? parseFloat(part) / 1000 : parseFloat(part)));
    return [...document.querySelectorAll('body, body *')].flatMap((element) => {
      const style = getComputedStyle(element);
      const animated = style.animationName !== 'none' && seconds(style.animationDuration) > 0.001;
      const transitioned = style.transitionProperty !== 'none' && seconds(style.transitionDuration) > 0.001;
      return animated || transitioned ? [element.tagName + (element.className ? `.${String(element.className)}` : '')] : [];
    });
  });
}

export const accessChecks: readonly ChangeCheck[] = [
  { id: 'A01', description: 'A01: the bracket is a table with a caption and th scope="col" header cells.', action: async (page) => {
    const run = await tournament(page, Four);
    const tables = page.locator('table').filter({ hasText: run.champion ?? '' });
    assert.ok(await tables.count() > 0, 'A table holds the bracket.');
    const shape = await tables.first().evaluate((table) => ({ caption: table.querySelector('caption')?.textContent?.trim() ?? '', headers: [...table.querySelectorAll('thead th, tr:first-child th')].map((cell) => cell.getAttribute('scope')) }));
    assert.ok(shape.caption !== '', 'The table has a caption.');
    assert.ok(shape.headers.length > 0 && shape.headers.every((scope) => scope === 'col'), 'Header cells have scope="col".');
  } },
  { id: 'A02', description: 'A02: the page has exactly one live region, and "Now playing: NAME" goes through it.', action: async (page) => {
    await press(page, Control.open);
    await setPlayers(page, ['x', 'x', 'x', 'x']); await press(page, Control.start);
    assert.equal((await liveRegions(page)).length, 1, 'An error adds no live region.');
    await setPlayers(page, Four); await press(page, Control.start);
    const regions = await liveRegions(page), name = await nowPlaying(page);
    assert.equal(regions.length, 1, 'One live region exists during play.');
    assert.ok(regions[0]?.includes(`Now playing: ${name}`), 'The turn is announced there.');
  } },
  { id: 'A03', description: 'A03: when the feature replaces the main area, focus moves to that screen\'s h2, which has tabindex="-1".', action: async (page) => {
    await press(page, Control.open); await focusIsOnHeading(page);
    await setPlayers(page, Four); await press(page, Control.start); await focusIsOnHeading(page);
  } },
  { id: 'A04', description: 'A04: leaving unfinished work asks in a native dialog with Keep playing, focused first, and Leave, and Escape keeps playing.', action: async (page) => {
    await begin(page, Four);
    const name = await nowPlaying(page);
    await press(page, Control.leave);
    const dialog = page.locator('dialog[open]');
    assert.equal(await dialog.count(), 1, 'A native dialog opens.');
    assert.deepEqual((await dialog.getByRole('button').allTextContents()).map((text) => text.trim()).sort(), [Control.confirmLeave, Control.keepPlaying].sort());
    assert.equal(await page.evaluate(() => document.activeElement?.textContent?.trim()), Control.keepPlaying, 'Keep playing takes focus.');
    await page.keyboard.press('Escape');
    assert.equal(await dialog.count(), 0, 'Escape closes the dialog.');
    assert.equal(await nowPlaying(page), name, 'Escape keeps playing.');
    await press(page, Control.leave);
    await button(page, Control.keepPlaying).click();
    assert.equal(await nowPlaying(page), name, 'Keep playing keeps playing.');
    await press(page, Control.leave); await press(page, Control.confirmLeave);
    await tournamentStopped(page);
  } },
  { id: 'A05', session: { viewport: PhoneViewport }, description: 'A05: every control is at least 44 by 44 CSS pixels at a 390 pixel wide viewport, on every Tournament screen.', action: async (page) => {
    const small: string[] = [];
    const look = async () => { small.push(...await smallControls(page)); };
    await press(page, Control.open); await look();
    await setPlayers(page, Four); await press(page, Control.start);
    await playOn(page, { visit: look });
    assert.deepEqual([...new Set(small)], []);
  } },
  { id: 'V01', session: { reducedMotion: true }, description: 'V01: under reduced motion nothing on a Tournament screen animates or transitions.', action: async (page) => {
    const moved: string[] = [];
    const look = async () => { moved.push(...await moving(page)); };
    await press(page, Control.open); await look();
    await setPlayers(page, Four); await press(page, Control.start);
    await playOn(page, { visit: look });
    assert.deepEqual([...new Set(moved)], []);
  } },
  { id: 'V02', description: 'V02: state is never color alone, so a winner is marked with the text "(winner)" after the name.', action: async (page) => {
    const run = await tournament(page, Four);
    const text = (await visibleText(page)).replace(/\s+/g, ' ');
    for (const match of run.matches.keys()) assert.ok(text.includes(`${winnerOf(run, match) ?? ''} ${Winner}`) || text.includes(`${winnerOf(run, match) ?? ''}${Winner}`), `Match ${match + 1} marks its winner.`);
  } },
];
