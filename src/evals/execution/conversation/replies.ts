import type { QuestionRound } from "../../history/results.types.ts";
import { DefaultReply, type BankEntry } from "./answer-bank.ts";
import type { ClassifiedQuestion } from "./classifier.ts";

export interface QuestionEntry extends QuestionRound {
  readonly round: number;
  readonly topicIds: readonly string[];
  readonly answered: boolean;
  readonly repeated: boolean;
  readonly documented: boolean;
  readonly answerLength: number;
  readonly answerBytes: number;
}

export function literalAnswers(questions: readonly ClassifiedQuestion[], bank: readonly BankEntry[], round: number, allowed: boolean, seen: Set<string>): QuestionEntry[] {
  const byTopic = new Map(bank.map((entry) => [entry.topicId, entry.answer]));
  return questions.map((question) => {
    const answers = question.topicIds.map((id) => byTopic.get(id));
    const answered = allowed && answers.length > 0 && answers.every((answer) => answer !== undefined);
    const answer = answered ? answers.join("\n") : DefaultReply;
    const key = question.topicIds.length > 0 ? [...question.topicIds].sort().join("\0") : question.text.toLowerCase().replaceAll(/\s+/g, " ").trim();
    const repeated = seen.has(key);
    seen.add(key);
    return { question: question.text, answer, round, topicIds: question.topicIds, answered, repeated, documented: question.documented, answerLength: answer.length, answerBytes: Buffer.byteLength(answer) };
  });
}
