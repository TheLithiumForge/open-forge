# Control bar

This page explains how `Controls.tsx` presents the deal actions and how those actions connect to the session.

The bar is deliberately small: a labeled size selector, New Game and Restart. It sits above the statistics and board in `src/game/Game.tsx`, so the actions that replace a deal are available before entering the board. Reproduction tools have a different rhythm and occupy their own disclosure below the board; see [Reproduction panel](reproduction-panel.md).

## Component boundary

`src/game/controls/Controls.tsx` accepts three props: `size`, `newGame(size?: number)` and `restart()`. It does not own a second copy of the selected size, choose a seed, reset a timer or update preferences. Its select is controlled by the `size` prop. That prop comes from `session.game.size`, which makes the displayed choice agree with the board after a size change, board-code load or snapshot import.

The optional argument on `newGame` explains the two call sites. The select converts `event.currentTarget.value` with `Number(...)` and calls `newGame` with that value. New Game calls `newGame()` without an argument. The default in `use-game.ts` takes the current game's size. Restart calls its separate callback. There is no DOM inspection to decide which size to use when either button is pressed.

The select and buttons are native elements. The two buttons explicitly use `type="button"`; they do not submit the reproduction form farther down the page. The wrapper is a `div` with class `controls`, not a toolbar widget with a separate keyboard model. Tab follows ordinary document order through the select, New Game and Restart. Board navigation begins after those controls, as described in [Board keyboard navigation](board-keyboard-navigation.md).

## Labels and options

`src/game/controls/control-labels.ts` exports `DealControl`. The visible strings are `Grid size`, `New Game` and `Restart`. The label is associated with `id="grid-size"` through `htmlFor="grid-size"`. A user or browser test can therefore find the select by its visible name instead of a CSS class or its position in the bar.

The options come directly from `GRID_SIZES` in `deck.ts`. Their values are the numeric side lengths rendered as native option values, and their text is the side repeated with a lowercase `x`:

| Display | Value | Cards |
| --- | --- | --- |
| 4x4 | 4 | 16 |
| 6x6 | 6 | 36 |
| 8x8 | 8 | 64 |
| 10x10 | 10 | 100 |
| 12x12 | 12 | 144 |
| 14x14 | 14 | 196 |
| 16x16 | 16 | 256 |

Those are deal sizes, rather than zoom settings. Choosing 16 changes the game to a 16-row, 16-column deck; it does not squeeze the existing sixteen cards into a different arrangement. The larger boards can exceed the available width inside their scroll container. [Pointer and touch](pointer-and-touch.md) explains the consequences for reaching cards.

The component itself receives `size: number`, but the session's `newGame` function checks `isValidSize(size)` before making a replacement. That guard is outside the control and also protects callers that do not originate in the native select. The option list is not the only validation boundary.

## Desktop arrangement

`controls.css` uses flex layout with `align-items: end`, `flex-wrap: wrap`, a `12px` gap and a `16px` bottom margin. The size group is another flex row with `align-items: center` and a `12px` gap. The label sits beside its select rather than above it. The outer alignment brings the actions to the same bottom line if control heights differ.

No explicit column count or button width is needed in this layout. Labels and the browser's select affordance contribute to their natural widths. Wrapping is enabled before the phone media query applies, so the bar can also adapt to a constrained content area on a wider window. The maximum content width and page padding come from `game.css`, not the controls stylesheet.

The shared `.control` class comes from `control.css`: `min-height: 44px`, `padding: 8px 12px`, a `1px` neutral border, an `8px` radius, panel background, text color and inherited font. The select and buttons get a pointer cursor. The stylesheet does not replace the select's browser interaction with a drawn menu. Its popup remains the native control's responsibility.

## Phone arrangement

At `max-width: 599px`, `.controls` changes to `align-items: stretch`. The size group occupies the full line with `width: 100%` and `justify-content: space-between`; its label and select move to opposite ends of that row. The direct child buttons each get `flex: 1`, sharing the following row when there is room.

The media query acts on the current DOM hierarchy. Moving a button into an extra wrapper would change the effect of `.controls > button`. A review of such a refactor can compare computed widths and wrapping before deciding that it preserves the arrangement. The apparent simplicity of a wrapper change can hide a layout change on phones.

The breakpoint is not a board-size breakpoint. A 4x4 and a 16x16 game use the same control bar at the same viewport width. The board handles its own width independently. This separation keeps the controls available even when a player has scrolled to the far end of a large board.

## What the actions replace

In `use-game.ts`, `newGame` creates a code using `freshBoardCode(size, current.current.game.seed)`, constructs the new game, resets the clock adapter and commits `replaceGame(...)`. Restart constructs a game from `current.current.game.boardCode` instead. Both begin with a closed selection, no matched positions, zero moves, zero elapsed time and `started: false` because that is what `createGame` returns.

`replaceGame` increments `session.generation`. The Board in `Game.tsx` is keyed by this generation, so it mounts with a fresh remembered tab stop. The BoardCodeForm input is also keyed by generation and receives the current board code as its default value. A reset is therefore observable in several places without each component inventing its own reset event.

The size is saved through the session preferences path. The current size's best record is read independently for the statistics. Choosing a size with no stored record shows `Best: none`; returning to a size with a record displays that size's value. For the exact display boundary, see [Status bar](status-bar.md) and [Best score records](best-score-records.md).

A pending mismatch cannot later alter the replacement deal. Clock reset and generation checks in the session make stale updates harmless. This is why replacing a deal during its short mismatch display is a meaningful controls test, rather than merely checking that the card count changes.

## Focus and continuity

Clicking New Game or Restart leaves focus on the action button. The components do not call `.focus()` on the board after a reset. A user can press the same action again or Tab forward to the new first card. A size change likewise uses the native select interaction rather than an application focus transfer.

Initial page load also does not select a card on the user's behalf. The board's first card is ready as its tab stop, but readiness is separate from current focus. [Focus policy](focus-policy.md) describes this distinction and the different treatment needed when Resume removes its own button.

There is no confirmation step in either callback today. Pressing an action replaces the current deal immediately. Do not infer a pending dialog from the text of an action or from the fact that the old game had moves. The separate [Dialog policy](dialog-policy.md) records the policy for future actions that leave unfinished work.

## A useful review path

The controls checks in `tests/controls.e2e.test.ts` inspect the native option values as well as their displayed text. `tests/control-contract.e2e.test.ts` checks the literal labels. `tests/keyboard.e2e.test.ts` checks the relationship between Restart and the remembered board entry point. Layout coverage includes a narrow `320x640` viewport in `tests/layout.e2e.test.ts` and the equivalent zoom layout in `tests/zoom.e2e.test.ts`.

These checks cover different failures. A label refactor could preserve a screenshot while breaking name-based selection. A wrapper refactor could preserve all labels while changing the mobile row. A session refactor could keep the bar visually identical while allowing an earlier timer to touch a new deal. Reading the component, CSS and callback together makes those differences visible.

For a manual check, choose a nondefault size, reveal a mismatching pair, then use Restart before it closes. The deck repeats and the counters reset. Use New Game at that same size and confirm that its code changes. Tab into the board after each action to observe the reset entry point. Finally load a valid code of another size from the reproduction panel and look back at Grid size: the controlled select follows the loaded game without another local state update.
