# Offline delivery

This page explains why Neon Pairs ships as one HTML file and how that delivery model is implemented and checked.

The finished game is `dist/index.html`. It carries the script, styles and artwork needed to play and can open directly from disk. The development project has Node packages, build configuration, test support and diagnostic entries, but those are tools for producing and checking the artifact rather than files a player needs beside it.

## Why the artifact is the unit of delivery

A single file is easy to copy, attach, archive and open without establishing an application host. It also makes the delivery question concrete: the copied HTML either contains what play needs or it does not. There is no backend deployment, account setup or API credential in the current game.

The application fits this model because its state transitions are local. Matching uses the immutable deck and current selection. Elapsed time is tracked in memory. Saved preferences use the browser's local storage when available. Board codes and snapshots carry explicit reproducibility data, so exchanging a game does not require a shared server database.

This rationale does not imply that a single file can provide every service a hosted application might offer. The current product has no such service dependency. [Architecture](architecture.md) and [Technology](technology.md) describe that existing scope; this page concerns delivery within it.

## Source HTML and delivered HTML differ

The root source `index.html` contains a module reference to `/src/main.tsx`. Opening that authored file is not the documented distribution path. Vite starts there, follows imports and constructs the production artifact. The single-file plugin in `vite.config.ts` participates in embedding the result.

React and React DOM are local package dependencies bundled for the browser. Component CSS is imported from source modules. Face artwork is inline SVG constructed from TypeScript design data. No font download is needed because the root stylesheet uses `system-ui, sans-serif`.

The resulting artifact is designed to operate without resolving the source tree. Copying `src`, `node_modules` or the Vite config is unnecessary for play. Conversely, inspecting the source HTML alone cannot establish whether the generated artifact includes every resource; the output is what the delivery checks examine.

## Configuration behind self-containment

The production Vite configuration uses the React plugin and `viteSingleFile()`, targets ES2023 and disables module preload. Its comment explains that an inline page has no chunks to preload, so a generated preload helper and its fetch call are unnecessary.

The module graph contains ordinary imported CSS and inline artwork. There is no application fetch call for a face, score table or settings file. The source audit checks selected network and media constructs, and the artifact test checks known unwanted generated markers.

The configuration does not use a service worker or an application cache to make a hosted multi-file application available later. The file itself carries the playable result. That distinction is operationally useful: a copied distribution does not need a prior successful visit to an origin before it can be opened offline.

## What still uses the network

Installing the development dependencies can contact the package registry. Installing Chromium for browser verification downloads a browser binary. The Vite development server serves development resources locally, and browser tests can serve built HTML over loopback for storage behavior.

None of those activities changes the delivery claim about play. They happen in the authoring and verification environment. A finished game does not contact a registry, development server or remote asset provider to reveal cards or resolve turns.

The acceptance journey opens the production HTML with network access blocked. This exercises the artifact in the condition the product promises. A development-server journey alone would be insufficient because that server can supply missing resources from the project tree.

## Disk delivery and saved preferences

Browser storage under a `file:` URL varies between browsers. The project therefore distinguishes direct-disk play from stable-origin preference checks. The README recommends a local HTTP origin when saved sizes and best moves need a stable browser origin.

This is not a second game implementation. `tests/artifact-server.ts` reads the already built HTML and returns it from `/` on a loopback address. The same game code runs with an HTTP origin. The Vite development command is also available for local use, but the test server deliberately serves production bytes.

`browserStore` catches storage acquisition failure. The preference reader and writer catch operation failures and malformed content. If storage is blocked, the current game remains playable. No remote persistence service takes over, and active progress is not silently uploaded somewhere else.

The preference record stores selected size and best moves. Active progress remains in memory until a snapshot is exported or imported. Copying the HTML to another device does not copy that device's local storage. Copying a snapshot transfers the described progress through the explicit format instead. [Persistence overview](persistence-overview.md) and [Reproducibility](reproducibility.md) explain those data boundaries.

## Explicit exchange fits offline use

Board codes reconstruct deals, while snapshots carry progress as text through native controls. No exchange server participates. Export synchronizes the clock; unfinished imports pause for Resume. [Reproducibility](reproducibility.md) describes those formats and the separate Node replay command, which has no button in the browser game.

## Diagnostic outputs have a separate purpose

The face sheet builds to `.art/index.html` and the unmount harness to `.session/index.html`, each from a separate HTML entry. Neither is needed by the production artifact. They reuse application components for measurement while leaving their diagnostic entry behavior outside the shipped module graph. [Build pipeline](build-pipeline.md) gives their roots, configuration and command order.

## Evidence at three levels

| Level | Existing evidence | What it establishes |
| --- | --- | --- |
| Authored source | Core and shipped-source audit patterns | Known browser-effect, media, network and diagnostic constructs are absent from scanned locations |
| Built text | `build-output.e2e.test.ts` | The HTML excludes the checked fetch and preload markers |
| Running artifact | Disk journeys and acceptance | The generated page opens and plays with its resources present |

These checks complement each other. A generated helper can exist even when the authored source has no corresponding call. A textual check can miss a resource dependency expressed in another way. A successful browser session can miss a dormant code path. The layered evidence makes the delivery result more credible without claiming a formal proof of every possible network behavior.

The browser suite builds current source before running. Its command stops on a failed build, so it does not accidentally demonstrate an earlier valid artifact after a current source failure. [Build pipeline](build-pipeline.md) gives the exact sequence.

## Browser and language assumptions

The build targets ES2023 and the README records a specific Chromium compatibility run. Offline delivery does not imply universal browser verification. Zoom remains available, and large boards scroll inside their viewport. [Browser support](browser-support.md) is the related evidence page for browser coverage.

## Investigating a broken copy

A request to `/src/main.tsx` from an allegedly distributed page suggests the authored HTML was copied instead of the built output. A missing stylesheet or asset request suggests the artifact has lost self-containment. A storage difference between two copies can come from their origins even if both files contain identical game code.

For an apparent state loss, the first distinction is preferences versus active progress. Preferences have a local record; progress has an explicit exchange format. A fresh browser open starts a fresh board for the selected size, because the application does not automatically persist the current game.

For unexpected requests, the generated HTML and browser request log are more useful than the presence of a package in `node_modules`. A runtime package can be fully embedded, while a generated preload helper can introduce a request path despite an otherwise local source graph. That is why the production configuration and built-output check sit together in the delivery story.

The local-file artifact remains the same unit whether it is copied by a person, opened from a working directory or served by the small test server. The surrounding origin can change storage behavior; it does not change which source implements the rules. [Source layout](source-layout.md) maps the entry and its imports for deeper investigation.
