import assert from "node:assert/strict";
import { readFileSync, writeFileSync } from "node:fs";
import { basename, dirname, join } from "node:path";
import { WrapperOutput } from "../layout.ts";
import { MainPackageName, type SupportedRuntime } from "../package-model.ts";
import { inspectPackageContents } from "../package-contents.ts";
import { sourceIdentity } from "../source.ts";
import { hashArtifact } from "../manifest.ts";
import { WrapperReadme } from "./package-manifests.ts";
import { resetOutput } from "../output.ts";

import { AllTargets, readTargets, targetDependencies } from "../targets.ts";

export function recordWrapper(root: string, archive: string, version: string, targets: readonly SupportedRuntime[] = AllTargets): void {
  const file = basename(archive);
  const contents = inspectPackageContents(dirname(archive), file);
  assert.equal(contents.name, MainPackageName);
  assert.equal(contents.version, version);
  assert.deepEqual(
    contents.files.map((entry) => entry.path),
    ["LICENSE", WrapperReadme, "bin/open-forge.js", "package-model.js", "package.json"],
  );
  assert.deepEqual(contents.optionalDependencies, targetDependencies(version, targets));
  const bytes = readFileSync(archive);
  const source = sourceIdentity(root);
  const directory = resetOutput(root, WrapperOutput);
  const output = resetOutput(root, `${WrapperOutput}/packages`);
  writeFileSync(join(output, file), bytes);
  const packed = { path: file, sha256: hashArtifact(output, file) };
  writeFileSync(join(output, "SHA256SUMS"), `${packed.sha256}  ${file}\n`);
  writeFileSync(join(directory, "manifest.json"), `${JSON.stringify({ ...source, version, targets: readTargets(targets), package: packed }, null, 2)}\n`);
}
