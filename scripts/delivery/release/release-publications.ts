import assert from "node:assert/strict";
import { readPackage } from "../package-json.ts";
import { MainPackageName, PlatformPackages } from "../package-model.ts";
import { validateOutput } from "../output.ts";
import { readPublicationPackage, type Publication } from "../npm/publication-package.ts";

export function readReleasePublications(root: string, directory: string, source: { sha: string; dirty: boolean }, version: string): Publication[] {
  const release = readPackage(validateOutput(root, `${directory}/release.json`));
  assert.equal(release["sha"], source.sha, "Release source mismatch.");
  assert.equal(release["version"], version, "Release version mismatch.");
  const files = release["artifacts"];
  assert.ok(Array.isArray(files));
  const names = [...Object.values(PlatformPackages).map((platform) => platform.packageName), MainPackageName];
  const packages = files.filter(
    (entry: unknown) => typeof entry === "object" && entry !== null && "path" in entry && typeof entry.path === "string" && entry.path.endsWith(".tgz"),
  );
  assert.equal(packages.length, names.length, "A complete npm release requires six native packages and one wrapper.");
  return names.map((name) => readPublicationPackage(root, directory, packages, name, version, source.dirty));
}
