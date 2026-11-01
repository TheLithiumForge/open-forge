import { execFile } from "node:child_process";
import { readFile } from "node:fs/promises";
import { promisify } from "node:util";
import { chromium } from "playwright";
import { object } from "./requirements.ts";

export const ToolchainClause = "T03.4";
export const NamedToolchain = { node: "26.12.1", npm: "11.20.0", playwright: "1.64.1", chromium: "156.0.8078.4" } as const;
export type Toolchain = { readonly [K in keyof typeof NamedToolchain]: string };
export const ToolchainWorker = "benchmarks/neon-pairs/checks/spec-layers/toolchain.ts";

export function parseToolchain(text: string): Toolchain {
  if (text.length > 1024) throw new Error("Verifier toolchain output exceeds its limit.");
  const value: unknown = JSON.parse(text);
  if (!object(value) || !Object.keys(NamedToolchain).every((name) => typeof value[name] === "string" && /^\d+\.\d+\.\d+(?:\.\d+)?$/.test(value[name]))) throw new Error("Invalid verifier toolchain versions.");
  return { node: String(value["node"]), npm: String(value["npm"]), playwright: String(value["playwright"]), chromium: String(value["chromium"]) };
}

export async function verifierToolchain(): Promise<Toolchain> {
  const npm = await promisify(execFile)("npm", ["--version"], { timeout: 10_000, maxBuffer: 1024 });
  const manifest: unknown = JSON.parse(await readFile(new URL(import.meta.resolve("playwright/package.json")), "utf8"));
  if (!object(manifest) || typeof manifest["version"] !== "string") throw new Error("No verifier Playwright version.");
  const browser = await chromium.launch();
  try { return { node: process.versions.node, npm: npm.stdout.trim(), playwright: manifest["version"], chromium: browser.version() }; }
  finally { await browser.close(); }
}

if (import.meta.main) process.stdout.write(JSON.stringify(await verifierToolchain()));
