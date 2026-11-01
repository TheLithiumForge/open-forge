import { mkdir, readFile, writeFile } from "node:fs/promises";
import { runNeonPairsChecks } from "./neon-pairs.ts";
import { record } from "./layer.ts";
import type { BuildResult } from "../../../history/results.types.ts";
import { CheckContainer, CheckWorkerEnvironment } from "./container-paths.ts";

// Fixed container paths keep this entry point from opening host submissions.
if (import.meta.main) {
  if (process.platform !== "linux" || process.env["OPEN_FORGE_CHECK_WORKER"] !== CheckWorkerEnvironment.OPEN_FORGE_CHECK_WORKER) throw new Error("Run browser checks through the verifier container.");
  const input: unknown = JSON.parse(await readFile(CheckContainer.input, "utf8"));
  const build = record(input) ? input["build"] : null;
  if (!record(build) || typeof build["passed"] !== "boolean" || (build["typecheck"] !== null && typeof build["typecheck"] !== "boolean")) throw new Error("Invalid browser-check input.");
  const checked: BuildResult = { passed: build["passed"], typecheck: build["typecheck"], seconds: 0, packages: null, html: "index.html", bytes: null, log: "" };
  const shots = `${CheckContainer.output}/${CheckContainer.shots}`;
  await mkdir(shots, { recursive: true });
  const report = await runNeonPairsChecks({ project: CheckContainer.project, build: checked, html: CheckContainer.page, shotsFolder: shots });
  await writeFile(`${CheckContainer.output}/${CheckContainer.report}`, JSON.stringify(report));
}
