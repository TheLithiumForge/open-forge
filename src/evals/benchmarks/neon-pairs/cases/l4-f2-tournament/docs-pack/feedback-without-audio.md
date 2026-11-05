# Feedback without audio

This page explains how Neon Pairs makes actions, turn results and completion understandable while the game stays silent.

The current contract is [Sound](sound.md), specification version 2.2. Silence covers revealing a card, finding a pair, missing a pair, resetting a deal and finishing the board. The application has no audio files, playback calls or sound control. This is a complete mode of play rather than a muted version waiting for someone to enable its feedback.

## Follow the change from state to display

`src/game/game.ts` owns selection and matching. `src/game/session/session.ts` derives turn feedback after an accepted activation. `src/game/Game.tsx` brings the board, status and feedback together. There is no audio step between a successful state change and its presentation.

| Player action or event | Card and status response | Result feedback |
| --- | --- | --- |
| Reveal the first eligible card | Its face replaces the back; the clock starts if this is the first reveal | The existing result text is unchanged |
| Activate that same first card again | Selection and moves are unchanged | No new turn result |
| Reveal a matching second card | Both faces remain visible with match marks; moves and pairs update | A pair-found sentence |
| Reveal a nonmatching second card | Both faces remain visible for the mismatch interval; moves update | A no-match sentence |
| Mismatch time expires | Both selected faces return to their common backs | The previous result remains; closing adds no result |
| Activate an already matched card | The matched card stays as it is | No new turn result |
| Find the last pair | All cards are matched; the final status and win banner are visible | A combined pair-found and completion sentence |
| New Game or Restart | The replacement board is closed, with cleared moves and elapsed time | The previous turn text is cleared |

The table describes normal play. An imported unfinished snapshot has an additional paused state, discussed below. The full selection rules belong in [Turn resolution](turn-resolution.md) and [Selection state](selection-state.md).

## A closed card and a revealed card carry different information

`Board.tsx` computes the visible positions through `faceUpCards` and passes a face number to `Card.tsx` only for those positions. Every other card receives `face={null}`. In that case the card contains a `.card-back` span, a small neutral diamond, rather than face artwork. The hidden face is also absent from the card's accessible name. Silence does not create a separate information path that reveals identities early.

`describeCard` in `src/game/board/card-state.ts` produces the class state and label from the card's position, visible face and matched flag. The first hidden card on any board is named:

> Row 1, column 1, closed

For face 0, a revealed card at that position is named:

> Row 1, column 1, revealed, cyan solid circle, marker top

After its pair is found, the same position is named:

> Row 1, column 1, matched, cyan solid circle, marker top

The inline SVG is `aria-hidden="true"` and `focusable="false"`. The button name carries the semantic description instead of making the player traverse individual paths and circles. The check SVG is hidden from the accessibility tree in the same way; the button's matched state supplies its meaning. See [Screen reader notes](screen-reader-notes.md) and [Card artwork](card-artwork.md).

## Identity survives loss of color

A revealed face uses shape, marker position and fill. Color reinforces the marker distinction. `faceDesign` in `src/game/board/face-design.ts` maps a face ID to those parts and to the phrase used in the button name. Faces 0 and 64 both have a cyan circle with a top marker, but one is solid and the other outlined. The drawing and name preserve that difference. See [Pair identity](pair-identity.md) for the full model.

There is also a deliberate difference between a face's marker and a matched card's check. The marker belongs to identity and can appear in different directions. The check belongs to outcome and stays in the card's top-right corner. In `card.css` the check occupies 16% of the card's width and height. The nearby comment explains that a wider mark would reach the top-right identity marker. That is a concrete example of feedback being added around the memory task rather than covering the information the player is trying to retain.

## A match stays readable

A match takes effect on the second eligible reveal. The pair goes into `game.matched`, selection returns to `closed`, and both cards remain face up. The player does not wait for a sound or for an animation to finish before the result exists.

Matched cards use the Green border, `#8BEA7D`, and a check mark in the same color. They retain full opacity. The border helps with scanning; the check makes the matched condition visible without relying on the border's hue. Their faces remain available for orientation as the rest of the board changes.

`Card.tsx` sets `aria-disabled={matched}` rather than using the native `disabled` attribute. A matched card remains reachable by board navigation and still has its position and face name. Its activation has no game effect. This matters when the player crosses a completed part of the board: focus movement remains spatially predictable, and an announced matched state explains why that card cannot start another pair.

Hover does not erase the outcome. The generic hover rule changes a card border to the Text color, but the later `.card.matched` rule retains the Green border on matched cards. A pointer passing over a secured pair therefore does not temporarily make it resemble an ordinary revealed card. The current CSS and `tests/product-contract.e2e.test.ts` both show this behavior.

For the first match, the visible result is:

> Pair found. 1 pair matched.

A later ordinary match substitutes the actual count and plural, for example:

> Pair found. 2 pairs matched.

## A miss has a beginning and an end

A miss first reveals the second face and increments moves. Both faces stay visible for `MISMATCH_MS`, currently 800 active milliseconds. The player can inspect them before they close together. The feedback says:

> No match. Cards close after a short pause.

There is no error tone, red failure panel or loss of previously matched cards. The message explains both the outcome and the upcoming closure. The visible faces provide the useful memory information during the interval.

During this interval, card activation is ignored, while keyboard focus can still move. Focus and selection are distinct: moving the white outline onto a third card does not reveal it. This distinction is particularly useful in silence, because the visible pair can remain stable while the player prepares the next selection. It also explains a common apparent nonresponse: pressing Enter during the mismatch pause does not queue a reveal for later.

