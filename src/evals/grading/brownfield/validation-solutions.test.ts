import assert from "node:assert/strict";
import { existsSync } from "node:fs";
import { readFile, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { BrownfieldFixture } from "../../materialize/brownfield-test-fixture.ts";
import { BrownfieldFiles } from "../../materialize/brownfield-case.ts";
import { loadBrownfieldSuite } from "../../materialize/brownfield.ts";
import { writeNew } from "../../materialize/files.ts";
import { CheckStatus } from "../../history/statuses.ts";
import { validationFixture, ValidationFixturePage } from "./validation-test-fixture.ts";
import { validateCase } from "./validate.ts";
import { ValidationCheck, ValidationPhase } from "./validation-expectations.ts";
import { ProjectTestCommands } from "./project-tests.ts";

const DifferentSource = "src/different-name.ts";
const OutputFolders = [".art", ".session", ".browser-work"];

test("both solution patches run the project's test commands before gates and ignored output cannot fail scope", async (t) => {
  const fixture = await validationFixture(t);
  await writeFile(join(fixture.reference, ".gitignore"), `${OutputFolders.map((folder) => `${folder}/`).join("\n")}\ndist/\n`);
  const patch = `--- a/${BrownfieldFixture.source}\n+++ /dev/null\n@@ -1 +0,0 @@\n-${BrownfieldFixture.broken.trimEnd()}\n--- /dev/null\n+++ b/${DifferentSource}\n@@ -0,0 +1 @@\n+${BrownfieldFixture.repaired.trimEnd()}\n`;
  await writeNew(fixture.caseRoot, BrownfieldFiles.alternative, patch);
  const suite = await loadBrownfieldSuite(fixture.caseRoot, undefined, fixture.reference, fixture.destination);
  assert.equal(suite.brownfield?.case.altSolutionPatch, BrownfieldFiles.alternative);
  const order: string[] = [];
  const result = await validateCase(suite, { ...fixture.tools,
    build: async (space) => {
      const path = existsSync(join(space.work, BrownfieldFixture.source)) ? BrownfieldFixture.source : DifferentSource;
      order.push(`build:${path}`);
      await writeNew(space.work, ValidationFixturePage, "<html>Fixture page</html>");
      return { passed: true, typecheck: true, seconds: 0, packages: null, html: ValidationFixturePage, bytes: null, log: "Fixture build." };
    },
    projectTests: async (space) => {
      for (const command of Object.values(ProjectTestCommands)) order.push(command.join(" "));
      for (const folder of OutputFolders) await writeNew(space.work, `${folder}/output.txt`, "Fixture browser output.");
      return { unit: { code: 0, stdout: "1 passed", stderr: "", timedOut: false }, browser: { code: 0, stdout: "1 passed", stderr: "", timedOut: false } };
    },
    gates: { ...fixture.tools.gates, changes: async (_item, checks, input) => {
      const path = existsSync(join(input.project, BrownfieldFixture.source)) ? BrownfieldFixture.source : DifferentSource;
      const fixed = await readFile(join(input.project, path), "utf8") === BrownfieldFixture.repaired;
      if (fixed) for (const folder of OutputFolders) assert.ok(await readFile(join(input.project, folder, "output.txt")));
      order.push(`gates:${path}`);
      return checks.map((check) => ({ id: `${check.kind}-${check.id}`, area: check.kind, status: fixed ? CheckStatus.pass : CheckStatus.fail, evidence: "Fixture change evidence." }));
    } },
  });
  assert.equal(result.passed, true);
  assert.deepEqual(result.stages.map((stage) => stage.phase), Object.values(ValidationPhase));
  assert.equal(result.stages[2]?.patch, BrownfieldFiles.alternative);
  assert.deepEqual(order, [`build:${BrownfieldFixture.source}`, `gates:${BrownfieldFixture.source}`,
    `build:${BrownfieldFixture.source}`, ...Object.values(ProjectTestCommands).map((command) => command.join(" ")), `gates:${BrownfieldFixture.source}`,
    `build:${DifferentSource}`, ...Object.values(ProjectTestCommands).map((command) => command.join(" ")), `gates:${DifferentSource}`]);
  assert.ok(result.stages.every((stage) => stage.pending.length > 0));
});

test("a failing project test fails validation even when all solution gates pass", async (t) => {
  const fixture = await validationFixture(t);
  const result = await validateCase(fixture.suite, { ...fixture.tools, projectTests: async () => ({
    unit: { code: 1, stdout: "1 failed", stderr: "", timedOut: false }, browser: { code: 0, stdout: "1 passed", stderr: "", timedOut: false },
  }) });
  assert.equal(result.passed, false);
  assert.equal(result.stages[1]?.outcomes.find((entry) => entry.id === ValidationCheck.unit)?.matches, false);
  assert.match(result.stages[1]?.outcomes.find((entry) => entry.id === ValidationCheck.unit)?.evidence ?? "", /1 failed/);
  assert.equal(result.stages[1]?.outcomes.find((entry) => entry.id === ValidationCheck.browser)?.matches, true);
});
