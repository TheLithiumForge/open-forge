import { join } from "node:path";
import type { JudgeChoice } from "../../config/config.types.ts";
import { askModel } from "../../execution/providers/structured.ts";
import type { JudgeAnswer } from "../../history/results.types.ts";
import { JudgedBy } from "../../benchmarks/suite-values.ts";
import type { Suite } from "../../benchmarks/suite.types.ts";

export const JudgeVerdict = { yes: "yes", no: "no", unsure: "unsure" } as const;
type JudgeVerdict = (typeof JudgeVerdict)[keyof typeof JudgeVerdict];

export const IntentArea = "intent";
const JudgeTools = ["Read", "Glob", "Grep"] as const;

interface JudgeQuestion {
  readonly id: string;
  readonly area: string;
  readonly question: string;
}

interface RawAnswer {
  readonly id: string;
  readonly answer: JudgeVerdict;
  readonly evidence: string;
}

const answersSchema = {
  type: "object",
  additionalProperties: false,
  required: ["answers"],
  properties: {
    answers: {
      type: "array",
      items: {
        type: "object",
        additionalProperties: false,
        required: ["id", "answer", "evidence"],
        properties: { id: { type: "string" }, answer: { type: "string", enum: Object.values(JudgeVerdict) }, evidence: { type: "string" } },
      },
    },
  },
} as const;

export function judgeQuestions(suite: Suite): JudgeQuestion[] {
  const wishes = suite.intent.filter((item) => item.judgedBy === JudgedBy.judge);
  return [...suite.rubric, ...wishes.map((item) => ({ id: item.id, area: IntentArea, question: `Does the result meet this wish of the user: ${item.text}` }))];
}

function judgePrompt(suite: Suite, questions: readonly JudgeQuestion[]): string {
  return [
    "You are reviewing a small project that a coding agent built for a user. Read the code in the current folder. Don't run anything.",
    `The user's request:\n<request>\n${suite.prompt}\n</request>`,
    "Answer each question with yes, no or unsure. Cite the files and lines your answer is based on. Answer unsure when the code doesn't let you tell.",
    questions.map((question) => `- [${question.id}] ${question.question}`).join("\n"),
  ].join("\n\n");
}

function rawAnswers(value: unknown): RawAnswer[] {
  if (typeof value !== "object" || value === null || !("answers" in value) || !Array.isArray(value.answers)) return [];
  return value.answers.filter((answer: unknown): answer is RawAnswer => typeof answer === "object" && answer !== null && "id" in answer && "answer" in answer);
}

/** Asks the judge every question several times. The counts show how sure it is. */
export async function judgeRun(suite: Suite, choice: JudgeChoice, folder: string, home: string, timeoutMs: number): Promise<JudgeAnswer[]> {
  const questions = judgeQuestions(suite);
  const rounds: RawAnswer[][] = [];
  for (let repeat = 0; repeat < choice.repeats; repeat += 1) {
    const result = await askModel({ choice, cwd: folder, home: join(home, `judge-${repeat}`), prompt: judgePrompt(suite, questions), schema: answersSchema, readTools: JudgeTools, timeoutMs });
    rounds.push(rawAnswers(result.value));
  }
  return questions.map((question) => {
    const answers = rounds.map((round) => round.find((answer) => answer.id === question.id));
    const tally = (verdict: JudgeVerdict): number => answers.filter((answer) => answer?.answer === verdict).length;
    return {
      ...question,
      yes: tally(JudgeVerdict.yes),
      no: tally(JudgeVerdict.no),
      unsure: choice.repeats - tally(JudgeVerdict.yes) - tally(JudgeVerdict.no),
      evidence: answers.map((answer) => answer?.evidence ?? "").filter((evidence) => evidence !== ""),
    };
  });
}
