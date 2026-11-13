import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { join, resolve } from "node:path";
import { AliasPackageName, PlatformPackages, type SupportedRuntime } from "../package-model.ts";
import { AllTargets } from "../targets.ts";
import { PythonAliasPackage, PythonMainPackage, pythonModule, pythonPlatformPackage } from "./package-model.ts";
import { pythonVersion } from "./version.ts";
import { findPython } from "./python.ts";

export function smokePythonPackages(root: string, runtime: SupportedRuntime, version: string, environment: NodeJS.ProcessEnv, directory?: string) {
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
  const pythonRelease = pythonVersion(version);
  const names = [PythonAliasPackage, PythonMainPackage];
  const known = new Set([...names, ...AllTargets.map(pythonPlatformPackage)]);
  const results = names.map((name) => {
    const venv = join(root, pythonModule(name));
    run(python.executable, [...python.args, "-m", "venv", venv]);
    const bin = join(venv, process.platform === "win32" ? "Scripts" : "bin");
    const interpreter = join(bin, process.platform === "win32" ? "python.exe" : "python");
    run(interpreter, [
      "-m",
      "pip",
      "install",
      "--disable-pip-version-check",
      "--no-cache-dir",
      "--only-binary=:all:",
      ...(directory === undefined ? ["--index-url", "https://pypi.org/simple"] : ["--no-index", "--find-links", resolve(directory)]),
      `${name}==${pythonRelease}`,
    ]);
    const installed: unknown = JSON.parse(run(interpreter, ["-m", "pip", "list", "--format=json", "--disable-pip-version-check"]));
    assert.ok(Array.isArray(installed));
    const packages = installed
      .flatMap((entry: unknown) => {
        assert.ok(typeof entry === "object" && entry !== null && "name" in entry && typeof entry.name === "string" && "version" in entry && typeof entry.version === "string");
        const normalized = entry.name.toLowerCase().replace(/[-_.]+/gu, "-");
        return known.has(normalized) ? [{ name: normalized, version: entry.version }] : [];
      })
      .sort((left, right) => left.name.localeCompare(right.name));
    const expected = [PythonMainPackage, pythonPlatformPackage(runtime), ...(name === PythonAliasPackage ? [PythonAliasPackage] : [])]
      .sort()
      .map((packageName) => ({ name: packageName, version: pythonRelease }));
    assert.deepEqual(packages, expected, "Installed Python graph must include exactly one matching native package.");
    assert.equal(run(join(bin, process.platform === "win32" ? PlatformPackages[runtime].nativeFileName : AliasPackageName), ["--version"]), version);
    process.stdout.write(`Wheel smoke passed: ${name} resolved ${packages.length} Open Forge packages and ran ${version}.\n`);
    return { name, packages };
  });
  return { status: "passed", results };
}
