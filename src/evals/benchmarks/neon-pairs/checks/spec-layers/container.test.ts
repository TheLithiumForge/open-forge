import assert from "node:assert/strict";
import { mkdir, mkdtemp, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { dirname, join, resolve } from "node:path";
import { test } from "node:test";
import type { Docker } from "../../../../execution/containers/docker.ts";
import { ContainerPaths, ContainerUsers } from "../../../../execution/containers/settings.ts";
import { loadLayer } from "../layer.ts";
import { loadDefinition } from "./load.ts";
import { LayerId, Method } from "./requirements.ts";
import { SpecLayerContainer } from "./paths.ts";
import { runLayerChecks } from "./container.ts";
import { CommandId } from "./commands.ts";

const Success = { code: 0, timedOut: false, stdout: "", stderr: "" };
const Build = { passed: true, typecheck: true, seconds: 0, packages: null, html: "dist/index.html", bytes: null, log: "" };
test("later grading isolates candidate commands and validates only plain copied-out artifacts", async (t) => {
  const root = await mkdtemp(join(tmpdir(), "eval-spec-layers-fake-"));
  t.after(async () => { assert.equal(dirname(resolve(root)), resolve(tmpdir())); await rm(root, { recursive: true, force: true }); });
  const project = join(root, "project"); await mkdir(project);
  await writeFile(join(project, "package.json"), "{}");
  const html = join(project, "index.html"); await writeFile(html, "<html></html>");
  const definition = await loadDefinition(LayerId.two), inherited = await loadLayer(), calls: (readonly string[])[] = [];
  const docker: Docker = {
    run: async (args) => {
      calls.push(args);
      if (args[0] === "cp" && args[1]?.endsWith(`:${SpecLayerContainer.evidence}/.`)) {
        const target = args[2]; assert.ok(target);
        await writeFile(join(target, SpecLayerContainer.report), JSON.stringify({ schemaVersion: 1, layer: LayerId.two,
          assertions: definition.checks.filter((check) => check.method !== Method.build).flatMap((check) => check.assertions.map((assertion) => ({ id: assertion.id, clause: assertion.clause, status: "pass", evidence: "Fake browser and source evidence" }))),
          inherited: { checks: inherited.checks.map((check) => ({ ...check, status: "pass", evidence: "Inherited fake evidence" })), shots: [], performance: null },
        }));
      }
      // Candidate results come only from docker exec output, never from files in the container.
      if (args.includes("timeout") && args.includes("npm")) return { ...Success, stdout: "Tests 1 passed\ntypecheck" };
      if (args.includes(SpecLayerContainer.pageReader)) return { ...Success, stdout: "<html><script>inline</script></html>" };
      return Success;
    },
    spawn: () => { throw new Error("No host candidate process is allowed."); },
  };
  const output = await runLayerChecks(LayerId.two, { project, html, build: Build, shotsFolder: join(root, "shots") }, { docker, timeoutMs: 30_000 });
  assert.equal(output.assertions.length, definition.checks.flatMap((check) => check.assertions).length);
  assert.equal(output.assertions.find((item) => item.clause === "T07.9"), undefined);
  assert.ok(output.review.some((item) => item.clause === "T07.9"));
  assert.ok(output.checks.filter((check) => check.id !== "l2-build").every((check) => check.status === "pass"));
  const commandCalls = calls.filter((args) => args.includes("timeout") && args.includes("npm"));
  assert.equal(commandCalls.length, [CommandId.ci, CommandId.test, CommandId.build].length);
  assert.ok(commandCalls.every((args) => args[args.indexOf("--user") + 1] === ContainerUsers.build));
  const cleanups = calls.filter((args) => args.includes(ContainerPaths.stopTools));
  assert.ok(cleanups.length >= commandCalls.length, "Every candidate command is followed by stopping the build account's processes.");
  assert.ok(calls.every((args) => !args.some((arg) => arg.includes(".eval-command-results"))), "No result passes through a folder the candidate can write.");
  const browser = calls.find((args) => args.includes(`${SpecLayerContainer.harness}/${SpecLayerContainer.worker}`));
  assert.equal(browser?.[browser.indexOf("--user") + 1], ContainerUsers.agent);
  const evidenceFolder = calls.find((args) => args.includes("700") && args.includes(SpecLayerContainer.evidence));
  assert.equal(evidenceFolder?.[evidenceFolder.indexOf("--user") + 1], ContainerUsers.agent, "The checker account makes its own evidence folder, since root has no capabilities.");
  assert.ok(calls.every((args) => !args.includes("chown")));
  assert.ok(calls.some((args) => args[0] === "network" && args[1] === "disconnect"));
  assert.ok(calls.some((args) => args[0] === "kill"));
  assert.ok(calls.some((args) => args[0] === "rm"));
});
