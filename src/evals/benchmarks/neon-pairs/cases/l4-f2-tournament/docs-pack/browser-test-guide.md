# Browser test guide
This guide explains the Node and Playwright harness used to exercise Neon Pairs through its built browser interface.

The suite opens generated HTML, not the Vite development page. It uses Node's test runner and Playwright's browser automation library. That combination explains several otherwise surprising details: tests import a local `test` wrapper, the package command controls file concurrency, and browser assertions have a locally configured timeout. Start with [Test strategy](test-strategy.md) for the reason each layer exists.

## The normal entry point

From a writable project checkout, the complete command is:

```sh
npm run test:e2e
```

In `package.json`, this expands to:

```sh
npm run build && npm run build:art && node --test --test-concurrency=1 "tests/*.e2e.test.ts"
```

`build` performs type checking before Vite emits `dist/index.html`. `build:art` emits `.art/index.html` and `.session/index.html` through separate configurations. A failure in either build stops the command before the test runner. This prevents accidentally testing an older distribution after a failed compile.

The commands write build outputs and browser artifacts, so they need a writable checkout with the reviewed JSON expectation files consumed by the tests; see [Known issues](known-issues.md). The README records a successful run.

During focused development, invoking Node for one browser file can shorten diagnosis after a current build exists. Such an invocation does not perform the build chain by itself and does not replace the full release command. A failure that disappears only when old output is used remains a failure of the source being reviewed.

## What the wrapper owns

`tests/browser-test.ts` imports `before`, `after` and `test` from `node:test`. Its file-level setup creates `.browser-work` and `test-results`, sets `TEMP` and `TMP` for browser work, obtains version information and launches headless Chromium. It then starts the private artifact server.

The exported `test` accepts a name, an async body, optional browser-context options, a scrollbar mode and a timeout. The body receives `browser`, `context`, `page` and `gameURL`, plus an artifact helper. Each case gets a new context and page. The context is registered for cleanup with the Node test context. The wrapper also collects uncaught page errors and expects the collection to be empty after the body completes.

A failed assertion can end the body before that final page-error assertion, so the final check is not a universal crash report. Browser automation errors and page errors are different sources of failure. Looking at the first meaningful failure usually gives a better diagnosis than reading only the trailing timeout.

The `after` hook closes the normal browser, any lazily created visible-scrollbar browser, and the server. Tests creating an extra context themselves close it explicitly, as the phone completion journey does in a `finally` block. This is practical resource ownership, not an assumption that process exit will always clean up successfully.

## Disk and HTTP URLs answer different questions

`GAME_URL` in `tests/game-fixture.ts` is a file URL for `dist/index.html`. `ART_URL` and `SESSION_URL` similarly address diagnostic builds. Disk loading is the delivery path: the game should remain playable without sibling assets or network requests.

`gameURL`, supplied by the wrapper, is the HTTP origin from `serveArtifact`. The server reads the emitted HTML, listens on `127.0.0.1` using an available port, serves `/`, and returns 404 for other paths. It does not serve source, fixture JSON or a diagnostic API. Saved-preference tests use this stable origin within their context so reload can observe the same local storage.

A persistence test that uses the disk URL can become dependent on browser-specific storage treatment. A delivery test that uses only HTTP can miss a file-loading problem. Existing tests deliberately use both paths. [Browser support](browser-support.md) describes the resulting compatibility claims; [Storage testing](storage-testing-guide.md) covers the persistence contract in more depth.

## Load a deterministic deal through the controls

`loadFixedBoard(page, url)` navigates, opens the native reproduction disclosure, fills Board code with `np1-4-1`, selects Load board, closes the disclosure and expects 16 cards. Its default URL is the built disk file. Tests that need HTTP pass the wrapper's `gameURL` explicitly.

The helper does not expose the application's state. Its pair positions come from `src/game/game.fixture.ts`, whose literal deck matches the published format example. This supports complete games while keeping hidden cards anonymous to the DOM. `play.e2e.test.ts` checks that closed cards have no SVG face and that diagnostic face attributes are absent before solving.

A compact behavior case follows the existing style:

```ts
import { expect, test } from './browser-test.ts';
import { loadFixedBoard, moves } from './game-fixture.ts';

test('a repeated first reveal leaves the move count at zero', async ({ page }) => {
  await loadFixedBoard(page);
  const card = page.locator('.card').nth(2);
  await card.click();
  await card.click();
  await expect(page.locator('.card.revealed')).toHaveCount(1);
  expect(await moves(page)).toBe(0);
});
```

The example uses the known deal, a native user action and observations available in the interface. The expected result does not call the implementation being tested.

## Selectors follow the observation

