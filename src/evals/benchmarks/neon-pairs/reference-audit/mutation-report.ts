import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { isJsonObject } from "../reference/src/game/saved-format.ts";

export interface Failure {
  name: string;
  message: string;
}
export async function failures(path: string, browser: boolean): Promise<Failure[]> {
  const value: unknown = JSON.parse(await readFile(path, "utf8"));
  if (browser) {
    assert.ok(Array.isArray(value));
    return value.map((failure) => {
      assert.ok(isJsonObject(failure) && typeof failure["name"] === "string" && typeof failure["message"] === "string");
      return { name: failure["name"], message: failure["message"] };
    });
  }
  assert.ok(isJsonObject(value));
  assert.ok(Array.isArray(value["testResults"]));
  return value["testResults"].flatMap((suite) => {
    assert.ok(isJsonObject(suite) && Array.isArray(suite["assertionResults"]));
    return suite["assertionResults"]
      .filter((a) => isJsonObject(a) && a["status"] === "failed")
      .map((a) => {
        assert.ok(isJsonObject(a) && typeof a["fullName"] === "string" && Array.isArray(a["failureMessages"]));
        assert.ok(a["failureMessages"].every((m) => typeof m === "string"));
        return { name: a["fullName"], message: a["failureMessages"].join("\n") };
      });
  });
}
export function matchingFailure(found: Failure[], testName: string, reason: string): Failure | undefined {
  return found.find((f) => f.name === testName && f.message.includes(reason));
}
