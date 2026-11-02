import assert from "node:assert/strict";
import { test } from "node:test";
import { outcomeForError, TurnOutcome } from "./agent-session.ts";
import { claudeLimitEvent } from "./claude-telemetry.ts";

test("Claude's limit wording and its structured limit events count as limits, other failures don't", () => {
  assert.equal(outcomeForError("You've hit your 5-hour limit · resets 4:40am (UTC)"), TurnOutcome.limited);
  assert.equal(outcomeForError("You've hit your usage limit."), TurnOutcome.limited);
  assert.equal(outcomeForError("Fixture failure."), TurnOutcome.failed);
  assert.equal(claudeLimitEvent({ type: "rate_limit_event", rate_limit_info: { status: "rejected" } }), true);
  assert.equal(claudeLimitEvent({ type: "rate_limit_event", rate_limit_info: { status: "allowed" } }), false);
  assert.equal(claudeLimitEvent({ type: "result", is_error: true, api_error_status: 429 }), true);
  assert.equal(claudeLimitEvent({ type: "result", is_error: true }), false);
});
