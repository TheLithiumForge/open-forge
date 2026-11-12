import assert from "node:assert/strict";
import { mkdirSync, readFileSync } from "node:fs";
import { join } from "node:path";
import { AliasPackageName, MainPackageName } from "../package-model.ts";
import { LauncherPath } from "../npm/package-manifests.ts";
import { npmInvocation, ProcessExit } from "../process.ts";
import { findPython } from "../aliases/python.ts";
import { pythonVersion } from "../pypi/version.ts";
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
  const python = findPython();
  assert.ok(python, "Published wheel smoke requires Python.");
  const venv = join(root, "python");
  run(python.executable, [...python.args, "-m", "venv", venv]);
  const bin = join(venv, process.platform === "win32" ? "Scripts" : "bin");
  run(join(bin, process.platform === "win32" ? "python.exe" : "python"), [
    "-m",
    "pip",
    "install",
    "--index-url",
    "https://pypi.org/simple",
    "--only-binary=:all:",
    "--no-deps",
    `${AliasPackageName}==${pythonVersion(version)}`,
  ]);
  assert.equal(run(join(bin, process.platform === "win32" ? "open-forge.exe" : AliasPackageName), ["--version"]), version);
}
