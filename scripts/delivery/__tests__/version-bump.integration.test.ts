import assert from "node:assert/strict";
import { execFileSync } from "node:child_process";
import { cpSync, mkdirSync, mkdtempSync, readFileSync, rmSync, symlinkSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { test } from "node:test";
import { fileURLToPath } from "node:url";
import { readPackage } from "../package-json.ts";
import { PlatformPackages } from "../package-model.ts";

const repository = fileURLToPath(new URL("../../../", import.meta.url));

test("the public npm version command invokes synchronization without committing or tagging", (context) => {
  const root = mkdtempSync(join(tmpdir(), "open-forge-version-command-"));
  context.after(() => rmSync(root, { recursive: true, force: true }));
  const npmCli = process.env["npm_execpath"];
  assert.ok(npmCli, "Run through npm run test:delivery.");
  const run = (executable: string, args: readonly string[]) =>
    execFileSync(executable, args, {
      cwd: root,
      encoding: "utf8",
      env: {
        ...process.env,
        npm_config_offline: "true",
        npm_config_update_notifier: "false",
        NO_UPDATE_NOTIFIER: "1",
        npm_config_cache: join(root, "cache"),
        npm_config_userconfig: join(root, "npmrc"),
        npm_config_globalconfig: join(root, "global-npmrc"),
      },
    });
  mkdirSync(join(root, "scripts"), { recursive: true });
  cpSync(join(repository, "scripts/delivery"), join(root, "scripts/delivery"), { recursive: true });
  symlinkSync(join(repository, "node_modules"), join(root, "node_modules"), process.platform === "win32" ? "junction" : "dir");
  const scripts = readPackage(join(repository, "package.json"))["scripts"];
  writeFileSync(join(root, "package.json"), JSON.stringify({ name: "version-fixture", type: "module", private: true, version: "0.0.0", scripts }));
  writeFileSync(
    join(root, "package-lock.json"),
    JSON.stringify({ name: "version-fixture", version: "0.0.0", lockfileVersion: 3, packages: { "": { name: "version-fixture", version: "0.0.0" } } }),
  );
  cpSync(join(repository, "Directory.Build.props"), join(root, "Directory.Build.props"));
  writeFileSync(join(root, ".gitignore"), "node_modules/\ncache/\n");
  writeFileSync(join(root, "npmrc"), "");
  writeFileSync(join(root, "global-npmrc"), "");
  run("git", ["init", "--quiet"]);
  run("git", ["config", "user.name", "Version Fixture"]);
  run("git", ["config", "user.email", "fixture@example.invalid"]);
  run("git", ["add", "."]);
  run("git", ["-c", "commit.gpgsign=false", "commit", "--quiet", "-m", "Seed version fixture"]);
  const head = run("git", ["rev-parse", "HEAD"]);
  const version = "0.1.0-beta.1";
  run(process.execPath, [npmCli, "run", "version:bump", "--", version]);
  assert.equal(readPackage(join(root, "package.json"))["version"], version);
  assert.equal(readPackage(join(root, "package-lock.json"))["version"], version);
  const main = readPackage(join(root, "scripts/delivery/npm/main/package.json"));
  assert.deepEqual(main["optionalDependencies"], Object.fromEntries(Object.values(PlatformPackages).map((platform) => [platform.packageName, version])));
  for (const directory of ["main", "linux-x64", "linux-arm64", "darwin-x64", "darwin-arm64", "win-x64", "win-arm64"])
    assert.equal(readPackage(join(root, "scripts/delivery/npm", directory, "package.json"))["version"], version);
  assert.ok(readFileSync(join(root, "Directory.Build.props"), "utf8").includes(`<OpenForgeCliInformationalVersion>${version}</OpenForgeCliInformationalVersion>`));
  assert.equal(run("git", ["rev-parse", "HEAD"]), head);
  assert.equal(run("git", ["tag", "--list"]), "");
});
