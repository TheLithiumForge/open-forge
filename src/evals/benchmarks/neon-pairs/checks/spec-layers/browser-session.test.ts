import assert from "node:assert/strict";
import { existsSync } from "node:fs";
import { mkdtemp, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { test } from "node:test";
import { chromium } from "playwright";
import { session } from "./browser-session.ts";
import { ZoomViewport } from "./browser-zoom.ts";
import { removeTemporary } from "./temporary.ts";

test("zoom session applies both CSS dimensions and device scale before loading", async () => {
  const root = await mkdtemp(join(tmpdir(), "eval-layout-session-"));
  const browser = await chromium.launch(existsSync(chromium.executablePath()) ? {} : { channel: "chrome" });
  try {
    const html = join(root, "index.html"); await writeFile(html, "<!doctype html><p>Checker page</p>");
    const open = await session(browser, html, ZoomViewport);
    try {
      assert.deepEqual(await open.page.evaluate(() => ({ width: innerWidth, height: innerHeight, deviceScaleFactor: devicePixelRatio })), ZoomViewport);
    } finally { await open.close(); }
  } finally { await browser.close(); await removeTemporary(root, "eval-layout-session-"); }
});
