# Import safety policy
This page explains how Neon Pairs validates untrusted exchange text before a browser session or replay result can use it.

The important boundary is between a draft and accepted game data. A text field can contain arbitrary edits, and a replay file can contain arbitrary bytes decoded as text. Neither is treated as a `GameState` just because JSON parsing succeeds. Shape checks, numeric checks and game-specific consistency checks happen before the browser commits a replacement.

## Field diagnostics and the current implementation

A field outside a format's declared shape is a validation failure, including inside a selection or replay action. The diagnostic policy makes that particular mistake identifiable to a person correcting copied data. An import with a field its format doesn't define shows "Unknown field: NAME." for the first such field and leaves the current state unchanged. This applies to the field diagnostic; it does not make an incomplete object or an unsupported version acceptable. The format's own required fields and state constraints still determine acceptance.

The implementation already rejects extra keys, but does not yet produce that field-specific notice. `hasExactKeys` returns a boolean, `decodeSnapshot` returns `null`, and the browser displays `Invalid snapshot.` for all snapshot failures. `readReplay` provides problem categories rather than the offending key name. The current adapters reject extensions without identifying the offending field as the diagnostic policy requires.

The existing code does not define a traversal order, object-path syntax or combined-error format for a field-specific diagnostic. Its boolean exact-key helper cannot report a first key. Those details cannot be inferred from a `null` result. For current behavior, examine the published notice and replay problem category rather than expecting a key name.

## Validation before replacement

`useGame.loadSnapshot(text)` calls `decodeSnapshot(text)` first. A failed result calls the notice helper and returns `false`. Only a successful result reaches `clock.reset()` and `replaceGame(current.current, game, true)`. There is no earlier step that installs the imported code or clears the existing matched positions.

`useGame.loadBoard(code)` follows the same broad shape with a board-code parser. It trims the control's draft, checks the result, and only then resets the clock and constructs a replacement game. A valid board-code string is sufficient for a fresh deal; a snapshot needs the additional position checks because it claims progress.

Failure changes visible feedback and the announcement counter. It does not replace the game, reset the running clock, save a different size or discard the edited draft. “Keeps the game” therefore does not mean that every field of the containing `Session` has reference equality: notices are session fields too. Nor does it freeze the active game while someone corrects text. A running game can continue to gain elapsed time after an unsuccessful load.

This distinction is visible in `tests/control-contract.e2e.test.ts`: the test enters bad board and snapshot drafts, verifies the notices and retained text, then advances the clock and verifies that the prior game is still running. A test that expected elapsed time to remain fixed indefinitely after rejection would assert behavior the product does not have.

## The shared JSON boundary

The small helpers in `src/game/saved-format.ts` separate JSON mechanics from game validation:

| Helper | Meaning at this boundary |
| --- | --- |
| `isWithinBytes(text, maxBytes)` | Measures UTF-8 encoded length, inclusively |
| `parseJson(text)` | Returns parsed unknown data, or `undefined` after a parse exception |
| `isJsonObject(value)` | Accepts non-null objects except arrays |
| `hasExactKeys(value, keys)` | Checks own-key count and presence of every expected own key |
| `isNonnegativeSafeInteger(value)` | Accepts numeric safe integers greater than or equal to zero |

`JSON.parse` can produce `null`, a list, a string, a boolean or a number. Those are valid JSON texts and invalid snapshot or replay roots. The object guard is what distinguishes them from a malformed text error. This is especially visible in replay results: `{` produces `malformedJson`, whereas `null` produces `format`.

`parseJson` does not execute input as code. A string containing script-like characters is still a string and fails where an object or number is required. The decoder does not merge the parsed object into the live session wholesale. It constructs the accepted state from a generated board and the fields it has validated.

Exact keys are checked as a set, not as an ordering constraint. Missing fields fail as well as extra fields. Nested objects are checked at their own format boundaries. A `closed` selection with an `index` is invalid even if that index would be an eligible position in a different selection variant. A replay action with an annotation field is invalid even if `atMs` and `index` are otherwise good.

The helpers operate on the parsed object, not the original token stream. JSON duplicate property names are consequently handled by `JSON.parse` before exact-key checking; the decoder does not retain a record of earlier overwritten occurrences. Likewise, numeric notation such as `1.0` becomes the number `1`. The integer predicate checks the value it receives rather than enforcing a preferred spelling of a JSON number.

