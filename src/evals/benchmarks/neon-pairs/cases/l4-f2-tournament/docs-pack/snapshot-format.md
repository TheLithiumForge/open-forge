# Snapshot format
This page documents version 1 progress snapshots and the validation helpers in `src/game/snapshot.ts` and `src/game/saved-format.ts`.

A snapshot is a position on a deterministic deal. It stores enough state to continue play without storing a duplicate deck or a sequence of earlier actions. `encodeSnapshot(state)` produces readable JSON; `decodeSnapshot(text)` returns an accepted `GameState` or `null`. Browser pause behavior is added after decoding by the session adapter, so it is not part of the JSON object.

## Root object

`SNAPSHOT_VERSION` is `1`, and `SNAPSHOT_MAX_BYTES` is `65_536`. The required root fields are listed by `SNAPSHOT_KEYS`. There are exactly seven:

| Field | Accepted value | Role |
| --- | --- | --- |
| `version` | Number `1` | Selects this snapshot format |
| `boardCode` | Valid version 1 board-code string | Regenerates size, seed and deck |
| `matched` | Array of unique valid position indices forming whole face pairs | Records permanent matches |
| `selection` | One accepted selection object | Records unmatched revealed cards |
| `moves` | Integer from `0` through `999999` | Records completed two-card attempts |
| `elapsedMs` | Integer from `0` through `359999999` | Records active elapsed milliseconds |
| `started` | Boolean | Records whether the first valid reveal occurred |

The decoder requires exactly these fields. It does not accept a second copy of the deck, a supplied completion flag, a pair counter or a browser pause flag. The board code determines deck length; matched positions determine the pair count and whether the game is won. These derived values cannot disagree with separately supplied copies because no such copies are accepted.

Root property order has no semantic effect during decoding. The exporter writes `version`, `boardCode`, `matched`, `selection`, `moves`, `elapsedMs`, `started`, using a two-space indent. Reformatting the JSON does not alter its meaning provided the result stays within the byte limit and preserves the same values.

## A complete position

This is the example from the reproducibility specification:

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

The generated deck has face six at both positions `0` and `11`, so `matched` contains one complete pair. Positions `1` and `2` have different faces and are not matched. They are pending for another 300 active milliseconds. There are enough moves to account for one matched pair and the pending miss.

Loading this object in the browser displays that position paused. Reading it through `decodeSnapshot` alone produces a state whose selection still contains the 300ms interval; the returned `GameState` has no `paused` property. The browser's `replaceGame` creates the pause in its enclosing `Session`. That separation lets the same accepted position be used in a pure test without emulating a Resume control.

## Selection variants

A selection is one of three exact object shapes:

```json
{"kind":"closed"}
```

```json
{"kind":"one","index":2}
```

```json
{"kind":"mismatch","first":0,"second":1,"remainingMs":800}
```

`closed` means that there is no revealed unmatched card. It can describe a fresh deal, a state after a matched pair, a state after a miss has closed, or a completed game. Those situations are distinguished by the other fields rather than by extra closed-state labels.

`one` means that a first eligible card has been revealed. The index must be inside the generated deck and absent from `matched`. This first reveal does not cost a move. In particular, `started: true`, `moves: 0`, no matches and a `one` selection is the normal state immediately after starting play.

`mismatch` means two different unmatched positions with different faces are temporarily revealed. Their remaining time is an integer from `1` through `800`. Zero is not a pending interval: once the remaining time reaches zero, the game represents the selection as closed. Two cards with the same face cannot be a pending mismatch because `activateCard` resolves that pair immediately.

`readSelection` and `readMatched`, both in `snapshot-selection.ts`, implement the board-dependent parts of these checks. [Snapshot selection notes](snapshot-selection-notes.md) gives a closer look at their construction and the edge cases that are easy to confuse.

## Matched positions are a set represented by an array

Every matched index is a nonnegative safe integer smaller than the deck length. It must appear only once. The decoder counts generated face identities at those positions and requires every represented face to occur twice. An empty array is valid; an even number of positions is necessary but insufficient.

