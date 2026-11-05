# Numbers style

This page explains how Neon Pairs presents counts, positions, durations and numeric values without confusing display text with saved data.

Numbers cross several boundaries in this game. The status bar shows compact values, accessible card names describe human positions, and snapshots carry machine-readable state. The same quantity can have different representations at those boundaries. The examples below come from `game.ts`, `elapsed-time.ts`, the status component and the version 1 readers; the underlying behavior belongs in [Scoring and moves](scoring-and-moves.md) and [Elapsed time rules](elapsed-time-rules.md).

## Counts in sentences and counts in a display

The current status component separates labels from values using a description list. `StatusLabel.moves` supplies “Moves:” and the adjacent value is `game.moves`. It is a compact display rather than a sentence about the count. Counts of moves read "1 move" and "2 moves", and durations read M:SS, such as 0:07 and 12:30. In prose, a unit makes an otherwise bare number understandable; beside an existing status label, the label already does that work.

This distinction is visible in `src/game/status/GameStatus.tsx`. Moves and best moves are plain numbers. Pairs are a matched count followed by the total. Time passes through `formatElapsed`. A missing best is the word “none”, not a numeric zero. Zero moves describes a real game state; no recorded best describes the absence of a record. Treating those as interchangeable would obscure the difference between starting a game and having completed one before.

The generated completion sentences currently interpolate `${game.moves} moves`. They do not call a general singular/plural helper. A legal completed board needs at least eight moves, so the completion surface does not encounter a one-move finish. The general prose convention above is therefore not evidence that a shared move-count formatter already exists.

The session does handle the singular pair count explicitly. It produces “Pair found. 1 pair matched.” for the first match and uses “pairs” for later nonfinal matches. The code tests `game.matched.length === 2`, because the stored list contains card positions. Counting array entries as if they were pairs would double the reported progress.

## A compact status example

A four-column game with one matched pair, two completed turns and a stored best of twelve moves has these count values:

| Surface | Example | Meaning |
| --- | --- | --- |
| Moves | `Moves: 2` | Two second-card reveals have resolved turns. |
| Pairs | `Pairs: 1/8` | One pair is matched out of eight pairs. |
| Best | `Best: 12` | The saved record for this size is twelve moves. |
| Best without a record | `Best: none` | This size has no saved result. |

The slash in the pair display expresses progress against a total. It is not a fraction used to calculate a percentage, and the UI does not show a percentage alongside it. A report that says “one of eight pairs” is a readable expansion of the same value. A report that says “one of sixteen pairs” has mixed the card total with the pair total.

## The clock's representation

`formatElapsed(milliseconds)` floors the total seconds, derives total minutes, then pads the seconds component to two digits. Minutes continue growing beyond an hour. The display is an elapsed duration, so there is no time zone, calendar date or time-of-day interpretation to supply.

| Active elapsed milliseconds | Formatted value | What the boundary illustrates |
| --- | --- | --- |
| `0` | `0:00` | An unstarted or just-started clock. |
| `999` | `0:00` | A partial second does not round upward. |
| `1000` | `0:01` | The first complete second. |
| `7000` | `0:07` | Seconds retain a leading zero. |
| `59999` | `0:59` | The minute changes only after its full interval. |
| `60000` | `1:00` | Seconds return to zero at a minute boundary. |
| `750000` | `12:30` | A longer ordinary game duration. |
| `3600000` | `60:00` | Minutes continue past fifty-nine. |
| `359999999` | `5999:59` | The game's elapsed-time ceiling. |

These values describe the formatter's result, without the `Time:` label added by the status bar. The completion banner uses the same formatter. A documentation screenshot may show an earlier rendered tick than an exported snapshot captures, because export synchronizes the active clock before encoding it. That difference does not imply two different elapsed-time formats.

## Active time and serialized milliseconds

