export const CapClause = {
  movesLimit: "R05.5", movesPlay: "R05.6", movesMatch: "R05.7", movesWin: "R05.8",
  elapsedLimit: "R07.9", elapsedPlay: "R07.10", elapsedDisplay: "R07.11",
  snapshotElapsed: "D07.7", snapshotMoves: "D07.8", savedBest: "D07.9",
} as const;
export const NewCapClauses: readonly string[] = Object.values(CapClause);
export const LayerThreeCaps = [CapClause.movesLimit, CapClause.movesPlay, CapClause.movesMatch, CapClause.movesWin, CapClause.elapsedLimit] as const;
