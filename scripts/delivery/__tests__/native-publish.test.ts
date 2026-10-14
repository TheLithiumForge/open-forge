import assert from "node:assert/strict";
import { test } from "node:test";
import { nativeCompilerProperties } from "../native-publish.ts";

test("only the low-memory macOS arm64 runner compiles with the workstation collector", () => {
  assert.deepEqual(nativeCompilerProperties("osx-arm64"), ["-p:IlcUseServerGc=false"]);
  for (const rid of ["osx-x64", "linux-x64", "linux-arm64", "win-x64", "win-arm64"] as const) assert.deepEqual(nativeCompilerProperties(rid), []);
});
