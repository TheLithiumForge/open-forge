import { useMemo } from "react";
import type { Batch } from "./records/records.types.ts";
import type { Comparison } from "./data/aggregate.ts";
import { change, compact, percent, points } from "./data/format.ts";
import { milestoneRows, packetRows } from "./data/work-views.ts";
import { CaseNames, isCaseId, WorkModeNames } from "./data/names.ts";
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

/** Each scenario's work on a packet in every work mode, shown only when a packet ran. */
export function PacketSection({ items, batches }: { readonly items: readonly Comparison[]; readonly batches: ReadonlyMap<string, Batch> }) {
  const rows = useMemo(() => packetRows(items, batches), [items, batches]);
  if (rows.length === 0) return null;
  return <section className="eval-section" aria-labelledby="eval-packets">
    <h2 id="eval-packets">Work packets</h2>
    <p>A work packet is several tasks handed over together. One agent can take the whole packet, a new agent can take each task from the last one's result, or an orchestrator can hand each task to a subagent. Task by task adds up its tasks' <Term id="tokens">tokens</Term>, and an orchestrator's tokens are the whole system's.</p>
    <div className="eval-scroll" tabIndex={0} role="region" aria-label="Work packets">
      <table className="eval-table">
        <thead><tr>{["Packet", "Scenario", "Work mode", "Implementer", "Tasks", "Tokens", "Check coverage", "Subagents", "Orchestrator's share"].map((title) => <th key={title} scope="col">{title}</th>)}</tr></thead>
        <tbody>{rows.map((row) => <tr key={`${row.packet}-${row.setup}-${row.workMode}-${row.modelLabel}`}>
          <th scope="row">{isCaseId(row.packet) ? CaseNames[row.packet] : row.packet}</th><td><Term id={row.setup}>{row.name}</Term></td><td>{WorkModeNames[row.workMode]}</td><td>{row.modelLabel}</td>
          <td>{row.steps}</td><td>{compact(row.tokens)}</td><td>{percent(row.coverage)}</td>
          <td>{row.subagents === null ? compact(null) : row.subagents.toFixed(1)}</td><td>{percent(row.orchestratorShare)}</td>
        </tr>)}</tbody>
      </table>
    </div>
  </section>;
}
