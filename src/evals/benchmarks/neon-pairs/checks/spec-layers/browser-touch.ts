import type { Page } from "playwright";
import { card } from "../board.ts";

const Gestures = 8;
const Steps = 10;
const Distance = 20;

export async function touchBoardEnd(page: Page): Promise<boolean> {
  const last = card(page, 255);
  const cdp = await page.context().newCDPSession(page);
  try {
    for (let gesture = 0; gesture < Gestures; gesture++) {
      const box = await last.evaluate((element) => {
        let parent = element.parentElement;
        while (parent?.parentElement && !/(auto|scroll)/.test(getComputedStyle(parent).overflowX + getComputedStyle(parent).overflowY)) parent = parent.parentElement;
        const rect = parent?.getBoundingClientRect();
        return rect ? { x: rect.x, y: rect.y, width: rect.width, height: rect.height } : null;
      });
      if (!box) return false;
      const viewport = page.viewportSize();
      if (!viewport) throw new Error("Touch checker needs a viewport.");
      const x = Math.min(viewport.width - 24, box.x + box.width - 24), y = Math.min(viewport.height - 24, box.y + box.height - 24);
      await cdp.send("Input.dispatchTouchEvent", { type: "touchStart", touchPoints: [{ x, y }] });
      for (let step = 1; step <= Steps; step++) {
        await cdp.send("Input.dispatchTouchEvent", { type: "touchMove", touchPoints: [{ x: Math.max(1, x - step * Distance), y: Math.max(1, y - step * Distance) }] });
        await page.waitForTimeout(16);
      }
      await cdp.send("Input.dispatchTouchEvent", { type: "touchEnd", touchPoints: [] });
    }
    await page.waitForTimeout(200);
    return last.evaluate((element) => {
      let parent = element.parentElement;
      while (parent?.parentElement && !/(auto|scroll)/.test(getComputedStyle(parent).overflowX + getComputedStyle(parent).overflowY)) parent = parent.parentElement;
      if (!parent) return false;
      const card = element.getBoundingClientRect(), clip = parent.getBoundingClientRect();
      return card.x >= clip.x && card.y >= clip.y && card.right <= Math.min(clip.right, innerWidth) && card.bottom <= Math.min(clip.bottom, innerHeight);
    });
  } finally { await cdp.detach(); }
}

export async function heldMenu(page: Page): Promise<boolean> {
  const target = card(page, 0), box = await target.boundingBox();
  if (!box) throw new Error("Held-touch checker cannot find its card.");
  const cdp = await page.context().newCDPSession(page);
  try {
    await cdp.send("Input.dispatchTouchEvent", { type: "touchStart", touchPoints: [{ x: box.x + box.width / 2, y: box.y + box.height / 2 }] });
    await page.waitForTimeout(1200);
    const allowed = await target.evaluate((element) => element.dispatchEvent(new MouseEvent("contextmenu", { bubbles: true, cancelable: true })));
    await cdp.send("Input.dispatchTouchEvent", { type: "touchEnd", touchPoints: [] });
    return allowed;
  } finally { await cdp.detach(); }
}
