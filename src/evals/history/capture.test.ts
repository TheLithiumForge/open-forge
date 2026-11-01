import assert from "node:assert/strict";
import { writeFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import type { Suite } from "../benchmarks/suite.types.ts";
import { assertFrozenExecution, captureExperiment, checkerDigests, LaterLayerCheckers, withReportedModels } from "./capture.ts";
import { LayerOneCase } from "../benchmarks/registry.ts";
import { FixtureSuite } from "../config/test-fixtures.ts";
import { fingerprint } from "./fingerprints.ts";
import { writeJson } from "./json-files.ts";
import { FixtureCliVersion, temporaryHistory, v2Fixture } from "./test-fixtures.ts";

test("batch capture freezes installed CLI content, pinned tools, image digests and source versions", async () => {
  const { root, cleanup } = await temporaryHistory();
  try {
    const { batch, run } = v2Fixture();
    const paths = { root, modules: join(root, "modules"), suites: join(root, "cases"), checks: join(root, "checks"), docker: join(root, "images"),
      imageDigests: { eval: `sha256:${fingerprint("image")}` } };
    const cli = join(paths.modules, "@thelithiumforge", "open-forge");
    await writeJson(join(cli, "package.json"), { version: FixtureCliVersion, dependencies: {} });
    await writeJson(join(root, "package.json"), { version: FixtureCliVersion, dependencies: { tool: "pinned-version" } });
    await writeJson(join(root, "execution", "source.json"), { source: "before" });
    await writeJson(join(paths.checks, "check.json"), { checks: "before" });
    const suite: Suite = { id: batch.suite, title: "Fixture", project: "fixture", baseline: "reference", setups: {}, prompt: "game", intent: [], rubric: [], fingerprint: "old-combined", intentEdited: false };
    const definition = join(paths.suites, suite.id, "suite.json");
    await writeJson(definition, { setups: {} });
    const captured = await captureExperiment(batch.id, batch.config, suite, "candidate", paths);
    assert.equal(captured.cliVersion, FixtureCliVersion);
    assert.equal(captured.candidateLabel, "candidate");
    assert.equal(captured.pinnedTools["tool"], "pinned-version");
    assert.deepEqual(captured.imageDigests, paths.imageDigests);
    assert.equal(captured.fingerprint, captured.fingerprints.benchmarkInputs);
    const original = structuredClone(captured);
    await writeFile(join(cli, "changed.ts"), "changed package");
    await writeJson(definition, { setups: { changed: true } });
    const next = await captureExperiment(batch.id, batch.config, suite, "candidate", paths);
    assert.notEqual(next.fingerprints.cliPackage, captured.fingerprints.cliPackage);
    assert.equal(next.fingerprints.harnessSource, captured.fingerprints.harnessSource);
    assert.equal(next.fingerprints.benchmarkInputs, captured.fingerprints.benchmarkInputs);
    assert.throws(() => assertFrozenExecution(captured, next));
    assert.deepEqual(captured, original);
    await writeFile(join(root, "paths.ts"), "new runner paths");
    const changed = await captureExperiment(batch.id, batch.config, suite, "candidate", paths);
    assert.notEqual(changed.fingerprints.harnessSource, next.fingerprints.harnessSource);
    const reported = withReportedModels(captured, [{ ...run, reportedModel: "reported" }, { ...run, reportedModel: null }]);
    assert.doesNotThrow(() => assertFrozenExecution(captured, reported));
    assert.deepEqual(reported.reportedModels, ["reported", null]);
    assert.equal(reported.fingerprints.reportedModels, fingerprint(["reported", null]));
    assert.equal(reported.fingerprints.tooling, captured.fingerprints.tooling);
    assert.deepEqual(captured, original);
  } finally { await cleanup(); }
});

test("Layer 1 checker digests leave out later layers' checkers while other suites keep them", async () => {
  const { root, cleanup } = await temporaryHistory();
  try {
    const checks = join(root, "checks"), later = join(checks, [...LaterLayerCheckers][0] ?? "");
    await writeJson(join(checks, "check.json"), { checks: "layer one" });
    await writeJson(join(later, "check.json"), { checks: "before" });
    const layerOne = await checkerDigests(checks, LayerOneCase), other = await checkerDigests(checks, FixtureSuite.id);
    await writeJson(join(later, "check.json"), { checks: "after" });
    assert.deepEqual(await checkerDigests(checks, LayerOneCase), layerOne);
    assert.notDeepEqual(await checkerDigests(checks, FixtureSuite.id), other);
    await writeJson(join(checks, "check.json"), { checks: "changed" });
    assert.notDeepEqual(await checkerDigests(checks, LayerOneCase), layerOne);
  } finally { await cleanup(); }
});
