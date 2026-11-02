import assert from "node:assert/strict";
import { test } from "node:test";
import { outcomeForError, TurnOutcome } from "./agent-session.ts";
import { claudeLimitEvent } from "./claude-telemetry.ts";
import { requireSignIns } from "../../config/auth.ts";
import { Tool } from "../../config/choices.ts";

test("Claude's limit wording and its structured limit events count as limits, other failures don't", () => {
  assert.equal(outcomeForError("You've hit your 5-hour limit · resets 4:40am (UTC)"), TurnOutcome.limited);
  assert.equal(outcomeForError("You've hit your usage limit."), TurnOutcome.limited);
  assert.equal(outcomeForError("Fixture failure."), TurnOutcome.failed);
  assert.equal(claudeLimitEvent({ type: "rate_limit_event", rate_limit_info: { status: "rejected" } }), true);
  assert.equal(claudeLimitEvent({ type: "rate_limit_event", rate_limit_info: { status: "allowed" } }), false);
  assert.equal(claudeLimitEvent({ type: "result", is_error: true, api_error_status: 429 }), true);
  assert.equal(claudeLimitEvent({ type: "result", is_error: true }), false);
});

test("a rejected sign-in pauses the batch like a usage limit", () => {
  assert.equal(outcomeForError("unexpected status 401 Unauthorized: Missing bearer or basic authentication in header"), TurnOutcome.limited);
  assert.equal(outcomeForError("Invalid API key · Please run /login"), TurnOutcome.limited);
});

test("work that calls a tool without a sign-in refuses to start, and mock tools need none", () => {
  assert.throws(() => requireSignIns([Tool.codex, Tool.claude], { claude: true, codex: false }));
  assert.doesNotThrow(() => requireSignIns([Tool.mock, Tool.codex], { claude: false, codex: true }));
});
