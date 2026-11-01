# Input
Input is a specification that defines requirements for Neon Pairs.

## Activation

- I01.1 - Each card is a native button.
- I01.2 - Primary click, completed tap, Enter and Space use the same activation action.

- I03.1 - Keep the normal context menu for right-click and long-press.
- I03.2 - Opening that menu doesn't activate a card.

- I04.1 - Hover leaves game state unchanged.
- I04.2 - Pointer cancellation leaves game state unchanged.

## Keyboard movement

- I05.1 - Give the board one remembered tab stop.
- I05.2 - Tab enters at that card.
- I05.3 - Use the first card before any board focus is remembered.

- I06.1 - ArrowLeft moves to the previous card in the same row.
- I06.2 - ArrowRight moves to the next card in the same row.
- I06.3 - ArrowUp moves to the previous row in the same column.
- I06.4 - ArrowDown moves to the next row in the same column.
- I06.9 - Movement changes focus without activating a card.
- I06.10 - Handle only ArrowLeft, ArrowRight, ArrowUp and ArrowDown without modifiers, plus Enter and Space. Leave every other key and modifier combination to the browser, without using it to move focus or activate a card.

- I07.1 - Arrow movement stops at board edges without wrapping.

- I08.1 - Native Tab and Shift+Tab leave the board.
- I08.2 - Movement keys navigate without putting every card in the page tab order.

## Focus

- I09.1 - Matched cards stay focusable.
- I09.2 - Matched cards have aria-disabled=true.
- I09.3 - Matched-card activation follows [R02](rules.md#turn).

- I10.1 - Allow focus movement during a mismatch interval.
- I10.2 - Block card activation during that interval.

- I11.1 - Initial load keeps the browser's current focus.

- I12.1 - A new deal or Restart resets the remembered tab stop to the first card.

- I13.1 - Scroll each focused card into view with nearest alignment inside the board.
- I13.2 - Give every card a scroll margin of at least 6px so its complete focus outline stays in view, including mid-board cards.

## Card names

- I14.1 - Name hidden cards by one-based row, column and closed state. For example, Row 1, column 1, closed.
- I14.2 - Hidden cards keep face SVG out of the DOM.
- I14.3 - Hidden cards keep face identifiers out of attributes and accessible text.

- I15.1 - Revealed and matched names include position, face name and state.
- I15.2 - Use [V13](visuals.md#card-faces)'s face names.

## Announcements

- I16.1 - Use one polite atomic live region.
- I16.2 - Announce each turn result exactly once.
- I16.3 - Announce a final match and win together once.
- I16.4 - Consecutive identical results each have their own announcement.
- I16.5 - Focus movement, clock renders and mismatch closing don't repeat a result.
- I16.6 - Announce the first match as "Pair found. 1 pair matched." Announce later matches as "Pair found. M pairs matched.", where M is matched pairs.
- I16.7 - Announce a miss as No match. Cards close after a short pause.
- I16.8 - Announce the final match as "Pair found. All pairs found in N moves and time.", where N is moves and time uses the [R07](rules.md#clock) display format.

## Screen reader

- I17.1 - Support a complete 4x4 keyboard game with a screen reader. Deliver automated accessibility-tree evidence for this journey.
- I17.2 - Find Grid size and New Game by name.
- I17.3 - Expose closed positions, revealed face names and states and one live-region result per turn in the accessibility tree.
- I17.4 - Make the win, final moves and time available.
- I17.5 - Tab out of the board and start another game.
- I17.6 - Record the browser and tool versions in the project README and whether a screen reader was used. If used, record its name and version. The user confirms spoken output during acceptance. Spoken confirmation is not the developer deliverable.

