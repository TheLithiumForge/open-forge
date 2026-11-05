# Competition results and local records

This page explains the boundary between competition play and the ordinary saved preferences in Neon Pairs.

The solo application stores a preferred board size and best move counts by size. Those values describe ordinary use of the device, rather than named participants. Competition introduces a different context for completed games, so reusing the solo session without examining its side effects can produce a correct-looking board and incorrect saved preferences.

## The competition boundary

A shared device may already contain records from someone who is not taking part in the event. Tournament games never change best scores or the saved board size. This separates the event's results from the device's ordinary play preferences without changing the meaning of a completed memory board.

For example, suppose the saved size is 10 and the saved best for size 4 is 12 moves. A competition attempt can produce a completed four-column board with 8 moves while those ordinary saved values remain size 10 and best 12. The game result and the preference record describe different things; the competition boundary prevents one from being substituted for the other.

The comparison between participants is in [competition ties](competition-ties.md). Competition adds no extra points, multiplier or group total. In the existing core, progress is the number of matched pairs, and a completed game also has its move count and elapsed time.

## What the game saves today

`src/game/session/storage.ts` defines `Preferences` with a numeric `size` and a `bestMoves` map keyed by size strings. `PREFERENCES_KEY` is `neon-pairs.preferences.v1`. `writePreferences` serializes a schema version together with those values.

An example stored value is:

```json
{
  "schemaVersion": 1,
  "size": 10,
  "bestMoves": {
    "4": 12,
    "6": 24
  }
}
```

There are no participant names or board codes in that value. A best belongs to a board size across ordinary games, rather than to a particular generated deal. A person who sees the size-4 best is not seeing the result of the current board code unless the circumstances happen to coincide.

The reader validates the schema version, supported size and best-score map. Score keys have to represent supported sizes in canonical numeric-string form. Values are nonnegative safe integers within the move cap and at least the number of pairs for that size. An invalid record falls back to default preferences rather than being partly interpreted as an event history.

[Preferences store](preferences-store.md) and [best score records](best-score-records.md) explain the ordinary mechanism. The competition decision does not add fields to this saved value or define another persistence format.

## Two ordinary paths have side effects

The first path is game replacement. `replaceGame` in `src/game/session/session.ts` copies preferences and assigns `size: game.size`. The ordinary browser actions for starting a new deal, restarting and successfully loading a board or snapshot use that replacement function. Its purpose is to keep solo play's chosen size in sync with the current game.

The second path is completion. `activateSession` calls `activateCard`, recognizes a won game and calls `rememberBest(session.preferences, game.size, game.moves)`. That helper compares the new moves with the previous entry for the size. A better value produces a new map, while an equal or worse value returns the original preference object.

Both behaviors are correct in the current solo context. Both matter to a competition extension. Avoiding the completion update alone would still leave size changes on the replacement path. Avoiding size selection alone would still leave a best-score update when ordinary activation completes a game.

There is no competition argument or mode check in either function. The source therefore does not currently implement the boundary described on this page. This is a concrete integration gap, not an indication that `rememberBest` has the wrong behavior for its existing caller.

## Where saving actually happens

`useGame` reads preferences at initialization and calls `useSavedPreferences(store, session.preferences)`. That hook has an effect that invokes `writePreferences` whenever the preference object or store dependency changes. It also runs when mounted.

Consequently, hiding the Best statistic would not prevent saved records from changing. The display does not own the write. Similarly, suppressing one call to `localStorage.setItem` would not explain what happened to the in-memory preferences that a later effect might serialize.

The distinction between values and writes is useful here. The competition sentence concerns the best-score and size values. It does not itself introduce a blanket prohibition on all storage API calls. A review that counts only writes can miss an in-memory change; a review that looks only at the current screen can miss a persisted change after reload.

`rememberBest` returning the original object for equal or worse ordinary results can reduce preference effect activity. That identity behavior is an implementation detail with a clear purpose, but it is not a substitute for deciding which context owns a completion. [Persistence overview](persistence-overview.md) and [React boundaries](react-boundaries.md) cover those existing responsibilities.

## Why completed snapshot import is a partial analogy

The current solo application already treats imported completion differently from completion through card activation. `replaceGame(session, game, true)` sets the imported state without calling `rememberBest`. A completed snapshot arrives with its banner static and without a celebration. An unfinished imported game can later be resumed and completed through ordinary activation, which can record a best.

That existing distinction is helpful evidence that rendering a won board and recording a local best are separate operations. It is not a complete solution for competition: snapshot replacement still updates the preference size, and later completion of unfinished imported progress follows the ordinary best-score path.

`src/game/session/session.test.ts` checks those differences. It verifies that normal completion records a best, completed import does not, and finishing restored incomplete progress can record one. Reading the full set of assertions prevents an engineer from generalizing the completed-import exception to every game associated with a snapshot.

## A concrete review example

Begin conceptually with the saved value shown above and a competition board at size 4. There are two distinct moments to inspect: the moment the board replaces the previous game, and the moment its final pair is activated. In the unmodified solo session, the former can change size to 4 and the latter can improve its stored best to 8.

The boundary for competition preserves the original values through both moments. This example exposes both source paths with a small amount of data. It is more informative than starting from empty preferences, because an empty map makes an accidental new best look like an ordinary first result.

A further comparison with ordinary solo play matters when reviewing an implementation. The existing solo behavior still provides the baseline: `tests/persistence-contract.e2e.test.ts` exercises better, equal and worse completed results across reloads, as well as successful and unsuccessful load effects on saved size. Those tests describe the current game; they do not exercise a competition screen.

The useful evidence is the before-and-after preference content alongside the completed game, with the active context made clear.

