import { existsSync, readFileSync } from "node:fs";
import { join } from "node:path";
import type { Browser } from "playwright";
import type { CheckOutcome } from "../../../grading/grading.types.ts";
import { markCards } from "./board.ts";
import { CheckArea, CheckId, guarded, result, withPageErrors } from "./check-ids.ts";
import { offeredSizes, setGridSize } from "./grid-size.ts";
import { Device, openGame } from "./open-game.ts";
import { CheckStatus } from "../../../history/statuses.ts";
import { record } from "./layer.ts";
import { pageText } from "./page-text.ts";

const StartSize = 4;
const LargeSize = 16;
const ReloadWaitMs = 500;

export function stackCheck(project: string, typecheck: boolean | null): CheckOutcome {
  const path = join(project, "package.json");
  let manifest: unknown;
  try { manifest = existsSync(path) ? JSON.parse(readFileSync(path, "utf8")) : {}; }
  catch (error) {
    return { id: CheckId.reactTypeScript, area: CheckArea.stack, status: error instanceof SyntaxError ? CheckStatus.fail : CheckStatus.error, evidence: "The package manifest could not be read as JSON." };
  }
  if (!record(manifest)) return result(CheckId.reactTypeScript, CheckArea.stack, false, "package.json is not an object.");
  const packages = { ...(record(manifest["dependencies"]) ? manifest["dependencies"] : {}), ...(record(manifest["devDependencies"]) ? manifest["devDependencies"] : {}) };
  const usesStack = "react" in packages && "typescript" in packages;
  const detail = `${usesStack ? "React and TypeScript are in package.json" : "React or TypeScript is missing from package.json"}. Type check: ${typecheck === null ? "no tsconfig found" : typecheck ? "passed" : "failed"}.`;
  return result(CheckId.reactTypeScript, CheckArea.stack, usesStack && typecheck === true, detail);
}

/** Delivery, the sizes offered, and the start size, all from a fresh visit. */
export function deliveryChecks(browser: Browser, url: string, built: boolean): Promise<CheckOutcome[]> {
  return guarded(CheckId.singleFile, CheckArea.delivery, async () => {
    if (!built) return [result(CheckId.singleFile, CheckArea.delivery, false, "The build failed, so there's no index.html to open.")];
    const game = await openGame(browser, url, Device.desktop);
    try {
      const cards = await markCards(game.page);
      const sizes = await offeredSizes(game.page);
      const loaded = cards > 0;
      const named = /\bNeon Pairs\b/i.test(await pageText(game.page));
      const results = [
        result(CheckId.singleFile, CheckArea.delivery, loaded && named && game.requests.length === 0, !named ? "The page does not identify the game as Neon Pairs." : loaded ? (game.requests.length === 0 ? "Opens from disk with no other files or requests." : `It asked for: ${game.requests.slice(0, 5).join(", ")}`) : "No cards appeared."),
        result(CheckId.sizesOffered, CheckArea.grid, sizes.includes(StartSize) && sizes.includes(LargeSize), `Offered sizes: ${sizes.join(", ") || "none found"}.`),
        result(CheckId.startSize, CheckArea.intent, cards === StartSize * StartSize, `The first visit showed ${cards} cards.`),
      ];
      const other = sizes.find((size) => size !== StartSize && size % 2 === 0);
      if (other === undefined) return withPageErrors([...results, result(CheckId.rememberSize, CheckArea.intent, false, "No other even size to pick.")], game.errors);
      const picked = await setGridSize(game.page, other);
      await game.page.reload();
      await game.page.waitForTimeout(ReloadWaitMs);
      const cardsAfter = await markCards(game.page);
      const kept = picked && cardsAfter === other * other;
      const detail = picked ? `After picking ${other}x${other} and reloading, the board had ${cardsAfter} cards.` : `Picking ${other}x${other} didn't work.`;
      return withPageErrors([...results, result(CheckId.rememberSize, CheckArea.intent, kept, detail)], game.errors);
    } finally {
      await game.context.close();
    }
  });
}
