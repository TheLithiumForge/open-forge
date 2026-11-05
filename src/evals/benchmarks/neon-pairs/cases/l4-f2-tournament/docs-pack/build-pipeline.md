# Build pipeline

This page follows the commands that turn the Neon Pairs source into a distributable HTML file and the builds used by browser verification.

The production entry is the root `index.html`. Vite follows its module entry into `src/main.tsx`, bundles the React application and processes imported styles. The single-file plugin makes `dist/index.html` the delivery artifact. TypeScript checks happen before that bundling step and do not emit JavaScript themselves.

## Commands in the manifest

| Script | Exact command | Effect |
| --- | --- | --- |
| `dev` | `vite --host 127.0.0.1` | Start the local development server |
| `typecheck` | `tsc -p tsconfig.app.json && tsc -p tsconfig.node.json && tsc -p tsconfig.e2e.json` | Check browser, tooling and browser-test projects |
| `test` | `vitest run` | Run included unit and source-inspection tests |
| `build` | `npm run typecheck && vite build` | Check types and produce the game distribution |
| `build:art` | `vite build --config vite.art.config.ts && vite build --config vite.session.config.ts` | Build two diagnostic entries |
| `test:e2e` | `npm run build && npm run build:art && node --test --test-concurrency=1 "tests/*.e2e.test.ts"` | Build current source and run browser journeys |
| `replay` | `node scripts/replay.ts` | Run the replay CLI against a supplied path |
| `format` | `prettier --write .` | Format authored files |
| `format:check` | `prettier --check .` | Check formatting without rewriting |

The shell conjunctions are significant. A failed compiler prevents the production Vite invocation. A failed production or diagnostic build prevents the browser runner. The script does not catch those failures and open whatever happened to be in `dist` previously.

The production build does not run `npm test`, and the browser command does not run that unit command separately. These commands are complementary checks, not aliases for a single aggregate verifier. A successful `build` establishes type checking and bundling, not completion of the rule and browser suites.

## Production configuration

`vite.config.ts` imports `react` from `@vitejs/plugin-react`, `defineConfig` from Vite and `viteSingleFile` from `vite-plugin-singlefile`. Its plugins are `[react(), viteSingleFile()]`. The build target is `es2023`.

The configuration sets `modulePreload: false`. Vite's preload helper would be unnecessary for an inline page with no external chunks, and its generated fetch call would undermine a straightforward inspection of the offline artifact. The code comment states that reason next to the option.

No custom production root or output directory is set in this configuration. The root HTML entry and the documented `dist/index.html` outcome follow the normal project build path. By contrast, the diagnostic configs explicitly change root and output paths, which is why they do not overwrite the game distribution.

The development server watch configuration ignores `.browser-work`, `.playwright-browsers` and `.npm-cache` patterns. Browser execution and installation can write heavily under those directories; ignoring them keeps that operational activity outside source watching. These watch exclusions are not product asset exclusions.

## Entry processing

The source HTML declares a UTF-8 document, English language, a responsive viewport, the dark theme color and the page title. Its body has one `root` div and the module script `/src/main.tsx`. That path is an input to the build, not an additional file a player must copy alongside the final HTML.

`main.tsx` imports React's StrictMode, React DOM's root API and `Game`. Component imports pull in the game, its controllers, inline artwork definitions and styles. `Game` imports the root stylesheet, and its children import their adjacent sheets. There is no separate handwritten stylesheet link in the source HTML.

Faces are JSX-generated SVG rather than external image references. The system font is set in CSS. These choices reduce the work the bundler must do to produce a self-contained result: the artwork is already part of the module graph, and typography requires no font asset.

## Compiler checks before bundling

All projects extend `tsconfig.base.json`, which sets `noEmit: true`. `tsc` is used for diagnostics, while Vite performs the transformation and bundle emission. Passing the Vite transform alone would not establish that the strict application types are sound.

The application compiler includes browser source and diagnostic TSX entries. The Node compiler includes configs, scripts, colocated tests and fixtures. The end-to-end compiler includes `tests/**/*.ts`. Each provides libraries and ambient type packages appropriate to its environment.

