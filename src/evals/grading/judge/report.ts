import { capReply, ReplyEvidenceBytes } from "../../execution/reply-json.ts";
import type { JudgeScores } from "./scoring.ts";
import type { StalePage } from "./work-evidence.ts";

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
  /** A rubric's scores and findings, once every ballot has been read. */
  readonly scoring?: JudgeScores;
  /** Which pages that described the game before the work still describe it after, read from the files and not from a judge. */
  readonly staleDocs?: readonly StalePage[];
}

export function ballotFailure(ballot: number, formatRetries: number, reply: string, error: unknown): BallotFailure {
  return { ballot, formatRetries, reply: capReply(reply), error: error instanceof Error ? error.message : String(error) };
}
