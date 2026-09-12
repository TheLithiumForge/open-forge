import assert from "node:assert/strict";
import { parseArgs } from "node:util";

export function readDeliveryOptions() {
  return parseArgs({ options: { rid: { type: "string" }, sha: { type: "boolean" }, offline: { type: "boolean" } } }).values;
}

export function readBuildOptions() {
  const values = readDeliveryOptions();
  assert.ok(!values.offline, "--offline applies to restore only; builds never restore.");
  return values;
}
