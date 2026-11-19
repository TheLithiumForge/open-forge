import assert from "node:assert/strict";
import { test } from "node:test";
import { readOptions } from "../../options.ts";
import { CiCommand } from "../command-names.ts";
import { SmokeArguments } from "../smoke-checks.ts";

const Version = "0.9.0-beta.18";

test("public smoke keeps new channels opt-in for versions published before they exist", () => {
  const options = readOptions(CiCommand.smoke, [...SmokeArguments.version, Version]);
  assert.equal(options?.aliases, undefined);
  assert.equal(readOptions(CiCommand.smoke, [...SmokeArguments.version, Version, ...SmokeArguments.aliases])?.aliases, true);
});
