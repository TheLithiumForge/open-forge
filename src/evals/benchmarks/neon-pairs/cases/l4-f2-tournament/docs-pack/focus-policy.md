# Focus policy

This page records focus behavior in the current game and the policy for a feature that changes the main screen.

Focus tells a keyboard user where input will go next. It is related to game selection but is not stored as part of a turn or snapshot. The current interface deliberately keeps deal-action focus, remembered board focus and card reveal state as distinct things.

## Existing page and future screens

`Game.tsx` currently renders one main area with an h1, deal controls, statistics, instructions, turn output, the Board and reproduction controls. It has no alternate-screen branch or screen-level h2 focus target. Replacing a deal remounts the board within that page; it does not replace the page with a different screen.

A feature that introduces a screen replacement has a different orientation problem from resetting a deck. When a feature replaces what the main area shows, focus moves to that screen's h2, which has tabindex="-1". This gives the new screen a programmatic landing place without adding that heading to ordinary Tab traversal. The current deal and load callbacks can still be understood on their own terms; they are not evidence that such a screen transition already exists.

The policy applies to the feature-level replacement described here. Expanding native details, updating statistics, closing a mismatch or displaying the completed-game paragraph does not currently create a new screen. Those operations retain the page structure and follow the existing behavior below. [Dialog policy](dialog-policy.md) covers a separate interaction with its own opening focus.

## Initial load is quiet

The hook `useBoardFocus(size)` initializes `focused` to zero. Board gives index 0 a tab index of zero and the other card buttons a tab index of minus one. This makes the first card available on entry, but no mount effect calls its focus method.

The browser's current focus therefore remains where it is at initial load. The page does not jump down to the board before a player has found Grid size or New Game. The board help paragraph describes how to enter and operate it. Its region uses `aria-describedby="board-help"` to associate those instructions with the board.

A remembered entry point is often described casually as “focus on the first card.” In this code that wording is incomplete. There is an entry point ready at the first card, while actual focus may be on the document, a deal button or another native control. Keeping those concepts separate prevents accidental mount-time focus stealing.

## Deal actions retain their location

New Game and Restart are native buttons that call the session controller. Neither callback moves focus into the board. After either action, a keyboard user remains on the button they used and can repeat the action or Tab toward the board. The size select uses its native interaction and likewise does not issue a board focus call.

The replacement increments `session.generation`, used as Board's React key. The newly mounted hook begins with a remembered index of zero. Thus the next board entry goes to the first card, even if the previous deal's memory was near its end. Actual action focus and future board entry are updated through different mechanisms.

A valid board-code load also increments generation. BoardCodeForm focuses its Load board ref on successful submission because its input is keyed by generation and will be replaced. That is especially important when the action was Enter in the input. An invalid submission keeps the input node and does not call the button ref, so correction can continue in place.

Clicking the load button starts with focus on that button, and an invalid click leaves it there. The distinction is based on the action's origin and successful input replacement, not on a general rule that all errors move focus into a field. [Error display guide](error-display-guide.md) follows those cases in detail.

## Remembered board focus

Board focus memory is a local hook state containing a card index. The `onFocus` callback updates it whenever a card receives focus. Board renders exactly that index with `tabIndex={0}`. All other cards remain programmatically focusable with `tabIndex={-1}`.

Arrow movement uses the DOM order of the deck and never changes game selection on its own. Tab and Shift+Tab are left to the browser, so the board is one stop in the page sequence rather than a trap. Returning to it finds the last focused card, even if that card is now matched.

Matched positions stay in the navigation geometry. A player does not need to relearn how many arrow presses separate two coordinates after a match. During a mismatch, focus can also move while activation is blocked. The same is true while imported unfinished progress waits for Resume. [Board keyboard navigation](board-keyboard-navigation.md) provides the calculations and edge behavior.

## Resume transfers before removal

Resume is the current exception to retaining focus on the action button. It exists only while `session.paused` is true. Clearing that flag removes the button, so leaving focus there would leave a disappearing target.

In `Game.tsx`, the local `resume()` function first calls `board.current?.focusRemembered()` and then `controller.resume()`. Board's imperative method focuses the remembered card with `preventScroll: true` and then brings it into view. Only after that does the controller clear the paused state and notices.

The ordering is observable. `tests/control-contract.e2e.test.ts` installs a focus listener and confirms that Resume is still present when the card receives focus. A visual assertion after the fact would not establish this timing. The focus-entry test also remembers a far card on a large imported board, scrolls away and confirms that Resume returns to that location with nearest alignment.

The remembered location after import is initially the first card because the import replaced the board. The player can change it while paused. Resume uses that current memory rather than the snapshot's revealed index or a hard-coded first card. A snapshot stores game progress, not the browser's last navigation location.

## Visible focus and scroll clearance

`game.css` defines `:focus-visible` as a `3px` white outline with a `2px` offset. `card.css` gives cards `scroll-margin: 6px`. This combination accommodates the ring when the card is brought to the edge of the scroll area.

The focus hook uses `card.matches(':focus-visible')` before scrolling in `cardFocused`. Keyboard movement can scroll immediately; pointer focus avoids moving a card under a held press. Board scrolls a clicked card after the click completes. The timing is part of reliable focus behavior, not only a visual polish detail.

`useNearestScroll` stores the board's last reported scroll position, restores it and then invokes `scrollIntoView` with nearest alignment. The source records the Chromium-specific reason: Tab or Shift+Tab can center a newly focused card before the handler runs. Starting from the prior view preserves the player's location as much as possible.

The helper leaves the outer page's scrolling to the browser. A review therefore measures both the card inside the board viewport and its visibility inside the window. A ring can be uncut by the board yet still sit partly below the window if the outer document did not move appropriately.

## Focus is not completion feedback

Winning does not move focus to the completion paragraph in GameStatus. The final card stays the current control, and the completed summary becomes available in document order. The final turn's feedback is published through the game's event output. The component does not force the player to leave the board simply because all pairs are now matched.

Likewise, a mismatch closing does not restore focus to either of its cards. Focus may have moved elsewhere while the pair was visible. The game-state update closes the faces while the user's navigation choice survives. An ordinary clock tick also leaves focus untouched.

These examples are useful when evaluating a proposed effect that focuses something whenever `game` changes. Game changes for elapsed time, first reveals, turn results and resets. An effect tied to that broad object would combine very different events and could interrupt typing or navigation. The existing code places its explicit focus operations at narrower action boundaries.

## Review evidence

The keyboard browser check covers Tab exit, remembered re-entry and matched-card focus. `tests/status-controls.e2e.test.ts` covers Enter submission from Board code. `tests/focus-entry.e2e.test.ts` covers nearest alignment and the switch from pointer use to keyboard arrows. `tests/layout.e2e.test.ts` checks outline clearance on large boards, including an interior card where container padding alone cannot protect the ring.

A useful focus report names the source control, action, destination and whether the source remains mounted. For example: “Enter in Board code loads 6x6, replaces the input and leaves Load board focused.” That is more precise than “load focuses correctly,” and it exposes why failure has a different result. [Interface review checklist](interface-review-checklist.md) gathers these journeys with layout and input checks.

For new screen work, the source review begins by locating the actual replacement boundary. For changes within the existing game page, it begins with the current action-specific behavior above. Screen replacement and the established board lifecycle have separate focus behavior.
