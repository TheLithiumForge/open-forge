import assert from "node:assert/strict";
import { test } from "node:test";
import type { Judge, JudgeScore } from "../records/records.types.ts";
import { fixture } from "../records/test-fixtures.ts";
import { dimensionScore, groupShare, headlineShare, JudgeHeadline, median, ScoreScale } from "./judge-scores.ts";

const judgeOf = (scores?: readonly JudgeScore[]): Judge => {
  const judge = fixture().assessments.find((item): item is Judge => item.kind === "judge");
  assert.ok(judge !== undefined);
  return { ...judge, ...(scores === undefined ? {} : { scores }) };
};
const score = (id: string, headline: JudgeHeadline, group: string, ballots: readonly number[]): JudgeScore => ({ id, headline, group, title: id, ballots, evidence: [] });
const Play = "play", Code = "code", Spec = "spec";

test("a dimension takes its middle ballot, so one odd reading doesn't move it", () => {
  assert.equal(dimensionScore(score(Play, JudgeHeadline.quality, Play, [2, 8, 9])), 8);
  assert.equal(median([]), null);
  assert.equal(median([2, 4]), 3);
});

test("a headline weighs its groups equally, whatever their number of dimensions", () => {
  const judge = judgeOf([
    score("a", JudgeHeadline.quality, Play, [ScoreScale, ScoreScale, ScoreScale]), score("b", JudgeHeadline.quality, Play, [ScoreScale, ScoreScale, ScoreScale]), score("c", JudgeHeadline.quality, Play, [ScoreScale, ScoreScale, ScoreScale]),
    score("d", JudgeHeadline.quality, Code, [0, 0, 0]),
    score("e", JudgeHeadline.adherence, Spec, [5, 5, 5]),
  ]);
  assert.equal(headlineShare(judge, JudgeHeadline.quality), 0.5);
  assert.equal(headlineShare(judge, JudgeHeadline.adherence), 0.5);
  assert.equal(groupShare(judge, Play), 1);
  assert.equal(groupShare(judge, Code), 0);
});

test("a judgment from a rubric with no scores has no headlines", () => {
  assert.equal(headlineShare(judgeOf(), JudgeHeadline.quality), null);
  assert.equal(groupShare(judgeOf(), Play), null);
});
