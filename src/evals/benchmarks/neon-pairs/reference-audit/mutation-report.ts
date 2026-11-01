import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { record } from "../reference/src/game/saved-format.ts";

export interface Failure {
  name: string;
  message: string;
}
export async function failures(path: string, browser: boolean): Promise<Failure[]> {
  const value: unknown = JSON.parse(await readFile(path, "utf8"));
  assert.ok(record(value));
  if (browser) {
    assert.ok(typeof value["name"] === "string" && typeof value["message"] === "string");
    return [{ name: value["name"], message: value["message"] }];
  }
  assert.ok(Array.isArray(value["testResults"]));
  return value["testResults"].flatMap((suite) => {
    assert.ok(record(suite) && Array.isArray(suite["assertionResults"]));
    return suite["assertionResults"]
      .filter((a) => record(a) && a["status"] === "failed")
      .map((a) => {
        assert.ok(record(a) && typeof a["fullName"] === "string" && Array.isArray(a["failureMessages"]));
        assert.ok(a["failureMessages"].every((m) => typeof m === "string"));
        return { name: a["fullName"], message: a["failureMessages"].join("\n") };
      });
  });
}
export function matchingFailure(found: Failure[], testName: string, reason: string): Failure | undefined {
  return found.find((f) => f.name === testName && f.message.includes(reason));
}
