import { capReply, ReplyEvidenceBytes } from "../../execution/reply-json.ts";

export const JudgeFormatRetries = 2;
export const JudgeFailureReplyBytes = ReplyEvidenceBytes;

export interface BallotFailure {
  readonly ballot: number;
  readonly formatRetries: number;
  readonly reply: string;
  readonly error: string;
}
export interface JudgeReport {
  readonly formatRetries: number;
  readonly failures: readonly BallotFailure[];
}

export function ballotFailure(ballot: number, formatRetries: number, reply: string, error: unknown): BallotFailure {
  return { ballot, formatRetries, reply: capReply(reply), error: error instanceof Error ? error.message : String(error) };
}
