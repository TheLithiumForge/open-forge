import assert from "node:assert/strict";
import { test } from "node:test";
import { cancellationReport, publishUploads } from "../publish-uploads.ts";
import type { Publication } from "../publication-package.ts";
import { MainPackageName, PlatformPackages } from "../../package-model.ts";
import { AllTargets } from "../../targets.ts";

const Publications: Publication[] = [...AllTargets.map((rid) => PlatformPackages[rid].packageName), MainPackageName].map((name) => ({
  name,
  version: "1.0.0",
  tarball: `${name}.tgz`,
  sha256: "fixture",
  dirty: false,
}));
const NoExisting = Publications.map(() => false);
const StopBoundary = { first: 0, middle: 3, last: Publications.length - 1 } as const;

for (const [boundary, count] of Object.entries(StopBoundary)) {
  test(`publication starts no upload after a stop before the ${boundary} upload`, async () => {
    const uploaded: string[] = [];
    const promoted: string[] = [];
    const progress = await publishUploads(
      Publications,
      NoExisting,
      () => uploaded.length === count,
      async (publication) => {
        uploaded.push(publication.name);
      },
      async (publication) => {
        promoted.push(publication.name);
      },
    );
    assert.deepEqual(
      uploaded,
      Publications.slice(0, count).map((publication) => publication.name),
    );
    assert.deepEqual(progress.published, uploaded);
    assert.deepEqual(
      progress.notStarted,
      Publications.slice(count).map((publication) => publication.name),
    );
    assert.deepEqual(progress.interrupted, []);
    assert.equal(progress.stopped, true);
    assert.ok(promoted.length <= uploaded.length);
  });
}

test("a signalled upload that fails has unknown final state and stops later packages", async () => {
  let stopped = false;
  const first = Publications[0];
  assert.ok(first);
  const progress = await publishUploads(
    Publications,
    NoExisting,
    () => stopped,
    async () => {
      stopped = true;
      throw new Error("interrupted upload fixture");
    },
    async () => assert.fail("No tag change after a signal."),
  );
  assert.deepEqual(progress.interrupted, [first.name]);
  assert.deepEqual(progress.published, []);
  assert.deepEqual(
    progress.notStarted,
    Publications.slice(1).map((publication) => publication.name),
  );
  assert.equal(
    cancellationReport(progress),
    `Publication stopped after a cancellation request.
Published: none
Interrupted (final state unknown): ${first.name}
Not started: ${progress.notStarted.join(", ")}
Already existed: none`,
  );
});

test("a signalled successful last upload is known published but cancellation still fails", async () => {
  let stopped = false;
  const progress = await publishUploads(
    Publications,
    NoExisting,
    () => stopped,
    async (publication) => {
      if (publication.name === MainPackageName) stopped = true;
    },
    async () => {},
  );
  assert.deepEqual(
    progress.published,
    Publications.map((publication) => publication.name),
  );
  assert.deepEqual(progress.notStarted, []);
  assert.deepEqual(progress.interrupted, []);
  assert.equal(progress.stopped, true);
});

test("existing packages skip without tag changes and an ordinary upload error propagates", async () => {
  const progress = await publishUploads(
    Publications,
    Publications.map(() => true),
    () => false,
    async () => assert.fail(),
    async () => assert.fail(),
  );
  assert.deepEqual(
    progress.existing,
    Publications.map((publication) => publication.name),
  );
  assert.equal(progress.stopped, false);
  await assert.rejects(
    publishUploads(
      Publications,
      NoExisting,
      () => false,
      async () => {
        throw new Error("upload failed");
      },
      async () => {},
    ),
    /upload failed/u,
  );
});

test("cancellation during a tag update keeps the successful upload known and starts no later package", async () => {
  let stopped = false;
  const first = Publications[0];
  assert.ok(first);
  const progress = await publishUploads(
    Publications,
    NoExisting,
    () => stopped,
    async () => {},
    async () => {
      stopped = true;
      throw new Error("interrupted tag update fixture");
    },
  );
  assert.deepEqual(progress.published, [first.name]);
  assert.deepEqual(progress.interrupted, []);
  assert.equal(progress.stopped, true);
  assert.deepEqual(
    progress.notStarted,
    Publications.slice(1).map((publication) => publication.name),
  );
});
