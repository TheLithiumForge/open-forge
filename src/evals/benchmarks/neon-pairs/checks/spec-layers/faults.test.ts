import assert from "node:assert/strict";
import { existsSync } from "node:fs";
import { test } from "node:test";
import { chromium } from "playwright";
import { accessibleNames, RevealedNameClauses } from "./browser-access.ts";
import { fakeGame } from "./browser-game.fixture.ts";
import { pageObservation } from "./browser-evidence.ts";
import { MismatchClosedClause, reporter, turns } from "./browser-play.ts";
import { faces, FaceClause } from "./browser-view.ts";
import { compareReplay, replayOutcomes } from "./commands.ts";
import { expectedDeck } from "./board-code.ts";
import { Evidence } from "./outcomes.ts";
import { activate, advance, freshSnapshot, parseReplay } from "./replay.ts";
import { Rules, SelectionKind } from "./rules.ts";
import { DistinctFacesClause, DistinctFacesVector, replayVectors } from "./vectors.ts";

function repeatedReplay(text: string): string {
  const replay = parseReplay(text); assert.ok(replay);
  const deck = expectedDeck(replay.boardCode).map((face) => face % 32);
  let state = freshSnapshot(replay.boardCode), previous = 0;
  for (const action of replay.actions) {
    state = advance(state, action.atMs - previous)!; previous = action.atMs;
    const selection = state.selection;
    if (selection.kind === SelectionKind.one && selection.index !== action.index && !state.matched.includes(action.index) && deck[selection.index] === deck[action.index]) {
      state = { ...state, moves: state.moves + 1, matched: [...state.matched, selection.index, action.index], selection: { kind: SelectionKind.closed } };
    } else state = activate(state, action.index);
  }
  return JSON.stringify(advance(state, replay.endMs - previous));
}

test("different-identity replays detect repeated faces that perfect completion cannot detect", () => {
  const vectors = replayVectors().filter((vector) => vector.clauses.includes(DistinctFacesClause));
  const result = (text: string) => ({ code: 0, timedOut: false, stdout: repeatedReplay(text), stderr: "" });
  for (const vector of vectors.filter((vector) => !vector.name.startsWith(DistinctFacesVector))) assert.equal(compareReplay(vector, result(vector.text)), true);
  const contrasting = vectors.filter((vector) => vector.name.startsWith(DistinctFacesVector));
  assert.equal(contrasting.length, Rules.sizes.length);
  assert.ok(contrasting.some((vector) => !compareReplay(vector, result(vector.text))));
  const evidence = new Evidence(); replayOutcomes(vectors, vectors.map((vector) => result(vector.text)), evidence);
  const failed = evidence.outcomes.find((outcome) => outcome.clause === DistinctFacesClause);
  assert.equal(failed?.status, "fail");
  assert.match(failed?.evidence ?? "", /Observed:.*expected.*snapshot.*actual.*stdout/s);
});

test("late closing fails the timing assertion after discovery without a face-name dependency", async () => {
  const browser = await chromium.launch(existsSync(chromium.executablePath()) ? {} : { channel: "chrome" });
  try {
    for (const mismatchMs of [Rules.mismatchMs, Rules.mismatchMs + 1]) {
      const page = await browser.newPage(); await fakeGame(page, { mismatchMs });
      const evidence = new Evidence(); await turns(page, evidence);
      const outcome = evidence.outcomes.find((outcome) => outcome.clause === MismatchClosedClause);
      assert.equal(outcome?.status, mismatchMs === Rules.mismatchMs ? "pass" : "fail", JSON.stringify(outcome));
      assert.match(outcome?.evidence ?? "", /Observed:.*activeMs.*names/s);
      if (mismatchMs > Rules.mismatchMs) assert.match(outcome?.evidence ?? "", /revealed/);
      await page.close();
    }
  } finally { await browser.close(); }
});

test("repeated faces fail identity checks and still collect names and matched-card assertions once", async () => {
  const browser = await chromium.launch(existsSync(chromium.executablePath()) ? {} : { channel: "chrome" });
  try {
    const page = await browser.newPage(); await fakeGame(page, { artwork: true });
    const evidence = new Evidence(); await faces(page, evidence);
    assert.equal(evidence.outcomes.find((outcome) => outcome.clause === FaceClause.unique)?.status, "fail");
    await page.getByRole("combobox").selectOption("4");
    await accessibleNames(page, evidence);
    for (const clause of [...RevealedNameClauses, FaceClause.closed, FaceClause.artwork, FaceClause.check, FaceClause.opacity]) {
      const outcomes = evidence.outcomes.filter((outcome) => outcome.clause === clause);
      assert.equal(outcomes.length, 1, clause); assert.equal(outcomes[0]?.status, "pass", JSON.stringify(outcomes));
    }
    assert.ok(evidence.outcomes.every((outcome) => outcome.status !== "error"));
  } finally { await browser.close(); }
});

test("failed browser assertions keep current text, values and clock beside their expectation", async () => {
  const browser = await chromium.launch(existsSync(chromium.executablePath()) ? {} : { channel: "chrome" });
  try {
    const page = await browser.newPage(); await fakeGame(page);
    await page.locator("#status").evaluate((element) => { element.textContent = "Time: 0:42"; });
    const evidence = new Evidence(); evidence.observation = () => pageObservation(page);
    reporter(evidence)(MismatchClosedClause, false, "Expected closed cards at 800 active milliseconds.");
    await evidence.flush();
    assert.match(evidence.outcomes[0]?.evidence ?? "", /Expected closed.*Observed page:.*clockMs.*Time: 0:42.*cardCount/s);
  } finally { await browser.close(); }
});
