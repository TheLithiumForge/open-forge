import assert from "node:assert/strict";
import type { Publication } from "./publication-package.ts";
import { npm } from "../process.ts";
import { isPrerelease, registryHasStableVersion, registryVersionExists } from "./registry-version.ts";

export function publishArguments(publication: Publication, tag: string): string[] {
  assert.ok(/^[a-z][a-z0-9-]*$/u.test(tag), "Supply an npm tag such as preview, beta or latest.");
  assert.ok(tag !== "latest" || !isPrerelease(publication.version), "Prereleases require a prerelease tag.");
  return ["publish", publication.tarball, "--access", "public", "--tag", tag, "--ignore-scripts"];
}

// A new prerelease also becomes `latest` while the package has no stable version.
// The registry is asked only when the answer can matter.
export function promotesLatest(version: string, tag: string, hasStableVersion: () => boolean): boolean {
  return tag !== "latest" && isPrerelease(version) && !hasStableVersion();
}

export function publishPackages(root: string, publications: readonly Publication[], tag: string, dryRun: boolean): void {
  assert.ok(publications.length > 0, "No packages selected for publication.");
  const commands = publications.map((publication) => publishArguments(publication, tag));
  if (!dryRun)
    assert.ok(
      publications.every((publication) => !publication.dirty),
      "Commit the source and rebuild before publication.",
    );
  process.stdout.write(
    `${JSON.stringify(
      publications.map((publication) => ({ ...publication, tag, latestUntilStable: tag !== "latest" && isPrerelease(publication.version), dryRun })),
      null,
      2,
    )}\n`,
  );
  if (dryRun) return;
  // Check the complete selection before starting uploads; a failed lookup must not mean "missing".
  const existing = publications.map((publication) => registryVersionExists(root, publication.name, publication.version, publication.optionalDependencies));
  const promote = publications.map((publication) => promotesLatest(publication.version, tag, () => registryHasStableVersion(root, publication.name)));
  for (const [index, publication] of publications.entries()) {
    if (existing[index]) {
      process.stderr.write(`Warning: ${publication.name}@${publication.version} already exists; skipping publication and leaving its npm tags unchanged.\n`);
    } else {
      const args = commands[index];
      assert.ok(args);
      npm(args, root);
      if (promote[index]) npm(["dist-tag", "add", `${publication.name}@${publication.version}`, "latest"], root);
    }
  }
}
