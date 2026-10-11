import assert from "node:assert/strict";
import { readPackage } from "../package-json.ts";
import { validateOutput } from "../output.ts";
import { readReleasePublications } from "./release-publications.ts";
import { ReleaseFile, type ReleasePlan, type ReleaseTarget } from "./release-plan.ts";

export function readReleasePlan(root: string, directory: string, source: { sha: string; dirty: boolean }, version: string, target: ReleaseTarget, channel: string): ReleasePlan {
  const publications = readReleasePublications(root, directory, source, version);
  const release = readPackage(validateOutput(root, `${directory}/release.json`));
  const artifacts = release["artifacts"];
  assert.ok(Array.isArray(artifacts));
  const files = artifacts.flatMap((value: unknown) => {
    assert.ok(typeof value === "object" && value !== null && "path" in value && typeof value.path === "string");
    return value.path.endsWith(ReleaseFile.portableSuffix) ? [`${directory}/${value.path}`] : [];
  });
  return { target, channel, version, sha: source.sha, packages: publications.map((publication) => publication.name), files: [...files, `${directory}/${ReleaseFile.checksums}`] };
}
