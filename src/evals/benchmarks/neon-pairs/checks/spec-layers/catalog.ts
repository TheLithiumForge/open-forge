import { loadLayer } from "../layer.ts";
import { LayerId, Method, supplied, type Clause } from "./requirements.ts";
import { Limitations } from "./limitations.ts";
import { reviewReason } from "./review-policy.ts";

export const Versions = { L2: "L2-v3", L3: "L3-v3" } as const;
export const ExpectedCounts = { L2: { checks: 48, assertions: 170, review: 17 }, L3: { checks: 100, assertions: 345, review: 56 } } as const;
export const BrowserGroups = {
  sizes: ["R01"], selection: ["R02", "R03"], mismatch: ["R04"], moves: ["R05"], completion: ["R06"],
  clock: ["R07"], preferences: ["R08"], reset: ["R09"], best: ["R10"], storage: ["R11"], stale: ["R12"],
  controls: ["C01", "C02", "C03"], focus: ["C13", "I11", "I12"], activation: ["I01", "I02", "I03", "I04"],
  navigation: ["I05", "I06", "I07", "I08"], matched: ["I09", "I10"], scroll: ["I13"], names: ["I14", "I15"],
  announcements: ["I16"], native: ["V01"], layout: ["V02", "V03", "V04", "V05"], colors: ["V06", "V07"],
  typography: ["V08", "V09"], faces: ["V10", "V13", "V14", "V15"], outline: ["V16"], motion: ["V17", "V18"],
  large: ["V19"], zoom: ["V20"], silence: ["S01"], offline: ["T08", "T09", "T10"],
} as const;
export interface Assertion { readonly id: string; readonly clause: string; readonly method: Method; readonly text: string }
export interface Declaration { readonly id: string; readonly area: string; readonly method: Method; readonly clauses: readonly string[]; readonly assertions: readonly Assertion[]; readonly inheritedFrom?: string }
export interface Definition {
  readonly schemaVersion: number; readonly specVersion: number; readonly version: string; readonly layer: LayerId;
  readonly checks: readonly Declaration[]; readonly stated: readonly string[]; readonly working: readonly string[];
  readonly wishes: readonly string[]; readonly review: readonly { readonly clause: string; readonly reason: string }[];
  readonly clausePassRule: "all"; readonly uncheckable: readonly { readonly clause: string; readonly reason: string }[];
}

export function assertionId(clause: string): string { return `clause-${clause}`; }

function declaration(id: string, method: Method, clauses: readonly Clause[]): Declaration {
  return { id, area: "requirements", method, clauses: clauses.map((clause) => clause.id),
    assertions: clauses.map((clause) => ({ id: assertionId(clause.id), clause: clause.id, method: clause.method, text: clause.text })) };
}

export async function definition(layer: LayerId, inventory: readonly Clause[]): Promise<Definition> {
  const l1 = await loadLayer();
  const clauses = supplied(inventory, layer);
  const executable = clauses.filter((clause) => reviewReason(clause, layer) === undefined);
  const checks: Declaration[] = l1.checks.filter((check) => l1.stated.includes(check.id)).map((check) =>
    ({ id: check.id, area: check.area, method: Method.browser, clauses: [], assertions: [], inheritedFrom: l1.version }));
  for (const [group, requirements] of Object.entries(BrowserGroups)) {
    const selected = executable.filter((clause) => clause.firstLayer === LayerId.two && clause.method === Method.browser && requirements.some((id) => id === clause.requirement));
    checks.push(declaration(`l2-${group}`, Method.browser, selected));
  }
  for (const method of [Method.static, Method.build]) checks.push(declaration(`l2-${method}`, method, executable.filter((clause) => clause.firstLayer === LayerId.two && clause.method === method)));
  if (layer === LayerId.three) {
    const groups = new Map<string, Clause[]>();
    for (const clause of executable.filter((item) => item.firstLayer === LayerId.three)) {
      const key = `l3-${clause.requirement.toLowerCase()}-${clause.method}`;
      groups.set(key, [...(groups.get(key) ?? []), clause]);
    }
    for (const [id, selected] of groups) checks.push(declaration(id, selected[0]!.method, selected));
  }
  return { schemaVersion: 1, specVersion: 2, version: Versions[layer], layer, checks, stated: checks.map((check) => check.id),
    working: l1.working, wishes: [], clausePassRule: "all", uncheckable: clauses.filter((clause) => Limitations[clause.id]).map((clause) => ({ clause: clause.id, reason: Limitations[clause.id] ?? "" })),
    review: clauses.flatMap((clause) => { const reason = reviewReason(clause, layer); return reason ? [{ clause: clause.id, reason }] : []; }) };
}
