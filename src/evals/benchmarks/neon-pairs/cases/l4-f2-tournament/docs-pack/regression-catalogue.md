# Regression catalogue
Neon Pairs regression tests protect against recognizable failures in game behavior.

The failures below led to regression coverage for clock timing, session lifecycle, saved formats, focus and feedback. The source contains the test names and current behavior. Related historical pages include [Clock drift](incident-clock-drift.md), [Focus trap](incident-focus-trap.md) and [Large-board layout](incident-16x16-layout.md).

## A mismatch used the last render's time

The visible symptom was a pair closing slightly too early or staying open too long when the second reveal fell between render ticks. An implementation that rounds every clock sample can lose fractional time before the mismatch begins. An implementation that starts the interval from the last status render can charge earlier active time to a newly revealed pair.

`fractional-clock.e2e.test.ts` drives `performance.now` directly. The first reveal is at 351.25ms, the second at 362.25ms and the first export at 363ms. The pending interval should reflect time since the second reveal, while elapsed play reflects time since the first. A later export with a quarter millisecond left still represents a pending mismatch in valid whole-millisecond snapshot data.

The paired unit protection is in `snapshot.test.ts` and `session/browser-clock.test.ts`. Fractions stay in active game data; serialization handles whole-millisecond output at the boundary. This avoids making display cadence part of the rules. [Clock design](clock-design.md) explains the implementation, and [Flaky-test notes](flaky-test-notes.md) explains the test instrument.

## A delayed timeout ate the next click

The player waited longer than the mismatch interval and clicked another card, but nothing happened. The scheduled timer had been delayed by other work, so the stale selection still appeared blocked when activation was handled.

The second fractional-clock browser case advances the controlled reading past the deadline and clicks a third card in the same task. No timeout can run between those two statements. The expected observation is one newly revealed card, proving that activation synchronizes active time before applying the card action.

This protection is stronger than waiting 900 real milliseconds and hoping a busy machine reproduces the delay. It isolates the ordering in `use-game.ts`: `clock.synchronize()` precedes `activateSession`. Changing either clock scheduling or action plumbing warrants rereading this case even if the ordinary 800ms journey passes.

## Hidden time was counted on return

One version of this failure paused display updates correctly but accumulated all hidden time when the next event sampled the clock. Frequent export checks could hide the bug by advancing the observation baseline during the pause.

`idle-clock.e2e.test.ts` deliberately avoids intermediate game reads. It allows 300ms before the second reveal, 200ms of the pending miss, then 5000ms hidden. On return, the pair stays open for its actual remaining interval. Final elapsed time includes active time only.

A second case imports a pending interval with 200ms remaining and waits 5000ms before Resume. It verifies that paused import time is excluded without relying on an export during the wait. `browser-environment.e2e.test.ts` adds the ordinary hidden-page path with explicit observations while hidden. Together these distinguish stopping callbacks from maintaining the correct active-time baseline.

## Restart left an older callback alive

After Restart during a miss, a newly revealed card could close when an earlier timeout arrived. The new game looked correct immediately, so a test that stopped after reset missed the damage.

`lifecycle.e2e.test.ts` checks the cleared state and later timer behavior. `session/session.test.ts` checks generation rejection directly. Replacing a game increments its generation; ticks carrying an earlier generation return the current session unchanged. The board is also keyed by generation, giving the replacement its first remembered tab stop.

`timer-lifecycle.e2e.test.ts` covers a different case: an earlier turn in the same deal. It completes one miss, starts another later, checks the new pair at 799ms and closes it at the next millisecond. A deal-generation check alone cannot prove correct behavior between turns inside that deal.

## Unmount removed the board but retained work

A page could look empty after removing Game while a timer and visibility listener remained active. That wastes work and can trigger updates against an abandoned session.

The session diagnostic build renders Game with an Unmount game button. `timer-lifecycle.e2e.test.ts` wraps timeout and visibility subscription APIs before loading the harness. It checks an active timer and listener after play starts, presses Unmount game, and expects both tracked counts to return to zero.

The production build has no unmount diagnostic button. Keeping the harness separate lets this test observe real effect cleanup without shipping a testing control. The art entry has a guard against reverting to a combined sheet-and-session branch.

## Snapshot export lagged behind the active clock

A status tick may occur every 40ms, but export can happen at any point between ticks. Serializing the last rendered state lost active time and lengthened a pending interval after import.

`clock-edges.e2e.test.ts` uses waits of 1035ms and 330ms, neither divisible by the render interval. It exports through the reproduction controls and checks exact elapsed and remaining values. This catches a serializer that reads React's last presentation state instead of synchronizing the current session.

The export path is intentionally observational from the player's perspective: it samples current time without resetting progress. It can resolve an interval that has actually expired. Tests using export to inspect an idle period need to account for that sampling effect rather than assuming export is an inert DOM read.

## A valid export could not be reloaded

Fractional active time near the end of a miss can round a pending interval to zero, which the version 1 parser correctly rejects. The apparent user failure is an exported snapshot that produces an invalid status when loaded immediately.

The snapshot unit suite checks export after 0.75ms of a miss and again with 0.25ms left. The result must decode successfully and remain within one millisecond of the active state. The browser fractional test exercises the same boundary through the UI. A full round trip from an ordinary integer time would not cover this edge.

Other parser regressions include duplicate matched positions, selecting a matched position, mismatches with equal faces and inconsistent move counts. `snapshot-selection.test.ts` and `snapshot.test.ts` cover those separately. The format validates state consistency, not a reconstructed history of every earlier reveal.

## An imported win followed the live completion path

A completed import could previously be confused with a newly played completion. The visible clues were a celebration on load or a saved record changing merely because progress was imported.

