import assert from 'node:assert/strict';
import { Base, NameControl, ProgressLimit, exported, fresh, load, named, nameField, revealed } from './game.ts';
import type { ChangeCheck } from './check.types.ts';

export const nameChecks: readonly ChangeCheck[] = [
  { id: 'F1-01', starting: 'fail', description: 'C15.1: the native Deal name control starts empty and limits input to 40 UTF-16 units.', action: async (page) => {
    await fresh(page); const field = await nameField(page);
    assert.equal(await field.inputValue(), '');
    assert.equal(await field.evaluate((element) => element.tagName), 'INPUT');
    for (const text of ['x'.repeat(NameControl.maximum + 1), '😀'.repeat(NameControl.maximum / 2), '😀'.repeat(NameControl.maximum / 2) + 'x']) {
      await field.fill(''); await field.pressSequentially(text);
      const actual = await field.inputValue();
      assert.equal(actual.length, NameControl.maximum);
      assert.equal(actual, text.slice(0, NameControl.maximum));
      assert.equal((await exported(page))['name'], actual);
    }
  } },
  { id: 'F1-02', starting: 'fail', description: 'C15.2: editing a name keeps cards, progress, active time and board code unchanged.', action: async (page) => {
    await fresh(page); await page.locator('.card').nth(0).click(); await page.clock.runFor(500);
    const before = await exported(page); await named(page); const after = await exported(page);
    const { version: oldVersion, ...oldProgress } = before;
    const { version: newVersion, name, ...newProgress } = after;
    assert.equal(oldVersion, 1); assert.equal(newVersion, 2); assert.equal(name, NameControl.value);
    assert.deepEqual(newProgress, oldProgress);
  } },
  { id: 'F1-03', starting: 'fail', description: 'D05 and C15.3: name, play a mismatch, wait 800ms, export, New Game and load the app text to restore the name and progress.', action: async (page) => {
    await fresh(page); await named(page); const value = await exported(page);
    assert.equal(value['version'], 2); assert.equal(value['name'], NameControl.value);
    assert.deepEqual(Object.keys(value).sort(), [...Object.keys(Base), 'name'].sort());
    await named(page, 'x'.repeat(NameControl.maximum)); assert.equal((await exported(page))['name'], 'x'.repeat(NameControl.maximum));
    await named(page);
    await page.getByRole('button', { name: 'Row 1, column 1, closed', exact: true }).click();
    await page.getByRole('button', { name: 'Row 1, column 2, closed', exact: true }).click();
    assert.equal(await revealed(page), 2); await page.clock.runFor(ProgressLimit.mismatchMs);
    assert.equal(await revealed(page), 0);
    const progress = await exported(page);
    const text = await page.getByLabel('Snapshot JSON', { exact: true }).inputValue();
    assert.equal(progress['name'], NameControl.value); assert.equal(progress['moves'], 1);
    assert.equal(progress['elapsedMs'], ProgressLimit.mismatchMs);
    await page.getByRole('button', { name: 'New Game', exact: true }).click();
    assert.equal(await (await nameField(page)).inputValue(), '');
    await load(page, text); assert.deepEqual(await exported(page), progress);
    assert.equal(await (await nameField(page)).inputValue(), NameControl.value);
  } },
  { id: 'F1-04', starting: 'pass', description: 'D05 and C08.2: unnamed exports and legacy imports keep version 1 and the original exact fields.', action: async (page) => {
    await fresh(page); assert.deepEqual(await exported(page), Base);
    const old = { ...Base, started: true, selection: { kind: 'one', index: 2 } };
    await load(page, old); assert.deepEqual(await exported(page), old);
  } },
  { id: 'F1-05', starting: 'fail', description: 'C15.4: a named import restores its name paused and a later legacy import clears it.', action: async (page) => {
    await fresh(page); await nameField(page);
    const value = { ...Base, version: 2, name: NameControl.value, started: true, selection: { kind: 'one', index: 2 } };
    await load(page, value); assert.deepEqual(await exported(page), value);
    assert.equal(await (await nameField(page)).inputValue(), NameControl.value);
    assert.equal(await page.getByRole('button', { name: 'Resume', exact: true }).count(), 1);
    await page.getByRole('button', { name: 'Row 1, column 1, closed', exact: true }).click({ force: true });
    assert.deepEqual(await exported(page), value);
    await page.clock.runFor(5000); assert.deepEqual(await exported(page), value);
    await load(page, Base); assert.equal(await (await nameField(page)).inputValue(), '');
  } },
];
