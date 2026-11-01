import { join } from "node:path";
import { EvidenceShotNames } from "../../benchmarks/neon-pairs/checks/screenshots.ts";
import { ResultFiles } from "../../paths.ts";
import type { RunView } from "../../history/records.types.ts";
import { assessmentEvidenceFolder, runFolder } from "../../history/store.ts";

export interface ShotStores {
  readonly run: (batch: string, run: string) => string;
  readonly assessment: (batch: string, assessment: string) => string;
}
const stores: ShotStores = { run: runFolder, assessment: assessmentEvidenceFolder };

export function judgeImagePaths(run: Pick<RunView, "batchId" | "id" | "shots">, paths: ShotStores = stores): string[] {
  return run.shots.map((shot) => {
    const parts = shot.split("/");
    const name = parts.at(-1);
    if (!name || !EvidenceShotNames.includes(name) || parts.length > 2 || parts.some((part) => !part || part === "." || part === ".." || /[\\:]/.test(part))) throw new Error("Invalid judge screenshot path.");
    const assessment = parts.length === 2 ? parts[0] : undefined;
    const root = assessment === undefined ? paths.run(run.batchId, run.id) : paths.assessment(run.batchId, assessment);
    return join(root, ResultFiles.shots, name);
  });
}
