import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { test } from "node:test";
import { checksPlan, jobPlan, docsPlan } from "../step-plan.ts";
import { CiCommand, NpmCommand } from "../command-names.ts";

const BuildWorkflow = new URL("../../../../.github/workflows/build.yml", import.meta.url);
const DocsWorkflow = new URL("../../../../.github/workflows/docs.yml", import.meta.url);
const PreCheckoutCommand = "git config --global core.longpaths true";
const MatrixRid = "${{ matrix.rid }}";
const Rid = "win-x64";

function runLines(text: string): string[] {
  return [...text.matchAll(/^(?: {6}- | {8})run: (.+)$/gmu)].map((match) => match[1] ?? assert.fail());
}

test("Build run steps match shared and matrix plans using single npm commands", () => {
  const text = readFileSync(BuildWorkflow, "utf8");
  const buildStart = text.indexOf("\n  build:");
  const checksStart = text.indexOf("\n  checks:");
  assert.ok(buildStart > 0);
  assert.deepEqual(runLines(text.slice(0, checksStart)), [`npm ${NpmCommand.run} ${CiCommand.reuse}`]);
  const shared = runLines(text.slice(checksStart, buildStart));
  const matrix = runLines(text.slice(buildStart)).map((line) => line.replaceAll(MatrixRid, Rid));
  assert.deepEqual(
    shared,
    checksPlan().map((step) => `npm ${step.args.join(" ")}`),
  );
  assert.deepEqual(matrix, [PreCheckoutCommand, `npm ${NpmCommand.run} ${CiCommand.prepare}`, ...jobPlan(Rid).map((step) => `npm ${step.args.join(" ")}`)]);
  assert.ok(!text.includes("shell: bash"));
  assert.ok(!text.includes("run: |"));
});

test("documentation plan mirrors the unchanged documentation workflow commands", () => {
  assert.deepEqual(
    runLines(readFileSync(DocsWorkflow, "utf8")),
    docsPlan().map((step) => `npm ${step.args.join(" ")}`),
  );
});
