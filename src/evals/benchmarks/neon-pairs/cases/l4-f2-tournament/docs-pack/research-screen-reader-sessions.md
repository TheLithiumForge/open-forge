# Screen-reader session notes

These are research notes about the information a screen-reader player needs to complete Neon Pairs, accompanied by the actual limits of the project's automated accessibility evidence.

The README records Chromium accessibility-tree checks with Playwright and leaves spoken-output confirmation to user acceptance.

## Why the study needed the whole game

A card button can have a correct name while the surrounding journey remains confusing. The research question was therefore broader than whether a particular control exposed a label. Could someone locate the size and starting controls, enter the board, distinguish closed from revealed information, understand a turn result, find the final result, and leave to begin again?

The participants used keyboard navigation and their own familiar screen-reader habits. The moderator did not ask them to emulate a sighted player's visual scan. The study explored information boundaries that a visual demonstration can overlook.

The smallest board was the useful starting point because a full game could be followed without making long-term memory the dominant research variable. A complete four-column journey also makes it practical to inspect every transition from hidden positions to revealed identities and finally to matched states.

## A closed card had to remain unknown

In the sessions, one participant inspected several closed positions before choosing where to reveal. That is a legitimate navigation action, but it should not reveal faces. The accessible name needs to say where the card is and that it is closed, just as the visual back gives position and state without identity.

The current implementation makes that boundary explicit. `Board.tsx` passes a face only for a position in the face-up set. `Card.tsx` renders the neutral back when the face is `null` and mounts the SVG only when the identity is available. `describeCard` constructs a positional name for a closed card and a fuller name for a visible one.

This approach avoids a misleading accessibility shortcut: hiding a full face label visually while leaving it in accessible text from the beginning. Such a shortcut would change the game for someone reading names. The [card-state guide](pair-identity.md) and [screen-reader notes](screen-reader-notes.md) explain the present information model.

## Identity names were long for a reason

The current visible face name includes palette name, fill, shape, and marker position. Face 0 is `cyan solid circle, marker top`; face 64 is `cyan outline circle, marker top`. Shape and marker alone would not distinguish those two, so fill is meaningful rather than decorative wording.

The discussion considered shorter names that omitted information a visual player could still see. They were easier to say but ambiguous across the full face catalogue. Another alternative assigned arbitrary nicknames to each identity. That would reduce phrase length only by requiring a separate vocabulary to be learned.

The current compositional names correspond to the same geometry used by the artwork. A participant can make their own memory shorthand after hearing them, while the interface retains a stable description. Shortening these names changes the information available to a player.

## Navigation and revelation were separate acts

In the sessions, participants often moved to a remembered location before deciding whether to activate it. This makes it important that focus changes do not consume a move or reveal a face. Current focus state belongs to the board hook; game selection belongs to `GameState`.

Matched positions remain available as spatial landmarks. They expose their matched state and an unavailable action through `aria-disabled`, while the core ignores an activation on them. A player moving down a column still moves down a column after finding several pairs. Removing solved positions from the route would make the board's geometry change while it was being memorized.

The plain arrow keys handle spatial movement, and native Tab traversal leaves the board. The focus-trap incident shows why a successful entry and reveal are not enough evidence for the complete path. [Incident: focus trap](incident-focus-trap.md) describes the failure, and [keyboard navigation](board-keyboard-navigation.md) documents the current destination rules.

## Results needed event meaning

The sessions distinguished hearing the currently focused card from learning that the turn had resolved. A second reveal might change several visible elements at once: two card states, a move count, pair progress, and perhaps the final banner. Reading every changed statistic as an independent event would be noisy and could obscure the outcome.

Current session feedback supplies an atomic turn result. A final pair combines the match and completed result into the winning message. The session announcement counter advances on each resolved attempt, so consecutive misses with identical text can remain separate events. Ordinary clock updates and focus moves do not create new turn feedback.

The keyed feedback span in `Game.tsx` is important here because equal strings do not necessarily cause a new text mutation. A second identical miss is still another attempt. The current browser announcement tests observe result changes around these cases; they do not listen to a screen reader speaking them.

The study therefore treated “the event is represented in the accessibility tree” and “the person heard it at the expected time” as related but separate observations. The former can be automated in this repository. The latter remains an acceptance activity with the actual assistive tool.

## A miss did not freeze orientation

While a mismatching pair waits to close, card activation is blocked but focus movement remains available. In the discussion, this allowed someone to orient toward the next remembered position without changing the pending attempt. Closing the mismatch itself did not need to repeat the old result.

The same separation applies to an imported unfinished board. It loads paused, exposes the restored card information, and waits for Resume before accepting a new reveal or advancing active time. The player can inspect positions while paused. Resume returns to the remembered card before its own button disappears.

These are not merely focus refinements. They preserve a clear relationship between information, input, and elapsed time. The [snapshot-selection notes](snapshot-selection-notes.md) and [dialog policy](dialog-policy.md) provide adjacent discussions of restoration and interaction context.

## What the automated checks record

The README identifies Chromium 156.0.8078.4 and Playwright 1.64.1 for the accessibility-tree journey. It says the journey checks control names, closed and revealed card names and states, turn results, the final summary, Tab exit, and New Game. It also records `Screen reader: not run.`

That is useful, bounded evidence. It can catch hidden identity leaks, missing names, incorrect disabled state, absent summaries, and a broken keyboard route. It cannot establish how a particular screen reader queues speech, handles browse versus focus mode, or interacts with a user's verbosity settings.

The study produced qualitative findings rather than a numerical task success rate. Those findings and the automated repository checks address different questions.

## Questions left for acceptance

The research notes carried forward several ordinary questions: can a user identify a visible face reliably from its name, do repeated identical results arrive distinctly, and does the winning result remain understandable without racing another announcement? Does Resume feel like a continuation from the restored location? Can someone leave the board and start again without assistance?

These questions correspond to existing input and controls behavior. The [acceptance page](acceptance.md) and [accessibility review notes](meeting-notes-accessibility.md) keep the current technical deliverable and the human confirmation separate. The sessions treated a full memory game as an information journey, not a collection of isolated labeled buttons.
