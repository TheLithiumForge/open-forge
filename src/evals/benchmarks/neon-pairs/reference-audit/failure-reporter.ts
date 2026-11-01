import type { TestEvent } from "node:test/reporters";
import type { Failure } from "./mutation-report.ts";

/** Node test reporter that writes every failed test's name and assertion message as one JSON list. */
export default async function* failureReporter(source: AsyncIterable<TestEvent>): AsyncGenerator<string> {
  const failed: Failure[] = [];
  for await (const event of source) {
    if (event.type !== "test:fail") continue;
    // The runner wraps a test's own error, so the assertion message is on its cause.
    const { error } = event.data.details;
    failed.push({ name: event.data.name, message: error.cause instanceof Error ? error.cause.message : error.message });
  }
  yield `${JSON.stringify(failed)}\n`;
}
