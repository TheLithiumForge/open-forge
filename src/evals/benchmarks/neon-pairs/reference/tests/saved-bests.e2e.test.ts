import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { EXAMPLE_CODE, EXAMPLE_PAIRS } from '../src/game/game.fixture.ts';
import { DealControl, ReproductionControl } from '../src/game/controls/control-labels.ts';
import { DEFAULT_SIZE, PREFERENCES_KEY, PREFERENCES_VERSION } from '../src/game/session/storage.ts';
import { MISMATCH_MS } from '../src/game/game.ts';
import { SelectionKind } from '../src/game/selection.ts';
import { SNAPSHOT_VERSION } from '../src/game/snapshot.ts';
import { StatusLabel } from '../src/game/status/status-labels.ts';
import { finishBoard, loadFixedBoard, loadTimedBoard } from './game-fixture.ts';
import { uiSnapshot } from './ui-snapshot.ts';
import type { Page } from '@playwright/test';

const PERFECT = EXAMPLE_PAIRS.length;

function shownBest(page: Page) {
  return page
    .locator('.stats > div')
    .filter({ has: page.getByText(StatusLabel.best, { exact: true }) })
    .locator('dd');
}

// Writing from the page itself fires no storage event there, as when another tab's event is late.
async function saveBest(page: Page, moves: number): Promise<void> {
  const saved = JSON.stringify({
    schemaVersion: PREFERENCES_VERSION,
    size: DEFAULT_SIZE,
    bestMoves: { [DEFAULT_SIZE]: moves },
  });
  await page.evaluate(([key, value]) => localStorage.setItem(key, value), [
    PREFERENCES_KEY,
    saved,
  ] as const);
}

function savedBest(page: Page) {
  return page.evaluate((key) => localStorage.getItem(key), PREFERENCES_KEY);
}

test("an idle tab shows another tab's best and keeps it through deals and sizes", async ({
  page,
  context,
  gameURL,
}) => {
  const idle = await context.newPage();
  await idle.goto(gameURL);
  await loadFixedBoard(page, gameURL);
  await finishBoard(page);
  const best = shownBest(idle);
  await expect(best, "R10 an idle tab shows another tab's best").toHaveText(String(PERFECT));
  await idle.getByRole('button', { name: DealControl.newGame, exact: true }).click();
  await expect(best).toHaveText(String(PERFECT));
  await idle.getByRole('button', { name: DealControl.restart, exact: true }).click();
  await expect(best).toHaveText(String(PERFECT));
  await idle.getByLabel(DealControl.size, { exact: true }).selectOption('6');
  await expect(best).toHaveText((await uiSnapshot()).bestNone);
  await idle.getByLabel(DealControl.size, { exact: true }).selectOption(String(DEFAULT_SIZE));
  await expect(best).toHaveText(String(PERFECT));
  await idle.close();
});

test('a win shows the saved best when another tab saved a better one first', async ({
  page,
  gameURL,
}) => {
  await loadTimedBoard(page, gameURL);
  await saveBest(page, PERFECT);
  await page.locator('.card').nth(0).click();
  await page.locator('.card').nth(1).click();
  await page.clock.runFor(MISMATCH_MS);
  await finishBoard(page);
  await expect(shownBest(page), 'R10 the shown best matches the saved best').toHaveText(
    String(PERFECT),
  );
});

for (const [saved, imported] of [
  [PERFECT, PERFECT + 2],
  [PERFECT + 4, PERFECT],
] as const)
  test(`a completed import in ${imported} moves leaves a saved best of ${saved} alone`, async ({
    page,
    gameURL,
  }) => {
    await page.goto(gameURL);
    await saveBest(page, saved);
    await page.reload();
    const best = shownBest(page);
    await expect(best).toHaveText(String(saved));
    const before = await savedBest(page);
    await best.evaluate((element) => {
      const changes: string[] = [];
      element.setAttribute('data-changes', '[]');
      new MutationObserver(() => {
        changes.push(element.textContent ?? '');
        element.setAttribute('data-changes', JSON.stringify(changes));
      }).observe(element, { childList: true, characterData: true, subtree: true });
    });
    const completed = {
      version: SNAPSHOT_VERSION,
      boardCode: EXAMPLE_CODE,
      matched: EXAMPLE_PAIRS.flat(),
      selection: { kind: SelectionKind.closed },
      moves: imported,
      elapsedMs: 0,
      started: true,
    };
    await page.locator('summary').click();
    await page
      .getByLabel(ReproductionControl.snapshot, { exact: true })
      .fill(JSON.stringify(completed));
    await page.getByRole('button', { name: ReproductionControl.loadSnapshot, exact: true }).click();
    await expect(page.locator('.card.matched')).toHaveCount(PERFECT * 2);
    const changes: unknown = JSON.parse((await best.getAttribute('data-changes')) ?? '[]');
    expect(changes, 'C12 import leaves the shown best alone').toEqual([]);
    expect(await savedBest(page), 'C12 import leaves the saved best alone').toBe(before);
  });
