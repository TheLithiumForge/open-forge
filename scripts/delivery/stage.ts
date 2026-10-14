import { commandData, commandProperty } from "./suite-diagnostics.ts";

function passed(name: string, started: number): void {
  process.stdout.write(`[PASS] ${name} (${((Date.now() - started) / 1000).toFixed(1)}s)\n`);
}

/** A GitHub error annotation for a failed stage. Annotations are public, unlike job logs. */
export function stageFailureAnnotation(name: string, message: string): string {
  return `::error title=${commandProperty(`${name} failed`)}::${commandData(message)}`;
}

function failed(name: string, error: unknown): Error {
  const message = error instanceof Error ? error.message : String(error);
  if (process.env["GITHUB_ACTIONS"] === "true") process.stdout.write(`${stageFailureAnnotation(name, message)}\n`);
  return new Error(`[FAIL] ${name}: ${message}`, { cause: error });
}

export function runStage<T>(name: string, action: () => T): T {
  process.stdout.write(`\n[START] ${name}\n`);
  const started = Date.now();
  try {
    const result = action();
    passed(name, started);
    return result;
  } catch (error) {
    throw failed(name, error);
  }
}

export async function runStageAsync<T>(name: string, action: () => Promise<T>): Promise<T> {
  process.stdout.write(`\n[START] ${name}\n`);
  const started = Date.now();
  try {
    const result = await action();
    passed(name, started);
    return result;
  } catch (error) {
    throw failed(name, error);
  }
}
