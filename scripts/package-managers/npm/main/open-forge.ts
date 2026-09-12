#!/usr/bin/env node

import { spawnSync } from "node:child_process";
import { dirname, join } from "node:path";
import { createRequire } from "node:module";

import { PlatformPackages } from "../package-model.ts";

const failureExitCode = 1;

function main(): void {
  const platformPackage = Object.values(PlatformPackages).find((candidate) => candidate.nodePlatform === process.platform && candidate.nodeArchitecture === process.arch);

  if (platformPackage === undefined) {
    fail(`Open Forge does not provide an npm package for ${process.platform}/${process.arch}.`);
    return;
  }

  let packageManifest: string;
  try {
    packageManifest = createRequire(import.meta.url).resolve(`${platformPackage.packageName}/package.json`);
  } catch {
    fail(`The required optional package ${platformPackage.packageName} is not installed.`);
    return;
  }

  const executable = join(dirname(packageManifest), "bin", platformPackage.nativeFileName);
  const completion = spawnSync(executable, process.argv.slice(2), {
    cwd: process.cwd(),
    env: process.env,
    shell: false,
    stdio: "inherit",
  });

  if (completion.error !== undefined) {
    fail(`Unable to start ${platformPackage.packageName}: ${completion.error.message}`);
    return;
  }

  if (completion.status !== null) {
    process.exitCode = completion.status;
    return;
  }

  if (completion.signal !== null) {
    process.kill(process.pid, completion.signal);
    return;
  }

  fail(`${platformPackage.packageName} ended without an exit code or signal.`);
}

function fail(message: string): void {
  process.stderr.write(`${message}\n`);
  process.exitCode = failureExitCode;
}

main();