Control actions generally use accessible roles or labels with exact names. The size control is a combobox. Board code is a labeled input. Snapshot JSON is a labeled textarea. The disclosure uses a native summary element. These selectors exercise the same discoverability that a keyboard or assistive-software user needs.

`.card` and state classes are useful when inspecting card counts, position order and geometry. A positional locator is appropriate because the format defines row-order indices. It should not become a way to discover hidden identities. Card identity expectations come from the deterministic input, not an undocumented browser global.

Shared label constants keep ordinary journeys readable, but literal contract cases protect the actual wording. Changing a control's constant can otherwise change the test lookup and production label together. The dedicated contract files are deliberate independent evidence, rather than duplicate stylistic preferences.

## Time is a controlled input

`loadTimedBoard` installs Playwright's clock before navigation, loads the fixed deal and pauses at a later clock value. Time-sensitive journeys then use `page.clock.runFor` for exact active intervals. One test opens a miss, advances 200ms, hides the page, advances 2000ms while hidden, returns and checks the remaining 599ms plus 1ms boundary.

Headless Chromium keeps its tabs visible in this environment. The visibility tests therefore override `document.hidden` and dispatch `visibilitychange`, exercising the adapter input the game actually reads. Opening another tab is not a reliable substitute for this setup.

The fractional-clock file uses a narrower instrument: an init script replaces `performance.now` and a test-only event changes its reading. It can advance time and issue a click in the same browser task. That reproduces an overdue mismatch even if the scheduled callback has not run. The test proves synchronization before activation rather than measuring how quickly a timer fires on the host.

`exportedSnapshot` uses the visible export control and parses its textarea. Export synchronizes active time, so it can observe more than the most recent status render. This is useful for clock evidence, but it can also hide an idle-return bug if called during every wait. The idle-clock cases deliberately make no intermediate reads while hidden or paused. See [Flaky-test notes](flaky-test-notes.md) for the distinction.

## Focus needs a real journey

Calling `.focus()` is useful to establish a starting position. It does not prove that Tab can enter the board naturally. The keyboard and accessibility journeys include real Tab, Shift+Tab, arrow, Enter and Space events. Edge assertions check that arrows stop rather than wrap; Tab exit checks that the board does not trap focus.

The `moveKeyboardTo` helper reads only the currently focused card, computes row and column deltas and presses one arrow per step. It neither activates the destination nor reads its face. Matched cards remain reachable, and their disabled action state is checked separately from their focusability.

`focus-bounds.ts` measures card rectangles against the board viewport and window. It includes outline width and outline offset, along with viewport client borders, so a visible card with a clipped ring does not pass as fully visible. The quarter-pixel comparison allowance handles geometry rounding. It does not weaken card target-size assertions.

The pointer-focus cases deliberately click a sliver of a card near the board edge. They use mouse coordinates or touchscreen taps after arranging scroll position. A locator click that automatically scrolls the target fully into view would erase the condition under test. Wheel cases wait for changing scroll position and then a sequence of stable frames before clicking.

## Layout modes and diagnostic entries

Context options configure viewport, device scale and touch emulation. Visible-scrollbar mode removes Chromium's default `--hide-scrollbars` argument; its tests establish that scrollbars occupy space before inspecting the board. [Browser support](browser-support.md) lists the actual configurations.

The face sheet and unmount harness are separate builds, with their own disk URLs. They keep diagnostic controls out of the game distribution. [Fixtures guide](fixtures-guide.md) explains how those entries support artwork and lifecycle observations.

## Artifacts and runtime records

The wrapper turns a test name into an alphanumeric-and-hyphen directory under `test-results`. `artifacts.outputPath` accepts a basename and rejects a path containing directories. Existing cases write face-sheet PNGs and an accessibility journey JSON file through this helper.

The runtime record is `test-results/browser-runtime.json`. It contains Node, npm, Playwright and Chromium versions, plus whether named-combination enforcement was requested. `NEON_PAIRS_NAMED_COMBINATION=1` adds an exact comparison to the named tool set. `PLAYWRIGHT_CHROMIUM_EXECUTABLE` selects an alternate executable, but the recorded browser version still determines what was actually tested.

There is no automatic trace or screenshot-on-every-failure configuration in this wrapper. The presence of a screenshot from another case does not prove the failed case captured its final state. Artifact names and content are evidence created by their specific tests. [Accessibility audit](accessibility-audit.md) describes the normalized tree artifact and its limits.

For a release, use the complete build-and-run command and interpret failures with their environment and fixture availability. For a local diagnosis, preserve the failing action order and control only the browser input needed to reproduce it. Those two habits keep a fast test useful without turning an incomplete run into a compatibility claim.
