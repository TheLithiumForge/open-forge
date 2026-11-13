import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { mkdtempSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { fileURLToPath } from "node:url";
import { test } from "node:test";
import { MainPackageName, PlatformPackages } from "../../package-model.ts";
import { AllTargets } from "../../targets.ts";
import { readPreviousGraph } from "../read-previous-graph.ts";
import { DeliveryOption, optionArgument } from "../../option-names.ts";
import { SmokeArguments } from "../../ci/smoke-checks.ts";
import { ReleaseFile } from "../release-plan.ts";

const Previous = "0.9.0-beta.18";
const Bad = "0.9.0-beta.19";
const Entrypoint = fileURLToPath(new URL("../restore-channels.ts", import.meta.url));
const Fixture = { registry: "registry-forbidden.ts" } as const;

test("offline recovery reads a pre-alias graph and never contacts the registry", (context) => {
  const root = mkdtempSync(join(tmpdir(), "open-forge-restore-"));
  context.after(() => rmSync(root, { recursive: true, force: true }));
  const names = [...AllTargets.map((target) => PlatformPackages[target].packageName), MainPackageName];
  writeFileSync(
    join(root, ReleaseFile.manifest),
    JSON.stringify({ version: Previous, targets: AllTargets, artifacts: names.map((name) => ({ path: `${name.replace(/^@/u, "").replaceAll("/", "-")}-${Previous}.tgz` })) }),
  );
  const forbiddenRegistry = join(root, Fixture.registry);
  // Any accidental npm invocation fails locally instead of reaching a user's registry.
  writeFileSync(forbiddenRegistry, "process.exit(1);\n");
  assert.deepEqual(readPreviousGraph(root, Previous, root, false).packages, names);
  const result = spawnSync(process.execPath, [Entrypoint, ...SmokeArguments.version, Bad, optionArgument(DeliveryOption.to), Previous, optionArgument(DeliveryOption.from), root], {
    cwd: root,
    env: { ...process.env, npm_execpath: forbiddenRegistry },
    encoding: "utf8",
    windowsHide: true,
  });
  assert.equal(result.error, undefined);
  assert.equal(result.status, 0, result.stderr);
  assert.equal(result.stderr.length, 0);
  assert.throws(() => readPreviousGraph(root, Previous, undefined, false));
  assert.throws(() => readPreviousGraph(root, Previous, root, true));
});
