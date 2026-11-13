import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { resolve } from "node:path";
import { valid } from "semver";
import { readPackage } from "../package-json.ts";
import { AliasPackageName, MainPackageName } from "../package-model.ts";
import { npmInvocation, ProcessExit } from "../process.ts";
import { registryVersionExists } from "../npm/registry-version.ts";
import { recordedReleaseGraph, registryReleaseGraph } from "./previous-release-graph.ts";
import { ReleaseFile } from "./release-plan.ts";

export function readPreviousGraph(root: string, version: string, from: string | undefined, registry: boolean) {
  assert.ok(valid(version), "--to must be an exact SemVer.");
  if (from !== undefined) {
    assert.equal(registry, false, "Choose --from or --registry, not both.");
    return recordedReleaseGraph(readPackage(resolve(root, from, ReleaseFile.manifest)), version);
  }
  assert.ok(registry, "Offline recovery needs --from <previous-release-directory>. Use --registry to read the previous npm graph.");
  const command = npmInvocation(["view", `${MainPackageName}@${version}`, "version", "optionalDependencies", "--json", "--prefer-online"]);
  const result = spawnSync(command.executable, command.args, { cwd: root, encoding: "utf8", windowsHide: true });
  assert.equal(result.error, undefined);
  assert.equal(result.status, ProcessExit.success, result.stderr);
  const wrapper: unknown = JSON.parse(result.stdout);
  const alias = registryVersionExists(root, AliasPackageName, version, undefined, { [MainPackageName]: version });
  return registryReleaseGraph(wrapper, version, alias);
}
