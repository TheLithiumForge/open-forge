import type { Page } from "playwright";
import type { Activate } from "./activation.ts";
import { boardLooks, waitForBacks, waitForFace, Waits } from "./board.ts";
import { faceDistance, type FaceSignature, looksLikeBack, NoFace, sameFace } from "./face-signature.ts";

export interface GameOutcome {
  readonly won: boolean;
  readonly turns: number;
  readonly error: string | null;
}

export interface PlayOptions {
  /** Extra turns spent on known mismatches, to play a clearly worse game. */
  readonly extraMisses: number;
}

const TurnsPerCard = 4;

/** Indexes of the cards that currently show something other than their back. */
async function faceUp(page: Page, backs: readonly FaceSignature[]): Promise<Set<number>> {
  const looks = await boardLooks(page);
  return new Set(backs.map((_, index) => index).filter((index) => !looksLikeBack(looks[index] ?? NoFace, backs[index] ?? NoFace)));
}

/**
 * Plays a game to the end like a careful player: turn a new card, then its known partner or another
 * new card. The game decides what a pair is: cards that stay up were matched, cards that turn back
 * were not, even if they turn back only on the next click.
 */
export async function playGame(page: Page, activate: Activate, backs: readonly FaceSignature[], options: PlayOptions): Promise<GameOutcome> {
  // A card's first look, before any matched style the game adds, is the one compared.
  const known = new Map<number, FaceSignature>();
  let matched = await faceUp(page, backs);
  let misses = options.extraMisses;
  let turns = 0;
  const latest = new Map<number, FaceSignature>();
  const reveal = async (index: number): Promise<boolean> => {
    await activate(index);
    let face = await waitForFace(page, index, backs[index] ?? NoFace);
    if (face === null && activate.fallback !== undefined) {
      await activate.fallback(index);
      face = await waitForFace(page, index, backs[index] ?? NoFace);
    }
    if (face !== null && !known.has(index)) known.set(index, face);
    if (face !== null) latest.set(index, face);
    return face !== null;
  };
  // Pairs the game turned back are never tried again, even when they look alike to the checks.
  const rejected = new Set<string>();
  const pairKey = (a: number, b: number): string => (a < b ? `${a}:${b}` : `${b}:${a}`);
  const open = (): number[] => backs.map((_, index) => index).filter((index) => !matched.has(index));
  const partnerOf = (index: number): number | undefined =>
    open().find((other) => other !== index && known.has(other) && !rejected.has(pairKey(index, other)) && sameFace(known.get(other) ?? NoFace, known.get(index) ?? NoFace));
  // Mismatched cards never carry a matched style, so their latest look replaces an older, noisier one.
  const finishTurn = async (a: number, b: number): Promise<void> => {
    const alike = faceDistance(known.get(a) ?? NoFace, known.get(b) ?? NoFace) <= 1;
    if (!(await waitForBacks(page, [a, b], backs, alike ? Waits.settle : Waits.hide))) {
      matched.add(a).add(b);
      return;
    }
    for (const index of [a, b]) known.set(index, latest.get(index) ?? known.get(index) ?? NoFace);
    rejected.add(pairKey(a, b));
  };
  const closestTo = (index: number): number | undefined =>
    open().filter((other) => other !== index && known.has(other) && !rejected.has(pairKey(index, other))).sort((x, y) => faceDistance(known.get(index) ?? NoFace, known.get(x) ?? NoFace) - faceDistance(known.get(index) ?? NoFace, known.get(y) ?? NoFace))[0];

  while (matched.size < backs.length && turns < backs.length * TurnsPerCard) {
    turns += 1;
    // Cards counted as matched that turned back were a mismatch the game flipped on the next click.
    const up = await faceUp(page, backs);
    matched = new Set([...matched].filter((index) => up.has(index)));
    const knownOpen = open().filter((index) => known.has(index));
    const fresh = open().filter((index) => !known.has(index));
    let pair: readonly [number, number] | null = null;
    if (misses > 0) {
      const miss = knownOpen.flatMap((a) => knownOpen.filter((b) => b > a && !sameFace(known.get(a) ?? NoFace, known.get(b) ?? NoFace)).map((b) => [a, b] as const))[0];
      if (miss !== undefined) {
        misses -= 1;
        pair = miss;
      }
    }
    pair ??= knownOpen.map((a) => [a, partnerOf(a)] as const).find((candidate): candidate is readonly [number, number] => candidate[1] !== undefined) ?? null;
    if (pair === null && fresh[0] === undefined) pair = closestPair(knownOpen, known, (a, b) => rejected.has(pairKey(a, b)));
    if (pair === null) {
      const first = fresh[0];
      if (first === undefined) return { won: false, turns, error: "Cards are left that have no partner." };
      if (!(await reveal(first))) return { won: false, turns, error: "A card didn't turn over." };
      const second = partnerOf(first) ?? fresh[1] ?? closestTo(first);
      if (second === undefined) return { won: false, turns, error: "A card is left with no partner." };
      pair = [first, second];
      if (!(await reveal(second))) return { won: false, turns, error: "A card didn't turn over." };
    } else if (!(await reveal(pair[0])) || !(await reveal(pair[1]))) {
      return { won: false, turns, error: "A card didn't turn over." };
    }
    await finishTurn(pair[0], pair[1]);
  }
  const won = matched.size === backs.length;
  return { won, turns, error: won ? null : "The game didn't finish in time." };
}

function closestPair(cards: readonly number[], known: ReadonlyMap<number, FaceSignature>, isRejected: (a: number, b: number) => boolean): readonly [number, number] | null {
  let best: readonly [number, number] | null = null;
  let bestDistance = Number.POSITIVE_INFINITY;
  for (const [position, a] of cards.entries()) {
    for (const b of cards.slice(position + 1)) {
      if (isRejected(a, b)) continue;
      const distance = faceDistance(known.get(a) ?? NoFace, known.get(b) ?? NoFace);
      if (distance < bestDistance) [best, bestDistance] = [[a, b], distance];
    }
  }
  return best;
}

/**
 * Turns every card once, two at a time, and returns each card's face. When the game keeps a pair up,
 * the two are twins, whatever matched style the second one shows. A pair still up after the next
 * click was matched. A pair that turned back then was a mismatch the game flips on the next click.
 */
export async function learnFaces(page: Page, activate: Activate, backs: readonly FaceSignature[]): Promise<FaceSignature[]> {
  const faces: FaceSignature[] = [];
  let pending: readonly [number, number] | null = null;
  const settlePending = async (): Promise<void> => {
    if (pending === null) return;
    const up = await faceUp(page, backs);
    if (up.has(pending[0]) && up.has(pending[1])) faces[pending[1]] = faces[pending[0]] ?? NoFace;
    pending = null;
  };
  for (let index = 0; index + 1 < backs.length; index += 2) {
    await activate(index);
    faces[index] = (await waitForFace(page, index, backs[index] ?? NoFace)) ?? NoFace;
    await settlePending();
    await activate(index + 1);
    faces[index + 1] = (await waitForFace(page, index + 1, backs[index + 1] ?? NoFace)) ?? NoFace;
    const alike = sameFace(faces[index] ?? NoFace, faces[index + 1] ?? NoFace);
    if (!(await waitForBacks(page, [index, index + 1], backs, alike ? Waits.settle : Waits.hide))) pending = [index, index + 1];
  }
  await settlePending();
  return faces;
}
