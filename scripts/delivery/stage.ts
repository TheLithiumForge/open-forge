export function runStage<T>(name: string, action: () => T): T {
  process.stdout.write(`\n[START] ${name}\n`);
  const started = Date.now();
  try {
    const result = action();
    process.stdout.write(`[PASS] ${name} (${((Date.now() - started) / 1000).toFixed(1)}s)\n`);
    return result;
  } catch (error) {
    throw new Error(`[FAIL] ${name}: ${error instanceof Error ? error.message : String(error)}`, { cause: error });
  }
}
