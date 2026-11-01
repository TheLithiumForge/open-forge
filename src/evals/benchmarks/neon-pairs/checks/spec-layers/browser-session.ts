import { createServer } from "node:http";
import { readFile } from "node:fs/promises";
import { pathToFileURL } from "node:url";
import type { Browser, BrowserContext, Page } from "playwright";
import { card } from "../board.ts";
import { markVisibleCards as markCards } from "./browser-cards.ts";
import { BehaviorFailure } from "./outcomes.ts";
import { cardName, faceIn } from "./view-values.ts";

export const Labels = { size: "Grid size", fresh: "New Game", restart: "Restart", reproduce: "Reproduce a game", code: "Board code", loadBoard: "Load board", snapshot: "Snapshot JSON", export: "Export snapshot", loadSnapshot: "Load snapshot", resume: "Resume" } as const;
export interface Session { readonly page: Page; readonly context: BrowserContext; readonly errors: string[]; readonly requests: string[]; readonly origin: string; close(): Promise<void> }

export async function session(browser: Browser, html: string, options: { readonly file?: boolean; readonly width?: number; readonly blockedStorage?: boolean } = {}): Promise<Session> {
  const bytes = await readFile(html);
  const server = createServer((request, response) => {
    if (request.url !== "/" && request.url !== "/index.html") { response.writeHead(404).end(); return; }
    response.setHeader("Content-Type", "text/html; charset=utf-8"); response.end(bytes);
  });
  await new Promise<void>((done) => server.listen(0, "127.0.0.1", done));
  const address = server.address();
  if (!address || typeof address === "string") throw new Error("HTTP checker failed to bind.");
  const origin = `http://127.0.0.1:${address.port}`;
  const context = await browser.newContext({ viewport: { width: options.width ?? 1280, height: 800 }, hasTouch: true });
  const errors: string[] = [], requests: string[] = [];
  try {
    await context.addInitScript(() => {
      Object.defineProperty(navigator, "clipboard", { configurable: true, get: () => undefined });
      let calls = 0;
      Object.defineProperty(window, "__evalAudioCalls", { get: () => calls });
      for (const name of ["Audio", "AudioContext", "webkitAudioContext"]) {
        const constructor = Reflect.get(window, name);
        if (typeof constructor === "function") Reflect.set(window, name, new Proxy(constructor, { construct(target, args) { calls++; return Reflect.construct(target, args); } }));
      }
      const play = HTMLMediaElement.prototype.play;
      HTMLMediaElement.prototype.play = function () { calls++; return play.call(this); };
    });
    if (options.blockedStorage) await context.addInitScript(() => {
      for (const name of ["getItem", "setItem", "removeItem"]) Object.defineProperty(Storage.prototype, name, { value: () => { throw new DOMException("Storage disabled", "SecurityError"); } });
    });
    await context.route("**/*", async (route) => {
      const url = route.request().url();
      if (url === `${origin}/` || url.startsWith("file:") || url.startsWith("data:")) await route.continue();
      else { requests.push(url.startsWith(origin) ? url.slice(origin.length) : "external request"); await route.abort(); }
    });
    const page = await context.newPage();
    page.setDefaultTimeout(4000);
    page.on("pageerror", (error) => errors.push(error.message));
    await page.clock.install();
    await page.goto(options.file ? pathToFileURL(html).href : `${origin}/`);
    await page.clock.pauseAt(new Date(Date.now() + 1000));
    await markCards(page);
    return { page, context, errors, requests, origin, close: async () => { await context.close(); await new Promise<void>((done) => server.close(() => done())); } };
  } catch (error) { await context.close(); server.close(); throw error; }
}

export async function names(page: Page): Promise<string[]> {
  const count = await markCards(page);
  const result: string[] = [];
  for (let index = 0; index < count; index++) result.push(cardName(await card(page, index).ariaSnapshot()) ?? "");
  return result;
}
export async function click(page: Page, index: number): Promise<void> {
  // A player can click aria-disabled cards while the game ignores the activation.
  await card(page, index).click({ force: true });
  await page.mouse.move(0, 0);
}
export async function step(page: Page, ms: number): Promise<void> { await page.clock.runFor(ms); }
export async function changeSize(page: Page, size: number): Promise<void> {
  const select = page.getByRole("combobox", { name: Labels.size, exact: true });
  const option = select.locator("option").filter({ hasText: new RegExp(`^${size}\\s*[x×]\\s*${size}$`) });
  const value = await option.getAttribute("value");
  if (value === null) throw new BehaviorFailure(`No ${size} by ${size} option.`);
  await select.selectOption(value); await markCards(page);
}
export async function reset(page: Page, fresh = false): Promise<void> { await page.getByRole("button", { name: fresh ? Labels.fresh : Labels.restart, exact: true }).click(); await markCards(page); }
export async function hidden(page: Page, value: boolean): Promise<void> {
  await page.evaluate((isHidden) => {
    Object.defineProperty(document, "hidden", { configurable: true, get: () => isHidden });
    Object.defineProperty(document, "visibilityState", { configurable: true, get: () => isHidden ? "hidden" : "visible" });
    document.dispatchEvent(new Event("visibilitychange"));
  }, value);
}
export async function visibleDeck(page: Page): Promise<string[]> {
  const count = await markCards(page), deck: string[] = [];
  for (let index = 0; index < count; index += 2) {
    await click(page, index); await click(page, index + 1);
    for (const position of [index, index + 1]) {
      const face = faceIn(cardName(await card(page, position).ariaSnapshot()) ?? "");
      if (!face) throw new BehaviorFailure(`Card ${position} supplied no stated face name after reveal.`);
      deck[position] = face;
    }
    await step(page, 800);
  }
  return deck;
}
export async function solve(page: Page, deck: readonly string[]): Promise<void> {
  const pairs = new Map<string, number[]>();
  deck.forEach((face, index) => pairs.set(face, [...(pairs.get(face) ?? []), index]));
  for (const positions of pairs.values()) {
    if (positions.length !== 2) throw new BehaviorFailure("Visible play did not find exactly two of each face.");
    for (const index of positions) await click(page, index);
  }
}
export async function body(page: Page): Promise<string> { return page.locator("body").innerText(); }
export function metric(text: string, label: "moves" | "pairs" | "time" | "best"): number | null {
  const prefixes = { moves: "(?<!best[ \\t])moves", pairs: "(?:matched[ \\t]+)?pairs", time: "(?:active[ \\t]+)?(?:time|elapsed)", best: "best(?:[ \\t]+moves)?" };
  const match = new RegExp(`${prefixes[label]}[ \\t:\\n]*(\\d+)`, "i").exec(text);
  return match ? Number(match[1]) : null;
}
