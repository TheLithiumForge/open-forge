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
import { readChangeChecks } from "./change-checks.ts";
import { runChangeChecks } from "./container.ts";
import { regressionDefinition, regressionOutcomes } from "./regression.ts";
import { BrownfieldGuard, changedFiles, projectHashes, scopeOutcome } from "./scope.ts";
import { protectedHashes, protectedOutcome } from "./protected.ts";
import type { CheckOutcome, Coverage } from "../grading.types.ts";
import { coverage } from "../counting.ts";
import { CheckStatus } from "../../history/statuses.ts";

export interface BrownfieldServices {
  readonly regression?: (input: SuiteCheckInput) => Promise<LayerOutput>;
  readonly changes?: typeof runChangeChecks;
  readonly protectedRoots?: readonly string[];
}
export const BrownfieldReport = "brownfield.json";
export interface BrownfieldOutput extends SuiteCheckOutput {
  readonly groups: { readonly feature: Coverage; readonly repair: Coverage; readonly regression: Coverage };
}

export async function runBrownfieldChecks(inputs: BrownfieldInputs, baseline: readonly FileHash[], input: SuiteCheckInput, services: BrownfieldServices = {}): Promise<BrownfieldOutput> {
  const item = inputs.case;
  if (JSON.stringify(hashesWithoutValidation(await inventory(item.root), item.checks)) !== JSON.stringify(item.inputs)) throw new Error("The frozen brownfield case inputs changed before verification.");
  const frozen = regressionDefinition(await loadDefinition(LayerId.three), item.supersedes);
  const declared = await readChangeChecks(item);
  const checks: CheckOutcome[] = [];
  const roots = services.protectedRoots ?? [join(evalsRoot, "benchmarks", "neon-pairs"), join(evalsRoot, "grading", "brownfield")];
  const before = await protectedHashes(roots, input.project);
  let regression: LayerOutput | undefined;
  try { regression = await (services.regression ?? runLayer3Checks)(input); }
  catch (error) {
    for (const check of frozen.checks) checks.push({ id: `regression-${check.id}`, area: "regression", status: CheckStatus.error, evidence: error instanceof Error ? error.message : String(error) });
  }
  if (regression) checks.push(...regressionOutcomes(frozen, regression.assertions, regression.checks));
  try { checks.push(...await (services.changes ?? runChangeChecks)(item, declared, input)); }
  catch (error) {
    for (const check of declared) checks.push({ id: `${check.kind}-${check.id}`, area: check.kind, status: CheckStatus.error, evidence: error instanceof Error ? error.message : String(error) });
  }
  checks.push(scopeOutcome(changedFiles(baseline, await projectHashes(input.project), item.diffScope)));
  checks.push(await protectedOutcome(item, input.project, before, await protectedHashes(roots, input.project)));
  const groups = { feature: coverage(checks.filter((check) => check.area === "feature")), repair: coverage(checks.filter((check) => check.area === "repair")), regression: coverage(checks.filter((check) => check.area === "regression")) };
  await mkdir(dirname(input.shotsFolder), { recursive: true });
  await writeFile(join(dirname(input.shotsFolder), BrownfieldReport), `${JSON.stringify({ schemaVersion: 1, groups, checks }, null, 2)}\n`);
  const ids = checks.map((check) => check.id);
  return { checks, selection: { applicable: ids, stated: ids, working: [...frozen.working.map((id) => `regression-${id}`), BrownfieldGuard.protected] },
    performance: regression?.performance ?? null, shots: regression?.shots ?? [], groups };
}
