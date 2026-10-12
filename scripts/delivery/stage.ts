function passed(name: string, started: number): void {
  process.stdout.write(`[PASS] ${name} (${((Date.now() - started) / 1000).toFixed(1)}s)\n`);
}

function failed(name: string, error: unknown): Error {
  return new Error(`[FAIL] ${name}: ${error instanceof Error ? error.message : String(error)}`, { cause: error });
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
