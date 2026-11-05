# Neon Pairs 1.4: snapshots

These release notes describe the addition of explicit progress export and import, including the pause that makes a restored position usable before play resumes.

Board codes answered where the cards started. They did not answer which pairs had already been found or how much of a mismatch interval remained. In this release, a long-board playtest and a difficult timing report exposed the same missing tool: a way to carry the current position without treating browser preferences as a hidden save-game system.

Snapshot fields and examples below describe the current version 1 format. The [snapshot-format page](snapshot-format.md) gives the complete reference without the release history.

## Progress became an explicit artifact

The shipped interaction is manual. Open Reproduce a game, select Export snapshot, and copy the readable JSON from Snapshot JSON. To restore it, paste the text and select Load snapshot. The game does not need the Clipboard API or a download permission to make the state available.

The current object contains `version`, `boardCode`, `matched`, `selection`, `moves`, `elapsedMs`, and `started`. The board code reconstructs the immutable deck. Matched indices and selection describe revealed progress. Moves and elapsed milliseconds preserve the attempt's counters. `started` distinguishes a board that has never received its first valid reveal from a started position whose counters may still be zero.

This representation avoided storing the deck twice. If both a code and a separate array were authoritative, an import would need to resolve disagreements between them. Deriving the deck also lets the parser check whether a claimed matched position really belongs to a complete face pair. Derived pair count and completion remain derived after loading.

## Loading did not mean racing immediately

An unfinished import loads paused. The current notice is `Loaded snapshot. Select Resume to continue.` A Resume button appears, and card activation is blocked until it is selected. Focus can still move within the board. The player can inspect where the restored cards are before the clock and a pending mismatch continue.

The early prototype resumed automatically. A copied mismatch with very little time remaining could close while the player was still finding the restored board. That was technically a continuation but practically a lost observation. The explicit pause made the state transport useful for a person as well as for a parser test.

Resume has a focus consequence because its button disappears. In `Game.tsx`, the handler first asks the board to focus its remembered position, then calls the session's resume action. Ordinary new, restart, load, and export controls keep their own focus. The disappearing control therefore has a specific handoff instead of leaving the browser to decide where an absent focused element belongs. The [snapshot pause decision](adr-037-snapshot-pause.md) records the rationale.

## A concrete restored mismatch

This position is consistent with the known seed-one board:

```json
{
  "version": 1,
  "boardCode": "np1-4-1",
  "matched": [0, 11],
  "selection": {
    "kind": "mismatch",
    "first": 1,
    "second": 2,
    "remainingMs": 300
  },
  "moves": 2,
  "elapsedMs": 1500,
  "started": true
}
```

Positions 0 and 11 both hold face 6. Positions 1 and 2 hold different faces, so they are a valid pending miss. Two moves account for the completed pair and the pending attempt. After loading, the remaining 300 active milliseconds wait for Resume. The import pause itself does not consume them.

The example matters because “JSON parses” is only the first validation step. A syntactically valid object could claim that one half of a pair is matched, select a matched position again, or label equal faces as a mismatch. Current decoding checks these structural and state constraints before committing a replacement.

## Validation was intentionally local

The parser verifies the stated consistency rules without proving that a particular sequence of earlier actions produced the position. It accepts a state with enough moves for its matched pairs and current miss; it does not reconstruct every past failed guess. A snapshot is a transport format for local progress, not a signed record of performance.

Top-level and selection keys are exact. Matched positions are unique and in range, and each represented face is complete. A mismatch has two distinct unmatched positions with different identities and an integer remaining interval from 1 to 800. Moves and elapsed time have their current numeric caps. These checks live across `snapshot.ts`, `snapshot-selection.ts`, and the small unknown-JSON helpers in `saved-format.ts`.

The import text has a 65536 UTF-8-byte limit, separate from the textarea's character limit. This distinction is mostly invisible for ordinary exports, whose content is ASCII, but it matters when checking arbitrary pasted text. The [snapshot parsing incident](incident-snapshot-rejects.md) explains how an implementation could confuse transport validation with an overly narrow canonical representation.

## Failed loads remained correctable

An invalid snapshot displays `Invalid snapshot.` while retaining the draft and the current attempt. The session controller calls `decodeSnapshot` first and only resets its clock and replaces the game after it receives a valid state. That order avoids the worst failure mode: clearing a playable board and then discovering that the replacement is unusable.

Ordinary progress does not rewrite the snapshot textarea. Export fills it explicitly, and a person can edit or retain a draft while the board changes. The board-code field follows its own generation-based reset behavior. These two text areas of responsibility are visible in the components rather than hidden in an effect that treats every render as an instruction to synchronize all inputs.

Export synchronizes the active browser clock before encoding, so it does not merely copy a rendered state that may trail the most recent observation. It floors elapsed milliseconds and rounds a pending mismatch's remaining time upward. A positive fraction therefore exports as at least one millisecond instead of accidentally creating an invalid zero-length mismatch. Fractional time remains available in live play; the whole-number representation belongs at the format boundary.

## Completion was a separate load case

A completed import appears completed without Resume. Its banner is static, and the load does not establish a new local record. A person is looking at supplied data, not causing the final match action. Conversely, loading unfinished progress and later completing it through play can update the local best for that size.

The current session model expresses this difference through `replaceGame` and `activateSession`. Replacement clears celebration and publishes a load notice. A winning activation computes result feedback, marks celebration, and calls `rememberBest`. Reading a completed state and performing a winning action are therefore different operations even if both render all cards matched.

This made snapshots suitable for issue reports and visual review without making them a shortcut through completion effects. The [saved-best scope decision](adr-032-best-score-scope.md) and [completion guide](win-conditions.md) explain those current boundaries.

The release left the game with three independent lifetimes: the active attempt in memory, local preferences and best move counts, and manually transported snapshot text. That separation is still useful when investigating a reported loss. A reload, an unavailable store, and a rejected import are different events. The [persistence overview](persistence-overview.md) and [2.0 notes](release-notes-2-0.md) place them in the wider current architecture.
