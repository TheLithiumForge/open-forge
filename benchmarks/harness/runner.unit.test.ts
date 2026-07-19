import { describe, expect, test } from "bun:test";
import { stableStringify, validateEvaluationDocument } from "./runner.ts";

const coreRatingIds = [
  "directive-compliance",
  "memory-growth",
  "routing-behavior",
  "communication",
  "product-fidelity",
];

describe("benchmark harness pure contracts", () => {
  test("serializes equivalent objects canonically", () => {
    expect(stableStringify({ zeta: 2, alpha: { second: false, first: true } })).toBe(
      stableStringify({ alpha: { first: true, second: false }, zeta: 2 })
    );
  });

  test("keeps executable evaluation validation aligned with the seed rating schema", () => {
    const ratings = coreRatingIds.map((id) => ({ id, score: 1, maxScore: 2, rationale: "fixture" }));
    expect(() => validateEvaluationDocument({
      schemaVersion: 1,
      status: "complete",
      objectiveAssertions: [],
      subjectiveRatings: [...ratings, { id: "seed-", score: 1, maxScore: 2, rationale: "invalid" }],
      traces: [],
      notes: [],
    })).toThrow("must match seed-");
  });
});
