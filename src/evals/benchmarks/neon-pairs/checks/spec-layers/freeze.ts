import { readFile, writeFile } from "node:fs/promises";
import { definition } from "./catalog.ts";
import { LayerId, loadRequirements } from "./requirements.ts";
import { rubric } from "./rubrics.ts";
import { fillTrace } from "./trace.ts";
import { NewCapClauses } from "./cap-policy.ts";

export const CapTraceFlag = "--caps";
export async function freeze(traceClauses?: readonly string[]): Promise<void> {
  const inventory = await loadRequirements();
  const slots = new Map<string, string[]>();
  for (const layer of Object.values(LayerId)) {
    const declared = await definition(layer, inventory), review = rubric(layer, inventory);
    await writeFile(new URL(`../../layers/${layer}.json`, import.meta.url), `${JSON.stringify(declared, null, 2)}\n`);
    await writeFile(new URL(`../../rubrics/${layer}.json`, import.meta.url), `${JSON.stringify(review, null, 2)}\n`);
    for (const check of declared.checks) for (const assertion of check.assertions) {
      const value = `${layer}:${check.id}#${assertion.id}`;
      slots.set(assertion.clause, [...(slots.get(assertion.clause) ?? []), value]);
    }
    for (const question of review.questions) for (const clause of question.clauses) slots.set(clause, [...(slots.get(clause) ?? []), `${layer}:rubric#${question.id}`]);
  }
  const path = new URL("../../trace.json", import.meta.url);
  const selected = traceClauses === undefined ? slots : new Map([...slots].filter(([clause]) => traceClauses.includes(clause)));
  await writeFile(path, fillTrace(await readFile(path, "utf8"), selected));
}
if (import.meta.main) {
  const [flag, extra] = process.argv.slice(2);
  if (extra || flag !== undefined && flag !== CapTraceFlag) throw new Error(`Use freeze.ts [${CapTraceFlag}].`);
  await freeze(flag === CapTraceFlag ? NewCapClauses : undefined);
}
