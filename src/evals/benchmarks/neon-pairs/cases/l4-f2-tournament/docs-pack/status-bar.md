# Status bar

This page explains the statistics and completion output rendered by `GameStatus.tsx`.

The status bar is a view of the current game and the current size's stored best move count. It does not run the clock, decide matches or write preferences. Keeping these jobs outside the component lets the same output describe ordinary play, a paused import and a completed import without separate counter logic.

## Inputs and placement

`src/game/status/GameStatus.tsx` receives `game: GameState`, `best: number | undefined` and `celebration: boolean`. In `Game.tsx`, the best value is selected with `session.preferences.bestMoves[String(game.size)]`. The component follows the deal controls and precedes board instructions. Its statistics remain in normal document flow above the board's internal scroll area.

The four labels come from `src/game/status/status-labels.ts`: `Moves:`, `Pairs:`, `Time:` and `Best:`. The punctuation belongs to each label. Values are separate definition descriptions, so the markup does not build a single visually positioned text string for the whole bar.

`GameStatus` returns a fragment containing a `dl` and, on completion, a paragraph. Each statistic is a `div` containing a `dt` and `dd`. This is a name/value summary of one game. It is not an editable form and does not add tab stops between the deal buttons and the board.

## Reading each value

| Label | Expression | Meaning |
| --- | --- | --- |
| Moves: | `game.moves` | Number of completed two-card attempts |
| Pairs: | `game.matched.length / 2` over `game.deck.length / 2` | Matched pairs and total pairs |
| Time: | `formatElapsed(game.elapsedMs)` | Active elapsed time, formatted for display |
| Best: | `best ?? 'none'` | Stored lowest move count for the current size, or no record |

A first reveal changes the card but does not increment Moves. The next eligible distinct reveal resolves the attempt and adds one move. A repeated first-card activation, matched-card activation or blocked mismatch activation does not add a move. This matters when reviewing a screenshot showing one open card and zero moves: it is a normal intermediate state, not a counter delay.

Matched positions are stored as card indices, with two positions per complete pair. Dividing by two produces the count a player cares about. The denominator is derived from the deck length, so the component does not maintain a second mapping from grid size to pair total. A four-column board begins at `Pairs: 0/8`; a sixteen-column board begins at `Pairs: 0/128`.

The Best value uses nullish fallback. An absent record displays `none`, keeping the label itself stable. A record is chosen by size in the parent, so loading another size can change Best even when the new game's Moves is zero. There is no total across all sizes and no face-specific score in this bar. [Scoring and moves](scoring-and-moves.md) gives the gameplay interpretation.

## Clock presentation

`formatElapsed` lives in `elapsed-time.ts`. It floors milliseconds to seconds, floors seconds divided by sixty to get minutes, and pads the seconds remainder to two digits. For example, 999ms displays `0:00`, 1000ms displays `0:01`, and 3600000ms displays `60:00`. Minutes continue to grow; the formatter does not switch to an hours field.

The function only formats a number. It does not advance the game or ask the browser what time it is. A clock render can therefore update the Time value without changing Moves, Pairs or the current turn output. The distinction is useful when debugging a report that the interface announces too often: the statistic and the turn feedback are different render paths.

Elapsed time starts with the first valid reveal, pauses while the document is hidden or an unfinished imported game awaits Resume, and stops at completion. The game caps it at 359999999ms, whose visible value is `5999:59`. Play can continue at that cap. The status bar has no special disabled state for a maximum clock value.

A freshly loaded unfinished snapshot may already have a nonzero elapsed value even though it is paused. That number describes the imported position. It remains still until Resume continues active time. Zero at the start and stillness while paused are both expected; neither is a signal to replace the display with a placeholder.

## Completion output

The component calls `isWon(game)`, which compares matched position count with deck length. When complete, it adds a paragraph with class `victory`. Its text is:

`Every pair found in {moves} moves and {formatted time}. Well played.`

The same values remain in the statistics. The paragraph gives a readable completed-game sentence without requiring a user to assemble the last Moves and Time values mentally. The sentence uses the game's actual counters, including the move cap if it was reached.

