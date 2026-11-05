import { existsSync } from "node:fs";
import { cp, lstat, mkdir, mkdtemp, readdir, readFile, rm } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join, relative, sep } from "node:path";
import { fileURLToPath } from "node:url";
import { requireDockerResult } from "../../execution/containers/docker.ts";
import { evalImageTag } from "../../execution/containers/docker-image.ts";
import { copyOutPlain } from "../../execution/containers/files.ts";
import { ContainerUsers, StageKind } from "../../execution/containers/settings.ts";
import { ContainerStage, type StageOptions } from "../../execution/containers/stage.ts";
import { evalsRoot } from "../../paths.ts";
import { ordinaryPath, writeNew } from "../../materialize/files.ts";
import { BrownfieldContainer, BrownfieldEnvironment } from "../brownfield/container.ts";
import type { Suite } from "../../benchmarks/suite.types.ts";

export const PlaythroughFiles = { folder: "playthrough", trace: "trace.json" } as const;
const HarnessPackages = ["playwright", "playwright-core"] as const;
const PageByteLimit = 12 * 1024 * 1024;
export const PlaythroughTimeoutMs = 15 * 60_000;

export interface Playthrough { readonly folder: string; readonly trace: string; readonly images: readonly string[] }

/** A case may have a script that plays its built game for the judges. */
export const casePlaythrough = (id: string): string => fileURLToPath(new URL(`./case-playthroughs/${id}.ts`, import.meta.url));

export async function savedPlaythrough(folder: string): Promise<Playthrough | null> {
  const trace = join(folder, PlaythroughFiles.trace);
  if (!existsSync(trace)) return null;
  const images = (await readdir(folder)).filter((name) => name.endsWith(".png")).sort().map((name) => join(folder, name));
  return { folder, trace, images };
}

/** Plays the built game in the verifier, offline, since agents wrote it. The script and the checks it borrows keep their places under the harness. */
export async function capturePlaythrough(suite: Pick<Suite, "id" | "brownfield">, html: string, target: string, options: Omit<StageOptions, "timeoutMs"> = {}): Promise<Playthrough> {
  const item = suite.brownfield?.case;
  if (item === undefined) throw new Error("Only a brownfield case has a playthrough.");
  const script = casePlaythrough(suite.id);
  const scratch = await mkdtemp(join(tmpdir(), "eval-playthrough-"));
  const stage = new ContainerStage({ ...options, timeoutMs: PlaythroughTimeoutMs });
  try {
    await ordinaryPath(html);
    const info = await lstat(html);
    if (!info.isFile() || info.size > PageByteLimit) throw new Error("The built page must be a bounded plain file.");
    const harness = join(scratch, "harness");
    const inHarness = (path: string) => relative(evalsRoot, path).split(sep).join("/");
    const caseFolder = inHarness(item.root);
    for (const file of item.inputs) if (file.path.startsWith(`${item.checks}/`)) await writeNew(harness, `${caseFolder}/${file.path}`, await readFile(join(item.root, file.path)));
    await writeNew(harness, inHarness(script), await readFile(script));
    for (const name of HarnessPackages) await cp(join(evalsRoot, "node_modules", name), join(harness, "node_modules", name), { recursive: true });
    await writeNew(harness, "package.json", JSON.stringify({ type: "module" }));
    await writeNew(harness, BrownfieldContainer.page, await readFile(html));
    await stage.open(evalImageTag(StageKind.verifier));
    for (const folder of [BrownfieldContainer.harness, BrownfieldContainer.evidence]) requireDockerResult(await stage.exec(["mkdir", "-m", "700", folder], ContainerUsers.agent));
    await stage.copyIn(`${harness}${sep}.`, BrownfieldContainer.harness);
    await stage.offline();
    requireDockerResult(await stage.exec(["node", `${BrownfieldContainer.harness}/${inHarness(script)}`, `${BrownfieldContainer.harness}/${BrownfieldContainer.page}`, BrownfieldContainer.evidence], ContainerUsers.agent, BrownfieldEnvironment));
    await stage.stop();
    await mkdir(target, { recursive: true });
    const collected = join(scratch, "collected");
    await copyOutPlain(stage, BrownfieldContainer.evidence, collected, scratch);
    await cp(collected, target, { recursive: true });
    const saved = await savedPlaythrough(target);
    if (saved === null) throw new Error("The playthrough left no trace.");
    return saved;
  } finally {
    try { await stage.remove(); }
    finally { await rm(scratch, { recursive: true, force: true }); }
  }
}
