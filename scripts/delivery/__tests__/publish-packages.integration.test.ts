import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { mkdtempSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { test } from "node:test";
import { MainPackageName, PlatformPackages } from "../package-model.ts";

const publisher = new URL("../npm/publish-packages.ts", import.meta.url).href;
const names = [...Object.values(PlatformPackages).map((platform) => platform.packageName), MainPackageName];
const version = "1.2.3";
const dependencies = { [PlatformPackages["linux-x64"].packageName]: version };
const existingNames = [names[0], names[3], MainPackageName];

for (const scenario of ["fresh", "partial", "all-existing", "lookup-failure", "graph-failure", "upload-failure", "dry-run"] as const) {
  test(`publication orchestration: ${scenario}`, (context) => {
    const root = mkdtempSync(join(tmpdir(), "open-forge-publication-retry-"));
    context.after(() => rmSync(root, { recursive: true, force: true }));
    const log = join(root, "calls.jsonl");
    writeFileSync(log, "");
    const fakeNpm = join(root, "npm.mjs");
    // Model the npm process boundary. No registry, credentials or real uploads are involved.
    writeFileSync(
      fakeNpm,
      `
      import { appendFileSync } from "node:fs";
      const [command, spec, field, extraField] = process.argv.slice(2);
      appendFileSync(${JSON.stringify(log)}, JSON.stringify({ command, spec }) + "\\n");
      const scenario = ${JSON.stringify(scenario)};
      if (command === "view") {
        if (scenario === "lookup-failure" && spec.startsWith(${JSON.stringify(MainPackageName + "@")})) {
          console.log(JSON.stringify({ error: { code: "E401" } })); process.exit(1);
        }
        if (scenario === "all-existing" || (scenario !== "fresh" && ${JSON.stringify(existingNames.map((name) => `${name}@${version}`))}.includes(spec))) {
          console.log(JSON.stringify(extraField === "optionalDependencies" ? { version: ${JSON.stringify(version)}, optionalDependencies: scenario === "graph-failure" ? {} : ${JSON.stringify(dependencies)} } : ${JSON.stringify(version)}));
        } else {
          console.log(JSON.stringify({ error: { code: "E404" } })); process.exit(1);
        }
      } else if (command === "publish" && scenario === "upload-failure") process.exit(1);
      else if (command !== "publish") process.exit(2);
    `,
    );
    const publications = names.map((name) => ({
      name,
      version,
      tarball: `${name}.tgz`,
      sha256: "fixture",
      dirty: false,
      ...(name === MainPackageName ? { optionalDependencies: dependencies } : {}),
    }));
    const completion = spawnSync(
      process.execPath,
      [
        "--input-type=module",
        "--eval",
        `
      import { publishPackages } from ${JSON.stringify(publisher)};
      publishPackages(${JSON.stringify(root)}, ${JSON.stringify(publications)}, "latest", ${scenario === "dry-run"});
    `,
      ],
      { cwd: root, encoding: "utf8", env: { ...process.env, npm_execpath: fakeNpm } },
    );
    assert.equal(completion.error, undefined);
    const lines = readFileSync(log, "utf8").trim().split("\n").filter(Boolean);
    const expectedLookups = names.map((name) => JSON.stringify({ command: "view", spec: `${name}@${version}` }));
    const expectedUploads = names.filter((name) => !existingNames.includes(name)).map((name) => JSON.stringify({ command: "publish", spec: `${name}.tgz` }));
    assert.equal(completion.status, scenario.endsWith("failure") ? 1 : 0, completion.stderr);
    if (scenario === "dry-run") assert.deepEqual(lines, []);
    else if (scenario === "fresh") assert.deepEqual(lines, [...expectedLookups, ...names.map((name) => JSON.stringify({ command: "publish", spec: `${name}.tgz` }))]);
    else if (scenario === "partial") {
      assert.deepEqual(lines, [...expectedLookups, ...expectedUploads]);
      for (const name of existingNames) assert.ok(completion.stderr.includes(`Warning: ${name}@${version} already exists; skipping`));
    } else if (scenario === "upload-failure") assert.deepEqual(lines, [...expectedLookups, expectedUploads[0]]);
    else assert.deepEqual(lines, expectedLookups);
    if (scenario === "graph-failure") assert.match(completion.stderr, /Published wrapper targets differ/);
    if (scenario === "lookup-failure") assert.match(completion.stderr, /E401/);
    if (scenario === "all-existing") for (const name of names) assert.ok(completion.stderr.includes(`Warning: ${name}@${version} already exists; skipping`));
  });
}
