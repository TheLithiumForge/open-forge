import { readFile } from "node:fs/promises";
import { existsSync } from "node:fs";
import { join } from "node:path";
import { EvalPaths } from "../paths.ts";
import { ImageVersions, dockerAvailable } from "../execution/containers/docker-image.ts";
import { lockedImageArguments } from "../images/lock.ts";
import { signInStatus, loadLocalEnv } from "./auth.ts";
import { object } from "./validation.ts";

export const MinimumNode = 24;
export const ToolPackages = { claude: "@anthropic-ai/claude-code", codex: "@openai/codex", openForge: "@thelithiumforge/open-forge" } as const;
export interface DoctorCheck { readonly name: string; readonly passed: boolean; readonly detail: string }
export interface DoctorServices {
  readonly node: string;
  readonly docker: () => Promise<boolean>;
  readonly signIns: () => ReturnType<typeof signInStatus>;
  readonly digests: () => unknown;
  readonly version: (name: string) => Promise<string | null>;
}

const services: DoctorServices = {
  node: process.versions.node, docker: dockerAvailable, signIns: signInStatus, digests: lockedImageArguments,
  async version(name) {
    const value = object(JSON.parse(await readFile(join(EvalPaths.modules, name, "package.json"), "utf8")));
    const entries = typeof value["bin"] === "string" ? [value["bin"]] : Object.values(object(value["bin"]));
    if (entries.length === 0 || entries.some((entry) => typeof entry !== "string" || !existsSync(join(EvalPaths.modules, name, entry)))) return null;
    return typeof value["version"] === "string" ? value["version"] : null;
  },
};

export async function doctor(input: DoctorServices = services): Promise<DoctorCheck[]> {
  const checks: DoctorCheck[] = [{ name: "Node", passed: Number(input.node.split(".")[0]) >= MinimumNode, detail: input.node }];
  try { checks.push({ name: "Docker", passed: await input.docker(), detail: "Docker server" }); }
  catch { checks.push({ name: "Docker", passed: false, detail: "Docker server unavailable" }); }
  for (const [tool, name] of Object.entries(ToolPackages)) {
    const expected = ImageVersions[tool === "claude" ? "claude" : tool === "codex" ? "codex" : "openForge"];
    try { const actual = await input.version(name); checks.push({ name, passed: actual === expected, detail: actual ?? "Missing package" }); }
    catch { checks.push({ name, passed: false, detail: "Missing package" }); }
  }
  const signIns = input.signIns();
  checks.push({ name: "Claude sign-in", passed: signIns.claude, detail: "Configured sign-in or API key" }, { name: "Codex sign-in", passed: signIns.codex, detail: "Configured sign-in or API key" });
  try { input.digests(); checks.push({ name: "Image digests", passed: true, detail: "Locked base images" }); }
  catch { checks.push({ name: "Image digests", passed: false, detail: "Fill images/lock.json before building" }); }
  return checks;
}

export async function configuredDoctor(): Promise<DoctorCheck[]> { loadLocalEnv(); return doctor(); }
