import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { chmodSync, copyFileSync, readFileSync, writeFileSync } from "node:fs";
import { join } from "node:path";
import { bundleNames, bundleReadme, checksumLine, BundleFile, BundleMode, BundleHash, TarExecutable, TarArgument } from "./bundle-model.ts";
import { CiCommand } from "./command-names.ts";
import { readOptions } from "../options.ts";
import { parseTargets } from "../targets.ts";
import { reportFailure, run } from "../process.ts";
import { repositoryRoot } from "../repository.ts";
import { resetOutput } from "../output.ts";

try {
  const options = readOptions(CiCommand.bundle);
  if (options) {
    assert.ok(options.rid, "ci:bundle requires --rid.");
    const [rid] = parseTargets(options.rid);
    assert.ok(rid, "Supply one supported RID.");
    assert.equal(parseTargets(options.rid).length, 1, "Supply one supported RID.");
    const names = bundleNames(rid);
    const content = resetOutput(repositoryRoot, names.content);
    const executable = join(content, names.executable);
    copyFileSync(join(repositoryRoot, names.source), executable);
    chmodSync(executable, BundleMode);
    copyFileSync(join(repositoryRoot, BundleFile.license), join(content, BundleFile.license));
    writeFileSync(join(content, BundleFile.readme), bundleReadme(rid));
    run(TarExecutable, [TarArgument.createGzip, names.archive, TarArgument.directory, names.content, TarArgument.content], repositoryRoot);
    const hash = createHash(BundleHash.algorithm)
      .update(readFileSync(join(repositoryRoot, names.archive)))
      .digest(BundleHash.encoding);
    writeFileSync(join(repositoryRoot, names.directory, BundleFile.checksums), checksumLine(hash, names.file));
  }
} catch (error) {
  reportFailure(error);
}
