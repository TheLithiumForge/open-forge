# Release checklist
This checklist assembles the existing build, test and acceptance steps for a Neon Pairs release.

It follows specification version 2.2 and the commands in the package. [Known issues](known-issues.md) records verification limits.

## Establish the candidate

- Identify the source being released and use a complete writable checkout for commands that install, build or write test artifacts.
- Read the current specification and [Acceptance](acceptance.md), especially the journey affected by the change.
- Check that reviewed test inputs are present. The suite reads nine reviewed JSON files; the inventory is in [Fixtures guide](fixtures-guide.md).
- Inspect the package and lockfile together. The project uses exact dependency versions and separate TypeScript compiler projects.

The package's version is currently `1.0.0`, while the README says it implements product specification 2.2. Those numbers describe different things. Board codes, snapshots and replays remain at format version 1. Do not infer a format migration from the product specification number alone; [Format versioning](format-versioning-guide.md) explains that distinction.

The release candidate is the emitted game HTML built from the selected source. A diagnostic sheet or a previously built file is not evidence for that candidate. The normal browser command performs a fresh build specifically to keep source and tested artifact connected.

## Install and identify the tools

Use the locked install:

```sh
npm ci
```

The technology specification supports Node 24.19 or later. The named compatibility combination is Node 26.12.1, npm 11.20.0, Playwright 1.64.1 and Chromium 156.0.8078.4. The README contains a prior successful record for that exact combination; other versions need their own results.

The README's PowerShell setup puts the npm cache, downloaded browser and browser temporary work inside the checkout. It installs Chromium through Playwright. The environment variables and browser installation are runner setup, not runtime requirements for opening the delivered game.

`PLAYWRIGHT_CHROMIUM_EXECUTABLE` can select another executable. That override does not make its version equal to the named Chromium build. Read the generated runtime record to establish what launched. [Browser support](browser-support.md) describes the compatibility claim that each result can support.

## Run the source checks

```sh
npm run typecheck
npm test
npm run format:check
```

The typecheck command invokes the app, Node and e2e compiler projects in sequence. A successful Vite build alone would not establish TypeScript correctness. Vitest selects the tests under `src` and `scripts`, including pure rules, parsers, session behavior, command-line replay and source audits.

Read failures by category. A missing reviewed fixture is not an assertion about game behavior. A source-pattern assertion is not a browser layout failure. A command-line test can fail because stdout contains extra text even if the final state calculation is correct. [Unit test guide](unit-test-guide.md) explains these seams.

Formatting has a check mode and a write mode. The release check uses `format:check`; `format` rewrites files. If a correction changes the candidate, the affected verification applies to that corrected source, rather than to the earlier version whose checks ran first.

## Build and run the browser journeys

For named-combination evidence in a POSIX shell:

```sh
NEON_PAIRS_NAMED_COMBINATION=1 npm run test:e2e
```

The README gives the equivalent PowerShell assignment before `npm run test:e2e`. Without the flag, the runner still writes its versions, but does not require equality with the named set.

The command typechecks and builds the game, builds the art and session entries, then runs browser files serially through Node. The `&&` chain prevents a failed build from falling through to older output. The tests use fresh browser contexts and close browsers and the local server through their hooks.

Review the result in context:

- `test-results/browser-runtime.json` identifies Node, npm, Playwright and Chromium and records whether named enforcement was requested.
- Per-test directories contain only the artifacts their tests explicitly write. The face-sheet cases write PNGs; the accessibility journey writes a normalized JSON capture.
- The wrapper checks uncaught page errors after successful test bodies. A passing visible assertion should not conceal a page exception reported later by that check.
- The diagnostic `.art` and `.session` outputs are test inputs. The distributable remains `dist/index.html`.

There is no configured automatic browser retry policy or trace-on-every-failure mechanism in the local wrapper. A rerun can assist diagnosis, but an unexplained intermittent failure remains unexplained. [Flaky-test notes](flaky-test-notes.md) describes the existing deterministic controls.

## Review the delivery path

The offline browser case opens the built file while network access is blocked, checks the card count and reveals a card. Its observed request list contains only the document. A separate emitted-HTML check looks for the unwanted fetch and module-preload markers.

Review those results together with the distribution specification. Inline script, styles and SVG artwork are part of the delivered file. External fonts, services or sibling asset files are not needed for play. The developer CLI and diagnostic builds are separate from that distribution.

Saved-preference evidence uses the local HTTP origin. Reload tests check the remembered size and per-size move records, while progress starts afresh. Browser-specific storage on a disk URL is not used as the persistence compatibility result. Denied storage and corrupt saved data have their own cases, preserving play when persistence is unavailable.

## Follow the acceptance journeys

[Acceptance](acceptance.md) defines the journeys. The most relevant evidence groups are:

| Journey | Existing test and review evidence |
| --- | --- |
| Finish and reset | Pure completion at every size, browser keyboard/tap completion, restart during a pending miss |
| Active time | Hidden-page, idle-pause, fractional-clock and between-tick export cases |
| Reproduction | Literal board, snapshot validation and round trips, replay ordering and CLI output |
| Focus and navigation | Edge stops, remembered entry, Resume handoff, Tab exit and clipped-pointer cases |
| Difficult layouts | Largest board, visible scrollbars, equivalent zoom and readable text checks |
| Appearance | Face sheets, computed styles, matched-mark clearance and reduced motion |
| Local records | Better/equal/worse completion, saved size, reload and denied-store cases |

A complete 4x4 keyboard game also has an accessibility-tree journey. Its record names Chromium and Playwright and states whether a screen reader was used. The README currently records no screen-reader run. Spoken output and real-device long-press behavior remain user acceptance tasks under the specification.

Visual review of the 44px grayscale sheet complements automated silhouette uniqueness. The audit records what those images can establish and what still needs human judgment. See [Accessibility audit](accessibility-audit.md) before turning automated geometry into a broader accessibility claim.

## Read the limits before writing the result

The suite has no quantitative startup or input benchmark. A passing browser journey establishes behavior under its conditions, not a measured latency budget. The supported runtime floor is also different from the one exact compatibility combination recorded as tested.

The README records a successful run for its named combination. A release note should identify the candidate, executed checks and remaining acceptance evidence for the release.

Finish by stopping any development servers or watchers started for the work. The browser harness owns its own server cleanup; a separately launched Vite server is outside those hooks. Preserve the concrete test result and runtime information used for the release record. [Changelog style](changelog-style.md) and [Browser support](browser-support.md) help describe the resulting evidence without overstating it.
