import { readFile } from "node:fs/promises";
import { LayerFiles, LayerVersion, record } from "../../benchmarks/neon-pairs/checks/layer.ts";

export const JudgeVerdict = { yes: "yes", no: "no", unsure: "unsure" } as const;
export type JudgeVerdict = (typeof JudgeVerdict)[keyof typeof JudgeVerdict];
export const JudgeEvidenceKind = { source: "source", images: "images", both: "source-and-images" } as const;
export type JudgeEvidenceKind = (typeof JudgeEvidenceKind)[keyof typeof JudgeEvidenceKind];
export const BallotCount = 3;

export interface JudgeQuestion {
  readonly id: string;
  readonly area: string;
  readonly evidence: JudgeEvidenceKind;
  readonly question: string;
}

function evidenceKind(value: unknown): JudgeEvidenceKind {
  switch (value) {
    case JudgeEvidenceKind.source: case JudgeEvidenceKind.images: case JudgeEvidenceKind.both: return value;
    default: throw new Error("Invalid judge evidence kind.");
  }
}

export async function frozenQuestions(): Promise<JudgeQuestion[]> {
  const value: unknown = JSON.parse(await readFile(new URL(`../../benchmarks/neon-pairs/${LayerFiles.rubric}`, import.meta.url), "utf8"));
  if (!record(value) || value["version"] !== LayerVersion || value["ballots"] !== BallotCount || !Array.isArray(value["questions"])) throw new Error("Invalid L1-v2 rubric.");
  const questions = value["questions"].map((question: unknown): JudgeQuestion => {
    if (!record(question) || typeof question["id"] !== "string" || typeof question["area"] !== "string" || typeof question["question"] !== "string") throw new Error("Invalid L1-v2 question.");
    return { id: question["id"], area: question["area"], evidence: evidenceKind(question["evidence"]), question: question["question"] };
  });
  if (new Set(questions.map((question) => question.id)).size !== questions.length) throw new Error("Duplicate L1-v2 question.");
  return questions;
}
