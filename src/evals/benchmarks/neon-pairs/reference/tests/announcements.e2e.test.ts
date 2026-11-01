import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { EXAMPLE_CODE, EXAMPLE_PAIRS } from '../src/game/game.fixture.ts';
import { ReproductionControl } from '../src/game/controls/control-labels.ts';
import { MISMATCH_MS } from '../src/game/game.ts';
import { SelectionKind } from '../src/game/selection.ts';
import { SessionNotice } from '../src/game/session/session.ts';
import { SNAPSHOT_VERSION } from '../src/game/snapshot.ts';
import { loadTimedBoard } from './game-fixture.ts';
import { uiSnapshot } from './ui-snapshot.ts';
import type { Locator, Page } from '@playwright/test';

// Five columns isn't a supported board size.
const UNSUPPORTED_CODE = 'np1-5-1';

// Each observer callback is one change a screen reader can announce.
async function recordResults(region: Locator): Promise<() => Promise<unknown>> {
  await region.evaluate((element) => {
    const results: string[] = [];
    element.setAttribute('data-result-history', '[]');
    new MutationObserver(() => {
      results.push(element.textContent ?? '');
      element.setAttribute('data-result-history', JSON.stringify(results));
    }).observe(element, { childList: true, subtree: true, characterData: true });
  });
  return async () => JSON.parse((await region.getAttribute('data-result-history')) ?? '[]');
}

test('each mismatch is announced once while clocks, focus and closing stay silent', async ({
  page,
}) => {
  await loadTimedBoard(page);
  const region = page.getByRole('status');
  await expect(page.locator('[aria-live="polite"]')).toHaveCount(1);
  await expect(region).toHaveAttribute('aria-atomic', 'true');
  const results = await recordResults(region);
  await page.locator('.card').nth(0).click();
  await page.locator('.card').nth(1).click();
  await page.keyboard.press('ArrowRight');
  await page.clock.runFor(MISMATCH_MS);
  const expected = await uiSnapshot();
  expect(await results()).toEqual([expected.mismatch]);
  await page.locator('.card').nth(0).click();
  await page.locator('.card').nth(1).click();
  await page.clock.runFor(MISMATCH_MS);
  expect(await results()).toEqual([expected.mismatch, expected.mismatch]);
});

function loadSnapshot(page: Page, text: string): () => Promise<void> {
  return async () => {
    await page.getByLabel(ReproductionControl.snapshot, { exact: true }).fill(text);
    await page.getByRole('button', { name: ReproductionControl.loadSnapshot, exact: true }).click();
  };
}

test('every load kind updates the one live region once per event and shows its status beside the reproduction panel', async ({
  page,
}) => {
  await loadTimedBoard(page);
  const region = page.getByRole('status');
  const results = await recordResults(region);
  const unfinished = {
    version: SNAPSHOT_VERSION,
    boardCode: EXAMPLE_CODE,
    matched: [],
    selection: { kind: SelectionKind.closed },
    moves: 0,
    elapsedMs: 0,
    started: false,
  };
  const completed = {
    ...unfinished,
    matched: EXAMPLE_PAIRS.flat(),
    moves: EXAMPLE_PAIRS.length,
    started: true,
  };
  const loads = [
    [SessionNotice.loaded, loadSnapshot(page, JSON.stringify(unfinished))],
    [SessionNotice.loadedCompleted, loadSnapshot(page, JSON.stringify(completed))],
    [
      SessionNotice.invalidBoard,
      async () => {
        await page.getByLabel(ReproductionControl.board, { exact: true }).fill(UNSUPPORTED_CODE);
        await page
          .getByRole('button', { name: ReproductionControl.loadBoard, exact: true })
          .click();
      },
    ],
    [SessionNotice.invalidSnapshot, loadSnapshot(page, '{')],
  ] as const;
  await page.locator('summary').click();
  const expected: string[] = [];
  for (const [notice, load] of loads)
    for (let repeat = 0; repeat < 2; repeat += 1) {
      await load();
      expected.push(notice);
      expect(await results(), 'I16 one live-region change per load').toEqual(expected);
      await expect(region).toHaveText(notice);
      await expect(page.locator('details + .status-row .load-notice')).toHaveText(notice);
    }
});