A true `celebration` prop adds the `celebrate` class. In `status.css`, that class applies `victory-pulse 600ms ease-out 1`. At the midpoint the background becomes the panel color and opacity is 0.75. After the single animation the banner remains. The reduced-motion media query sets animation to none.

Completion and celebration are separate inputs for a reason. A completed snapshot satisfies `isWon`, so its banner is visible, but importing it sets celebration false. The interface shows the completed result without presenting an imported position as a newly achieved win. [Motion policy](motion-policy.md) and [Win conditions](win-conditions.md) provide the wider context.

## Statistics and event feedback

The class name `stats` and the component name GameStatus can make it tempting to use this component for every status message. Today, turn feedback is rendered separately in `Game.tsx`, while load text is also visible beside the reproduction panel. Neither the `dl` nor the completion paragraph has an explicit live attribute in this component.

This separation lets a player inspect the persistent values while turn events follow their own publication path. The final matching action produces event feedback containing the match and win, and the persistent completion paragraph appears as part of the completed game. A load error does not replace Moves with an error or turn Best into a status string.

The component also does not attach an alternate accessible label to a `dd`. Its visible value remains the value available in the document. `tests/control-contract.e2e.test.ts` specifically checks that statistics descriptions have no `aria-label` attributes. [Screen reader notes](screen-reader-notes.md) explains the reading journey and [Announcements policy](announcements-policy.md) owns the publication policy.

## Wrapping and typography

`.stats` is a wrapping flex container. Its gap is `12px 24px`, and its bottom margin is `16px`. Each name/value group has a minimum width of `8ch`. A narrow viewport can wrap whole groups onto additional lines instead of compressing four columns into illegibility.

`dt` and `dd` are inline. The term uses secondary color and `14px` text. The description resets its margin and uses `font-variant-numeric: tabular-nums`. With a supporting system font, equal-width digits reduce visual jitter as Time or Moves changes. The group is still content sized; it is not a fixed-width four-cell scoreboard.

The banner has `12px 16px` padding, a `12px` bottom margin, text color, a neutral `1px` border and an `8px` radius. It is a normal paragraph and can wrap. Its appearance does not reserve a permanent blank banner before completion. Review of completion at narrow widths therefore includes the extra space it adds above the board.

The layout at `640x400` with device scale 2 is covered by `tests/zoom.e2e.test.ts`. That test checks readable controls, statistics and completion text without overlap and keeps the outer page within its viewport. The board may scroll inside its own region, but that is independent of wrapping the statistics.

## Common interpretation mistakes

A best count smaller than the current Moves is expected during a less efficient game. Best records completed play; it is not a target that stops the current attempt. The current move count can also stay still at 999999 while pairs continue to increase. That limit is enforced in `game.ts`, not by formatting the text.

Pairs is not a points label with a hidden multiplier. A match contributes one pair. There is no bonus for speed, no mismatch penalty beyond the move and no display of an accumulated score from previous deals. The page's visual language remains consistent when one size has many more pairs than another.

A loaded completed snapshot can show a result better than the stored Best without the stored value changing. Loading that result is not the same event as completing it through play. Conversely, a resumed unfinished import that later completes follows the play completion path. The view is faithfully showing two different sources: the loaded game's counters and preferences selected by the parent.

## Evidence and diagnosis

`tests/status.e2e.test.ts` finishes a board with controlled elapsed time, checks the completion text and confirms the clock stays stopped after more browser time passes. `tests/status-controls.e2e.test.ts` checks that the Best label uses the same wording before and after a record exists. The accessibility journey inspects the completed summary in the browser's accessibility tree.

When a displayed value is wrong, first locate its source. An incorrect pair total points toward matched positions or deck length. A wrong Best after a size load points toward the preferences selection or replacement path. A clock that advances while paused points toward the session clock boundary. A sentence with the right number but the wrong text points toward the component or its label constants. This keeps a display fix from hiding an upstream state problem.
