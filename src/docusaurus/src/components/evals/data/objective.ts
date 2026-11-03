import type { Analysis, Batch } from "../records/records.types.ts";
import type { Comparison } from "./aggregate.ts";
import type { Cell } from "./cells.ts";
import { bySetup, identity, type ChartRow } from "./chart-data.ts";

/**
 * The objective measures from each run's analysis: where its input tokens went by class and file type, and its tool
 * calls by kind. Totals are exact. The split by class and file type is estimated, and each run's parts sum to its input.
 */

/** Context classes in reading order: the up-front text, the project's files, tool output, then the conversation. */
export const ContextClasses = [
  { key: "upfront", title: "Up front" }, { key: "spec", title: "Spec documents" }, { key: "framework", title: "Open Forge files" },
  { key: "code", title: "Code" }, { key: "tests", title: "Tests" }, { key: "search", title: "Searches and listings" },
  { key: "test-output", title: "Test and build output" }, { key: "command", title: "Other commands" }, { key: "agent", title: "Agent's own output" },
  { key: "user", title: "User answers" }, { key: "delegation", title: "Delegation briefs" }, { key: "report", title: "Subagent reports" }, { key: "other", title: "Other" },
] as const;
export const ToolKinds = [
  { key: "read", title: "Reads" }, { key: "search", title: "Searches" }, { key: "edit", title: "Edits" }, { key: "run", title: "Test and build runs" }, { key: "other", title: "Other" },
] as const;
/** The analysis method that splits tokens from events alone, a rougher estimate than the request logs give. */
export const RoughMethod = "events";

export interface ObjectiveMeasures {
  /** Runs with an analysis that has token totals. */
  readonly runs: number;
  /** Of those, runs whose split came from events alone. */
  readonly rough: number;
  /** Mean tokens per run: all input and output. */
  readonly tokens: number | null;
  readonly classes: Readonly<Record<string, number | null>>;
  readonly fileTypes: Readonly<Record<string, number>>;
  readonly calls: Readonly<Record<string, number | null>>;
  readonly stepsBeforeEdit: number | null;
  readonly failed: number | null;
  readonly peakContext: number | null;
}

const mean = (values: readonly (number | null | undefined)[]): number | null => {
  const known = values.filter((value): value is number => typeof value === "number" && Number.isFinite(value));
  return known.length === 0 ? null : known.reduce((sum, value) => sum + value, 0) / known.length;
};
const keysOf = (records: readonly Readonly<Record<string, unknown>>[]) => [...new Set(records.flatMap((record) => Object.keys(record)))];

/** Each run's newest analysis without an error. A later analyzer version is appended after the one it replaces. */
export function latestAnalyses(batch: Batch): Map<string, Analysis> {
  return new Map((batch.analyses ?? []).filter((item) => item.error === null).map((item) => [item.runId, item]));
}

const sum = (analysis: Analysis, pick: (part: Analysis["parts"][number]) => boolean) => analysis.parts.filter(pick).reduce((total, part) => total + part.input, 0);

/** The mean measures of a set of analyses. Process measures count every analysis, and the split only those that have one. */
export function measuresOf(analyses: readonly Analysis[]): ObjectiveMeasures {
  const counted = analyses.filter((item) => item.tokens !== null), split = counted.filter((item) => item.parts.length > 0);
  const types = [...new Set(split.flatMap((item) => item.parts.flatMap((part) => (part.fileType === null ? [] : [part.fileType]))))];
  return {
    runs: counted.length, rough: split.filter((item) => item.method === RoughMethod).length,
    tokens: mean(counted.map((item) => item.tokens === null ? null : item.tokens.newInput + item.tokens.cachedInput + item.tokens.cacheWrite + item.tokens.output)),
    classes: Object.fromEntries(ContextClasses.map(({ key }) => [key, mean(split.map((item) => sum(item, (part) => part.class === key)))])),
    fileTypes: Object.fromEntries(types.map((type) => [type, mean(split.map((item) => sum(item, (part) => part.fileType === type))) ?? 0])),
    calls: Object.fromEntries(ToolKinds.map(({ key }) => [key, mean(analyses.map((item) => item.process?.calls[key]))])),
    stepsBeforeEdit: mean(analyses.map((item) => item.process?.stepsBeforeEdit)), failed: mean(analyses.map((item) => item.process?.failed)),
    peakContext: mean(counted.map((item) => item.peakContext)),
  };
}

/** The means of several groups' means, so each case and model counts once whatever its number of runs. */
export function averaged(groups: readonly ObjectiveMeasures[]): ObjectiveMeasures {
  const field = <K extends "tokens" | "stepsBeforeEdit" | "failed" | "peakContext">(key: K) => mean(groups.map((group) => group[key]));
  const record = (pick: (group: ObjectiveMeasures) => Readonly<Record<string, number | null>>) =>
    Object.fromEntries(keysOf(groups.map(pick)).map((key) => [key, mean(groups.map((group) => pick(group)[key]))]));
  return { runs: groups.reduce((total, group) => total + group.runs, 0), rough: groups.reduce((total, group) => total + group.rough, 0),
    tokens: field("tokens"), classes: record((group) => group.classes), calls: record((group) => group.calls),
    fileTypes: Object.fromEntries(keysOf(groups.map((group) => group.fileTypes)).map((key) => [key, mean(groups.map((group) => group.fileTypes[key] ?? 0)) ?? 0])),
    stepsBeforeEdit: field("stepsBeforeEdit"), failed: field("failed"), peakContext: field("peakContext") };
}

export function cellMeasures(cell: Cell, batch: Batch | undefined): ObjectiveMeasures {
  const latest = batch === undefined ? new Map<string, Analysis>() : latestAnalyses(batch);
  return measuresOf(cell.runIds.flatMap((id) => latest.get(id) ?? []));
}

/** One setup's objective measures beside its control's, over the same cases and models. */
export interface ObjectiveRow extends Omit<ChartRow, "value"> {
  readonly measures: ObjectiveMeasures;
  /** Null for the control itself. */
  readonly control: ObjectiveMeasures | null;
}

/** Each setup's measures. Judge rows repeat a cell's runs, so each batch and model counts once. */
export function objectiveRows(items: readonly Comparison[], batches: ReadonlyMap<string, Batch>): ObjectiveRow[] {
  const setups = [...new Set(items.map((item) => item.cell.setup))];
  return setups.map((setup) => {
    const groups = [...new Map(items.filter((item) => item.cell.setup === setup).map((item) => [JSON.stringify([item.cell.batchId, item.cell.model]), item])).values()];
    const measures = averaged(groups.map((item) => cellMeasures(item.cell, batches.get(item.cell.batchId))));
    const controlled = groups.filter((item) => item.cell.setup !== item.cell.control && item.controlCell !== undefined);
    const control = controlled.length === 0 ? null : averaged(controlled.map((item) => item.controlCell === undefined ? measuresOf([]) : cellMeasures(item.controlCell, batches.get(item.cell.batchId))));
    return { ...identity(setup), measures, control };
  }).filter((row) => row.measures.runs > 0 || Object.values(row.measures.calls).some((value) => value !== null)).sort(bySetup);
}
