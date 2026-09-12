import assert from "node:assert/strict";
import { readFileSync, writeFileSync } from "node:fs";
import { join } from "node:path";
import { fileURLToPath } from "node:url";
import { PlatformPackages } from "../package-managers/npm/package-model.ts";
import { validateVersion } from "./version.ts";

export function readPackage(path: string): Record<string, unknown> {
  const value: unknown = JSON.parse(readFileSync(path, "utf8"));
  assert.ok(typeof value === "object" && value !== null && !Array.isArray(value));
  return Object.fromEntries(Object.entries(value));
}

export function committedVersion(root: string): string {
  const version = readPackage(join(root, "package.json"))["version"];
  assert.ok(typeof version === "string", "Root package.json must own the version.");
  return validateVersion(version);
}

export function synchronizeVersion(root: string): void {
  const version = committedVersion(root);
  const propertiesPath = join(root, "Directory.Build.props");
  let properties = readFileSync(propertiesPath, "utf8");
  for (const name of ["OpenForgeCliVersion", "OpenForgeCliInformationalVersion"]) {
    const element = new RegExp(`<${name}>[^<]*</${name}>`, "u");
    assert.ok(element.test(properties), `Missing ${name} version property.`);
    properties = properties.replace(element, `<${name}>${version}</${name}>`);
  }
  const directories = [
    "main",
    ...Object.values(PlatformPackages).map((platform) => `${platform.nodePlatform === "darwin" ? "darwin" : platform.runtime.split("-")[0]}-${platform.nodeArchitecture}`),
  ];
  for (const directory of directories) {
    const path = join(root, "scripts/package-managers/npm", directory, "package.json");
    const manifest = readPackage(path);
    manifest["version"] = version;
    if (directory === "main") manifest["optionalDependencies"] = Object.fromEntries(Object.values(PlatformPackages).map((platform) => [platform.packageName, version]));
    writeFileSync(path, `${JSON.stringify(manifest, null, 2)}\n`);
  }
  writeFileSync(propertiesPath, properties);
}

if (import.meta.main) synchronizeVersion(fileURLToPath(new URL("../../", import.meta.url)));
