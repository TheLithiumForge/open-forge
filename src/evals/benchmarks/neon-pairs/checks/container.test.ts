import assert from "node:assert/strict";
import { mkdir, mkdtemp, readFile, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { dirname, join, resolve } from "node:path";
import { test } from "node:test";
import type { Docker } from "../../../execution/containers/docker.ts";
import { CheckStatus } from "../../../history/statuses.ts";
import { runContainerChecks } from "./container.ts";
import { CheckContainer } from "./container-paths.ts";
import { ContainerPaths } from "../../../execution/containers/settings.ts";
import { loadLayer } from "./layer.ts";
import { runNeonPairsChecks } from "./neon-pairs.ts";
import { DiagnosticEnabled, DiagnosticEnvironment, DiagnosticFolder, DiagnosticKind } from "./diagnostics.ts";
import { PNG } from "pngjs";

const success = { code: 0, timedOut: false, stdout: "", stderr: "" };
const build = { passed: true, typecheck: true, seconds: 0, packages: null, html: "index.html", bytes: null, log: "" };

test("browser grading crosses the container interface and validates its copied-out report", async (t) => {
  const root = await mkdtemp(join(tmpdir(), "eval-check-container-"));
  t.after(async () => { if (dirname(resolve(root)) !== resolve(tmpdir())) throw new Error("Test cleanup escaped its folder."); await rm(root, { recursive: true, force: true }); });
  const project = join(root, "project");
  await mkdir(project);
  await writeFile(join(project, "package.json"), "{}");
  const html = join(project, "index.html");
  await writeFile(html, "<html></html>");
  const layer = await loadLayer();
  const calls: (readonly string[])[] = [];
  const previousDiagnostics = process.env[DiagnosticEnvironment];
  t.after(() => { if (previousDiagnostics === undefined) delete process.env[DiagnosticEnvironment]; else process.env[DiagnosticEnvironment] = previousDiagnostics; });
  process.env[DiagnosticEnvironment] = DiagnosticEnabled;
  const diagnosticImage = `step-0000-${DiagnosticKind.backs}-card-0.png`;
  const client: Docker = {
    run: async (args) => {
      calls.push(args);
      if (args[0] === "cp" && args[1]?.endsWith(`:${CheckContainer.output}/.`)) {
        const target = args[2];
        assert.ok(target);
        await mkdir(target, { recursive: true });
        await writeFile(join(target, CheckContainer.report), JSON.stringify({ checks: layer.checks.map((check) => ({ ...check, status: CheckStatus.pass, evidence: "Fake container outcome" })), shots: [], performance: null }));
        const diagnostics = join(target, CheckContainer.shots, DiagnosticFolder);
        await mkdir(diagnostics, { recursive: true });
        await writeFile(join(diagnostics, diagnosticImage), PNG.sync.write(new PNG({ width: 2, height: 2 })));
        await writeFile(join(diagnostics, `step-0000-${DiagnosticKind.backs}.json`), JSON.stringify({ page: 0, kind: DiagnosticKind.backs, comparisons: [] }));
      }
      return success;
    },
    spawn: () => { throw new Error("No process spawn is needed for browser checks."); },
  };
  const report = await runContainerChecks({ project, html, build, shotsFolder: join(root, "shots") }, { docker: client, timeoutMs: 30_000 });
  assert.ok(report.checks.every((check) => check.status === CheckStatus.pass));
  assert.ok(calls.some((args) => args[0] === "network" && args[1] === "disconnect"));
  assert.ok(calls.some((args) => args.includes(`${CheckContainer.harness}/${CheckContainer.worker}`)));
  assert.ok(calls.some((args) => args.includes(`${DiagnosticEnvironment}=${DiagnosticEnabled}`)));
  assert.equal(PNG.sync.read(await readFile(join(root, "shots", DiagnosticFolder, diagnosticImage))).width, 2);
  assert.ok(calls.some((args) => args[0] === "rm"));
  const copiedHarness = calls.findIndex((args) => args[0] === "cp" && args[2]?.endsWith(`:${CheckContainer.harness}`));
  const preparedHarness = calls.findIndex((args) => args[0] === "exec" && args.includes(ContainerPaths.prepare) && args.includes(CheckContainer.harness));
  assert.ok(copiedHarness >= 0 && preparedHarness > copiedHarness);
});

test("a browser launch failure returns checker errors for every browser check", async (t) => {
  const root = await mkdtemp(join(tmpdir(), "eval-launch-failure-"));
  t.after(async () => { if (dirname(resolve(root)) !== resolve(tmpdir())) throw new Error("Test cleanup escaped its folder."); await rm(root, { recursive: true, force: true }); });
  await writeFile(join(root, "package.json"), JSON.stringify({ dependencies: { react: "pinned", typescript: "pinned" } }));
  const report = await runNeonPairsChecks({ project: root, html: join(root, "unused.html"), build, shotsFolder: join(root, "shots") }, { launch: async () => { throw new Error("Browser crash"); } });
  assert.equal(report.checks.filter((check) => check.status === CheckStatus.pass).length, 1);
  assert.ok(report.checks.filter((check) => check.status !== CheckStatus.pass).every((check) => check.status === CheckStatus.error));
});
