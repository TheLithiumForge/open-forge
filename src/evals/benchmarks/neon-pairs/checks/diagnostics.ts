import { mkdir, writeFile } from "node:fs/promises";
import { join } from "node:path";
import type { Page } from "playwright";
import { colorDistance, type FaceSignature, looksLikeBack, meanDistance, shapeShare } from "./face-signature.ts";
import { capturedPng, sameBackImage } from "./turn-image.ts";

export const DiagnosticEnvironment = "EVALS_L1_DIAGNOSTICS";
export const DiagnosticEnabled = "1";
export const DiagnosticFolder = "diagnostics";
export const DiagnosticSteps = 400;
export const DiagnosticCards = 16;
export const DiagnosticKind = { board: "board", face: "face", backs: "backs" } as const;
export type DiagnosticKind = (typeof DiagnosticKind)[keyof typeof DiagnosticKind];
export const DiagnosticFile = new RegExp(`^step-\\d{4}-(${Object.values(DiagnosticKind).join("|")})(-card-\\d+\\.png|\\.json)$`);
let output: string | null = null;
let step = 0;
let pages = new WeakMap<Page, number>();
let pageCount = 0;

export function startDiagnostics(folder: string): void {
  output = process.env[DiagnosticEnvironment] === DiagnosticEnabled ? join(folder, DiagnosticFolder) : null;
  step = 0;
  pages = new WeakMap();
  pageCount = 0;
}

/** Keeps small-board crops and comparisons apart from the judge's screenshots. */
export async function diagnose(page: Page, kind: DiagnosticKind, looks: readonly FaceSignature[], backs: readonly FaceSignature[], indexes = looks.map((_, index) => index)): Promise<void> {
  if (output === null || step >= DiagnosticSteps || looks.length > DiagnosticCards) return;
  const name = `step-${String(step++).padStart(4, "0")}-${kind}`;
  if (!pages.has(page)) pages.set(page, pageCount++);
  await mkdir(output, { recursive: true });
  const comparisons = [];
  for (const [at, look] of looks.entries()) {
    const index = indexes[at] ?? at;
    const back = backs[at];
    const png = capturedPng(look);
    const image = `${name}-card-${index}.png`;
    if (png) await writeFile(join(output, image), png);
    comparisons.push({ index, image: png ? image : null, back: back ? looksLikeBack(look, back) : null,
      colors: back ? colorDistance(look, back) : null, shape: back ? shapeShare(look, back) : null,
      mean: back ? meanDistance(look, back) : null, pixels: back ? sameBackImage(look, back) : null });
  }
  await writeFile(join(output, `${name}.json`), JSON.stringify({ page: pages.get(page), kind, comparisons }));
}
