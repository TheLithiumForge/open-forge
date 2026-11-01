export const DealControl = { size: 'Grid size', newGame: 'New Game', restart: 'Restart' } as const;
export const ReproductionControl = {
  summary: 'Reproduce a game',
  board: 'Board code',
  loadBoard: 'Load board',
  snapshot: 'Snapshot JSON',
  exportSnapshot: 'Export snapshot',
  loadSnapshot: 'Load snapshot',
  resume: 'Resume',
} as const;
export const BOARD_CODE_MAX_CHARACTERS = 32;
export const SNAPSHOT_MAX_CHARACTERS = 65_536;
