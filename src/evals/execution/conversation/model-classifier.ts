import { mkdir, mkdtemp, rm } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { Tool } from "../../config/choices.ts";
import type { ModelChoice } from "../../config/config.types.ts";
import { runAssessorStage, AssessorTools } from "../containers/assessor.ts";
import type { StageOptions } from "../containers/stage.ts";
import type { BankEntry } from "./answer-bank.ts";
import { classifierPrompt, readClassification, type ClassifiedQuestion } from "./classifier.ts";
import { readAssessorMessage, SimulatorModelFailure, type SimulatorUsage } from "./model-reply.ts";
import { capReply, replyJson } from "../reply-json.ts";

export interface ClassifierInput {
  readonly choice: ModelChoice;
  readonly message: string;
  readonly bank: readonly BankEntry[];
  readonly documents: readonly string[];
  readonly options: StageOptions;
}
export interface Classification { readonly questions: readonly ClassifiedQuestion[]; readonly usage: SimulatorUsage; readonly calls?: readonly SimulatorUsage[] }
export type Classify = (input: ClassifierInput) => Promise<Classification>;
export const ClassifierFormatRetries = 2;

export class ClassifierFailure extends Error {
  readonly calls: readonly SimulatorUsage[];
  constructor(error: unknown, calls: readonly SimulatorUsage[]) {
    super(error instanceof Error ? error.message : String(error));
    this.calls = calls;
  }
}

export async function classifyQuestions(input: ClassifierInput, assess = runAssessorStage): Promise<Classification> {
  if (input.choice.tool === Tool.mock) return {
    questions: input.message.trim().endsWith("?") ? [{ text: input.message, topicIds: [], documented: false }] : [],
    usage: { tokens: null, model: null, tools: AssessorTools.off, seconds: 0 },
  };
  const root = await mkdtemp(join(tmpdir(), "eval-classifier-"));
  const work = join(root, "evidence"), prompt = classifierPrompt(input.message, input.bank, input.documents);
  const calls: SimulatorUsage[] = [];
  let reply = "";
  try {
    await mkdir(work);
    for (let attempt = 0; attempt <= ClassifierFormatRetries; attempt += 1) {
      const space = { root, work, home: join(root, `try-${attempt}`), project: "evidence" };
      await mkdir(space.home);
      const started = Date.now();
      let retryable = false;
      let usage: SimulatorUsage = { tokens: null, model: null, tools: input.choice.tool === Tool.codex ? AssessorTools.restricted : AssessorTools.off, seconds: 0 };
      try {
        const result = await assess(space, input.choice, prompt, input.options);
        reply = result.stdout;
        usage = { ...usage, tools: result.tools ?? usage.tools };
        if (result.code !== 0 || result.timedOut || result.truncated) throw new Error("The simulated user assessor failed.");
        retryable = true;
        const parsed = readAssessorMessage(result.stdout, input.choice.tool);
        reply = parsed.reply;
        usage = { ...usage, ...parsed.usage };
        const questions = readClassification(replyJson(reply), input.message, input.bank);
        usage = { ...usage, seconds: (Date.now() - started) / 1000, reply: capReply(reply) };
        calls.push(usage);
        return { questions, usage, calls };
      } catch (error) {
        const failed = { ...usage, seconds: (Date.now() - started) / 1000, error: error instanceof Error ? error.message : String(error) };
        if (retryable && !(error instanceof SimulatorModelFailure) && attempt < ClassifierFormatRetries) { calls.push(failed); continue; }
        calls.push({ ...failed, reply: capReply(reply) });
        throw new ClassifierFailure(error, calls);
      }
    }
    throw new Error("The classifier exhausted its attempts.");
  } finally { await rm(root, { recursive: true, force: true }); }
}
