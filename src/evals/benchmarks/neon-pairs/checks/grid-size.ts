import type { Page } from "playwright";
import { CardAttribute, markCards } from "./board.ts";
import { newGameButton } from "./page-text.ts";
import { dismissModal } from "./modal.ts";

const ApplyWaitMs = 400;

/** The square grid sizes the game offers, read from its controls: a select, buttons or a slider. */
export function offeredSizes(page: Page): Promise<number[]> {
  return page.evaluate(() => {
    const sizes = new Set<number>();
    const labels = [
      ...Array.from(document.querySelectorAll("option")).map((option) => `${option.textContent ?? ""} ${option.value}`),
      ...Array.from(document.querySelectorAll("button, [role=radio], label")).map((element) => element.textContent ?? ""),
    ];
    for (const label of labels) {
      for (const match of label.matchAll(/(\d+)\s*(?:[x×]|by)\s*(\d+)/gi)) if (match[1] === match[2]) sizes.add(Number(match[1]));
    }
    for (const option of Array.from(document.querySelectorAll("option"))) {
      if (/^\s*\d+\s*$/.test(option.value)) sizes.add(Number(option.value));
    }
    for (const range of Array.from(document.querySelectorAll<HTMLInputElement>("input[type=range], input[type=number]"))) {
      const step = Number(range.step || "1");
      for (let size = Number(range.min); size <= Number(range.max); size += step) sizes.add(size);
    }
    return [...sizes].filter((size) => size >= 2 && size <= 32).sort((a, b) => a - b);
  });
}

async function chooseSize(page: Page, size: number): Promise<boolean> {
  const label = new RegExp(`^\\s*${size}\\s*[x×]\\s*${size}\\s*$|^\\s*${size}\\s*$`);
  for (const select of await page.locator("select").all()) {
    const options = await select.locator("option").all();
    for (const option of options) {
      const text = `${(await option.textContent()) ?? ""}`;
      const value = (await option.getAttribute("value")) ?? text;
      if (label.test(text) || label.test(value) || new RegExp(`\\b${size}\\s*[x×]\\s*${size}\\b`).test(text)) {
        await select.selectOption(value);
        return true;
      }
    }
  }
  // Exact labels first, then labels with more text, such as "16 × 16 · 128 pairs".
  const within = new RegExp(`(^|\\D)${size}\\s*([x×]|by)\\s*${size}(\\D|$)`, "i");
  for (const name of [label, within]) {
    const control = page.getByRole("button", { name }).or(page.getByRole("radio", { name })).or(page.getByRole("tab", { name }));
    if ((await control.count()) > 0) {
      await control.first().click({ force: true });
      return true;
    }
  }
  const radio = page.locator(`input[type=radio][value="${size}"], input[type=radio][value="${size}x${size}"], input[type=radio][value="${size}×${size}"]`);
  if ((await radio.count()) > 0) {
    await radio.first().check({ force: true });
    return true;
  }
  const range = page.locator("input[type=range], input[type=number]").first();
  if ((await range.count()) === 0) return false;
  await range.fill(String(size));
  await range.dispatchEvent("change");
  return true;
}

/** Checks the card count and square layout, allowing one empty cell on odd boards. */
export async function boardFitsSize(page: Page, size: number): Promise<boolean> {
  const count = await markCards(page);
  if (count !== size * size - size % 2) return false;
  return page.evaluate(({ attribute, size }) => {
    const boxes = Array.from(document.querySelectorAll(`[${attribute}]`)).map((element) => element.getBoundingClientRect());
    const groups = (values: number[], tolerance: number): number[] => values.sort((a, b) => a - b).reduce<number[]>((result, value) => {
      if (!result.some((other) => Math.abs(other - value) < tolerance)) result.push(value);
      return result;
    }, []);
    const first = boxes[0];
    if (!first) return false;
    const columns = groups(boxes.map((box) => box.x + box.width / 2), first.width / 4);
    const rows = groups(boxes.map((box) => box.y + box.height / 2), first.height / 4);
    const cells = boxes.map((box) => `${columns.findIndex((x) => Math.abs(x - box.x - box.width / 2) < first.width / 4)}:${rows.findIndex((y) => Math.abs(y - box.y - box.height / 2) < first.height / 4)}`);
    return columns.length === size && rows.length === size && new Set(cells).size === boxes.length;
  }, { attribute: CardAttribute, size });
}

/** Picks a grid size, starting a new game if the size only applies then. */
export async function setGridSize(page: Page, size: number): Promise<boolean> {
  await dismissModal(page);
  if (!(await chooseSize(page, size))) return false;
  await page.waitForTimeout(ApplyWaitMs);
  if (await boardFitsSize(page, size)) return true;
  await (await newGameButton(page))?.click();
  await page.waitForTimeout(ApplyWaitMs);
  return boardFitsSize(page, size);
}
