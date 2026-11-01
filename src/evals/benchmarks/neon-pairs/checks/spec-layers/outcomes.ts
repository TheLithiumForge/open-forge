import { CheckStatus } from "../../../../history/statuses.ts";
import type { CheckOutcome } from "../../../../grading/grading.types.ts";
import type { Declaration, Definition } from "./catalog.ts";

export class BehaviorFailure extends Error {}
export interface AssertionOutcome extends CheckOutcome { readonly clause: string }
export class Evidence {
  readonly outcomes: AssertionOutcome[] = [];
  readonly seen = new Set<string>();
  test(clause: string, passed: boolean, evidence: string): void {
    this.record(clause, passed ? CheckStatus.pass : CheckStatus.fail, evidence);
  }
  record(clause: string, status: CheckOutcome["status"], evidence: string): void {
    if (this.seen.has(clause)) throw new Error(`Duplicate assertion ${clause}.`);
    this.seen.add(clause);
    this.outcomes.push({ id: `clause-${clause}`, clause, area: "requirements", status, evidence });
  }
  merge(clause: string, status: CheckOutcome["status"], evidence: string): void {
    const index = this.outcomes.findIndex((item) => item.clause === clause), previous = this.outcomes[index];
    if (!previous) { this.record(clause, status, evidence); return; }
    const order = [CheckStatus.pass, CheckStatus.blocked, CheckStatus.error, CheckStatus.fail];
    const combined = order.indexOf(status) > order.indexOf(previous.status) ? status : previous.status;
    this.outcomes[index] = { ...previous, status: combined, evidence: `${previous.evidence}\n${evidence}` };
  }
  async journey(clauses: readonly string[], action: () => Promise<void>): Promise<void> {
    let failure: unknown;
    try { await action(); } catch (error) { failure = error; }
    const stopped = this.outcomes.find((item) => clauses.includes(item.clause) && item.status !== CheckStatus.pass);
    for (const clause of clauses.filter((id) => !this.seen.has(id))) {
      this.record(clause, stopped ? CheckStatus.blocked : CheckStatus.error,
        stopped ? `Stopped after ${stopped.clause}: ${stopped.evidence}` : failure instanceof Error ? failure.message : "The checker did not collect this assertion.");
    }
  }
}

export function aggregate(check: Declaration, outcomes: readonly AssertionOutcome[]): CheckOutcome {
  const results = check.assertions.map((assertion) => outcomes.find((outcome) => outcome.id === assertion.id));
  const status = results.some((item) => item?.status === CheckStatus.fail) ? CheckStatus.fail
    : results.some((item) => !item || item.status === CheckStatus.error) ? CheckStatus.error
      : results.some((item) => item?.status === CheckStatus.blocked) ? CheckStatus.blocked : CheckStatus.pass;
  return { id: check.id, area: check.area, status, evidence: results.map((item, index) => item ? `${item.clause}: ${item.status}. ${item.evidence}` : `${check.assertions[index]?.clause}: missing assertion`).join("\n") };
}

export function complete(definition: Definition, assertions: readonly AssertionOutcome[], inherited: readonly CheckOutcome[]): CheckOutcome[] {
  return definition.checks.map((check) => check.inheritedFrom ? inherited.find((item) => item.id === check.id)
    ?? { id: check.id, area: check.area, status: CheckStatus.error, evidence: "Inherited checker returned no result." } : aggregate(check, assertions));
}
