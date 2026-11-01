import { addPreference, readAssessments, readBatch, readRuns, runFolder } from "../../history/store.ts";
import type { Preference } from "../../history/results.types.ts";
import { allPairs } from "./compare.ts";

interface PickHistory {
  readonly readBatch: typeof readBatch; readonly readRuns: typeof readRuns; readonly readAssessments: typeof readAssessments;
  readonly runFolder: typeof runFolder; readonly addPreference: typeof addPreference;
}
export async function recordPick(batchId: string, preference: Preference, history: PickHistory = { readBatch, readRuns, readAssessments, runFolder, addPreference }): Promise<boolean> {
  const batch = await history.readBatch(batchId);
  const pairs = allPairs(await history.readRuns(batch), (batch, run) => history.runFolder(batch, run), await history.readAssessments(batchId));
  const eligible = pairs.some(([a, b]) => (a === preference.left && b === preference.right) || (b === preference.left && a === preference.right));
  if (!eligible) return false;
  await history.addPreference(batchId, preference);
  return true;
}
