import assert from "node:assert/strict";
import { AliasPackageName, MainPackageName, PlatformPackages } from "../package-model.ts";
import { AllTargets, readTargets } from "../targets.ts";

export interface PreviousReleaseGraph {
  version: string;
  packages: readonly string[];
}

export function registryReleaseGraph(value: unknown, version: string, aliasPublished: boolean): PreviousReleaseGraph {
  assert.ok(typeof value === "object" && value !== null && "version" in value && value.version === version && "optionalDependencies" in value);
  const dependencies = value.optionalDependencies;
  assert.ok(typeof dependencies === "object" && dependencies !== null && !Array.isArray(dependencies));
  const native = AllTargets.map((target) => PlatformPackages[target].packageName).filter((name) => Object.hasOwn(dependencies, name));
  assert.ok(native.length > 0 && Object.keys(dependencies).length === native.length, "Previous wrapper has an unsupported package graph.");
  for (const dependency of Object.values(dependencies)) assert.equal(dependency, version, "Previous wrapper dependencies must pin its exact version.");
  return { version, packages: [...native, MainPackageName, ...(aliasPublished ? [AliasPackageName] : [])] };
}

export function recordedReleaseGraph(value: unknown, version: string): PreviousReleaseGraph {
  assert.ok(typeof value === "object" && value !== null && "version" in value && value.version === version && "targets" in value && "artifacts" in value);
  const targets = readTargets(value.targets);
  assert.ok(Array.isArray(value.artifacts));
  const paths = value.artifacts
    .map((entry: unknown) => {
      assert.ok(typeof entry === "object" && entry !== null && "path" in entry && typeof entry.path === "string");
      return entry.path;
    })
    .filter((path) => path.endsWith(".tgz"));
  const tarball = (name: string) => `${name.replace(/^@/u, "").replaceAll("/", "-")}-${version}.tgz`;
  const names = [...targets.map((target) => PlatformPackages[target].packageName), MainPackageName];
  if (paths.includes(tarball(AliasPackageName))) names.push(AliasPackageName);
  assert.deepEqual([...paths].sort(), names.map(tarball).sort(), "Previous release artifacts do not match its package graph.");
  return { version, packages: names };
}
