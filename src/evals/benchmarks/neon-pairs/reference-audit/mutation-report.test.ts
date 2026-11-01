import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { mkdtemp, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { test } from "node:test";
import { failures, matchingFailure } from "./mutation-report.ts";
import { exactName } from "./mutations.ts";

test("a kill needs both the declared test and its declared assertion", () => {
  const expected = { name: "matching pair", message: "R03.2 matching pair stays matched" };
  assert.equal(matchingFailure([expected], "matching pair", "R03.2 matching pair stays matched"), expected);
  assert.equal(matchingFailure([{ ...expected, name: "unrelated test" }], "matching pair", "R03.2 matching pair stays matched"), undefined);
  assert.equal(matchingFailure([{ ...expected, message: "browser launch failed" }], "matching pair", "R03.2 matching pair stays matched"), undefined);
  assert.equal(matchingFailure([], "matching pair", "R03.2 matching pair stays matched"), undefined);
});

test("the failure reporter records each failed test with its own assertion message", async () => {
  const folder = await mkdtemp(join(tmpdir(), "failure-reporter-"));
  try {
    const sample = join(folder, "sample.test.ts");
    await writeFile(sample, 'import assert from "node:assert/strict";\nimport { test } from "node:test";\ntest("passing", () => {});\ntest("failing", () => assert.equal(1, 2, "declared reason"));\n');
    const report = join(folder, "failures.json");
    const reporter = new URL("./failure-reporter.ts", import.meta.url).href;
    // Without this, the nested run would report to the runner of this test instead.
    const result = spawnSync(process.execPath, ["--test", "--test-reporter", reporter, "--test-reporter-destination", report, sample], { encoding: "utf8", env: { ...process.env, NODE_TEST_CONTEXT: undefined } });
    assert.equal(result.status, 1);
    const found = await failures(report, true);
    assert.deepEqual(found.map((failure) => failure.name), ["failing"]);
    assert.ok(matchingFailure(found, "failing", "declared reason"));
  } finally {
    await rm(folder, { recursive: true });
  }
});

test("a browser name filter matches its declared test name and nothing longer", () => {
  const name = "Tab and Shift+Tab enter (once)";
  const filter = new RegExp(exactName(name));
  assert.ok(filter.test(name));
  assert.equal(filter.test(`${name} again`), false);
});
