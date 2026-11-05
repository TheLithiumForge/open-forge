# Accessibility audit
This page records accessibility evidence and findings from the Neon Pairs source and tests.

The README identifies Chromium 156.0.8078.4 and Playwright 1.64.1 for the recorded accessibility-tree journey. It says a screen reader was not run and leaves spoken confirmation to user acceptance.

## Audit scope and disposition

The review covers native controls, keyboard movement, information available in card names, result events, focus visibility, scrolling, reduced motion, face distinctions and the equivalent-zoom layout. It follows [Input](input.md), [Controls](controls.md), [Visuals](visuals.md) and the journeys in [Acceptance](acceptance.md).

The disposition is supported implementation with outstanding manual confirmation. It is not a declaration of conformance to an accessibility standard. The visual specification itself cautions that choosing a minimum card target size does not establish complete conformance.

| Area | Evidence present in source | Qualification |
| --- | --- | --- |
| Complete keyboard play | Keyboard journey and full Chromium tree test | Spoken output remains unverified |
| Card information | Pure name tests and exact hidden-DOM assertions | Stored name expectations and tree assertions |
| Result events | Session event tests and DOM mutation observations | Speech was not tested |
| Large-board focus | Bounds, remembered-entry and clipped-edge cases | Automated geometry in Chromium |
| Touch movement | Tap completion and CDP scroll journeys | Real-device long press remains acceptance work |
| Equivalent zoom | 640x400 viewport at scale 2 | Does not exercise every browser zoom implementation |
| Face identity | 128-face geometry and screenshot comparisons | Human distinguishability still needs review |
| Reduced motion | Computed-style assertions | Automated style checks |

The qualifiers identify where evidence stops. They are not findings that the game necessarily fails those behaviors. [Known issues](known-issues.md) covers test-input dependencies and unverified compatibility.

## Native controls and discoverability

`Card.tsx` renders a native button for each position. The main size choice is a native select, action controls are buttons, and reproduction uses a details/summary disclosure with labeled fields. This gives the browser established focus and activation behavior instead of reproducing it with generic clickable containers.

The browser tests locate Grid size, New Game and the reproduction actions by role or label. The accessibility journey captures control nodes before entering the board. Literal contract tests complement the shared label constants, so renaming a constant cannot silently change both the UI and every test lookup.

The board has a region label of Game board and references the visible `board-help` paragraph. The help explains Tab entry, arrows and Enter or Space activation. It is part of the rendered page rather than a hidden developer instruction. A current focus on page load is preserved; the game does not immediately pull the user into the grid.

Finding: the markup follows the specified native-control approach. The source contains browser checks for names, field limits and focus after actions. The tree artifact captures the control names and states exposed by Chromium.

## Keyboard movement stays separate from play

`navigationTarget` handles plain directional arrows. It calculates row and column destinations and stops at edges. Unsupported keys and modifier combinations remain with the browser. The pure keyboard tests exercise both the destinations and the excluded inputs.

The board remembers a tab stop and assigns the remaining cards negative tab indices. Native Tab and Shift+Tab leave the board; arrows move within it. Focused matched cards expose an inactive action state through `aria-disabled` while remaining focusable. This lets a player revisit known positions without accidentally increasing moves.

During a pending miss, arrows still move focus while activations have no game effect. Imported unfinished progress also blocks activation until Resume without presenting all unmatched positions as disabled cards. The distinction between a matched card and paused session is preserved in the accessibility journey's expectations.

Finding: the implementation separates navigation, eligibility and focusability. `keyboard.e2e.test.ts`, `play.e2e.test.ts` and `accessibility.e2e.test.ts` cover the user journey, while `board/keyboard.test.ts` covers the coordinate calculation. Held keys and additional shortcut combinations outside the specification are not covered by these tests.

## Hidden and revealed information

A hidden card's accessible name describes its one-based row, column and closed state. It contains the same neutral back markup as every other hidden card. Face SVGs mount only after reveal or match; the face ID is not left in an attribute for a testing solver to read.

`input-contract.e2e.test.ts` checks each initial button's attributes and inner markup against its position. `play.e2e.test.ts` completes the literal deal without inspecting hidden faces. The expected pair locations come from the documented board code, not from a production diagnostic channel.

Revealed and matched names add a lower-case face description and state. The design combines palette name, fill, shape and marker position, allowing a player to remember the distinction without relying on the image. The pure `describeCard` test and full tree journey are intended to hold the wording stable.

Finding: the information boundary is visible in the source and exact DOM tests. `card-state.fixture.json` and `accessibility.fixture.json` store reviewed expected names. Using reviewed expectations is preferable to deriving them from the current name function, which could preserve the same defect on both sides of an assertion.

## Result feedback and repeated events

`session.ts` distinguishes first reveals from completed turns. A second eligible reveal produces match or miss feedback; a winning turn produces feedback containing the match and final result. Clock updates and closing a miss do not create a new turn event. The rendered status is polite and atomic, so its complete text is exposed as the result.