These projects are invoked sequentially in the manifest. A diagnostic in the first project stops later commands through the conjunction. The source audit checks the literal typecheck command, which makes omission of a compiler project observable. [TypeScript settings](typescript-settings.md) gives the exact options and explains import-graph coverage.

## Separate diagnostic builds

`vite.art.config.ts` sets its root from `new URL('./art', import.meta.url)`, converted with `fileURLToPath`. It uses the React and single-file plugins and writes to `../.art` relative to that root. The resulting `.art/index.html` renders all faces through the real card component.

`vite.session.config.ts` similarly roots the build at `./art/session` and writes to `../../.session`. The harness mounts the real game and adds an unmount button. Browser tests use it to inspect pending timer and listener cleanup after removing the application.

Both diagnostic configs set `emptyOutDir: true` and target ES2023. Their separate roots, source files and output directories explain the application boundary concretely. The face sheet is not a hash branch of the production entry, and the session harness is not a conditional control embedded in `Game`.

`build:art` performs both builds in sequence. If the first fails, the second is not started; if either fails, `test:e2e` does not proceed. The diagnostic outputs are verification resources rather than files required for ordinary play.

## How browser verification opens the result

The browser support defines disk URLs for `dist/index.html`, `.art/index.html` and `.session/index.html`. Journeys that use those URLs open the generated files directly. The support also starts a tiny HTTP server that reads the built game HTML and serves it at `/` on an ephemeral loopback port.

That server is `tests/artifact-server.ts`, not the Vite development server. It serves the bytes already produced by the build, returns 404 for other paths and exposes a close function. Storage journeys can use its HTTP origin without accidentally switching verification to source-time development transforms.

`tests/browser-test.ts` launches Chromium, creates fresh contexts for journeys, gathers page errors and closes contexts, browsers and the server during teardown. The command uses `--test-concurrency=1` at the Node runner level. This arrangement makes lifecycle ownership visible instead of depending on a long-running external development server.

## Inspecting delivery output

`tests/build-output.e2e.test.ts` reads the finished HTML and checks that it contains neither the `fetch(` marker nor the `relList` preload marker. These assertions are intentionally concrete and textual. They check the particular unwanted output known to the build configuration.

A textual artifact check and a browser journey provide different evidence. The former can detect a generated helper even if that helper would not make a request in the tested browser. The latter can reveal unresolved resources, page errors or direct-disk behavior that a narrow text search would miss. [Offline delivery](offline-delivery.md) discusses this combination.

The configured output is one HTML file carrying script, styles and artwork for play. The setup process can still require package downloads and a browser installation. Offline refers to opening the finished artifact, not to creating a clean development environment without a network.

## Runtime evidence and supported tools

The engine minimum is Node `24.19.0`; the README's named tested combination is Node `26.12.1`, npm `11.20.0`, Playwright `1.64.1` and Chromium `156.0.8078.4`. The browser helper writes observed versions to `test-results/browser-runtime.json` and enforces the named values when `NEON_PAIRS_NAMED_COMBINATION=1`. Its optional executable override makes recording the actual browser particularly useful. [Dependency policy](dependency-policy.md) records the locked package set.

## Failures by stage

A typecheck failure points to a compiler project before bundle output. A Vite failure concerns transform, dependency resolution or output construction. A diagnostic build failure can concern the face sheet or harness even while the ordinary game build is valid. A browser setup failure can concern executable availability or named-version mismatch before any journey begins.

A missing fixture affects unit or browser assertions that read it.

Once a journey starts, page errors, missing controls, incorrect focus, storage behavior and visual measurements are application-level evidence. Keeping the failure stage visible saves time: an unavailable Chromium executable is not a deck-generation regression, and a correct deck does not prove the artifact is self-contained.

The related review path is in [Code review guide](code-review-guide.md). The source-side checks are catalogued in [Source audit notes](source-audit-notes.md), and the complete command responsibilities are also stated by [Technology](technology.md).
