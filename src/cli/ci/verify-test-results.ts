import type { TestCaseIdentity } from "./test-result-documents.ts";
import assert from "node:assert/strict";
import { readFileSync, writeFileSync } from "node:fs";
import { join } from "node:path";
import { observeArtifact, readArtifact, readCandidate, verifyArtifactManifest } from "./artifact-manifest.ts";
import inventory from "./test-inventory.json" with { type: "json" };
import { readDiscovery, readTestResults } from "./test-result-documents.ts";

export interface DeferredTheory {
  readonly placeholder: TestCaseIdentity;
  readonly sourcePath: string;
  readonly sourceSha256: string;
  readonly executedCases: readonly { readonly identity: TestCaseIdentity; readonly count: number }[];
}

export interface SuiteExpectation {
  readonly discovered: number;
  readonly executed: number;
  readonly deferredTheories: readonly DeferredTheory[];
  readonly publicSubjects: Readonly<Record<string, number>>;
}

export interface TestEvidenceInput {
  readonly discovery: unknown;
  readonly report: unknown;
  readonly discoveryExit: number;
  readonly executionExit: number;
  readonly expected: SuiteExpectation;
  readonly sourceHashes: Readonly<Record<string, string>>;
}

export interface QualifiedTestResults {
  readonly state: "passed";
  readonly discovered: number;
  readonly executed: number;
  readonly deferredTheories: number;
  readonly cases: readonly { readonly identity: TestCaseIdentity; readonly count: number }[];
}

export function qualifyTestResults(input: TestEvidenceInput): QualifiedTestResults {
  assert.ok(input.discoveryExit === 0 && input.executionExit === 0, "test-process-failed");
  const discovery = readDiscovery(input.discovery);
  const report = readTestResults(input.report);
  const summary = report.summary;
  assert.ok(
    summary.tests === report.tests.length && summary.passed === summary.tests && summary.failed === 0 && summary.skipped === 0 && summary.pending === 0 && summary.other === 0,
    "test-summary-mismatch",
  );
  assert.ok(
    report.tests.every((test) => test.status === "passed"),
    "test-case-not-passed",
  );
  assert.ok(report.suiteErrors.length === 0, "test-suite-errors");
  assert.ok(
    new Set(discovery.map((test) => test.uid)).size === discovery.length && new Set(report.tests.map((test) => test.uid)).size === report.tests.length,
    "test-duplicate-id",
  );
  assert.ok(discovery.length > 0 && discovery.length === input.expected.discovered && report.tests.length === input.expected.executed, "test-count-mismatch");
  const expectedCases = caseCounts(discovery);
  for (const theory of input.expected.deferredTheories) {
    assert.ok(input.sourceHashes[theory.sourcePath] === theory.sourceSha256, "test-deferred-source-changed");
    const placeholder = identityKey(theory.placeholder);
    assert.ok(expectedCases.get(placeholder) === 1 && theory.executedCases.length > 0, "test-case-mismatch");
    expectedCases.delete(placeholder);
    for (const row of theory.executedCases) {
      assert.ok(Number.isSafeInteger(row.count) && row.count > 0, "test-case-mismatch");
      const key = identityKey(row.identity);
      expectedCases.set(key, (expectedCases.get(key) ?? 0) + row.count);
    }
  }
  const executedCases = caseCounts(report.tests);
  assert.ok(expectedCases.size === executedCases.size && [...expectedCases].every(([key, count]) => executedCases.get(key) === count), "test-case-mismatch");
  if (Object.keys(input.expected.publicSubjects).length > 0) {
    const subjects = new Map<string, number>();
    for (const test of discovery) subjects.set(test.subject, (subjects.get(test.subject) ?? 0) + 1);
    const expectedSubjects = Object.entries(input.expected.publicSubjects);
    assert.ok(subjects.size === expectedSubjects.length && expectedSubjects.every(([subject, count]) => subjects.get(subject) === count), "test-public-inventory-mismatch");
  }
  const identities = new Map(report.tests.map((test) => [identityKey(test), { type: test.type, method: test.method, name: test.name }]));
  const cases = [...executedCases.keys()].sort().map((key) => {
    const identity = identities.get(key);
    const count = executedCases.get(key);
    assert.ok(identity && count !== undefined, "test-case-mismatch");
    return { identity, count };
  });
  return { state: "passed", discovered: discovery.length, executed: report.tests.length, deferredTheories: input.expected.deferredTheories.length, cases };
}

