import type { ModelChoice } from "../../config/config.types.ts";
import type { IntentItem } from "../../benchmarks/suite.types.ts";
import type { StageOptions } from "../containers/stage.ts";
import { answerBank, type BankEntry } from "./answer-bank.ts";
import { classifyQuestions, type Classify, type Classification } from "./model-classifier.ts";
export { DefaultReply } from "./answer-bank.ts";

export interface UserTurn {
  readonly choice: ModelChoice;
  readonly request: string;
  readonly intent: readonly IntentItem[];
  readonly agentMessage: string;
  readonly home: string;
  readonly timeoutMs: number;
  readonly bank?: readonly BankEntry[];
  readonly documents?: readonly string[];
  readonly options?: Omit<StageOptions, "timeoutMs">;
}

export async function replyAsUser(turn: UserTurn, classify: Classify = classifyQuestions): Promise<Classification> {
  return classify({ choice: turn.choice, message: turn.agentMessage, bank: turn.bank ?? answerBank(turn.intent), documents: turn.documents ?? [turn.request],
    options: { ...turn.options, timeoutMs: turn.timeoutMs } });
}
