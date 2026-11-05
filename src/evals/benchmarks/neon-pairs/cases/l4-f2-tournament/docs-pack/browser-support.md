# Browser support
This page explains which Neon Pairs browser and tool combinations have recorded evidence and which conditions are represented by the current test suite.

The README records Node 26.12.1, npm 11.20.0, Playwright 1.64.1 and Chromium 156.0.8078.4 as the named tested combination. It says the locked install, type check, Vitest tests, current-source build and Chromium journeys passed with named-combination enforcement enabled.

## Tested, supported and unrecorded

The technology specification supports Node 24.19 or later. Its named compatibility combination supplies a concrete verification point within that range. A version meeting the supported floor is not automatically a version for which this repository has a recorded successful run.

| Environment | Evidence in the project | Interpretation |
| --- | --- | --- |
| Named Node/npm/Playwright/Chromium set | Explicit README pass record and exact runner comparison | Recorded tested combination |
| Other Node versions at or above 24.19 | Allowed by technology specification | Need their own run result |
| Other Chromium executables | Runner accepts an executable override | Actual version is recorded; success is not assumed |
| Firefox | No launch path or project in this suite | No recorded result here |
| WebKit or Safari | No launch path or project in this suite | No recorded result here |
| Mobile Chromium emulation | Touch and viewport contexts in browser tests | Emulated interaction coverage |
| Physical phones or tablets | No device run recorded | Physical-device compatibility unverified |
| Screen reader with a browser | README records no screen-reader run | Spoken journey remains user acceptance |

The package version `1.0.0` and specification version 2.2 are not browser support versions. The build targets ES2023 through Vite. That target describes emitted syntax; it does not replace tests of native focus, scrolling, storage or accessibility behavior in a browser engine.

## How the runner establishes identity

`tests/browser-test.ts` reads the running Node version, obtains npm's version from its script environment or a fallback command, reads the installed Playwright package version and asks the launched browser for its version. It writes the result to `test-results/browser-runtime.json`.

With `NEON_PAIRS_NAMED_COMBINATION=1`, the setup compares those values with the named set. A mismatch fails the run. Without the flag, versions are still recorded but are not required to equal the named set. A successful unflagged run can supply evidence for its actual combination; it cannot be relabeled as a named-combination pass if the values differ.

`PLAYWRIGHT_CHROMIUM_EXECUTABLE` changes the executable path passed to `chromium.launch`. It is useful for investigating another Chromium build. The runner still records that browser's reported version and, when the named flag is set, compares it normally. Setting the override does not suppress the compatibility check.

The README's install example uses PowerShell and records project-local browser and temporary directories. That is a reproducible setup example, not evidence that every test ran on every operating system with PowerShell available. The runtime JSON does not include an operating-system field, so the four version values alone cannot establish a broader OS matrix.

## Browsers are driven by Node tests

The suite uses Node's test runner with Playwright as an automation library. It does not define Firefox and WebKit projects in a Playwright Test configuration. The package command runs matching browser files serially after building both the distribution and diagnostic entries.

The wrapper creates a fresh browser context for each ordinary case. This isolates storage and page instrumentation between cases. A shared browser instance still supplies the engine being tested. Starting a new context does not create a different browser version or simulate a separate operating system.

Some cases use the Chromium DevTools Protocol directly. The accessibility helper requests the full accessibility tree, and touch-scroll cases dispatch touch events through a CDP session. These tests cannot be assumed to run unchanged against another engine. A future compatibility result for another engine would need to distinguish portable journeys from Chromium-specific evidence.

## Viewport and input coverage

The suite represents several concrete layouts rather than a continuous device catalogue:

| Configuration | Main coverage |
| --- | --- |
| 360x800 CSS pixels | Phone layout, large-board navigation, taps and touch scrolling |
| 1280x800 CSS pixels | Desktop large-board layout and touch-scroll counterpart |
| 640x400, device scale 2 | Equivalent layout for the specified zoom scenario |
| 320x640 | Default-board fit and main control/card dimensions |
| 1280x1400 | Large-board viewport-height cap |
| 900x600, 1000x600 and 360x640 | Intermediate boards beside visible scrollbars |

The art sheet uses a separate 1200x900 viewport for captures. That page is diagnostic artwork evidence, not an additional phone or desktop support tier. Pointer-focus cases reuse the phone, desktop and equivalent-zoom dimensions while arranging clipped targets near a board edge.

Touch contexts set `hasTouch` and `isMobile`; they exercise taps and scrolling with Chromium's emulation. Keyboard cases use native Tab, arrows, Enter and Space. Coordinate-based cases test hit targets after scrolling, where automatic locator scrolling would otherwise make the test easier than the real interaction.

These checks establish particular browser conditions. A 360-pixel viewport is not a named phone model, and a touch-enabled desktop-size context is not proof of a physical tablet run. Device-scale configuration affects pixels per CSS pixel; it does not by itself change the CSS layout width.

## Zoom and visible scrollbars

The equivalent-zoom journey combines a smaller CSS viewport with scale 2. It checks readable controls, status and completion, small-board play, paused imports, Resume focus and large-board reachability. Text rectangles are inspected for clipping and overlap.

This is the specification's equivalent layout setup. It does not automate the browser zoom menu, prove pinch behavior on every device or cover arbitrary zoom factors. The game leaves zoom available and relies on responsive layout plus internal board scrolling for the specified cases. [Zoom behaviour](zoom-behaviour.md) carries the full layout contract.

The normal browser hides scrollbars by default. For visible-scrollbar tests, the wrapper launches a second browser without the `--hide-scrollbars` default argument. Each case verifies that the page scrollbar actually takes space before checking width. The result is evidence for that scrollbar mode in Chromium, not a universal claim about every platform's overlay-scrollbar settings.

## Origins and offline play

The distribution is tested directly as a disk file with the context offline. The request list is observed, the default board appears and a card can be revealed. This checks the intended self-contained delivery path in the tested Chromium combination.

Persistence cases use an HTTP server bound to loopback and an available private port. That origin is stable across reloads inside a context. It serves the same emitted HTML rather than the development server's module graph. This lets the tests inspect saved size and score behavior without relying on file-origin storage semantics.

The technology specification explicitly notes that disk-origin storage varies among browsers. The adapter catches unavailable-store and JSON failures so the session stays playable. There is no compatibility promise that a moved HTML file will preserve its previous browser storage association. [Private browsing notes](private-browsing-notes.md) and [Storage testing](storage-testing-guide.md) discuss the related limits.

## Accessibility evidence is engine-specific

The full journey captures Chromium's accessibility tree, including names, states, feedback and completion. Its record identifies the browser and Playwright and records no screen-reader run. Tree structure can differ between engines, and speech remains user acceptance. [Accessibility audit](accessibility-audit.md) describes the findings and qualifications.

A new compatibility report is most useful when its artifact, actual runtime, origin and input path are clear. A CDP helper failing before play is different from a game interaction failing. The current support claim remains the recorded combination, specified runtime floor and conditions listed above. [Release checklist](release-checklist.md) explains how a run becomes a release record.
