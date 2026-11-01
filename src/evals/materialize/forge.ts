import { readFile } from "node:fs/promises";
import { dirname, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { isRecord } from "./frontmatter.ts";
import { runCommand, type CommandResult, type CommandRunner } from "./runner.ts";

export const FrameworkVersion = "0.9.0-beta.18";
export const ForgeStatus = { completed: "completed" } as const;
export const ForgeFlags = { dryRun: "--dry-run", format: "--format", json: "json", version: "--version", detail: "--detail", full: "full" } as const;
export const ForgePreset = { minimal: "minimal", essentials: "essentials", fullCore: "full-core" } as const;
export type ForgePreset = (typeof ForgePreset)[keyof typeof ForgePreset];
export const ForgeRequests = [
  ["install", "--preset", ForgePreset.minimal, "--frontmatter", "scoped", "--automatic"],
  ["route", "include", "./docs", "--tag", "LoadNow"],
  ["index", "./docs/_docs.md"],
] as const;
export type ForgeRunner = (args: readonly string[], cwd: string) => Promise<CommandResult>;
export interface ForgeReceipt { readonly args: readonly string[]; readonly status: string }

export function createForgeRunner(run: CommandRunner = runCommand): ForgeRunner {
  return async (args, cwd) => {
    const manifestPath = fileURLToPath(import.meta.resolve("@thelithiumforge/open-forge/package.json"));
    const manifest: unknown = JSON.parse(await readFile(manifestPath, "utf8"));
    if (!isRecord(manifest) || !isRecord(manifest["bin"]) || typeof manifest["bin"]["open-forge"] !== "string") throw new Error("Open Forge package has no CLI entry.");
    return run(process.execPath, [resolve(dirname(manifestPath), manifest["bin"]["open-forge"]), ...args], cwd);
  };
}

export function frameworkRequests(preset: ForgePreset = ForgePreset.minimal): readonly (readonly string[])[] {
  return [["install", "--preset", preset, "--frontmatter", "scoped", "--automatic"], ...ForgeRequests.slice(1)];
}

export async function installFramework(destination: string, run: ForgeRunner, preset: ForgePreset = ForgePreset.minimal): Promise<ForgeReceipt[]> {
  const version = await run([ForgeFlags.version], destination);
  if (version.code !== 0 || version.stdout.trim() !== FrameworkVersion) throw new Error(`Expected Open Forge ${FrameworkVersion}.`);
  const receipts: ForgeReceipt[] = [];
  for (const request of frameworkRequests(preset)) {
    for (const preview of [true, false]) {
      const args = [...request, ...(preview ? [ForgeFlags.dryRun] : []), ForgeFlags.format, ForgeFlags.json, ForgeFlags.detail, ForgeFlags.full];
      const result = await run(args, destination);
      const envelope: unknown = JSON.parse(result.stdout);
      const status = isRecord(envelope) ? envelope["status"] ?? (isRecord(envelope["result"]) ? envelope["result"]["status"] : undefined) : undefined;
      if (result.code !== 0 || status !== ForgeStatus.completed) throw new Error(`Open Forge stopped for ${request.join(" ")}.`);
      receipts.push({ args, status });
    }
  }
  return receipts;
}
