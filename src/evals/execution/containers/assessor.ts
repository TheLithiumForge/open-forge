import { readdir, readFile } from "node:fs/promises";
import { join, relative } from "node:path";
import { Tool } from "../../config/choices.ts";
import type { ModelChoice } from "../../config/config.types.ts";
import { waitForProcess, type ProcessResult } from "../process.ts";
import type { RunSpace } from "./executor.ts";
import { copyProjectIn, projectFileAllowed } from "./files.ts";
import { evalImageTag } from "./docker-image.ts";
import { ContainerPaths, StageKind } from "./settings.ts";
import { ContainerStage, type StageOptions } from "./stage.ts";
import { ToolHome, toolEnvironment, type ToolHomeOptions } from "./tool-home.ts";

export const AssessorEvidenceLimit = 1024 * 1024;
export const AssessorTools = { off: "off", restricted: "restricted" } as const;
export const CodexAssessorSettings = ['web_search="disabled"', 'approval_policy="never"', "features.shell_tool=false", "features.multi_agent=false", "features.apps=false", "features.plugins=false", "features.browser_use=false", "features.computer_use=false", "features.image_generation=false", "features.code_mode_host=false"] as const;
export interface AssessorResult extends ProcessResult { readonly tools?: (typeof AssessorTools)[keyof typeof AssessorTools] }

export function assessorCommand(choice: ModelChoice): string[] {
  if (choice.tool === Tool.mock) throw new Error("The assessor needs a model provider.");
  const { model, effort } = choice;
  if (choice.tool === Tool.codex) return [Tool.codex, "exec", "--json", "--skip-git-repo-check", "--sandbox", "read-only",
    ...CodexAssessorSettings.flatMap((setting) => ["-c", setting]), ...(model === null ? [] : ["-m", model]),
    ...(effort === null ? [] : ["-c", `model_reasoning_effort=${JSON.stringify(effort)}`]), "-"];
  return [Tool.claude, "-p", "--output-format", "json", "--strict-mcp-config", "--mcp-config", '{"mcpServers":{}}', "--tools", "", "--max-turns", "1",
    ...(model === null ? [] : ["--model", model]), ...(effort === null ? [] : ["--effort", effort])];
}

async function evidence(root: string, folder = root, files: Record<string, string> = {}): Promise<Record<string, string>> {
  for (const entry of await readdir(folder, { withFileTypes: true })) {
    const path = join(folder, entry.name);
    if (!projectFileAllowed(path, true)) continue;
    if (entry.isDirectory()) await evidence(root, path, files);
    if (entry.isFile()) files[relative(root, path)] = await readFile(path, "utf8");
    if (Buffer.byteLength(JSON.stringify(files)) > AssessorEvidenceLimit) throw new Error("Assessor evidence exceeds its input limit.");
  }
  return files;
}

export async function runAssessorStage(space: RunSpace, choice: ModelChoice, prompt: string, options: StageOptions & ToolHomeOptions): Promise<AssessorResult> {
  const command = assessorCommand(choice);
  const input = `${prompt}\n\n${JSON.stringify(await evidence(space.work))}`;
  const stage = new ContainerStage(options);
  const home = new ToolHome(choice.tool, space, options);
  let ready = false;
  try {
    await stage.open(evalImageTag(StageKind.assessor));
    // Codex has restricted tools. Its working folder stays empty and all evidence goes through stdin.
    if (choice.tool === Tool.claude) await copyProjectIn(stage, space, true);
    await home.copyIn(stage);
    await stage.prepare(ContainerPaths.work, ContainerPaths.home);
    ready = true;
    const child = stage.spawn(command, toolEnvironment(choice.tool), home.auth);
    const result = await waitForProcess(child, { timeoutMs: options.timeoutMs, input });
    return { ...result, tools: choice.tool === Tool.codex ? AssessorTools.restricted : AssessorTools.off };
  } finally {
    try {
      if (ready) {
        await stage.stop();
        await home.sync(stage);
      }
    } finally { home.dispose(); await stage.remove(); }
  }
}
