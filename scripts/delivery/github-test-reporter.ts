import { failureAnnotation, SuiteLimits } from "./suite-diagnostics.ts";

interface TestEvent {
  type: string;
  data: { name?: string; file?: string; line?: number; details?: { error?: unknown } };
}

const NpmScriptVariable = "npm_lifecycle_event";
const TestScriptPrefix = "test:";

/** A failed test's error message. node:test wraps the thrown error as its cause. */
export function failureMessage(error: unknown): string {
  const cause = error instanceof Error && error.cause !== undefined ? error.cause : error;
  return cause instanceof Error ? cause.message : String(cause ?? "");
}

/** Names the checks by the npm script that runs them, such as delivery for test:delivery. */
export function suiteName(script: string | undefined): string {
  return script?.startsWith(TestScriptPrefix) ? script.slice(TestScriptPrefix.length) : (script ?? "node");
}

/**
 * A node:test reporter that turns failed tests into GitHub error annotations, which are readable
 * without the signed-in session job logs need. Outside GitHub Actions it prints nothing.
 */
export default async function* githubTestReporter(source: AsyncIterable<TestEvent>): AsyncGenerator<string> {
  const annotate = process.env["GITHUB_ACTIONS"] === "true";
  const suite = suiteName(process.env[NpmScriptVariable]);
  let annotated = 0;
  for await (const event of source) {
    if (!annotate || event.type !== "test:fail" || annotated >= SuiteLimits.annotations) continue;
    annotated++;
    const { name = "(unnamed test)", file, line, details } = event.data;
    yield `${failureAnnotation(process.cwd(), suite, { name, status: "failed", message: failureMessage(details?.error), file, line, milliseconds: 0 })}\n`;
  }
}
