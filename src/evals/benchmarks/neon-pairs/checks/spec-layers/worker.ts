import { mkdir, readFile, writeFile } from "node:fs/promises";
import { join } from "node:path";
import type { SuiteCheckInput } from "../../../suite-checks.types.ts";
import { runNeonPairsChecks } from "../neon-pairs.ts";
import { definition } from "./catalog.ts";
import { browserChecks } from "./browser.ts";
import { layerId, loadRequirements, Method, object } from "./requirements.ts";
import { Evidence } from "./outcomes.ts";
import { sourceFiles } from "./source.ts";
import { staticChecks } from "./static-checks.ts";
import { faceFormulaChecks } from "./numeric.ts";
import { SpecLayerContainer, SpecLayerEnvironment } from "./paths.ts";

export async function worker(): Promise<void> {
  if (process.platform !== "linux" || process.env["OPEN_FORGE_LATER_WORKER"] !== SpecLayerEnvironment.OPEN_FORGE_LATER_WORKER || process.getuid?.() !== 10000) throw new Error("Browser checks require the isolated checker user.");
  const input: unknown = JSON.parse(await readFile(SpecLayerContainer.input, "utf8"));
  if (!object(input) || !object(input["build"])) throw new Error("Invalid browser worker input.");
  const layer = layerId(input["layer"]), declared = await definition(layer, await loadRequirements()), evidence = new Evidence();
  const shots = join(SpecLayerContainer.evidence, SpecLayerContainer.shots);
  await mkdir(shots, { recursive: true });
  const inheritedInput: SuiteCheckInput = { project: "/work", html: SpecLayerContainer.page, shotsFolder: shots,
    build: { passed: true, typecheck: input["build"]["typecheck"] === true, seconds: 0, packages: null, html: "dist/index.html", bytes: null, log: "" } };
  const inherited = await runNeonPairsChecks(inheritedInput);
  try {
    const files = await sourceFiles("/work");
    staticChecks(files, evidence, layer === "L3");
    if (layer === "L3") faceFormulaChecks(files.filter((file) => file.path.startsWith("src/") && !file.path.includes("__tests__") && !file.path.includes(".test.")), evidence);
  } catch (error) {
    for (const assertion of declared.checks.filter((check) => check.method === Method.static).flatMap((check) => check.assertions)) {
      if (!evidence.seen.has(assertion.clause)) evidence.record(assertion.clause, "error", `Expected ${assertion.text}. Observed source-checker error: ${error instanceof Error ? error.message : String(error)}`);
    }
  }
  try { await browserChecks(SpecLayerContainer.page, declared, evidence); }
  catch (error) {
    for (const assertion of declared.checks.filter((check) => check.method === Method.browser).flatMap((check) => check.assertions)) {
      if (!evidence.seen.has(assertion.clause)) evidence.record(assertion.clause, "error", `Expected ${assertion.text}. Observed browser-checker error: ${error instanceof Error ? error.message : String(error)}`);
    }
  }
  const corrected = { ...inherited, checks: inherited.checks.map((check) => evidence.inherited.get(check.id) ?? check) };
  await writeFile(join(SpecLayerContainer.evidence, SpecLayerContainer.report), JSON.stringify({ schemaVersion: 1, layer, assertions: evidence.outcomes, inherited: corrected }));
}
if (import.meta.main) await worker();
