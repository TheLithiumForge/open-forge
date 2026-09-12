import assert from "node:assert/strict";
import { AllTargets, parseTargets, readTargets } from "./targets.ts";
import { chmodSync, copyFileSync, mkdirSync, writeFileSync } from "node:fs";
import { basename, join, relative } from "node:path";
import { PlatformPackages, type SupportedRuntime } from "./package-model.ts";
import { readBuilt } from "./built-artifacts.ts";
import { reportFailure, run } from "./process.ts";
import { readOptions } from "./options.ts";
import { repositoryRoot } from "./repository.ts";
import { committedVersion } from "./version.ts";
import { deliveryDirectory, hostRuntime, nativeDirectory } from "./layout.ts";
import { hashArtifact } from "./manifest.ts";
import { removeOutputs, resetOutput } from "./output.ts";
import { packNpmPackages } from "./npm/pack-packages.ts";
import { testInstalledNative } from "./npm/__tests__/installed-native.ts";
import { runStage } from "./stage.ts";
import { recordWrapper } from "./npm/wrapper-artifact.ts";

export function pack(root: string, rid: SupportedRuntime, skipTests = false, targets: readonly SupportedRuntime[] = AllTargets): void {
  targets = readTargets(targets);
  assert.ok(targets.includes(rid), "The wrapper target selection must include the native package being packed.");
  const manifest = readBuilt(root, rid, !skipTests);
  if (skipTests) process.stderr.write("Warning: tests skipped; these packages are untested and cannot enter the native release publisher.\n");
  const directory = join(root, deliveryDirectory(rid));
  removeOutputs(root, [`${deliveryDirectory(rid)}/package-path.txt`]);
  const output = resetOutput(root, `${deliveryDirectory(rid)}/packages`);
  const platform = PlatformPackages[rid];
  const native = `${nativeDirectory(rid)}/open-forge/OpenForge.Cli${platform.nodePlatform === "win32" ? ".exe" : ""}`;
  const portable = join(output, "portable");
  mkdirSync(portable);
  copyFileSync(join(root, native), join(portable, platform.nativeFileName));
  chmodSync(join(portable, platform.nativeFileName), 0o755);
  copyFileSync(join(root, "LICENSE"), join(portable, "LICENSE"));
  const archive = `open-forge-${manifest.version}-${rid}.tar.gz`;
  run("tar", ["-czf", join(output, archive), "-C", portable, platform.nativeFileName, "LICENSE"], root);
  const packages = runStage("Create npm tarballs", () =>
    packNpmPackages({
      repositoryRoot: root,
      artifactsRoot: output,
      outputDirectory: output,
      runtime: rid,
      targets,
      nativeArtifact: join(root, native),
      version: { kind: "release", value: manifest.version },
      compiledLauncher: join(directory, "build/launcher"),
    }),
  );
  if (!skipTests)
    runStage("Test installed npm package", () =>
      testInstalledNative({
        resultRoot: join(output, "journey"),
        runtime: rid,
        nativeArtifact: join(root, native),
        expectedVersion: manifest.version,
        ...packages,
      }),
    );
  const paths = [archive, basename(packages.mainTarball), basename(packages.platformTarball)];
  const files = paths.map((path) => ({ path, sha256: hashArtifact(output, path) }));
  writeFileSync(join(output, "SHA256SUMS"), files.map((file) => `${file.sha256}  ${basename(file.path)}\n`).join(""));
  readBuilt(root, rid, !skipTests);
  recordWrapper(root, join(output, `thelithiumforge-open-forge-${manifest.version}.tgz`), manifest.version, targets);
  writeFileSync(
    join(output, "package.json"),
    `${JSON.stringify({ sha: manifest.sha, version: manifest.version, rid, targets, tested: !skipTests, nativeSha256: hashArtifact(root, native), files }, null, 2)}\n`,
  );
  writeFileSync(join(directory, "package-path.txt"), relative(root, output).replaceAll("\\", "/"));
  process.stdout.write(`Packed ${manifest.version}: ${relative(root, output)}\n`);
}

if (import.meta.main) {
  try {
    const values = readOptions("pack");
    if (values) {
      committedVersion(repositoryRoot);
      runStage("Pack artifacts", () => pack(repositoryRoot, hostRuntime(values.rid), values["skip-tests"], parseTargets(values.targets)));
    }
  } catch (error) {
    reportFailure(error);
  }
}
