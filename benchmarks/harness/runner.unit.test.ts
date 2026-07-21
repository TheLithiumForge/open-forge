import { describe, expect, test } from "bun:test";
import {
  validateMetaScenarioDocument,
  validatePrimitiveDocument,
  validateScenarioDocument,
} from "./runner.ts";

describe("benchmark composition manifests", () => {
  test("keeps scenario identity explicit and folder-independent", () => {
    expect(validateScenarioDocument({ id: "Delivery / unusual id" })).toEqual({ id: "Delivery / unusual id" });
    expect(() => validateScenarioDocument({ id: "" })).toThrow("non-empty string");
    expect(() => validateScenarioDocument({ id: " padded " })).toThrow("whitespace");
    expect(() => validateScenarioDocument({ id: "case", extensions: [] })).toThrow("unknown fields: extensions");
  });

  test("accepts every primitive kind and rejects invented package fields", () => {
    for (const kind of ["directive", "pattern", "memory", "guidance", "workspace", "skill", "workflow", "tool"]) {
      expect(validatePrimitiveDocument({ id: `fixture ${kind}`, kind })).toEqual({ id: `fixture ${kind}`, kind });
    }
    expect(() => validatePrimitiveDocument({ id: "case", kind: "persona" })).toThrow("kind must be one of");
    expect(() => validatePrimitiveDocument({ id: "case", kind: "memory", dependencies: [] })).toThrow(
      "unknown fields: dependencies",
    );
  });

  test("preserves ordered primitive selection and validates extension ids", () => {
    expect(validateMetaScenarioDocument({
      id: "base + trap",
      scenario: "plain task",
      primitives: ["control", "trap"],
      extensions: ["implementation-workflow"],
    })).toEqual({
      id: "base + trap",
      scenario: "plain task",
      primitives: ["control", "trap"],
      extensions: ["implementation-workflow"],
    });
    expect(() => validateMetaScenarioDocument({
      id: "duplicate",
      scenario: "plain",
      primitives: ["same", "same"],
      extensions: [],
    })).toThrow("must not contain duplicates");
    expect(() => validateMetaScenarioDocument({
      id: "bad extension",
      scenario: "plain",
      primitives: [],
      extensions: ["Not Bundled"],
    })).toThrow("bundled extension id");
    expect(() => validateMetaScenarioDocument({ id: "incomplete", scenario: "plain", primitives: [] })).toThrow(
      "missing fields: extensions",
    );
  });
});