`snapshots.e2e.test.ts` checks the completed load's static presentation. `saved-bests.e2e.test.ts` compares completed imports with several saved move records. `session.test.ts` distinguishes replacement from activation-driven victory and also checks later completion after an unfinished import is resumed.

The distinction is causal. `replaceGame` sets up imported state, while `activateSession` can recognize a new winning turn. A render that sees all positions matched is not sufficient evidence that a local completion just occurred. [Snapshot selection notes](snapshot-selection-notes.md) and [Best-score records](best-score-records.md) provide the related model.

## Counter limits accidentally ended play

A defensive move cap can become a hidden stop condition if later actions return early at the maximum. Similarly, reaching the elapsed limit can block a pending miss forever if its remaining interval is tied to a saturated display value.

`move-cap.test.ts` checks normal matching, completion and export at 999999 moves. `elapsed-time.test.ts` checks continued game transitions at 359999999 elapsed milliseconds. Storage and session tests cover a capped move record and reject larger imported values. The cap bounds representation while the rest of play continues.

These tests construct boundary progress directly. Running the browser for an enormous number of turns would make the test slower without better isolation. Browser status-contract tests separately establish that valid imported endpoint states render and that minutes continue beyond an hour.

## The shuffle passed ordinary seeds but biased a boundary

Modulo arithmetic without rejection can produce the right published example and still implement the format incorrectly. A subtler error accepts a draw equal to the rejection limit when it should draw again.

`deck.test.ts` combines literal generator states, the seed-one deck and comparisons with an independent BigInt oracle at every size. `deck-rejection.test.ts` reads two independent stored vectors: one above the limit, one exactly on it.

The distinction between ordinary generation checks and rare-branch vectors is useful beyond this bug. Counting two of each face proves deck composition. It says nothing about the exact ordering required for a reproducible board code. A format change cannot be hidden behind a pair-count assertion.

## Board focus became a trap or forgot its position

A board containing 256 separately tabbable cards makes leaving the game laborious. Removing all but one tab stop can introduce another failure if that tab stop resets on every render or matched cards stop receiving focus.

`keyboard.e2e.test.ts` checks remembered entry, native Tab exit and focus on matched cards with inactive game actions. `play.e2e.test.ts` checks row and column edge stops. `board/keyboard.test.ts` verifies the destination calculation and leaves unsupported modifier combinations to the browser.

`focus-entry.e2e.test.ts` makes the remembered card lie outside the current scroll area, scrolls back to the start and enters with both Tab directions. It also checks Resume returning to a far remembered position. A test that calls `.focus()` directly on its final target cannot establish those entry behaviors.

## Scrolling during a pointer press lost activation

On a large board, pressing the visible sliver of a card can focus it. If focus handling immediately scrolls that card, the release lands somewhere else and the click disappears. The board can appear unresponsive only near clipped edges, making it easy to miss during full-card automation.

`pointer-focus.e2e.test.ts` uses coordinate presses after wheel, programmatic and clipped-edge scroll arrangements. It checks the intended card's revealed state and its final nearest-aligned position. Both mouse and touch variants cover the phone, desktop and equivalent-zoom layouts.

The current implementation avoids pointer-triggered nearest scrolling during focus and scrolls after a completed click instead. Keyboard focus still scrolls on focus. `focus-entry.e2e.test.ts` checks visible outlines after switching from pointer to each arrow direction, protecting the other half of that split.

## A focusable card had a clipped outline

Keeping the card rectangle in view is insufficient when the focus outline extends beyond it. This affected interior cards as well as the ends of the grid, particularly with nested scrolling.

`layout.e2e.test.ts`, `focus-entry.e2e.test.ts` and `zoom.e2e.test.ts` use geometry helpers that include outline width, outline offset and board client borders. They inspect window clearance as well as board clearance. `focus-bounds.ts` tolerates small subpixel rounding while still detecting visibly clipped edges.

The relevant specification values live in [Visuals](visuals.md) and [Input](input.md). The regression test's job is to arrange the difficult scroll state, not to invent a different outline or target size to make the assertion pass.

## Scrollbars changed the layout width

A board sized against the window can overflow when a traditional scrollbar consumes part of the document's usable width. Hidden-scrollbar browser defaults can conceal this defect.

`scrollbars.e2e.test.ts` launches the visible-scrollbar mode and first verifies that the page scrollbar occupies space. It tests 16x16 at 1280x800, 10x10 at 900x600, 12x12 at 1000x600 and 6x6 at 360x640. The assertions inspect page overflow, board overflow and card width. Other layout tests cover intentional scrolling on narrow large boards.

These cases prevent a tempting but incorrect repair: shrinking cards below the specified minimum to fit the outer page. Large boards preserve card geometry and scroll inside the board where needed. [Large-board rendering](large-board-rendering.md) covers the layout rationale.

## Repeated results disappeared from feedback

Two consecutive misses have identical words, but each is a new result. A rendering path based only on string equality can suppress the second event. The opposite failure repeats a result whenever the clock renders or a miss closes.

`session.test.ts` checks announcement progression. `announcements.e2e.test.ts` observes DOM mutations for actual result events, quiet clock/focus updates and repeated load outcomes. The complete accessibility journey also records the result sequence during a keyboard finish.

This is event evidence, not a recording of speech. The audit explicitly leaves spoken confirmation to acceptance.

## Distribution checks

The source audit, emitted-HTML scan and offline disk journey protect the boundary between development tools and the self-contained game. Their coverage is summarized in [Test strategy](test-strategy.md). [Release checklist](release-checklist.md) brings those results together with the gameplay regressions above.
