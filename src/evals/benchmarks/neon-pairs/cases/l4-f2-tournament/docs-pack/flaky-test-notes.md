# Flaky test notes
This page explains how Neon Pairs tests control time, scrolling and browser effects when host scheduling would otherwise obscure a failure.

An intermittent failure is a symptom to investigate. The suite has explicit timeouts and several deterministic instruments, but no configured automatic retry policy. The mechanisms below distinguish timing defects through controlled inputs and observations.

## Three clocks appear in a test report

Game time is active elapsed time. Browser automation time is the time Playwright allows an assertion or action to become true. Runner time is the time Node or Vitest allows the whole case to finish. Those clocks can move independently.

The game samples `performance.now` and computes positive active deltas. Ordinary updates are scheduled at 40ms, or sooner when a pending miss has less time left. A mismatch lasts 800 active milliseconds; elapsed time starts at the first valid reveal and stops at completion. Hidden and imported-pause periods are excluded.

The browser wrapper configures assertions for 15000ms and cases for 30000ms or 90000ms. Replay CLI tests have a 60000ms allowance because child processes can be slow on a busy verifier. These are harness allowances. Increasing one does not change the 800ms game interval or establish acceptable player-visible latency.

When a failure says an assertion timed out, first identify which fact never became true. A missing fixture, failed navigation, lost click and legitimately slow page can all end with a timeout. The message alone does not identify scheduling as the cause.

## Install the controlled clock before the game starts

`loadTimedBoard` installs Playwright's clock before navigation, loads the fixed deal and pauses at a controlled baseline. Cases then use `page.clock.runFor` to advance timer callbacks and active readings predictably. Installing the clock after the game has already created its clock state would mix observations from different time domains.

The useful boundary pattern is the interval just before an event, followed by the exact final step. Tests check a pending pair at 799ms and closure after another millisecond. When 200ms has elapsed before hiding, the return path checks 599ms and then 1ms of remaining active time.

A real sleep can overshoot either boundary under host contention. It also leaves ambiguity about how much time elapsed between a click, an assertion and a second click. Controlled time makes the intended timeline visible in the test, while Playwright's retrying assertions allow React's resulting DOM work to settle.

The controlled clock is not a performance benchmark. A case can execute 5000ms of logical waiting quickly or take a long host time to complete its automation. [Performance budget](performance-budget.md) keeps those measurements separate.

## A delayed callback needs a different instrument

Playwright time advancement normally runs scheduled work. To reproduce a mismatch whose deadline has passed before its callback runs, `fractional-clock.e2e.test.ts` replaces `performance.now` with a controlled value using an init script.

The test changes that value through a page event. For the overdue case, it dispatches the new reading and clicks a card within the same browser task. A timeout cannot intervene between those statements. The expected new reveal establishes that the activation path synchronizes the clock before checking selection eligibility.

The fractional case uses quarter-millisecond values because they are exactly representable in binary. The first reveal is at 351.25ms, the second at 362.25ms and export at 363ms. The expected remaining interval is measured from the second reveal, not from the first reveal or most recent render.

Near closure, a quarter millisecond remains active. Snapshot export represents a pending interval with a positive integer, and the test allows less than a millisecond of serialization difference. That comparison is a format-rounding check, not tolerance for closing the in-memory mismatch early.

This instrument is installed only in the test page. It does not add a clock setter to shipped source or provide hidden game state to the browser solver.

## Visibility is an adapter input

Headless Chromium leaves its tabs visible in the environment used by these tests. Opening a second tab is therefore not a dependable way to pause the game. The visibility cases override `document.hidden` and dispatch `visibilitychange`, matching the input the session hook reads.

The hook synchronizes before switching its stored visibility flag. That accounts for time up to hiding under the earlier visible state and excludes time before returning under the earlier hidden state. Reordering those operations can charge the wrong interval even if timers appear to stop correctly.

`browser-environment.e2e.test.ts` observes a miss while hidden and checks that play resumes with the correct remainder. `idle-clock.e2e.test.ts` adds a stronger arrangement: no intermediate game read or export during the hidden wait. It catches a stale baseline that only becomes apparent on return.

The analogous imported-pause case waits 5000ms without inspecting game data, selects Resume and checks the pending interval. If a diagnostic export occurred repeatedly during that wait, it could accidentally maintain the observation baseline and conceal the original defect.

## Export is an observation with synchronization

