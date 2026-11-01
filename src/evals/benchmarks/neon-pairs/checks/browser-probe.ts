import assert from "node:assert/strict";
import { chromium } from "playwright";
import { markCards } from "./board.ts";

export async function probeCardDetection(): Promise<void> {
  const browser = await chromium.launch();
  try {
    const page = await browser.newPage({ javaScriptEnabled: false });
    for (const size of [4, 6, 8, 10, 12, 14, 16]) {
      const cards = Array.from({ length: size * size }, () => '<button style="width:54px;height:54px"></button>').join("");
      await page.setContent(`<main><div><button style="width:88px;height:32px">New Game</button></div><div style="display:grid;grid-template-columns:repeat(${size},54px)">${cards}</div></main>`);
      assert.equal(await markCards(page), size * size);
      process.stdout.write(`${size}x${size}: ${size * size} cards, New Game excluded\n`);
    }
  } finally { await browser.close(); }
}

if (import.meta.main) await probeCardDetection();
