# Gameplay FAQ

This page answers common player questions and points maintainers to the source behavior behind the answers.

## Why does revealing one card leave moves at zero?

A move is a completed two-card attempt. The first reveal begins an attempt; the second distinct eligible reveal counts it. Activating the same first card again does not complete an attempt. A match and a miss each cost one move, subject to the counter's cap.

[Scoring and moves](scoring-and-moves.md) contains a worked sequence and the minimum completed count for every size.

## Why can I not reveal a third card?

After a miss, the two different faces remain visible for 800 active milliseconds. During that selection state, all card activation is ignored. The game does not queue your choice for when the interval ends.

Moving focus can still be possible because navigation is separate from activation. If the page has just become visible again, only the mismatch's remaining active time needs to pass. See [Mismatch interval](mismatch-interval.md).

## Does a single revealed card close after a while?

No. The one-card selection has no timeout. It stays until another eligible card resolves the attempt or an operation replaces the game. Time still counts after the first valid reveal while the visible game is active.

The automatic closing interval belongs to two different revealed faces. It is not a general duration for all temporary reveals.

## Why did clicking a card seem to end the previous miss?

The browser measures active time before processing the activation. If the previous mismatch is already due to close, that synchronization closes it first. The clicked eligible card can then become the first reveal of a new attempt.

A timeout callback does not need to run first for the game to recognize expiry. If time still remains, the activation is ignored. [Turn resolution](turn-resolution.md) explains the exact ordering.

## The cards have the same shape. Why was it a miss?

Shape is one part of a face. Marker direction distinguishes repeated shapes, and the larger boards also contain outline versions of solid faces. Two solid circles with different marker positions are different identities. A solid and outline circle with the same marker are different too.

The full revealed label identifies color, fill, shape, and marker. The rule compares the two numeric face values in the deck. [Pair identity](pair-identity.md) maps those values to designs.

## Are adjacent matching cards a bad deal?

No spatial separation rule is applied. The generator shuffles two copies of each face and can leave mates next to each other. In the published `np1-4-1` board, positions 4 and 5 are a pair, as are 12 and 13.

A pair is valid wherever its two distinct positions happen to lie. The shuffle does not move it apart after generation.



## What is the difference between New Game and Restart?

New Game keeps the size and requests a seed different from the immediately current seed. Restart uses the current board code, reproducing its positions. Both clear the current selection, found pairs, moves, and elapsed time.

Changing size also starts fresh progress. There is no collection of suspended games behind the size options. [New Game and Restart](new-game-and-restart.md) compares all replacement paths.

## Can I recover progress with only a board code?

A board code recovers the starting deal. It contains no matches, selection, moves, or elapsed duration. A snapshot captures those progress fields and can restore a later position.

Restart after importing a snapshot uses that snapshot's code but starts its deal over. Loading the snapshot again restores its captured progress. See [Reproducibility](reproducibility.md).

## Why is a loaded snapshot waiting for Resume?

Unfinished imported progress is paused so its clock and any mismatch remainder do not run while you inspect it. Resume clears that pause. If the restored game was already started, active visible time then continues; if it was an unstarted position, the first valid reveal still starts its time.

A completed import needs no Resume because its game is already finished. It shows a static completion banner.

## Why did my clock stop while I was away?

The clock excludes the period while the page is hidden. A pending mismatch excludes that same period. Returning resumes the remainder rather than granting a fresh mismatch interval.

The implementation uses page visibility, not pointer activity or the time since the last guess. Leaving a visible started game between turns still counts. [Elapsed time rules](elapsed-time-rules.md) explains the running predicate.

## Why can the clock show more than 59 minutes?

Minutes keep increasing: one hour is `60:00`. Seconds round down. The maximum stored duration displays as `5999:59`, and reaching it does not end play. See [Elapsed time rules](elapsed-time-rules.md).

## What happens at the move limit?

The counter stops increasing at 999,999. Turns still resolve normally and completion still depends on finding every pair. A miss at the cap retains its normal interval, and a match adds the normal two matched positions.

The cap also bounds imported progress and saved records. It is a numeric limit, not a move allowance that can be exhausted.

## Why did a completed import not improve my best?

Importing an already finished position is state replacement, not a newly played completion. The local record helper runs on the final activation of a played game. If you import unfinished progress, Resume it, and later finish, that final activation can establish a record using the resulting full move count.

Records are per size and compare moves only. A faster result with the same number of moves ties the existing best. [Best score records](best-score-records.md) describes the comparison and storage reader.

## Why did a reload keep my best but lose my cards?

Preferences save the last supported size and per-size best move counts. Startup uses those preferences to create a fresh deal. Current progress stays in memory unless you export it as a snapshot.

If storage is blocked or saved preferences are invalid, the game uses size 4 with no saved records. In-memory play remains available. The relevant persistence behavior is in [Preferences store](preferences-store.md).

## Can a finished game keep taking time or moves?

No. Time advancement and card activation leave a completed valid game unchanged. The board remains visible, and the controls can replace it with another game. [Win conditions](win-conditions.md) describes the final state.

## What should a maintainer capture for an unexpected turn?

The board code, position sequence, and active-time steps make a compact reproduction. For timing cases, include hiding, showing, snapshot loading, and Resume. Use zero-based indices consistently or give full row-and-column locations.

A snapshot is useful for the immediate position. A replay adds the ordered actions and active timestamps that led to it. A screenshot can explain the visible symptom, but cannot by itself show whether a previous activation was ignored or how much active time remained.

## Which tests answer which question?

Generation vectors establish the deal; core tests establish turns; session tests add pause and records; browser tests establish measured time and visible outcomes. [Game state model](game-state-model.md) maps those boundaries.