`exportedSnapshot` drives the visible export button. `use-game.ts` synchronizes the clock before encoding current progress. It is therefore more precise than reading the last displayed seconds, but it is not the same as inspecting a passive attribute.

`clock-edges.e2e.test.ts` chooses 1035ms and 330ms waits, both between ordinary render ticks. Export must include all active time and the correct pending interval. That test intentionally relies on synchronization. The idle-return tests intentionally avoid it until after the critical wait.

When reducing a failing timing test, preserve its observation pattern. Removing an export may expose a baseline bug; adding one may hide it. Both versions can look like harmless cleanup if the helper's active-clock behavior is overlooked.

## Separate stale deals from stale turns

A replacement increments session generation. `tickSession` rejects a tick with an older generation, and the effect also checks its captured generation before doing work. Cleanup marks the effect inactive, cancels its timer and removes the visibility subscription.

The lifecycle suite tests Restart during a pending miss and later input in the replacement. The pure session test isolates generation rejection. These protect against earlier deals changing the current state.

An older turn inside the same deal is a different case. `timer-lifecycle.e2e.test.ts` finishes one miss, advances another 100ms, opens a second miss and checks its own 799ms plus 1ms boundary. Both turns have the same generation, so a generation-only assertion would not establish correct behavior.

The unmount harness tests actual resource cleanup by tracking timeouts and visibility listeners. Its instrumentation is installed before navigation. The expected empty tracker after Unmount game is stronger evidence than merely observing that the board disappeared.

## Scroll settling follows geometry

Wheel and touch movement can continue after the input command returns. Clicking immediately can hit a different card from the one the test intended. A fixed sleep might work on one machine and fail on another.

`pointer-focus.e2e.test.ts` first waits for wheel position to change, then waits for ten stable animation frames. The touch-scroll cases similarly wait for a sequence of stable scroll positions after their gestures. The comments explain why a single unchanged frame is insufficient on a busy verifier.

The clipped-edge cases also arrange the target deliberately. They find a card crossing the board's lower edge and press a point 4px inside that edge. When no card crosses initially, a half-card scroll creates the condition. This is a test arrangement for a real hit-testing problem, rather than dependence on whatever fractional position a previous action happened to leave.

Locator clicks automatically help bring elements into view. That is useful for ordinary completion but can erase a clipped-target reproduction. Coordinate mouse clicks and touchscreen taps are used where the edge condition itself is the subject of the test.

## Style transitions can outlive an action

Card borders transition over 120ms. A computed-style comparison made while a hover border is fading can differ from the same card's settled appearance. `art.e2e.test.ts` waits for the card's animations to finish before comparing its non-placement styles with the face-sheet card.

Other style tests use retrying `toHaveCSS` assertions or advance the controlled clock through the transition. Reduced-motion tests explicitly emulate the preference before loading. Without that setup, a host preference could change which animation behavior the test observes.

The completion pulse is a single 600ms animation. Motion tests inspect duration and iteration count; reduced-motion cases inspect its absence. Completed imports use a static completion path. A screenshot taken at an arbitrary point in a live pulse could capture temporary opacity rather than the stable appearance, so the reason for the capture timing matters.

## Geometry tolerances are local

`expectFocusInside` includes the outline's width and offset and allows a quarter-pixel rounding margin. Pointer-settling checks account for whole-pixel scroll offsets around fractional card dimensions. Readability checks use text rectangles and a small overlap comparison threshold.

These allowances address the measured representation of geometry. They do not permit smaller card targets, missing focus clearance or outer-page overflow. Moving a tolerance from one helper to another without its original reason can turn a real defect into a pass.

The visible-scrollbar suite has an explicit precondition that the scrollbar occupies layout space. A missing scrollbar is not “close enough” to that arrangement. The correct diagnosis is an environment or launch-mode mismatch before interpreting the board-width assertion.

## Keep the first failure interpretable

Fresh contexts isolate storage and instrumentation, while the runtime record identifies the actual tools. Missing fixture inputs and failed builds are deterministic setup problems; [Known issues](known-issues.md) covers verification limits. [Browser support](browser-support.md) explains version enforcement and origin distinctions.

Preserve the first failing action and its observation conditions during a focused rerun. The useful repair controls the uncertain browser input or fixes the underlying behavior, then the complete command verifies the current build. An eventual unexplained pass does not identify the cause.
