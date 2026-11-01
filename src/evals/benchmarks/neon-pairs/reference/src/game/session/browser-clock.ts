export const CLOCK_INTERVAL_MS = 40;

interface ActiveClock {
  readonly observedAt: number;
  readonly remainderMs: number;
}

export function createClock(now: number): ActiveClock {
  return { observedAt: now, remainderMs: 0 };
}

export function elapsedStep(clock: ActiveClock, now: number, active: boolean) {
  const delta = now - clock.observedAt;
  if (!Number.isFinite(delta) || delta < 0) {
    return { clock: createClock(now), milliseconds: 0 };
  }
  if (!active) return { clock: { ...clock, observedAt: now }, milliseconds: 0 };
  const total = clock.remainderMs + delta;
  const milliseconds = Math.floor(total);
  if (!Number.isSafeInteger(milliseconds)) return { clock: createClock(now), milliseconds: 0 };
  return { clock: { observedAt: now, remainderMs: total - milliseconds }, milliseconds };
}
