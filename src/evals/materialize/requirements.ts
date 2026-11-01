import { isRecord } from "./frontmatter.ts";
import { safeRelative } from "./files.ts";

export const SpecVersion = 2.2;
export const PreviousSpecVersion = 2.1;
export const CaseLayer = { two: "L2", three: "L3" } as const;
export interface ProductClause { readonly id: string; readonly text: string; readonly firstLayer: string }
export interface ProductRequirement { readonly id: string; readonly document: string; readonly heading: string; readonly clauses: readonly ProductClause[] }

export function readRequirements(value: unknown): ProductRequirement[] {
  if (!isRecord(value) || value["specVersion"] !== SpecVersion || !Array.isArray(value["requirements"])) throw new Error(`Expected the version ${SpecVersion} requirements map.`);
  const ids = new Set<string>();
  return value["requirements"].map((entry: unknown) => {
    if (!isRecord(entry) || typeof entry["id"] !== "string" || typeof entry["document"] !== "string" || typeof entry["heading"] !== "string" || !Array.isArray(entry["clauses"])) throw new Error("Invalid product requirement.");
    const id = entry["id"];
    const clauses = entry["clauses"].map((clause: unknown): ProductClause => {
      if (!isRecord(clause) || typeof clause["id"] !== "string" || !clause["id"].startsWith(`${id}.`) || typeof clause["text"] !== "string"
        || ![CaseLayer.two, CaseLayer.three].some((layer) => layer === clause["firstLayer"]) || typeof clause["firstLayer"] !== "string" || ids.has(clause["id"])) throw new Error("Invalid or repeated product clause.");
      ids.add(clause["id"]);
      return { id: clause["id"], text: clause["text"], firstLayer: clause["firstLayer"] };
    });
    return { id, document: safeRelative(entry["document"]), heading: entry["heading"], clauses };
  });
}
