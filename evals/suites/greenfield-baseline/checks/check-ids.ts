import type { CheckResult } from "../../../src/results/results.types.ts";

/** Every hidden check of Neon Pairs, by the area of the prompt it covers. */
export const CheckArea = {
  delivery: "Delivery",
  stack: "Stack",
  grid: "Grid sizes",
  faces: "Faces",
  matching: "Matching",
  input: "Input",
  moves: "Moves",
  newGame: "New Game",
  win: "Win",
  best: "Best score",
  phone: "Phone",
  keyboard: "Keyboard",
  intent: "Intent",
} as const;

export const CheckId = {
  singleFile: "delivery-single-file",
  reactTypeScript: "stack-react-typescript",
  sizesOffered: "grid-4-and-16",
  sizesFinishable: "grid-sizes-pair-up",
  facesDrawn: "faces-drawn",
  facesDistinct: "faces-distinct-16",
  matching: "matching-pairs",
  sameCardTwice: "input-same-card-twice",
  clicksDuringDelay: "input-clicks-during-delay",
  moves: "moves-count",
  movesCompared: "moves-compared",
  newGame: "new-game-resets",
  win: "win-shown",
  best: "best-score",
  phone: "phone-full-game",
  keyboard: "keyboard-full-game",
  startSize: "start-4x4",
  rememberSize: "remember-size",
  noSound: "no-sound",
} as const;

export function result(id: string, area: string, passed: boolean | null, detail: string): CheckResult {
  return { id, area, passed, detail };
}

/** Runs one check. A check that throws fails with the error, instead of stopping the others. */
export async function guarded(id: string, area: string, check: () => Promise<CheckResult[]>): Promise<CheckResult[]> {
  try {
    return await check();
  } catch (error) {
    return [result(id, area, false, `The check stopped: ${error instanceof Error ? error.message.split("\n")[0] : String(error)}`)];
  }
}

/**
 * Runs a group of checks again when one fails, since a busy machine can miss a short flip-back delay.
 * A pass on the second try says so, so people can still see an intermittent game bug.
 */
export async function retriedOnce(group: () => Promise<CheckResult[]>): Promise<CheckResult[]> {
  const first = await group();
  if (first.every((check) => check.passed !== false)) return first;
  const second = await group();
  return second.map((check) => {
    const earlier = first.find((other) => other.id === check.id);
    return check.passed === true && earlier?.passed === false ? { ...check, detail: `${check.detail} Passed on the second try, after: ${earlier.detail}` } : check;
  });
}
