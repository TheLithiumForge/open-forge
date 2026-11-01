---
open-forge:
  description: "Pointer, touch, keyboard and focus behavior"
  tags: ["LoadNow", "Game", "Input"]
---
# Input
Input is a set of controls that lets you play Neon Pairs with a pointer, touch or keyboard.

Status: spec version 2.2.

You can reveal a card with a tap, then move to its partner with the arrow keys. Every activation uses the same game action, so changing input methods keeps the same turn.

## Activation

The card controls follow these rules:

- I01 - Render each card as a native button. A primary click, a completed tap, Enter and Space activate the card through the same action.
- I03 - Keep the browser's normal context menu on right-click or long-press. Opening that menu doesn't activate a card.
- I04 - Hover and pointer cancellation leave the game state unchanged.

The game decides whether an activation changes a card. See [Rules](rules.md) for matches, repeated selections, blocked turns and turn timing.

## Keyboard movement

- I05 - Give the board one remembered tab stop. When Tab enters the board, focus that card. Use the first card before any board focus has been remembered.

- I06 - Move focus with these keys. Movement changes focus without activating a card:

| Key | Focus destination |
| --- | --- |
| ArrowLeft | Previous card in the same row |
| ArrowRight | Next card in the same row |
| ArrowUp | Card in the previous row and same column |
| ArrowDown | Card in the next row and same column |

The board handles only these four arrow keys without modifiers, plus Enter and Space. It leaves every other key and modifier combination to the browser and doesn't use them to move focus or activate a card.

- I07 - Stop arrow movement at a board edge. Moving left from the first column keeps focus on the same card.

- I08 - Let native Tab and Shift+Tab leave the board. Moving through the board uses the movement keys instead of adding every card to the page's tab order.

## Focus

The board keeps focus separate from play:

- I09 - Keep matched cards focusable and mark them `aria-disabled="true"`. Their game action follows the matched-card rule in the gameplay document.
- I10 - While a mismatching pair waits to turn back, allow focus movement and block card activation.
- I11 - On initial load, keep the browser's current focus. Loading the game doesn't move focus to the board.
- I12 - After a new deal, Restart or a successful load, reset the board's remembered tab stop to its first card.
- I13 - When a card receives focus, scroll it into view with nearest alignment in the board's scroll area. Give each card a scroll margin of at least 6px, so its complete focus outline stays in view, including mid-board cards.

The session buttons keep their focus as defined in [Controls](controls.md).

## Card names

The button names describe the information available to the player:

- I14 - Name a hidden card by its one-based row, column and closed state. Keep its face SVG and face identifier out of the DOM while it is hidden, including attributes and accessible text.
- I15 - Name a revealed or matched card by its position, face name and state. Use the face names defined in [Visuals](visuals.md).

For example, the first hidden button is named "Row 1, column 1, closed". Once revealed, its name includes the face, so a player can remember it without seeing the drawing.

## Announcements

- I16 - Use one polite, atomic live region for turn results and completion. Announce each turn result exactly once. A winning turn has one announcement containing both the match and the win. Consecutive identical results each get their own announcement. Focus movement, clock renders and closing a mismatch don't repeat a result.

Use these turn announcements, where `M` is the matched pair count, `N` is moves and `time` uses R07's display format:

| Event | Announcement |
| --- | --- |
| First match | Pair found. 1 pair matched. |
| Later match | Pair found. M pairs matched. |
| Miss | No match. Cards close after a short pause. |
| Final match | Pair found. All pairs found in N moves and time. |

Send the load statuses in Controls through this same live region once per load event. Keep the visible status beside the reproduction controls.

For example, moving across three cards leaves the current announcement unchanged. Resolving the next pair announces that turn's result.

## Screen reader

- I17 - Support a complete 4x4 keyboard game with a screen reader. Find Grid size and New Game by name, enter the board, expose closed positions and revealed faces by name and state, announce one result per turn and make the win, final moves and time available. Leave the board with Tab and start another game. Deliver automated accessibility-tree evidence for this journey. Record the browser and tool versions in the project README and say whether a screen reader was used. If one was used, record its name and version. The user confirms spoken output during acceptance. Spoken confirmation isn't the developer's deliverable.

The complete game pack starts in [Neon Pairs](README.md).
