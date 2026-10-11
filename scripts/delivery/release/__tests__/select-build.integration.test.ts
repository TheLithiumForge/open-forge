import assert from "node:assert/strict";
import { test } from "node:test";
import { selectBuild } from "../select-build.ts";
import { BuildId, BuildRepository, BuildSha, PackageArtifacts, recordedBuild } from "../../__tests__/build-fixtures.ts";
import { GithubApi } from "../../ci/github-api.ts";
import { BuildWorkflow } from "../../ci/build-runs.ts";

const Connection = { repository: BuildRepository, api: GithubApi.root, token: "recorded-workflow-token" };
const ListPath = `actions/workflows/${BuildWorkflow.file}/runs`;
const RunPath = `actions/runs/${BuildId.older}`;
const ArtifactPath = `${RunPath}/artifacts`;

// Recorded responses exercise the HTTP composition without any GitHub request.
for (const supplied of [false, true]) {
  test(`release Build selection validates artifacts for ${supplied ? "supplied" : "automatic"} runs`, async (context) => {
    const paths: string[] = [];
    context.mock.method(globalThis, "fetch", async (input: string) => {
      const url = new URL(input);
      const path = url.pathname.replace(`/repos/${BuildRepository}/`, "");
      paths.push(path);
      if (path === ListPath) {
        assert.equal(url.searchParams.get("head_sha"), BuildSha);
        return Response.json({ workflow_runs: [recordedBuild()] });
      }
      if (path === RunPath) return Response.json(recordedBuild());
      assert.equal(path, ArtifactPath);
      return Response.json({ artifacts: PackageArtifacts });
    });
    assert.equal(await selectBuild(Connection, BuildSha, supplied ? String(BuildId.older) : ""), String(BuildId.older));
    assert.deepEqual(paths, [supplied ? RunPath : ListPath, ArtifactPath]);
  });
}

test("a supplied Build with a missing artifact fails instead of triggering a fresh Build", async (context) => {
  context.mock.method(globalThis, "fetch", async (input: string) => Response.json(input.includes(ArtifactPath) ? { artifacts: PackageArtifacts.slice(1) } : recordedBuild()));
  await assert.rejects(selectBuild(Connection, BuildSha, String(BuildId.older)), /Missing or expired/u);
});

test("automatic selection leaves build_run_id empty when artifacts expired", async (context) => {
  context.mock.method(globalThis, "fetch", async (input: string) =>
    Response.json(input.includes(ArtifactPath) ? { artifacts: PackageArtifacts.map((artifact) => ({ ...artifact, expired: true })) } : { workflow_runs: [recordedBuild()] }),
  );
  assert.equal(await selectBuild(Connection, BuildSha, ""), "");
});
