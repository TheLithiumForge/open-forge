import { pathToFileURL } from "node:url";
import { chromium, type Browser } from "playwright";
import type { SuiteCheckInput, SuiteCheckOutput } from "../../suite-checks.types.ts";
import type { CheckOutcome } from "../../../grading/grading.types.ts";
import { CheckStatus } from "../../../history/statuses.ts";
import { deliveryChecks, stackCheck } from "./checks-delivery.ts";
import { keyboardCheck, phoneCheck } from "./checks-devices.ts";
import { largeBoardChecks } from "./checks-large.ts";
import { playChecks } from "./checks-play.ts";
import { scoreChecks } from "./checks-score.ts";
import { completeChecks, loadLayer, selection } from "./layer.ts";
import { takeShots } from "./screenshots.ts";
import { Device, openGame } from "./open-game.ts";

export interface CheckDependencies { readonly launch: () => Promise<Browser> }
const dependencies: CheckDependencies = { launch: () => chromium.launch() };

export async function runNeonPairsChecks(input: SuiteCheckInput, tools: CheckDependencies = dependencies): Promise<SuiteCheckOutput> {
  const layer = await loadLayer();
  const stack = stackCheck(input.project, input.build.typecheck);
  const checks: CheckOutcome[] = [stack];
  if (input.html === null) {
    const outcomes = layer.checks.map((check) => check.id === stack.id ? stack : {
      id: check.id, area: check.area, status: CheckStatus.blocked, evidence: "The failed build left no page to check.",
    });
    return { checks: outcomes, selection: selection(layer), performance: null, shots: [] };
  }
  const url = pathToFileURL(input.html).href;
  let browser: Browser | undefined;
  try {
    browser = await tools.launch();
    const first = await openGame(browser, url, Device.desktop);
    await first.context.close();
    checks.push(...await deliveryChecks(browser, url, true));
    checks.push(...await playChecks(browser, url, input.shotsFolder));
    checks.push(...await scoreChecks(browser, url));
    checks.push(...await phoneCheck(browser, url));
    checks.push(...await keyboardCheck(browser, url));
    const large = await largeBoardChecks(browser, url, first.firstRenderMs, input.shotsFolder);
    checks.push(...large.checks);
    return { checks: completeChecks(layer, checks), selection: selection(layer), performance: large.performance, shots: [...large.shots, ...await takeShots(browser, url, input.shotsFolder)] };
  } catch (error) {
    const cause = error instanceof Error ? error.message : String(error);
    return { checks: completeChecks(layer, checks, cause), selection: selection(layer), performance: null, shots: [] };
  } finally {
    await browser?.close().catch(() => undefined);
  }
}
