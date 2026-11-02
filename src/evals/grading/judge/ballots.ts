import type { JudgeAnswer } from "../../history/results.types.ts";
import { record } from "../../benchmarks/neon-pairs/checks/layer.ts";
import { boundCitation, type JudgeEvidence } from "./evidence.ts";
import { BallotCount, JudgeEvidenceKind, JudgeVerdict, type JudgeQuestion } from "./rubric.ts";

export interface Ballot { readonly id: string; readonly answer: JudgeVerdict; readonly evidence: string; readonly citations: readonly string[] }

export function parseBallot(value: unknown, questions: readonly JudgeQuestion[], evidence: JudgeEvidence): Ballot[] {
  if (!record(value) || !Array.isArray(value["answers"]) || value["answers"].length !== questions.length) throw new Error("Judge returned an incomplete ballot.");
  const seen = new Set<string>();
  return value["answers"].map((item: unknown): Ballot => {
    if (!record(item) || typeof item["id"] !== "string" || !questions.some((question) => question.id === item["id"]) || seen.has(item["id"]) || typeof item["evidence"] !== "string" || !item["evidence"].trim() || !Array.isArray(item["citations"])) throw new Error("Judge returned an invalid answer.");
    const answer = item["answer"];
    if (answer !== JudgeVerdict.yes && answer !== JudgeVerdict.no && answer !== JudgeVerdict.unsure) throw new Error("Judge returned an invalid verdict.");
    if (!item["citations"].length) throw new Error(`Judge gave no citation for ${item["id"]}.`);
    const citations = item["citations"].map((citation: unknown) => {
      const bound = typeof citation === "string" ? boundCitation(citation, evidence) : null;
      if (bound === null) throw new Error(`Judge cited evidence outside its bundle: ${JSON.stringify(citation).slice(0, 200)}.`);
      return bound;
    });
    const question = questions.find((entry) => entry.id === item["id"]);
    if (answer !== JudgeVerdict.unsure && question?.evidence !== JudgeEvidenceKind.source && !citations.some((citation) => evidence.images.some((image) => image.id === citation))) throw new Error("A visual verdict needs image evidence.");
    if (answer !== JudgeVerdict.unsure && question?.evidence !== JudgeEvidenceKind.images && !citations.some((citation) => citation.startsWith("source:"))) throw new Error("A source verdict needs file and line evidence.");
    seen.add(item["id"]);
    return { id: item["id"], answer, evidence: item["evidence"], citations };
  });
}

export function tallyBallots(questions: readonly JudgeQuestion[], ballots: readonly (readonly Ballot[])[]): JudgeAnswer[] {
  if (ballots.length !== BallotCount) throw new Error("L1-v2 needs three judge ballots.");
  return questions.map((question) => {
    const votes = ballots.map((ballot) => ballot.find((answer) => answer.id === question.id));
    if (votes.some((vote) => vote === undefined)) throw new Error("Judge ballot omitted a question.");
    return { id: question.id, area: question.area, question: question.question,
      yes: votes.filter((vote) => vote?.answer === JudgeVerdict.yes).length,
      no: votes.filter((vote) => vote?.answer === JudgeVerdict.no).length,
      unsure: votes.filter((vote) => vote?.answer === JudgeVerdict.unsure).length,
      evidence: votes.flatMap((vote) => vote ? [`${vote.evidence} [${vote.citations.join(", ")}]`] : []),
    };
  });
}
