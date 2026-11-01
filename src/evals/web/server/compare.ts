import { randomInt } from "node:crypto";
import { existsSync } from "node:fs";
import { join } from "node:path";
import { ResultFiles } from "../../paths.ts";
import type { Preference, RunRecord } from "../../history/results.types.ts";
import { RunStatus } from "../../history/statuses.ts";
import { runFolder } from "../../history/store.ts";
import { agentLabel } from "../../history/summary.ts";
import type { ComparePair } from "./api.types.ts";
import type { AssessmentRecord } from "../../history/records.types.ts";
import { gradingKey } from "./result-groups.ts";

/** Each game opens on its own origin, so games never share saved best scores. */
export function gameUrl(runId: string, port: number): string {
  return `http://${runId}.localhost:${port}/`;
}

function pairKey(a: string, b: string): string {
  return [a, b].sort().join(":");
}

/** Every pair of finished games from the same tool and model, since setups are compared only within one. */
export function allPairs(runs: readonly RunRecord[], folder = runFolder, assessments: readonly AssessmentRecord[] = []): [string, string][] {
  const playable = runs.filter((run) => run.finishedAt !== null && (run.status === RunStatus.done || run.status === RunStatus.failed)
    && run.build?.passed === true && existsSync(join(folder(run.batchId, run.id), ResultFiles.game)));
  const pairs: [string, string][] = [];
  for (const [index, a] of playable.entries()) {
    for (const b of playable.slice(index + 1)) if (agentLabel(a.agent) === agentLabel(b.agent) && a.reportedModel === b.reportedModel && gradingKey(a, assessments) === gradingKey(b, assessments)) pairs.push([a.id, b.id]);
  }
  return pairs;
}

/** A random pair not judged yet, with a random side for each game. */
export function nextPair(runs: readonly RunRecord[], preferences: readonly Preference[], port: number, folder = runFolder, assessments: readonly AssessmentRecord[] = []): ComparePair | null {
  const judged = new Set(preferences.map((preference) => pairKey(preference.left, preference.right)));
  const open = allPairs(runs, folder, assessments).filter(([a, b]) => !judged.has(pairKey(a, b)));
  const pair = open[randomInt(Math.max(1, open.length))];
  if (pair === undefined) return null;
  const [left, right] = randomInt(2) === 0 ? pair : ([pair[1], pair[0]] as const);
  return { left, right, leftGame: gameUrl(left, port), rightGame: gameUrl(right, port) };
}
