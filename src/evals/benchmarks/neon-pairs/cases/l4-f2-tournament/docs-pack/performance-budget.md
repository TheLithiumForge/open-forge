# Performance budget
This page describes the performance constraints already present in Neon Pairs and how to investigate first render and input work.

There is no numeric first-render, input-latency, frame-rate, bundle-byte or memory budget in the specification or test configuration. The current enforceable constraints are functional: the distribution is self-contained, every supported board renders, input remains usable in the specified layouts, and game time follows active elapsed time rather than callback frequency. Browser-test timeouts are automation allowances.

## What the current numbers mean

Several timing values occur in the source, but each has a specific job:

| Value | Owner | Meaning |
| --- | --- | --- |
| 40ms | `session/browser-clock.ts` | Ordinary scheduling interval for active clock updates |
| 800ms | `game.ts` | Active duration of a mismatching pair |
| 120ms | `board/card.css` | Border-color transition duration |
| 600ms | `status/status.css` | Single completion pulse duration |
| 15000ms | `tests/browser-test.ts` | Default browser assertion allowance |
| 30000ms | `tests/browser-test.ts` | Standard case timeout |
| 90000ms | `tests/browser-test.ts` | Longer case timeout for larger journeys |

The first four values describe game scheduling or presentation. The last three allow an automated verifier to complete on a busy machine. A test finishing in twenty seconds does not establish that a card responds within twenty seconds, and a 40ms clock update interval is not a promise about input latency.

Likewise, the caps on elapsed time and moves are data limits. They prevent unbounded counters and preserve valid formats. The snapshot and replay byte limits constrain accepted input, not the size of the shipped HTML. [Elapsed-time rules](elapsed-time-rules.md) and [Data exchange](data-exchange-policy.md) describe those separate contracts.

## First render: what is actually loaded

The production entry is `index.html`, which loads `src/main.tsx`. Vite builds it with React integration and `vite-plugin-singlefile`. `vite.config.ts` targets ES2023 and disables the module-preload helper. The emitted `dist/index.html` carries the game script, styles and inline artwork.

This removes external request dependencies from the play path. It does not remove JavaScript parse, React initialization, storage access, deck generation, DOM construction, style calculation or paint. A single file can still be expensive to initialize, so request count alone is incomplete evidence for first render.

The offline journey in `play.e2e.test.ts` opens the disk URL with the browser context offline. It expects the default 16-card board and a successful reveal, while the observed request list contains only the document navigation. `build-output.e2e.test.ts` separately checks the HTML for the `fetch(` and `relList` markers. These are delivery checks; neither records a first-paint timestamp.

The main build and diagnostic builds have different contents. `.art/index.html` renders all 128 face identities at once. `.session/index.html` includes the cleanup harness. Measuring either diagnostic page as the game startup cost would misrepresent the artifact users receive. The game distribution does not include those entry controls.

## Initial board size changes the workload

The default is a 4x4 board unless valid saved preferences select another supported size. A fresh HTTP context with no preferences and a warm context remembering 16x16 therefore have different first-render workloads. A report that omits storage state can compare unlike runs.

The largest board has 256 card buttons. All are rendered even if the board viewport shows only part of them. The layout uses internal scrolling to preserve card geometry; it does not virtualize rows or create cards lazily as the player scrolls.

Hidden cards mount the neutral back, not their face SVG. `Board.tsx` passes `null` for a hidden face and `Card.tsx` chooses the back markup. This serves the hidden-information contract and reduces initial artwork. Revealed and matched cards mount their SVGs, so a completed large board has a different rendering cost from the same board before any reveal.

The deck is generated from the code before it can be rendered. Generation is small and bounded by the supported sizes, but the documentation contains no measured duration for it. The independent generator checks establish correct ordering, not execution speed. Replacing generation for speed still has to preserve the version 1 deal.

## Input: follow the work rather than the click count

The card's native click handler calls the shared activation function and then performs nearest scrolling for the clicked element. `use-game.ts` synchronizes the active clock before applying `activateSession`. The pure action validates the index and current selection, then returns an unchanged or updated state.

