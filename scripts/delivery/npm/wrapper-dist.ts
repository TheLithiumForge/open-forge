import assert from "node:assert/strict";
import { readdirSync } from "node:fs";
import { join } from "node:path";
import { readOptions } from "../options.ts";
import { parseTargets } from "../targets.ts";
import { compileLauncher } from "./stage.ts";
import { stageWrapperPackage } from "./wrapper-stage.ts";
import { resetOutput } from "../output.ts";
import { sourceIdentity } from "../source.ts";
import { candidateVersion, committedVersion } from "../version.ts";
import { npm, reportFailure } from "../process.ts";
import { repositoryRoot } from "../repository.ts";
import { WrapperOutput } from "../layout.ts";
import { recordWrapper } from "./wrapper-artifact.ts";

try {
  const values = readOptions("dist:wrapper");
  if (values) {
    const targets = parseTargets(values.targets);
    const source = sourceIdentity(repositoryRoot);
    const version = candidateVersion(committedVersion(repositoryRoot), values.sha ? source.sha : undefined);
    const directory = resetOutput(repositoryRoot, WrapperOutput);
    const build = join(directory, "build");
    compileLauncher(repositoryRoot, build);
    const stage = join(directory, "stage");
    stageWrapperPackage(repositoryRoot, stage, version, build, targets);
    const output = resetOutput(repositoryRoot, `${WrapperOutput}/packages`);
    npm(["pack", stage, "--pack-destination", output, "--offline", "--ignore-scripts", "--no-audit", "--no-fund"], repositoryRoot);
    const files = readdirSync(output);
    assert.equal(files.length, 1);
    const file = files[0];
    assert.ok(file && file.endsWith(".tgz"));
    assert.deepEqual(sourceIdentity(repositoryRoot), source, "Source changed while packaging the wrapper.");
    recordWrapper(repositoryRoot, join(output, file), version, targets);
    process.stdout.write(`Packed wrapper ${version}: ${output}\n`);
  }
} catch (error) {
  reportFailure(error);
}
