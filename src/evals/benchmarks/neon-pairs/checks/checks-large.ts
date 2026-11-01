import type { Browser, Page } from "playwright";
import type { PerformanceResult } from "../../../history/results.types.ts";
import type { CheckOutcome } from "../../../grading/grading.types.ts";
import { clicking, frameTimings } from "./activation.ts";
import { CardAttribute } from "./board.ts";
import { CheckArea, CheckId, guarded, result, withPageErrors } from "./check-ids.ts";
import { faceDistance, type FaceSignature } from "./face-signature.ts";
import { offeredSizes, setGridSize } from "./grid-size.ts";
import { Device, openGame, readBoard } from "./open-game.ts";
import { learnFaces } from "./solver.ts";
import { saveFaceSheet } from "./face-sheet.ts";

const LargeSize = 16;
const P95 = 0.95;

export interface LargeBoardResult {
  readonly checks: CheckOutcome[];
  readonly performance: PerformanceResult;
  readonly shots: readonly string[];
}

function percentile(values: readonly number[], share: number): number | null {
  if (values.length === 0) return null;
  const sorted = [...values].sort((a, b) => a - b);
  return sorted[Math.min(sorted.length - 1, Math.floor(sorted.length * share))] ?? null;
}

// Two cards this close are the same render. Different faces, even ones that differ only by a small
// mark, sit well above it, while screenshot noise between twins never makes two strangers this close.
const IdenticalFace = 0.08;
// A few cards can fall inside that noise when faces differ only by a tiny mark. A repeated set of faces
// puts most cards over it, so the check fails above this share.
const SharedFaceShare = 0.05;

/**
 * Cards drawn exactly like two or more others, as when two pairs share one face. Whether similar
 * faces are easy to tell apart is for people and the judge, since screenshots can't settle it.
 */
function cardsSharingAFace(faces: readonly FaceSignature[]): number {
  return faces.filter((face, index) => faces.filter((other, position) => position !== index && faceDistance(face, other) <= IdenticalFace).length >= 2).length;
}

/** Cards that show an image file or an emoji instead of a shape drawn by code. */
function imageOrEmojiCards(page: Page): Promise<number> {
  return page.evaluate((attribute) => {
    const emoji = /\p{Extended_Pictographic}/u;
    return Array.from(document.querySelectorAll(`[${attribute}]`)).filter((card) => {
      const images = Array.from(card.querySelectorAll("img, image"));
      return images.length > 0 || emoji.test(card.textContent ?? "");
    }).length;
  }, CardAttribute);
}

/** The 16x16 board: every face once per pair, faces drawn by code, how fast it answers, and odd sizes. */
export async function largeBoardChecks(browser: Browser, url: string, firstRenderMs: number | null, shotsFolder: string): Promise<LargeBoardResult> {
  let tapP95: number | null = null;
  const shots: string[] = [];
  const checks = await guarded(CheckId.facesDistinct, CheckArea.faces, async () => {
    const game = await openGame(browser, url, Device.desktop);
    try {
      const sizes = await offeredSizes(game.page);
      const unpaired: number[] = [];
      for (const size of sizes) {
        const changed = await setGridSize(game.page, size);
        if (!changed) unpaired.push(size);
      }
      if (!(await setGridSize(game.page, LargeSize))) return [result(CheckId.facesDistinct, CheckArea.faces, false, `Couldn't set up a ${LargeSize}x${LargeSize} board.`)];
      const board = await readBoard(game.page);
      const frames: Buffer[] = [];
      let drawnElsewhere = 0;
      const faces = await learnFaces(game.page, clicking(game.page), board.backs, async (index, png) => {
        frames[index] = png;
        drawnElsewhere = Math.max(drawnElsewhere, await imageOrEmojiCards(game.page));
      });
      const revealed = faces.length === board.count && faces.every((face) => face.shape.length > 0);
      if (revealed) shots.push(await saveFaceSheet(shotsFolder, frames));
      tapP95 = percentile(await frameTimings(game.page), P95);
      const wrong = cardsSharingAFace(faces);
      return withPageErrors([
        result(CheckId.facesDistinct, CheckArea.faces, revealed && wrong <= faces.length * SharedFaceShare, !revealed ? "Not every card could be revealed through visible play." : wrong === 0 ? `No face among ${faces.length} cards is drawn more than twice.` : `${wrong} of ${faces.length} cards share their face with more than one other card.`),
        result(CheckId.facesDrawn, CheckArea.faces, revealed ? drawnElsewhere === 0 : null, !revealed ? "Face inspection stopped because some cards could not be revealed." : drawnElsewhere === 0 ? "No image files or emoji on the revealed cards." : `${drawnElsewhere} revealed cards show an image file or emoji.`),
        result(CheckId.sizesFinishable, CheckArea.grid, unpaired.length === 0, unpaired.length === 0 ? "Every offered size has its requested square layout and an even card count, with one empty cell on odd boards." : `Incorrect card counts or unavailable boards at: ${unpaired.join(", ")}.`),
      ], game.errors);
    } finally {
      await game.context.close();
    }
  });
  return { checks, shots, performance: { firstRenderMs, largeBoardTapP95Ms: tapP95 } };
}