The elapsed display omits subsecond detail, while game state retains it during play. `encodeSnapshot` floors `elapsedMs` to a whole millisecond for export. A pending mismatch's `remainingMs` is rounded upward instead. Those two directions serve different fields: flooring elapsed time records completed whole milliseconds; rounding remaining time upward avoids writing a pending interval as zero.

For example, a state with `elapsedMs` equal to `1500.75` exports `1500`. A mismatch with `remainingMs` equal to `0.25` exports `1`. The textarea does not contain the clock's formatted string in place of `elapsedMs`; it contains a numeric JSON field. The display can show only a whole second while the exported state still distinguishes positions within that second.

The usual mismatch begins with `800` active milliseconds remaining. Time while the page is hidden or while an imported unfinished snapshot awaits Resume does not consume that interval. Thus “800 milliseconds after the second reveal” is incomplete if it suggests uninterrupted wall time. “800 active milliseconds” is the precise phrase used when the timing distinction matters.

In engineering prose, the `Ms` suffix on `elapsedMs`, `remainingMs`, `atMs` and `endMs` identifies the unit. It is part of each real field name. Replacing `elapsedMs` with a friendlier invented key in a JSON example would change the format, rather than just improve its wording. [Snapshot format](snapshot-format.md) and [Replay format](replay-format.md) own the complete schemas.

## Board dimensions, card totals and pair totals

The control options are `4x4`, `6x6`, `8x8`, `10x10`, `12x12`, `14x14` and `16x16`. Their underlying select values are the side lengths. The visible `x` is an ordinary letter in the current labels. Descriptive prose can say “a board with four rows and four columns” when explaining the meaning, while an instruction naming the option uses the actual option text.

| Side length | Cards | Pairs |
| --- | --- | --- |
| 4 | 16 | 8 |
| 6 | 36 | 18 |
| 8 | 64 | 32 |
| 10 | 100 | 50 |
| 12 | 144 | 72 |
| 14 | 196 | 98 |
| 16 | 256 | 128 |

The formula is `N * N` cards and half that many pairs. The size is not the number of pairs, and a face ID is not a card's position. Those distinctions matter when a bug report mentions “card 8” without saying whether it means a row-order index, a visible location or an artwork identity. [Board size catalogue](board-size-catalogue.md) covers dimensions; [Pair identity](pair-identity.md) covers what is being matched.

## Human positions and zero-based indices

Deck positions run from zero in row order. Accessible card names use rows and columns starting at one. For a size `N`, the name uses `floor(index / N) + 1` for the row and `(index % N) + 1` for the column. This is a conversion between two coordinate systems, not two different deck orders.

On a `4x4` board, index `0` is row 1, column 1; index `6` is row 2, column 3; and index `15` is row 4, column 4. A replay action's `index: 6` therefore targets the third card in the second row. It does not target a card whose face happens to be 6.

The replay CLI has another human-facing number: an action ordinal in an error. Internally, `readReplay` reports a zero-based action position. `runReplay` adds one when composing “action N” for a person reading stderr. That ordinal identifies the action's location in the list, while the action's `index` identifies a card on the board. An error about action 2 can therefore concern card index 0 without any contradiction.

## Limits are different kinds of numbers

`MOVES_MAX` is `999_999` in TypeScript and `999999` as a plain rendered count. `ELAPSED_MAX_MS` is `359_999_999`. The separators help a source reader scan literals; they are not included in JSON numbers or the current status display. Documentation discussing the constant can show the source spelling, while a literal exported example uses JSON's numeric syntax.

Reaching either gameplay ceiling does not end the game. Moves stop increasing at their cap, and elapsed time stops increasing at its cap, while cards can still be matched and the board can still be completed. “Maximum moves” is consequently different from “a move allowance”. The game does not count down to a loss condition.

Input sizes have a separate distinction. The board-code input has a `maxLength` of 32. The snapshot textarea has a `maxLength` of 65536, and the decoder also checks a UTF-8 byte limit of 65536. Character length in a DOM control and encoded byte length are different measurements, even when their limit has the same numeral. A multibyte character is enough to demonstrate why the second check still matters.
