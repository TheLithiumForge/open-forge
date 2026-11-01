const cleanups = new Set<() => Promise<void>>();
const SignalExitCodes = { SIGINT: 130, SIGTERM: 143 } as const;

function interrupted(signal: keyof typeof SignalExitCodes): void {
  void Promise.allSettled([...cleanups].map((cleanup) => cleanup())).finally(() => process.exit(SignalExitCodes[signal]));
}
const interrupt = (): void => { interrupted("SIGINT"); };
const terminate = (): void => { interrupted("SIGTERM"); };

export function registerStageCleanup(cleanup: () => Promise<void>): () => void {
  if (cleanups.size === 0) {
    process.once("SIGINT", interrupt);
    process.once("SIGTERM", terminate);
  }
  cleanups.add(cleanup);
  return () => {
    cleanups.delete(cleanup);
    if (cleanups.size === 0) {
      process.removeListener("SIGINT", interrupt);
      process.removeListener("SIGTERM", terminate);
    }
  };
}