That synchronous clock sampling is essential when a scheduled callback is late. An overdue mismatch must resolve before the next valid reveal is considered. Removing synchronization to shorten an input handler could make a trace look cheaper while reintroducing a lost-click defect. The fractional-clock browser case protects this ordering.

Keyboard arrows use a different path. `navigationTarget` computes a destination; the focus hook focuses it with browser scrolling suppressed and applies nearest alignment when appropriate. Focus movement does not reveal cards or add moves. A performance investigation can therefore distinguish an arrow-navigation cost from a reveal cost without changing the product behavior.

The clipped-pointer tests cover a particularly sensitive arrangement. Moving a card during the press can lose the release. The implementation waits for a completed click before pointer-related nearest scrolling. A change that eagerly scrolls on all focus events could reduce a later layout step but break activation at the board edge. [Regression catalogue](regression-catalogue.md) describes that failure.

## Work avoided on clock-only renders

`Board.tsx` memoizes the visible-position set from `matched` and `selection`, and the matched-position set from `matched`. A clock tick that changes neither input can reuse them. This avoids rebuilding the lookup collections solely because elapsed time changed.

`Card` is wrapped with React's `memo`. The board keeps a stable activation callback that invokes the latest handler through a ref. Focus and navigation callbacks come from the board focus hook. When a tick changes no card props, memoized cards can skip rendering even though the surrounding game and status update.

While a mismatch is pending, `advanceGame` creates a new selection with reduced remaining time. The set memo then has a changed dependency, although the two visible positions may be the same. Card props can still compare equal because they receive face, match and tab-stop values rather than the entire selection object.

A focus change updates tab indices for the remembered old and new card. A reveal changes the face prop of the newly exposed position. A match changes the two relevant matched flags. The parent maps the bounded deck, while memoization narrows child rendering. There is no global state library, worker-based board renderer or canvas layer in this implementation.

## Scheduling correctness under load

The session hook uses `performance.now` observations to calculate active deltas. The timeout is a request to sample again, not the source of elapsed truth. When a callback is delayed, the next sample accounts for elapsed active time instead of adding one fixed tick.

When a mismatch has less than an ordinary interval remaining, the scheduled delay is the smaller value. Visibility changes synchronize against the prior visible state, update visibility and reschedule. Imported pause and completion affect whether the clock is running. Cleanup clears work and removes the listener.

This design tolerates delayed presentation while preserving game data. It does not promise a particular paint cadence during host contention. The controlled-clock suite proves arithmetic and ordering, while a real browser trace is needed to diagnose long tasks or stuttering on a particular device. [Flaky-test notes](flaky-test-notes.md) separates those two investigations.

## Collecting a useful measurement

A first-render investigation can use the existing distribution and browser performance tools without adding product telemetry. Keep the artifact, actual browser version, origin, viewport and saved-size state with the observation. A clean default board and a context remembering 16x16 have different workloads; cold and warm loading also answer different questions.

Inspect navigation through the first usable controls and board. The trace can distinguish script evaluation, React initialization, synchronous preference access, layout and paint. An absent network waterfall or a screenshot after navigation supplies no startup duration by itself.

For input, compare first reveal, matching and mismatching second reveals, large-board navigation and session replacement. A completed 16x16 board has more SVG content than a hidden one. Keep actual coordinate presses for hit-testing complaints; a direct DOM click isolates event ordering but bypasses that geometry.

The source contains no measured median or tail timings for these paths. A before/after report needs comparable conditions before attributing a difference to code. The current structural choices explain costs without establishing a numerical latency claim.

## Release interpretation

The release process checks the complete supported workloads and offline delivery, then records the runtime actually used. No existing test enforces a millisecond first-render threshold or a byte ceiling for the HTML. A release report can therefore state that the functional checks passed on the recorded combination, while leaving measured responsiveness unspecified.

If an investigation finds a visible slowdown, preserve the reproduction and trace with its conditions, then use the relevant unit and browser regressions to verify the repair. [Known issues](known-issues.md) records the current measurement gap, and [Release checklist](release-checklist.md) keeps build correctness, compatibility and manual acceptance separate from performance claims.
