# Test strategy
This page explains how Neon Pairs divides evidence between game tests, browser journeys, source checks and user acceptance.

The product contract is specification version 2.2. The README records a successful named compatibility run. [Known issues](known-issues.md) tracks verification limits, and [Fixtures](fixtures-guide.md) explains why replacing reviewed JSON expectations with current output would weaken the evidence.

## Start with the question

A deck test asks whether a code produces the published cards. A browser test asks whether a player can reach and activate those cards through rendered controls. An accessibility-tree capture asks what Chromium exposes to assistive software. None can substitute for the others.

The useful division follows the architecture's existing boundaries:

| Question | Existing evidence | What it cannot establish |
| --- | --- | --- |
| Does a turn change the right data? | Vitest tests beside `game.ts` | Native focus, scrolling or pointer dispatch |
| Does the seeded deal match the format? | Literal vectors and `tests/deal-oracle.ts` | Whether hidden identities leak into markup |
| Does the browser adapter count active time? | Pure clock tests plus controlled browser clocks | Real device scheduling latency |
| Can a player finish through the interface? | Built-page keyboard and touch journeys | Spoken screen-reader output |
| Does local storage survive reload? | HTTP-origin persistence journeys | Cross-browser behavior of disk-origin storage |
| Does the distribution stand alone? | Offline disk navigation and built-output inspection | Every browser engine or operating system |
| Does the layout survive difficult geometry? | Large-board, scrollbar, zoom and focus tests | Every possible window size |

This is a map of current checks, not a demand to create one test of each kind for every change. A correction to snapshot validation usually needs parser evidence first. A clipped focus ring needs browser geometry. A change to explanatory copy may need only the relevant reviewed expectation and a visual check. [Unit tests](unit-test-guide.md) and [Browser tests](browser-test-guide.md) show the local conventions.

## Pure behavior carries the broad matrix

`vitest.config.ts` includes `src/**/*.test.ts` and `scripts/**/*.test.ts`. The package's `test` script is `vitest run`. Most of these tests import `test` from Vitest and assertions from `node:assert/strict`. They do not mount React to prove a card transition.

The core state includes the immutable board description, its deck, matched positions, selection, moves, elapsed milliseconds and the started flag. Selection has closed, one-card and mismatch branches. Tests exercise transitions directly, allowing exact questions such as whether advancing a pending miss by 799ms leaves it open and whether the next millisecond closes both cards.

The suite checks complete pairs at all seven supported sizes. `completion.test.ts` finishes each size through `activateCard`; it does not infer that a 4x4 success proves a 16x16 finish. `deck.test.ts` checks the published seed-one vector, generator states, the inclusive maximum seed, frozen data and independent generation at each size. `deck-rejection.test.ts` targets uncommon rejection-sampling boundaries using separately stored expected decks.

The game tests also make negative cases visible. A fractional or out-of-range card index returns the old state. Repeating the first selected card adds no move. During a miss, every activation is ignored. Invalid elapsed steps leave the game unchanged. Keeping the original object in those cases is observable behavior in these tests; asserting identity can distinguish an ignored action from an unnecessary reconstruction.

Snapshot and replay tests carry a different matrix: exact object fields, integer ranges, byte limits, valid selections and action ordering. A round trip is useful, but two matching mistakes can round trip successfully. Independent expected fields and malformed inputs provide the stronger evidence. A valid started game with one revealed card and zero moves is the baseline for several invalid-counter cases, so rejection can be attributed to the changed counter rather than an unrelated invalid state.

## Session tests connect results to browser-independent policy

`src/game/session/session.ts` is a data adapter. Its tests create sessions and call `activateSession`, `replaceGame` and `tickSession` directly. This makes several integration rules cheap to check without pretending to test browser effects.

A replacement increments `generation`. A tick carrying an earlier generation has no effect. An unfinished imported game is paused and cannot accept card input or active time until resumed. A locally completed game can improve the appropriate saved move record. A completed import follows the import path instead of the live completion path. These are separate causes even when both render a completed board.

Session feedback tests inspect event progression as well as text. Two misses with the same wording still represent two turns. Focus movement and time advancement do not represent new turn results. Browser announcement tests then verify that these data changes become the intended DOM updates. Checking both layers helps distinguish a missing session event from a rendering problem.

Storage tests use the narrow `PreferenceStore` interface. A Map-backed implementation covers round trips and per-size minima; a throwing implementation covers failures without a browser. Browser tests separately exercise a denied `window.localStorage` getter. The distinction matters because failure while obtaining the store occurs before either `getItem` or `setItem` can run.

