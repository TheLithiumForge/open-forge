import assert from "node:assert/strict";
import { chmodSync, copyFileSync, mkdirSync, mkdtempSync, readdirSync, readFileSync, writeFileSync } from "node:fs";
import { basename, join, relative } from "node:path";
import { PlatformPackages, type SupportedRuntime } from "./package-model.ts";
import { readBuilt } from "./built-artifacts.ts";
import { reportFailure, run } from "./process.ts";
import { readBuildOptions } from "./options.ts";
import { repositoryRoot } from "./repository.ts";
import { committedVersion } from "./version.ts";
import { deliveryDirectory, hostRuntime, nativeDirectory } from "./layout.ts";
import { hashArtifact } from "./manifest.ts";

function pack(root: string, rid: SupportedRuntime): void {
  const manifest = readBuilt(root, rid, true);
  const directory = join(root, deliveryDirectory(rid));
  const output = mkdtempSync(join(directory, "packages-"));
  const platform = PlatformPackages[rid];
  const native = `${nativeDirectory(rid)}/open-forge/OpenForge.Cli${platform.nodePlatform === "win32" ? ".exe" : ""}`;
  const portable = join(output, "portable");
  mkdirSync(portable);
  copyFileSync(join(root, native), join(portable, platform.nativeFileName));
  chmodSync(join(portable, platform.nativeFileName), 0o755);
  copyFileSync(join(root, "LICENSE"), join(portable, "LICENSE"));
  const archive = `open-forge-${manifest.version}-${rid}.tar.gz`;
  run("tar", ["-czf", join(output, archive), "-C", portable, platform.nativeFileName, "LICENSE"], root);
  const journey = join(output, "journey");
  run(
    process.execPath,
    [
      "scripts/delivery/npm/__tests__/native-package-journey.ts",
      journey,
      rid,
      join(root, native),
      "release",
      manifest.version,
      manifest.version,
      join(directory, "build/launcher"),
    ],
    root,
  );
  const receipt: unknown = JSON.parse(readFileSync(join(journey, "receipt.json"), "utf8"));
  assert.ok(typeof receipt === "object" && receipt !== null && "outcome" in receipt && receipt.outcome === "passed");
  const packs = readdirSync(join(journey, "packs"), { recursive: true, withFileTypes: true }).filter((file) => file.isFile() && file.name.endsWith(".tgz"));
  assert.equal(packs.length, 2, "Expected main and matching platform npm packages.");
  for (const file of packs) copyFileSync(join(file.parentPath, file.name), join(output, file.name));
  const paths = [archive, ...packs.map((file) => file.name)];
  const files = paths.map((path) => ({ path, sha256: hashArtifact(output, path) }));
  writeFileSync(join(output, "SHA256SUMS"), files.map((file) => `${file.sha256}  ${basename(file.path)}\n`).join(""));
  writeFileSync(
    join(output, "package.json"),
    `${JSON.stringify({ sha: manifest.sha, version: manifest.version, rid, nativeSha256: hashArtifact(root, native), files }, null, 2)}\n`,
  );
  readBuilt(root, rid, true);
  writeFileSync(join(directory, "package-path.txt"), relative(root, output).replaceAll("\\", "/"));
  process.stdout.write(`Packed ${manifest.version}: ${relative(root, output)}\n`);
}

try {
  const values = readBuildOptions();
  committedVersion(repositoryRoot);
  assert.ok(!values.sha);
  pack(repositoryRoot, hostRuntime(values.rid));
} catch (error) {
  reportFailure(error);
}
