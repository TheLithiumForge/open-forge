import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { test } from "node:test";
import { fileURLToPath } from "node:url";
import { PipelineCommand, LeafCommand } from "../command-names.ts";
import { ProcessFixture } from "../../__tests__/process-fixture.ts";

const Cli = fileURLToPath(new URL("../../cli.ts", import.meta.url));
const Fixture = fileURLToPath(new URL("../../__tests__/fixtures/pipeline-output.ts", import.meta.url));

for (const [command, fail, next] of [
  [PipelineCommand.delivery, LeafCommand.deliveryLint, LeafCommand.deliveryFormat],
  [PipelineCommand.verify, LeafCommand.packageLayout, LeafCommand.dotnet],
  [PipelineCommand.checks, LeafCommand.setup, PipelineCommand.verify],
] as const) {
  test(`${command} stops at the failing ordered stage and names it`, () => {
    const result = spawnSync(process.execPath, [Cli, command], {
      encoding: "utf8",
      env: {
        ...process.env,
        npm_execpath: Fixture,
        [ProcessFixture.failCommandVariable]: fail,
      },
    });
    assert.equal(result.status, ProcessFixture.failure, result.stderr);
    assert.ok(result.stdout.includes(`executed:${fail}`));
    assert.ok(!result.stdout.includes(`executed:${next}`));
    assert.ok(result.stderr.includes(`[FAIL] ${fail}`));
  });
}
