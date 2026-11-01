import assert from "node:assert/strict";
import { test } from "node:test";
import { commandArguments } from "./candidate-commands.ts";
import { buildOutcomes, CommandId, commandResult, compareReplay, replayOutcomes } from "./commands.ts";
import { expectedReplay, freshSnapshot } from "./replay.ts";
import { decodeSnapshot } from "./snapshot.ts";
import { Evidence } from "./outcomes.ts";
import { KnownCode, replayVectors, snapshotVectors } from "./vectors.ts";
import { SpecLayerContainer } from "./paths.ts";

const Success = { code: 0, timedOut: false, stdout: "", stderr: "" };
test("command parser bounds untrusted results and keeps CLI output exact", () => {
  assert.deepEqual(commandResult(Success), Success);
  for (const value of [{ ...Success, code: "0" }, { ...Success, timedOut: "no" }, { ...Success, stdout: 3 }, { ...Success, html: [] }]) assert.throws(() => commandResult(value));
  assert.deepEqual(commandArguments(CommandId.replay, "vector-0.json"), ["run", "--silent", "replay", "--", `${SpecLayerContainer.vectors}/vector-0.json`]);
  assert.throws(() => commandArguments(CommandId.replay, "../input.json"));
  assert.throws(() => commandArguments("arbitrary"));
});
test("replay comparator rejects dropped moves, invalid success and checker timeouts", () => {
  const vector = { name: "pair", clauses: ["D09.2"], text: JSON.stringify({ version: 1, boardCode: KnownCode, actions: [{ atMs: 0, index: 0 }, { atMs: 0, index: 11 }], endMs: 0 }) };
  const expected = expectedReplay(vector.text); assert.ok(expected);
  assert.equal(compareReplay(vector, { ...Success, stdout: JSON.stringify(expected) }), true);
  assert.equal(compareReplay(vector, { ...Success, stdout: JSON.stringify(freshSnapshot(KnownCode)) }), false);
  assert.equal(compareReplay({ ...vector, text: "{" }, { ...Success, code: 1 }), true);
  assert.equal(compareReplay({ ...vector, text: "{" }, Success), false);
  assert.throws(() => compareReplay(vector, { ...Success, timedOut: true }));
  const evidence = new Evidence(); replayOutcomes([vector], [{ ...Success, stdout: JSON.stringify(freshSnapshot(KnownCode)) }], evidence);
  assert.equal(evidence.outcomes[0]?.status, "fail");
});
test("frozen vectors cover rejection, valid boundary states and known literal deals", () => {
  const snapshots = snapshotVectors(), replays = replayVectors();
  assert.ok(snapshots.some((item) => decodeSnapshot(item.text)));
  assert.ok(snapshots.some((item) => !decodeSnapshot(item.text)));
  assert.ok(replays.some((item) => item.name.includes("739806647")));
  const literal = replays.find((item) => item.name === `known deal ${KnownCode}`);
  assert.ok(literal);
  assert.equal(expectedReplay(literal.text)?.matched.length, 16);
  const results = replays.map((item) => { const expected = expectedReplay(item.text); return { ...Success, code: expected ? 0 : 1, stdout: expected ? JSON.stringify(expected) : "" }; });
  const evidence = new Evidence(); replayOutcomes(replays, results, evidence);
  assert.ok(evidence.outcomes.every((item) => item.status === "pass"));
});
test("build assertions use fake isolated commands and leave unsupported evidence as errors", () => {
  const result = { ...Success, stdout: "Tests 10 passed\nnpm run typecheck\nvite building\n2 passed\nLocal: http://127.0.0.1", html: "<html><script>inline</script></html>" };
  const commands = new Map(Object.values(CommandId).map((id) => [id, id === CommandId.stale ? { ...result, code: 1 } : result]));
  const evidence = new Evidence(); buildOutcomes(commands, evidence, true);
  assert.ok(evidence.outcomes.filter((item) => item.clause !== "T03.4" && item.clause !== "T07.9").every((item) => item.status === "pass"));
  assert.equal(evidence.outcomes.find((item) => item.clause === "T03.4")?.status, "error");
});
