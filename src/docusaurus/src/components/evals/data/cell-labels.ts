import { StartingPoints, type Cell, type StartingPointId } from "./cells.ts";
import { CaseNames, isCaseId, setupName, type CaseId } from "./names.ts";

export function startingPointTitle(id: StartingPointId): string {
  return StartingPoints.find((point) => point.id === id)?.title ?? id;
}

/** The definition to open for a cell's case: the case of an existing app, otherwise its starting point. */
export function caseDefinition(cell: Pick<Cell, "caseId" | "startingPoint">): CaseId | StartingPointId {
  return isCaseId(cell.caseId) ? cell.caseId : cell.startingPoint;
}

/** A cell's case in reader terms. Only an existing app has several cases. */
export function caseLabel(cell: Pick<Cell, "caseId" | "startingPoint">): string {
  return isCaseId(cell.caseId) ? CaseNames[cell.caseId] : startingPointTitle(cell.startingPoint);
}

/** Words a reader might search for in a cell's row. */
export function searchText(cell: Cell): string {
  return [startingPointTitle(cell.startingPoint), caseLabel(cell), cell.caseId, setupName(cell.setup), cell.setup, cell.modelLabel, cell.judgeLabel ?? "", cell.batchId].join(" ").toLowerCase();
}
