import { mkdir, mkdtemp, rename, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { dirname, join, resolve } from "node:path";
import type { Suite } from "../../benchmarks/suite.types.ts";
import type { SuiteCheckInput } from "../../benchmarks/suite-checks.types.ts";
import { Isolation } from "../../config/choices.ts";
import type { RunSpace } from "../../execution/containers/executor.ts";
import type { BuildResult } from "../../history/results.types.ts";
import { CheckStatus } from "../../history/statuses.ts";
import { sanitizeRecords } from "../../history/sanitize.ts";
import { BrownfieldFiles } from "../../materialize/brownfield-case.ts";
import { ordinaryPath, type FileHash } from "../../materialize/files.ts";
import { buildProject } from "../build.ts";
import { runBrownfieldChecks, type BrownfieldServices } from "./gates.ts";
import { projectHashes } from "./scope.ts";
import { compareValidation, readValidationExpectations, ValidationCheck, ValidationPhase, type ValidationComparison, type ValidationExpectation } from "./validation-expectations.ts";
import { validationInputs } from "./validation-inputs.ts";
import { validationProject } from "./validation-project.ts";

export const CaseValidationVersion = 1;
export const CaseValidationTimeoutMs = 90 * 60_000;
const TemporaryPrefix = "eval-case-validation-";
export interface CaseValidationStage { readonly phase: ValidationPhase; readonly passed: boolean; readonly outcomes: readonly ValidationComparison[]; readonly error: string | null }
export interface CaseValidation { readonly schemaVersion: typeof CaseValidationVersion; readonly case: string; readonly passed: boolean; readonly inputs: Awaited<ReturnType<typeof validationInputs>>; readonly stages: readonly CaseValidationStage[] }
export interface CaseValidationServices {
  readonly build?: (space: RunSpace) => Promise<BuildResult>;
  readonly gates?: BrownfieldServices;
}

function required(id: string, passed: boolean): ValidationComparison {
  return { id, expected: CheckStatus.pass, actual: passed ? CheckStatus.pass : CheckStatus.fail, matches: passed, evidence: null };
}

async function validateStage(suite: Suite, root: string, phase: ValidationPhase, baseline: readonly FileHash[], expected: readonly ValidationExpectation[], tools: CaseValidationServices): Promise<CaseValidationStage> {
  const inputs = suite.brownfield;
  if (!inputs) throw new Error("Case validation needs a Layer 4 case.");
  const outcomes: ValidationComparison[] = [];
  let error: string | null = null;
  try {
    const folder = join(root, phase), work = join(folder, "project"), home = join(folder, "home");
    await validationProject(inputs, work, phase === ValidationPhase.solution);
    await mkdir(home, { recursive: true });
    const space: RunSpace = { root: folder, work, home, project: suite.project };
    const build = await (tools.build ?? ((space) => buildProject(Isolation.docker, space, CaseValidationTimeoutMs)))(space);
    outcomes.push(required(ValidationCheck.build, build.passed), required(ValidationCheck.typecheck, build.typecheck === true));
    const input: SuiteCheckInput = { project: work, build, html: build.html === null ? null : join(work, build.html), shotsFolder: join(folder, "evidence", "shots") };
    const checked = await runBrownfieldChecks(inputs, baseline, input, tools.gates);
    outcomes.push(...compareValidation(expected, checked.checks, phase).map((check) => ({ ...check, evidence: check.evidence === null ? null : String(sanitizeRecords(check.evidence)) })));
  } catch (cause) {
    error = String(sanitizeRecords(cause instanceof Error ? cause.message : String(cause)));
    outcomes.push(required(ValidationCheck.stage, false));
  }
  return { phase, passed: outcomes.every((entry) => entry.matches), outcomes, error };
}

export async function validateCase(suite: Suite, tools: CaseValidationServices = {}): Promise<CaseValidation> {
  const inputs = suite.brownfield;
  if (!inputs) throw new Error("validate-case accepts Layer 4 cases only.");
  const expected = await readValidationExpectations(inputs.case), before = await validationInputs(inputs);
  if (JSON.stringify(before.case) !== JSON.stringify(inputs.case.inputs)) throw new Error("Case inputs changed after loading.");
  if (JSON.stringify(before.referenceSource) !== JSON.stringify(before.reference)) throw new Error("The frozen reference changed after loading.");
  const root = await mkdtemp(join(tmpdir(), TemporaryPrefix));
  try {
    const snapshot = join(root, "starting-snapshot");
    await validationProject(inputs, snapshot, false);
    const baseline = await projectHashes(snapshot), stages: CaseValidationStage[] = [];
    for (const phase of Object.values(ValidationPhase)) {
      const stage = await validateStage(suite, root, phase, baseline, expected, tools);
      const unchanged = await validationInputs(inputs).then((after) => JSON.stringify(before) === JSON.stringify(after), () => false);
      stages.push({ ...stage, passed: stage.passed && unchanged, outcomes: [...stage.outcomes, required(ValidationCheck.inputs, unchanged)] });
    }
    const report: CaseValidation = { schemaVersion: CaseValidationVersion, case: suite.id, passed: stages.every((stage) => stage.passed), inputs: before, stages };
    const target = join(inputs.case.root, inputs.case.checks, BrownfieldFiles.validation), temporary = `${target}.tmp`;
    await ordinaryPath(target); await ordinaryPath(temporary);
    await writeFile(temporary, `${JSON.stringify(report, null, 2)}\n`, { flag: "wx" });
    try { await rename(temporary, target); }
    finally { await rm(temporary, { force: true }); }
    return report;
  } finally {
    if (dirname(resolve(root)) !== resolve(tmpdir())) throw new Error("Case validation cleanup escaped its temporary folder.");
    await rm(root, { recursive: true, force: true, maxRetries: 3 });
  }
}
