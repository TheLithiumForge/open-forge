import { existsSync, readFileSync } from "node:fs";
import { join } from "node:path";
import type { Browser } from "playwright";
import type { CheckResult } from "../../../src/results/results.types.ts";
import { markCards } from "./board.ts";
import { CheckArea, CheckId, guarded, result } from "./check-ids.ts";
import { offeredSizes, setGridSize } from "./grid-size.ts";
import { Device, openGame } from "./open-game.ts";

interface Manifest {
  readonly dependencies?: Readonly<Record<string, string>>;
  readonly devDependencies?: Readonly<Record<string, string>>;
}

const StartSize = 4;
const LargeSize = 16;
const ReloadWaitMs = 500;

export function stackCheck(project: string, typecheck: boolean | null): CheckResult {
  const path = join(project, "package.json");
  const manifest: Manifest = existsSync(path) ? JSON.parse(readFileSync(path, "utf8")) : {};
  const packages = { ...manifest.dependencies, ...manifest.devDependencies };
  const usesStack = "react" in packages && "typescript" in packages;
  const detail = `${usesStack ? "React and TypeScript are in package.json" : "React or TypeScript is missing from package.json"}. Type check: ${typecheck === null ? "no tsconfig found" : typecheck ? "passed" : "failed"}.`;
  return result(CheckId.reactTypeScript, CheckArea.stack, usesStack && typecheck === true, detail);
}

/** Delivery, the sizes offered, and the start size, all from a fresh visit. */
export function deliveryChecks(browser: Browser, url: string, built: boolean): Promise<CheckResult[]> {
  return guarded(CheckId.singleFile, CheckArea.delivery, async () => {
    if (!built) return [result(CheckId.singleFile, CheckArea.delivery, false, "The build failed, so there's no index.html to open.")];
    const game = await openGame(browser, url, Device.desktop);
    try {
      const cards = await game.page.locator("[data-eval-card]").count();
      const sizes = await offeredSizes(game.page);
      const loaded = cards > 0;
      const results = [
        result(CheckId.singleFile, CheckArea.delivery, loaded && game.requests.length === 0, loaded ? (game.requests.length === 0 ? "Opens from disk with no other files or requests." : `It asked for: ${game.requests.slice(0, 5).join(", ")}`) : "No cards appeared."),
        result(CheckId.sizesOffered, CheckArea.grid, sizes.includes(StartSize) && sizes.includes(LargeSize), `Offered sizes: ${sizes.join(", ") || "none found"}.`),
        result(CheckId.startSize, CheckArea.intent, cards === StartSize * StartSize, `The first visit showed ${cards} cards.`),
      ];
      const other = sizes.find((size) => size !== StartSize && size % 2 === 0);
      if (other === undefined) return [...results, result(CheckId.rememberSize, CheckArea.intent, false, "No other even size to pick.")];
      const picked = await setGridSize(game.page, other);
      await game.page.reload();
      await game.page.waitForTimeout(ReloadWaitMs);
      const cardsAfter = await markCards(game.page);
      const kept = picked && cardsAfter === other * other;
      const detail = picked ? `After picking ${other}x${other} and reloading, the board had ${cardsAfter} cards.` : `Picking ${other}x${other} didn't work.`;
      return [...results, result(CheckId.rememberSize, CheckArea.intent, kept, detail)];
    } finally {
      await game.context.close();
    }
  });
}
