import { useMemo } from "react";
import type { Batch } from "./records/records.types.ts";
import type { Comparison } from "./data/aggregate.ts";
import { change, compact, dollars, NoValue, percent, points } from "./data/format.ts";
import { milestoneRows, packetRows } from "./data/work-views.ts";
import { CaseNames, isCaseId, WorkModeNames } from "./data/names.ts";
import { Term } from "./info/Term.tsx";

const ratio = (value: number | null, base: number | null) => value === null || base === null || base === 0 ? null : value / base;
const difference = (value: number | null, base: number | null) => value === null || base === null ? null : value - base;

/** Each task of a task-by-task packet beside the same task run alone, shown only when such a packet ran. */
export function MilestoneSection({ items }: { readonly items: readonly Comparison[] }) {
  const rows = useMemo(() => milestoneRows(items), [items]);
  if (rows.length === 0) return null;
  return <section className="eval-section" aria-labelledby="eval-milestones">
    <h2 id="eval-milestones">Task by task</h2>
    <p>In task-by-task work, each task after the first starts from the previous task's result, so earlier changes and records carry over. Its twin is the same task run alone from a clean start, and the difference is the effect of what built up.</p>
    <div className="eval-scroll" tabIndex={0} role="region" aria-label="Task by task">
      <table className="eval-table">
        <thead><tr>{["Packet", "Step", "Task", "Scenario", "Tokens", "Alone", "Token change", "Check coverage", "Coverage change"].map((title) => <th key={title} scope="col">{title}</th>)}</tr></thead>
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
    <p>A work packet is several tasks handed over together. One agent can take the whole packet, a new agent can take each task from the last one's result, or an orchestrator can hand each task to a subagent. Task by task adds up its tasks' <Term id="tokens">tokens</Term> and <Term id="cost">cost</Term>, and an orchestrator's are the whole system's. Cost per task divides the packet's cost by its tasks, so packets of different sizes compare.</p>
    <div className="eval-scroll" tabIndex={0} role="region" aria-label="Work packets">
      <table className="eval-table">
        <thead><tr>{["Packet", "Scenario", "Work mode", "Implementer", "Tasks", "Tokens", "Cost", "Cost per task", "Check coverage", "Subagents", "Orchestrator's input", "Orchestrator's cost"].map((title) => <th key={title} scope="col">{title}</th>)}</tr></thead>
        <tbody>{rows.map((row) => <tr key={`${row.packet}-${row.setup}-${row.workMode}-${row.modelLabel}`}>
          <th scope="row">{isCaseId(row.packet) ? CaseNames[row.packet] : row.packet}</th><td><Term id={row.setup}>{row.name}</Term></td><td>{WorkModeNames[row.workMode]}</td><td>{row.modelLabel}</td>
          <td>{row.tasks}</td><td>{compact(row.tokens)}</td><td>{dollars(row.cost)}</td><td>{dollars(row.costPerTask)}</td><td>{percent(row.coverage)}</td>
          <td>{row.subagents === null ? compact(null) : row.subagents.toFixed(1)}</td><td>{row.orchestratorShare === null ? NoValue : percent(row.orchestratorShare)}</td><td>{row.orchestratorCostShare === null ? NoValue : percent(row.orchestratorCostShare)}</td>
        </tr>)}</tbody>
      </table>
    </div>
  </section>;
}
