import assert from "node:assert/strict";
import { parseArgs } from "node:util";
import { repositoryRoot } from "../repository.ts";
import { sourceIdentity } from "../source.ts";
import { committedVersion } from "../version.ts";
import { reportFailure } from "../process.ts";
import { publishPackages } from "../npm/publish-packages.ts";
import { readReleasePublications } from "./release-publications.ts";

try {
  const { values } = parseArgs({ options: { from: { type: "string", default: "artifacts/release" }, tag: { type: "string" }, "dry-run": { type: "boolean" } } });
  assert.ok(values.tag, "Publication requires an explicit --tag.");
  const publications = readReleasePublications(repositoryRoot, values.from, sourceIdentity(repositoryRoot), committedVersion(repositoryRoot));
  publishPackages(repositoryRoot, publications, values.tag, values["dry-run"] === true);
} catch (error) {
  reportFailure(error);
}
