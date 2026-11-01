import { mkdtemp, mkdir, readdir, readFile, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { dirname, join, relative, resolve } from "node:path";
import { loadLocalEnv } from "../../config/auth.ts";
import { Tool } from "../../config/choices.ts";
import type { ModelChoice } from "../../config/config.types.ts";
import { waitForProcess, type ProcessResult } from "../process.ts";
import type { RunSpace } from "./executor.ts";
import { copyProjectIn, ImageMediaType, projectFileAllowed, readAssessorImages, type AssessorImage } from "./files.ts";
import { evalImageTag } from "./docker-image.ts";
import { ContainerPaths, StageKind } from "./settings.ts";
import { ContainerStage, type StageOptions } from "./stage.ts";
import { ToolHome, toolEnvironment, type ToolHomeOptions } from "./tool-home.ts";

export const AssessorEvidenceLimit = 1024 * 1024;
export const AssessorTools = { off: "off", restricted: "restricted" } as const;
export const CodexAssessorSettings = ['web_search="disabled"', 'approval_policy="never"', "features.shell_tool=false", "features.multi_agent=false", "features.apps=false", "features.plugins=false", "features.browser_use=false", "features.computer_use=false", "features.image_generation=false", "features.code_mode_host=false"] as const;
export interface AssessorResult extends ProcessResult { readonly tools?: (typeof AssessorTools)[keyof typeof AssessorTools] }
export interface AssessorOptions extends StageOptions, ToolHomeOptions { readonly images?: readonly string[] }
export const AssessorImagePrefix = "image-";
export const AssessorImageSmoke = { timeoutMs: 5 * 60_000, prompt: "Describe what you see in the attached image. Use no tools." } as const;

export function assessorCommand(choice: ModelChoice, images: readonly string[] = []): string[] {
  if (choice.tool === Tool.mock) throw new Error("The assessor needs a model provider.");
  const { model, effort } = choice;
  if (choice.tool === Tool.codex) return [Tool.codex, "exec", "--json", "--skip-git-repo-check", "--sandbox", "read-only",
    ...CodexAssessorSettings.flatMap((setting) => ["-c", setting]), ...(model === null ? [] : ["-m", model]),
    ...(effort === null ? [] : ["-c", `model_reasoning_effort=${JSON.stringify(effort)}`]), ...images.flatMap((path) => ["--image", path]), "-"];
  // Images travel as content blocks, and Claude Code takes stream-json input only with stream-json output.
  const formats = images.length ? ["--input-format", "stream-json", "--output-format", "stream-json", "--verbose"] : ["--output-format", "json"];
  return [Tool.claude, "-p", ...formats, "--strict-mcp-config", "--mcp-config", '{"mcpServers":{}}', "--tools", "", "--max-turns", "1",
    ...(model === null ? [] : ["--model", model]), ...(effort === null ? [] : ["--effort", effort])];
}

async function evidence(root: string, images: readonly string[], folder = root, files: Record<string, string> = {}): Promise<Record<string, string>> {
  for (const entry of await readdir(folder, { withFileTypes: true })) {
    const path = join(folder, entry.name);
    if (!projectFileAllowed(path, true) || images.some((image) => resolve(image) === resolve(path))) continue;
    if (entry.isDirectory()) await evidence(root, images, path, files);
    if (entry.isFile()) files[relative(root, path)] = await readFile(path, "utf8");
    if (Buffer.byteLength(JSON.stringify(files)) > AssessorEvidenceLimit) throw new Error("Assessor evidence exceeds its input limit.");
  }
  return files;
}

/** A stream-json reply ends with one result event, the same object plain json output prints, so readers stay unchanged. */
export function finalStreamResult(stdout: string): string {
  const results = stdout.split("\n").filter((line) => line.trim() !== "").filter((line) => {
    try {
      return (JSON.parse(line) as { type?: unknown }).type === "result"; // Each line is one stream event.
    } catch {
      return false;
    }
  });
  return results.at(-1) ?? stdout;
}

export function assessorInput(text: string, images: readonly AssessorImage[]): string {
  return `${JSON.stringify({ type: "user", parent_tool_use_id: null, message: { role: "user", content: [
    { type: "text", text }, ...images.flatMap((image) => [
      { type: "text", text: `Image: ${image.name}` },
      { type: "image", source: { type: "base64", media_type: image.mediaType, data: image.bytes.toString("base64") } },
    ]),
  ] } })}\n`;
}

export async function runAssessorStage(space: RunSpace, choice: ModelChoice, prompt: string, options: AssessorOptions): Promise<AssessorResult> {
  assessorCommand(choice);
  const images = await readAssessorImages(space.root, options.images ?? []);
  const paths = images.map((image, index) => `${ContainerPaths.work}/${AssessorImagePrefix}${index}${image.mediaType === ImageMediaType.png ? ".png" : ".jpg"}`);
  const command = assessorCommand(choice, paths);
  const text = `${prompt}\n\n${JSON.stringify(await evidence(space.work, options.images ?? []))}`;
  const input = choice.tool === Tool.claude && images.length ? assessorInput(text, images) : text;
  const stage = new ContainerStage(options);
  const home = new ToolHome(choice.tool, space, options);
  let ready = false;
  try {
    await stage.open(evalImageTag(StageKind.assessor));
    // Codex gets only attached images in its working folder. Source evidence stays in stdin.
    if (choice.tool === Tool.claude) await copyProjectIn(stage, space, true);
    if (choice.tool === Tool.codex && images.length) {
      const staging = await mkdtemp(join(space.root, "images-"));
      try {
        for (const [index, image] of images.entries()) {
          const path = paths[index];
          if (path === undefined) throw new Error("Missing assessor image path.");
          const file = join(staging, `${AssessorImagePrefix}${index}`);
          await writeFile(file, image.bytes);
          await stage.copyIn(file, path);
        }
      } finally { await rm(staging, { recursive: true, force: true }); }
    }
    await home.copyIn(stage);
    await stage.prepare(ContainerPaths.work, ContainerPaths.home);
    ready = true;
    const child = stage.spawn(command, toolEnvironment(choice.tool), home.auth);
    const result = await waitForProcess(child, { timeoutMs: options.timeoutMs, input });
    const stdout = choice.tool === Tool.claude && images.length ? finalStreamResult(result.stdout) : result.stdout;
    return { ...result, stdout, tools: choice.tool === Tool.codex ? AssessorTools.restricted : AssessorTools.off };
  } finally {
    try {
      if (ready) {
        await stage.stop();
        await home.sync(stage);
      }
    } finally { home.dispose(); await stage.remove(); }
  }
}

async function imageSmoke(path: string): Promise<void> {
  const images = await readAssessorImages(dirname(resolve(path)), [resolve(path)]);
  const root = await mkdtemp(join(tmpdir(), "eval-assessor-image-"));
  try {
    const work = join(root, "project");
    await mkdir(work);
    const paths: string[] = [];
    for (const image of images) {
      const file = join(root, image.name);
      await writeFile(file, image.bytes);
      paths.push(file);
    }
    loadLocalEnv();
    for (const tool of [Tool.claude, Tool.codex]) {
      const space: RunSpace = { root, work, home: join(root, tool), project: "image-smoke" };
      const result = await runAssessorStage(space, { tool, model: null, effort: null }, AssessorImageSmoke.prompt, { images: paths, timeoutMs: AssessorImageSmoke.timeoutMs });
      if (result.code !== 0 || result.timedOut) throw new Error(`${tool} image smoke failed (exit ${result.code}): ${result.stderr || result.stdout.slice(-2000)}`);
      console.log(`${tool} (${result.tools}): ${result.stdout}`);
    }
  } finally { await rm(root, { recursive: true, force: true }); }
}

if (import.meta.main) {
  const [path, extra] = process.argv.slice(2);
  if (!path || extra) throw new Error("Usage: node src/evals/execution/containers/assessor.ts <image.png>");
  await imageSmoke(path);
}