## Resource bounds are checked at the text boundary

Snapshot decoding measures its input before parsing and rejects text above `SNAPSHOT_MAX_BYTES`, which is `65_536`. Replay reading checks `REPLAY_MAX_BYTES`, which is `1_048_576`, before parsing. The replay parser then rejects more than `REPLAY_MAX_ACTIONS`, which is `10_000`. Trailing JSON whitespace is legal within the byte budget and still consumes that budget.

These checks bound the accepted format, but they should not be described as a streaming parser. `TextEncoder` allocates an encoded representation for measurement. The CLI also calls `readFile` before `readReplay` checks text size. A very large input can therefore already have been read into memory when it is rejected. The code provides explicit acceptance limits rather than a promise that arbitrary input has no allocation cost.

The browser textarea's `maxLength` is a separate character limit. Multibyte text can satisfy that limit and exceed the decoder's byte limit. The existing byte helper test uses an accented character to establish that bytes, rather than JavaScript string length, control acceptance. [Snapshot format](snapshot-format.md) discusses how this interacts with readable export text.

## Snapshot checks depend on the regenerated deal

After validating the version and board code, `decodeSnapshot` generates the board used to interpret positions. `readMatched` checks unique in-range positions and requires both copies of every represented face. An even-length list alone is not enough: on `np1-4-1`, `[2, 6]` is a pair while `[0, 1]` is not.

`readSelection` checks availability against that matched list. A mismatch needs different positions with different faces and an integer remaining interval from `1` through `800`. It cannot describe a same-face pair awaiting closure, because normal play resolves matching cards immediately. When every position is matched, availability also prevents any nonclosed selection.

The full decoder checks counter bounds, a minimum move count and consistency with `started`. These checks establish the stated constraints without reconstructing every earlier action. [Snapshot format](snapshot-format.md) gives the complete validation sequence; [Snapshot selection notes](snapshot-selection-notes.md) explains the board-dependent helpers.

## Replay checks and isolated evaluation

`readReplay` checks root shape, version, board code, action count and `endMs` before walking actions. Each action then needs its exact object shape, safe nonnegative numbers, a nondecreasing timestamp no later than `endMs`, and a position inside the deck. Its index into the action array is included in action-specific failures.

Evaluation can already have computed earlier actions when a later action fails. That is safe because the evolving state is local to `readReplay`; no browser session has been committed. The function returns a problem rather than that partial state. This is different from a streaming UI that visibly applies each imported action as it reads.

An in-range activation during a mismatch is valid replay data even though the game ignores it. So are an activation of an already matched card and a repeated activation of the current first card. Validation checks whether the action is well formed; `activateCard` decides whether it changes play. Treating every ignored activation as a corrupt replay would conflate these two jobs.

Action timestamps may be much larger than the elapsed clock cap. The replay evaluator advances the logical timeline using safe integers, while `advanceGame` caps the resulting elapsed counter. That distinction prevents a legitimate long timeline from being rejected merely because the visible clock has saturated. [Replay format](replay-format.md) explains exact timeout edges and equal-time order.

## Failure reports that preserve the evidence

For current snapshot failures the decoder deliberately supplies no reason. The most effective local investigation isolates a single change against a known accepted object: replace just the selection, just the code, or just one counter. The tests in `snapshot.test.ts` use this pattern so that an unrelated invalid baseline cannot hide a missing check.

Replay reports have more detail. `readReplay` distinguishes file size, JSON syntax, root format, board code, actions array, end time, individual action shape, time order and index bounds. The CLI translates these into stderr text and uses a one-based action number for readers. Internal action positions remain zero-based. Mixing these conventions can lead an engineer to inspect the wrong action.

Retaining original text matters when a failure depends on bytes or syntax. Parsing and reserializing a padded document can remove the very length excess that caused its rejection. A screenshot of a notice cannot establish whether an extra property was nested or at the root. For the current accepted shapes, use [Board code format](board-code-format.md), [Snapshot format](snapshot-format.md) and [Replay format](replay-format.md); for the browser steps around them, use [Sharing FAQ](sharing-faq.md).
