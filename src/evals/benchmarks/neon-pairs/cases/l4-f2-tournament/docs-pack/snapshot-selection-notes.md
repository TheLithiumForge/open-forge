# Snapshot selection notes
This page explains the board-dependent validation in `src/game/snapshot-selection.ts`, especially where selection and matched positions constrain each other.

These are implementation notes for reading or changing the decoder. The complete wire shape belongs in [Snapshot format](snapshot-format.md). The two exported helpers here are `readMatched(value, state)` and `readSelection(value, state, matched)`. Both accept unknown input and return accepted game data or `null`; neither changes the supplied state.

## The order of the two reads matters

`snapshot.ts` regenerates an initial game from the accepted board code, reads the matched list, and then reads the selection using that list. A selection index is only eligible if it is not already matched. Reading the selection before validating matches would require either postponing that membership check or trusting an unvalidated array.

The `state` argument supplies deck length and face identities. The helpers do not rely on its move count, elapsed time or started flag. Those are checked by the outer decoder after it has both helper results. Although the parameter is typed as `GameState`, its role at this boundary is chiefly to provide the board against which positions can be interpreted.

This division is visible with a one-card selection. `readSelection({kind: 'one', index: 2}, state, [])` is valid on the example board regardless of whether a caller intends to claim the game is unstarted. Only `decodeSnapshot` can reject the whole object for combining that selection with `started: false`. A helper-level test is not a substitute for a full snapshot test.

## Matched validation, line by line

`readMatched` first requires an array. It then creates a new `matched` array and a `Map<number, number>` of counts by face. For each supplied position it checks that the value is a nonnegative safe integer, is below the deck length, and has not already been appended. It reads the generated face, appends the position, and increments that face's count.

At the end, every count in the map must be exactly two. Since a generated deck contains each face exactly twice, this is equivalent to asking for complete face pairs. It does not require pair members to be adjacent in the supplied array. It also does not infer pairs from the position numbers themselves.

The empty array succeeds. The map then has no counts that violate the condition, and the returned empty array is truthy in JavaScript. The outer decoder's `if (!matched)` correctly distinguishes `null` from an accepted empty list. Confusing an empty list with failure in a port would make fresh-state snapshots impossible to load.

The helper copies values into its own array and preserves input order. It does not sort, freeze or reuse the caller's array. The returned type is readonly, but that TypeScript annotation is distinct from the runtime freezing used for a generated deck. Export sorting belongs to `encodeSnapshot`, which sorts a separate copy.

Some useful examples on `np1-4-1`:

| Input | Result | Explanation |
| --- | --- | --- |
| `[]` | `[]` | No complete pairs claimed |
| `[2, 6]` | `[2, 6]` | Both copies of face zero |
| `[6, 2]` | `[6, 2]` | Same pair, input order retained |
| `[0, 2, 11, 6]` | Same array values | Interleaved complete pairs for faces six and zero |
| `[2]` | `null` | One copy of a face |
| `[0, 1]` | `null` | Two positions, different faces |
| `[2, 2]` | `null` | Position repeated |
| `[2, 6, 2, 6]` | `null` | Complete pairs cannot be repeated |
| `[2, 16]` | `null` | Position 16 is outside this deck |

The loop also rejects an undefined face after the bounds check. For an ordinary generated dense deck that branch is defensive: a valid index has a face. Keeping the check local means the helper does not turn a malformed manually supplied state into a match count for an undefined value.

## One shared availability predicate

Inside `readSelection`, `available(index)` checks three things: a nonnegative safe integer, an index below `state.deck.length`, and absence from the accepted matched list. Its TypeScript type predicate narrows an unknown input to a number after those checks.

The predicate does not ask whether a position's face is elsewhere in the matched list. That is unnecessary after `readMatched`: a valid matched face has both of its positions already present. Index membership therefore excludes every position of every matched face. If a caller supplies an invalid matched list directly, it has bypassed the intended read order.

The helper first requires a non-null non-array object and then switches on `value.kind`. `SelectionKind` provides the three string discriminants, keeping accepted names aligned with the game core. Any other kind returns `null`. The result is reconstructed as a known variant rather than passing the parsed object through.

