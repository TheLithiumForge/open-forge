/** GitHub's rate-limit response headers and the statuses that carry them. */
export const RateLimitHeader = { remaining: "x-ratelimit-remaining", reset: "x-ratelimit-reset", retryAfter: "retry-after" } as const;
export const RateLimitStatus = { forbidden: 403, tooMany: 429 } as const;

/** A small margin so the next request lands after GitHub's reset second. */
export const ResetMarginMilliseconds = 5_000;
const MillisecondsPerSecond = 1_000;

export interface RateLimitResponse {
  readonly status: number;
  readonly header: (name: string) => string | null;
}

/**
 * Returns how long to wait before retrying a rate-limited request, or
 * undefined when the response is not a rate limit. GitHub signals a primary
 * limit with zero remaining requests and a reset time, and a secondary limit
 * with Retry-After.
 */
export function rateLimitDelay(response: RateLimitResponse, nowMilliseconds: number): number | undefined {
  if (response.status !== RateLimitStatus.forbidden && response.status !== RateLimitStatus.tooMany) return undefined;
  const retryAfter = Number(response.header(RateLimitHeader.retryAfter));
  if (Number.isFinite(retryAfter) && retryAfter > 0) return retryAfter * MillisecondsPerSecond + ResetMarginMilliseconds;
  if (response.header(RateLimitHeader.remaining) !== "0") return undefined;
  const resetHeader = response.header(RateLimitHeader.reset);
  if (resetHeader === null) return undefined;
  const reset = Number(resetHeader);
  if (!Number.isFinite(reset)) return undefined;
  return Math.max(0, reset * MillisecondsPerSecond - nowMilliseconds) + ResetMarginMilliseconds;
}

/** Unauthenticated requests share 60 per hour, so poll slowly without a token. */
export function pollDelay(hasToken: boolean, authenticated: number, anonymous: number): number {
  return hasToken ? authenticated : anonymous;
}
