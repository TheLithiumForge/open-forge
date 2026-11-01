import type { CheckOutcome } from "../../../grading/grading.types.ts";
import { CheckStatus } from "../../../history/statuses.ts";

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

export function result(id: string, area: string, passed: boolean | null, evidence: string): CheckOutcome {
  return { id, area, status: passed === null ? CheckStatus.blocked : passed ? CheckStatus.pass : CheckStatus.fail, evidence };
}

export class GameFailure extends Error {}

/** A checker exception stays separate from a game that fails its check. */
export async function guarded(id: string, area: string, check: () => Promise<CheckOutcome[]>): Promise<CheckOutcome[]> {
  try {
    return await check();
  } catch (error) {
    return [{ id, area, status: error instanceof GameFailure ? CheckStatus.fail : CheckStatus.error,
      evidence: `The check stopped: ${error instanceof Error ? error.message.split("\n")[0] : String(error)}` }];
  }
}

export function withPageErrors(checks: readonly CheckOutcome[], errors: readonly string[]): CheckOutcome[] {
  return checks.map((check) => errors.length && check.status === CheckStatus.pass
    ? { ...check, status: CheckStatus.fail, evidence: `${check.evidence} Uncaught page errors: ${errors.join(" | ")}` }
    : check);
}
