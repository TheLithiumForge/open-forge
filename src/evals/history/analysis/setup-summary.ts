import type { RunAnalysis } from "./analysis.types.ts";
import { ContextClass, ToolKind } from "./classes.ts";

/** One setup's mean objective measures over its analyzed runs. Null means no run reported the measure. */
export interface SetupUsage {
  readonly setup: string;
  readonly runs: number;
  readonly newInput: number | null;
  readonly cachedInput: number | null;
  readonly cacheWrite: number | null;
  readonly output: number | null;
  readonly requests: number | null;
  readonly peakContext: number | null;
  /** Input before the first edit as a share of all input, from runs whose logs report each request. */
  readonly beforeEdit: number | null;
  /** Mean input tokens per run by class and by file type. Estimated, but each run's parts sum to its exact input. */
  readonly classes: Readonly<Record<string, number | null>>;
  readonly fileTypes: Readonly<Record<string, number>>;
  readonly calls: Readonly<Record<string, number | null>>;
  readonly failed: number | null;
  readonly stepsBeforeEdit: number | null;
  readonly repeatedReads: number | null;
  readonly filesRead: number | null;
}

export const mean = (values: readonly (number | null | undefined)[]): number | null => {
  const known = values.filter((value): value is number => typeof value === "number");
  return known.length === 0 ? null : known.reduce((sum, value) => sum + value, 0) / known.length;
};

/** The mean of each key's value, keyed by class or tool kind. */
function byKey<K extends string>(keys: readonly K[], analyses: readonly RunAnalysis[], pick: (analysis: RunAnalysis, key: K) => number | null): Record<string, number | null> {
  return Object.fromEntries(keys.map((key) => [key, mean(analyses.map((analysis) => pick(analysis, key)))]));
}

/** Each setup's means, in the batch's setup order. Runs without token data count for process measures only. */
export function setupUsage(setups: readonly string[], analyses: readonly { readonly setup: string; readonly analysis: RunAnalysis }[]): SetupUsage[] {
  return setups.map((setup) => {
    const group = analyses.filter((item) => item.setup === setup).map((item) => item.analysis);
    const split = group.filter((analysis) => analysis.parts.length > 0);
    const types = [...new Set(split.flatMap((analysis) => analysis.parts.flatMap((part) => (part.fileType === null ? [] : [part.fileType]))))];
    const fileTypes = Object.fromEntries(types.map((type) => [type, mean(split.map((analysis) => analysis.parts.filter((part) => part.fileType === type).reduce((sum, part) => sum + part.input, 0))) ?? 0]));
    const input = (analysis: RunAnalysis) => analysis.tokens === null ? null : analysis.tokens.newInput + analysis.tokens.cachedInput + analysis.tokens.cacheWrite;
    return {
      setup, runs: group.length,
      newInput: mean(group.map((analysis) => analysis.tokens?.newInput)), cachedInput: mean(group.map((analysis) => analysis.tokens?.cachedInput)),
      cacheWrite: mean(group.map((analysis) => analysis.tokens?.cacheWrite)), output: mean(group.map((analysis) => analysis.tokens?.output)),
      requests: mean(group.map((analysis) => analysis.requests)), peakContext: mean(group.map((analysis) => analysis.peakContext)),
      beforeEdit: mean(group.map((analysis) => { const all = input(analysis); return analysis.beforeEdit === null || all === null || all === 0 ? null : analysis.beforeEdit.input / all; })),
      classes: byKey(Object.values(ContextClass), split, (analysis, type) => analysis.parts.filter((part) => part.class === type).reduce((sum, part) => sum + part.input, 0)),
      fileTypes,
      calls: byKey(Object.values(ToolKind), group, (analysis, kind) => analysis.process?.calls[kind] ?? null),
      failed: mean(group.map((analysis) => analysis.process?.failed)), stepsBeforeEdit: mean(group.map((analysis) => analysis.process?.stepsBeforeEdit)),
      repeatedReads: mean(group.map((analysis) => analysis.process?.repeatedReads)), filesRead: mean(group.map((analysis) => analysis.process?.filesRead)),
    };
  });
}
