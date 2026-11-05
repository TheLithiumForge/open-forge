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

/** What a setup adds to a plain install: Extensions, files it places once their routes exist, and a full index over them. */
export interface FrameworkAdditions {
  readonly extensions?: readonly string[];
  readonly place?: () => Promise<void>;
  /** Set when the setup places its documents in the categories itself, so nothing routes an outside folder. Names the categories the preset lacks, for the loader to list. */
  readonly customRoots?: readonly string[];
}

export function frameworkRequests(preset: ForgePreset = ForgePreset.minimal, extensions: readonly string[] = [], customRoots?: readonly string[]): readonly (readonly string[])[] {
  const install = ["install", "--preset", preset, "--frontmatter", "scoped", "--automatic"];
  if (customRoots !== undefined) return [install, ...customRoots.map((root) => ["route", "include", root]), ["index"]];
  if (extensions.length === 0) return [install, ...ForgeRequests.slice(1)];
  // Extension routes exist only after their install, and a full index lists what lands on them.
  return [install, ["extension", "install", ...extensions, "--automatic"], ...ForgeRequests.slice(1), ["index"]];
}

const ForgeErrorChars = 1500;
export async function installFramework(destination: string, run: ForgeRunner, preset: ForgePreset = ForgePreset.minimal, additions: FrameworkAdditions = {}): Promise<ForgeReceipt[]> {
  const version = await run([ForgeFlags.version], destination);
  if (version.code !== 0 || version.stdout.trim() !== FrameworkVersion) throw new Error(`Expected Open Forge ${FrameworkVersion}.`);
  const receipts: ForgeReceipt[] = [];
  const requests = frameworkRequests(preset, additions.extensions, additions.customRoots);
  // Placed files need the installed categories, and a custom category must exist before the loader can list it.
  const routing = additions.customRoots !== undefined ? 1 : requests.indexOf(requests.find((request) => request[0] === ForgeRequests[1][0]) ?? []);
  for (const [index, request] of requests.entries()) {
    if (index === routing) await additions.place?.();
    for (const preview of [true, false]) {
      const args = [...request, ...(preview ? [ForgeFlags.dryRun] : []), ForgeFlags.format, ForgeFlags.json, ForgeFlags.detail, ForgeFlags.full];
      const result = await run(args, destination);
      const envelope: unknown = JSON.parse(result.stdout);
      const status = isRecord(envelope) ? envelope["status"] ?? (isRecord(envelope["result"]) ? envelope["result"]["status"] : undefined) : undefined;
      if (result.code !== 0 || status !== ForgeStatus.completed) throw new Error(`Open Forge stopped for ${request.join(" ")}: ${result.stdout.slice(0, ForgeErrorChars)}`);
      receipts.push({ args, status });
    }
  }
  return receipts;
}
