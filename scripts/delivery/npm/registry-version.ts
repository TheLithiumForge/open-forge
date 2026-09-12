import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";

export function registryVersionExists(root: string, name: string, version: string): boolean {
  const cli = process.env["npm_execpath"];
  assert.ok(cli, "Run this command through npm run.");
  const result = spawnSync(process.execPath, [cli, "view", `${name}@${version}`, "version", "--json", "--prefer-online"], {
    cwd: root,
    shell: false,
    encoding: "utf8",
  });
  if (result.error) throw result.error;
  assert.equal(result.signal, null, `Registry check interrupted for ${name}@${version}.`);
  return readRegistryVersion(result.status, result.stdout, name, version);
}

export function readRegistryVersion(status: number | null, output: string, name: string, version: string): boolean {
  let value: unknown;
  try {
    value = JSON.parse(output);
  } catch {
    throw new Error(`Registry check for ${name}@${version} returned invalid JSON (exit ${status}). No upload attempted.`);
  }
  if (status === 0 && value === version) return true;
  if (status !== null && status > 0 && typeof value === "object" && value !== null && "error" in value) {
    const error = value.error;
    if (typeof error === "object" && error !== null && "code" in error) {
      if (error.code === "E404") return false;
      throw new Error(`Registry check failed for ${name}@${version}: ${String(error.code)}. No upload attempted.`);
    }
  }
  throw new Error(`Unexpected registry response for ${name}@${version} (exit ${status}). No upload attempted.`);
}
