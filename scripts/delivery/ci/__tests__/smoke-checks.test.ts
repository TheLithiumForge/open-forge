import assert from "node:assert/strict";
import { test } from "node:test";
import { evaluateSmoke, SmokeCheck, SmokeText } from "../smoke-checks.ts";
import { ProcessExit } from "../../process.ts";

const Version = "0.9.0-beta.11";
const RecordedOutput = {
  version: `${Version}\r\n`,
  status: `${SmokeText.current}\n`,
  configure: `Preview\n  .agents/loader.md  ${SmokeText.entries}\n`,
  replaced: `Existing file would be ${SmokeText.replaced}\n`,
} as const;
const success = (stdout: string, stderr = "") => ({ status: ProcessExit.success, stdout, stderr });

test("public smoke evaluates each recorded command output by name", () => {
  assert.equal(evaluateSmoke(SmokeCheck.version, success(RecordedOutput.version), Version), true);
  assert.equal(evaluateSmoke(SmokeCheck.version, success("another-version"), Version), false);
  assert.equal(evaluateSmoke(SmokeCheck.install, success("Installed Essentials."), Version), true);
  assert.equal(evaluateSmoke(SmokeCheck.status, success(RecordedOutput.status), Version), true);
  assert.equal(evaluateSmoke(SmokeCheck.status, success("Update required."), Version), false);
  assert.equal(evaluateSmoke(SmokeCheck.configure, success(RecordedOutput.configure), Version), true);
  assert.equal(evaluateSmoke(SmokeCheck.configure, success(RecordedOutput.configure, RecordedOutput.replaced), Version), false);
  assert.equal(evaluateSmoke(SmokeCheck.configure, success("No changes."), Version), false);
  for (const name of Object.values(SmokeCheck)) assert.equal(evaluateSmoke(name, { ...success(RecordedOutput.status), status: ProcessExit.failure }, Version), false);
});
