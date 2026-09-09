import assert from "node:assert/strict";

export interface TestCaseIdentity {
  readonly type: string;
  readonly method: string;
  readonly name: string;
}

export interface DiscoveredCase extends TestCaseIdentity {
  readonly uid: string;
  readonly subject: string;
}

export interface ExecutedCase extends TestCaseIdentity {
  readonly uid: string;
  readonly status: string;
}

export interface TestResultDocument {
  readonly tests: readonly ExecutedCase[];
  readonly summary: Readonly<Record<"tests" | "passed" | "failed" | "pending" | "skipped" | "other", number>>;
  readonly suiteErrors: readonly unknown[];
}

export function readDiscovery(document: unknown): readonly DiscoveredCase[] {
  const value = object(document);
  assert.ok(value["schemaVersion"] === 1 && Array.isArray(value["tests"]), "test-document-invalid");
  return value["tests"].map((input: unknown) => {
    const row = object(input);
    const type = object(row["type"]);
    const subject = text(type["typeName"]);
    return { uid: text(row["uid"]), name: text(row["displayName"]), type: `${text(type["namespace"])}.${subject}`, method: text(type["methodName"]), subject };
  });
}

export function readTestResults(document: unknown): TestResultDocument {
  const value = object(document);
  assert.ok(value["reportFormat"] === "CTRF" && value["specVersion"] === "0.0.0", "test-document-invalid");
  const results = object(value["results"]);
  const summary = object(results["summary"]);
  const suites = object(results["extra"])["suites"];
  assert.ok(Array.isArray(results["tests"]) && Array.isArray(suites) && suites.length > 0, "test-document-invalid");
  const tests = results["tests"].map((input: unknown) => {
    const row = object(input);
    const extra = object(row["extra"]);
    return { uid: text(extra["id"]), name: text(row["name"]), status: text(row["status"]), type: text(extra["type"]), method: text(extra["method"]) };
  });
  const suiteErrors = suites.flatMap((suite: unknown): unknown[] => {
    const errors = object(suite)["errors"];
    assert.ok(Array.isArray(errors), "test-document-invalid");
    return errors;
  });
  return {
    tests,
    suiteErrors,
    summary: {
      tests: count(summary["tests"]),
      passed: count(summary["passed"]),
      failed: count(summary["failed"]),
      skipped: count(summary["skipped"]),
      pending: count(summary["pending"]),
      other: count(summary["other"]),
    },
  };
}

function object(value: unknown): Record<string, unknown> {
  assert.ok(typeof value === "object" && value !== null && !Array.isArray(value), "test-document-invalid");
  return Object.fromEntries(Object.entries(value));
}

function text(value: unknown): string {
  assert.ok(typeof value === "string" && value.length > 0, "test-document-invalid");
  return value;
}

function count(value: unknown): number {
  assert.ok(typeof value === "number" && Number.isSafeInteger(value) && value >= 0, "test-document-invalid");
  return value;
}
