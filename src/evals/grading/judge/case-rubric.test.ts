import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { test } from "node:test";
import { DocumentScaleCase } from "../../materialize/brownfield-case.ts";
import type { Suite } from "../../benchmarks/suite.types.ts";
import { caseRubric, judgePrompt, judgeQuestions } from "./judge.ts";

interface PlantedRule { readonly id: string; readonly sentence: string }
interface Question { readonly question: string; readonly rules: readonly string[] }
/** Judges see no tests and no starting manifest, so these two stay with the source checks. */
const HiddenFromJudges = ["K04", "K05"];
const Pack = "A pack document far too large for a prompt";

const planted = async (): Promise<readonly PlantedRule[]> => (JSON.parse(await readFile(new URL(`../../benchmarks/neon-pairs/cases/${DocumentScaleCase}/rules.json`, import.meta.url), "utf8")) as { rules: PlantedRule[] }).rules;

test("a case's rubric quotes each rule it asks about word for word, and misses only the rules its evidence hides", async () => {
  const rubric = JSON.parse(await readFile(caseRubric(DocumentScaleCase), "utf8")) as { questions: Question[] };
  for (const rule of await planted()) {
    const asking = rubric.questions.filter((question) => question.rules.includes(rule.id));
    if (HiddenFromJudges.includes(rule.id)) { assert.equal(asking.length, 0); continue; }
    assert.equal(asking.length, 1, rule.id);
    assert.ok(asking[0]?.question.includes(rule.sentence), rule.id);
  }
  assert.equal((await judgeQuestions({ id: DocumentScaleCase })).length, rubric.questions.length);
});

test("a judge's prompt leaves out a case's own pack, and keeps a shared specification", async () => {
  const questions = await judgeQuestions({ id: DocumentScaleCase }), evidence = { sources: [], images: [] };
  const suite = (documents: string | undefined) => ({ prompt: "A request", documents: [Pack], brownfield: { case: { documents } } }) as unknown as Suite;
  assert.equal(judgePrompt(suite("docs-pack"), questions, evidence).includes(Pack), false);
  assert.equal(judgePrompt(suite(undefined), questions, evidence).includes(Pack), true);
});
