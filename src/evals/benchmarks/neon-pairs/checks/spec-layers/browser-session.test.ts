import assert from "node:assert/strict";
import { existsSync } from "node:fs";
import { mkdtemp, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { test } from "node:test";
import { chromium } from "playwright";
import { drainTasks, session, SettleTasks } from "./browser-session.ts";
import { ZoomViewport } from "./browser-zoom.ts";
import { removeTemporary } from "./temporary.ts";

test("settling outlasts work an app queued on the same task source, where one task doesn't", async () => {
  const root = await mkdtemp(join(tmpdir(), "eval-layout-session-"));
  const browser = await chromium.launch(existsSync(chromium.executablePath()) ? {} : { channel: "chrome" });
  try {
    const html = join(root, "index.html");
    // Three queued tasks, as a framework's effect that starts a later render.
    await writeFile(html, `<!doctype html><script>
      const later = (work) => { const channel = new MessageChannel(); channel.port1.onmessage = work; channel.port2.postMessage(null); };
      window.work = () => { window.done = false; later(() => later(() => later(() => { window.done = true; }))); };
    </script>`);
    const open = await session(browser, html);
    try {
      const doneAfter = (tasks: number) => open.page.evaluate(`(async () => { window.work(); await (${drainTasks.toString()})(${tasks}); return window.done; })()`);
      assert.equal(await doneAfter(1), false);
      assert.equal(await doneAfter(SettleTasks), true);
    } finally { await open.close(); }
  } finally { await browser.close(); await removeTemporary(root, "eval-layout-session-"); }
});

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
