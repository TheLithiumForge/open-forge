import { expect, test } from './browser-test.ts';
import { SESSION_URL, loadTimedBoard, exportedSnapshot } from './game-fixture.ts';

test('unmount clears the active timer and visibility listener', async ({ page }) => {
  await page.addInitScript(() => {
    const pending = new Set<number>();
    const listeners = new Set<EventListenerOrEventListenerObject>();
    const set = window.setTimeout.bind(window),
      clear = window.clearTimeout.bind(window);
    Object.defineProperty(window, 'setTimeout', {
      value: (handler: () => void, delay?: number) => {
        const id = set(() => {
          pending.delete(id);
          handler();
        }, delay);
        pending.add(id);
        document.documentElement.dataset.pendingTimers = String(pending.size);
        return id;
      },
    });
    Object.defineProperty(window, 'clearTimeout', {
      value: (id?: number) => {
        pending.delete(id ?? -1);
        document.documentElement.dataset.pendingTimers = String(pending.size);
        clear(id);
      },
    });
    const add = document.addEventListener.bind(document),
      remove = document.removeEventListener.bind(document);
    Object.defineProperty(document, 'addEventListener', {
      value: (
        type: string,
        listener: EventListenerOrEventListenerObject,
        options?: boolean | AddEventListenerOptions,
      ) => {
        if (type === 'visibilitychange') {
          listeners.add(listener);
          document.documentElement.dataset.visibilityListeners = String(listeners.size);
        }
        add(type, listener, options);
      },
    });
    Object.defineProperty(document, 'removeEventListener', {
      value: (
        type: string,
        listener: EventListenerOrEventListenerObject,
        options?: boolean | EventListenerOptions,
      ) => {
        if (type === 'visibilitychange') {
          listeners.delete(listener);
          document.documentElement.dataset.visibilityListeners = String(listeners.size);
        }
        remove(type, listener, options);
      },
    });
  });
  await page.goto(SESSION_URL);
  await page.locator('.card').first().click();
  await expect(page.locator('html')).toHaveAttribute('data-pending-timers', '1');
  await expect(page.locator('html')).toHaveAttribute('data-visibility-listeners', '1');
  await page.getByRole('button', { name: 'Unmount game', exact: true }).click();
  await expect(page.locator('html'), 'A04 active timer cancelled at unmount').toHaveAttribute(
    'data-pending-timers',
    '0',
  );
  await expect(page.locator('html'), 'A04 visibility subscription removed').toHaveAttribute(
    'data-visibility-listeners',
    '0',
  );
});

test('a prior turn deadline cannot close a later mismatch in the same deal', async ({ page }) => {
  await loadTimedBoard(page);
  await page.locator('.card').nth(0).click();
  await page.locator('.card').nth(1).click();
  await page.clock.runFor(800);
  await expect(page.locator('.card.revealed')).toHaveCount(0);
  await page.clock.runFor(100);
  await page.locator('.card').nth(3).click();
  await page.locator('.card').nth(4).click();
  await page.clock.runFor(799);
  await expect(page.locator('.card.revealed')).toHaveCount(2);
  expect((await exportedSnapshot(page)).selection).toMatchObject({
    kind: 'mismatch',
    first: 3,
    second: 4,
  });
  await page.clock.runFor(1);
  await expect(page.locator('.card.revealed')).toHaveCount(0);
});
