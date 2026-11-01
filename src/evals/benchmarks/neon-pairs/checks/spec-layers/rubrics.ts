import type { Clause, LayerId } from "./requirements.ts";
import { LayerId as Layer, supplied } from "./requirements.ts";
import { Versions } from "./catalog.ts";
import { LayerTwoCapReview } from "./cap-policy.ts";
import { reviewReason } from "./review-policy.ts";
import { Rules } from "./rules.ts";

interface Question { readonly id: string; readonly area: string; readonly evidence: string; readonly question: string; readonly clauses: readonly string[] }
const question = (id: string, area: string, evidence: string, text: string, clauses: readonly string[] = []): Question => ({ id, area, evidence, question: `${text} Answer yes, no or unsure and cite file lines or image IDs. Use unsure when the evidence cannot answer it.`, clauses });
export function rubric(layer: LayerId, inventory: readonly Clause[]) {
  const review = supplied(inventory, layer).filter((clause) => reviewReason(clause, layer) !== undefined);
  const ids = (prefixes: readonly string[]): string[] => review.filter((clause) => prefixes.some((prefix) => clause.id === prefix || clause.id.startsWith(`${prefix}.`))).map((clause) => clause.id);
  const questions = layer === Layer.two ? [
    question("screen-reader", "accessibility", "source", "Does recorded evidence show a keyboard and screen reader finish, named controls, closed positions, revealed faces, one result per turn, win moves and time, leaving the board and starting again, with browser and screen reader versions?", ids(["I17"])),
    question("face-identity", "visual", "source-and-images", "Do shape, marker and fill identify faces, with color repeating marker position?", ids(["V11"])),
    question("grayscale", "visual", "images", "Can you distinguish all 128 identities at 44px in grayscale, including solid and outline squares with diagonal markers?", ids(["V12.7"])),
    question("offline-scope", "delivery", "source", "Does the game avoid services, accounts, external assets, analytics and API keys, allow network only for dependency installation and record that authoring servers and watchers stopped?", ids(["T10.2", "T10.3", "T07.9"])),
    question("locality", "code", "source", "Does each behavior keep its code, styles and tests together where practical?"),
    question("findability", "code", "source", "Can a developer find matching, timing, focus and saved scores from names and direct calls?"),
    question("types", "code", "source", "Do useful types and narrowing describe game states and saved data?"),
    question("complexity", "code", "source", "Does the implementation avoid abstractions and dependencies that add work without helping this game?"),
    question("neon", "visual", "images", "Do the board images show the specified dark neon appearance with clear controls and status?"),
    question("legibility", "visual", "source-and-images", "Are the board, focus outlines, controls and win summary readable at the supplied phone and desktop sizes?"),
    question("counter-caps", "rules", "source", `Does source keep elapsed time at ${Rules.elapsedMaximumMs}ms and moves at ${Rules.movesMaximum}, while matching, mismatch closing and victory continue at those caps? Layer 2 can't import boundary states and a seconds-only clock can't show the exact stored millisecond limit.`, ids(Object.keys(LayerTwoCapReview))),
  ] : [
    question("pure-core", "architecture", "source", "Are generation, transitions, logical time, snapshot validation and replay pure in src/game, with direct inputs and no DOM, storage, external randomness or browser clock?", ids(["A01"])),
    question("state", "architecture", "source", "Are board and deck immutable, progress represented by the documented state and formats, victory and pairs derived, and closed, one and mismatch branches direct and exhaustive with nearby named constants?", ids(["A02", "A03"])),
    question("lifecycle", "architecture", "source", "Does src/game/session handle React state, monotonic time, visibility and pending-work cleanup with callback invalidation on deal changes?", ids(["A04"])),
    question("storage", "architecture", "source", "Does the session storage module validate unknown saved JSON, sizes and finite integer scores, catch failures, keep unrelated keys and hold active progress in memory until import or export?", ids(["A05", "A06"])),
    question("components", "architecture", "source", "Does Game.tsx compose status, board, controls and reproduction, with board navigation, control and session behavior in their specified folders, nearby props and styles, native input and board-local focus?", ids(["A07", "A09"])),
    question("tests-and-change", "architecture", "source", "Are focused tests beside behavior and journeys at application level, with independent vectors, rule transitions, disk and served storage tests, and changes made at their behavior without speculative shared support, global stores, buses, routers or generic utility folders?", ids(["A10", "A12"])),
    question("exact-types", "code", "source", "Are data shapes exact, unknown inputs validated, unions derived from named values and narrowing used instead of any, suppressed errors or unchecked casts?", ids(["T05"])),
    question("findability", "code", "source", "Are code, styles and tests together, files near the 100-line aim, growing files split by behavior and imports direct from definitions?", ids(["T06"])),
    question("tested-toolchain", "delivery", "source", "Are locked install, build, unit and browser results recorded with exact versions before calling a combination tested, with separate results for other versions and evidence that authoring servers and watchers stopped?", ids(["T03", "T07.9"])),
    question("dependencies", "delivery", "source", "Are dependencies kept in development and build tooling where practical, with no service, account, external asset, analytics or API key and network allowed for installation?", ids(["T10"])),
    question("screen-reader", "accessibility", "source", "Does the recorded screen reader journey cover every I17 step and give exact screen reader and browser versions?", ids(["I17"])),
    question("face-legibility", "visual", "source-and-images", "Do shape, marker and fill define identity, color repeat marker distinction and all 128 faces remain distinct at 44px in grayscale, including outlined and solid squares with diagonal markers?", ids(["V11", "V12.7"])),
  ];
  return { schemaVersion: 1, specVersion: 2, version: Versions[layer], ballots: 3, answers: ["yes", "no", "unsure"], questions };
}
