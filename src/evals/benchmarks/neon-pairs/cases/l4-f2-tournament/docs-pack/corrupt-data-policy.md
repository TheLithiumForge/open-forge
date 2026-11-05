# Corrupt data policy

This page explains how Neon Pairs handles unreadable or invalid stored data and how read rejection relates to a later save.

Browser storage contains strings outside the type system. A value may have been written by an older build, edited by a person, truncated by a debugging experiment or replaced by another script on the origin. The current preference reader accepts only a usable preference aggregate and otherwise returns its default. It does not try to repair individual score entries.

## Rejection at the read boundary

A failed decode produces empty application data rather than a partially interpreted value. The read path can abandon unusable content without deleting the browser string as part of that read. A stored value that fails to parse or validate is treated as empty, without any error, and stays untouched until the next save replaces it. The distinction between reading and saving matters because a save can follow immediately during application initialization.

For current preferences, “empty” is the complete fallback `{ size: DEFAULT_SIZE, bestMoves: {} }`. `DEFAULT_SIZE` is 4. The result is therefore a fresh default-size game with no known bests, rather than an empty board or a blocked screen. `readPreferences` makes that fallback before entering its `try` block and returns it from each rejection branch.

The reader has no write or removal call. However, `useSavedPreferences` runs an effect on mounting with the preferences obtained during initialization. That effect can overwrite the rejected string with valid fallback preferences after rendering. The original text remains available only until a save occurs, and the ordinary mount effect is such a save.

## Parsing is only the first filter

`parseJson` in `src/game/saved-format.ts` catches `JSON.parse` errors and returns `undefined`. A string such as `{` therefore fails without throwing an uncaught exception from the preference reader. Valid JSON can still be unusable: `null`, an array, a string, a boolean or a number does not provide the required preference object.

`isJsonObject` rejects null and arrays. After that narrowing, the reader checks `schemaVersion === PREFERENCES_VERSION` and requires `size` to have JavaScript type `number`. It then checks that size against `isValidSize` and requires `bestMoves` to be an object. A string value such as `"6"` is not accepted as a selected size even though the UI select ultimately uses string option values.

The empty string is handled even earlier. `if (!text) return fallback` covers a missing key returned as `null`, an empty stored string and a null store's absent result. It avoids parsing text that cannot represent the preference object. Whitespace-only text is truthy, reaches the parser and fails there.

These branches are worth keeping separate in a test log. They converge on the same application state, but one shows absent data, another shows malformed syntax and another shows a structural mismatch. No error copy is rendered for these preference cases.

## Whole-value validation

The reader builds a fresh `bestMoves` object and iterates through `Object.entries(value.bestMoves)`. For each entry it converts the key with `Number(size)`, then compares `String(numberSize)` with the original key. That round trip requires the canonical size spelling. It rejects `04`, `4.0`, `4e0` and strings padded with whitespace, even when numerical conversion would produce 4.

The numeric key must then be supported by `isValidSize`. Each move count must be a nonnegative safe integer, no greater than `MOVES_MAX`, and no smaller than the number of pairs for its size. `MOVES_MAX` is 999999. The lower bound is `numberSize ** 2 / 2`, because completing the board takes at least one move for each pair.

| Side length | Lowest accepted completed move count |
| --- | ---: |
| 4 | 8 |
| 6 | 18 |
| 8 | 32 |
| 10 | 50 |
| 12 | 72 |
| 14 | 98 |
| 16 | 128 |

A failure at any entry returns the fallback for the entire preference value. A valid selected size of 6 does not survive alongside an invalid 4x4 score of 1. Likewise, valid score entries are not salvaged around one bad entry. The local object being assembled is never returned on that failure path.

This behavior is observable after reload: both the size preference and all previously accepted bests can appear reset when one map entry becomes invalid. It does not indicate that the size failed its own check. [Preferences store](preferences-store.md) explains why the aggregate is read as a unit.

## Examples with different failure causes

This value parses, but its version discriminator is unsupported:

```json
{"schemaVersion":2,"size":6,"bestMoves":{}}
```

