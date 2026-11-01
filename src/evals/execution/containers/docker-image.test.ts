import assert from "node:assert/strict";
import { test } from "node:test";
import { join } from "node:path";
import { EvalPaths } from "../../paths.ts";
import { BaseImages, lockedImageArguments } from "../../images/lock.ts";
import { buildEvalImages, evalImageTag, ImageVersions } from "./docker-image.ts";
import { StageKind } from "./settings.ts";
import { containerFixture } from "./test-docker.ts";

const Digest = `sha256:${"a".repeat(64)}`;
const Lock = { node: { tag: BaseImages.node, digest: Digest }, playwright: { tag: BaseImages.playwright, digest: Digest } };

test("image build refuses every incomplete or mismatched lock before invoking Docker", async () => {
  const fixture = await containerFixture();
  try {
    for (const lock of [null, {}, { ...Lock, node: { tag: BaseImages.node, digest: null } }, { ...Lock, playwright: { tag: BaseImages.playwright, digest: "latest" } }]) {
      await assert.rejects(buildEvalImages(fixture.client, false, lock));
      assert.equal(fixture.client.calls.length, 0);
    }
  } finally { await fixture.dispose(); }
});

test("image builds pin both base digests and all harness tool versions", async () => {
  const fixture = await containerFixture();
  try {
    await buildEvalImages(fixture.client, false, Lock);
    const args = [...lockedImageArguments(Lock), `CLAUDE_VERSION=${ImageVersions.claude}`, `CODEX_VERSION=${ImageVersions.codex}`, `OPEN_FORGE_VERSION=${ImageVersions.openForge}`];
    assert.deepEqual(fixture.client.calls.map((call) => call.args), Object.values(StageKind).map((kind) => ["build", "--tag", evalImageTag(kind), "--file", join(EvalPaths.docker, `${kind}.Dockerfile`),
      ...args.flatMap((value) => ["--build-arg", value]), EvalPaths.docker]));
  } finally { await fixture.dispose(); }
});
