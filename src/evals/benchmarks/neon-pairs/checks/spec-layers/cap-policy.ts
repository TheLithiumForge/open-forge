export const CapClause = {
  movesLimit: "R05.5", movesPlay: "R05.6", movesMatch: "R05.7", movesWin: "R05.8",
  elapsedLimit: "R07.9", elapsedPlay: "R07.10", elapsedDisplay: "R07.11",
  snapshotElapsed: "D07.7", snapshotMoves: "D07.8", savedBest: "D07.9",
} as const;
export const NewCapClauses: readonly string[] = Object.values(CapClause);
const MoveReview = "Layer 2 has no snapshot import. Reaching 999999 moves needs nearly two million card activations. Review the cap and continued play in source instead of adding a private state hook.";
export const LayerTwoCapReview: Readonly<Record<string, string>> = {
  [CapClause.movesLimit]: MoveReview, [CapClause.movesPlay]: MoveReview,
  [CapClause.movesMatch]: MoveReview, [CapClause.movesWin]: MoveReview,
  [CapClause.elapsedLimit]: "Layer 2 can reach the displayed time cap with a controlled clock, but its display can round to seconds. Review the exact stored millisecond limit without requiring snapshot controls.",
};