## Browser journeys use the shipped shape

The package's `test:e2e` command runs the main build, builds diagnostic entries, and then invokes Node's test runner over `tests/*.e2e.test.ts` with concurrency set to one. Playwright drives Chromium, while `node:test` owns the tests. There is no Playwright Test project configuration controlling this suite's retry or worker behavior.

Most journey files import the wrapper in `tests/browser-test.ts`. It launches a browser, creates a fresh context per case, collects page errors and closes resources. `tests/artifact-server.ts` serves the freshly built HTML on a private loopback port. `GAME_URL` points at the same build on disk. Persistence journeys use the served URL; offline-delivery checks use the disk URL.

Fresh contexts isolate browser storage, permissions and page-installed instrumentation. They do not make every global resource independent: the artifact server and browser belong to file-level hooks, and the runtime record has a shared output location. The serial command is part of the implementation being documented. [Flaky-test notes](flaky-test-notes.md) discuss the timing consequences.

The browser tests use public controls to load `np1-4-1`. They know its deck from the documented format and test fixture, rather than asking the application for hidden state. The board's hidden buttons expose positions and closed state. The tests inspect that absence directly in `input-contract.e2e.test.ts`, including the neutral back and lack of face artwork. A solver that depended on hidden SVGs would invalidate this evidence.

## Contract checks complement shared labels

Ordinary journey helpers import the control-label constants so they can focus on behavior. Contract tests also contain literal labels and limits. This prevents changing a production constant and every test lookup together from silently changing the public contract.

`control-contract.e2e.test.ts` covers native reproduction controls, their input limits, invalid statuses and action focus. `product-contract.e2e.test.ts` covers status presentation, continuing minutes and successful load size selection. `persistence-contract.e2e.test.ts` checks record comparisons, saved sizes and fresh progress after reload. These overlap broader journeys at specific seams, where a shared helper could otherwise conceal a mistake.

Overlap is useful when the assertions answer different questions. Loading a snapshot appears in parser tests, session tests and browser tests because validation, paused data and Resume focus are different behaviors. Repeating the same serializer implementation as an expected-value helper would add little protection. The suite's independent literal expectations are intended to break that circularity.

## Visual and accessibility evidence are concrete

The art entry renders every face at the minimum card size. Tests capture color, grayscale and neutral versions. Neutral silhouettes are compared for uniqueness, and separate assertions inspect marker geometry and palette application. This establishes machine-distinguishable geometry; a person still reviews whether the distinctions are usable. [Accessibility audit](accessibility-audit.md) explains that limit.

Layout tests inspect page width, card bounds, board scrolling and complete focus outlines. The large-board checks cover 360x800 and 1280x800 CSS-pixel viewports. A separate equivalent-zoom journey uses 640x400 with device scale 2. Visible-scrollbar cases remove Chromium's default scrollbar-hiding launch argument and test widths where scrollbar space changes the available board area.

The accessibility journey uses the Chromium DevTools Protocol to capture the accessibility tree during a complete keyboard game. It checks controls, revealed and matched names, disabled state exposure, results, completion, Tab exit and a fresh game. The artifact records browser and Playwright versions and explicitly records that a screen reader was not run. User acceptance owns the spoken journey. Tree assertions support that review; they do not prove speech delivery.

## Source and delivery checks

`source-audit.test.ts` inspects package pins, lockfile agreement, compiler settings and source conventions required by the architecture and technology specifications. It reads authored files rather than the compiled bundle. `source-conventions.test.ts` checks current export ownership, question-shaped predicate names, behavior-oriented test names and the small set of environment reads the runners use.

These are text checks with intentionally limited reach. The core effect scan names particular source files. It is not an analysis of every transitive call. The TypeScript source scan can reject spelling patterns, but cannot prove full semantic purity. Review remains necessary when moving behavior to a new module. [Code review](code-review-guide.md) and [Source audit notes](source-audit-notes.md) provide related architectural context.

The built-output test searches the emitted HTML for `fetch(` and the preload helper's `relList` marker. The offline journey blocks network access and checks that only the document request occurs while a card remains playable. Together they cover bundle content and observed loading. Neither measures first-render duration; [Performance budget](performance-budget.md) documents that evidence gap.

## Following through

Choose the smallest test that exposes the changed behavior, then follow it into the browser where native interaction or effects matter. [Regression catalogue](regression-catalogue.md) shows paired investigations, and [Release checklist](release-checklist.md) assembles the complete verification sequence. The remaining evidence gaps are tracked in [Known issues](known-issues.md).
