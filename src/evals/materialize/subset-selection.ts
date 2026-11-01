import { isRecord } from "./frontmatter.ts";
import { sha256, type FileBytes } from "./files.ts";

export const SubsetFlag = { acceptSelectionChange: "--accept-selection-change" } as const;
export interface SelectedClause { readonly id: string; readonly text: string }
export interface SelectionStamp { readonly sha256: string; readonly count: number; readonly sourceHash: string }
export interface SelectionChange {
  readonly from: SelectionStamp;
  readonly to: SelectionStamp;
  readonly added: readonly string[];
  readonly removed: readonly string[];
  readonly updated: readonly string[];
  readonly reason: string;
}
export interface SubsetSelection {
  readonly sourceHash: string;
  readonly clauses: readonly SelectedClause[];
  readonly changes: readonly SelectionChange[];
}

export function selectedClauses(documents: readonly FileBytes[]): SelectedClause[] {
  const clauses = documents.flatMap((file) => [...file.bytes.toString().matchAll(/^- ([A-Z]\d{2}\.\d+) - (.+)\r?$/gm)].map((match) => {
    const [, id, text] = match;
    if (!id || !text) throw new Error("Invalid player clause.");
    return { id, text };
  }));
  if (!clauses.length || new Set(clauses.map((clause) => clause.id)).size !== clauses.length) throw new Error("Player clauses must be nonempty and unique.");
  return clauses;
}

export function selectionStamp(sourceHash: string, clauses: readonly SelectedClause[]): SelectionStamp {
  return { sha256: sha256(JSON.stringify(clauses)), count: clauses.length, sourceHash };
}

export function selectionChanges(previous: SubsetSelection, sourceHash: string, clauses: readonly SelectedClause[], reason?: string): readonly SelectionChange[] {
  if (reason !== undefined && !reason.trim()) throw new Error("A selection change needs a reason.");
  const from = selectionStamp(previous.sourceHash, previous.clauses);
  const to = selectionStamp(sourceHash, clauses);
  if (from.sha256 === to.sha256) return previous.changes;
  if (reason === undefined) throw new Error(`The frozen Layer 2 selection changed. Use ${SubsetFlag.acceptSelectionChange} <reason> to record its acceptance.`);
  const before = new Map(previous.clauses.map((clause) => [clause.id, clause.text]));
  const after = new Map(clauses.map((clause) => [clause.id, clause.text]));
  return [...previous.changes, { from, to, reason: reason.trim(),
    added: clauses.filter((clause) => !before.has(clause.id)).map((clause) => clause.id),
    removed: previous.clauses.filter((clause) => !after.has(clause.id)).map((clause) => clause.id),
    updated: clauses.filter((clause) => before.has(clause.id) && before.get(clause.id) !== clause.text).map((clause) => clause.id) }];
}

function readStamp(value: unknown): SelectionStamp {
  if (!isRecord(value) || typeof value["sha256"] !== "string" || !/^[a-f\d]{64}$/.test(value["sha256"])
    || typeof value["sourceHash"] !== "string" || !/^[a-f\d]{64}$/.test(value["sourceHash"])
    || typeof value["count"] !== "number" || !Number.isSafeInteger(value["count"]) || value["count"] < 1) throw new Error("Invalid selection change fingerprint.");
  return { sha256: value["sha256"], count: value["count"], sourceHash: value["sourceHash"] };
}

function readIds(value: unknown): string[] {
  if (!Array.isArray(value) || !value.every((id: unknown): id is string => typeof id === "string" && /^[A-Z]\d{2}\.\d+$/.test(id)) || new Set(value).size !== value.length) throw new Error("Invalid selection change clause IDs.");
  return value;
}

export function readSelectionChanges(value: unknown): SelectionChange[] {
  if (value === undefined) return [];
  if (!Array.isArray(value)) throw new Error("Invalid selection change history.");
  return value.map((change: unknown) => {
    if (!isRecord(change) || typeof change["reason"] !== "string" || !change["reason"].trim()) throw new Error("Invalid selection change reason.");
    return { from: readStamp(change["from"]), to: readStamp(change["to"]), reason: change["reason"],
      added: readIds(change["added"]), removed: readIds(change["removed"]), updated: readIds(change["updated"]) };
  });
}
