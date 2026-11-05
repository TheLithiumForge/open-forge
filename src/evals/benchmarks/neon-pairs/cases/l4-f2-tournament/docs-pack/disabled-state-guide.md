# Disabled state guide

This page explains why some card activations have no effect while the controls remain focusable and usable for navigation.

The game separates control semantics, focus and game eligibility. A matched card exposes an unavailable state, but it stays in the board. A mismatching pair temporarily blocks further activation, yet arrows can still explore the same coordinates. These are deliberate differences rather than one universal disabled flag.

## Matched cards use ARIA state

`Card.tsx` sets `aria-disabled={matched}` on its native button. It does not set the HTML `disabled` attribute. When matched is true, the button remains available to programmatic focus and the board's remembered tab-stop mechanism.

The card's accessible name includes `matched`, along with its coordinate and face description. Its face remains visible, its border becomes the green palette color and a corner check mark appears. Matched cards retain full opacity. The state is communicated through text, semantics and visual treatment rather than by dimming the card until it is difficult to identify.

The click handler still calls `activate(index)` for a matched card. ARIA describes the state; it does not suppress native events. The actual game guard in `activateCard` checks `state.matched.includes(index)` and returns the same state. The session sees that identity and returns without publishing a new result.

This is an important code-reading boundary. Removing the guard because a button “is already disabled” would mistake an accessibility attribute for an event blocker. Conversely, adding native disabled would change focus behavior even if ignored activation still produced no move.

## Why focus remains available

The board uses one remembered card in the page's tab sequence. Arrow movement traverses coordinates without skipping solved positions. A match can therefore leave focus on its second card, and the next arrow still means the adjacent row or column.

If matched buttons became natively disabled, they would no longer participate in the same focus behavior. Remembered entry could point to an unavailable native focus target, and movement would need a different policy for skipping positions. The current input specification instead keeps the rectangular geometry stable.

A player can leave with Tab and return to the remembered matched card. Activating it again has no game effect. This is covered in `tests/keyboard.e2e.test.ts`, which checks the disabled attribute, continued focus and unchanged move count after another Space activation.

[Board keyboard navigation](board-keyboard-navigation.md) explains the tab-index pattern and edge clamping. The disabled state does not create a second movement algorithm or change the size of the board.

## A revealed first card is different

The first revealed card has state `revealed` and is not matched. It therefore exposes `aria-disabled="false"`. Activating it again is nevertheless ignored by the game's `SelectionKind.one` branch because `first === index`.

This is not inconsistent with the matched-card treatment. The first reveal is an intermediate turn state. The game waits for a different eligible position to complete the attempt, while the first card remains readable and focusable. The button name describes what the player can know, and the core determines whether this particular activation advances the turn.

There is no separate “selected but disabled” CSS class or native disabling operation for that card. A repeated click may keep focus there and apply completed-click scrolling, but it does not add a move, restart elapsed time or produce a turn result.

The same separation lets a player navigate away from the first card and return without changing selection. Focus is not stored in the `selection` union. A review that derives reveal state from the currently focused element would collapse two independent parts of the interface.

## Mismatch blocking is in the game

After two different faces are revealed, selection becomes `mismatch` with a remaining active interval. The core's mismatch branch returns unchanged state for every attempted activation until advancement closes the pair. The unmatched card buttons do not all acquire ARIA disabled state during this interval.

Movement and focus remain available. A player can inspect coordinates or prepare a next choice while the pair stays visible. Closing the mismatch does not force focus back to either revealed position. The game-state change removes their faces, while navigation memory remains where the player moved it.

The controller synchronizes active time before processing a card action. An activation arriving when the mismatch has already expired can therefore be processed after closure rather than being rejected against stale rendered state. The guard operates on current synchronized game state, not a CSS class observed a moment earlier.

The interval pauses while the document is hidden. That is a clock lifecycle behavior, not a mass change to card attributes. [Mismatch interval](mismatch-interval.md) and [Clock design](clock-design.md) describe the timing model.

## Imported pause is in the session

Loading unfinished progress sets `session.paused`. `activateSession` returns immediately while that flag is true, before calling the core activation function. `tickSession` also rejects advancement while paused.

The Board does not receive paused as a prop. It continues rendering the loaded game's visible faces, matched states and navigation callbacks. Unmatched buttons remain semantically unmatched rather than becoming a new paused-card class. The load notice and Resume explain the paused session state outside the board.

A player can move focus while paused. Resume returns to the current remembered card before clearing the pause and removing its button. A snapshot with a pending mismatch then continues its remaining interval. A completed import is not paused and has no Resume button; all of its cards are already matched.

The automated accessibility journey explicitly checks that a paused unfinished import does not mark unmatched cards disabled. This is stronger than assuming paused and disabled are interchangeable words. [Reproduction panel](reproduction-panel.md) describes the visible continuation controls.

## Deal and text controls stay usable

New Game and Restart are available during ordinary play, mismatch waiting and imported pause. Their controller operations replace the game and clear the old session context. There is no busy-state branch disabling them while a timer is active.

The reproduction actions also remain available. An invalid draft is submitted for validation rather than causing its load button to become disabled before the user can request a result. Export reads current progress without replacing the game. A valid load can replace a paused or active position through the same validated boundary.

The current forms do not use `required`, a disabled submit button or `aria-invalid` as their validation mechanism. Their native length limits constrain editing, and the controller reports invalid content on action. [Error display guide](error-display-guide.md) traces those outcomes and the preservation of drafts.

This describes the existing controls, not a new general rule that every possible action in a future feature is always enabled. The relevant question in review is which layer owns the reason an action cannot change state, and whether the element's semantics and focus behavior match that reason.

## Rendering, counters and no-op actions

Core activation returns the original object for invalid indices, matched positions, repeated first positions and mismatch blocking. The session preserves identity when the core does. That lets the surrounding controller avoid committing a new state for an ignored action.

Synchronization can still advance elapsed time before the ignored activation is considered. Seeing Time change around a no-op click does not mean the click added a move. The move count changes only when a second eligible card resolves an attempt, subject to its cap.

At completion, every index is matched, so further card activation follows the matched guard. The final clock stays stopped because advancement checks `isWon`. There is no extra completed-overlay element that captures clicks above the board.

These details help distinguish a correct ignored action from a lost input. A click on a hidden eligible card in a closed active game should reveal it. A click on a matched card should not change the turn. Both can deliver the same native event; their game-state eligibility differs.

## Review evidence

The useful checks pair an input with both its state effect and its focus effect. The keyboard test checks matched inactivity while focus remains. The input-contract test checks hidden controls' exact ARIA attributes. The accessibility journey checks matched disabled exposure and paused unmatched exposure. Session unit tests check paused activation and advancement independently of the DOM.

A defect report can name the card coordinate, its visible state, session pause status, selection kind and expected counter change. That information reveals whether the problem belongs to native focus, ARIA exposure, controller timing or core eligibility. Treating every ignored input as the same disabled-control problem would hide those distinctions.
