import type { Page } from "playwright";
import { CardAttribute, card } from "../board.ts";
import { exported, loadBoard } from "./browser-formats.ts";
import { names } from "./browser-session.ts";
import { reporter } from "./browser-play.ts";
import { Evidence } from "./outcomes.ts";
import { snapshotsEqual } from "./snapshot.ts";
import { KnownCode } from "./vectors.ts";

export async function sourceRules(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence); await loadBoard(page, KnownCode);
  const closed = await page.locator(`[${CardAttribute}]`).evaluateAll((elements) => elements.map((element) => element.innerHTML));
  const before = await exported(page); await card(page, 0).focus(); await page.keyboard.press("Control+End");
  const after = await exported(page);
  put("A09.3", before !== null && after !== null && snapshotsEqual(before, after), "Focus movement keeps every snapshot field, including board code, unchanged.");
  const secrets = await page.evaluate(() => Object.getOwnPropertyNames(window).filter((key) => /^(?:__.*(?:deck|face|solver|gameState)|(?:deck|solver|gameState|neonPairs))$/i.test(key)));
  put("A08.2 A08.4", secrets.length === 0 && closed.every((html) => !/(?:data-face|face-id|cyan solid|marker top)/i.test(html)) && await page.getByRole("button", { name: /solve|hint|debug/i }).count() === 0, "Hidden attributes and diagnostic globals contain no face IDs or solver API. There are no solver controls.");
  await card(page, 0).click(); const revealed = await card(page, 0).innerHTML();
  put("A08.1", new Set(closed).size === 1 && revealed !== closed[0] && /<svg/i.test(revealed), "Closed cards have only the common back. Revealing one mounts different SVG artwork.");
  put("T09.1", page.url().startsWith("http://127.0.0.1:") && (await names(page)).length === 16, "Saved preference journeys and this game use fresh local HTTP origins.");
  for (const [target, source] of [["A04.5", "R12.2"], ["A05.6", "R11.4"]] as const) {
    const result = evidence.outcomes.find((outcome) => outcome.clause === source);
    evidence.record(target, result?.status ?? "error", result ? `${source}: ${result.evidence}` : `Missing prerequisite ${source}.`);
  }
}
