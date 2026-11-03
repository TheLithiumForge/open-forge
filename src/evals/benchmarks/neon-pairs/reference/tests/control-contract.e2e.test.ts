import { expect, test } from './browser-test.ts';
import { loadTimedBoard, exportedSnapshot } from './game-fixture.ts';

test('literal control labels, limits and invalid statuses match the public contract', async ({
  page,
}) => {
  await loadTimedBoard(page);
  await expect(page.locator('.stats > div').nth(3)).toHaveText('Best: none');
  await expect(page.locator('.stats dd[aria-label]')).toHaveCount(0);
  await expect(
    page.getByRole('combobox', { name: 'Grid size', exact: true }).locator('option'),
  ).toHaveText(['4x4', '6x6', '8x8', '10x10', '12x12', '14x14', '16x16']);
  for (const label of ['New Game', 'Restart'])
    await expect(page.getByRole('button', { name: label, exact: true })).toBeVisible();
  await expect(page.locator('details')).not.toHaveAttribute('open');
  await expect(page.locator('summary')).toHaveText('Reproduce a game');
  await page.locator('summary').click();
  await expect(page.getByLabel('Board code', { exact: true })).toHaveAttribute('maxlength', '32');
  await expect(page.getByLabel('Snapshot JSON', { exact: true })).toHaveAttribute(
    'maxlength',
    '65536',
  );
  for (const label of ['Load board', 'Export snapshot', 'Load snapshot'])
    await expect(page.getByRole('button', { name: label, exact: true })).toBeVisible();
  await expect(page.getByRole('button', { name: /replay/i })).toHaveCount(0);
  await page.locator('summary').click();
  await page.locator('.card').nth(2).click();
  await page.locator('.card').nth(6).click();
  await page.locator('.card').nth(0).click();
  await page.clock.runFor(1200);
  const before = await exportedSnapshot(page);
  expect(before).toMatchObject({
    matched: [2, 6],
    selection: { kind: 'one', index: 0 },
    moves: 1,
    elapsedMs: 1200,
  });
  await page.locator('summary').click();
  const board = page.getByLabel('Board code', { exact: true });
  await board.fill('invalid');
  await page.getByRole('button', { name: 'Load board', exact: true }).click();
  await expect(page.locator('.load-notice')).toHaveText('Invalid board code.');
  await expect(board).toHaveValue('invalid');
  const snapshot = page.getByLabel('Snapshot JSON', { exact: true });
  await snapshot.fill('{');
  await page.getByRole('button', { name: 'Load snapshot', exact: true }).click();
  await expect(page.locator('.load-notice')).toHaveText('Invalid snapshot.');
  await expect(snapshot).toHaveValue('{');
  await page.locator('summary').click();
  expect(await exportedSnapshot(page)).toEqual(before);
  await page.clock.runFor(1000);
  expect((await exportedSnapshot(page)).elapsedMs).toBe(2200);
});

test('draft resets, load focus, paused notice and Resume focus order survive remembered positions', async ({
  page,
}) => {
  await loadTimedBoard(page);
  await page.locator('.card').nth(9).focus();
  await page.locator('summary').click();
  const board = page.getByLabel('Board code', { exact: true });
  for (const label of ['New Game', 'Restart']) {
    await board.fill('stale');
    await page.getByRole('button', { name: label, exact: true }).click();
    await expect(board).not.toHaveValue('stale');
    await expect(page.getByRole('button', { name: label, exact: true })).toBeFocused();
    await expect(page.locator('.card').first()).toHaveAttribute('tabindex', '0');
  }
  await page.locator('.card').nth(9).focus();
  await board.fill('stale');
  await page.getByLabel('Snapshot JSON', { exact: true }).fill(
    JSON.stringify({
      version: 1,
      boardCode: 'np1-4-1',
      matched: [],
      moves: 1,
      elapsedMs: 500,
      started: true,
      selection: { kind: 'closed' },
    }),
  );
  const load = page.getByRole('button', { name: 'Load snapshot', exact: true });
  await load.click();
  await expect(load).toBeFocused();
  await expect(board).toHaveValue('np1-4-1');
  await expect(page.locator('.card').first()).toHaveAttribute('tabindex', '0');
  await expect(page.locator('.load-notice')).toHaveText(
    'Loaded snapshot. Select Resume to continue.',
  );
  const resume = page.getByRole('button', { name: 'Resume', exact: true });
  await expect(resume).toBeVisible();
  await page.locator('.card').nth(3).focus();
  await page.evaluate(() => {
    document.addEventListener('focusin', (event) => {
      if (event.target instanceof Element && event.target.matches('.card'))
        document.body.dataset.resumePresentAtFocus = String(
          [...document.querySelectorAll('button')].some((b) => b.textContent === 'Resume'),
        );
    });
  });
  await resume.click();
  await expect(page.locator('.card').nth(3)).toBeFocused();
  await expect(page.locator('body')).toHaveAttribute('data-resume-present-at-focus', 'true');
  await expect(resume).toHaveCount(0);
});

test('export keeps populated progress running and exposes a static imported win', async ({
  page,
}) => {
  await loadTimedBoard(page);
  await page.locator('.card').nth(2).click();
  await page.locator('.card').nth(6).click();
  await page.locator('.card').nth(0).click();
  await page.clock.runFor(1200);
  const before = await exportedSnapshot(page);
  expect(await exportedSnapshot(page)).toEqual(before);
  await page.clock.runFor(1000);
  expect(await exportedSnapshot(page)).toMatchObject({ ...before, elapsedMs: 2200 });
  await page.locator('summary').click();
  await page.getByLabel('Snapshot JSON', { exact: true }).fill(
    JSON.stringify({
      version: 1,
      boardCode: 'np1-4-1',
      matched: Array.from({ length: 16 }, (_, i) => i),
      moves: 8,
      elapsedMs: 3000,
      started: true,
      selection: { kind: 'closed' },
    }),
  );
  await page.getByRole('button', { name: 'Load snapshot', exact: true }).click();
  await expect(page.locator('.victory')).toBeVisible();
  await expect(page.locator('.victory')).toHaveText(
    'Every pair found in 8 moves and 0:03. Well played.',
  );
  await expect(page.locator('.victory')).not.toHaveClass(/celebrate/);
  await expect(page.locator('.stats dd').nth(3)).toHaveText('none');
});
