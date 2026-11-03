import { readFile } from "node:fs/promises";
import { join } from "node:path";
import { isRecord } from "../../materialize/frontmatter.ts";
import type { BrownfieldCase } from "../../materialize/brownfield-case.ts";
import { ChangeType } from "../../materialize/brownfield-case.ts";
import { CheckStatus } from "../../history/statuses.ts";
import type { CheckOutcome } from "../grading.types.ts";
import type { AssertionOutcome } from "../../benchmarks/neon-pairs/checks/spec-layers/outcomes.ts";

export const ChangeCheckFiles = { manifest: "checks.json", worker: "worker.ts", report: "report.json" } as const;
export const ChangeCheckVersion = 1;
/** A case's own checks number 10 to 20. A case graded only by full-spec clauses needs at least three of them. */
export const ChangeCheckLimits = { minimum: 10, maximum: 20, clauseMinimum: 3, reportBytes: 1024 * 1024 } as const;
export const ChangeKind = { feature: "feature", repair: "repair" } as const;
/** Clause checks take their result from the full-spec checker's assertion for one clause, so the case needs no worker of its own. */
export const ChangeMethod = { clause: "clause" } as const;
const ClausePattern = /^[A-Z]\d{2}\.\d+$/;
export interface ChangeCheck { readonly id: string; readonly kind: (typeof ChangeKind)[keyof typeof ChangeKind]; readonly clause?: string }

export async function readChangeChecks(item: BrownfieldCase): Promise<ChangeCheck[]> {
  const value: unknown = JSON.parse(await readFile(join(item.root, item.checks, ChangeCheckFiles.manifest), "utf8"));
  if (!isRecord(value) || value["schemaVersion"] !== ChangeCheckVersion || !Array.isArray(value["checks"])) throw new Error("Invalid brownfield checks/checks.json.");
  const checks = value["checks"].map((check: unknown): ChangeCheck => {
    if (!isRecord(check) || typeof check["id"] !== "string" || !/^[\w-]+$/.test(check["id"]) || check["kind"] !== ChangeKind.feature && check["kind"] !== ChangeKind.repair) throw new Error("Invalid brownfield change check.");
    if (check["method"] !== ChangeMethod.clause) return { id: check["id"], kind: check["kind"] };
    if (typeof check["clause"] !== "string" || !ClausePattern.test(check["clause"])) throw new Error("A clause check names one full-spec clause.");
    return { id: check["id"], kind: check["kind"], clause: check["clause"] };
  });
  const clauses = checks.flatMap((check) => check.clause === undefined ? [] : [check.clause]);
  const minimum = clauses.length === checks.length ? ChangeCheckLimits.clauseMinimum : ChangeCheckLimits.minimum;
  if (checks.length < minimum || checks.length > ChangeCheckLimits.maximum || new Set(checks.map((check) => check.id)).size !== checks.length || new Set(clauses).size !== clauses.length) {
    throw new Error(`A case needs ${minimum} to ${ChangeCheckLimits.maximum} distinct change checks.`);
  }
  for (const kind of Object.values(ChangeKind)) {
    const needed = item.changeType === ChangeType.combined || item.changeType === (kind === ChangeKind.feature ? ChangeType.feature : ChangeType.repair);
    if (checks.some((check) => check.kind === kind) !== needed) throw new Error("Change check groups differ from the case changeType.");
  }
  return checks;
}

export function parseChangeReport(value: unknown, declared: readonly ChangeCheck[]): CheckOutcome[] {
  if (!isRecord(value) || value["schemaVersion"] !== ChangeCheckVersion || !Array.isArray(value["checks"])) throw new Error("Invalid brownfield change report.");
  const seen = new Set<string>();
  const outcomes = value["checks"].map((entry: unknown): CheckOutcome => {
    if (!isRecord(entry) || typeof entry["id"] !== "string" || seen.has(entry["id"]) || typeof entry["status"] !== "string"
      || !Object.values(CheckStatus).some((status) => status === entry["status"]) || typeof entry["evidence"] !== "string" || !entry["evidence"].trim()) throw new Error("Invalid or duplicate change outcome.");
    const check = declared.find((check) => check.id === entry["id"]);
    if (!check) throw new Error("The change report names an undeclared check.");
    const status = Object.values(CheckStatus).find((status) => status === entry["status"]);
    if (status === undefined) throw new Error("Unknown change outcome status.");
    seen.add(check.id);
    return { id: `${check.kind}-${check.id}`, area: check.kind, status, evidence: entry["evidence"] };
  });
  if (outcomes.length !== declared.length) throw new Error("The change report is incomplete.");
  return outcomes;
}

/** The full-spec clauses a case grades as its own change checks. */
export function clauseTargets(declared: readonly ChangeCheck[]): string[] {
  return declared.flatMap((check) => check.clause === undefined ? [] : [check.clause]);
}

/** Clause checks from the full-spec checker's assertions. When that checker gave no result, each one carries its failure. */
export function clauseOutcomes(declared: readonly ChangeCheck[], assertions: readonly AssertionOutcome[] | null, failure: string): CheckOutcome[] {
  return declared.flatMap((check) => {
    if (check.clause === undefined) return [];
    const found = assertions?.find((outcome) => outcome.clause === check.clause);
    const evidence = found === undefined ? assertions === null ? failure : `The full-spec checker reported no assertion for ${check.clause}.` : `${check.clause}: ${found.evidence}`;
    return [{ id: `${check.kind}-${check.id}`, area: check.kind, status: found?.status ?? CheckStatus.error, evidence }];
  });
}