function identityKey(test: TestCaseIdentity): string {
  return JSON.stringify([test.type, test.method, test.name]);
}

function caseCounts(tests: readonly TestCaseIdentity[]): Map<string, number> {
  const counts = new Map<string, number>();
  for (const test of tests) {
    const key = identityKey(test);
    counts.set(key, (counts.get(key) ?? 0) + 1);
  }
  return counts;
}

if (import.meta.main && process.argv[2] === "record-execution") {
  const [root, buildPath, rid, executablePath, discoveryStatus, executionStatus, output, ...extra] = process.argv.slice(3);
  assert.ok(
    root && buildPath && rid && executablePath && discoveryStatus && executionStatus && output && extra.length === 0,
    "Provide record-execution, root, build manifest, RID, executable, discovery exit, execution exit and output JSON.",
  );
  assert.ok(/^(linux|osx|win)-(x64|arm64)$/.test(rid), "test-process-failed");
  assert.ok(
    /^\d+$/.test(discoveryStatus) && /^\d+$/.test(executionStatus) && Number.isSafeInteger(Number(discoveryStatus)) && Number.isSafeInteger(Number(executionStatus)),
    "test-process-failed",
  );
  const build: unknown = JSON.parse(readFileSync(buildPath, "utf8"));
  assert.ok(typeof build === "object" && build !== null && "candidate" in build && "files" in build && Array.isArray(build.files), "artifact-manifest-invalid");
  const candidate = readCandidate(build.candidate);
  const executable = build.files.map((file: unknown) => readArtifact(file)).find((file) => file.path === executablePath);
  assert.ok(executable, "artifact-missing");
  verifyArtifactManifest({ rootDirectory: root, candidate, manifest: { schemaVersion: 1, candidate, files: [executable] }, verifyModes: process.platform !== "win32" });
  const receipt = { candidate, rid, executable, discoveryExit: Number(discoveryStatus), executionExit: Number(executionStatus) };
  writeFileSync(output, `${JSON.stringify(receipt, null, 2)}\n`, { flag: "wx" });
  process.exit(0);
}

if (import.meta.main) {
  const [suite, root, discoveryPath, reportPath, executionPath, output, ...extra] = process.argv.slice(2);
  assert.ok(
    (suite === "unit" || suite === "integration" || suite === "public") && root && discoveryPath && reportPath && executionPath && output && extra.length === 0,
    "Provide suite, root, discovery, CTRF, execution and output paths.",
  );
  const execution: unknown = JSON.parse(readFileSync(join(root, executionPath), "utf8"));
  assert.ok(typeof execution === "object" && execution !== null && "candidate" in execution && "executable" in execution && "rid" in execution, "test-process-failed");
  assert.ok(
    "discoveryExit" in execution && typeof execution.discoveryExit === "number" && "executionExit" in execution && typeof execution.executionExit === "number",
    "test-process-failed",
  );
  const candidate = readCandidate(execution.candidate);
  const executable = readArtifact(execution.executable);
  verifyArtifactManifest({ rootDirectory: root, candidate, manifest: { schemaVersion: 1, candidate, files: [executable] }, verifyModes: process.platform !== "win32" });
  const deferredTheories = suite === "unit" ? inventory.deferredTheories : [];
  const sourceHashes = Object.fromEntries(deferredTheories.map((theory) => [theory.sourcePath, observeArtifact(root, theory.sourcePath).sha256]));
  const discovery: unknown = JSON.parse(readFileSync(join(root, discoveryPath), "utf8"));
  const report: unknown = JSON.parse(readFileSync(join(root, reportPath), "utf8"));
  const expected = { ...inventory.suites[suite], deferredTheories, publicSubjects: suite === "public" ? inventory.publicSubjects : {} };
  const qualification = qualifyTestResults({ discovery, report, expected, sourceHashes, discoveryExit: execution.discoveryExit, executionExit: execution.executionExit });
  const receipt = {
    schemaVersion: 1,
    candidate,
    rid: execution.rid,
    suite,
    executable,
    discovery: observeArtifact(root, discoveryPath),
    report: observeArtifact(root, reportPath),
    sourceHashes,
    ...qualification,
  };
  writeFileSync(output, `${JSON.stringify(receipt, null, 2)}\n`, { flag: "wx" });
}
