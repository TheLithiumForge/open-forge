import { readFile } from "node:fs/promises";

export const LayerId = { two: "L2", three: "L3" } as const;
export type LayerId = (typeof LayerId)[keyof typeof LayerId];
export const Method = { browser: "browser", replay: "replay", static: "static", build: "build", review: "review" } as const;
export type Method = (typeof Method)[keyof typeof Method];
export const SpecVersion = 2.1;
export const RequirementsFile = new URL("../../requirements.json", import.meta.url);
export interface Clause { readonly id: string; readonly requirement: string; readonly text: string; readonly firstLayer: LayerId; readonly method: Method }

export function object(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}

export function layerId(value: unknown): LayerId {
  if (value !== LayerId.two && value !== LayerId.three) throw new Error("Expected layer L2 or L3.");
  return value;
}

export function method(value: unknown): Method {
  switch (value) {
    case Method.browser: case Method.replay: case Method.static: case Method.build: case Method.review: return value;
    default: throw new Error("Unknown clause method.");
  }
}

export function parseRequirements(value: unknown): Clause[] {
  if (!object(value) || value["specVersion"] !== SpecVersion || !Array.isArray(value["requirements"])) throw new Error(`Expected Neon Pairs requirements for spec v${SpecVersion}.`);
  const clauses: Clause[] = [];
  const seen = new Set<string>();
  for (const requirement of value["requirements"]) {
    if (!object(requirement) || typeof requirement["id"] !== "string" || !Array.isArray(requirement["clauses"])) throw new Error("Invalid requirement record.");
    for (const clause of requirement["clauses"]) {
      if (!object(clause) || typeof clause["id"] !== "string" || typeof clause["text"] !== "string" || seen.has(clause["id"]) || !clause["id"].startsWith(`${requirement["id"]}.`)) throw new Error("Invalid or duplicate requirement clause.");
      seen.add(clause["id"]);
      clauses.push({ id: clause["id"], requirement: requirement["id"], text: clause["text"], firstLayer: layerId(clause["firstLayer"]), method: method(clause["method"]) });
    }
  }
  return clauses;
}

export async function loadRequirements(): Promise<Clause[]> {
  return parseRequirements(JSON.parse(await readFile(RequirementsFile, "utf8")));
}

export function supplied(clauses: readonly Clause[], layer: LayerId): Clause[] {
  return clauses.filter((clause) => layer === LayerId.three || clause.firstLayer === LayerId.two);
}
