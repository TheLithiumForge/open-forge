# Error message style

This page describes the error wording Neon Pairs currently exposes and the different jobs of browser notices, replay diagnostics and internal exceptions.

The browser has two deliberately short invalid-load notices. The replay CLI gives more specific diagnostics because it works with a file and a structured action list. Neither surface needs to guess who caused the problem. This guide records those existing choices; validation details remain in [Import safety policy](import-safety-policy.md) and the individual format pages.

## The browser notices are exact strings

`SessionNotice` in `src/game/session/session.ts` contains the browser load messages. The invalid notices are “Invalid board code.” and “Invalid snapshot.” `use-game.ts` chooses the appropriate one when parsing fails. It returns before resetting the clock or replacing the game.

| Attempt | Visible error | What was rejected |
| --- | --- | --- |
| Load board with an unacceptable code | `Invalid board code.` | The board-code draft. |
| Load snapshot with unacceptable progress | `Invalid snapshot.` | The snapshot draft. |

The noun is useful. A player can distinguish the board field from the larger JSON field without interpreting a generic “Something went wrong”. The wording also avoids an unsupported diagnosis: “invalid” does not assert that the text is corrupted, that the device failed, or that a particular character is responsible.

Both notices end in a period and stand as complete status text. They do not name a function, a parser exception or a schema utility. The code has more knowledge than the short UI exposes, but it does not currently present field-by-field snapshot diagnostics. An example that shows such a diagnostic would describe a different interface.

The related success notices help explain the same design. Unfinished progress says “Loaded snapshot. Select Resume to continue.” Completed progress says “Loaded completed snapshot.” One includes an action because the resulting state needs it. The other reports the state without that instruction. A writing change that makes every load outcome use the same generic sentence would remove useful information.

## Failure preserves the correction context

The browser keeps the entered draft and the current game when a board or snapshot load is invalid. A useful help paragraph can therefore say to correct the text in its field and use the same load action again. There is no need to describe starting over from an empty form, since the current components do not clear failed input.

The board-code path trims surrounding whitespace before calling `parseBoardCode`. The snapshot path validates the whole textarea content. These facts matter when diagnosing a report, but they are not separate error messages. For example, a board code with an unsupported side length still produces the same board-code notice as a code with an invalid prefix.

The board input is an uncontrolled field keyed to session generation. A successful game replacement refreshes it from the active board code. A failed load does not replace the generation, so the draft remains. `Reproduction.tsx` stores snapshot text in local component state. Those are two implementation routes to preserving useful correction context; neither turns the draft into accepted game state.

The notices appear beside the reproduction controls and are also passed through session feedback. A repeated invalid submission is a separate event even if its sentence is identical. Copy review can inspect the text here, while [Announcements policy](announcements-policy.md) and [Error display guide](error-display-guide.md) describe its presentation and delivery.

## The replay command has a diagnostic audience

Replay is a developer command rather than a browser control. Run it as `npm run --silent replay -- PATH`. On success it prints the final snapshot JSON to stdout and exits with status 0. On failure it leaves stdout empty, writes a diagnostic to stderr and exits with status 1. This separation allows another program to consume successful output as JSON.

`scripts/replay-cli.ts` builds most failure messages through `fail(file, reason)`. The shape is `Could not replay FILE: reason.` followed by a newline. `FILE` is the argument supplied by the caller. It is not replaced in the message with the internally resolved absolute path. Relative file lookup uses the caller folder, accounting for npm's `INIT_CWD`.

For a file named `broken.json` containing `{`, the diagnostic is:

```text
Could not replay broken.json: the file is not valid JSON.
```

For an absent file named `missing.json`, the message is:

```text
Could not replay missing.json: the file does not exist.
```

A different read failure uses “the file can't be read”. That wording reflects the actual distinction in `isMissing`: `ENOENT` gets the missing-file reason, and other caught read failures get the broader reason. It does not speculate about permissions, locking or a device problem when the adapter has not established that cause.

## Diagnostic specificity follows the parser result

`readReplay` returns either a state or a `problem`, with an optional action position. `ReplayProblem` gives those cases names such as `malformedJson`, `boardCode`, `order` and `index`. The CLI's `PROBLEMS` mapping turns them into explanations. That mapping keeps the core independent of presentation wording.

| Problem kind | Current reason text |
| --- | --- |
| `tooLarge` | `the file is larger than 1048576 bytes` |
| `malformedJson` | `the file is not valid JSON` |
| `format` | `it isn't a version 1 replay with exactly version, boardCode, actions and endMs` |
| `boardCode` | `its board code is invalid` |
| `actions` | `actions must be a list of at most 10000` |
| `endTime` | `endMs must be a nonnegative safe integer` |
| `action` | `needs exactly atMs and index as nonnegative safe integers` |
| `order` | `comes before the previous action or after endMs` |
| `index` | `names a card outside the board` |

For an action-specific problem the CLI inserts a human action ordinal before the reason. The parser's position is zero-based, but the diagnostic adds one. An invalid card index in the second action of `rule.json` therefore reads:

```text
Could not replay rule.json: action 2 names a card outside the board.
```

That number identifies the action's list position. It is not the offending card index. The distinction helps someone find the problem in a longer replay. It also explains why the diagnostic can be useful without quoting the entire action object.

When arguments are absent or there is more than one argument, the command prints its usage line instead of the file-and-reason form:

```text
Usage: npm run --silent replay -- PATH
```

There is no file-specific cause to report in that situation because the invocation itself is unacceptable. The message names the accepted command shape, making the next attempt concrete. [Replay format](replay-format.md) covers the file contents after that invocation succeeds in reading a file.

## Internal exceptions belong to another layer

Not every string containing “Invalid” is an interface notice. `boardCode` and `generateBoard` throw `Error('Invalid board code.')` when called with an unacceptable code. `faceDesign` throws `Error('Invalid face.')` for an invalid face ID. `generateBoard` also guards against unavailable deck entries with “Deck index out of range.”

Those exceptions describe invalid internal calls or an invariant failure. They are not additional browser validation states. The normal browser load path parses the code before creating a game, and snapshot decoding returns `null` for rejected external state. Copying an internal exception into a player help page as though it were a visible notice would confuse these boundaries.

Likewise, storage failures are caught by the storage adapter and leave play available. The current browser does not turn a blocked preference write into an error dialog or an inline save warning. The fallback keeps the game available when storage fails. [Storage failure policy](storage-failure-policy.md) covers the underlying behavior.
