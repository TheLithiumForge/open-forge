import { expect, test } from './browser-test.ts';
import { EXAMPLE_CODE } from '../src/game/game.fixture.ts';
import { DealControl, ReproductionControl } from '../src/game/controls/control-labels.ts';
import { GRID_SIZES } from '../src/game/deck.ts';
import { SessionNotice } from '../src/game/session/session.ts';
import { loadTimedBoard, exportedSnapshot, GAME_URL } from './game-fixture.ts';

// The published input limits, kept apart from the components that set them.
const BOARD_CODE_MAX_CHARACTERS = 32;
const SNAPSHOT_MAX_CHARACTERS = 65_536;

test('native reproduction controls keep drafts, limits, exact errors and action focus', async ({
  page,
}) => {
  await page.goto(GAME_URL);
  await expect(page.locator('details')).not.toHaveAttribute('open');
  await expect(page.locator('summary')).toHaveText(ReproductionControl.summary);
  const options = await page
    .getByLabel(DealControl.size, { exact: true })
    .locator('option')
    .evaluateAll((items) =>
      items.map((item) => {
        if (!(item instanceof HTMLOptionElement)) throw new Error('Expected a native option.');
        return { value: item.value, text: item.textContent };
      }),
    );
  expect(options).toEqual(
    GRID_SIZES.map((size) => ({ value: String(size), text: `${size}x${size}` })),
  );
  await loadTimedBoard(page);
  await page.locator('summary').click();
  const code = page.getByLabel(ReproductionControl.board, { exact: true });
  const field = page.getByLabel(ReproductionControl.snapshot, { exact: true });
  await expect(code).toHaveAttribute('maxlength', String(BOARD_CODE_MAX_CHARACTERS));
  await expect(field).toHaveAttribute('maxlength', String(SNAPSHOT_MAX_CHARACTERS));
  await code.fill('draft');
  await page.locator('.card').first().click();
  expect(await code.inputValue()).toBe('draft');
  const load = page.getByRole('button', { name: ReproductionControl.loadBoard, exact: true });
  await load.click();
  await expect(load).toBeFocused();
  await expect(page.locator('.load-notice')).toHaveText(SessionNotice.invalidBoard);
  await field.fill('{');
  const importButton = page.getByRole('button', {
    name: ReproductionControl.loadSnapshot,
    exact: true,
  });
  await importButton.click();
  await expect(importButton).toBeFocused();
  await expect(page.locator('.load-notice')).toHaveText(SessionNotice.invalidSnapshot);
  const exportButton = page.getByRole('button', {
    name: ReproductionControl.exportSnapshot,
    exact: true,
  });
  await exportButton.click();
  await expect(exportButton).toBeFocused();
  const text = await field.inputValue();
  expect(text).toContain('\n');
  await field.selectText();
  expect(
    await field.evaluate((element) => {
      if (!(element instanceof HTMLTextAreaElement)) throw new Error('Expected a textarea.');
      return element.selectionEnd - element.selectionStart;
    }),
  ).toBe(text.length);
  await code.fill(EXAMPLE_CODE);
  await load.click();
  await expect(load).toBeFocused();
  await page.locator('summary').click();
  expect(await exportedSnapshot(page)).toMatchObject({ moves: 0, elapsedMs: 0, started: false });
});