`advanceGame` subtracts active milliseconds from `selection.remainingMs`. When the remainder reaches zero, selection becomes `closed`; `faceUpCards` then omits those two positions. Closure changes the board without generating another result sentence. `tickSession` updates the game but leaves the previous feedback and announcement value alone. The no-match sentence can therefore remain visible after the faces have closed. It describes the latest turn, rather than acting as a label for the instantaneous selection state.

Hiding the document suspends active timing. Returning continues the remaining interval. A player who switches away shortly after a miss does not lose the rest of that face-viewing interval while the game is hidden. The implementation is in `use-session-clock.ts`; timing details and boundary cases belong in [Mismatch interval](mismatch-interval.md) and [Elapsed time rules](elapsed-time-rules.md).

## Status gives a persistent account

`GameStatus.tsx` renders a definition list with moves, matched pairs, elapsed time and the saved best for the current size. These values are available before any turn result and continue to be available after it. A short result message need not carry all the context the player might want to inspect.

## The result text is also accessible feedback

`Game.tsx` renders `session.feedback` inside a paragraph with `role="status"`, `aria-live="polite"` and `aria-atomic="true"`. The sentence is ordinary visible text as well as accessible status content. It is not a transcript of audio generated by the application.

The child span uses `key={session.announcement}`. A new resolved turn increments that value in `activateSession`. Two consecutive misses have identical feedback strings, but distinct event values cause the span to be replaced for the second miss. A text-only equality check would miss that distinction and could leave an assistive technology user without evidence that another turn had resolved.

This event mechanism deliberately does not follow the clock. `tickSession` can create a new game object many times while leaving the event value unchanged. Focus changes and the first reveal likewise do not publish a fresh turn result. That is why a time update, an arrow-key press or mismatch closure does not repeatedly present the last result as if it had just happened.

“Silent” here describes what the game itself plays. A screen reader may speak the accessible names and status supplied by the page according to the user's own setup. Neon Pairs does not call `speechSynthesis.speak` to read its messages. A user choosing speech output is using their assistive technology; that does not require a game-level sound switch. See [Announcements policy](announcements-policy.md) for the announcement contract and [Audio research notes](audio-research-notes.md) for the design rationale.

## Completion remains after the moment passes

The last match produces a combined result. With eight moves and a displayed elapsed time of 0:03, the feedback is:

> Pair found. All pairs found in 8 moves and 0:03.

The visible banner is separate:

> Every pair found in 8 moves and 0:03. Well played.

Both derive their moves and formatted time from the completed game. The board also remains fully matched, and the status stays present. A player who looks away at the final activation can recover the result from the page afterwards. There is no need to have heard a brief victory cue.

A completion reached through play sets `session.celebration` to true. `GameStatus` adds the `celebrate` class to the `.victory` paragraph. `status.css` gives it a single 600ms `victory-pulse`, with a panel background and reduced opacity at its midpoint. The banner then stays visible. Under `prefers-reduced-motion: reduce`, the animation is disabled. Card border transitions are also disabled under that preference; normally they last 120ms.

## Replacement, import and pause

New Game and Restart replace the current session game through `replaceGame`. That clears the previous feedback and load notice, removes the celebration flag and increments the generation. The board is keyed by that generation in `Game.tsx`, which resets its remembered focus position with the replacement. The player sees a closed board and cleared counters rather than a reset sound.

An unfinished snapshot is different from a new deal. Its visible cards, matched positions and counters describe saved progress, but the imported session is paused. Its status says:

> Loaded snapshot. Select Resume to continue.

A Resume button appears beside the load notice. Activation and active time stay blocked until Resume. The cards themselves are not all given matched styling to indicate this pause; a paused unmatched card is still unmatched. The explicit notice explains why a revealed face might remain on screen longer than the ordinary mismatch interval.

The Resume handler in `Game.tsx` focuses the board's remembered card and then calls the controller's resume action. That action clears feedback and load notice while leaving the imported game intact. If the imported selection was a mismatch, its remaining active interval can now continue. Resume does not manufacture a new pair result for the saved selection.

A completed snapshot reports `Loaded completed snapshot.` and shows its completed board and banner without the celebration class. Invalid data reports `Invalid board code.` or `Invalid snapshot.` while retaining the current game. Load notices appear beside the reproduction controls and in session feedback. See [Reproduction panel](reproduction-panel.md) and [Snapshot selection notes](snapshot-selection-notes.md).

## Finding the evidence

The following tests cover the feedback behavior:

- `tests/browser-environment.e2e.test.ts` instruments audio constructors, media playback and synthesized speech, counts media requests, finishes a board and restarts. It also checks that no audio or video element is present.
- `tests/announcements.e2e.test.ts` observes result DOM changes. It covers repeated misses, focus movement, clock advancement, closure and repeated load notices.
- `src/game/session/session.test.ts` checks event increments, unchanged events during closure, paused imports and the distinction between played completion and imported completion.
- `tests/matched-mark.e2e.test.ts` checks the check mark against every face on a 16x16 board at a small viewport. It looks for overlap with either the main shape or its marker.
- `tests/motion.e2e.test.ts` checks border transition duration, the completion pulse and reduced motion.
- `tests/accessibility.e2e.test.ts` follows a complete keyboard game and captures names, states and result text in Chromium accessibility trees.
