import assert from "node:assert/strict";
import { setImmediate } from "node:timers/promises";
import type { Publication } from "./publication-package.ts";
import { isPrerelease, registryHasStableVersion, registryVersionExists } from "./registry-version.ts";
import { cancellationReport, publishUploads } from "./publish-uploads.ts";
import { uploadCommand } from "./upload-command.ts";
import { distTagArguments, LatestTag } from "./dist-tag.ts";

export function publishArguments(publication: Publication, tag: string): string[] {
  assert.ok(/^[a-z][a-z0-9-]*$/u.test(tag), "Supply an npm tag such as preview, beta or latest.");
  assert.ok(tag !== LatestTag || !isPrerelease(publication.version), "Prereleases require a prerelease tag.");
  return ["publish", publication.tarball, "--access", "public", "--tag", tag, "--ignore-scripts"];
}

// A new prerelease also becomes `latest` while the package has no stable version.
// The registry is asked only when the answer can matter.
export function promotesLatest(version: string, tag: string, hasStableVersion: () => boolean): boolean {
  return tag !== LatestTag && isPrerelease(version) && !hasStableVersion();
}

export async function publishPackages(root: string, publications: readonly Publication[], tag: string, dryRun: boolean, stopRequested: () => boolean = () => false): Promise<void> {
  assert.ok(publications.length > 0, "No packages selected for publication.");
  const commands = publications.map((publication) => publishArguments(publication, tag));
  if (!dryRun)
    assert.ok(
      publications.every((publication) => !publication.dirty),
      "Commit the source and rebuild before publication.",
    );
  process.stdout.write(
    `${JSON.stringify(
      publications.map((publication) => ({ ...publication, tag, latestUntilStable: tag !== LatestTag && isPrerelease(publication.version), dryRun })),
      null,
      2,
    )}\n`,
  );
  if (dryRun) return;
  // Check the complete selection before starting uploads; a failed lookup must not mean "missing".
  let existing: boolean[] = [];
  let promote: boolean[] = [];
  try {
    await setImmediate();
    if (!stopRequested()) {
      existing = publications.map((publication) => registryVersionExists(root, publication.name, publication.version, publication.optionalDependencies));
      promote = publications.map((publication) => promotesLatest(publication.version, tag, () => registryHasStableVersion(root, publication.name)));
    }
  } catch (error) {
    await setImmediate();
    if (!stopRequested()) throw error;
  }
  const progress = await publishUploads(
    publications,
    existing,
    stopRequested,
    async (publication) => {
      const args = commands[publications.indexOf(publication)];
      assert.ok(args);
      await uploadCommand(root, args);
    },
    async (publication) => {
      if (promote[publications.indexOf(publication)] && !stopRequested()) await uploadCommand(root, distTagArguments(publication.name, publication.version, LatestTag));
    },
  );
  if (progress.stopped) {
    process.stderr.write(`${cancellationReport(progress)}\n`);
    throw new Error("Release publication cancelled.");
  }
}
