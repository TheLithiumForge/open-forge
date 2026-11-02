import type { Batch } from "../records/records.types.ts";
import { ballotShare, interval, type Interval } from "./analysis.ts";
import { checkKey, choiceKey, choiceLabel, judges, observations } from "./observations.ts";
export const JudgeAreas = ["rules", "code", "intent"] as const;
export interface Difference {
  readonly key: string;
  readonly setup: string;
  readonly implementer: string;
  readonly first: string;
  readonly second: string;
  readonly scores: readonly {
    readonly area: string;
    readonly value: Interval;
  }[];
  readonly missing: number;
  readonly errors: number;
  readonly incompatible: number;
}
export function differences(batch: Batch): Difference[] {
  const choices = judges(batch);
  const views = observations(batch);
  const rows: Difference[] = [];
  for (const [index, first] of choices.entries())
    for (const second of choices.slice(index + 1)) {
      const groups = new Map<string, typeof views>();
      for (const view of views) {
        const key = JSON.stringify([view.agent, view.reportedModel, view.setup, view.checksAssessment?.versions ?? null]);
        groups.set(key, [...groups.get(key) ?? [], view]);
      }
      for (const [groupKey, members] of groups) {
        const candidates = [...new Set(members.map((run) => run.id))].map((id) => ({
          first: members.filter((run) => run.id === id && choiceKey(run.judgeChoice) === choiceKey(first)).at(-1),
          second: members.filter((run) => run.id === id && choiceKey(run.judgeChoice) === choiceKey(second)).at(-1),
        }));
        const gradingGroups = new Map<string, typeof candidates>();
        for (const pair of candidates) {
          const key = JSON.stringify([groupKey, pair.first?.judgeAssessment?.versions, pair.second?.judgeAssessment?.versions,
            pair.first?.judgeAssessment?.reportedModel, pair.second?.judgeAssessment?.reportedModel,
            pair.first?.judgeAssessment?.ballots, pair.second?.judgeAssessment?.ballots]);
          gradingGroups.set(key, [...gradingGroups.get(key) ?? [], pair]);
        }
        for (const [key, pairs] of gradingGroups) {
          const missing = pairs.filter((pair) => pair.first?.judgeAssessment == null || pair.second?.judgeAssessment == null).length;
          const errors = pairs.filter((pair) => pair.first?.judgeAssessment?.error != null || pair.second?.judgeAssessment?.error != null).length;
          const compatible = (pair: typeof pairs[number]) => {
            const left = pair.first?.judgeAssessment, right = pair.second?.judgeAssessment;
            return left != null && right != null && left.error === null && right.error === null
              && checkKey(left) === checkKey(right) && JSON.stringify(left.versions) === JSON.stringify(right.versions);
          };
          const sample = members[0];
          if (sample === undefined)
            continue;
          rows.push({
            key: JSON.stringify([key, first, second]), setup: sample.setup, implementer: choiceLabel(sample.agent),
            first: choiceLabel(first), second: choiceLabel(second), missing, errors,
            incompatible: pairs.filter((pair) => pair.first?.judgeAssessment != null && pair.second?.judgeAssessment != null && !compatible(pair)
              && pair.first.judgeAssessment.error === null && pair.second.judgeAssessment.error === null).length,
            scores: JudgeAreas.map((area) => ({
              area, value: interval(pairs.map((pair) => {
                if (!compatible(pair) || pair.first === undefined || pair.second === undefined)
                  return null;
                const a = ballotShare(pair.first, area), b = ballotShare(pair.second, area);
                return a === null || b === null ? null : b - a;
              }))
            })),
          });
        }
      }
    }
  return rows;
}
