import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { join, resolve } from "node:path";
import { PlatformPackages, type SupportedRuntime } from "../package-model.ts";
import { PythonPackageName } from "./wheel-model.ts";
import { pythonVersion } from "./version.ts";
import { findPython } from "./python.ts";

// Installs the wheel into a clean venv and runs the executable pip placed in the venv's scripts directory.
export function smokePythonPackage(root: string, runtime: SupportedRuntime, version: string, environment: NodeJS.ProcessEnv, directory?: string) {
  const python = findPython();
  if (python === undefined) {
    assert.ok(directory !== undefined, "Published wheel smoke requires Python.");
    process.stdout.write("Wheel smoke not run: Python is unavailable.\n");
    return { status: "skipped" };
  }
  const run = (executable: string, args: string[]) => {
    const result = spawnSync(executable, args, { cwd: root, env: environment, encoding: "utf8", windowsHide: true });
    assert.equal(result.error, undefined);
    assert.equal(result.status, 0, result.stdout + result.stderr);
    return result.stdout.trim();
  };
  const venv = join(root, "venv");
  run(python.executable, [...python.args, "-m", "venv", venv]);
  const bin = join(venv, process.platform === "win32" ? "Scripts" : "bin");
  const source = directory === undefined ? ["--index-url", "https://pypi.org/simple"] : ["--no-index", "--find-links", resolve(directory)];
  run(join(bin, process.platform === "win32" ? "python.exe" : "python"), [
    "-m",
    "pip",
    "install",
    "--disable-pip-version-check",
    "--no-cache-dir",
    "--only-binary=:all:",
    ...source,
    `${PythonPackageName}==${pythonVersion(version)}`,
  ]);
  assert.equal(run(join(bin, PlatformPackages[runtime].nativeFileName), ["--version"]), version);
  process.stdout.write(`Wheel smoke passed: ${PythonPackageName} installed ${version} on PATH.\n`);
  return { status: "passed" };
}
