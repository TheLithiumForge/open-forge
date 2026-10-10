import assert from "node:assert/strict";
import { mkdirSync, mkdtempSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { valid } from "semver";
import { MainPackageName } from "../package-model.ts";
import { readOptions } from "../options.ts";
import { npmInvocation, reportFailure, ProcessExit } from "../process.ts";
import { capture } from "./smoke-process.ts";
import { CiCommand } from "./command-names.ts";
import { evaluateSmoke, SmokeCheck, SmokeArguments } from "./smoke-checks.ts";

const SmokeLayout = {
  prefix: "open-forge-public-",
  workspace: "workspace",
  home: "home",
  modules: "node_modules",
  launcher: "bin/open-forge.js",
  manifest: "package.json",
  cache: "cache",
} as const;
const SmokeEnvironment = { home: "OPENFORGE_DATA_HOME", cache: "npm_config_cache" } as const;
const SmokeNpm = { install: "install", prefix: "--prefix", flags: ["--no-audit", "--no-fund"] } as const;

try {
  const options = readOptions(CiCommand.smoke);
  if (options) {
    const version = options.version;
    assert.ok(version && valid(version) === version, "smoke:public requires an exact --version.");
    const root = mkdtempSync(join(tmpdir(), SmokeLayout.prefix));
    process.stdout.write(`Public smoke workspace: ${root}\n`);
    const workspace = join(root, SmokeLayout.workspace);
    mkdirSync(workspace);
    writeFileSync(join(root, SmokeLayout.manifest), JSON.stringify({ private: true }));
    const environment = { ...process.env, [SmokeEnvironment.home]: join(root, SmokeLayout.home), [SmokeEnvironment.cache]: join(root, SmokeLayout.cache) };
    const install = npmInvocation([SmokeNpm.install, SmokeNpm.prefix, root, ...SmokeNpm.flags, `${MainPackageName}@${version}`]);
    const installed = capture(install.executable, install.args, root, environment);
    assert.equal(installed.status, ProcessExit.success, "Install published npm package failed.");
    const launcher = join(root, SmokeLayout.modules, MainPackageName, SmokeLayout.launcher);
    const checks = [
      { name: SmokeCheck.version, args: SmokeArguments.version },
      { name: SmokeCheck.install, args: SmokeArguments.install },
      { name: SmokeCheck.status, args: SmokeArguments.status },
      { name: SmokeCheck.configure, args: SmokeArguments.configure },
    ];
    for (const check of checks) {
      const output = capture(process.execPath, [launcher, ...check.args], workspace, environment);
      const passed = evaluateSmoke(check.name, output, version);
      process.stdout.write(`${passed ? "[PASS]" : "[FAIL]"} ${check.name}\n`);
      if (!passed) {
        process.exitCode = output.status || ProcessExit.failure;
        throw new Error(`${check.name} failed.`);
      }
    }
  }
} catch (error) {
  reportFailure(error);
}
