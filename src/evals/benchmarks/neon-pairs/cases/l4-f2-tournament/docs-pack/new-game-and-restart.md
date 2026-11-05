# New Game and Restart

This page compares the operations that replace a game, the progress they reset, and the session data they retain.

New Game asks for a fresh seed at the current size. Restart recreates the current board code. Both return to hidden cards, zero moves, zero elapsed time, no matched positions, and an unstarted game. A size change follows the New Game path with another supported side length.

The implementation is in `src/game/session/use-game.ts`, with shared replacement behavior in `session.ts`.

## The choice that matters

Use Restart to repeat the same positions. If a player has learned where the pairs are, restarting preserves that knowledge's usefulness because the code and generated deck are the same.

Use New Game for a new seed. `newGame(size = current.current.game.size)` passes the current seed to `freshBoardCode`, which rejects it. The new code therefore has a different seed from the immediately preceding game, even if this operation also changes size.

The implementation does not keep a history of all earlier deals. It excludes the immediate seed, rather than proving the board has never appeared before. It also does not compare the new shuffled array with the previous one. The reproducible identity is the code, and deterministic generation is described in [Deck generation](deck-generation.md).

Restart does not call the fresh-seed function. It calls `createGame(current.current.game.boardCode)`, so invoking it repeatedly continues to reproduce the same original hidden deal.

## Replacement comparison

| Operation | Deal | Progress | Imported pause |
| --- | --- | --- | --- |
| New Game | Fresh seed at current size | Initial | Cleared |
| Choose another supported size | Fresh seed at selected size | Initial | Cleared |
| Restart | Same code | Initial | Cleared |
| Load valid board code | Entered code | Initial | Cleared |
| Load unfinished snapshot | Snapshot's code | Restored | Set |
| Load completed snapshot | Snapshot's code | Restored completion | Clear |
| Reload page | Fresh seed at saved supported size | Initial | Clear |

Here “initial” means the values returned by `createGame`: empty matched array, closed selection, zero moves, zero elapsed milliseconds, and started false. A restored snapshot instead supplies its own validated progress.

Reload has no in-memory current seed to exclude. Preferences save size and best records, so startup knows the preferred size but does not recover the previous code. A fresh reload is consequently distinct from Restart.

## Shared session work

`replaceGame(session, game, imported)` performs the common transition. It installs the supplied game, increments `generation`, clears celebration, and sets preferred size to `game.size`. It retains the best map in preferences.

For ordinary replacements it clears feedback and the load notice. For imported snapshots it publishes the corresponding loaded notice and increments the announcement counter. An imported unfinished game is paused; an imported completed game is not.

The generation increment happens even when the replacement code is identical. Restart and loading the current code are still new attempts. An old clock callback is associated with the earlier session generation and cannot be treated as work for the replacement.

`Game.tsx` also keys Board by generation. Replacing the game remounts that component, resetting its remembered tab stop to the initial position. Clearing progress and resetting navigation state are separate consequences of the same replacement.

## Clock observation and game time

Successful replacements call `clock.reset()` before commit, establishing a new `performance.now` observation so the old game's duration cannot carry over.

Resetting that observation does not itself zero `elapsedMs`: New Game and Restart get zero from `createGame`, while snapshots retain validated imported time. Resume resets observation again before clearing pause, preserving all restored progress. A resumed unstarted snapshot still waits for its first valid reveal. See [Elapsed time rules](elapsed-time-rules.md).

## Restart during a mismatch

A pending mismatch is simply a selection in the old game. Restart drops it when it creates the new game. Both temporary faces disappear, as do all earlier matched faces from that attempt, and moves return to zero.

There is no wait for the old 800-millisecond interval. The action takes effect as a replacement. The timer effect is cleaned up, and generation checks reject a callback carrying the previous generation even if it had already been queued.

A useful example is:

```text
old attempt: reveal 0 and 1 -> mismatch
Restart: same code, hidden board, zero moves
new attempt: reveal 2 -> one-card selection
old generation's tick: ignored
```

A newly selected card does not inherit the old mismatch's remaining duration. Likewise, if the new attempt creates its own mismatch, that new selection gets the full current `MISMATCH_MS` value.

`tests/lifecycle.e2e.test.ts` checks Restart during a partially elapsed mismatch, including a later mismatch in the restarted game. `session.test.ts` directly passes an old generation to `tickSession` and checks that the replacement is unchanged.

## Loading a board code

The load function trims surrounding whitespace and validates the code before resetting the clock or replacing the game. A valid code always starts from hidden progress, even if that same code is already active and partially solved.

The entered size becomes the selected size, and the preference effect can save it. A valid code for 10x10 replaces a 4x4 game rather than trying to apply a hundred-card deck to a four-column state.

Invalid input publishes `Invalid board code.` and returns false. The game, code, selected size, and progress remain current. It does not reset a pending mismatch or pause time. Editing the code draft alone also does not load anything.

The board-code control follows generation changes so New Game, Restart, and successful loads show the current code. Ordinary card activity preserves an edited draft. [Reproduction panel](reproduction-panel.md) covers the component behavior.

## Loading progress and exporting it

`loadSnapshot` runs `decodeSnapshot` before replacement. Invalid JSON or inconsistent progress reports `Invalid snapshot.` without installing a partial game. A failed load also retains the entered draft for correction.

A valid unfinished snapshot restores progress paused. The UI shows Resume even if the reproduction details have been collapsed, since the pause control is outside those details. Until Resume, activation is blocked at session level. Board focus movement remains separate from that block.

A valid completed snapshot shows completion without a Resume control and with celebration false. It does not create a new played-best event. An unfinished imported game that is resumed and later completed can record its final count normally. See [Best score records](best-score-records.md).

Export is not a replacement. It synchronizes the current clock and fills the snapshot textarea with the encoded state. It does not increment generation, clear progress, or set an import pause. If play continues after exporting, that text is a captured position, not a live view of subsequent progress.

Restart after an import uses the imported board code but drops the imported progress. To restore that progress again, the snapshot has to be loaded again.

## What survives an ordinary reset

The session retains per-size best records. A successful replacement may select a different entry for display because the game size changed, but it does not erase the map. The preferred size becomes the replacement game's size.

The local snapshot textarea is component state in `Reproduction`, which is not keyed by session generation. Starting another deal does not itself clear that text. This can be useful when retaining a captured position, but it also means visible text can describe a different game from the one currently on the board.

Focus stays on the control used for starting, restarting, loading, or exporting. The board's remembered tab stop resets on replacement, but that does not immediately move focus there. Resume is the special case: `Game.tsx` focuses the remembered board position before its disappearing button is removed. Details belong to [Focus policy](focus-policy.md).

## Saved size is not saved progress

Startup reads supported size and best records, then makes a fresh code. It cannot restore the previous cards from those preferences. A code recovers a starting deal, while an exported snapshot recovers progress. This explains a reload showing the old best beside a new hidden board.

## Unsupported calls

`newGame` returns before changing the game or clock for an unsupported size. Its fresh-seed function uses browser cryptographic randomness and supplies no fallback seed. Preference storage failures are handled separately and do not change deal generation.
