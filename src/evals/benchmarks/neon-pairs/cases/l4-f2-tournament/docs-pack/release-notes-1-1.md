# Neon Pairs 1.1: keyboard work

These release notes describe the keyboard-focused stage of Neon Pairs and explain how its navigation choices relate to the current implementation.

The central change in this release was making a board navigable as a board. A native button already gives Enter and Space a familiar activation behavior, but sixteen or more independent tab stops produce a long route through a page. The keyboard work made entering, moving within, and leaving the cards distinct actions. It also exposed assumptions that pointer-only demonstrations had left unexamined.

The keyboard behavior is implemented in `src/game/board` and defined by the input specification.

## Enter once, move spatially

The current board remembers a focused position and gives that card `tabIndex={0}`. Other cards have `tabIndex={-1}`. Before a position has been remembered, the first card is the entry point. Tab can therefore cross into the board once, after which plain arrow keys move between neighboring positions. Tab and Shift+Tab continue out through the page's normal order.

The release review contrasted this with two early approaches. Making every card tabbable preserved native behavior locally but obscured the page structure on larger boards. Capturing Tab to move through cards made the board feel self-contained but interfered with leaving it. The chosen behavior retained native page traversal and used spatial keys for spatial movement. The [roving-focus decision](adr-024-roving-focus.md) and [keyboard navigation guide](board-keyboard-navigation.md) describe the resulting model.

Movement follows rows and columns without wrapping. At the left edge, ArrowLeft remains at that row's first position. At the bottom edge, ArrowDown remains in the same column. `navigationTarget` in `keyboard.ts` computes these destinations without touching the game state. It returns `null` for keys outside its small navigation set and for modified key combinations. The browser therefore retains its own commands rather than having a game claim every key that sounds useful.

## Movement stopped being a guess

During the review, the team used a simple exercise: enter a four-column board, move right three times, then move down. The destination should be the rightmost card of the next row, and no card should have opened. This sounds trivial until focus and selection share one variable. An implementation can accidentally turn moving across cards into a series of reveals, or can lose the remembered entry position when a mismatch closes.

Today's separation is visible in the code. `useBoardFocus` holds the focused index. `GameState.selection` holds revealed unmatched positions. Moving focus updates the former. Activating a card reaches `activateCard` through the session controller and changes the latter only when the play rules permit it. A player can thus inspect the board's layout while a miss is waiting to close, with no accidental third reveal.

Matched cards also remain in the navigation geometry. They are marked with `aria-disabled` rather than removed from focus. If matched positions vanished from the route, “one row down” could cease to mean one row down partway through a game. Their action has no game effect, but their position is still useful information. The [disabled-state guide](disabled-state-guide.md) expands on this difference between an unavailable action and an absent location.

## Names followed available information

Keyboard support made the card's accessible name part of play rather than incidental metadata. A closed card announces its row and column and that it is closed. Once revealed, its name includes the face description and state. The current first closed button is named `Row 1, column 1, closed`. A visible face can be described by color, fill, shape, and marker direction.

An early alternative placed the face identity in a hidden label from the start, on the theory that only visible text needed to remain secret. That would have offered a different game to someone reading names instead of artwork. The surviving implementation avoids that split: a closed card receives `face={null}` and mounts the neutral back, while a visible card mounts `Face`. Hidden identity is absent from the button name and its face drawing.

This boundary matters for engineering diagnostics too. Tests can calculate the deck from a known board code, but the shipped interface does not publish a special answer endpoint. The [pair-identity page](pair-identity.md) addresses what equality means, and [card artwork](card-artwork.md) covers how a visible identity becomes a drawing.

## Focus after changing a deal

The keyboard review found that reset behavior needed to distinguish the board's remembered entry position from the element that currently owns focus. Starting a new deal resets the remembered card to the first position. It does not pull focus away from the New Game button. Restart and successful loads follow the same general pattern for their controls.

In the current composition, `Board` is keyed by `session.generation`. Replacement creates a new board focus state, while the controls remain mounted. This gives the next Tab entry a predictable starting position without making every control action jump into the cards. The later snapshot feature adds an explicit exception: Resume returns focus to the remembered card before its own button disappears. That behavior belongs to the [1.4 release](release-notes-1-4.md).

The distinction was initially easy to miss in screenshots. Two screens can look identical while one has keyboard focus lost on the document body. The review began treating the active element as part of the outcome of an action. The [focus incident](incident-focus-trap.md) records a failure caused by getting that boundary wrong.

## Feedback and restraint

Adding navigation did not mean announcing every movement as a turn result. Current result feedback changes when a pair attempt resolves, and the session tracks an announcement counter so two identical results can still represent separate events. Focus movement and ordinary timer renders do not create new turn results. This lets a player travel across several cards without the game repeatedly interrupting with the last miss.

The keyboard stage also strengthened visible focus. The current shared outline is white, 3px thick, with a 2px offset. The card's scroll margin leaves space around that outline. Hover changes the border; it does not substitute for keyboard focus. Pointer-to-keyboard switching is meaningful because someone may reveal a card by click and then use an arrow key to find another position. Current focus-entry tests exercise that transition.

## Reading the evidence now

`src/game/board/keyboard.test.ts` checks destination calculations independently of React. Browser journeys in `tests/keyboard.e2e.test.ts`, `tests/focus-entry.e2e.test.ts`, and `tests/accessibility.e2e.test.ts` cover the interaction with real buttons and rendered cards.

The README records accessibility-tree checks with Chromium and Playwright, while explicitly saying no screen reader was run. Spoken-output confirmation remains a separate acceptance activity. The difference is examined further in [screen-reader session notes](research-screen-reader-sessions.md).

The release's durable contribution was a route through the game: find a control, enter the board, move without activating, reveal with a native action, hear or read the result, leave, and start another attempt. The next stage, [large boards](release-notes-1-2.md), put that route under far more demanding layout conditions.
