import { mkdir, mkdtemp, rm } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { Tool } from "../../config/choices.ts";
import type { ModelChoice } from "../../config/config.types.ts";
import { runAssessorStage, AssessorTools } from "../containers/assessor.ts";
import type { StageOptions } from "../containers/stage.ts";
import type { BankEntry } from "./answer-bank.ts";
import { classifierPrompt, readClassification, type ClassifiedQuestion } from "./classifier.ts";
import { parseAssessorReply, type SimulatorUsage } from "./model-reply.ts";

export interface ClassifierInput {
  readonly choice: ModelChoice;
  readonly message: string;
  readonly bank: readonly BankEntry[];
  readonly documents: readonly string[];
  readonly options: StageOptions;
}
export interface Classification { readonly questions: readonly ClassifiedQuestion[]; readonly usage: SimulatorUsage }
export type Classify = (input: ClassifierInput) => Promise<Classification>;

export const classifyQuestions: Classify = async (input) => {
  if (input.choice.tool === Tool.mock) return {
    questions: input.message.trim().endsWith("?") ? [{ text: input.message, topicIds: [], documented: false }] : [],
    usage: { tokens: null, model: null, tools: AssessorTools.off, seconds: 0 },
  };
  const root = await mkdtemp(join(tmpdir(), "eval-classifier-"));
  const space = { root, work: join(root, "evidence"), home: join(root, "home"), project: "evidence" };
  try {
    await mkdir(space.work);
    await mkdir(space.home);
    const started = Date.now();
    const result = await runAssessorStage(space, input.choice, classifierPrompt(input.message, input.bank, input.documents), input.options);
    if (result.code !== 0 || result.timedOut) throw new Error("The simulated user assessor failed.");
    const parsed = parseAssessorReply(result.stdout, input.choice.tool);
    return { questions: readClassification(parsed.value, input.message, input.bank), usage: { ...parsed.usage, tools: result.tools ?? (input.choice.tool === Tool.codex ? AssessorTools.restricted : AssessorTools.off), seconds: (Date.now() - started) / 1000 } };
  } finally { await rm(root, { recursive: true, force: true }); }
};
