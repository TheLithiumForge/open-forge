import type { SuiteCheckOutput } from "../../suite-checks.types.ts";
import type { CheckOutcome } from "../../../grading/grading.types.ts";
import { CheckStatus } from "../../../history/statuses.ts";
import { record, selection, type Layer } from "./layer.ts";
import { EvidenceShotNames } from "./screenshots.ts";

const ShotNames = new Set(EvidenceShotNames);
const EvidenceLimit = 64 * 1024;

function checkStatus(value: unknown): CheckStatus {
  switch (value) {
    case CheckStatus.pass: case CheckStatus.fail: case CheckStatus.error: case CheckStatus.blocked: return value;
    default: throw new Error("Verifier returned an invalid check status.");
  }
}

export function parseCheckReport(value: unknown, layer: Layer): SuiteCheckOutput {
  if (!record(value) || !Array.isArray(value["checks"]) || value["checks"].length !== layer.checks.length || !Array.isArray(value["shots"])) throw new Error("Verifier returned an incomplete report.");
  const seen = new Set<string>();
  const checks = value["checks"].map((check: unknown): CheckOutcome => {
    if (!record(check) || typeof check["id"] !== "string" || seen.has(check["id"]) || typeof check["evidence"] !== "string" || !check["evidence"].trim() || check["evidence"].length > EvidenceLimit) throw new Error("Verifier returned an invalid check outcome.");
    const declared = layer.checks.find((entry) => entry.id === check["id"]);
    if (!declared) throw new Error("Verifier returned an undeclared check.");
    seen.add(declared.id);
    return { id: declared.id, area: declared.area, status: checkStatus(check["status"]), evidence: check["evidence"] };
  });
  const shots = value["shots"];
  if (!shots.every((shot: unknown): shot is string => typeof shot === "string" && ShotNames.has(shot)) || new Set(shots).size !== shots.length) throw new Error("Verifier returned an invalid screenshot name.");
  const performance = value["performance"];
  if (performance !== null && (!record(performance) || ![performance["firstRenderMs"], performance["largeBoardTapP95Ms"]].every((number) => number === null || (typeof number === "number" && Number.isFinite(number) && number >= 0)))) throw new Error("Verifier returned invalid performance data.");
  return { checks, selection: selection(layer), performance: record(performance) ? {
    firstRenderMs: typeof performance["firstRenderMs"] === "number" ? performance["firstRenderMs"] : null,
    largeBoardTapP95Ms: typeof performance["largeBoardTapP95Ms"] === "number" ? performance["largeBoardTapP95Ms"] : null,
  } : null, shots };
}
