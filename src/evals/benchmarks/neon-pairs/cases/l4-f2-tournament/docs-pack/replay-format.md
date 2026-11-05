# Replay format
This page documents version 1 replay input, its logical timeline and the developer command that converts it to a final snapshot.

A replay evaluates a single deal from its hidden starting state. Its actions are card activations, not precomputed turn outcomes. The evaluator uses the same `advanceGame` and `activateCard` functions as ordinary play, so ignored actions, immediate matches, mismatch intervals and counter caps have their normal meanings. The format does not record focus movement, page visibility events or a browser pause.

## Wire shape

The root object has exactly `version`, `boardCode`, `actions` and `endMs`. Version is the number `1`. `boardCode` is accepted by the same parser used for snapshot board codes. `actions` is an array containing at most 10,000 objects, each with exactly `atMs` and `index`. `endMs` is the time at which evaluation stops after processing actions.

```json
{
  "version": 1,
  "boardCode": "np1-4-1",
  "actions": [
    {"atMs": 0, "index": 0},
    {"atMs": 0, "index": 11}
  ],
  "endMs": 800
}
```

This finds face six at positions `0` and `11` immediately. It ends with one matched pair, one move, a closed selection and 800 elapsed milliseconds. The final `endMs` is meaningful even though no action occurs at 800ms: it advances the active game after the last activation.

All action times, action indices and `endMs` must be nonnegative safe integers. Positions also need to be smaller than the generated deck length. Action times are nondecreasing, and no action can occur after `endMs`. Object-key order is immaterial, but action-array order is part of the replay's meaning.

`REPLAY_VERSION` is `1`, `REPLAY_MAX_BYTES` is `1_048_576`, and `REPLAY_MAX_ACTIONS` is `10_000`. The byte limit measures all UTF-8 text before parsing. The action limit counts array entries, including entries whose activation will be ignored by the game. The empty actions array is valid.

## Two clocks to keep distinct

The replay timeline begins at logical time zero. The game's elapsed clock begins at its first valid activation. They are related but not identical. Advancing an unstarted state does nothing, so waiting on the replay timeline before the first reveal does not count as played elapsed time.

Consider these actions and endpoint:

```json
{
  "version": 1,
  "boardCode": "np1-4-1",
  "actions": [
    {"atMs": 100, "index": 0},
    {"atMs": 150, "index": 1}
  ],
  "endMs": 200
}
```

The state is unstarted for the first 100 timeline milliseconds. Index `0` then starts play. Fifty milliseconds later index `1` creates a mismatch with its own fresh 800ms interval. Another 50ms passes before the endpoint. The result has `elapsedMs: 100`, one move and 750ms remaining on the miss. The leading idle period accounts for the difference between elapsed time and `endMs`.

Completion also separates the clocks. Once every pair is matched, further `advanceGame` calls return the completed state without increasing elapsed time. A replay can end much later than its last winning action without making the game's elapsed result larger. Repeated activations after completion are well-formed in-range actions but cannot add another move or pair.

The elapsed counter saturates at `359999999`. Replay timestamps can reach `Number.MAX_SAFE_INTEGER`, which is `9007199254740991`. A long endpoint is still an accepted timeline value; it is not limited to the elapsed cap. The mismatch countdown continues using the time delta even when the elapsed display has reached its maximum.

## Evaluation order in `readReplay`

After initial validation, the reader calls `createGame(boardCode)` and sets `previousTime = 0`. For each action it validates that action, computes `action.atMs - previousTime`, advances the state by that delta, and then activates the action's index. It updates `previousTime` to the action time. After the loop, it advances by `endMs - previousTime` and returns the final state.

The operative expression is:

```typescript
state = activateCard(advanceGame(state, action.atMs - previousTime), action.index);
```

Advancing first is essential. If a mismatch expires at the action's timestamp, both old cards close before the new activation is considered. Switching those two calls would make the same boundary action be ignored, changing the position without changing any JSON field.

