import assert from "node:assert/strict";
import { test } from "node:test";
import { validateReleaseTag } from "../release-tag.ts";

const tag = "v0.1.0-beta.1";
const sha = "a".repeat(40);
const otherSha = "b".repeat(40);
const annotationSha = "c".repeat(40);
const refPath = `git/ref/tags/${tag}`;
const annotationPath = `git/tags/${annotationSha}`;
const tagResponse = (type: string, objectSha: string) => Response.json({ object: { type, sha: objectSha } });

// The service boundary is supplied as responses because this gate must be tested without contacting GitHub.
test("a missing release tag permits creation at the selected commit", async () => {
  await validateReleaseTag(
    async (path) => {
      assert.equal(path, refPath);
      return new Response(null, { status: 404 });
    },
    tag,
    sha,
  );
});

test("an existing lightweight release tag must match the selected commit", async () => {
  await validateReleaseTag(async () => tagResponse("commit", sha), tag, sha);
  await assert.rejects(
    validateReleaseTag(async () => tagResponse("commit", otherSha), tag, sha),
    /selected commit/,
  );
});

test("an annotated release tag is dereferenced before comparing the selected commit", async () => {
  const reads: string[] = [];
  const read = async (path: string) => {
    reads.push(path);
    return path === refPath ? tagResponse("tag", annotationSha) : tagResponse("commit", sha);
  };
  await validateReleaseTag(read, tag, sha);
  assert.deepEqual(reads, [refPath, annotationPath]);
  await assert.rejects(
    validateReleaseTag(async (path) => (path === refPath ? tagResponse("tag", annotationSha) : tagResponse("commit", otherSha)), tag, sha),
    /selected commit/,
  );
});

test("request failures cannot be mistaken for an absent release tag", async () => {
  await assert.rejects(
    validateReleaseTag(async () => new Response(null, { status: 403 }), tag, sha),
    /403/,
  );
  await assert.rejects(
    validateReleaseTag(async (path) => (path === refPath ? tagResponse("tag", annotationSha) : new Response(null, { status: 404 })), tag, sha),
    /404/,
  );
});
