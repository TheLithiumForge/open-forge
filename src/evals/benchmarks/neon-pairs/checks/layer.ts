import { readFile } from "node:fs/promises";
import { CheckStatus } from "../../../history/statuses.ts";
import type { CheckOutcome, CheckSelection } from "../../../grading/grading.types.ts";
import { CheckId } from "./check-ids.ts";

export const LayerVersion = "L1-v3";
export const LayerFiles = { layer: "layers/L1.json", requirements: "layers/L1.requirements.json", rubric: "rubrics/L1.json" } as const;
export const CheckGroup = { delivery: "delivery", stack: "stack", large: "large", play: "play", score: "score", phone: "phone", keyboard: "keyboard" } as const;
type CheckGroup = (typeof CheckGroup)[keyof typeof CheckGroup];
interface DeclaredCheck { readonly id: string; readonly area: string; readonly group: CheckGroup }
export interface Layer {
  readonly version: string;
  readonly suite: string;
  readonly checks: readonly DeclaredCheck[];
  readonly stated: readonly string[];
  readonly working: readonly string[];
  readonly wishes: readonly string[];
}
const ExpectedCounts = { stated: 16, working: 6, wishes: 3 } as const;
const groupValues: readonly string[] = Object.values(CheckGroup);
const checkValues: readonly string[] = Object.values(CheckId);

export function record(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}

function ids(value: unknown): string[] {
  if (!Array.isArray(value) || !value.every((id: unknown): id is string => typeof id === "string" && checkValues.includes(id)) || new Set(value).size !== value.length) throw new Error("Invalid Layer 1 check IDs.");
  return value;
}

function group(value: unknown): CheckGroup {
  switch (value) {
    case CheckGroup.delivery: case CheckGroup.stack: case CheckGroup.large: case CheckGroup.play:
    case CheckGroup.score: case CheckGroup.phone: case CheckGroup.keyboard: return value;
    default: throw new Error("Invalid Layer 1 check group.");
  }
}

export function parseLayer(value: unknown): Layer {
  if (!record(value) || value["version"] !== LayerVersion || typeof value["suite"] !== "string" || !Array.isArray(value["checks"])) throw new Error("Invalid Layer 1 definition.");
  const checks = value["checks"].map((check: unknown): DeclaredCheck => {
    if (!record(check) || typeof check["id"] !== "string" || !checkValues.includes(check["id"]) || typeof check["area"] !== "string" || !groupValues.includes(String(check["group"]))) throw new Error("Invalid Layer 1 check declaration.");
    return { id: check["id"], area: check["area"], group: group(check["group"]) };
  });
  const stated = ids(value["stated"]), working = ids(value["working"]), wishes = ids(value["wishes"]);
  if (stated.length !== ExpectedCounts.stated || working.length !== ExpectedCounts.working || wishes.length !== ExpectedCounts.wishes || checks.length !== checkValues.length || new Set(checks.map((check) => check.id)).size !== checks.length) throw new Error("Layer 1 check counts differ from L1-v3.");
  if (working.some((id) => !stated.includes(id)) || wishes.some((id) => stated.includes(id)) || new Set([...stated, ...wishes]).size !== checks.length) throw new Error("Layer 1 sets overlap or omit checks.");
  return { version: LayerVersion, suite: value["suite"], checks, stated, working, wishes };
}

export async function loadLayer(): Promise<Layer> {
  return parseLayer(JSON.parse(await readFile(new URL(`../${LayerFiles.layer}`, import.meta.url), "utf8")));
}

export function selection(layer: Layer): CheckSelection {
  return { applicable: layer.stated, stated: layer.stated, working: layer.working };
}

export function completeChecks(layer: Layer, checks: readonly CheckOutcome[], cause?: string): CheckOutcome[] {
  const first = new Map<string, CheckOutcome>();
  for (const check of checks) if (!first.has(check.id)) first.set(check.id, check);
  return layer.checks.map((declaration) => {
    const actual = first.get(declaration.id);
    if (actual) return actual;
    const stopped = layer.checks.filter((check) => check.group === declaration.group).map((check) => first.get(check.id)).find((check) => check && check.status !== CheckStatus.pass);
    return { id: declaration.id, area: declaration.area, status: stopped ? CheckStatus.blocked : CheckStatus.error,
      evidence: stopped ? `Stopped after ${stopped.id}: ${stopped.evidence}` : cause ?? "The checker returned no outcome for this declared check." };
  });
}
