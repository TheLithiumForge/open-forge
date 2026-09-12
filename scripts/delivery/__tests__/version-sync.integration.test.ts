import assert from "node:assert/strict";
import { mkdirSync, mkdtempSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { dirname, join } from "node:path";
import { test } from "node:test";
import { synchronizeVersion } from "../version-sync.ts";
import { MainPackageName, PlatformPackages } from "../../package-managers/npm/package-model.ts";

test("version synchronization updates .NET, all npm manifests and exact optional dependencies", (context) => {
  const root = mkdtempSync(join(tmpdir(), "open-forge-version-"));
  context.after(() => rmSync(root, { recursive: true, force: true }));
  const version = "0.1.0-beta.1";
  writeFileSync(join(root, "package.json"), JSON.stringify({ version }));
  writeFileSync(
    join(root, "Directory.Build.props"),
    "<Project><OpenForgeCliVersion>0.0.0</OpenForgeCliVersion><OpenForgeCliInformationalVersion>0.0.0-dev</OpenForgeCliInformationalVersion></Project>",
  );
  const names = Object.values(PlatformPackages).map((platform) => platform.packageName);
  const paths = ["main", "linux-x64", "linux-arm64", "darwin-x64", "darwin-arm64", "win-x64", "win-arm64"].map((directory) =>
    join(root, "scripts/package-managers/npm", directory, "package.json"),
  );
  for (const path of paths) {
    mkdirSync(dirname(path), { recursive: true });
    writeFileSync(
      path,
      JSON.stringify({
        name: path === paths[0] ? MainPackageName : "platform",
        version: "0.0.0",
        ...(path === paths[0] ? { optionalDependencies: Object.fromEntries(names.map((name) => [name, "0.0.0"])) } : {}),
      }),
    );
  }
  synchronizeVersion(root);
  for (const path of paths) {
    const manifest: unknown = JSON.parse(readFileSync(path, "utf8"));
    assert.ok(typeof manifest === "object" && manifest !== null && "version" in manifest);
    assert.equal(manifest.version, version);
    if ("optionalDependencies" in manifest) assert.deepEqual(manifest.optionalDependencies, Object.fromEntries(names.map((name) => [name, version])));
  }
  assert.match(readFileSync(join(root, "Directory.Build.props"), "utf8"), /<OpenForgeCliVersion>0\.1\.0-beta\.1<\/OpenForgeCliVersion>/);
  assert.match(readFileSync(join(root, "Directory.Build.props"), "utf8"), /<OpenForgeCliInformationalVersion>0\.1\.0-beta\.1<\/OpenForgeCliInformationalVersion>/);
});
