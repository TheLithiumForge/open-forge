import assert from "node:assert/strict";
import { test } from "node:test";
import { candidateVersion } from "../version.ts";

const sha = "1234567890abcdef1234567890abcdef12345678";

test("candidate versions preserve the committed prerelease", () => {
  assert.equal(candidateVersion("0.1.0-beta.1"), "0.1.0-beta.1");
});

test("SHA candidates append to the existing prerelease or create a development prerelease", () => {
  assert.equal(candidateVersion("0.1.0-beta.1", sha), `0.1.0-beta.1.sha-${sha}`);
  assert.equal(candidateVersion("1.2.3", sha), `1.2.3-dev.sha-${sha}`);
});

test("invalid candidate inputs reject before any artifact is created", () => {
  assert.throws(() => candidateVersion("1.2.3", "short"), /SHA/);
  assert.throws(() => candidateVersion("unversioned"), /version/i);
});
