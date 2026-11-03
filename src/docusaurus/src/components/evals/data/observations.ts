import type { Assessment, Batch, Checks, Choice, Judge, Run } from "../records/records.types.ts";
import { latestRun, type AssessedRun } from "./analysis.ts";
export const ReferenceImplementer = "reference";
export const Layers = [1, 2, 3, 4] as const;
// A team is part of the implementer, so a solo agent and the same agent leading subagents count apart. A solo key is unchanged.
export const choiceKey = (choice: Choice | null | undefined) => JSON.stringify(choice == null ? null
  : choice.team === undefined ? [choice.tool, choice.model, choice.effort] : [choice.tool, choice.model, choice.effort, choice.team.model, choice.team.effort]);
export const choiceLabel = (choice: Choice | null | undefined) => choice == null ? "Missing judge" : `${choice.tool} ${choice.model ?? "unreported model"} (${choice.effort ?? "unreported effort"})`;
export const isReference = (run: Run) => run.implementer === ReferenceImplementer;
export const checkKey = (item: Checks | Judge) => JSON.stringify([item.versions.checker, item.versions.rubric, item.versions.spec ?? null, item.versions.specVersion ?? null]);
function lastBy<T>(items: readonly T[], key: (item: T) => string): T[] {
  return [...new Map(items.map((item) => [key(item), item])).values()];
}
export function judges(batch: Batch): readonly (Choice | null)[] {
  const configured = batch.experiment.config.judges ?? (batch.experiment.config.judge == null ? [] : [batch.experiment.config.judge]);
  const choices = [...configured,
  ...batch.assessments.flatMap((item) => item.kind === "judge" && item.requestedModel !== null ? [item.requestedModel] : [])];
  return choices.length === 0 ? [null] : lastBy(choices, choiceKey);
}
export function observations(batch: Batch, includeReference = false): AssessedRun[] {
  return batch.runs.filter((run) => isReference(run) === includeReference).flatMap((run) => {
    const records = batch.assessments.filter((item) => item.kind !== "pick" && item.runId === run.id);
    const checks = lastBy(records.filter((item): item is Checks => item.kind === "checks"), checkKey);
    const ballots = lastBy(records.filter((item): item is Judge => item.kind === "judge"), (item) => JSON.stringify([choiceKey(item.requestedModel), item.versions, item.reportedModel]));
    const views: AssessedRun[] = [];
    for (const check of checks)
      for (const choice of judges(batch)) {
        const matches = ballots.filter((item) => choiceKey(item.requestedModel) === choiceKey(choice) && checkKey(item) === checkKey(check));
        for (const judge of matches.length === 0 ? [null] : matches) {
          const selected: Assessment[] = judge === null ? [check] : [check, judge];
          views.push({ ...latestRun(run, selected), judgeChoice: choice });
        }
      }
    for (const judge of ballots.filter((item) => !checks.some((check) => checkKey(check) === checkKey(item)))) {
      views.push({ ...latestRun({ ...run, functionalResult: null }, [judge]), judgeChoice: judge.requestedModel });
    }
    // A run still waiting or working has nothing to compare yet. Published batches are finished, so this only trims local views.
    if (views.length === 0 && run.finishedAt === null)
      return [];
    if (views.length === 0)
      for (const choice of judges(batch))
        views.push({ ...latestRun(run, []), judgeChoice: choice });
    return views;
  });
}
