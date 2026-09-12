import assert from "node:assert/strict";
import { test } from "node:test";
import { AllTargets, parseTargets, readTargets } from "../targets.ts";
import { wrapperManifest } from "../npm/package-manifests.ts";

const version = "1.2.3";

test("target selection uses deterministic supported RIDs and rejects ambiguous selections", () => {
  assert.deepEqual(parseTargets(), AllTargets);
  assert.deepEqual(parseTargets(" win-x64,osx-x64,linux-x64 "), ["osx-x64", "linux-x64", "win-x64"]);
  for (const input of ["", "linux-x64,", "linux-x64,linux-x64", "darwin-x64", "all", "linux-mips"]) assert.throws(() => parseTargets(input));
  for (const input of [undefined, {}, [], [null]]) assert.throws(() => readTargets(input));
});

test("an x64 wrapper version advertises exactly its selected native packages", () => {
  assert.deepEqual(wrapperManifest(version, parseTargets("linux-x64,win-x64,osx-x64")).optionalDependencies, {
    "@thelithiumforge/open-forge-darwin-x64": version,
    "@thelithiumforge/open-forge-linux-x64": version,
    "@thelithiumforge/open-forge-win-x64": version,
  });
});
