---
open-forge:
  description: "Card matching, moves, timing and victory rules"
  tags: ["LoadNow", "Game", "Rules"]
---
# Rules
The rules specification is a game contract that defines turns, scores and session changes in Neon Pairs.

Status: spec version 2.1.

A turn reveals two eligible cards. A match adds one pair, while a miss gives you time to remember the faces before both cards turn back.

## Board

- R01 - Support square boards with side lengths `4`, `6`, `8`, `10`, `12`, `14` and `16`. Every face occurs exactly twice. Positions run from zero in row order.

The deal comes from the board code defined in [Reproducibility](reproducibility.md).

## Turn

- R02 - The first valid card activation reveals that card. Activating it again or activating a matched card has no game effect.
- R03 - The next valid activation reveals a different unmatched card. If the two faces match, keep them revealed and add one matched pair immediately.
- R04 - If the faces differ, show both for `800` active milliseconds, then close both together. Ignore all card activation during this interval. Pause the interval while the page is hidden or an imported snapshot awaits Resume.
- R05 - Add one move when the second valid card is revealed. The first reveal and ignored activation add no moves. Matches and misses each cost one move until the count reaches `999999`. At that cap the count stops increasing. Play continues normally, pairs still match and the game still completes. Show the current move count throughout play as `Moves: N`.

For example, revealing two matching cards produces one move and one matched pair. Revealing the same first card twice still leaves one revealed card and zero moves.

## Completion

- R06 - Give one point per matched pair and no other score. Show matched pairs against the board's total pairs throughout play as `Pairs: M/T`, where `M` is matched pairs and `T` is the total. Complete the game when every pair is matched. Show the final moves and elapsed time with a completion banner. Apply the completion motion defined in [Visuals](visuals.md).


## Clock

- R07 - Start elapsed time on the first valid reveal. Count active milliseconds while the page is shown. Pause while the page is hidden or an imported snapshot awaits Resume. Stop at completion. Rendering frequency doesn't change the elapsed time. Cap elapsed time at `359999999` milliseconds. At the cap, play continues normally and the clock shows the capped time.

Show the clock throughout play as `Time: m:ss`. Round elapsed seconds down, pad seconds to two digits and let minutes continue past 59. For example, 3600000ms shows `Time: 60:00`.

The mismatch interval and elapsed clock use the same active time. Showing the page again continues the interval from its remaining time.

## Session

- R08 - Start with `4` rows and `4` columns unless a saved supported size exists. Remember the last chosen size after reload. A page load starts a fresh deal. Loading a valid board code or snapshot also saves its size as the last chosen size. A failed load keeps the saved size unchanged.
- R09 - New Game creates a fresh seed at the current size. Restart keeps the current board code. Choosing a different size creates a fresh seed at that size. Each action clears the selection, matched pairs, moves and elapsed time.
- R10 - When play completes a game, keep its moves if they beat the saved best for that size. Keep these records after reload. An equal or worse result keeps the existing best. Starting or restarting a game keeps these records. Show the current size's best move count throughout play and after completion as `Best: N`. Before that size has a record, show `Best: none`.
- R11 - Invalid board codes and snapshots show an error and keep the current game. Invalid saved preferences are ignored. If storage is unavailable, play continues without saving.
- R12 - Starting, restarting, replacing or leaving a game invalidates its pending timer updates. An update from an earlier game or turn can't change the current game.

For example, restarting during a mismatch produces the original hidden deal with zero moves. The earlier mismatch timer can't close a card revealed in the restarted game.

Control behavior is defined in [Input](input.md). Return to [Neon Pairs](README.md) for the full specification.
