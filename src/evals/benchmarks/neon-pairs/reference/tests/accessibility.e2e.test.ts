import { readFile, writeFile } from 'node:fs/promises';
import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { EXAMPLE_PAIRS } from '../src/game/game.fixture.ts';
import { accessibilityTree, accessibilityNode, moveKeyboardTo } from './accessibility-tree.ts';
import { accessibilityFixture, playwrightVersion } from './accessibility-fixture.ts';
import { loadTimedBoard } from './game-fixture.ts';

test('a complete keyboard journey exposes names, states and each result in Chromium accessibility trees', async ({
  page,
  context,
  browser,
}, artifacts) => {
  const expected = await accessibilityFixture();
  await loadTimedBoard(page);
  const session = await context.newCDPSession(page);
  await session.send('Accessibility.enable');
  const captures: { stage: string; tree: Awaited<ReturnType<typeof accessibilityTree>> }[] = [];
  async function capture(stage: string) {
    const tree = await accessibilityTree(session);
    captures.push({ stage, tree });
    return tree;
  }
  await page.getByRole('combobox', { name: expected.controls.size, exact: true }).focus();
  const controls = await capture('controls');
  expect(accessibilityNode(controls, 'combobox', expected.controls.size).properties.focused).toBe(
    true,
  );
  accessibilityNode(controls, 'button', expected.controls.newGame);
  await page.keyboard.press('Tab');
  expect(
    accessibilityNode(await capture('new-game-control'), 'button', expected.controls.newGame)
      .properties.focused,
  ).toBe(true);
  await page.keyboard.press('Tab');
  await page.keyboard.press('Tab');
  const closed = await capture('board-entry');
  for (const name of expected.closed) accessibilityNode(closed, 'button', name);
  expect(accessibilityNode(closed, 'button', expected.closed[0]).properties.focused).toBe(true);
  await page.getByRole('status').evaluate((element) => {
    const results: string[] = [];
    element.setAttribute('data-result-history', '[]');
    new MutationObserver(() => {
      results.push(element.textContent ?? '');
      element.setAttribute('data-result-history', JSON.stringify(results));
    }).observe(element, { childList: true, subtree: true, characterData: true });
  });
  for (const [turn, pair] of EXAMPLE_PAIRS.entries()) {
    await moveKeyboardTo(page, pair[0], expected.size);
    await page.keyboard.press('Enter');
    const revealed = await capture(`revealed-${turn + 1}`);
    expect(
      accessibilityNode(revealed, 'button', expected.revealed[pair[0]]).properties.focused,
    ).toBe(true);
    if (turn === 0) await page.clock.runFor(3000);
    await moveKeyboardTo(page, pair[1], expected.size);
    await page.keyboard.press('Space');
    const matched = await capture(`turn-${turn + 1}`);
    for (const index of pair)
      expect(
        accessibilityNode(matched, 'button', expected.matched[index]).properties.disabled,
      ).toBe(true);
    const result = accessibilityNode(matched, 'status');
    expect(result.properties.live).toBe('polite');
    expect(result.properties.atomic).toBe(true);
    expect(result.text).toBe(expected.turns[turn]);
  }
  const win = await capture('completed');
  expect(win.filter((node) => node.role === 'paragraph').map((node) => node.text)).toContain(
    expected.win,
  );
  const announcements: unknown = JSON.parse(
    (await page.getByRole('status').getAttribute('data-result-history')) ?? '[]',
  );
  expect(announcements).toEqual(expected.turns);
  await page.keyboard.press('Tab');
  expect(await page.locator('.card:focus').count()).toBe(0);
  const exit = await capture('board-exit');
  expect(
    accessibilityNode(exit, 'DisclosureTriangle', expected.controls.summary).properties.focused,
  ).toBe(true);
  await page.keyboard.press('Shift+Tab');
  await page.keyboard.press('Shift+Tab');
  await page.keyboard.press('Shift+Tab');
  await page.keyboard.press('Enter');
  const reset = await capture('new-game');
  expect(accessibilityNode(reset, 'button', expected.controls.newGame).properties.focused).toBe(
    true,
  );
  for (const name of expected.closed) accessibilityNode(reset, 'button', name);
  expect(accessibilityNode(reset, 'status').text).toBe(expected.newGame);
  await page.locator('summary').click();
  await page.getByRole('button', { name: 'Export snapshot', exact: true }).click();
  const snapshot = await page.getByLabel('Snapshot JSON', { exact: true }).inputValue();
  const loadEvents = [
    {
      stage: 'invalid-board',
      field: 'Board code',
      value: 'invalid',
      button: 'Load board',
      notice: 'Invalid board code.',
    },
    {
      stage: 'invalid-snapshot',
      field: 'Snapshot JSON',
      value: '{',
      button: 'Load snapshot',
      notice: 'Invalid snapshot.',
    },
    {
      stage: 'loaded-snapshot',
      field: 'Snapshot JSON',
      value: snapshot,
      button: 'Load snapshot',
      notice: 'Loaded snapshot. Select Resume to continue.',
    },
  ];
  for (const event of loadEvents) {
    await page.getByLabel(event.field, { exact: true }).fill(event.value);
    const button = page.getByRole('button', { name: event.button, exact: true });
    for (let repeat = 1; repeat <= 2; repeat += 1) {
      await button.click();
      await expect(button).toBeFocused();
      await expect(page.locator('.load-notice')).toHaveText(event.notice);
      const tree = await capture(`${event.stage}-${repeat}`);
      const status = accessibilityNode(tree, 'status');
      expect(tree.filter((node) => node.role === 'status')).toHaveLength(1);
      expect(status.properties.live).toBe('polite');
      expect(status.properties.atomic).toBe(true);
      expect(status.text).toBe(event.notice);
    }
  }
  await expect(page.getByRole('button', { name: 'Resume', exact: true })).toBeVisible();
  // A paused import blocks activation without marking its unmatched cards disabled.
  await expect(page.locator('.card[aria-disabled="true"]')).toHaveCount(0);
  expect(
    JSON.parse((await page.getByRole('status').getAttribute('data-result-history')) ?? '[]'),
  ).toEqual([
    ...expected.turns,
    expected.newGame,
    ...loadEvents.flatMap((event) => [event.notice, event.notice]),
  ]);
  const readme = await readFile(new URL('../README.md', import.meta.url), 'utf8');
  expect(readme.split(/\r?\n/)).toContain(expected.readmeRecord);
  const record =
    JSON.stringify(
      {
        schemaVersion: 1,
        status: 'tree-checked',
        chromium: browser.version(),
        playwright: await playwrightVersion(),
        screenReader: null,
        spokenReview: 'User acceptance. No spoken output was tested by the developer.',
        captures,
      },
      null,
      2,
    ) + '\n';
  expect(record).not.toMatch(
    /\b(?:file|https?):\/\/|[A-Za-z]:[\\/]|nodeId|frameId|backendDOMNodeId/,
  );
  await writeFile(artifacts.outputPath('accessibility-journey.json'), record);
  await session.detach();
});
