# Best score records

This page explains how the current session stores the lowest completed move count for each supported board size.

Records live in preferences, alongside the last selected size. They survive a reload when browser storage is available. They do not contain a saved deal, an action history, or elapsed-time rankings. The game shows the current size's entry as `Best: N`, and shows `Best: none` when that size has no entry.

The scoring distinction is described in [Scoring and moves](scoring-and-moves.md). Broader browser storage behavior belongs to [Persistence overview](persistence-overview.md).

## Data in memory

`src/game/session/storage.ts` declares:

```ts
export interface Preferences {
  readonly size: number;
  readonly bestMoves: Readonly<Record<string, number>>;
}
```

`size` is the selected side length. `bestMoves` maps its decimal string to a move count. Missing entries represent sizes with no record. Zero is not used as a sentinel: even the smallest completed board needs eight moves, so a zero entry would be invalid saved data.

For example:

```json
{
  "size": 6,
  "bestMoves": {
    "4": 12,
    "6": 20
  }
}
```

This says the current preferred size is 6, with separate records of 12 moves for 4x4 and 20 for 6x6. It says nothing about which board code produced either result or how long either game took.

The record scope is a side length. All board codes at that size share the same comparison entry. Restarting the same deal and playing a fresh seed both compete with that entry. The current implementation has no per-code record map.

## Recording a played completion

The call to `rememberBest` is in `activateSession`. After `activateCard` produces a completed game, the session passes its preferences, the game's size, and the final move count to that helper.

This placement matters. Core completion can be calculated without storage, and a replay can finish without writing a browser record. The browser session decides when an actual activation resulting in victory is eligible to update its preferences.

`rememberBest` reads the old value at `String(size)`. Its comparison is:

```ts
if (previous !== undefined && previous <= moves) return preferences;
```

A smaller result creates a replacement preferences object containing a copied map with the improved value. An absent previous entry also creates a record. An equal or larger result returns the original preferences object. There is no elapsed-time tiebreaker.

| Existing size-4 best | Played result | Resulting best |
| ---: | ---: | ---: |
| Missing | 14 | 14 |
| 14 | 12 | 12 |
| 12 | 12 | 12 |
| 12 | 15 | 12 |

All other size entries are preserved by the spread. Improving size 4 does not alter size 6's value or the preferred size. Successful game replacement updates preferred size separately through `replaceGame`.

## Completion imports are a different path

Loading a completed snapshot uses `replaceGame`, not `activateSession`. It displays completion and retains the imported moves in that game, but does not call `rememberBest`. A completed import can therefore show fewer moves than the current local best while the best display remains the local record.

Loading unfinished progress pauses it. After Resume, a later final match does go through `activateSession` and can establish or improve the local record. The move count considered is the full imported-and-continued count, not only the attempts made after Resume.

This is a distinction between state replacement and a played completion. Snapshot validation checks consistency of the position; it does not attest to the history of its moves. [Snapshot format](snapshot-format.md) explains that validation boundary.

Activating a position on an already completed imported game does not turn it into a new completion event. Every valid position is matched, so `activateCard` returns the same game and the session exits before record handling.

## Persistence representation

The current storage key is `neon-pairs.preferences.v1`. `PREFERENCES_VERSION` is 1, and the saved object includes that version under `schemaVersion`:

```json
{
  "schemaVersion": 1,
  "size": 6,
  "bestMoves": {
    "4": 12,
    "6": 20
  }
}
```

`writePreferences` serializes this object and passes it to `setItem`. `useSavedPreferences` calls the writer in an effect when the preference object or store changes. Card reveals and time ticks normally preserve the same preference reference, so they are not individual score writes.

The equal-or-worse return path in `rememberBest` also preserves that reference. Winning can still update session feedback and celebration, while the unchanged preferences do not create a new preference dependency for the saving effect.

At page startup, `browserStore` attempts to obtain `window.localStorage`. Access itself can throw, so it returns `null` on failure. `readPreferences` also catches retrieval and parsing failures. The fallback is `{ size: 4, bestMoves: {} }`.

The application writes only the designated preference key through this module. It has no clearing action for all storage and does not need to modify keys belonging to other applications. Details of storage envelopes and failure scenarios are in [Preferences store](preferences-store.md) and [Storage failure policy](storage-failure-policy.md).

## What is accepted from saved JSON

The reader treats parsed JSON as unknown data. It first checks that the outer value is an object with the expected schema version and a numeric supported size. It then requires `bestMoves` to be an object.

For each entry, it converts the key to a number and checks all of the following existing conditions:

- Converting that number back to a string yields the original key.
- The number is one of the supported side lengths.
- The score is a nonnegative safe integer.
- The score is at most 999,999.
- The score is at least the number of pairs for that size.

The round-trip key check rejects a key such as `"04"`, even though converting it with `Number` gives a supported size. Keys are canonical decimal side lengths in the data this reader accepts.

The minimum bounds are 8, 18, 32, 50, 72, 98, and 128 for sizes 4 through 16. A fractional score, a string containing digits, a negative number, and an implausibly low completed count are all rejected. The upper bound is shared with `MOVES_MAX` from `game.ts`.

The reader does not apply snapshot-style exact-key validation to the outer preference object. Its explicit checks concern version, size, and the record map. This is one reason the saved preference representation and the snapshot representation should not be treated as interchangeable formats.

## Failure is handled as one invalid preference value

If any record entry fails a check, `readPreferences` returns the full fallback. It does not salvage other entries from that object. An otherwise valid selected size is also lost in that fallback result.

For example, a saved object with size 6, a valid size-6 best of 20, and a size-4 best of 1 falls back to default size 4 with no bests. The invalid size-4 entry makes the stored preferences unusable as a whole.

This is different from a size with no entry. An empty record map is valid and retains a valid saved size. Loading a saved size of 6 with `{}` is a normal way to have a six-column startup board without any records.

Neither malformed saved data nor blocked storage prevents play. `writePreferences` catches write errors, and a `null` store makes the operation inert. The active session can still hold an improved record in memory and display it. If the browser did not persist it, a later reload cannot recover that in-memory change.

## Reload and replacement

Reload creates a fresh deal at the saved size, retaining the record map. New Game and Restart also retain the map while clearing progress. Successful loads select their game's size, changing which entry is visible; invalid loads retain the current size.

A completed import can therefore change the displayed best by selecting another size without establishing a record. [New Game and Restart](new-game-and-restart.md) compares these paths.

## Caps, ties, and ordinary comparisons

A capped result of 999,999 is a valid record if the size has no previous entry. A later result below it improves the record. Another capped result ties and retains the existing preferences object.

Elapsed time is irrelevant to this ordering. A 12-move completion at several minutes and a 12-move completion at a shorter time have equal score for this map. The stored representation has no duration field in which to remember the difference.

There is also no list of past attempts. The map contains at most one score for each supported size because each size is one key. It does not retain the second-best result, a date, a player name, or a streak.

## Evidence available in the source

`storage.test.ts` checks separate size records, better/equal/worse comparisons, round-trip saving, invalid values, the maximum, and denied storage. `session.test.ts` distinguishes played completion, completed imports, and completion after continuing imported progress. Browser persistence tests check the UI and actual store path.

For a missing record, compare the in-memory map after victory, the serialized preferences at the active origin, and the reader's result after reload. That separates a failed write from a validation fallback or a current-size display lookup.
