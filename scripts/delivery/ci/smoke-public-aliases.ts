import assert from "node:assert/strict";
import { mkdirSync, readFileSync } from "node:fs";
import { join } from "node:path";
import { AliasPackageName, MainPackageName } from "../package-model.ts";
import { LauncherPath } from "../npm/package-manifests.ts";
import { npmInvocation, ProcessExit } from "../process.ts";
import { smokePythonPackages } from "../pypi/smoke.ts";
import { hostRuntime } from "../layout.ts";
import { capture } from "./smoke-process.ts";

export function smokePublicAliases(root: string, version: string, environment: NodeJS.ProcessEnv): void {
  const aliasRoot = join(root, "alias");
  mkdirSync(aliasRoot);
  const install = npmInvocation(["install", "--prefix", aliasRoot, "--no-audit", "--no-fund", `${AliasPackageName}@${version}`]);
  const run = (executable: string, args: readonly string[]) => {
    const result = capture(executable, args, root, environment);
    assert.equal(result.status, ProcessExit.success);
    return result.stdout.trim();
  };
  run(install.executable, install.args);
  const modules = join(aliasRoot, "node_modules");
  const scoped: unknown = JSON.parse(readFileSync(join(modules, MainPackageName, "package.json"), "utf8"));
  assert.ok(typeof scoped === "object" && scoped !== null && "version" in scoped);
  assert.equal(scoped.version, version);
  assert.equal(run(process.execPath, [join(modules, AliasPackageName, LauncherPath), "--version"]), version);
  smokePythonPackages(root, hostRuntime(), version, environment);
}
