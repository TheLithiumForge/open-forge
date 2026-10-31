import type { Locator, Page } from "playwright";

// What the game shows as text. The checks read it the way a player would, never from the game's state.
const movesPatterns = [/moves?\s*[:\-–]?\s*(\d+)/i, /(\d+)\s*moves?/i, /turns?\s*[:\-–]?\s*(\d+)/i];
// A best score sits on its label's line, as in "Best: 12", or alone on the next line under a label such
// as "PERSONAL BEST". Lines with words after "best" are descriptions, such as "A personal best for every grid".
const bestLabel = /best/i;
const notScores = /\d+\s*(?:[x×]|by)\s*\d+|\d+\s*(?:cards|pairs)/gi;
const scorePattern = /\d+(?:[.:]\d+)?/;
const valueOnly = /^[^\p{L}]{1,12}$/u;
const noScoreYet = /^[—–-]+$/;
const winPattern = /\b(you win|you won|winner|won|victory|complete|completed|congrat\w*|solved|well done|cleared|all pairs)\b/i;
// Names that start with the action come first, so a size button that "starts a new game" isn't taken for it.
const newGamePatterns = [/^\W*new game/i, /^\W*(restart|reset|play again|new round|shuffle)/i, /new game/i, /restart|reset|play again|new round|shuffle/i];

export function pageText(page: Page): Promise<string> {
  return page.evaluate(() => document.body.innerText);
}

export function movesIn(text: string): number | null {
  for (const pattern of movesPatterns) {
    const match = pattern.exec(text);
    if (match?.[1] !== undefined) return Number(match[1]);
  }
  return null;
}

/** The best score as shown, compared as text, so a time or a move count both work. */
export function bestIn(text: string): string | null {
  const lines = text.split(/\r?\n/).map((line) => line.trim()).filter((line) => line !== "");
  for (const [index, line] of lines.entries()) {
    const at = line.search(bestLabel);
    if (at < 0) continue;
    const sameLine = scorePattern.exec(line.slice(at).replace(notScores, " "))?.[0];
    if (sameLine !== undefined) return sameLine;
    const next = lines[index + 1] ?? "";
    if (!valueOnly.test(next)) continue;
    const score = scorePattern.exec(next)?.[0];
    if (score !== undefined) return score;
    // A dash means no best score yet. Other symbols, such as an icon line, belong to something else.
    if (noScoreYet.test(next)) return null;
  }
  return null;
}

export function showsWin(text: string): boolean {
  return winPattern.test(text);
}

export async function newGameButton(page: Page): Promise<Locator | null> {
  for (const name of newGamePatterns) {
    const button = page.getByRole("button", { name });
    if ((await button.count()) > 0) return button.first();
  }
  return null;
}
