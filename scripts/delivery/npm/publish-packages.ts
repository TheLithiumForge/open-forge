import assert from "node:assert/strict";
import type { Publication } from "./publication-package.ts";
import { npm } from "../process.ts";
import { registryVersionExists } from "./registry-version.ts";

export function publishArguments(publication: Publication, tag: string): string[] {
  assert.ok(/^[a-z][a-z0-9-]*$/u.test(tag), "Supply an npm tag such as preview, beta or latest.");
  assert.ok(tag !== "latest" || !publication.version.split("+")[0]?.includes("-"), "Prereleases require a prerelease tag.");
  return ["publish", publication.tarball, "--access", "public", "--tag", tag, "--ignore-scripts"];
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
      publications.map((publication) => ({ ...publication, tag, dryRun })),
      null,
      2,
    )}\n`,
  );
  if (dryRun) return;
  // Check the complete selection before starting uploads; a failed lookup must not mean "missing".
  const existing = publications.map((publication) => registryVersionExists(root, publication.name, publication.version));
  for (const [index, publication] of publications.entries()) {
    if (existing[index]) {
      process.stderr.write(`Warning: ${publication.name}@${publication.version} already exists; skipping publication and leaving its npm tags unchanged.\n`);
    } else {
      const args = commands[index];
      assert.ok(args);
      npm(args, root);
    }
  }
}