The array does not need to be sorted on input. `readMatched` preserves its supplied order in a newly constructed array. The exporter sorts a copy numerically with `(left, right) => left - right`. That gives stable output without mutating the live state's order of discovered matches.

For example, play might produce `matched: [6, 2]` on the example board. Export writes `[2, 6]`, and decode retains `[2, 6]`. This is a successful semantic round trip even though the arrays are not equal in order. Code that treats discovery order as part of the snapshot contract would infer information the format does not retain reliably.

Numeric sorting is significant on larger boards. Lexicographic string ordering would put position `10` before position `2`; the implementation explicitly uses numeric subtraction. Sorting is an export presentation choice, not a prerequisite for acceptance and not a way to repair duplicate or incomplete matches.

## Counter checks and consistency

The shared safe-integer helper rejects strings, fractions, negative numbers and numbers outside JavaScript's safe integer range. Snapshot-specific checks then apply the tighter move and elapsed caps. A value spelled as JSON `1.0` parses to integer `1` and passes that numeric test; a value spelled as `"1"` remains a string and fails. The boundary concerns values, not a canonical spelling for numeric tokens.

After reading matches and selection, `decodeSnapshot` calculates:

```text
minimumMoves = matched.length / 2 + (selection.kind === 'mismatch' ? 1 : 0)
```

The supplied move count cannot be below that result. One move is needed for every matched pair, and a pending miss needs one further move. An extra revealed first card does not require another move because the move is counted on the second eligible reveal.

This is a lower bound, not a full action-history validator. A closed started state with no matches and 17 moves is accepted. It could describe previous misses, but the decoder does not reconstruct those misses. The source tests explicitly accept such a state. The format supports local reproduction and hand-authored fixtures rather than proof of a played score.

Elapsed time is independently bounded. There is no rule that a certain move count requires a minimum positive number of milliseconds. Multiple eligible activations can occur at the same logical replay time, so zero elapsed time can accompany completed moves. Likewise, the decoder does not compare elapsed time against the age of a pending mismatch to reconstruct exactly when it began.

## The `started` flag has real constraints

When `started` is false, all progress must be initial: zero moves, zero elapsed time, no matched positions and a closed selection. This prevents an object from combining hidden elapsed progress with an unstarted clock. It also ensures that importing an unstarted deal has a coherent state to resume.

When `started` is true and moves are zero, a closed selection is rejected. With zero moves the minimum-move check already prevents matches and a pending mismatch, leaving the valid one-card case. That is the point at which play has started but no two-card attempt is complete.

Completion is derived from `matched.length === deck.length`. A fully matched state necessarily has a closed selection because every position is unavailable to `readSelection`. It also has at least one move for every pair through the lower-bound check, and it cannot be unstarted. The decoder gets those consequences from its normal validations rather than a separate serialized completion tag.

A fully matched four-column example can use all indices `0` through `15`, moves `8`, any accepted elapsed count and `started: true`. This is accepted data; it is not a claim that an imported file has independently verified those moves.

## Decoder sequence

The order in `decodeSnapshot` is useful when diagnosing failures:

1. Measure text against the inclusive UTF-8 byte limit, then parse JSON if it fits.
2. Require a JSON object, the exact root keys and version `1`.
3. Require a string `boardCode` accepted by `parseBoardCode`.
4. Check move and elapsed values against safe-integer and format caps, and require a boolean `started`.
5. Call `createGame(boardCode)` to obtain a validated initial board and deck.
6. Read matched positions, then the selection against that matched list.
7. Construct the state and check minimum moves and started-state consistency.
8. Return the accepted state.

Every failed branch returns `null`. The function does not expose a partial state or structured error. Its caller cannot distinguish a bad counter from a malformed selection by inspecting the return value. Current browser messaging is described in [Import safety](import-safety-policy.md).

Creating the initial game in the middle of validation is intentional. Position range and face pairing cannot be checked against the sender's implied assumptions; they need the actual deck from the code. A forged `size` field cannot influence those checks because the field is not accepted in the root object.

## Export takes a copy at a point in time

`encodeSnapshot` builds a new object, copies and sorts the matched array, and serializes the selection. For a mismatch it copies the selection while replacing `remainingMs` with `Math.ceil(...)`. `elapsedMs` is written with `Math.floor(...)`. Moves and started are written directly.

