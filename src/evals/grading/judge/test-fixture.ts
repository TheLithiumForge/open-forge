import { mkdir, writeFile } from "node:fs/promises";
import { join } from "node:path";
import type { TestContext } from "node:test";
import { Tool } from "../../config/choices.ts";
import { FixtureSuite } from "../../config/test-fixtures.ts";
import { temporaryHistory } from "../../history/test-fixtures.ts";
import type { ProcessResult } from "../../execution/process.ts";
import { frozenQuestions, JudgeVerdict } from "./rubric.ts";

export const BallotSource = "game.ts";
export const BallotSourceText = "export const pairCount = 8;\n";
export const BallotTimeoutMs = 1000;

export async function ballotFixture(t: TestContext) {
  const history = await temporaryHistory(); t.after(history.cleanup);
  const folder = join(history.root, "project"), home = join(history.root, "home");
  await mkdir(folder);
  await writeFile(join(folder, BallotSource), BallotSourceText);
  const questions = await frozenQuestions();
  const answers = questions.map((question) => ({ id: question.id, answer: JudgeVerdict.unsure, evidence: "Fixture source evidence.", citations: [`source:${BallotSource}:1`] }));
  return { ...history, folder, home, suite: FixtureSuite, questions, answers, reply: JSON.stringify({ answers }) };
}

export function assessorResult(tool: Tool, reply: string): ProcessResult {
  return { code: 0, timedOut: false, stderr: "", stdout: tool === Tool.codex
    ? JSON.stringify({ type: "item.completed", item: { type: "agent_message", text: reply } })
    : JSON.stringify({ result: reply }) };
}
