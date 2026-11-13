import assert from "node:assert/strict";
import { copyFileSync, mkdirSync } from "node:fs";
import { basename, join } from "node:path";
import { hashArtifact } from "../manifest.ts";
import { readPackage } from "../package-json.ts";
import { readTargets } from "../targets.ts";
import { validateOutput, resetOutput } from "../output.ts";
import { pythonVersion } from "./version.ts";
import { PythonPackage, wheelName } from "./wheel-model.ts";
import { PythonAliasPackage, PythonMainPackage, pythonPlatformPackage } from "./package-model.ts";

export const PyPIOutput = "artifacts/pypi";
export const PyPIStage = { platform: "platform", entry: "entry", alias: "alias" } as const;

export function releaseWheels(root: string, directory: string, source: { sha: string; dirty: boolean }, version: string): string[] {
  assert.equal(source.dirty, false, "PyPI release requires committed source.");
  assert.ok(!pythonVersion(version).includes("+"), "PyPI rejects local development versions.");
  const manifest = readPackage(validateOutput(root, `${directory}/release.json`));
  assert.equal(manifest["sha"], source.sha);
  assert.equal(manifest["version"], version);
  const targets = readTargets(manifest["targets"]);
  const artifacts = manifest["artifacts"];
  assert.ok(Array.isArray(artifacts));
  const wheels = artifacts.filter(
    (entry: unknown) => typeof entry === "object" && entry !== null && "path" in entry && typeof entry.path === "string" && entry.path.endsWith(PythonPackage.suffix),
  );
  const python = pythonVersion(version);
  const names = [...targets.map((target) => wheelName(python, pythonPlatformPackage(target), target)), wheelName(python, PythonMainPackage), wheelName(python, PythonAliasPackage)];
  assert.equal(wheels.length, names.length, "PyPI release must include platform, entry and alias wheels.");
  return names.map((path) => {
    const matches = wheels.filter((entry: unknown) => typeof entry === "object" && entry !== null && "path" in entry && entry.path === path);
    assert.equal(matches.length, 1, "Missing or repeated release wheel.");
    const entry: unknown = matches[0];
    assert.ok(typeof entry === "object" && entry !== null && "sha256" in entry && typeof entry.sha256 === "string");
    assert.equal(hashArtifact(root, `${directory}/${path}`), entry.sha256, "Wheel bytes changed after collection.");
    return validateOutput(root, `${directory}/${path}`);
  });
}

export function preparePyPI(root: string, directory: string, source: { sha: string; dirty: boolean }, version: string): void {
  const wheels = releaseWheels(root, directory, source, version);
  const output = resetOutput(root, PyPIOutput);
  for (const wheel of wheels) {
    const name = basename(wheel);
    const python = pythonVersion(version);
    const stage = name === wheelName(python, PythonMainPackage) ? PyPIStage.entry : name === wheelName(python, PythonAliasPackage) ? PyPIStage.alias : PyPIStage.platform;
    const destination = join(output, stage);
    mkdirSync(destination, { recursive: true });
    copyFileSync(wheel, join(destination, name));
  }
}
