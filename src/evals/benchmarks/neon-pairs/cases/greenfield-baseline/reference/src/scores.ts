const bestKey = "neon-pairs:best";
const sizeKey = "neon-pairs:size";

type BestScores = Record<string, number>;

/** Saved data can be missing or broken. Both read as no score yet. */
export function loadBest(): BestScores {
  try {
    const value: unknown = JSON.parse(localStorage.getItem(bestKey) ?? "{}");
    return typeof value === "object" && value !== null ? (value as BestScores) : {};
  } catch {
    return {};
  }
}

export function saveBest(size: number, moves: number): BestScores {
  const best = loadBest();
  const previous = best[size];
  const next = previous === undefined || moves < previous ? { ...best, [size]: moves } : best;
  try {
    localStorage.setItem(bestKey, JSON.stringify(next));
  } catch {
    // Storage can be full or blocked. The score still shows for this visit.
  }
  return next;
}

export function loadSize(sizes: readonly number[]): number {
  const saved = Number(localStorage.getItem(sizeKey));
  return sizes.includes(saved) ? saved : sizes[0]!;
}

export function saveSize(size: number): void {
  try {
    localStorage.setItem(sizeKey, String(size));
  } catch {
    // Not remembering the size is fine.
  }
}
