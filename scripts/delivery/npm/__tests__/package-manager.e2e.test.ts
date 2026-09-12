import assert from "node:assert/strict";
import { execFileSync, spawnSync } from "node:child_process";
import { chmodSync, mkdirSync, mkdtempSync, readdirSync, realpathSync, rmSync, statSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import test from "node:test";
import { fileURLToPath } from "node:url";

const repositoryRoot = fileURLToPath(new URL("../../../../", import.meta.url));
const managerPath = join(import.meta.dirname, "../manage.ts");
const localGitSha = "0123456789abcdef0123456789abcdef01234567";
const fixtureArgument = "--version";
const fixtureEnvironmentName = "OPEN_FORGE_PACKAGE_E2E";
const fixtureEnvironmentValue = "forwarded";
const fixtureOutput = "open-forge owned package fixture\n";
const npmFlags = ["--offline", "--ignore-scripts", "--no-save", "--no-package-lock", "--no-audit", "--no-fund"] as const;

test("PackageEndToEnd: installed main launcher forwards one harmless invocation to the current Linux payload", () => {
  const npmCli = process.env["npm_execpath"];
  if (npmCli === undefined) {
    throw new Error("Run the package-manager journey through its root npm script.");
  }

  const temporaryRoot = mkdtempSync(join(tmpdir(), "open-forge-npm-e2e-"));
  try {
    const artifactsRoot = join(temporaryRoot, "artifacts");
    const nativeArtifact = join(temporaryRoot, "native", "open-forge");
    const packsRoot = join(temporaryRoot, "packs");
    const installRoot = join(temporaryRoot, "install");
    const npmEnvironment = createNpmEnvironment(temporaryRoot);

    mkdirSync(join(temporaryRoot, "native"), { recursive: true });
    writeFileSync(
      nativeArtifact,
      `#!/bin/sh
test "$#" -eq 1 || exit 90
test "$1" = "${fixtureArgument}" || exit 91
test "$${fixtureEnvironmentName}" = "${fixtureEnvironmentValue}" || exit 92
printf '%s' "${fixtureOutput}"
`,
      "utf8",
    );
    chmodSync(nativeArtifact, 0o755);

    execFileSync(process.execPath, [managerPath, "stage", artifactsRoot, "linux-x64", nativeArtifact, "local", localGitSha], {
      cwd: repositoryRoot,
      env: npmEnvironment,
      stdio: "pipe",
    });

    const stagedRoot = join(artifactsRoot, "npm", "stage", "linux-x64");
    const mainTarball = pack(npmCli, join(stagedRoot, "open-forge"), join(packsRoot, "main"), npmEnvironment);
    const platformTarball = pack(npmCli, join(stagedRoot, "open-forge-linux-x64"), join(packsRoot, "linux-x64"), npmEnvironment);
    runNpm(npmCli, ["install", "--prefix", installRoot, mainTarball, platformTarball], npmEnvironment);

    const modulesRoot = join(installRoot, "node_modules");
    const mainDirectory = join(modulesRoot, "@thelithiumforge", "open-forge");
    const platformDirectory = join(modulesRoot, "@thelithiumforge", "open-forge-linux-x64");
    const installedNative = join(platformDirectory, "bin", "open-forge");
    const installedBin = join(modulesRoot, ".bin", "open-forge");
    const installedLauncher = join(mainDirectory, "bin", "open-forge.js");

    assert.equal(statSync(mainDirectory).isDirectory(), true);
    assert.equal(statSync(platformDirectory).isDirectory(), true);
    assert.equal(statSync(installedNative).isFile(), true);
    assert.equal(realpathSync(installedBin), realpathSync(installedLauncher));

    const completion = spawnSync(realpathSync(installedBin), [fixtureArgument], {
      cwd: installRoot,
      encoding: "utf8",
      env: { ...npmEnvironment, [fixtureEnvironmentName]: fixtureEnvironmentValue },
      shell: false,
    });
    assert.equal(completion.error, undefined);
    assert.equal(completion.signal, null);
    assert.equal(completion.status, 0);
    assert.equal(completion.stdout, fixtureOutput);
    assert.equal(completion.stderr, "");
  } finally {
    rmSync(temporaryRoot, { force: true, recursive: true });
  }
});

function createNpmEnvironment(temporaryRoot: string): NodeJS.ProcessEnv {
  const home = join(temporaryRoot, "home");
  const cache = join(temporaryRoot, "cache");
  const prefix = join(temporaryRoot, "prefix");
  const userConfig = join(temporaryRoot, "npmrc");
  mkdirSync(home, { recursive: true });
  mkdirSync(cache, { recursive: true });
  mkdirSync(prefix, { recursive: true });
  writeFileSync(userConfig, "", "utf8");

  return {
    ...process.env,
    HOME: home,
    USERPROFILE: home,
    npm_config_cache: cache,
    npm_config_prefix: prefix,
    npm_config_userconfig: userConfig,
    npm_config_update_notifier: "false",
    NO_UPDATE_NOTIFIER: "1",
  };
}

function pack(npmCli: string, packageDirectory: string, packDirectory: string, environment: NodeJS.ProcessEnv): string {
  mkdirSync(packDirectory, { recursive: true });
  runNpm(npmCli, ["pack", packageDirectory, "--pack-destination", packDirectory], environment);
  const tarball = readdirSync(packDirectory).at(0);
  if (tarball === undefined) {
    throw new Error(`npm pack did not create a tarball in ${packDirectory}.`);
  }

  return join(packDirectory, tarball);
}

function runNpm(npmCli: string, argumentsToPass: readonly string[], environment: NodeJS.ProcessEnv): void {
  execFileSync(process.execPath, [npmCli, ...argumentsToPass, ...npmFlags], {
    cwd: repositoryRoot,
    env: environment,
    stdio: "pipe",
  });
}
