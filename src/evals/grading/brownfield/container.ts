import { cp, lstat, mkdir, mkdtemp, readFile, rm, writeFile } from "node:fs/promises";
import { join, sep } from "node:path";
import { tmpdir } from "node:os";
import { evalsRoot } from "../../paths.ts";
import { ContainerStage } from "../../execution/containers/stage.ts";
import { ContainerPaths, ContainerUsers, StageKind } from "../../execution/containers/settings.ts";
import { copyOutPlain, copyProjectIn } from "../../execution/containers/files.ts";
import { requireDockerResult } from "../../execution/containers/docker.ts";
import { evalImageTag } from "../../execution/containers/docker-image.ts";
import type { SuiteCheckInput } from "../../benchmarks/suite-checks.types.ts";
import type { BrownfieldCase } from "../../materialize/brownfield-case.ts";
import { ordinaryPath, writeNew } from "../../materialize/files.ts";
import { ChangeCheckFiles, ChangeCheckLimits, parseChangeReport, type ChangeCheck } from "./change-checks.ts";
import type { CheckOutcome } from "../grading.types.ts";

export const BrownfieldContainer = { harness: "/home/agent/brownfield-harness", evidence: "/home/agent/brownfield-evidence", input: "input.json", page: "index.html" } as const;
export const BrownfieldEnvironment = { HOME: "/home/agent", PLAYWRIGHT_BROWSERS_PATH: "/ms-playwright", OPEN_FORGE_BROWNFIELD_WORKER: "1" } as const;
const HarnessPackages = ["playwright", "playwright-core"] as const;
const PageByteLimit = 12 * 1024 * 1024;
const ChangeTimeoutMs = 10 * 60_000;

export async function runChangeChecks(item: BrownfieldCase, declared: readonly ChangeCheck[], input: SuiteCheckInput): Promise<CheckOutcome[]> {
  const scratch = await mkdtemp(join(tmpdir(), "eval-brownfield-checks-"));
  const stage = new ContainerStage(input.stage ?? { timeoutMs: ChangeTimeoutMs });
  try {
    const harness = join(scratch, "harness");
    for (const file of item.inputs) {
      const bytes = await readFile(join(item.root, file.path));
      await writeNew(harness, `case/${file.path}`, bytes);
    }
    for (const name of HarnessPackages) await cp(join(evalsRoot, "node_modules", name), join(harness, "node_modules", name), { recursive: true });
    await writeNew(harness, "package.json", JSON.stringify({ type: "module" }));
    if (input.html !== null) {
      await ordinaryPath(input.html);
      const info = await lstat(input.html);
      if (!info.isFile() || info.nlink !== 1 || info.size > PageByteLimit) throw new Error("The built page must be a bounded plain file.");
      await writeNew(harness, BrownfieldContainer.page, await readFile(input.html));
    }
    const report = `${BrownfieldContainer.evidence}/${ChangeCheckFiles.report}`;
    await writeNew(harness, BrownfieldContainer.input, JSON.stringify({ schemaVersion: 1, project: ContainerPaths.work,
      html: input.html === null ? null : `${BrownfieldContainer.harness}/${BrownfieldContainer.page}`, report, checks: declared, changeType: item.changeType }));
    await stage.open(evalImageTag(StageKind.verifier));
    await copyProjectIn(stage, { root: scratch, work: input.project, home: join(scratch, "home"), project: "neon-pairs" }, true);
    await stage.prepare(ContainerPaths.work);
    for (const folder of [BrownfieldContainer.harness, BrownfieldContainer.evidence]) requireDockerResult(await stage.exec(["mkdir", "-m", "700", folder], ContainerUsers.agent));
    await stage.copyIn(`${harness}${sep}.`, BrownfieldContainer.harness);
    await stage.offline();
    const worker = `${BrownfieldContainer.harness}/case/${item.checks}/${ChangeCheckFiles.worker}`;
    requireDockerResult(await stage.exec(["node", worker, `${BrownfieldContainer.harness}/${BrownfieldContainer.input}`], ContainerUsers.agent, BrownfieldEnvironment));
    await stage.stop();
    const collected = join(scratch, "collected");
    await copyOutPlain(stage, BrownfieldContainer.evidence, collected, scratch);
    const path = join(collected, ChangeCheckFiles.report);
    const stat = await lstat(path);
    if (!stat.isFile() || stat.isSymbolicLink() || stat.size > ChangeCheckLimits.reportBytes) throw new Error("The change report must be a bounded plain file.");
    const outcomes = parseChangeReport(JSON.parse(await readFile(path, "utf8")), declared);
    await mkdir(input.shotsFolder, { recursive: true });
    await writeFile(join(input.shotsFolder, "brownfield-checks.json"), JSON.stringify(outcomes));
    return outcomes;
  } finally {
    try { await stage.remove(); }
    finally { await rm(scratch, { recursive: true, force: true }); }
  }
}
