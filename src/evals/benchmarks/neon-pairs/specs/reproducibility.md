# Reproducibility
The reproducibility specification is a set of versioned formats that recreates Neon Pairs deals, progress and actions.

Status: spec version 2. Board codes, snapshots and replays keep format version 1.

A board code reproduces the starting cards. A snapshot restores a later position. A replay shows the actions that produced a position, so each format answers a different debugging question.

## Board code

- D01 - Use `np1-N-seed`, where `N` is a supported side length and `seed` is an integer from `1` through `2147483646`. Use decimal digits without leading zeros. The parser accepts exactly this form. A text control can trim surrounding whitespace before parsing.
- D02 - Version `np1` fixes the generation algorithm below. The same code produces the same deck in every supported runtime. Changing generation requires another version. Time, animation and other random draws can't change this deck.

For example, `np1-4-1` describes a 4 by 4 board with generation version 1 and seed 1. It contains no progress.

## Generate

- D03 - Start the Park-Miller generator at `state = seed`. Each draw sets `state = (state * 16807) % 2147483647`. Use exact integer arithmetic. Seed 1 starts with states `16807`, `282475249`, `1622650073`, `984943658` and `1144108930`.
- D04 - Create the deck as `[0, 0, 1, 1, ...]` through face `N * N / 2 - 1`. Shuffle from index `N * N - 1` down to `1`. At index `i`, draw `j` in `0..i` and swap positions `i` and `j`.

For a draw with `b` possible results, set `q = 2147483646` and `limit = floor(q / b) * b`. Advance the generator and set `x = state - 1`. If `x >= limit`, draw again. Otherwise return `x % b`. This rejection step gives every result the same number of generator values.

The resulting deck for `np1-4-1` is `[6, 7, 0, 5, 2, 2, 0, 5, 3, 7, 1, 6, 4, 4, 1, 3]`.

## Snapshot

- D05 - Encode progress as a JSON object with exactly `version`, `boardCode`, `matched`, `selection`, `moves`, `elapsedMs` and `started`. `version` is `1`. `matched` is an array of matched position indices. Derive the deck, pair count and completion from these fields.

The `selection` object has one of these forms:

| `kind` | Other fields | Meaning |
| --- | --- | --- |
| `closed` | None | No unmatched card is revealed |
| `one` | `index` | One unmatched card is revealed |
| `mismatch` | `first`, `second`, `remainingMs` | Two different faces await closing |

- D06 - Validate the complete object before replacing the game. Reject unknown keys. Require unique in-range matched indices containing whole face pairs. Selection indices must be in range and unmatched. A mismatch needs distinct indices with different faces and an integer `remainingMs` from `1` through `800`. Completion requires a closed selection.
- D07 - Require boolean `started` and nonnegative safe integers for `moves` and `elapsedMs`. Reject `elapsedMs` above `359999999` and `moves` above `999999`. Apply the move cap to saved best scores too. Require at least one move per matched pair and one further move for a pending mismatch. If `started` is false, require zero moves, zero elapsed time, no matches and a closed selection. A started game with zero moves requires one revealed card. Import pauses the clock and pending mismatch until Resume.

Illustrative snapshot:

```json
{"version":1,"boardCode":"np1-4-1","matched":[0,11],"selection":{"kind":"mismatch","first":1,"second":2,"remainingMs":300},"moves":2,"elapsedMs":1500,"started":true}
```

Importing this snapshot shows one matched pair and two different faces. Resume lets those faces close after another `300` active milliseconds.

## Replay

- D08 - Encode a single deal as `{version: 1, boardCode, actions: [{atMs, index}], endMs}`. Require exactly these fields and exactly `atMs` and `index` per action. Indices must be in range. Times are nonnegative safe integers. Action times must be nondecreasing. If actions exist, `endMs` must be at least the final action time. Equal-time actions keep array order.
- D09 - Start from the code's hidden deal. Advance active time to each action's `atMs` before activating its index, then advance to `endMs`. Resolve a mismatch whose interval ends exactly at an action time before that action. Apply the game rules to ignored activation too. Cap replayed elapsed time at `359999999` milliseconds as R07 requires. Action times and `endMs` keep the nonnegative safe integer range in D08.

Illustrative replay:

```json
{"version":1,"boardCode":"np1-4-1","actions":[{"atMs":0,"index":0},{"atMs":0,"index":11}],"endMs":800}
```

This replay ends with one matched pair, one move and `800` elapsed milliseconds. Replay timestamps use a logical timeline. The game's elapsed clock still starts at its first valid activation. Replay omits hidden-page and imported-pause time. A new deal starts another replay.

## Limits

- D10 - Limit snapshot JSON to `65536` UTF-8 bytes. Limit replay JSON to `1048576` UTF-8 bytes and `10000` actions. Reject malformed JSON and unsupported format versions. For snapshots, inconsistent data means a violation of D05, D06 or D07. For replays, it means a violation of D08 or D09. For board codes, validate D01. Validation checks these stated constraints, without requiring a reconstructed history of earlier turns. Invalid board codes and snapshots keep the current browser game. Invalid replay files are rejected by the CLI and game core.

These local formats and saved scores are untrusted input. Validation checks the state rules, not a verified play history. The game has no accounts or trusted leaderboard.

Turn and clock behavior is defined in [Rules](rules.md). Return to [Neon Pairs](README.md) for the full specification.
