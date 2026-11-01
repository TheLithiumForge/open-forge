import type { ModelChoice } from "../../config/config.types.ts";
import type { SimulatorUsage } from "./model-reply.ts";
import type { QuestionEntry } from "./replies.ts";
import { ClassifierVersion } from "./classifier.ts";
import { ProtocolVersion } from "./answer-bank.ts";

export interface ConversationReport {
  readonly version: typeof ProtocolVersion;
  readonly classifier: typeof ClassifierVersion;
  readonly bankHash: string;
  readonly requestedModel: ModelChoice;
  readonly questions: readonly QuestionEntry[];
  readonly calls: readonly SimulatorUsage[];
  readonly rounds: number;
  readonly questionCount: number;
  readonly repeats: number;
  readonly documented: number;
  readonly unanswered: number;
  readonly error: string | null;
}

export class SimulatorFailure extends Error {
  readonly report: ConversationReport;
  constructor(report: ConversationReport) {
    super(report.error ?? "The simulated user failed.");
    this.name = "SimulatorFailure";
    this.report = report;
  }
}
