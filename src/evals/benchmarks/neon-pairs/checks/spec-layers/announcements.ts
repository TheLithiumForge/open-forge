import type { Page } from "playwright";
export const LiveRegion = '[aria-live="polite"][aria-atomic="true"]';
export async function observeAnnouncements(page: Page): Promise<() => Promise<string[]>> {
  await page.locator(LiveRegion).evaluate((element) => {
    const entries: string[] = [];
    Object.defineProperty(window, "__evalAnnouncements", { value: entries, configurable: true });
    new MutationObserver(() => { const text = element.textContent?.trim(); if (text) entries.push(text); }).observe(element, { subtree: true, childList: true, characterData: true });
  });
  return () => page.evaluate(() => [...Reflect.get(window, "__evalAnnouncements")] as string[]);
}
