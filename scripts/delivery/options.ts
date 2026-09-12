import assert from "node:assert/strict";
import { parseArgs } from "node:util";

export function readDeliveryOptions() {
  return parseArgs({ options: { offline: { type: "boolean" } } }).values;
}

export function readBuildOptions(native = false, build = false) {
  const { values } = parseArgs({
    options: {
      rid: { type: "string" },
      sha: { type: "boolean" },
      offline: { type: "boolean" },
      "no-restore": { type: "boolean" },
    },
  });
  assert.ok(native || values.rid === undefined, "--rid applies to native artifacts only.");
  assert.ok((native && build) || values.sha === undefined, "--sha applies to native builds only.");
  assert.ok(build || (values.offline === undefined && values["no-restore"] === undefined), "Artifact consumers do not restore.");
  assert.ok(!(values.offline && values["no-restore"]), "Choose --offline or --no-restore, not both.");
  return values;
}
