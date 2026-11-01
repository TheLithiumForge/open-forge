import assert from "node:assert/strict";
import { test } from "node:test";
import { matchingFailure } from "./mutation-report.ts";

test("a kill needs both the declared test and its declared assertion", () => {
  const expected = { name: "matching pair", message: "R03.2 matching pair stays matched" };
  assert.equal(matchingFailure([expected], "matching pair", "R03.2 matching pair stays matched"), expected);
  assert.equal(matchingFailure([{ ...expected, name: "unrelated test" }], "matching pair", "R03.2 matching pair stays matched"), undefined);
  assert.equal(matchingFailure([{ ...expected, message: "browser launch failed" }], "matching pair", "R03.2 matching pair stays matched"), undefined);
  assert.equal(matchingFailure([], "matching pair", "R03.2 matching pair stays matched"), undefined);
});