The session tracks an announcement number, and `Game.tsx` keys the feedback span with that event value. Identical wording can therefore represent successive events. The announcement tests install a MutationObserver and compare observed result changes with the expected sequence, including repeated invalid loads and repeated misses.

This is a useful approximation of what assistive software can receive. It does not prove what a particular screen reader speaks, whether its queue interrupts a prior message, or how quickly it announces the final turn. The source comment about observer callbacks is an explanation of the test model, not a speech recording.

Finding: event progression and DOM exposure have targeted tests. Spoken output remains outstanding acceptance evidence, especially for consecutive equal results and the final match. [Screen-reader notes](screen-reader-notes.md) and [Announcements policy](announcements-policy.md) provide the related user-facing contract.

## Focus after actions and scrolling

New Game, Restart, load and export actions preserve focus on their initiating control. Resume is different because it removes itself: `Game.tsx` calls the board's remembered-focus method before clearing pause. The control-contract and snapshot browser cases inspect that ordering.

For keyboard focus, `use-board-focus.ts` suppresses the browser's centering scroll and uses the nearest-scroll behavior. Card CSS provides a 6px scroll margin. The visible focus outline is white, 3px wide and offset by 2px. The bounds helper includes both dimensions when checking visibility.

Pointer focus has a separate timing concern. Scrolling a partially visible card during the press can move the release target. The implementation allows the click to complete and scrolls afterward. Coordinate-based mouse and touch cases exercise slivers at clipped edges, while the focus-entry suite checks outlines when keyboard navigation follows a pointer click.

Finding: the most difficult cases are explicitly represented, including remembered entry after scrolling away and an interior 16x16 card. Automated bounds checks tolerate only small geometry rounding. They do not substitute for reviewing focus visibility on an actual display or in an untested browser engine.

## Layout, targets and equivalent zoom

The large-board checks render all 256 positions and preserve card size while allowing the board to scroll. At 360x800 and 1280x800, tests reach both ends by keyboard and touch. A further 1280x1400 case exercises the viewport height cap. A 320x640 check covers the default board and main controls.

The equivalent-zoom journey sets a 640x400 CSS viewport with device scale 2. It completes a small board, imports completed and unfinished progress, resumes, changes to 16x16 and reaches the final card. `expectReadableText` collects text rectangles for controls, status and completion, checking width and overlap rather than only container boxes.

Visible-scrollbar cases add evidence for layouts where traditional scrollbars consume usable width. They first establish that scrollbars are present. This precondition matters because a hidden-scrollbar pass would say little about the intended geometry.

Finding: the specified difficult layouts have dedicated automated checks. The equivalent setup is intentionally described as equivalent layout evidence, not a recording of using a browser's zoom menu or pinch gesture. The viewport meta tag and CSS should continue to be considered alongside that test when reviewing zoom changes; [Zoom behaviour](zoom-behaviour.md) holds the product details.

## Artwork and non-color distinctions

Faces derive from eight shapes, eight marker positions and two fills, for 128 identities. Color repeats the marker distinction. The art build renders each face at 44px, and tests save color, grayscale and neutral sheets. Neutralized rendered silhouettes are checked for uniqueness.

Additional tests inspect the marker's radius, surrounding stroke and palette application. The matched-mark browser case imports a completed 16x16 board containing every face, then calculates whether the corner check overlaps the shape or marker. This catches a legibility problem that inspecting the matched border alone would miss.

Finding: the automated evidence checks geometric distinctness and occlusion, while human recognition at the minimum size remains a review task. Unique pixel masks do not prove that every person can distinguish two faces. The screenshots are useful audit artifacts precisely because they allow that visual review to happen separately from the numeric checks.

The source uses the specified dark page, panel, card and text colors. `style.e2e.test.ts` checks computed values and hover changes; it does not calculate a broad contrast conformance report. The related discussion belongs in [Contrast audit](contrast-audit.md).

## Motion and silent feedback

Card CSS transitions only border color over 120ms. Completion uses a single 600ms pulse, and reduced-motion styles disable both effects. Imported completed progress renders its summary without the live-completion celebration path. The motion browser cases inspect computed transition and animation properties.

The game does not depend on sound to reveal a card, report a miss or show a win. Browser tests instrument audio-related APIs, check media elements and watch media requests during play and reset. This supports [Feedback without audio](feedback-without-audio.md). Browser-supplied assistive speech remains a separate tool, rather than audio generated by the game.

Finding: motion preferences and silent operation have explicit implementation and tests.

## Evidence still to obtain

The accessibility test writes `accessibility-journey.json` under its test-result directory, recording normalized tree captures, browser and Playwright versions, and a null screen-reader field. It rejects environment-specific URLs and internal node identifiers in that record.

The documented browser command regenerates the tree evidence against the current build. Manual acceptance then follows the keyboard journey with a screen reader and checks spoken results, repeated equal feedback, completion, leaving the board and starting again. Real-device long press is another explicit acceptance boundary.

These acceptance items follow the specification. [Release checklist](release-checklist.md) shows where the evidence belongs, and [Browser support](browser-support.md) keeps the Chromium record distinct from untested engines and assistive-software combinations.
