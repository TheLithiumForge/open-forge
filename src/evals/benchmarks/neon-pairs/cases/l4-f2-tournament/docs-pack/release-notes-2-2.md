# Neon Pairs 2.2: current release

These release notes describe the current Neon Pairs game at specification version 2.2, covering the work that brought its browser and core behavior into agreement.

The game implements specification version 2.2; the package version is `1.0.0`.

## The release in practical terms

Neon Pairs currently offers seven square board sizes, pointer and keyboard card activation, a remembered board tab stop, local best move counts by size, reproducible board codes, explicit snapshot export and import, and a developer replay command. The distribution is one offline HTML document. There is no account or service required for play.

The smaller release is about edges rather than another menu. A fractional remainder of a miss remains meaningful until it expires. An overdue interval is synchronized before another click. A restored unfinished board waits for Resume. Large boards and the equivalent zoom layout keep their controls readable and their focused card visible. These details make the same rules survive the conditions that a short demonstration rarely reaches.

## Time keeps its precision until export

`performance.now()` supplies the browser's monotonic observations. `elapsedStep` computes positive finite deltas while active, and live game state retains fractions of milliseconds. The 40ms clock interval is a scheduling cadence, not an instruction to add exactly 40ms on each callback.

This distinction matters when the browser is busy. A callback that arrives late can observe the full elapsed delta. A card activation synchronizes the clock first, so a miss that has already expired cannot remain an input barrier simply because its scheduled timer has not run yet. `tests/fractional-clock.e2e.test.ts` exercises both a fractional interval and an overdue mismatch followed by a click in the same task.

At the snapshot boundary, elapsed time is rounded down and pending mismatch time is rounded up. A final quarter millisecond therefore exports as one remaining millisecond. This keeps the text format integral without making live state expire early. The [clock incident](incident-clock-drift.md) gives a failure analysis around these choices.

Elapsed time is capped at 359999999ms, displayed at the limit as 5999:59. Moves are capped at 999999. Reaching either cap does not end the game or prevent later matches. The elapsed cap also does not stop a pending mismatch's remaining interval from advancing: `advanceGame` clamps the accumulated total while still subtracting the supplied delta from the selection's remaining time.

## Pause and completion have explicit boundaries

The page's hidden interval is excluded from active time. An unfinished snapshot also introduces a pause, with card activation blocked and focus movement available. Resume establishes a fresh clock observation and hands focus back to the remembered board card before its button is removed.

A completed snapshot follows a different route. It appears completed, has no Resume button, and shows a static result. It does not pass through the winning-action record update. Completing unfinished imported progress through actual play can update the local best, because that path reaches `activateSession` with a newly won state.

These distinctions are covered by session unit tests and browser snapshot journeys in the repository. They are also useful when describing a bug. “Loading finished progress” and “finding the last pair after Resume” may produce the same matched positions but have different session effects. The [snapshot guide](snapshot-format.md) describes the data; [session module](session-module.md) describes its browser meaning.

## Navigation reaches the visible result of a move

The board still renders all card buttons, including matched ones. Only the remembered position participates in the page's tab order, and ordinary arrows move within row and column bounds. Matched cards have an unavailable action while retaining their place in the geometry.

Focus tests cover entering with Tab and Shift+Tab, returning from Resume, and changing from a pointer click to arrow navigation. Nearest alignment matters especially when the remembered card is far from the current scroll window. The current helper remembers the board's scroll position, restores that basis, and then brings the card into view, counteracting browser centering during native Tab entry.

A visible card is not enough if its outline is clipped. Current card CSS gives a 6px scroll margin, while the shared focus style uses a 3px white outline offset 2px from the target. Browser checks inspect the bounds around first, last, and interior positions rather than treating focus as merely an active-element assertion.

## Zoom and scrolling have concrete evidence

The specification describes the equivalent layout of 200% browser zoom from 1280x800 as a 640x400 CSS-pixel viewport with device scale 2. The game's zoom journey uses that configuration and exercises a completed small board, snapshot loads, and a large board with keyboard movement to the last card.

That evidence is more specific than a general claim that zoom works everywhere. It checks readable controls, status, and summaries without overlap, page width within the viewport, and a focused large-board card inside the scroll area. The game leaves zoom enabled. The [zoom behaviour page](zoom-behaviour.md) explains the present layout, and [phone research](research-phone-usage.md) gives the observational context.

The large-board contract also names 360x800 and 1280x800 CSS-pixel views. All 256 positions remain present at the largest size, while overflow belongs to the board. Native controls remain reachable outside that internal scrolling surface.

## What the repository records about tools

The README records a named combination of Node 26.12.1, npm 11.20.0, Playwright 1.64.1, and Chromium 156.0.8078.4. It says the locked install, type checking, Vitest unit tests, build, and Chromium journeys passed on that combination with the named-combination flag set. Other supported runtimes need their own result records.

The browser command builds current game source and diagnostic entries before starting its Node test runner. The diagnostic face sheet and session harness are separate from the shipped game. A stale `dist` is not the intended fallback if the current build fails. `test-results/browser-runtime.json` is the named runtime record described by the README.

The runtime packages are React and React DOM 19.3.1. The checked-in tool configuration includes TypeScript 7.0.2, Vite 8.3.5, and Vitest 5.0.5. [Technology](technology.md) and [build pipeline](build-pipeline.md) cover their roles.

## Accessibility evidence has a stated limit

The README records an accessibility-tree journey with Chromium 156.0.8078.4 and Playwright 1.64.1. It explicitly records that a screen reader was not run. Spoken-output confirmation remains a user acceptance activity.

The journey covers control names, hidden and visible card information, turn results, final moves and time, leaving the board, and starting again. Consecutive identical outcomes are distinct session events; timer renders do not republish them. The [screen-reader research page](research-screen-reader-sessions.md) explains how to interpret that evidence alongside the study findings.

Version 2.2 retains version 1 board codes, snapshots, and replays. The current outcome is a small, inspectable game whose behavior is described at the same boundaries the code uses: pure transitions, browser session effects, presentation, and explicit data exchange. [Known issues](known-issues.md) and [roadmap notes](roadmap-notes.md) address unresolved questions without expanding this release's commitments.
