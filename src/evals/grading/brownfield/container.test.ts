import assert from "node:assert/strict";
import { writeFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { brownfieldFixture } from "../../materialize/brownfield-test-fixture.ts";
import { brownfieldInputs } from "../../materialize/brownfield-inputs.ts";
import { writeNew } from "../../materialize/files.ts";
import { ContainerPaths, ContainerUsers } from "../../execution/containers/settings.ts";
import type { Docker } from "../../execution/containers/docker.ts";
import { CheckStatus } from "../../history/statuses.ts";
import { ChangeCheckFiles, ChangeCheckVersion, parseChangeReport, readChangeChecks } from "./change-checks.ts";
import { BrownfieldContainer, runChangeChecks } from "./container.ts";

const Success = { code: 0, timedOut: false, stdout: "", stderr: "" };
const Build = { passed: true, typecheck: true, seconds: 0, packages: null, html: "dist/index.html", bytes: null, log: "" };

test("change workers run offline as the checker and only validated plain reports come back", async (t) => {
  const fixture = await brownfieldFixture(t), inputs = await brownfieldInputs(fixture.caseRoot, fixture.reference, fixture.destination);
  const project = join(fixture.root, "candidate");
  for (const file of inputs.seed) await writeNew(project, file.path, file.bytes);
  const html = join(project, "dist/index.html"); await writeNew(project, "dist/index.html", "<html>Fixture</html>");
  const declared = await readChangeChecks(inputs.case), calls: (readonly string[])[] = [];
  const report = { schemaVersion: ChangeCheckVersion, checks: declared.map((check) => ({ id: check.id, status: CheckStatus.pass, evidence: "Fixture checker evidence." })) };
  const docker: Docker = { spawn: () => { throw new Error("No host candidate process."); }, run: async (args) => {
    calls.push(args);
    if (args[0] === "cp" && args[1]?.endsWith(`:${BrownfieldContainer.evidence}/.`)) {
      const target = args[2]; assert.ok(target);
      await writeFile(join(target, ChangeCheckFiles.report), JSON.stringify(report));
    }
    return Success;
  } };
  const output = await runChangeChecks(inputs.case, declared, { project, html, build: Build, shotsFolder: join(fixture.root, "shots"), stage: { docker, timeoutMs: 10_000 } });
  assert.equal(output.length, declared.length);
  assert.ok(output.every((check) => check.status === CheckStatus.pass));
  const worker = calls.findIndex((args) => args.some((arg) => arg.endsWith(`/${ChangeCheckFiles.worker}`)));
  assert.ok(worker > calls.findIndex((args) => args[0] === "network" && args[1] === "disconnect"));
  assert.ok(calls[worker]?.includes(ContainerUsers.agent));
  assert.ok(calls.some((args) => args[0] === "cp" && args[2]?.endsWith(`:${ContainerPaths.work}`)));
  assert.equal(calls.some((args) => args.includes("--mount") || args.includes("--volume") || args.includes("npm")), false);
  assert.throws(() => parseChangeReport({ ...report, checks: report.checks.slice(1) }, declared));
  assert.throws(() => parseChangeReport({ ...report, checks: [...report.checks, report.checks[0]] }, declared));
  assert.throws(() => parseChangeReport({ ...report, checks: [{ id: "invented", status: CheckStatus.pass, evidence: "Invented." }] }, declared));
});
