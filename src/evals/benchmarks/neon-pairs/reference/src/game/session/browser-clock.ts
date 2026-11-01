export const CLOCK_INTERVAL_MS = 40;

export function elapsedStep(previous: number, now: number): { milliseconds: number; next: number } {
  const milliseconds = Math.floor(now - previous);
  if (!Number.isSafeInteger(milliseconds) || milliseconds < 0) return { milliseconds: 0, next: now };
  return { milliseconds, next: previous + milliseconds };
}
