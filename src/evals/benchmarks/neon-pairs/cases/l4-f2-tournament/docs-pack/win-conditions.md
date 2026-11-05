# Win conditions

This page explains when a game is complete, the final state that the core returns, and the browser behavior following the last match.

`isWon` in `src/game/game.ts` has one comparison:

```ts
return state.matched.length === state.deck.length;
```

Every card position must belong to a found pair. Moves and elapsed time can be large, small, or capped without changing that result. There is no losing state caused by either counter.

## The final pair is an ordinary match

`activateCard` does not contain a special final-pair branch. It compares two eligible face values, appends both positions to `matched`, charges the second reveal as one move subject to the cap, and closes selection. If that append fills the matched list, the resulting game is won.

For `np1-4-1`, the eight position pairs in `EXAMPLE_PAIRS` are:

```text
2,6   10,14   4,5   8,15   12,13   3,7   0,11   1,9
```

Revealing those pairs in order, without misses, completes the board in eight moves. Immediately before the last pair, 14 positions are matched. Revealing position 1 creates a one-card selection and leaves moves at seven. Revealing 9 appends both positions, sets selection to closed, and makes moves eight. That returned state already satisfies `isWon`.

No further timer, animation completion, acknowledgment, or automatic reveal is needed. The last pair does not wait through a mismatch interval. [Turn resolution](turn-resolution.md) explains the shared matching branch.

## What a valid completed state contains

A completed state retains its board code, size, seed, and frozen deck. The matched array contains every position exactly once when produced by normal play. Its order reflects the order in which the pairs were found. Selection is closed, `started` remains true, and moves and elapsed time retain their final values.

There is no stored `won` boolean or independent final-score object. `GameStatus`, the clock hook, and the session functions derive completion by calling `isWon` on the game. This keeps the visible banner and the rule for stopping time tied to the same progress data.

The count comparison assumes valid state. A hand-constructed array with duplicated positions could have the correct length without representing a completed board. Normal activation prevents duplicates by ignoring matched positions. Snapshot decoding separately checks unique in-range indices and complete pairs before constructing a restored game.

Completion does not depend on matched-array order. Snapshot export sorts that array numerically, and importing the sorted array still yields the same victory. Set membership and total coverage are what matter to play.

## What stops

`advanceGame` returns the same state when `isWon(state)` is true. Neither elapsed time nor selection changes after completion, even if a large positive delta is supplied. The browser clock's `running` predicate also excludes a won game, and its scheduling effect stops arranging active timeouts for that game.

Card activation has no effect because every in-range position is matched. It is caught by the matched-position guard. Invalid indices remain ignored as they were during play. The final matched faces stay visible.

The board still exists, and completion does not replace it with another screen. Focus and ordinary page controls remain available. New Game, Restart, size selection, board loading, and snapshot controls can still operate. Those actions are replacements or exports rather than further turns in the completed game.

The best display also remains visible. Its value may have improved from the played completion, or it may show an earlier better result. [Best score records](best-score-records.md) covers that comparison.

## Capturing the final time

`useGame.activate` synchronizes the browser clock before calling `activateSession`. The time up to the final activation is therefore accounted for before the core returns a won game. Once won, later ticks do not add more.

In a pure test or replay, the same outcome depends on explicit ordering: advance the active duration first, then activate the final card. Calling `advanceGame` after completion cannot retroactively add that duration. This follows directly from the completed-state early return.

The formatter floors elapsed seconds and keeps minutes increasing beyond an hour. A displayed final value is consequently a presentation of the stored duration, not a separate timer measurement taken when the banner mounts. [Elapsed time rules](elapsed-time-rules.md) contains examples of the conversion and caps.

`game.test.ts` includes a completion sequence that supplies 10 milliseconds between the first and second card of each of eight pairs. It ends with eight moves and 80 elapsed milliseconds, then checks that a later time advance returns the same state.

## Browser result and record update

After the final `activateCard` result, `activateSession` calls `isWon`. For a new played win it updates feedback to include all pairs found, moves, and formatted elapsed time. It increments the announcement counter, calls `rememberBest`, and sets `celebration: true`.

`GameStatus.tsx` displays this completion banner:

```text
Every pair found in N moves and TIME. Well played.
```

Here `N` comes from `game.moves`, and `TIME` comes from `formatElapsed(game.elapsedMs)`. The banner's presence depends on `isWon`; the additional celebration class depends on the session flag. These are separate decisions.

In `status.css`, that class selects a 600-millisecond, single-iteration pulse. Reduced-motion styling removes the animation. The pulse does not control when victory is recognized, when the record is calculated, or when the clock stops. Visual details are in [Motion policy](motion-policy.md).

There is no sound effect attached to the win path. The current game is silent, as described in [Sound](sound.md).

## Loading a completed snapshot

A valid completed import arrives through `replaceGame`. Since `isWon(game)` is already true, the session is not paused and no Resume control appears. It reports the completed-load notice and sets celebration to false.

The banner is still shown because its condition is completion. It remains static because the import did not mark a played win. The import also does not run the record helper. This avoids interpreting an already completed position as a new final-card event.

An unfinished import behaves differently: it is paused until Resume, and a later final match uses the normal activation path. That later completion can update the local best and set celebration. The distinction is covered in [New Game and Restart](new-game-and-restart.md).

## Counter limits

Neither numeric cap is a loss condition. A game can finish at 999,999 moves or 359,999,999 elapsed milliseconds. Matching continues normally and completion still depends on matched coverage. [Scoring and moves](scoring-and-moves.md) covers the saturation checks.

## Evidence across sizes

`src/game/completion.test.ts` generates a board for every supported size, groups positions by their face value, and reveals every pair. It checks victory, full matched coverage, and exactly one move per pair. `game.test.ts` checks stopped time and ignored activation after the example completion.

Session tests add the played-win effects and distinguish imported completion. Browser tests cover the final status and banner as visible outcomes. A pure completion check alone cannot establish animation or persistence behavior, just as a visible banner alone cannot establish that later clock advances are ignored.

For a game that appears finished but has no banner, compare `matched.length` and `deck.length` first. For a game with a banner whose time keeps changing, inspect whether the update path is using `advanceGame` and whether the rendered status belongs to the current session generation. These are separate symptoms at the core and session boundaries.
