import { object } from "./requirements.ts";
import type { Definition } from "./catalog.ts";
import type { AssertionOutcome } from "./outcomes.ts";
import { parseCheckReport } from "../report.ts";
import { loadLayer } from "../layer.ts";

export async function parseReport(value: unknown, definition: Definition) {
  if (!object(value) || value["schemaVersion"] !== 1 || value["layer"] !== definition.layer || !Array.isArray(value["assertions"])) throw new Error("Invalid spec-layer report.");
  const allowed = new Map(definition.checks.flatMap((check) => check.assertions.map((assertion) => [assertion.clause, assertion.id])));
  const seen = new Set<string>(), assertions: AssertionOutcome[] = [];
  for (const item of value["assertions"]) {
    if (!object(item) || typeof item["clause"] !== "string" || typeof item["id"] !== "string" || typeof item["evidence"] !== "string" || item["evidence"].length > 64_000 || !["pass", "fail", "error", "blocked"].includes(String(item["status"])) || seen.has(item["id"])) throw new Error("Invalid assertion outcome.");
    seen.add(item["id"]);
    if (item["clause"] === "checker" && item["status"] === "error") continue;
    if (allowed.get(item["clause"]) !== item["id"]) throw new Error("Undeclared assertion in verifier report.");
    const status = item["status"];
    if (status !== "pass" && status !== "fail" && status !== "error" && status !== "blocked") throw new Error("Invalid status.");
    assertions.push({ id: item["id"], clause: item["clause"], area: "requirements", status, evidence: item["evidence"] });
  }
  return { assertions, inherited: parseCheckReport(value["inherited"], await loadLayer()) };
}
