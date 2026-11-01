export const JudgeFormatRetries = 2;
export const JudgeFailureReplyBytes = 64 * 1024;

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
  // Streaming decode leaves an incomplete final UTF-8 character outside the byte limit.
  const capped = new TextDecoder().decode(Buffer.from(reply).subarray(0, JudgeFailureReplyBytes), { stream: true });
  return { ballot, formatRetries, reply: capped, error: error instanceof Error ? error.message : String(error) };
}
