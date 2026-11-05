# Dependency policy

This page records the packages Neon Pairs already uses and explains how their roles fit the small offline application.

The source of truth for installed package names and versions is `package.json` together with `package-lock.json`. [Technology](technology.md) specifies the stack and pinned tools; the manifest also contains formatting and image-analysis tools used by this implementation. The distinction matters when investigating a lockfile change: the complete current manifest is broader than the technology page's main stack table.

## The current package boundary

The runtime dependency set is React and React DOM. Build, type checking, tests, formatting and image analysis sit in development tooling. A feature adds no entries to dependencies or devDependencies. Existing packages cover rendering, native control interaction, deterministic rules, build delivery and the verification tools described below.

This is an application with a private package manifest, not a published library. Its package name is `neon-pairs-reference`, version `1.0.0`, with `private: true` and `type: module`. The engine field accepts Node `>=24.19.0`. Those facts describe the project package; they do not mean the Node runtime becomes part of the distributed HTML.

## Installed packages and their jobs

| Manifest section | Package | Exact version | Role in this repository |
| --- | --- | --- | --- |
| dependencies | `react` | `19.3.1` | Components, state, refs, hooks and memoization |
| dependencies | `react-dom` | `19.3.1` | Browser root rendering |
| devDependencies | `@types/react` | `19.3.0` | React TypeScript declarations |
| devDependencies | `@types/react-dom` | `19.3.0` | DOM renderer declarations |
| devDependencies | `typescript` | `7.0.2` | Separate compiler checks |
| devDependencies | `vite` | `8.3.5` | Development server and production bundling |
| devDependencies | `@vitejs/plugin-react` | `6.1.2` | React integration for Vite |
| devDependencies | `vite-plugin-singlefile` | `2.3.4` | Inline distribution output |
| devDependencies | `vitest` | `5.0.5` | Unit and source checks |
| devDependencies | `@playwright/test` | `1.64.1` | Chromium automation and assertions |
| devDependencies | `@types/node` | `24.19.2` | Node declarations for tooling |
| devDependencies | `prettier` | `3.9.6` | Repository formatting |
| devDependencies | `pngjs` | `7.0.0` | PNG handling in visual test support |
| devDependencies | `@types/pngjs` | `6.0.5` | PNG tooling declarations |

The versions above are recorded in the manifest. The project uses the reviewed locked set. A reader reproducing the implementation needs that set, rather than a moving registry tag.

## Runtime code stays small in purpose

React state and effects are sufficient for the current session. `use-game.ts` stores one `Session` and a ref to its latest value, then returns named actions. There is no state-management package coordinating the core. Components get data and functions through props, and the board keeps focus locally.

Browser-native buttons, select, details, input, textarea and form submission provide the interaction primitives. The board uses CSS Grid. Faces are inline SVG assembled by `Face.tsx` from `faceDesign`. There is no component library, icon font, font download or media pack supporting ordinary play.

The deterministic generator is plain TypeScript. Snapshot and replay readers use JSON and explicit validation. Timing uses a browser adapter plus numerical deltas. Preferences use a narrow storage interface. These implementations are small enough to inspect against the specification, which is especially valuable where a package's general behavior would be broader than the game's contract.

React and React DOM are bundled into the distribution rather than fetched from a CDN. A runtime dependency in the manifest therefore does not imply a runtime network dependency. The network boundary is the finished artifact: it carries what play needs. [Offline delivery](offline-delivery.md) explains how this differs from dependency installation and local development.

## Build tooling is not product functionality

Vite transforms and bundles the browser entry. The React plugin participates in that transformation. The single-file plugin inlines what the distribution needs into `dist/index.html`. None of these tools runs as a build service when a player opens the finished file.

TypeScript checks three compiler projects before the production Vite build. The compiler's `noEmit` setting keeps its job separate from bundling. The distribution target is ES2023; both the compiler settings and Vite configuration make that choice visible. [Build pipeline](build-pipeline.md) follows the commands and outputs, while [TypeScript settings](typescript-settings.md) covers compiler coverage.

Prettier is a development tool with `format` and `format:check` scripts. The former writes files; the latter checks formatting. `.prettierignore` excludes dependencies, build output and browser work directories. Running a formatter is therefore different from testing the game and is not part of the production runtime.

`pngjs` and its declarations support screenshot interpretation. The artwork and contrast checks need to inspect image pixels produced by the browser. That purpose belongs to test support. The game itself draws SVG elements and has no PNG decoder in its application source.

## Browser automation has two parts

The repository uses Playwright's Chromium API and assertions under Node's test runner. The browser binary is a separate installation from the locked JavaScript package. The browser helper records actual versions and can enforce the README's named combination; [Build pipeline](build-pipeline.md) explains that evidence and the executable override.

## Installation and lockfile evidence

`npm ci` is the documented installation path. It uses the lockfile rather than treating the version table as an instruction to resolve a fresh tree. Dependency installation can use the network. A finished game does not need the package registry or a Node process.

`scripts/source-audit.test.ts` reads the manifest and lockfile as unknown JSON, narrows their shapes and checks the root lockfile dependency maps against the manifest. It also compares reviewed versions against a package fixture referenced by the test.

The audit explicitly asserts that the sorted runtime dependency names are `react` and `react-dom`. For the reviewed version fixture, it compares installed manifest values by name. It does not recursively prove the behavior of every transitive package in the lockfile, and its source is not a vulnerability scanner. Those limits are useful when interpreting a passing result.

An unexpected lockfile diff can arise from a changed root manifest, a changed transitive resolution or a different installer action. The repository's evidence lets a reviewer compare the root maps and exact package values. It does not justify treating every lockfile line as a hand-authored product change.

## Where package changes become observable

A renderer or React plugin change can affect component mounting, effects and the production output. A TypeScript change can alter narrowing diagnostics without changing emitted gameplay code because this compiler invocation emits nothing. A Vite or single-file plugin change can affect external references, preload helpers and direct disk delivery. A browser automation change can alter screenshots, accessibility trees or scroll behavior.

These are different kinds of evidence. Unit transitions help with rules, but they cannot show that a bundle opens from disk without requests. A successful type check cannot show that the browser used the named executable. A screenshot cannot show that the lockfile and manifest agree. [Test policy](test-policy.md) maps those checks to their actual responsibilities.

The production config sets `modulePreload: false` because an inline page has no chunks to preload. `tests/build-output.e2e.test.ts` inspects the resulting HTML for the fetch call and preload marker it intends to exclude. This is a concrete example of a tooling choice becoming part of delivery behavior rather than an invisible implementation detail.

## Reading package evidence in a review

The useful questions are specific: which script imports this package, whether any application import was introduced, whether the output still carries its assets, and which recorded versions produced the result. The current architecture has direct imports, so tracing a package from a tool or component is straightforward.

A manifest entry can be unused even if it is locked, and a browser binary can differ even if the package version is pinned. The source audit and browser runtime record address different portions of that uncertainty. Neither replaces an inspection of the changed import or the finished HTML.

For adjacent decisions, see [Dependency review history](adr-029-pinned-versions.md), [Build pipeline](build-pipeline.md) and [Source audit notes](source-audit-notes.md). They explain the stability goal, execution path and automated checks without changing the dependency set recorded here.
