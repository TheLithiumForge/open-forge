import assert from "node:assert/strict";
import { npm, reportFailure } from "./process.ts";
import { repositoryRoot } from "./repository.ts";

try {
  const args = process.argv.slice(2);
  assert.ok(args.length > 0, "Supply patch, minor, major, prerelease or an explicit version.");
  npm(["version", ...args, "--no-git-tag-version"], repositoryRoot);
} catch (error) {
  reportFailure(error);
}
