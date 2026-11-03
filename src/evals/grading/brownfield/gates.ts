import { dirname, join } from "node:path";
import { mkdir, writeFile } from "node:fs/promises";
import { evalsRoot } from "../../paths.ts";
import type { BrownfieldInputs } from "../../materialize/brownfield-inputs.ts";
import { hashesWithoutValidation } from "../../materialize/brownfield-case.ts";
import { inventory, type FileHash } from "../../materialize/files.ts";
import type { SuiteCheckInput, SuiteCheckOutput } from "../../benchmarks/suite-checks.types.ts";
import { runLayer3Checks, type LayerOutput } from "../../benchmarks/neon-pairs/checks/spec-layers/container.ts";
import { loadDefinition } from "../../benchmarks/neon-pairs/checks/spec-layers/load.ts";
import { LayerId } from "../../benchmarks/neon-pairs/checks/spec-layers/requirements.ts";
import { clauseOutcomes, clauseTargets, readChangeChecks } from "./change-checks.ts";
import { runChangeChecks } from "./container.ts";
import { caseRegression, regressionOutcomes, regressionReview } from "./regression.ts";
import { BrownfieldGuard, changedFiles, projectHashes, scopeOutcome } from "./scope.ts";
import { protectedHashes, protectedOutcome } from "./protected.ts";
import type { CheckOutcome, Coverage } from "../grading.types.ts";
import { coverage } from "../counting.ts";
import { CheckStatus } from "../../history/statuses.ts";
import type { ScopePolicy } from "../../materialize/scope.types.ts";
import { captureScope, verifyScope } from "../../materialize/scope-policy.ts";

export interface BrownfieldServices {
  readonly scope?: ScopePolicy;
  readonly regression?: (input: SuiteCheckInput) => Promise<LayerOutput>;
  readonly changes?: typeof runChangeChecks;
  readonly protectedRoots?: readonly string[];
}
export const BrownfieldReport = "brownfield.json";
export interface BrownfieldOutput extends SuiteCheckOutput {
  readonly pending: readonly CheckOutcome[];
  readonly groups: { readonly feature: Coverage; readonly repair: Coverage; readonly regression: Coverage };
}

export async function runBrownfieldChecks(inputs: BrownfieldInputs, baseline: readonly FileHash[], input: SuiteCheckInput, services: BrownfieldServices = {}): Promise<BrownfieldOutput> {
  const item = inputs.case;
  if (JSON.stringify(hashesWithoutValidation(await inventory(item.root), item.checks)) !== JSON.stringify(item.inputs)) throw new Error("The frozen brownfield case inputs changed before verification.");
  const declared = await readChangeChecks(item);
  const frozen = caseRegression(await loadDefinition(LayerId.three), item.supersedes, clauseTargets(declared));
  const workerChecks = declared.filter((check) => check.clause === undefined);
  const policy = services.scope ?? captureScope(inputs.seed, false);
  verifyScope(policy, baseline);
  const checks: CheckOutcome[] = [];
  const roots = services.protectedRoots ?? [join(evalsRoot, "benchmarks", "neon-pairs"), join(evalsRoot, "grading", "brownfield")];
  const before = await protectedHashes(roots, input.project);
  let regression: LayerOutput | undefined, regressionError = "";
  try { regression = await (services.regression ?? runLayer3Checks)(input); }
  catch (error) {
    regressionError = error instanceof Error ? error.message : String(error);
    for (const check of frozen.checks) checks.push({ id: `regression-${check.id}`, area: "regression", status: CheckStatus.error, evidence: regressionError });
  }
  if (regression) checks.push(...regressionOutcomes(frozen, regression.assertions, regression.checks));
  checks.push(...clauseOutcomes(declared, regression?.assertions ?? null, regressionError));
  if (workerChecks.length > 0) {
    try { checks.push(...await (services.changes ?? runChangeChecks)(item, workerChecks, input)); }
    catch (error) {
      for (const check of workerChecks) checks.push({ id: `${check.kind}-${check.id}`, area: check.kind, status: CheckStatus.error, evidence: error instanceof Error ? error.message : String(error) });
    }
  }
  checks.push(scopeOutcome(changedFiles(baseline, await projectHashes(input.project, policy), item.diffScope, policy)));
  checks.push(await protectedOutcome(item, input.project, before, await protectedHashes(roots, input.project)));
  const groups = { feature: coverage(checks.filter((check) => check.area === "feature")), repair: coverage(checks.filter((check) => check.area === "repair")), regression: coverage(checks.filter((check) => check.area === "regression")) };
  await mkdir(dirname(input.shotsFolder), { recursive: true });
  const pending = regressionReview(frozen);
  await writeFile(join(dirname(input.shotsFolder), BrownfieldReport), `${JSON.stringify({ schemaVersion: 1, groups, checks, pending }, null, 2)}\n`);
  const ids = checks.map((check) => check.id);
  return { checks: [...checks, ...pending], selection: { applicable: ids, stated: ids, working: [...frozen.working.map((id) => `regression-${id}`), BrownfieldGuard.protected] },
    performance: regression?.performance ?? null, shots: regression?.shots ?? [], groups, pending };
}
