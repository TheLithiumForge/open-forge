import type { Comparison } from "./aggregate.ts";
import { caseLabel, startingPointTitle } from "./cell-labels.ts";
import { CellMeasures, ChangeKind, csvChange, measureChange } from "./measures.ts";
import { setupName } from "./names.ts";

export type CsvValue = string | number | null;

const LineBreak = "\r\n";
/** Text a spreadsheet would run as a formula. Record text is untrusted, so it is written as plain text instead. */
const FormulaStart = /^[=+\-@\t\r]/;
const NeedsQuotes = /[",\r\n]/;

export function csvField(value: CsvValue): string {
  if (value === null) return "";
  if (typeof value === "number") return Number.isFinite(value) ? String(value) : "";
  const text = FormulaStart.test(value) ? `'${value}` : value;
  return NeedsQuotes.test(text) ? `"${text.replaceAll("\"", "\"\"")}"` : text;
}

/** RFC 4180 text with a header row. */
export function toCsv(header: readonly string[], rows: readonly (readonly CsvValue[])[]): string {
  return [header, ...rows].map((row) => row.map(csvField).join(",")).join(LineBreak) + LineBreak;
}

/** The shown table rows with raw numbers: shares in percent, changes in points or percent. */
export function resultsCsv(items: readonly Comparison[]): string {
  const header = ["Starting point", "Task", "Scenario", "Control", "Model", "Judge", "Batch", "CLI version", "Runs", "Checked runs", "Runs waiting on a recheck", "Judged runs",
    ...CellMeasures.flatMap((spec) => [spec.title, `${spec.title} change, ${spec.changeKind === ChangeKind.points ? "points" : "percent"}`])];
  const rows = items.map((item) => [
    startingPointTitle(item.cell.startingPoint), caseLabel(item.cell), setupName(item.cell.setup), setupName(item.cell.control), item.cell.modelLabel, item.cell.judgeLabel,
    item.cell.batchId, item.cell.cliVersion, item.cell.runs, item.cell.checked, item.cell.recheck, item.cell.judged,
    ...CellMeasures.flatMap((spec) => [spec.csv(spec.value(item.cell).mean), csvChange(spec, measureChange(spec, item))]),
  ]);
  return toCsv(header, rows);
}
