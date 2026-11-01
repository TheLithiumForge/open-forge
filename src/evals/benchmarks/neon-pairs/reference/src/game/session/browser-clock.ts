export const CLOCK_INTERVAL_MS = 40;

interface ActiveClock {
  readonly observedAt: number;
}

export function createClock(now: number): ActiveClock {
  return { observedAt: now };
}

// Fractions stay in the game, so a mismatch runs from its own activation and only exports round.
export function elapsedStep(clock: ActiveClock, now: number, active: boolean) {
  const delta = now - clock.observedAt;
  const milliseconds = active && Number.isFinite(delta) && delta > 0 ? delta : 0;
  return { clock: createClock(now), milliseconds };
}
