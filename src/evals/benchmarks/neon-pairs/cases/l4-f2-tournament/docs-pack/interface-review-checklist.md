# Interface review checklist

This page is a review route through the existing controls, input behavior and related interface policies.

A useful review follows a few populated journeys and checks source at the transitions. An empty board screenshot cannot show preserved drafts, matched-card focus or a stale timer after Restart. The checks below are grounded in the current source and specifications; links identify the separate policies for feature surfaces that do not exist yet.

## Establish what changed

Start with the actual component and its callback. A control can retain its appearance while changing submission, focus or session behavior. A CSS-only change can also alter the hit area or clipping without touching an event handler.

- Identify whether the change affects `Controls`, `Reproduction`, `BoardCodeForm`, `GameStatus`, `Board`, `Card` or the focus hook.
- Follow callbacks into `use-game.ts` and `session.ts` when the change replaces a game, accepts input or publishes feedback.
- Compare element types, ids, label associations, explicit button types and source order as well as class names.
- Check mobile selectors that depend on direct children before accepting a wrapper-only refactor as behaviorally neutral.

The current page has no name editor, dialog or result table. A change introducing one has more scope than styling an existing control. Its relevant policy is linked below, and its current behavior has to be established from its own implementation.

## Deal controls

Use [Control bar](control-bar.md) and the Controls specification as the reference for these observations.

- Grid size is a labeled native select with the seven supported choices, and its selected value agrees with the current game after either successful load type.
- New Game keeps the current size and creates a fresh deal; Restart repeats the current board code.
- Both actions reset selection, pairs, moves and elapsed time, and leave their action button focused.
- Replacing a game during a mismatch does not let an old timer change the replacement.
- The board's next native entry starts at its first card after a replacement, rather than retaining an index from the old deal.

A code comparison can distinguish New Game from Restart even when the two visible decks happen to share some positions. A move-count comparison alone cannot establish that distinction. For size changes, check both card count and the controlled select value.

The current buttons act immediately. A reviewer should not claim existing confirmation coverage from the presence of a leave policy elsewhere. [Dialog policy](dialog-policy.md) records that separate feature boundary.

## Text tools and errors

The useful draft is one that differs from the currently displayed code, and the useful game state has some progress. [Reproduction panel](reproduction-panel.md) and [Error display guide](error-display-guide.md) provide the source trace.

- The native disclosure starts collapsed and is named Reproduce a game.
- Board code and Snapshot JSON retain visible, correctly associated labels and their existing native limits.
- Editing Board code, then playing a card, keeps the edited draft.
- Invalid board-code and snapshot submissions preserve the current game and the entered text, and show their exact existing load outcomes.
- Successful Board code submission refreshes the field from the accepted code and updates the selected size.
- Export fills selectable, readable JSON without replacing or pausing play.

Try Enter from Board code as well as clicking Load board. Success focuses Load board because the input is replaced. Failure from the input keeps editing focus there. A click-origin failure keeps focus on the clicked action. The distinction can be lost in a test that only inspects the final board.

The snapshot textarea is multiline and has its own local state. Enter is not an implicit import action. Its draft is not automatically rewritten on every turn or deal replacement. The visible notice sits outside the disclosure, so closing it does not hide an unfinished import's way to continue.

## Import and Resume

A valid unfinished snapshot and a valid completed snapshot exercise different paths. The current specification gives concrete examples and constraints in [Snapshot format](snapshot-format.md).

- Unfinished progress loads paused, including an imported position that has not started its clock yet.
- Card activation is blocked while paused, but focus movement still works.
- Resume focuses the board's remembered card before its own button is removed.
- A pending mismatch continues its remaining active interval after Resume.
- Completed progress loads completed, with static completion output and no Resume button.
- Import is not treated as a newly achieved play completion for best-record updates.

A far remembered card on a large board makes the Resume check meaningful. Scroll the board away from that card before resuming and inspect both focus and nearest visibility. Merely finding any focused button afterward would not establish that the remembered destination survived.

## Board movement and activation

Use [Board keyboard navigation](board-keyboard-navigation.md), [Pointer and touch](pointer-and-touch.md) and [Disabled state guide](disabled-state-guide.md) together.

