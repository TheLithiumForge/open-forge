# ADR 009: Identify a deal with a board code

This record explains why a compact versioned code identifies the initial deck separately from progress.

## Context

A report such as “the second card looked wrong” is hard to investigate when a fresh shuffle changes every position. Restart also needs to mean something more precise than making another random board at the same size. The project needed a stable description of a deal that a person could copy without carrying a full list of face IDs.

The chosen code is `np1-N-seed`, with a supported side length and a decimal seed from 1 through 2147483646. `np1-4-1` is a concrete example. The prefix fixes the generation algorithm, not the product release number. Spec version 2.2 still uses this generation version.

## Decision

A board code names the starting arrangement. It contains no moves, elapsed time, matches or selected cards. `createGame` generates that arrangement and adds empty progress. Restart calls `createGame` with the current code. New Game obtains a different seed before creating its state. These two actions can therefore share construction without sharing meaning.

The parser accepts canonical decimal syntax: no leading zeros, no signs and no alternate separators. The browser control trims surrounding whitespace before parsing, while the core parser itself expects the exact code. This separation allows forgiving copy and paste at the input boundary without broadening the format accepted inside snapshots or replay files.

The seed and size regenerate a frozen board description and deck. No face list needs to be serialized into the code. That keeps the identifier short and lets independent tools reconstruct the deal using the documented algorithm. [ADR 021](adr-021-seeded-decks.md) explains the shuffle; [Board code format](board-code-format.md) gives the parser details.

## Why not a URL or deck dump

A shareable URL would tie the identifier to an origin and route scheme. The playable distribution can open from disk and has no router, so a plain text code fits its delivery model better. An application can be served from many locations without changing the meaning of the deal.

A complete deck dump would be explicit but cumbersome to enter or quote. It would also create another source of disagreement if supplied alongside a seed. The current formats derive the deck from the code and validate progress against it. A photograph might show artwork, but it would not establish a canonical arrangement or allow replay to apply actions deterministically.

## Versioning cost

The compact representation is only useful if its generation contract stays stable. A seemingly harmless change to draw order, rejection sampling or the shuffle loop can change all downstream positions. That is why tests include independent literal vectors rather than merely checking that two calls to the same implementation agree.

An algorithm change would need a different generation version. A visual correction that preserves face identity is a separate question: the code specifies face IDs and order, while the design table specifies how those identities look and are named. [Format versioning guide](format-versioning-guide.md) discusses this distinction.

## Limits

A code is a reproducibility tool, not a claim about how somebody played. It is easy to inspect or generate its deal outside the game. The shipped UI still keeps closed face identity out of the DOM, and tests use documented formats rather than a solver endpoint. A snapshot answers the different question of progress on a deal; see [ADR 010](adr-010-snapshot-json.md). A replay records actions instead; see [ADR 015](adr-015-replay-cli.md).

## Canonical parsing and construction

The core returns null for an invalid code and the constructor checks its own output through the same parser.

```ts
export function parseBoardCode(code: string): { size: number; seed: number } | null {
  const parts = new RegExp(`^${BOARD_VERSION}-([1-9]\\d*)-([1-9]\\d*)$`).exec(code);
  if (!parts) return null;
  const size = Number(parts[1]);
  const seed = Number(parts[2]);
  return isValidSize(size) && Number.isSafeInteger(seed) && seed <= SEED_MAX
    ? { size, seed }
    : null;
}

export function boardCode(size: number, seed: number): string {
  const code = `${BOARD_VERSION}-${size}-${seed}`;
  if (!parseBoardCode(code)) throw new Error('Invalid board code.');
  return code;
}

```
