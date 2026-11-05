# ADR 042: Use short errors that preserve the draft

This record explains the current reproduction errors and why failed validation leaves the game available.

## Context

A board-code or snapshot load is an explicit request to replace a game. Invalid input should explain that the request failed without pretending that the current board was replaced. The player also needs the original draft available to correct. An error that clears the field or resets the game would make validation itself destructive.

The decision is short, control-specific feedback. The board loader reports “Invalid board code.” The snapshot loader reports “Invalid snapshot.” The visible notice stays beside the reproduction controls. The current session's game remains intact after a failed parse.

## Validation before replacement

`useGame.loadBoard` trims the submitted code, checks it and returns `false` after publishing the invalid-code notice if parsing fails. Only a valid code reaches clock reset and game replacement. `loadSnapshot` follows the same order with `decodeSnapshot`. This order is the behavioral foundation underneath the copy.

The controller's `notice` function updates feedback and the visible load notice and increments the announcement sequence. It does not replace the deck or change the selected size. A failed load therefore remains an error on the submitted operation, rather than a partially applied session transition. [Error display guide](error-display-guide.md) covers placement.

## Why the browser errors are broad

Snapshot validation checks many relationships: shape, version, code, integer limits, matched pairs and selection consistency. Exposing every parser branch as a separate browser sentence would produce a much larger message surface than the current form needs. The submitted JSON remains available for inspection, and engineering tools can use focused tests and formats to investigate specific cases.

The replay CLI is different. It is a developer command processing a file, so its problem map reports a more specific reason and can name a one-based action position. That is a different audience and output channel, not an inconsistency in how the browser button behaves. [ADR 015](adr-015-replay-cli.md) explains that command contract.

## Focus and correction

The code form's input is keyed by session generation. Invalid input does not change generation, so the edited draft remains. A successful load remounts the input with the current canonical code and focuses the load button. The snapshot draft is local component state and is likewise retained for correction on a failed load.

Action buttons describe their notice with `aria-describedby`. The notice is persistent visible text rather than a disappearing browser alert. The text does not ask the player to reconstruct a lost draft or infer which game is active. [Focus policy](focus-policy.md) and [ADR 006](adr-006-native-controls.md) explain the surrounding behavior.

## Alternatives and consequences

A modal error would interrupt correction with an extra dismissal step. An automatic fallback to a fresh deal would hide failure behind an apparently successful action. Silently ignoring the request would preserve state but provide no explanation. The chosen message makes failure visible while keeping correction local.

The tradeoff is limited diagnostic detail in the UI. A malformed JSON document and a coherent-looking but inconsistent snapshot receive the same browser message.

Browser tests check that invalid drafts leave the current state and selected size intact and that repeated invalid submissions remain separate feedback events. Parser tests cover the underlying malformed inputs. [Error message style](error-message-style.md), [Controls](controls.md) and [Import safety policy](import-safety-policy.md) provide the complementary references.

## The guarded load operations

Both loaders validate first. Clock reset and replacement occur only after a complete valid result is available.

```ts
function loadBoard(code: string): boolean {
    code = code.trim();
    if (!parseBoardCode(code)) {
      notice(SessionNotice.invalidBoard);
      return false;
    }
    clock.reset();
    commit(replaceGame(current.current, createGame(code), false));
    return true;
  }

  function loadSnapshot(text: string): boolean {
    const game = decodeSnapshot(text);
    if (!game) {
      notice(SessionNotice.invalidSnapshot);
      return false;
    }
    clock.reset();
    commit(replaceGame(current.current, game, true));
    return true;
  }
```
