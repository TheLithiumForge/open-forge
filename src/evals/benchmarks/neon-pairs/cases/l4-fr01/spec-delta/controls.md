# Controls
Controls is a specification that defines the game options and reproduction tools in Neon Pairs.

Status: spec version 2.

You can start another deal, repeat the current one or load saved progress. Each control keeps the same label, so you can find the same action after a game changes.

## Deal controls

The main controls are:

- C01 - Label the native size select "Grid size". Give it options `4x4`, `6x6`, `8x8`, `10x10`, `12x12`, `14x14` and `16x16`, with numeric side length `N` as each value.
- C02 - Label the fresh-deal button "New Game". It creates a fresh seed and keeps the current size.
- C03 - Label the repeat-deal button "Restart". It starts the same board code again.

Changing size, starting and restarting follow the session rules in [Rules](rules.md).

## Reproduction tools

- C04 - Put reproduction controls inside a native `details` element that starts collapsed. Give its summary the exact text "Reproduce a game".

The controls use the board-code and snapshot formats defined in [Reproducibility](reproducibility.md). A board code starts a deal. A snapshot loads progress on a deal.

## Load a board

- C05 - Label the text input "Board code" and limit it to 32 characters. Show the current board code after a new deal, Restart or a successful load. Keep an edited draft when a card action changes the game.

- C06 - Label its button "Load board". Validate the entered code before replacing the current game with that deal's starting state.

For example, loading `np1-4-1` starts its hidden four-column board. Entering that code alone keeps the current game until you select Load board.

## Snapshot tools

- C07 - Label the textarea "Snapshot JSON" and limit it to 65536 characters. The parser also applies the UTF-8 byte limit in the snapshot format.

- C08 - Label the export button "Export snapshot". Fill the textarea with readable snapshot JSON that you can select and copy manually. Use version 2 for a named game and version 1 for an unnamed game. Export leaves the game state and clock unchanged. It requires no Clipboard API.

- C09 - Label the import button "Load snapshot". Validate the complete draft before replacing the current game. Load valid progress paused and show "Loaded snapshot. Select Resume to continue."

- C10 - Show a "Resume" button only while an imported game is paused. Selecting it clears that pause. Unfinished play continues from the loaded state. While paused, block every card activation and allow board focus movement. On Resume, move focus to the board's remembered tab stop before removing the Resume button. A loaded completed game stays completed after Resume.

- C11 - On invalid input, keep the current game and the entered draft for correction. Show the exact status for that control:

| Control | Status |
| --- | --- |
| Load board | Invalid board code. |
| Load snapshot | Invalid snapshot. |

- C12 - A loaded completed snapshot keeps the win banner static and plays no celebration. Import doesn't write a best score. Resuming an unfinished snapshot and later winning can record a local best.

For example, loading a completed game with fewer moves than the saved best shows that game without replacing the saved record.

## Focus

- C13 - Keep focus on the button used for starting, restarting, loading or exporting. These actions don't move focus to the board. Resume follows C10 because its button disappears. The board's remembered tab stop follows the focus rules in [Input](input.md#focus).

## Deal names

- C15 - Add a native text input labelled "Deal name" inside the reproduction controls. Start empty. Limit a name to 40 UTF-16 code units and reject carriage returns and line feeds. Keep all other characters and surrounding spaces unchanged. Editing a name leaves the board code, cards, moves, clock and pause unchanged. Allow editing while paused or completed. A named import restores its name with the existing paused behavior. A version 1 import clears the name. New Game, Restart, size changes and board-code loads clear the name. Names aren't saved in browser preferences. Reload follows R08 and starts an unnamed fresh deal. Importing completed named progress keeps C12's static banner and best-score rules.

For example, a game named "Café" exports a version 2 snapshot. After importing it, Deal name reads "Café" and Resume continues its saved progress.

## Replay

- C14 - Keep replay in the developer CLI and game core. V1 has no replay controls in the game UI.

Run `npm run replay -- PATH` to read a replay file and print its final snapshot.

The complete game pack starts in [Neon Pairs](README.md).
