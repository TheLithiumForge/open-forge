# ADR 032: Compare best moves within each size

This record explains per-size best move records and why elapsed time is not a competing score.

## Context

A 4x4 game has eight pairs; a 16x16 game has 128. A single best move count across both would mostly reward choosing the smaller board. The interface needs a record that means something when the player changes size, without introducing a formula whose interpretation is harder than the game itself.

The decision is one best move count per supported side length. Completion compares the final moves with the saved entry for that size. A strictly lower count replaces it; an equal or worse count retains the existing entry. Before there is an entry, the interface shows `Best: none`.

## Data and transition

`Preferences.bestMoves` is a readonly record keyed by canonical side-length strings. `rememberBest` obtains `String(size)`, compares the existing number and returns either the original preferences object or a new map with one changed entry. The equality case is deliberately a no-op rather than a rewrite with identical data.

`activateSession` calls that helper when a player action completes the board. The pure game core knows how to recognize a win, but does not write a browser record. Storage remains an adapter concern. This allows a game transition to be replayed or validated without requiring a local storage object.

`GameStatus` reads the entry for the active board's size. Loading a different supported board therefore changes which record is visible without merging the sizes. [Best score records](best-score-records.md) gives the operational details and [ADR 011](adr-011-storage-keys.md) explains the original key's history.

## Why moves alone

The task is to finish in as few moves as possible. Elapsed time remains useful feedback, but it does not break ties between equal move counts or become a separate stored best in the current model. A fast player and a deliberate player can compare their own move efficiency without the stored record pressuring speed.

A weighted score combining time and moves would have needed a new formula and an explanation of why one second was worth some fraction of a turn. A percentage-of-perfect score could compare sizes, but it would obscure the direct count a player already sees. Neither was needed for the local record behavior.

## Validation

Saved scores are untrusted JSON. The parser accepts only canonical supported size keys and integer counts within the move cap. A stored best also cannot be lower than the board's pair count, because even perfect play needs one move per pair. A 4x4 score below eight is therefore rejected as part of an invalid preference value.

These checks establish a plausible stored record, not a verified history of play. The game is local and has no account authority. The preference reader's fallback makes corrupted data a persistence problem rather than a reason to prevent play. [Corrupt data policy](corrupt-data-policy.md) covers that boundary.

## Costs and evidence

Seven records can exist instead of one, but the map is tiny and the current size's entry is simple to display. Starting another deal or restarting continues to use the same preferences model. A page reload starts a fresh deal while retaining usable local preferences.

Focused tests cover better, equal and worse results and the storage validator's bounds. Browser tests complete games, reload and verify the selected size's displayed record. That combination checks the comparison itself and its persistence path. [ADR 030](adr-030-move-cap.md), [Scoring and moves](scoring-and-moves.md) and [Persistence overview](persistence-overview.md) are the related references.

## The comparison itself

The helper returns the old object for a tie or worse result. The surrounding session, not this function, decides when a completed result is eligible for comparison.

```ts
export function rememberBest(preferences: Preferences, size: number, moves: number): Preferences {
  const previous = preferences.bestMoves[String(size)];
  if (previous !== undefined && previous <= moves) return preferences;
  return { ...preferences, bestMoves: { ...preferences.bestMoves, [size]: moves } };
}
```