Time deltas are nonnegative because order is checked before evaluation. A zero delta is legitimate and `advanceGame` returns its input for zero. Nothing in the evaluator sorts the actions. It trusts the supplied order only after confirming that timestamps do not decrease, and then uses the array as the tie-breaker for equal times.

## Exact mismatch boundary

The following example isolates an activation just before and exactly at the closing time:

```json
{
  "version": 1,
  "boardCode": "np1-4-1",
  "actions": [
    {"atMs": 0, "index": 0},
    {"atMs": 0, "index": 1},
    {"atMs": 799, "index": 2},
    {"atMs": 800, "index": 2}
  ],
  "endMs": 800
}
```

The first two actions reveal different faces, costing one move. At time 799 there is one millisecond left, so the activation of position `2` is ignored. Advancing to time 800 closes the mismatch. The next activation of position `2` is now eligible and becomes the first card of a new turn. The result is `selection: {kind: 'one', index: 2}`, one move and 800 elapsed milliseconds.

The same boundary applies to the final advance. If the replay instead contains only the first two actions and ends at 799, it ends in a mismatch with one millisecond remaining. If it ends at 800, it ends closed. No explicit “close cards” action is needed or accepted in the action shape.

The browser may receive a delayed timer callback, but the replay has no scheduler delay. It models the logical rule. Browser synchronization tests separately establish that a click can resolve an overdue mismatch before activation even if a scheduled timer has not run. A successful replay is evidence about core order, while a browser journey is evidence about the adapter's time accounting.

## Equal times preserve array order

Three actions at time zero targeting `0`, `1` and `2` do not reveal three cards. The first two start a mismatch; the third sees that pending state and is ignored. Reordering the third action can change which mismatch is pending, even though every timestamp stays zero.

Likewise, an equal-time sequence targeting `0`, `11` and `2` first finds a pair and then opens position `2` as the next first card. A sequence targeting `0`, `0` and `11` ignores the repeated first card and then finds the same pair. Both examples show why equal-time actions are valid and why sorting by index would corrupt their meaning.

`replay-order.test.ts` directly checks the blocked third activation with `[0, 1, 2]` at equal time. `replay.test.ts` checks the exact timeout edge. These tests exercise transition semantics rather than merely accepting an ordered list of numbers.

## Valid actions that do nothing

An action is valid when it has the right fields, numbers, order and bounds. Whether it changes game state is a separate decision in `activateCard`. The core ignores an already matched position, the currently revealed first position, and all activations during a mismatch.

Keeping such actions is useful when reproducing input behavior. A report can include a click made during a blocked interval without pretending that the player made fewer inputs. The move count still reflects second eligible reveals, not raw action count. A replay containing 10,000 repeated activations of one position can finish with zero moves and one revealed card.

An out-of-range index is different: `readReplay` rejects it even though a direct call to `activateCard` would defensively return its input. The exchange format validates its action domain before invoking the core. This deliberate strictness gives an author feedback about a typo instead of silently discarding a position that does not exist.

## Result types and validation precedence

`readReplay(text)` returns a union with either `state` or `problem`. A problem may also have `action`, the zero-based position of the offending array entry. `replayGame(text)` is a convenience wrapper returning the state on success and `null` on any failure. Neither throws a format error for ordinary invalid input.

The reader's checks have this precedence:

| Problem | What is checked at that stage |
| --- | --- |
| `tooLarge` | UTF-8 byte limit before parsing |
| `malformedJson` | JSON syntax |
| `format` | Object root, exact root keys, numeric version 1 |
| `boardCode` | String type and board-code parser |
| `actions` | Array type and action-count limit |
| `endTime` | Nonnegative safe-integer endpoint |
| `action` | Per-action object shape and numeric types |
| `order` | Time before previous action or after endpoint |
| `index` | Position outside the generated deck |

A negative action index fails the safe-integer stage and yields `action`; an integer equal to deck length yields `index`. A fractional timestamp yields `action`; an integer timestamp later than `endMs` yields `order`. A malformed action is not evaluated, and the reader does not continue to collect every problem in the file.

