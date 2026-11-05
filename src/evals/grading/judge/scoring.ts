import type { JudgeFinding, JudgeScore } from "../../history/results.types.ts";
import { record } from "../../benchmarks/neon-pairs/checks/layer.ts";
import { boundCitations } from "./ballots.ts";
import type { JudgeEvidence } from "./evidence.ts";

export const ScoreHeadline = { quality: "quality", adherence: "adherence" } as const;
export type ScoreHeadline = (typeof ScoreHeadline)[keyof typeof ScoreHeadline];
export const FindingKind = { concern: "concern", strength: "strength" } as const;
export const FindingArea = { play: "play", code: "code", spec: "spec" } as const;
export const FindingImpact = { blocks: "blocks", annoys: "annoys", polish: "polish" } as const;
export const ScoreRange = { lowest: 0, highest: 10 } as const;
export const FindingLimits = { [FindingKind.concern]: 5, [FindingKind.strength]: 2 } as const;
const AnchorPoints = ["2", "5", "8"] as const;

export interface JudgeDimension {
  readonly id: string;
  readonly headline: ScoreHeadline;
  /** Dimensions of one group average together first, so a headline weighs its groups equally. */
  readonly group: string;
  readonly title: string;
  readonly question: string;
  readonly anchors: Readonly<Record<(typeof AnchorPoints)[number], string>>;
}
export interface JudgeScoring { readonly instructions: string; readonly findings: string; readonly dimensions: readonly JudgeDimension[] }
export interface BallotScore { readonly id: string; readonly score: number; readonly evidence: string; readonly citations: readonly string[] }
export interface BallotFinding { readonly kind: string; readonly area: string; readonly impact: string | null; readonly text: string; readonly citations: readonly string[] }
export interface BallotScoring { readonly scores: readonly BallotScore[]; readonly findings: readonly BallotFinding[]; readonly summary: string }
/** What a judgment holds beside its answers, every ballot's value kept so the spread stays visible. */
export interface JudgeScores { readonly scores: readonly JudgeScore[]; readonly findings: readonly JudgeFinding[]; readonly summaries: readonly string[] }

const filled = (value: unknown): value is string => typeof value === "string" && value.trim() !== "";
const oneOf = (values: Record<string, string>, value: unknown): value is string => typeof value === "string" && Object.values(values).includes(value);

export function parseScoring(value: unknown): JudgeScoring | null {
  if (value === undefined) return null;
  if (!record(value) || !filled(value["instructions"]) || !filled(value["findings"]) || !Array.isArray(value["dimensions"]) || value["dimensions"].length === 0) throw new Error("Invalid judge scoring.");
  const dimensions = value["dimensions"].map((item: unknown): JudgeDimension => {
    if (!record(item) || !filled(item["id"]) || !oneOf(ScoreHeadline, item["headline"]) || !filled(item["group"]) || !filled(item["title"]) || !filled(item["question"]) || !record(item["anchors"])) throw new Error("Invalid judge dimension.");
    const anchors = item["anchors"];
    if (AnchorPoints.some((point) => !filled(anchors[point]))) throw new Error("A judge dimension needs its anchors.");
    return { id: item["id"], headline: item["headline"] as ScoreHeadline, group: item["group"], title: item["title"], question: item["question"], anchors: anchors as JudgeDimension["anchors"] };
  });
  if (new Set(dimensions.map((dimension) => dimension.id)).size !== dimensions.length) throw new Error("Duplicate judge dimension.");
  return { instructions: value["instructions"] as string, findings: value["findings"] as string, dimensions };
}

export const scoringPrompt = (scoring: JudgeScoring): readonly string[] => [
  scoring.instructions,
  `Dimensions:\n${JSON.stringify(scoring.dimensions.map(({ id, title, question, anchors }) => ({ id, title, question, anchors })))}`,
  scoring.findings,
  `Beside "answers" the JSON also has "scores", "findings" and "summary". Include one scores entry per dimension, with id, score, evidence and citations, where score is a whole number from ${ScoreRange.lowest} to ${ScoreRange.highest}. Each findings entry has kind (${Object.values(FindingKind).join(" or ")}), area (${Object.values(FindingArea).join(", ")}), impact (${Object.values(FindingImpact).join(", ")}, and null for a strength), text and citations. summary is a string.`,
];

function parseFinding(item: unknown, evidence: JudgeEvidence): BallotFinding {
  if (!record(item) || !oneOf(FindingKind, item["kind"]) || !oneOf(FindingArea, item["area"]) || !filled(item["text"])) throw new Error("Judge returned an invalid finding.");
  const concern = item["kind"] === FindingKind.concern;
  if (concern && !oneOf(FindingImpact, item["impact"])) throw new Error("A concern needs its impact.");
  return { kind: item["kind"], area: item["area"], impact: concern ? item["impact"] as string : null, text: item["text"], citations: boundCitations(item["citations"], "a finding", evidence) };
}

export function parseBallotScoring(value: unknown, scoring: JudgeScoring, evidence: JudgeEvidence): BallotScoring {
  if (!record(value) || !Array.isArray(value["scores"]) || !Array.isArray(value["findings"]) || !filled(value["summary"]) || value["scores"].length !== scoring.dimensions.length) throw new Error("Judge returned incomplete scores.");
  const seen = new Set<string>();
  const scores = value["scores"].map((item: unknown): BallotScore => {
    if (!record(item) || !filled(item["id"]) || !scoring.dimensions.some((dimension) => dimension.id === item["id"]) || seen.has(item["id"]) || !filled(item["evidence"])) throw new Error("Judge returned an invalid score.");
    const score = item["score"];
    if (typeof score !== "number" || !Number.isInteger(score) || score < ScoreRange.lowest || score > ScoreRange.highest) throw new Error(`Judge gave ${item["id"]} a score outside ${ScoreRange.lowest} to ${ScoreRange.highest}.`);
    seen.add(item["id"]);
    return { id: item["id"], score, evidence: item["evidence"], citations: boundCitations(item["citations"], item["id"], evidence) };
  });
  const findings = value["findings"].map((item: unknown) => parseFinding(item, evidence));
  for (const kind of Object.values(FindingKind)) if (findings.filter((finding) => finding.kind === kind).length > FindingLimits[kind]) throw new Error(`Judge gave more than ${FindingLimits[kind]} of ${kind}.`);
  return { scores, findings, summary: value["summary"] as string };
}

export function tallyScoring(scoring: JudgeScoring, ballots: readonly BallotScoring[]): JudgeScores {
  return {
    scores: scoring.dimensions.map(({ id, headline, group, title }) => {
      const votes = ballots.map((ballot) => ballot.scores.find((score) => score.id === id));
      if (votes.some((vote) => vote === undefined)) throw new Error("Judge ballot omitted a score.");
      return { id, headline, group, title, ballots: votes.flatMap((vote) => vote ? [vote.score] : []), evidence: votes.flatMap((vote) => vote ? [`${vote.evidence} [${vote.citations.join(", ")}]`] : []) };
    }),
    findings: ballots.flatMap((ballot, index) => ballot.findings.map((finding) => ({ ballot: index + 1, ...finding }))),
    summaries: ballots.map((ballot) => ballot.summary),
  };
}
