import { existsSync } from "node:fs";
import { copyFile, mkdir } from "node:fs/promises";
import { join } from "node:path";
import { NpmCommand, spawnInRun } from "../execution/containers/executor.ts";
import { waitForProcess } from "../execution/process.ts";
import { createRunSpace, removeFolder } from "../execution/workspace.ts";
import { buildProject } from "./build.ts";
import { Isolation } from "../config/choices.ts";
import { ResultFiles } from "../paths.ts";
import { assessmentBase, checksAssessment, failedChecksAssessment } from "../history/assessments.ts";
import { gradingVersions } from "../history/capture.ts";
import type { AssessmentVersions, ChecksAssessment, RunView } from "../history/records.types.ts";
import { appendAssessment, assessmentEvidenceFolder, readBatch, readRuns, runFolder } from "../history/store.ts";
import { loadSuite } from "../benchmarks/suite.ts";
import { suiteChecks } from "../benchmarks/registry.ts";
import type { SuiteCheckOutput } from "../benchmarks/suite-checks.types.ts";
import type { BuildResult } from "../history/results.types.ts";

const RebuildFlag = "--rebuild";
const InstallMs = 10 * 60_000;
const BuildMs = 15 * 60_000;

export interface RecheckDependencies {
  readonly evaluate: (run: RunView, assessmentId: string) => Promise<SuiteCheckOutput & { readonly build?: BuildResult }>;
  readonly append: (assessment: ChecksAssessment) => Promise<void>;
}

export async function recheckRun(run: RunView, versions: AssessmentVersions, dependencies: RecheckDependencies): Promise<ChecksAssessment> {
  const base = assessmentBase(run.batchId, versions, run.checksAssessmentId);
  let assessment: ChecksAssessment;
  try {
    const result = await dependencies.evaluate(run, base.id);
    const selection = run.checkSelection.working.length === 0 ? undefined : run.checkSelection;
    assessment = { ...checksAssessment({ ...run, build: result.build ?? run.build, performance: result.performance, shots: result.shots }, result.checks, versions, selection), ...base };
  } catch (error) {
    assessment = { ...base, ...failedChecksAssessment(run, error, versions), id: base.id };
  }
  await dependencies.append(assessment);
  return assessment;
}

async function main(): Promise<void> {
  const args = process.argv.slice(2);
  const [batchId, onlyRun] = args.filter((arg) => arg !== RebuildFlag);
  if (batchId === undefined) throw new Error("Name the batch to check again.");
  const batch = await readBatch(batchId);
  const suite = await loadSuite(batch.suite);
  const checks = suiteChecks[suite.id];
  if (checks === undefined) throw new Error(`No hidden checks for ${suite.id}.`);
  const versions = await gradingVersions(suite);
  for (const run of await readRuns(batch)) {
    if (run.build === null || (onlyRun !== undefined && run.id !== onlyRun)) continue;
    const savedBuild = run.build;
    await recheckRun(run, versions, { append: appendAssessment, evaluate: async (_, assessmentId) => {
      const folder = runFolder(batchId, run.id);
      const evidence = assessmentEvidenceFolder(batch.id, assessmentId);
      const shotsFolder = join(evidence, ResultFiles.shots);
      await mkdir(shotsFolder, { recursive: true });
      let space: Awaited<ReturnType<typeof createRunSpace>> | null = null;
      try {
        let build = savedBuild;
        let project = join(folder, ResultFiles.files);
        let game = join(folder, ResultFiles.game);
        if (args.includes(RebuildFlag) || !build.passed) {
          space = await createRunSpace(suite.project, project);
          await waitForProcess(spawnInRun({ isolation: Isolation.local, space, command: NpmCommand, args: ["ci", "--no-audit", "--no-fund"], network: true }), { timeoutMs: InstallMs });
          build = await buildProject(Isolation.local, space, BuildMs);
          project = space.work;
          game = build.html === null ? "" : join(project, build.html);
          if (existsSync(game)) await copyFile(game, join(evidence, ResultFiles.game));
        }
        const result = await checks({ project, build, html: existsSync(game) && build.passed ? game : null, shotsFolder });
        return { ...result, shots: result.shots.map((shot) => `${assessmentId}/${shot}`), build };
      } finally {
        if (space !== null) await removeFolder(space.root);
      }
    } });
  }
}

if (import.meta.main) await main();