The encoder assumes a legitimate core state. It does not call `decodeSnapshot` on its output or sanitize arbitrary JavaScript objects passed as though they were a `GameState`. In normal use its caller has a state created by the game functions or by an accepted import. The decoder, not the encoder's TypeScript signature, is the trust boundary for exchange text.

Live clock fractions stay in memory. Suppose a mismatch has 0.25ms left: export writes `remainingMs: 1`, keeping it pending and acceptable. If elapsed time is 810.75ms, export writes `810`. Each quantization differs from the internal value by less than one millisecond for these valid live values. Export does not close the mismatch early to make its duration integral.

A pending interval of exactly zero is not rounded up by this rule in legitimate state, because `advanceGame` has already changed its kind to `closed`. The distinction is between preserving a positive fractional interval and manufacturing one for an already closed miss.

The browser's export handler calls `clock.synchronize()` before encoding `current.current.game`. Rendered state can lag active time by a timer tick. Synchronization accounts for the real elapsed interval, including a mismatch that has become due, before the point-in-time copy is produced. The export operation does not introduce a pause; normal clock progression continues afterward.

## Text measurement and parsing details

`isWithinBytes` uses `new TextEncoder().encode(text).byteLength <= maxBytes`. It measures the entire text supplied to the decoder, including indentation and trailing whitespace. A valid snapshot padded to exactly 65,536 UTF-8 bytes is accepted, and appending one more ASCII space is rejected. The existing tests exercise both endpoints.

`parseJson` catches exceptions and returns `undefined` for malformed input. No JSON text can parse to `undefined`, so that value is an unambiguous failure marker for this helper. A literal `null` is different: it parses successfully but fails `isJsonObject`. Arrays are also excluded explicitly even though JavaScript describes them as objects.

`hasExactKeys` combines an `Object.keys` count with `Object.hasOwn` for each required key. It does not care about enumeration order. At this string boundary the input has already been produced by `JSON.parse`; the helper is not a general schema engine or a tokenizer that remembers duplicate property occurrences. The same helpers serve replay parsing with different field lists and limits.

## Import lifecycle is outside the wire shape

`loadSnapshot` only commits after successful decoding. It resets the browser clock and calls `replaceGame` with `imported: true`. Unfinished imports receive a paused session and the notice `Loaded snapshot. Select Resume to continue.` Completed imports receive `Loaded completed snapshot.` and no pause.

That includes an initial-state snapshot: even if its `started` flag is false, it is unfinished and loads paused. Resume removes the import pause, after which a first valid activation still starts the elapsed clock. An already started import resumes counting from its saved elapsed value. A mismatch continues from its saved remaining interval rather than restarting at 800ms.

No pause flag is exported. If a player exports while an imported game is paused, the position remains static during export, but the resulting JSON still only describes game progress. Loading that JSON elsewhere will independently create the receiver's import pause. Browser hidden-page time is also not serialized; elapsed time is already active time.

Importing a completed position does not call the played winning-turn path or start its celebration. Completing an unfinished imported position later through play does use normal session completion handling. These are documented browser effects, not additional snapshot fields. See [Session module](session-module.md) and [Sharing FAQ](sharing-faq.md).

## Reading evidence without overclaiming round trips

`snapshot.test.ts` covers a pending mismatch round trip, exact exported fields, numeric sorting of matches, complete states, invalid field combinations, byte limits and fractional export time. `saved-format.test.ts` covers UTF-8 measurement, malformed text, root object guards, exact keys and safe-integer boundaries. Selection-focused tests add variant-specific rejection cases.

A deep-equality round trip works for some integral states, but is too strong as the general contract. Match order can be normalized and time fractions are quantized. A useful comparison checks the board, match membership, selection identity, moves, started flag and the documented millisecond conversion. Conversely, merely seeing valid JSON is too weak: it says nothing about pair identity or counter consistency.

For format changes, the seven-key root and three selection variants are the starting point. Adding a property without changing the reader causes version 1 rejection. Changing a derived behavior can alter decoded play even without changing any key. [Format versioning](format-versioning-guide.md) discusses both cases.
