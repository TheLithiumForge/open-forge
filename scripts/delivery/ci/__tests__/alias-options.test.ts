import assert from "node:assert/strict";
import { test } from "node:test";
import { readOptions } from "../../options.ts";

const Version = "0.9.0-beta.18";

test("public smoke keeps new channels opt-in for versions published before they exist", () => {
  const options = readOptions("smoke:public", ["--version", Version]);
  assert.equal(options?.aliases, undefined);
  assert.equal(readOptions("smoke:public", ["--version", Version, "--aliases"])?.aliases, true);
});
