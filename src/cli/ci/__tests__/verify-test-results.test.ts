import assert from "node:assert/strict";
import test from "node:test";

import { qualifyTestResults } from "../verify-test-results.ts";
import type { SuiteExpectation, TestEvidenceInput } from "../verify-test-results.ts";

const subject = "OwnedEvidenceTests";
const namespace = "OpenForge.Ci";
const type = `${namespace}.${subject}`;
const method = "PreservesOwnedEvidence";
const firstCase = "Owned evidence case one";
const secondCase = "Owned evidence case two";
const sourcePath = "src/cli/tests/unit/OwnedEvidenceTests.cs";
const sourceSha256 = "a".repeat(64);
const expected: SuiteExpectation = { discovered: 2, executed: 2, deferredTheories: [], publicSubjects: {} };

function fixture() {
  const suiteErrors: unknown[] = [];
  const discovery = {
    schemaVersion: 1,
    tests: [firstCase, secondCase].map((displayName, index) => ({
      uid: `discovery-${index}`,
      displayName,
      type: { namespace, typeName: subject, methodName: method },
    })),
  };
  const report = {
    reportFormat: "CTRF",
    specVersion: "0.0.0",
    results: {
      summary: { tests: 2, passed: 2, failed: 0, skipped: 0, pending: 0, other: 0 },
      extra: { suites: [{ errors: suiteErrors }] },
      tests: [firstCase, secondCase].map((name, index) => ({ name, status: "passed", extra: { id: `execution-${index}`, type, method } })),
    },
  };
  return { discovery, report, discoveryExit: 0, executionExit: 0, expected, sourceHashes: {} };
}

function rejects(input: TestEvidenceInput, message: string): void {
  assert.throws(() => qualifyTestResults(input), { message });
}

test("Unit: complete owned cases qualify without assuming portable runtime identifiers", () => {
  const result = qualifyTestResults(fixture());
  assert.deepEqual(result, {
    state: "passed",
    discovered: 2,
    executed: 2,
    deferredTheories: 0,
    cases: [firstCase, secondCase].map((name) => ({ identity: { type, method, name }, count: 1 })),
  });
});

test("Unit: case multiplicity survives repeated display names", () => {
  const input = fixture();
  input.discovery.tests[1]!.displayName = firstCase;
  input.report.results.tests[1]!.name = firstCase;
  assert.deepEqual(qualifyTestResults(input).cases, [{ identity: { type, method, name: firstCase }, count: 2 }]);
});

test("Unit: the exact frozen deferred theory expands to independently named runtime cases", () => {
  const input = fixture();
  const placeholder = "Owned deferred theory";
  input.discovery.tests = [{ uid: "placeholder", displayName: placeholder, type: { namespace, typeName: subject, methodName: method } }];
  const result = qualifyTestResults({
    ...input,
    sourceHashes: { [sourcePath]: sourceSha256 },
    expected: {
      ...expected,
      discovered: 1,
      deferredTheories: [
        {
          placeholder: { type, method, name: placeholder },
          sourcePath,
          sourceSha256,
          executedCases: [firstCase, secondCase].map((name) => ({ identity: { type, method, name }, count: 1 })),
        },
      ],
    },
  });
  assert.equal(result.deferredTheories, 1);
  assert.equal(result.executed, 2);
});

test("Unit: empty discovery cannot qualify an empty successful report", () => {
  const input = fixture();
  input.discovery.tests = [];
  input.report.results.tests = [];
  input.report.results.summary.tests = 0;
  input.report.results.summary.passed = 0;
  rejects(input, "test-count-mismatch");
});

test("Unit: partial result arrays cannot borrow a successful total", () => {
  const input = fixture();
  input.report.results.tests.pop();
  rejects(input, "test-summary-mismatch");
});

test("Unit: replacing a missing case with another copy cannot compensate within one method", () => {
  const input = fixture();
  input.report.results.tests[1]!.name = firstCase;
  rejects(input, "test-case-mismatch");
});

test("Unit: skipped required cases reject even if the summary claims every case passed", () => {
  const input = fixture();
  input.report.results.tests[0]!.status = "skipped";
  rejects(input, "test-case-not-passed");
});

test("Unit: suite-level errors reject otherwise successful cases", () => {
  const input = fixture();
  input.report.results.extra.suites[0]!.errors.push({ message: "Owned fixture error" });
  rejects(input, "test-suite-errors");
});

test("Unit: failed pending or other summary counts cannot qualify an all-passed case array", () => {
  for (const field of ["failed", "pending", "skipped", "other"] as const) {
    const input = fixture();
    input.report.results.summary[field] = 1;
    rejects(input, "test-summary-mismatch");
  }
});

test("Unit: an undeclared deferred expansion cannot invent accepted case names", () => {
  const input = fixture();
  input.discovery.tests.pop();
  rejects({ ...input, expected: { ...expected, discovered: 1 } }, "test-case-mismatch");
});

test("Unit: duplicate execution identifiers cannot hide double counting", () => {
  const input = fixture();
  input.report.results.tests[1]!.extra.id = input.report.results.tests[0]!.extra.id;
  rejects(input, "test-duplicate-id");
});

test("Unit: duplicate discovery identifiers cannot select the same case twice", () => {
  const input = fixture();
  input.discovery.tests[1]!.uid = input.discovery.tests[0]!.uid;
  rejects(input, "test-duplicate-id");
});

test("Unit: an executed count below the frozen expectation cannot redefine acceptance", () => {
  rejects({ ...fixture(), expected: { ...expected, executed: 3 } }, "test-count-mismatch");
});

test("Unit: discovery and execution process failures cannot become passing evidence", () => {
  rejects({ ...fixture(), discoveryExit: 1 }, "test-process-failed");
  rejects({ ...fixture(), executionExit: 1 }, "test-process-failed");
});

test("Unit: malformed or unsupported documents fail at the report boundary", () => {
  rejects({ ...fixture(), discovery: { schemaVersion: 2, tests: [] } }, "test-document-invalid");
  rejects({ ...fixture(), report: { results: {} } }, "test-document-invalid");
});

test("Unit: frozen deferred source changes invalidate its expansion", () => {
  rejects(
    {
      ...fixture(),
      sourceHashes: { [sourcePath]: "b".repeat(64) },
      expected: { ...expected, deferredTheories: [{ placeholder: { type, method, name: firstCase }, sourcePath, sourceSha256, executedCases: [] }] },
    },
    "test-deferred-source-changed",
  );
});

test("Unit: public subject totals must match the independent command inventory", () => {
  assert.equal(qualifyTestResults({ ...fixture(), expected: { ...expected, publicSubjects: { [subject]: 2 } } }).state, "passed");
  rejects({ ...fixture(), expected: { ...expected, publicSubjects: { [subject]: 1, MissingCommandTests: 1 } } }, "test-public-inventory-mismatch");
});
