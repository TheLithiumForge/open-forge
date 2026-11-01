import assert from "node:assert/strict";
import { cp, mkdir, mkdtemp, readFile, rm, writeFile, access } from "node:fs/promises";
import { spawnSync } from "node:child_process";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";
import { isJsonObject } from "../reference/src/game/saved-format.ts";
import { failures, matchingFailure } from "./mutation-report.ts";

const AUDIT = dirname(fileURLToPath(import.meta.url));
const ROOT = resolve(AUDIT, "../reference");
const WORK = join(ROOT, ".test-work");
const VITEST = join(ROOT, "node_modules/vitest/vitest.mjs");
// Browser failures reach the harness through a reporter, so the game's tests need no audit hook.
const FAILURE_REPORTER = pathToFileURL(join(AUDIT, "failure-reporter.ts")).href;
const INPUTS = [
  ".npmrc",
  "src",
  "tests",
  "scripts",
  "art",
  "package.json",
  "package-lock.json",
  "index.html",
  "vite.config.ts",
  "vite.art.config.ts",
  "vite.session.config.ts",
  "vitest.config.ts",
];
interface Patch {
  path: string;
  find: string;
  replace: string;
}
interface Mutation extends Patch {
  id: string;
  clauses: string[];
  test: string;
  testName: string;
  reason: string;
  browser: boolean;
  patches: Patch[];
}
function patch(value: unknown): Patch {
  assert.ok(isJsonObject(value) && typeof value["path"] === "string" && typeof value["find"] === "string" && typeof value["replace"] === "string");
  return { path: value["path"], find: value["find"], replace: value["replace"] };
}
function mutation(value: unknown): Mutation {
  assert.ok(
    isJsonObject(value) &&
      typeof value["id"] === "string" &&
      typeof value["test"] === "string" &&
      typeof value["testName"] === "string" &&
      typeof value["reason"] === "string" &&
      typeof value["browser"] === "boolean",
  );
  assert.ok(Array.isArray(value["clauses"]) && value["clauses"].every((c): c is string => typeof c === "string"));
  assert.ok(value["patches"] === undefined || Array.isArray(value["patches"]));
  return {
    ...patch(value),
    id: value["id"],
    clauses: value["clauses"],
    test: value["test"],
    testName: value["testName"],
    reason: value["reason"],
    browser: value["browser"],
    patches: (value["patches"] ?? []).map(patch),
  };
}
// Node reads the name filter as a regular expression, so a name such as Shift+Tab must match itself.
export function exactName(name: string): string {
  return `^${name.replace(/[.*+?^${}()|[\]\\]/g, "\\$&")}$`;
}
function run(folder: string, args: string[]) {
  const result = spawnSync(process.execPath, args, {
    cwd: folder,
    encoding: "utf8",
    env: { ...process.env, TEMP: join(ROOT, ".browser-work"), TMP: join(ROOT, ".browser-work") },
    timeout: 120_000,
    maxBuffer: 4 * 1024 * 1024,
  });
  if (result.error) throw result.error;
  return { status: result.status, output: result.stdout + result.stderr };
}
function sanitize(text: string, folder: string): string {
  return text
    .replaceAll(folder, "reference")
    .replaceAll(folder.replaceAll("\\", "/"), "reference")
    .replaceAll(ROOT, "reference")
    .replaceAll(ROOT.replaceAll("\\", "/"), "reference")
    .replaceAll("file:///reference/", "reference/")
    .replace(/\u001b\[[0-9;]*m/g, "");
}
export async function mutationCheck(): Promise<void> {
  const input: unknown = JSON.parse(await readFile(new URL("./mutations.fixture.json", import.meta.url), "utf8"));
  assert.ok(Array.isArray(input));
  const mutations = input.map(mutation);
  assert.equal(mutations.length, 35);
  await mkdir(WORK, { recursive: true });
  assert.equal(run(ROOT, [VITEST, "run"]).status, 0, "Clean unit baseline");
  const browserFiles = [...new Set(mutations.filter((m) => m.browser).map((m) => m.test))];
  assert.equal(run(ROOT, ["--test", "--test-concurrency=1", ...browserFiles]).status, 0, "Clean browser baseline");
  const results = [];
  for (const m of mutations.filter((m) => process.argv.length === 2 || process.argv.slice(2).includes(m.id))) {
    const folder = await mkdtemp(join(WORK, "mutation-"));
    try {
      for (const path of INPUTS) await cp(join(ROOT, path), join(folder, path), { recursive: true });
      for (const p of [m, ...m.patches]) {
        const path = join(folder, p.path),
          text = (await readFile(path, "utf8")).replaceAll("\r\n", "\n");
        assert.equal(text.split(p.find).length, 2, `${m.id} exact patch target`);
        await writeFile(
          path,
          text.replace(p.find, () => p.replace),
        );
      }
      if (m.browser) {
        assert.equal(run(folder, [join(ROOT, "node_modules/vite/bin/vite.js"), "build"]).status, 0, `${m.id} built game`);
        assert.equal(run(folder, [join(ROOT, "node_modules/vite/bin/vite.js"), "build", "--config", "vite.art.config.ts"]).status, 0, `${m.id} built diagnostic`);
        assert.equal(run(folder, [join(ROOT, "node_modules/vite/bin/vite.js"), "build", "--config", "vite.session.config.ts"]).status, 0, `${m.id} built session harness`);
        if (m.id === "M19") assert.match(await readFile(join(folder, "dist/index.html"), "utf8"), /outline-offset:4px/, "M19 changed CSS reaches the delivered game");
      }
      const report = join(folder, "failure.json");
      const result = m.browser
        ? run(folder, ["--test", "--test-reporter", FAILURE_REPORTER, "--test-reporter-destination", report, "--test-reporter", "spec", "--test-reporter-destination", "stdout", "--test-name-pattern", exactName(m.testName), m.test])
        : run(folder, [VITEST, "run", m.test, "--reporter=json", "--outputFile", report]);
      const exists = await access(report).then(
        () => true,
        () => false,
      );
      const found = exists ? await failures(report, m.browser) : [];
      const target = matchingFailure(found, m.testName, m.reason);
      const caught = result.status === 1 && target !== undefined;
      results.push({
        id: m.id,
        clauses: m.clauses,
        test: m.test,
        testName: m.testName,
        reason: m.reason,
        outcome: caught ? "caught" : "survived",
        exit: result.status,
        assertion: target ? sanitize(target.message, folder) : null,
        otherFailures: found.filter((f) => f !== target).map((f) => ({ name: f.name, message: sanitize(f.message, folder) })),
      });
      process.stdout.write(`${m.id}: ${caught ? "caught" : "survived"} for ${m.reason}\n`);
      if (!caught) process.stdout.write(sanitize(result.output, folder));
    } finally {
      await rm(folder, { recursive: true });
    }
  }
  await writeFile(
    join(AUDIT, "mutation-results.json"),
    JSON.stringify({ schemaVersion: 2, node: process.versions.node, cleanUnitBaseline: "passed", cleanBrowserBaseline: "passed", results }, null, 2) + "\n",
  );
  assert.ok(
    results.every((r) => r.outcome === "caught"),
    "Every mutation must fail its declared assertion",
  );
}
if (import.meta.main) await mutationCheck();
