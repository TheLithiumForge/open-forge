import { useMemo } from "react";
import type { Batch } from "./records/records.types.ts";
import type { Comparison } from "./data/aggregate.ts";
import { change, compact, percent, points } from "./data/format.ts";
import { milestoneRows, orchestrationRows } from "./data/work-views.ts";
import { Term } from "./info/Term.tsx";

const ratio = (value: number | null, base: number | null) => value === null || base === null || base === 0 ? null : value / base;
const difference = (value: number | null, base: number | null) => value === null || base === null ? null : value - base;

/** Chained milestones beside the same task run alone, shown only when a chain exists. */
export function MilestoneSection({ items }: { readonly items: readonly Comparison[] }) {
  const rows = useMemo(() => milestoneRows(items), [items]);
  if (rows.length === 0) return null;
  return <section className="eval-section" aria-labelledby="eval-milestones">
    <h2 id="eval-milestones">Milestones</h2>
    <p>Each milestone of a chain starts from its own lineage's result, so earlier changes and records carry over. Its twin is the same task run on its own from the shared start, and the difference is the effect of what built up.</p>
    <div className="eval-scroll" tabIndex={0} role="region" aria-label="Milestones">
      <table className="eval-table">
        <thead><tr>{["Chain", "Milestone", "Task", "Setup", "Tokens", "Alone", "Token change", "Check coverage", "Coverage change"].map((title) => <th key={title} scope="col">{title}</th>)}</tr></thead>
        <tbody>{rows.map((row) => <tr key={`${row.chain}-${row.milestone}-${row.setup}`}>
          <th scope="row">{row.chain}</th><td>{row.milestone}</td><td>{row.caseId}</td><td><Term id={row.setup}>{row.name}</Term></td>
          <td>{compact(row.tokens)}</td><td>{compact(row.standaloneTokens)}</td><td>{change(ratio(row.tokens, row.standaloneTokens))}</td>
          <td>{percent(row.coverage)}</td><td>{points(difference(row.coverage, row.standaloneCoverage))}</td>
        </tr>)}</tbody>
      </table>
    </div>
  </section>;
}

/** Orchestrated setups against one agent on the same work, shown only when an orchestrated setup ran. */
export function OrchestrationSection({ items, batches }: { readonly items: readonly Comparison[]; readonly batches: ReadonlyMap<string, Batch> }) {
  const rows = useMemo(() => orchestrationRows(items, batches), [items, batches]);
  if (rows.length === 0) return null;
  return <section className="eval-section" aria-labelledby="eval-orchestration">
    <h2 id="eval-orchestration">One agent or an orchestrator</h2>
    <p>An orchestrator plans the work and hands each task to a subagent. Its <Term id="tokens">tokens</Term> are the whole system's: the orchestrator and every subagent. Each row sits beside the one-agent setup it was built from, in the same batch.</p>
    <div className="eval-scroll" tabIndex={0} role="region" aria-label="One agent or an orchestrator">
      <table className="eval-table">
        <thead><tr>{["Setup", "Task", "System tokens", "One agent", "Token change", "Orchestrator's share", "Subagents", "Check coverage", "Coverage change"].map((title) => <th key={title} scope="col">{title}</th>)}</tr></thead>
        <tbody>{rows.map((row) => <tr key={`${row.setup}-${row.caseId}`}>
          <th scope="row"><Term id={row.setup}>{row.name}</Term></th><td>{row.caseId}</td>
          <td>{compact(row.tokens)}</td><td>{compact(row.oneAgentTokens)}</td><td>{change(ratio(row.tokens, row.oneAgentTokens))}</td>
          <td>{percent(row.orchestratorShare)}</td><td>{row.subagents === null ? compact(null) : row.subagents.toFixed(1)}</td>
          <td>{percent(row.coverage)}</td><td>{points(difference(row.coverage, row.oneAgentCoverage))}</td>
        </tr>)}</tbody>
      </table>
    </div>
  </section>;
}
