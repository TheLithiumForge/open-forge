import { cp, lstat, mkdir, mkdtemp, readFile, rm, writeFile } from "node:fs/promises";
import { dirname, join, resolve, sep } from "node:path";
import { tmpdir } from "node:os";
import type { SuiteCheckInput, SuiteCheckOutput } from "../../suite-checks.types.ts";
import { evalsRoot } from "../../../paths.ts";
import { copyOutPlain } from "../../../execution/containers/files.ts";
import { evalImageTag } from "../../../execution/containers/docker-image.ts";
import { requireDockerResult } from "../../../execution/containers/docker.ts";
import { ContainerStage, type StageOptions } from "../../../execution/containers/stage.ts";
import { ContainerPaths, ContainerUsers, StageKind } from "../../../execution/containers/settings.ts";
import { completeChecks, loadLayer, selection } from "./layer.ts";
import { stackCheck } from "./checks-delivery.ts";
import { parseCheckReport } from "./report.ts";
import { CheckContainer, CheckWorkerEnvironment } from "./container-paths.ts";
import { CheckStatus } from "../../../history/statuses.ts";
import { PNG } from "pngjs";

const TimeoutMs = 20 * 60_000;
const ReportByteLimit = 1024 * 1024;
const ImageByteLimit = 8 * 1024 * 1024;
const ImagePixelLimit = 16 * 1024 * 1024;
const HarnessPackages = ["playwright", "playwright-core", "pngjs"];

export async function runContainerChecks(input: SuiteCheckInput, options: StageOptions = input.stage ?? { timeoutMs: TimeoutMs }): Promise<SuiteCheckOutput> {
  const layer = await loadLayer();
  if (input.html === null) {
    const stack = stackCheck(input.project, input.build.typecheck);
    return { checks: layer.checks.map((check) => check.id === stack.id ? stack : { id: check.id, area: check.area, status: CheckStatus.blocked, evidence: "The failed build left no page to check." }), selection: selection(layer), performance: null, shots: [] };
  }
  const root = await mkdtemp(join(tmpdir(), "eval-browser-checks-"));
  const stage = new ContainerStage(options);
  try {
    const harness = join(root, "harness");
    const source = join(evalsRoot, "benchmarks", "neon-pairs");
    for (const folder of ["checks", "layers"]) {
      await cp(join(source, folder), join(harness, "benchmarks", "neon-pairs", folder), { recursive: true });
    }
    for (const file of ["history/statuses.ts", "execution/containers/settings.ts"]) {
      await mkdir(dirname(join(harness, file)), { recursive: true });
      await cp(join(evalsRoot, file), join(harness, file));
    }
    for (const name of HarnessPackages) await cp(join(evalsRoot, "node_modules", name), join(harness, "node_modules", name), { recursive: true });
    await writeFile(join(harness, "package.json"), JSON.stringify({ type: "module" }));
    const work = join(root, "work");
    await mkdir(work);
    await cp(input.html, join(work, "index.html"));
    try { await cp(join(input.project, "package.json"), join(work, "package.json")); }
    catch (error) {
      if (!(error instanceof Error && "code" in error && error.code === "ENOENT")) throw error;
      await writeFile(join(work, "package.json"), "{}");
    }
    await writeFile(join(work, "input.json"), JSON.stringify({ build: { ...input.build, html: "index.html", log: "" } }));
    await stage.open(evalImageTag(StageKind.verifier));
    await stage.copyIn(harness, CheckContainer.harness);
    await stage.copyIn(`${work}${sep}.`, ContainerPaths.work);
    await stage.prepare(CheckContainer.harness, ContainerPaths.work, ContainerPaths.output);
    await stage.offline();
    requireDockerResult(await stage.exec(["node", `${CheckContainer.harness}/${CheckContainer.worker}`], ContainerUsers.agent, CheckWorkerEnvironment));
    await stage.stop();
    const collected = join(root, "collected");
    await copyOutPlain(stage, ContainerPaths.output, collected, root);
    const reportPath = join(collected, CheckContainer.report);
    if (!(await lstat(reportPath)).isFile() || (await lstat(reportPath)).size > ReportByteLimit) throw new Error("Verifier report exceeds its file limit.");
    const report = parseCheckReport(JSON.parse(await readFile(reportPath, "utf8")), layer);
    await mkdir(input.shotsFolder, { recursive: true });
    for (const shot of report.shots) {
      const path = join(collected, CheckContainer.shots, shot);
      if (!(await lstat(path)).isFile() || (await lstat(path)).size > ImageByteLimit) throw new Error("Invalid verifier image file.");
      const png = await readFile(path);
      if (png.length < 24 || png.readUInt32BE(16) * png.readUInt32BE(20) > ImagePixelLimit) throw new Error("Verifier image exceeds its pixel limit.");
      PNG.sync.read(png);
      await writeFile(join(input.shotsFolder, shot), png);
    }
    return report;
  } catch (error) {
    return { checks: completeChecks(layer, [], error instanceof Error ? error.message : String(error)), selection: selection(layer), performance: null, shots: [] };
  } finally {
    try { await stage.remove(); }
    finally {
      if (dirname(resolve(root)) !== resolve(tmpdir())) throw new Error("Check cleanup escaped its temporary folder.");
      await rm(root, { recursive: true, force: true });
    }
  }
}
