import { writeFileSync } from "node:fs";
import { join, relative } from "node:path";
import type { SupportedRuntime } from "./package-model.ts";
import { readBuilt } from "./built-artifacts.ts";
import { resetOutput, removeOutputs } from "./output.ts";
import { reportFailure } from "./process.ts";
import { runSuites } from "./test-suites.ts";
import { readOptions } from "./options.ts";
import { repositoryRoot } from "./repository.ts";
import { committedVersion } from "./version.ts";
import { deliveryDirectory, hostRuntime, suites } from "./layout.ts";

function testBuilt(root: string, rid: SupportedRuntime): void {
  const manifest = readBuilt(root, rid);
  const directory = join(root, deliveryDirectory(rid));
  writeFileSync(join(directory, "manifest.json"), `${JSON.stringify({ ...manifest, tested: false }, null, 2)}\n`);
  removeOutputs(root, [`${deliveryDirectory(rid)}/package-path.txt`]);
  const reports = resetOutput(root, `${deliveryDirectory(rid)}/reports`);
  runSuites(root, suites(rid), reports);
  readBuilt(root, rid);
  writeFileSync(join(directory, "manifest.json"), `${JSON.stringify({ ...manifest, tested: true, reports: relative(root, reports).replaceAll("\\", "/") }, null, 2)}\n`);
}

try {
  const values = readOptions("test:built");
  if (values) {
    committedVersion(repositoryRoot);
    testBuilt(repositoryRoot, hostRuntime(values.rid));
  }
} catch (error) {
  reportFailure(error);
}
