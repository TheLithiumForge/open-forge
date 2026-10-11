import assert from "node:assert/strict";
import { test } from "node:test";
import { pollDelay, rateLimitDelay, RateLimitHeader, RateLimitStatus, ResetMarginMilliseconds } from "../github-rate-limit.ts";

const Now = 1_791_670_000_000;
const ResetSeconds = 1_791_670_120;
const Ok = 200;
const NotFound = 404;

function response(status: number, headers: Record<string, string>) {
  return { status, header: (name: string) => headers[name] ?? null };
}

test("a primary rate limit waits until GitHub's reset time plus a margin", () => {
  const limited = response(RateLimitStatus.forbidden, { [RateLimitHeader.remaining]: "0", [RateLimitHeader.reset]: String(ResetSeconds) });
  assert.equal(rateLimitDelay(limited, Now), ResetSeconds * 1_000 - Now + ResetMarginMilliseconds);
});

test("a reset time already passed retries after the margin only", () => {
  const passed = response(RateLimitStatus.forbidden, { [RateLimitHeader.remaining]: "0", [RateLimitHeader.reset]: String(Now / 1_000 - 10) });
  assert.equal(rateLimitDelay(passed, Now), ResetMarginMilliseconds);
});

test("a secondary rate limit follows Retry-After", () => {
  const secondary = response(RateLimitStatus.tooMany, { [RateLimitHeader.retryAfter]: "30" });
  assert.equal(rateLimitDelay(secondary, Now), 30_000 + ResetMarginMilliseconds);
});

test("other failures are not treated as rate limits", () => {
  assert.equal(rateLimitDelay(response(RateLimitStatus.forbidden, { [RateLimitHeader.remaining]: "12" }), Now), undefined);
  assert.equal(rateLimitDelay(response(NotFound, {}), Now), undefined);
  assert.equal(rateLimitDelay(response(Ok, {}), Now), undefined);
  assert.equal(rateLimitDelay(response(RateLimitStatus.forbidden, { [RateLimitHeader.remaining]: "0" }), Now), undefined);
});

test("polling is slower without a token", () => {
  assert.equal(pollDelay(true, 15, 180), 15);
  assert.equal(pollDelay(false, 15, 180), 180);
});
