import assert from "node:assert/strict";
import { test } from "node:test";
import { productTests } from "./tests.ts";

test("product discovery keeps reference game tests outside the product suite", async () => {
  const files = await productTests();
  assert.ok(files.length > 0);
  assert.equal(files.some((path) => path.split(/[\\/]/).includes("reference")), false);
});
