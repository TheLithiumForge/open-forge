import assert from "node:assert/strict";
import { readFileSync, writeFileSync } from "node:fs";
import { join } from "node:path";
import { fileURLToPath } from "node:url";
import { committedVersion } from "./version.ts";

export function synchronizeVersion(root: string): void {
  const version = committedVersion(root);
  const propertiesPath = join(root, "Directory.Build.props");
  const properties = readFileSync(propertiesPath, "utf8");
  const element = /<OpenForgeCliVersion>[^<]*<\/OpenForgeCliVersion>/u;
  assert.ok(element.test(properties), "Missing OpenForgeCliVersion property.");
  writeFileSync(propertiesPath, properties.replace(element, `<OpenForgeCliVersion>${version}</OpenForgeCliVersion>`));
}

if (import.meta.main) synchronizeVersion(fileURLToPath(new URL("../../", import.meta.url)));
