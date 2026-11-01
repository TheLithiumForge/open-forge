import type { ModelChoice } from "../../config/config.types.ts";
import type { SimulatorUsage } from "./model-reply.ts";
import type { QuestionEntry } from "./replies.ts";
import { ClassifierVersion } from "./classifier.ts";
import { ProtocolVersion } from "./answer-bank.ts";
import type { EvalConfig } from "../../config/config.types.ts";

export const ConversationRole = { user: "user", agent: "agent" } as const;
export interface ConversationMessage { readonly role: (typeof ConversationRole)[keyof typeof ConversationRole]; readonly text: string }

export interface ConversationReport {
  readonly version: typeof ProtocolVersion;
  readonly classifier: typeof ClassifierVersion;
  readonly bankHash: string | null;
  readonly requestedModel: ModelChoice;
  readonly questions: readonly QuestionEntry[];
  readonly calls: readonly SimulatorUsage[];
  readonly rounds: number;
  readonly questionCount: number;
  readonly repeats: number;
  readonly documented: number;
  readonly unanswered: number;
  readonly error: string | null;
  readonly messages?: readonly ConversationMessage[];
}

export function emptyConversation(config: EvalConfig, bankHash: string | null = config.protocol?.hash ?? null): ConversationReport {
  return { version: ProtocolVersion, classifier: ClassifierVersion, bankHash, requestedModel: config.simulatedUser,
    questions: [], calls: [], rounds: 0, questionCount: 0, repeats: 0, documented: 0, unanswered: 0, error: null, messages: [] };
}

export class SimulatorFailure extends Error {
  readonly report: ConversationReport;
  constructor(report: ConversationReport) {
    super(report.error ?? "The simulated user failed.");
    this.name = "SimulatorFailure";
    this.report = report;
  }
}
