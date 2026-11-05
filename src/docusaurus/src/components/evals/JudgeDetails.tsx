import { useState } from "react";
import type { Judge, JudgeFinding } from "./records/records.types.ts";
import { dimensionScore, groupShare, headlineShare, JudgeHeadline, ScoreScale } from "./data/judge-scores.ts";
import { NoValue, percent } from "./data/format.ts";

const ConcernKind = "concern";
const GroupNames: Readonly<Record<string, string>> = { play: "How it plays", code: "The code", spec: "The spec", docs: "The documents" };
const HeadlineNames: Readonly<Record<JudgeHeadline, string>> = { [JudgeHeadline.quality]: "Quality", [JudgeHeadline.adherence]: "Adherence to the spec", [JudgeHeadline.upkeep]: "Documents kept current" };

function FindingList({ title, findings }: { readonly title: string; readonly findings: readonly JudgeFinding[] }) {
  if (findings.length === 0) return null;
  return <>
    <h4>{title}</h4>
    <ul>{findings.map((finding, index) => <li key={index}>
      <strong>{GroupNames[finding.area] ?? finding.area}{finding.impact === null ? "" : `, ${finding.impact}`}.</strong> {finding.text} <span className="eval-key">Ballot {finding.ballot}. {finding.citations.join(", ")}</span>
    </li>)}</ul>
  </>;
}

/** One judge's scores and free writing for a run. The dimensions keep every ballot, so a split between ballots shows. */
export function JudgeDetails({ record }: { readonly record: Judge }) {
  const scores = record.scores ?? [], findings = record.findings ?? [];
  const [open, setOpen] = useState<string | null>(null);
  if (scores.length === 0) return null;
  const share = (value: number | null) => value === null ? NoValue : percent(value);
  const groups = [...new Set(scores.map((score) => score.group))];
  return <>
    <dl className="eval-judge-headlines">
      {Object.values(JudgeHeadline).map((headline) => <div key={headline}><dt>{HeadlineNames[headline]}</dt><dd>{share(headlineShare(record, headline))}</dd></div>)}
      {groups.map((group) => <div key={group}><dt>{GroupNames[group] ?? group}</dt><dd>{share(groupShare(record, group))}</dd></div>)}
    </dl>
    {(record.summaries ?? []).length === 0 ? null : <>
      <h4>What each ballot concluded</h4>
      <ul>{record.summaries?.map((summary, index) => <li key={index}>{summary}</li>)}</ul>
    </>}
    {(record.staleDocs ?? []).length === 0 ? null : <>
      <h4>Pages that describe the game</h4>
      <ul>{record.staleDocs?.map((page) => <li key={page.document}><code>{page.document}</code>: {page.current ? "kept current" : page.remaining > 0 ? `${page.remaining} of ${page.statements} old statements remain` : "old statements gone, but the feature isn't mentioned"}.</li>)}</ul>
    </>}
    <FindingList title="Concerns" findings={findings.filter((finding) => finding.kind === ConcernKind)} />
    <FindingList title="Strengths" findings={findings.filter((finding) => finding.kind !== ConcernKind)} />
    <h4>Scores out of {ScoreScale}</h4>
    <div className="eval-scroll" tabIndex={0} role="region" aria-label="Judge scores">
      <table><thead><tr><th scope="col">Dimension</th><th scope="col">Part of</th><th scope="col">Middle ballot</th><th scope="col">Each ballot</th><th scope="col">Why</th></tr></thead>
        <tbody>{scores.map((score) => <tr key={score.id}>
          <th scope="row">{score.title}</th><td>{GroupNames[score.group] ?? score.group}</td><td>{dimensionScore(score) ?? NoValue}</td><td>{score.ballots.join(", ")}</td>
          <td className="eval-evidence">
            <button type="button" className="eval-link-button" aria-expanded={open === score.id} onClick={() => setOpen(open === score.id ? null : score.id)}>{open === score.id ? "Hide" : "Show"}</button>
            {open === score.id ? <ul>{score.evidence.map((item, index) => <li key={index}>{item}</li>)}</ul> : null}
          </td>
        </tr>)}</tbody>
      </table>
    </div>
  </>;
}
