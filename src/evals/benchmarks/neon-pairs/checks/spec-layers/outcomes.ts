import { CheckStatus } from "../../../../history/statuses.ts";
import type { CheckOutcome } from "../../../../grading/grading.types.ts";
import type { Declaration, Definition } from "./catalog.ts";

export class BehaviorFailure extends Error {}
export const EvidenceLimit = 24_000;
export interface StoppedClause { readonly status: "error" | "blocked" | "fail"; readonly detail: string }
/**
 * What a journey that stopped early means for its clauses. A game that didn't behave fails the first clause still open, and the
 * rest wait on it, as they do after an earlier failure. Anything else is a checker fault, and only that is an error.
 */
export function stoppedClauses(clauses: readonly string[], seen: ReadonlySet<string>, error: unknown, earlier: string | undefined, detail: string): Map<string, StoppedClause> {
  const failing = earlier === undefined && error instanceof BehaviorFailure ? clauses.find((clause) => !seen.has(clause)) : undefined;
  const after = earlier ?? failing;
  return new Map(clauses.map((clause) => [clause, clause === failing ? { status: "fail", detail } : after !== undefined ? { status: "blocked", detail: `Stopped after ${after}. ${detail}` } : { status: "error", detail }]));
}
function bounded(text: string): string {
  if (text.length <= EvidenceLimit) return text;
  const omitted = "\n[Evidence shortened. First and last samples kept.]\n", half = Math.floor((EvidenceLimit - omitted.length) / 2);
  return `${text.slice(0, half)}${omitted}${text.slice(-half)}`;
}
export interface AssertionOutcome extends CheckOutcome { readonly clause: string }
export class Evidence {
  readonly outcomes: AssertionOutcome[] = [];
  readonly inherited = new Map<string, CheckOutcome>();
  readonly seen = new Set<string>();
  observation: (() => Promise<unknown>) | undefined;
  readonly pending: Promise<void>[] = [];
  readonly captured = new Set<string>();
  test(clause: string, passed: boolean, evidence: string, observed?: unknown): void {
    this.record(clause, passed ? CheckStatus.pass : CheckStatus.fail, evidence, observed);
  }
  record(clause: string, status: CheckOutcome["status"], evidence: string, observed?: unknown): void {
    if (this.seen.has(clause)) throw new Error(`Duplicate assertion ${clause}.`);
    this.seen.add(clause);
    this.outcomes.push({ id: `clause-${clause}`, clause, area: "requirements", status, evidence: bounded(observed === undefined ? evidence : `${evidence}\nObserved: ${JSON.stringify(observed)}`) });
    this.capture(clause, status);
  }
  merge(clause: string, status: CheckOutcome["status"], evidence: string, observed?: unknown): void {
    const index = this.outcomes.findIndex((item) => item.clause === clause), previous = this.outcomes[index];
    if (!previous) { this.record(clause, status, evidence, observed); return; }
    const order = [CheckStatus.pass, CheckStatus.blocked, CheckStatus.error, CheckStatus.fail];
    const combined = order.indexOf(status) > order.indexOf(previous.status) ? status : previous.status;
    this.outcomes[index] = { ...previous, status: combined, evidence: bounded(`${previous.evidence}\n${evidence}${observed === undefined ? "" : `\nObserved: ${JSON.stringify(observed)}`}`) };
    this.capture(clause, status);
  }
  private capture(clause: string, status: CheckOutcome["status"]): void {
    if (!this.observation || this.captured.has(clause) || status === CheckStatus.pass || status === CheckStatus.blocked) return;
    this.captured.add(clause);
    const append = (observed: unknown): void => {
      const index = this.outcomes.findIndex((item) => item.clause === clause), previous = this.outcomes[index];
      if (previous) this.outcomes[index] = { ...previous, evidence: bounded(`${previous.evidence}\nObserved page: ${JSON.stringify(observed)}`) };
    };
    this.pending.push(this.observation().then(append, (error: unknown) => append({ captureError: error instanceof Error ? error.message : String(error) })));
  }
  async flush(): Promise<void> {
    await Promise.all(this.pending.splice(0));
  }
  async journey(clauses: readonly string[], action: () => Promise<void>): Promise<void> {
    let failure: unknown;
    try { await action(); } catch (error) { failure = error; }
    const stopped = this.outcomes.find((item) => clauses.includes(item.clause) && item.status !== CheckStatus.pass);
    for (const clause of clauses.filter((id) => !this.seen.has(id))) {
      this.record(clause, stopped ? CheckStatus.blocked : CheckStatus.error,
        stopped ? `Stopped after ${stopped.clause}: ${stopped.evidence}` : `Expected the ${clause} probe to finish. Observed: ${failure instanceof Error ? failure.message : failure === undefined ? "no assertion collected" : String(failure)}`);
    }
  }
}

export function aggregate(check: Declaration, outcomes: readonly AssertionOutcome[]): CheckOutcome {
  const results = check.assertions.map((assertion) => outcomes.find((outcome) => outcome.id === assertion.id));
  const status = results.some((item) => item?.status === CheckStatus.fail) ? CheckStatus.fail
    : results.some((item) => !item || item.status === CheckStatus.error) ? CheckStatus.error
      : results.some((item) => item?.status === CheckStatus.blocked) ? CheckStatus.blocked : CheckStatus.pass;
  return { id: check.id, area: check.area, status, evidence: results.map((item, index) => item ? `${item.clause}: ${item.status}. ${item.evidence}` : `Expected ${check.assertions[index]?.text}. Observed missing assertion ${check.assertions[index]?.clause}.`).join("\n") };
}

export function complete(definition: Definition, assertions: readonly AssertionOutcome[], inherited: readonly CheckOutcome[]): CheckOutcome[] {
  return definition.checks.map((check) => check.inheritedFrom ? inherited.find((item) => item.id === check.id)
    ?? { id: check.id, area: check.area, status: CheckStatus.error, evidence: "Inherited checker returned no result." } : aggregate(check, assertions));
}
