# Accessibility review notes

These are meeting notes about the complete Neon Pairs access journey, including the distinction between automated browser evidence and human confirmation of spoken output.

The README records Chromium accessibility-tree checks and leaves confirmation of spoken output to user acceptance.

## Start with the route through the page

The review began at Grid size rather than at an isolated card. A complete player journey finds the size and start controls, enters the board, moves without revealing, activates two positions, understands the result, completes the board, leaves it, and starts again. Reproduction adds another route through draft input, validation, paused progress, and Resume.

The keyboard reviewer emphasized that each route needed an exit. The current board uses a remembered tab stop and plain arrows for spatial movement. It leaves Tab and Shift+Tab to native traversal. A matched card remains focusable and exposes an unavailable action rather than disappearing from the board's coordinate system.

The discussion reviewed the current keyboard design. [Input](input.md), [board keyboard navigation](board-keyboard-navigation.md), and [focus policy](focus-policy.md) define the behavior. The [focus incident](incident-focus-trap.md) gives an example of a review that tested entry and forgot departure.

## The information boundary had to match the picture

The reviewer examining names checked a closed position before opening it. Its name communicates row, column, and closed state. It does not disclose which face is waiting there. Current `Card.tsx` also withholds the face SVG while closed, so the DOM does not offer another route to the hidden identity.

Once visible, the name includes the structured face description. Shape, marker direction, and fill are all meaningful because the full face catalogue uses them to distinguish pairs. Color reinforces marker position and remains part of the stable verbal description. A matched state is available in the name and disabled property as well as the visible green treatment and check.

The review rejected the idea that adding descriptive hidden text is always an access improvement. Description can disclose information that the game is deliberately withholding. The correct question is whether the player receives the same currently available information through the accessible representation.

The [screen-reader notes](screen-reader-notes.md), [pair identity](pair-identity.md), and [card artwork](card-artwork.md) explain the current boundary in more detail.

## Results were reviewed as events

The group then followed a miss, another identical miss, a match, and the final pair. This sequence matters because repeated equal text can be one event in a renderer even when it represents two turns to a person. Current `Session.announcement` advances for each published result, and the keyed span in `Game.tsx` makes the event visible to the DOM.

Clock updates, focus movement, and the closing of a miss do not publish another turn outcome. The final pair produces feedback that includes both the successful match and the completed result. This prevents the last turn from being split into competing pieces of result information.

Browser tests observe result mutations and inspect accessibility-tree properties. Those checks provide evidence that the intended event is represented. They do not establish how a particular screen reader queues or voices it. The meeting kept that difference explicit instead of treating an observed mutation as a recording of speech.

The [announcements page](announcements-policy.md) documents the current implementation expectations. The [screen-reader research notes](research-screen-reader-sessions.md) discuss the research findings alongside the automated evidence record.

## Focus had visible and lifecycle consequences

The visual reviewer asked to see a far interior card on the largest board, then the last card. Owning focus was not sufficient if the outline was clipped. Current cards have a 6px scroll margin around a 3px white outline with 2px offset, and focus movement uses nearest alignment.

The group also inspected returning with Tab after leaving the board. Chromium may center a card before a focus handler runs, so the current nearest-scroll helper restores the recorded board position before requesting nearest alignment. The purpose is a visible destination with a restrained change in view, not merely a mathematically correct active element.

Resume raised a different focus question. An unfinished imported board is paused; selecting Resume removes that button. Current `Game.tsx` focuses the board's remembered card before clearing the pause. New Game, Restart, loading, and export controls remain focused because their buttons remain present. These are different lifecycle cases, even though all may leave the board looking newly ready to play.

The review did not add a global instruction to focus the first card after every action. That would disrupt the control journey. The [snapshot pause decision](adr-037-snapshot-pause.md) explains the specific current handoff.

## Visual review included dense and enlarged layouts

The artwork reviewer used the current identity scheme rather than judging colors alone. Eight shapes, eight marker directions, and two fills provide 128 faces. The diagnostic sheet draws them at the minimum size, and grayscale review checks whether structure remains distinguishable when palette differences are unavailable.

Matched cards keep full opacity, a border, and a check. Hover does not remove the matched border. Reduced motion removes the card border transition and completion animation while leaving the state information and final summary visible. The mismatch observation interval remains a game rule rather than an effect removed by a media preference.

The layout discussion used the specified large-board views and the equivalent zoom configuration of 640x400 CSS pixels at device scale 2. Controls, status, and summaries need readable space around the board, while the grid itself may scroll. The current game leaves zoom enabled. These checks do not amount to a blanket claim of standards conformance.

The [contrast audit](contrast-audit.md), [zoom behaviour](zoom-behaviour.md), and [motion policy](motion-policy.md) contain the adjacent current detail. The meeting notes record why those topics were reviewed together as access to information rather than as separate visual polish.

## Touch evidence stayed bounded

The browser suite can send touch movement and verify that scrolling reaches far cards without revealing them. The current card activation uses completed native clicks rather than pointer-down. This supports the distinction between a tap and moving the viewport.

A real long press is explicitly different evidence. The specification leaves confirmation of context-menu behavior on a physical device to user acceptance, because automated browsers cannot faithfully generate that gesture.

The [phone usage notes](research-phone-usage.md) discuss this distinction in context.

## Recorded accessibility evidence

The README identifies Chromium 156.0.8078.4 and Playwright 1.64.1 for the accessibility-tree journey. That journey captures named controls, card states, resolved turns, completion, board exit, and a new game. It records the absence of a screen-reader run and leaves spoken review to acceptance.

The meeting closed with agreement that tree checks and human confirmation are separate evidence. Human confirmation can add its own record of the tool and environment used.

The review connected the existing input, controls, visuals, and acceptance contracts into one complete route through the game, and named where the current evidence stops. [Acceptance](acceptance.md) remains the place to follow the finished journey; [roadmap notes](roadmap-notes.md) retain further investigation as discussion.
