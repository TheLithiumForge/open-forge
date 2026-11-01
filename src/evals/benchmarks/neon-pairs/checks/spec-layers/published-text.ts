export const StatusClause = { moves: "R05.9", pairs: "R06.2", clock: "R07.12", best: "R10.6", emptyBest: "R10.7" } as const;
export const AnnouncementClause = { matches: "I16.6", miss: "I16.7", win: "I16.8", loads: "I16.9" } as const;
export const LoadNotice = { unfinished: "Loaded snapshot. Select Resume to continue.", completed: "Loaded completed snapshot.", boardError: "Invalid board code.", snapshotError: "Invalid snapshot." } as const;
export function clockText(milliseconds: number): string {
  const seconds = Math.floor(milliseconds / 1000);
  return `${Math.floor(seconds / 60)}:${String(seconds % 60).padStart(2, "0")}`;
}
export function hasStatus(text: string, label: "Moves" | "Pairs" | "Time" | "Best", value: string | number): boolean {
  const escaped = String(value).replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
  return new RegExp(`(?:^|\\s)${label === "Moves" ? "(?<!Best\\s)" : ""}${label}:\\s*${escaped}(?=\\s|$|[.!])`).test(text);
}
export const Announcement = {
  miss: "No match. Cards close after a short pause.",
  match: (pairs: number): string => `Pair found. ${pairs} ${pairs === 1 ? "pair" : "pairs"} matched.`,
  win: (moves: number, elapsedMs: number): string => `Pair found. All pairs found in ${moves} moves and ${clockText(elapsedMs)}.`,
} as const;
