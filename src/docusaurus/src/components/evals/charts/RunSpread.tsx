import { useState } from "react";
import type { Cell } from "../data/cells.ts";
import { caseDefinition, caseLabel } from "../data/cell-labels.ts";
import { spreadRows } from "../data/chart-data.ts";
import { ChangeKind, CellMeasures } from "../data/measures.ts";
import { Term } from "../info/Term.tsx";
import { SpreadChart } from "./SpreadChart.tsx";

const ShareDomain = [0, 1] as const;
const AmountDomain = [0, "auto"] as const;

/** Every run of one model on one starting point, by case. An existing app has several cases, so a reader picks one. */
export function RunSpread({ cells }: { readonly cells: readonly Cell[] }) {
  const cases = [...new Map(cells.map((cell) => [cell.caseId, cell])).values()];
  const [chosen, choose] = useState<string | null>(null);
  const shown = cases.find((cell) => cell.caseId === chosen) ?? cases[0];
  if (shown === undefined) return null;
  const caseCells = cells.filter((cell) => cell.caseId === shown.caseId);
  return <div className="eval-spread">
    <h3>Every run</h3>
    <p>Each dot is one run, and the short line is the mean. Judge scores show one dot per run and judge. Time compares these setups fairly because they share one model.</p>
    {cases.length < 2 ? null : <div className="eval-segmented" role="group" aria-label="Case">
      {cases.map((cell) => <button key={cell.caseId} type="button" aria-pressed={cell.caseId === shown.caseId} onClick={() => choose(cell.caseId)}>{caseLabel(cell)}</button>)}
    </div>}
    <p className="eval-muted"><Term id={caseDefinition(shown)}>{caseLabel(shown)}</Term>, {shown.modelLabel}.</p>
    <div className="eval-charts">
      {CellMeasures.filter((spec) => spec.inSpread).map((spec) => <SpreadChart key={spec.id} rows={spreadRows(caseCells, spec)} format={spec.format}
        domain={spec.changeKind === ChangeKind.points ? ShareDomain : AmountDomain}
        title={<Term id={spec.definition}>{spec.title}</Term>}
        description={spec.perJudge ? "One dot per run and judge." : "One dot per run."} />)}
    </div>
  </div>;
}
