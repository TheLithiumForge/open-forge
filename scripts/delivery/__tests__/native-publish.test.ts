import assert from "node:assert/strict";
import { test } from "node:test";
import { nativeCompilerProperties, ShippedCliFolder } from "../native-publish.ts";

test("only osx-arm64 test executables are optimized for size", () => {
  assert.deepEqual(nativeCompilerProperties("osx-arm64", "integration"), ["-p:OptimizationPreference=Size"]);
  assert.deepEqual(nativeCompilerProperties("osx-arm64", "end-to-end"), ["-p:OptimizationPreference=Size"]);
  assert.deepEqual(nativeCompilerProperties("osx-arm64", ShippedCliFolder), []);
  for (const rid of ["osx-x64", "linux-x64", "linux-arm64", "win-x64", "win-arm64"] as const) assert.deepEqual(nativeCompilerProperties(rid, "integration"), []);
});