Root problems have no action position. Earlier actions may have been evaluated when a later one fails, but the result exposes no partial state. The reader's evolving local state cannot alter a running browser game. [Import safety](import-safety-policy.md) covers the broader isolation model.

## Running the developer command

From a checkout with the locked tools installed, run:

```sh
npm run --silent replay -- ./case.json
```

The command accepts one path. The `--silent` flag suppresses npm's normal script banner so successful stdout is just JSON. The script entry is `scripts/replay.ts`, which passes `process.argv.slice(2)` to `runReplay` and assigns its numeric return value to `process.exitCode`.

`runReplay` in `scripts/replay-cli.ts` resolves the path against `process.env.INIT_CWD ?? process.cwd()` by default. npm runs scripts from the package folder but records the invoking folder in `INIT_CWD`; using that value makes a relative path refer to the caller's location. The function also accepts an explicit caller folder for direct invocation or tests.

The CLI reads the file as UTF-8 text with `readFile`, then calls `readReplay`. The text-byte limit is enforced after this read, not as an operating-system file-size check before allocating the text. On success it writes the result through `encodeSnapshot` followed by a newline and returns `0`.

On failure it writes nothing to stdout, writes a reason to stderr and returns `1`. Missing files receive a different reason from other read failures. Missing or extra arguments produce this usage line:

```text
Usage: npm run --silent replay -- PATH
```

An action-specific format problem is presented with a one-based action number. For example, the first action using index `16` on the four-column board is reported as action 1 naming a card outside the board. The API's `action: 0` and the CLI's “action 1” refer to the same entry.

## The output is a snapshot

A successful run does not print an annotated replay or an animation. It prints the final version 1 snapshot. Its matched positions are numerically sorted and its time fields use snapshot serialization. The input action array is not included, so saving only stdout loses the action history.

To inspect the result in the game, paste that stdout into Snapshot JSON and select Load snapshot. An unfinished result loads paused until Resume, because that is the browser's normal snapshot import behavior. The CLI does not encode a pause or ask the browser to auto-run the remaining interval.

The example at the beginning of this page outputs matched positions `[0, 11]`, closed selection, moves `1`, elapsed time `800` and `started: true` on `np1-4-1`. A replay with no actions, even with a very large valid endpoint, outputs the unstarted hidden deal with zero elapsed time. These two small cases are useful checks of both conversion and clock semantics.

## What the replay leaves out

A new deal begins another replay. The root has one board code and no action that changes size or seed. The format also has no initial snapshot field, so it cannot start halfway through a saved game. To reproduce later behavior, provide earlier actions that produce the desired state, or use a snapshot when only that state is needed.

Timeline values omit hidden-page time and imported-pause time. They are logical active intervals rather than wall-clock timestamps, and they do not identify a calendar date. Focus movement, draft edits, announcements and browser rendering frequency are not replay actions. Those omissions keep core evaluation independent of DOM and session effects.

There is no replay recorder or replay loader in the version 1 game UI. The core reader and CLI are the supported developer entry points. Adding a file picker or a play/pause viewer would be interface work beyond what the current format itself provides. For current sharing steps, see [Sharing FAQ](sharing-faq.md); for compatibility implications of changing ordering or time, see [Format versioning](format-versioning-guide.md).

## Test coverage

`replay.test.ts` covers first-activation time, exact timeout edges, invalid roots, versions, codes, actions, order, numeric bounds and inclusive byte and action limits. It checks a maximum-safe-integer endpoint both with and without a started game. `replay-order.test.ts` covers equal-time ordering directly.

`scripts/replay.test.ts` exercises child-process exit codes, snapshot stdout, bad arguments, unreadable paths and caller-relative path behavior. One of its comparisons loads `replay-messages.fixture.json`. The implementation's message mapping is in `replay-cli.ts`.

A regression report is strongest when it keeps the replay text as well as the resulting snapshot. The snapshot identifies the final disagreement; the replay lets an engineer locate the first divergent transition. Keeping both avoids confusing an input validation issue with a game-rule or output-serialization issue.
