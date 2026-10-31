import type { Browser, BrowserContext, Page } from "playwright";
import { frameTimingScript } from "./activation.ts";
import { backsOf, markCards } from "./board.ts";
import type { FaceSignature } from "./face-signature.ts";

export const Device = { desktop: "desktop", phone: "phone" } as const;
export type Device = (typeof Device)[keyof typeof Device];

const viewports = {
  [Device.desktop]: { viewport: { width: 1280, height: 900 } },
  [Device.phone]: { viewport: { width: 390, height: 844 }, isMobile: true, hasTouch: true, deviceScaleFactor: 2 },
} as const satisfies Record<Device, object>;

// Counts every sound the page starts, so "No sound" can be checked without listening.
const soundScript = `
  window.__evalSounds = 0;
  const count = () => { window.__evalSounds += 1; };
  for (const name of ["AudioContext", "webkitAudioContext"]) {
    const Original = window[name];
    if (Original) window[name] = class extends Original { constructor(...args) { super(...args); count(); } };
  }
  const play = HTMLMediaElement.prototype.play;
  HTMLMediaElement.prototype.play = function (...args) { count(); return play.apply(this, args); };
`;

// A phone's tap highlight briefly covers a tapped card, which the checks would read as its face.
const noTapHighlightScript = `
  document.addEventListener("DOMContentLoaded", () => {
    const style = document.createElement("style");
    style.textContent = "* { -webkit-tap-highlight-color: transparent !important; }";
    document.head.append(style);
  });
`;

const BoardWaitMs = 10_000;

export interface OpenGame {
  readonly context: BrowserContext;
  readonly page: Page;
  /** Every URL the page asked for, apart from the page itself. */
  readonly requests: string[];
  readonly firstRenderMs: number | null;
}

export async function openGame(browser: Browser, url: string, device: Device, context?: BrowserContext): Promise<OpenGame> {
  const owned = context ?? (await browser.newContext({ ...viewports[device], locale: "en-US", timezoneId: "UTC" }));
  if (context === undefined) {
    await owned.addInitScript(soundScript);
    await owned.addInitScript(frameTimingScript);
    await owned.addInitScript(noTapHighlightScript);
    // Blocks everything outside the page, so a game that needs the network fails instead of loading it.
    await owned.route("**/*", (route) => (/^(file|data|blob):/.test(route.request().url()) ? route.continue() : route.abort()));
  }
  const page = await owned.newPage();
  const requests: string[] = [];
  page.on("request", (request) => {
    if (request.url() !== url && !/^(data|blob):/.test(request.url())) requests.push(request.url());
  });
  const started = Date.now();
  await page.goto(url);
  const deadline = Date.now() + BoardWaitMs;
  let cards = await markCards(page);
  while (cards === 0 && Date.now() < deadline) {
    await page.waitForTimeout(200);
    cards = await markCards(page);
  }
  return { context: owned, page, requests, firstRenderMs: cards === 0 ? null : Date.now() - started };
}

export interface Board {
  readonly count: number;
  readonly backs: FaceSignature[];
}

/** Marks the cards again and remembers each card's back, as the start of a game. */
export async function readBoard(page: Page): Promise<Board> {
  const count = await markCards(page);
  return { count, backs: await backsOf(page) };
}
