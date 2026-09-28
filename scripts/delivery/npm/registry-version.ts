import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";

export function registryVersionExists(root: string, name: string, version: string, optionalDependencies?: Record<string, string>): boolean {
  const cli = process.env["npm_execpath"];
  assert.ok(cli, "Run this command through npm run.");
  const result = spawnSync(
    process.execPath,
    [cli, "view", `${name}@${version}`, "version", ...(optionalDependencies === undefined ? [] : ["optionalDependencies"]), "--json", "--prefer-online"],
    {
      cwd: root,
      shell: false,
      encoding: "utf8",
    },
  );
  if (result.error) throw result.error;
  assert.equal(result.signal, null, `Registry check interrupted for ${name}@${version}.`);
  return readRegistryVersion(result.status, result.stdout, name, version, optionalDependencies);
}

export function readRegistryVersion(status: number | null, output: string, name: string, version: string, optionalDependencies?: Record<string, string>): boolean {
  let value: unknown;
  try {
    value = JSON.parse(output);
  } catch {
    throw new Error(`Registry check for ${name}@${version} returned invalid JSON (exit ${status}). No upload attempted.`);
  }
  if (status === 0) {
    if (optionalDependencies === undefined && value === version) return true;
    if (optionalDependencies !== undefined && typeof value === "object" && value !== null && "version" in value && value.version === version) {
      assert.ok("optionalDependencies" in value, "Published wrapper is missing its target dependencies.");
      assert.deepEqual(value.optionalDependencies, optionalDependencies, "Published wrapper targets differ. Use a new version; an existing npm version cannot be changed.");
      return true;
    }
  }
  if (status !== null && status > 0 && typeof value === "object" && value !== null && "error" in value) {
    const error = value.error;
    if (typeof error === "object" && error !== null && "code" in error) {
      if (error.code === "E404") return false;
      throw new Error(`Registry check failed for ${name}@${version}: ${String(error.code)}. No upload attempted.`);
    }
  }
  throw new Error(`Unexpected registry response for ${name}@${version} (exit ${status}). No upload attempted.`);
}

// Until a package has a stable release, its newest prerelease also becomes
// `latest`, because the npm package page shows the `latest` version.
export function registryHasStableVersion(root: string, name: string): boolean {
  const cli = process.env["npm_execpath"];
  assert.ok(cli, "Run this command through npm run.");
  const result = spawnSync(process.execPath, [cli, "view", name, "versions", "--json", "--prefer-online"], { cwd: root, shell: false, encoding: "utf8" });
  if (result.error) throw result.error;
  assert.equal(result.signal, null, `Registry check interrupted for ${name}.`);
  return readHasStableVersion(result.status, result.stdout, name);
}

export function readHasStableVersion(status: number | null, output: string, name: string): boolean {
  let value: unknown;
  try {
    value = JSON.parse(output);
  } catch {
    throw new Error(`Registry version list for ${name} returned invalid JSON (exit ${status}).`);
  }
  if (status === 0 && (typeof value === "string" || Array.isArray(value))) {
    const versions: unknown[] = Array.isArray(value) ? value : [value];
    return versions.some((version) => typeof version === "string" && !isPrerelease(version));
  }
  if (status !== null && status > 0 && typeof value === "object" && value !== null && "error" in value) {
    const error = value.error;
    if (typeof error === "object" && error !== null && "code" in error && error.code === "E404") return false;
  }
  throw new Error(`Unexpected registry version list for ${name} (exit ${status}).`);
}

export function isPrerelease(version: string): boolean {
  return (version.split("+")[0] ?? "").includes("-");
}
