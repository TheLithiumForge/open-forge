import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { test } from "node:test";
import { DocumentScaleCase } from "../../materialize/brownfield-case.ts";
import { caseRubric, judgeScoring } from "./judge.ts";
import type { JudgeEvidence } from "./evidence.ts";
import { FindingArea, FindingImpact, FindingKind, FindingLimits, parseBallotScoring, parseScoring, ScoreHeadline, ScoreRange, scoringPrompt, tallyScoring, type JudgeScoring } from "./scoring.ts";

const SourcePath = "game.ts", Citation = `source:${SourcePath}:1`, Evidence = "Fixture evidence.", Text = "Fixture text.";
const evidence: JudgeEvidence = { sources: [{ path: SourcePath, text: "x\n", lines: 2 }], images: [] };
const scoring = async (): Promise<JudgeScoring> => {
  const value = await judgeScoring({ id: DocumentScaleCase });
  assert.ok(value !== null);
  return value;
};
const scored = (rubric: JudgeScoring, score: number) => rubric.dimensions.map(({ id }) => ({ id, score, evidence: Evidence, citations: [Citation] }));
const concern = { kind: FindingKind.concern, area: FindingArea.play, impact: FindingImpact.annoys, text: Text, citations: [Citation] };
const strength = { kind: FindingKind.strength, area: FindingArea.code, impact: null, text: Text, citations: [Citation] };

test("the case's rubric scores both headlines, each dimension with its anchors, and asks for them in the prompt", async () => {
  const rubric = await scoring();
  for (const headline of Object.values(ScoreHeadline)) assert.ok(rubric.dimensions.some((dimension) => dimension.headline === headline));
  const prompt = scoringPrompt(rubric).join("\n");
  for (const { id, anchors } of rubric.dimensions) for (const anchor of [id, ...Object.values(anchors)]) assert.ok(prompt.includes(anchor));
});

test("a rubric without scoring has none, and a broken one is refused", async () => {
  assert.equal(parseScoring(undefined), null);
  const { scoring: valid } = JSON.parse(await readFile(caseRubric(DocumentScaleCase), "utf8")) as { scoring: JudgeScoring };
  assert.deepEqual(parseScoring(valid)?.dimensions.length, valid.dimensions.length);
  for (const broken of [{ ...valid, dimensions: [] }, { ...valid, instructions: "" }, { ...valid, dimensions: [valid.dimensions[0], valid.dimensions[0]] }, { ...valid, dimensions: [{ ...valid.dimensions[0], anchors: {} }] }]) assert.throws(() => parseScoring(broken));
});

test("a ballot's scores must cover every dimension once, as whole numbers in range with citations", async () => {
  const rubric = await scoring(), scores = scored(rubric, ScoreRange.highest);
  const ballot = { scores, findings: [concern, strength], summary: Text };
  assert.equal(parseBallotScoring(ballot, rubric, evidence).scores.length, rubric.dimensions.length);
  const [first, ...rest] = scores;
  assert.ok(first !== undefined);
  for (const changes of [{ score: ScoreRange.highest + 1 }, { score: ScoreRange.lowest - 1 }, { score: 1.5 }, { score: "8" }, { citations: [] }, { citations: ["source:other.ts:1"] }, { evidence: "" }]) assert.throws(() => parseBallotScoring({ ...ballot, scores: [{ ...first, ...changes }, ...rest] }, rubric, evidence));
  assert.throws(() => parseBallotScoring({ ...ballot, scores: rest }, rubric, evidence));
  assert.throws(() => parseBallotScoring({ ...ballot, scores: [first, first, ...rest.slice(1)] }, rubric, evidence));
  assert.throws(() => parseBallotScoring({ ...ballot, summary: "" }, rubric, evidence));
});

test("findings are optional, bounded and cited, and a strength has no impact", async () => {
  const rubric = await scoring(), scores = scored(rubric, ScoreRange.lowest);
  assert.deepEqual(parseBallotScoring({ scores, findings: [], summary: Text }, rubric, evidence).findings, []);
  const read = parseBallotScoring({ scores, findings: [{ ...strength, impact: FindingImpact.blocks }], summary: Text }, rubric, evidence);
  assert.equal(read.findings[0]?.impact, null);
  for (const finding of [{ ...concern, impact: null }, { ...concern, kind: "note" }, { ...concern, area: "other" }, { ...concern, citations: [] }, { ...concern, text: "" }]) assert.throws(() => parseBallotScoring({ scores, findings: [finding], summary: Text }, rubric, evidence));
  assert.throws(() => parseBallotScoring({ scores, findings: Array.from({ length: FindingLimits[FindingKind.concern] + 1 }, () => concern), summary: Text }, rubric, evidence));
  assert.throws(() => parseBallotScoring({ scores, findings: Array.from({ length: FindingLimits[FindingKind.strength] + 1 }, () => strength), summary: Text }, rubric, evidence));
});

test("a tally keeps every ballot's score, finding and summary", async () => {
  const rubric = await scoring();
  const ballots = [ScoreRange.lowest, ScoreRange.highest].map((score) => parseBallotScoring({ scores: scored(rubric, score), findings: [concern], summary: Text }, rubric, evidence));
  const tally = tallyScoring(rubric, ballots);
  assert.deepEqual(tally.scores.map((score) => score.ballots), rubric.dimensions.map(() => [ScoreRange.lowest, ScoreRange.highest]));
  assert.deepEqual(tally.findings.map((finding) => finding.ballot), [1, 2]);
  assert.equal(tally.summaries.length, ballots.length);
  assert.throws(() => tallyScoring(rubric, [{ ...ballots[0]!, scores: [] }]));
});
