import assert from "node:assert/strict";
import { test } from "node:test";
import { failureExcerpt } from "../process.ts";
import { stageFailureAnnotation } from "../stage.ts";

test("a failed stage becomes one escaped GitHub error annotation", () => {
  assert.equal(
    stageFailureAnnotation("Publish native integration (osx-arm64)", "dotnet failed (1); build.log\nerror: out of memory"),
    "::error title=Publish native integration (osx-arm64) failed::dotnet failed (1); build.log%0Aerror: out of memory",
  );
  assert.equal(stageFailureAnnotation("a: b, c", "x"), "::error title=a%3A b%2C c failed::x");
});

test("a failure excerpt keeps the last error lines, or the last lines when none name an error", () => {
  const output = ["Restoring", "  warning: slow", "ILC: error IL1005: one", "", "Linking", "ILC exited with code 137", "Done"].join("\n");
  assert.deepEqual(failureExcerpt(output, 12), ["ILC: error IL1005: one", "ILC exited with code 137"]);
  assert.deepEqual(failureExcerpt(output, 1), ["ILC exited with code 137"]);
  assert.deepEqual(failureExcerpt("one\r\ntwo\nthree\n", 2), ["two", "three"]);
  assert.deepEqual(failureExcerpt("Generating native code\nld: b(l) ARM64 branch out of range\nclang: error: linker command failed", 12), [
    "ld: b(l) ARM64 branch out of range",
    "clang: error: linker command failed",
  ]);
});
