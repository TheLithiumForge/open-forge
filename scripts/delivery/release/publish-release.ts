import assert from "node:assert/strict";
import { parseArgs } from "node:util";
import { repositoryRoot } from "../repository.ts";
import { sourceIdentity } from "../source.ts";
import { committedVersion } from "../version.ts";
import { reportFailure } from "../process.ts";
import { publishPackages } from "../npm/publish-packages.ts";
import { readReleasePublications } from "./release-publications.ts";
import { publicationStop } from "./publication-stop.ts";
import { ReleaseDirectory } from "./release-plan.ts";

const stop = publicationStop();
try {
  const { values } = parseArgs({ options: { from: { type: "string", default: ReleaseDirectory }, tag: { type: "string" }, "dry-run": { type: "boolean" } } });
  assert.ok(values.tag, "Publication requires an explicit --tag.");
  const publications = readReleasePublications(repositoryRoot, values.from, sourceIdentity(repositoryRoot), committedVersion(repositoryRoot));
  await publishPackages(repositoryRoot, publications, values.tag, values["dry-run"] === true, stop.requested);
} catch (error) {
  reportFailure(error);
} finally {
  stop.dispose();
}