## Closed is the smallest variant

A closed selection is accepted only with the key `kind`. There is no `index`, no empty array of selected cards and no remaining-time field. The returned value is a new `{kind: SelectionKind.closed}` object.

The shape is intentionally unable to say why the selection is closed. It may be an untouched board, a just-found pair, an expired mismatch or a win. The matched list and outer counters carry that context. A decoder change that adds a second “closed after miss” shape would have no existing field to populate and would change the accepted format.

A closed selection by itself is not enough to establish a valid snapshot. For example, the outer decoder rejects a closed, started game at zero moves. `readSelection` accepts the shape because it has no responsibility for move history. This small scope makes its failure cases easier to reason about.

## One means an unpaid first reveal

The `one` variant has exactly `kind` and `index`. That index must pass availability. The returned object contains only the known fields, preserving the numeric position.

There is no minimum positive move count at this point. A first reveal starts the clock and sets `started`, but a move is counted on the next different eligible reveal. A full snapshot with no matches, `started: true`, zero moves and index `2` revealed is therefore valid. A full snapshot with the same fields and `started: false` is invalid.

A one-card selection may coexist with already matched pairs as long as its index is different from every matched position. For `matched: [2, 6]`, selecting `0` is eligible and selecting `2` is not. The outer minimum-move calculation counts the pair but does not charge the extra first reveal as another attempt.

## Mismatch has three independent checks

The exact key set is `kind`, `first`, `second`, `remainingMs`. Both positions need to be available, and they must be distinct. The generated faces then need to differ. Finally, remaining time needs to be an integer in `1..MISMATCH_MS`, where `MISMATCH_MS` is `800` from `game.ts`.

Distinct positions alone do not establish a miss: positions `2` and `6` are different but contain the same face. Different faces alone do not establish an acceptable selection: either position might already be matched or out of range. The checks protect separate invariants and cannot be replaced by one comparison.

The lower remaining-time endpoint is included. A one-millisecond mismatch is a real pending state, including the result of rounding a positive sub-millisecond interval upward during export. The upper endpoint is included too because a miss has the full interval immediately after its second reveal. Fractional remaining time is accepted in live state, but not in snapshot JSON.

`first` and `second` retain their supplied roles. Swapping them would show the same two faces, but the helper does not normalize them. The game records the first-revealed and second-revealed position in that order. Export preserves those fields rather than sorting them the way it sorts matched positions.

## Completion falls out of availability

When every position is matched, no position can pass `available`. That rejects both the one-card and mismatch variants without a special completion branch. Closed remains possible. The outer decoder's move and started checks then complete the consistency validation for a won game.

This is useful when reviewing a reported missing validation. The code does not contain a literal `isWon` check in this module, but completion with a revealed unmatched selection is still impossible after a valid full matched list. Adding a duplicate branch might make the invariant easier to see, yet its absence is not by itself a bug.

Conversely, this module cannot establish that a claimed move count is sufficient. A full matched list and a closed selection can be accepted here while the full snapshot is rejected for zero moves. The helpers establish positional consistency, and `snapshot.ts` establishes consistency across those positions and the other fields.

## Tests and a few traps for maintenance

`snapshot-selection.test.ts` tests extra keys on all three variants, remaining-time endpoints and a fractional interval, plus half-pair and duplicate-position failures. `snapshot.test.ts` reaches these helpers through the full decoder and adds matched/selected overlap, same-face pending pairs, invalid bounds and counter combinations.

A good isolated failing case starts from an accepted baseline. For example, a pending mismatch with `moves: 0` already fails at the outer lower bound; it cannot prove that a bad `remainingMs` value was rejected by this helper. The tests use positive moves and `started: true` when isolating interval validation.

There is no generic recursive object validator here. Each selection branch knows its own key list and semantics. That makes the relationship to `activateCard` direct: closed opens one card; one either matches and closes or becomes a mismatch; mismatch closes after active time. If a format change affects one of those meanings, review the transition functions as well as these read helpers. [Turn resolution](turn-resolution.md), [Mismatch interval](mismatch-interval.md) and [Format versioning](format-versioning-guide.md) provide the adjacent context.