- Tab enters the remembered card and can leave the board again; reverse traversal is native too.
- Plain arrows move one row or column, stop at edges and do not reveal cards.
- Modifier combinations and unlisted keys retain browser behavior.
- Enter, Space, primary click and completed tap reach the shared card action.
- Hover, touch scrolling, pointer cancellation and context-menu interaction do not choose a card.
- Matched cards stay focusable and expose their matched state while activation leaves the turn unchanged.
- Repeated activation of the first revealed card does not add a move.

For a mismatch, attempt a third activation while the pair is visible and move focus elsewhere. The pair stays until its active wait expires; closure does not drag focus back. Hide and show the page during the interval when a lifecycle change is in scope.

Pointer checks include a card partly clipped at the viewport edge. The source intentionally avoids moving a card under a held pointer, then scrolls it after the click. A centered-card click can pass even when that edge behavior regresses.

## Names and accessible content

Inspect actual DOM and browser accessibility exposure. Visual equivalence is insufficient when hidden data or labels are involved.

- Hidden card names expose position and closed state without a face description or hidden face artwork in their DOM.
- Revealed and matched card names include the correct face description and state.
- Face and check SVGs do not introduce extra focus targets or duplicate the button's description.
- Current statistics expose their visible terms and values without alternate labels replacing the numbers.
- Completion makes the final moves and elapsed time available in the persistent summary.

[Screen reader notes](screen-reader-notes.md) records the full journey and the distinction between tree evidence and spoken review. The repository README records the browser and tool versions and says no screen reader was run. A review report should describe the evidence it actually has.

When a feature introduces name input, consult [Text entry policy](text-entry-policy.md), [Name length policy](name-length-policy.md) and [Duplicate names policy](duplicate-names-policy.md). The existing code and JSON text fields do not establish those name checks. For new result content, consult [Tables policy](tables-policy.md).

## Results and repeated events

[Announcements policy](announcements-policy.md) owns the publication model. In a populated game, inspect the event output while also checking that ordinary rendering stays quiet.

- A first reveal changes the card without publishing a resolved-turn result.
- A match or miss publishes its result once per resolved turn, including consecutive identical misses.
- Focus movement, clock updates and mismatch closure do not repeat the prior result.
- The winning result combines the last match and completion information.
- Repeated invalid loads and repeated successful snapshot loads each publish their load outcome.

The session's `announcement` value and the keyed feedback span explain repeated identical text. A refactor that only compares feedback strings can accidentally remove the event distinction. A refactor that updates the identity on every render can produce the opposite failure.

## Layout and reachability

Read [Form layout guide](form-layout-guide.md), [Target size policy](target-size-policy.md) and [Focus policy](focus-policy.md) for their detailed scope.

The current visual contract includes a default board fitting a phone, large-board overflow confined to the board viewport and complete focus outlines when cards are brought into view. Review the first, last and an interior card. Container padding can protect corners while an interior nearest-scroll target still clips its outline.

The stated large-board views include `360x800` and `1280x800`. The equivalent zoom layout is `640x400` with device scale 2. Controls, statistics and the completion sentence remain readable in those constrained layouts, with the outer page staying within its viewport width.

Check the expanded reproduction panel as well as the collapsed game. Fields shrink within their rows, action rows wrap and the textarea remains selectable. The native select popup and summary behavior are browser interactions; the surrounding CSS should not be assessed as though they were custom drawn widgets.

## Choose evidence that matches the change

Pure geometry is covered by `keyboard.test.ts`; focus and browser default behavior are covered by browser journeys. Parser tests cover malformed data independently of field layout. The accessibility journey captures roles, names, states and result text. These are complementary forms of evidence.

A small label or spacing change does not call for inventing a new test framework. Existing contract, controls, layout and zoom checks already target many interface failures. A callback or lifecycle change needs the populated journey that can expose its effect, especially when a pending mismatch or disappearing Resume button is involved.

The review note is most useful when it names the behavior changed, the evidence observed and any remaining limit. It can say that a tree exposes the expected result without claiming speech was heard, or that a phone layout was measured without claiming every browser was exercised. [Browser test guide](browser-test-guide.md), [Accessibility audit](accessibility-audit.md) and [Acceptance](acceptance.md) connect these checks to the wider project process.