This value has the expected version and size, but the score field has the wrong shape:

```json
{"schemaVersion":1,"size":6,"bestMoves":[]}
```

This value has a valid map shape and an invalid key spelling:

```json
{"schemaVersion":1,"size":6,"bestMoves":{"04":8}}
```

This value uses a canonical supported key but an impossible completed score:

```json
{"schemaVersion":1,"size":6,"bestMoves":{"4":7}}
```

All four produce the same preference fallback. The reader does not emit different messages or a machine-readable rejection reason. A diagnostic harness can identify the failed condition by choosing one changed field at a time. The production API does not expose those distinctions to components.

By contrast, this is a valid value with no score for the selected size:

```json
{"schemaVersion":1,"size":6,"bestMoves":{"4":8}}
```

The 4x4 record is retained, the board starts at 6x6, and the 6x6 best display is `none`. Absence of a current-size best is not evidence of corruption.

## Extra properties and normalization

The preference reader does not enforce an exact property count at the top level. A value with valid expected fields and an additional property can be accepted. The returned application value includes only the selected size and the newly built score map. A normal subsequent write serializes those known fields and the schema discriminator, omitting the extra property.

This is different from the snapshot parser, which rejects unknown fields according to its own format. A shared JSON helper does not make all parsers equally strict. Tests that copy a snapshot rejection case into preferences without checking the reader could accidentally demand behavior the game does not implement.

The current reader also does not coerce score strings to numbers, round fractions or clamp out-of-range scores. A value of `"8"` is not accepted as a move count. A fraction is rejected rather than rounded. A number above the move cap invalidates the aggregate rather than being reduced to the cap. Validation produces a known state or the fallback, not a best-effort approximation of the edited input.

## The timeline of replacement

There are three useful points in a corruption test. Before initialization, the raw store contains the original text. After `readPreferences`, the caller has either accepted preferences or fallback preferences, and the raw store is still unchanged by that function. After `writePreferences`, the store may contain a valid serialized replacement if the write succeeded.

In the browser, those latter two stages are separated by React's render and effect cycle rather than by a player's explicit action. `useGame` initializes session state from the read result, then `useSavedPreferences` writes that state after mounting. A test inspecting storage only after the UI has settled can see valid defaults and miss the original corrupt text entirely.

The simplest way to prove read nonmutation is a store double whose `setItem` records calls. Invoke only `readPreferences`, inspect the original string and check that no write occurred. A separate browser test can demonstrate mount replacement. Combining both assertions into a single post-navigation observation loses the distinction this policy depends on.

## Silent rejection is local to stored preferences

Invalid input typed into the board-code and snapshot controls follows another interaction. Those controls keep the current game, preserve the draft for correction and show their defined validation status. They do not silently replace the active game with a default board. That feedback is appropriate because the person has just requested an explicit load operation.

Stored preference rejection happens during initialization with no draft to correct in the game UI. It returns a playable state quietly. There is no recovery dialog, corrupted-data banner or download of rejected text. Browser access failure also returns defaults, but concerns operation availability rather than text acceptance; see [Storage failure policy](storage-failure-policy.md).

None of these parsers establishes authenticity. A manually edited score of 8 for 4x4 is accepted if the surrounding value is valid, even without evidence that eight pairs were played. The constraints establish a plausible stored aggregate, not a verified score history.

## Evidence and useful extensions to a diagnosis

`storage.test.ts` already covers malformed JSON, null, an empty object, unsupported size, scores below the pair count, negative and fractional values, and a noncanonical key. It separately checks that a score at the move cap round-trips and one beyond it is rejected. The browser persistence contract test seeds malformed text before navigation and checks for a playable 16-card default board.

Those tests are focused evidence, not an exhaustive enumeration of every possible JSON string. A new bug report is easiest to isolate with its exact raw value and one field changed at a time. Comparing an independently written valid baseline with the failing value exposes whether the issue concerns syntax, object shape, selected size, key spelling or score bounds. [Storage testing guide](storage-testing-guide.md) gives the browser setup and store doubles for doing that without modifying the source.
