# Source layout

This page is a map of the current Neon Pairs source tree and the responsibilities of its files.

Paths here are relative to the project root. They describe authored source and configured outputs. [Module layout policy](module-layout-policy.md) explains placement decisions; this page is for finding the implementation.

## Entry and project root

`index.html` contains the document metadata, the `root` element and the development module entry `/src/main.tsx`. It declares English, a responsive viewport, the theme color `#090B14` and the title Neon Pairs. The source HTML references a module; the production HTML is assembled by Vite and the single-file plugin.

`src/main.tsx` looks up the root, throws `Missing application root.` if it is absent and renders `<Game />` under React `StrictMode`. There is no second application page or router mounted beside it. The game begins at `src/game/Game.tsx`.

The root also holds `package.json`, `package-lock.json`, four compiler configuration files, three Vite configurations and `vitest.config.ts`. `.prettierrc.json` configures formatting; `.prettierignore` and `.gitignore` identify generated or local work. The README records setup, commands, delivery, replay and the named runtime evidence.

## Game composition

`Game.tsx` calls `useGame`, passes session values and controller actions into the UI, and imports `game.css`. Its output is a single main element containing the heading, instructions, deal controls, statistics, turn feedback, board and reproduction controls.

Its one extra coordination operation is Resume. It asks the board's imperative ref to focus the remembered card, then calls the controller's resume action. That keeps the DOM focus handoff outside the core state. `Board` is keyed by `session.generation`, so a replacement remounts the board's local state.

`game.css` defines root colors and spacing values, system typography, page geometry, global focus styling and the reserved turn-feedback area. The more specific board, control and status rules live beside their components. [CSS organization](css-organization.md) records selectors and cascade relationships.

## Core modules directly under src/game

| File | What a reader finds there |
| --- | --- |
| `deck.ts` | Supported sizes, seed bounds, board-code parser and formatter, seeded shuffle and immutable board description |
| `selection.ts` | The closed, one-card and mismatch discriminated union |
| `game.ts` | `GameState`, caps, creation, victory, face-up positions, activation and logical advancement |
| `elapsed-time.ts` | Display formatting from elapsed milliseconds |
| `saved-format.ts` | Small shared JSON, object, key, integer and byte helpers |
| `snapshot-selection.ts` | Validation of matched positions and nested selection |
| `snapshot.ts` | Snapshot version and bound, serialization and complete-state validation |
| `replay.ts` | Replay limits, structured problems, ordered actions and final state |

These files use ordinary TypeScript functions and data. The core is not a React context or reducer hook. It can be imported by the session adapter and by the replay CLI without mounting the application.

`generateBoard` returns a frozen description with a frozen deck. `GameState` extends that description. Runtime progress is returned as new objects by transitions; ignored actions can return the existing object. This pattern explains why the board's memoized sets can survive clock-only updates.

## Core test files

`game.test.ts` checks card transitions and logical advancement. `completion.test.ts` finishes every supported size. `move-cap.test.ts` exercises play and records at the maximum move count. `elapsed-time.test.ts` covers the formatter and related elapsed behavior.

`deck.test.ts` checks parsing and generation. `deck-rejection.test.ts` references independently generated vectors for rejection-sampling boundaries. `game.fixture.ts` contains the example code, literal deck, pair positions, initial generator states and shared numerical specification vectors.

`snapshot.test.ts` and `snapshot-selection.test.ts` cover the outer state and nested validation. `replay.test.ts` and `replay-order.test.ts` cover replay acceptance and ordering. These files live near the rules even when a narrow regression deserves a different test filename from the principal module.

## Session directory

`session/session.ts` defines `Session`, the import notices and the pure session operations `initialSession`, `replaceGame`, `tickSession` and `activateSession`. It connects a game transition to feedback, completion state and preferences without itself calling the browser.

`session/use-game.ts` is the React controller. It reads initial preferences, creates the initial fresh board, keeps the latest session ref in step with state, exposes actions and composes the clock and saving hooks. Successful loads replace the game; failed loads publish notices. Exports and activations synchronize the clock first.

`session/browser-clock.ts` defines the 40ms scheduling interval and arithmetic over observed timestamps. `session/use-session-clock.ts` supplies observations, maintains visibility, schedules timeouts and removes pending work on effect cleanup. `session/browser-clock.test.ts` checks the arithmetic independently.

`session/fresh-board.ts` converts browser entropy into a valid board code, rejecting out-of-range candidates and an optional previous seed. `session/storage.ts` defines the preference key, version, default size, store interface, validators and best-score update. `session/use-saved-preferences.ts` obtains browser storage safely and writes changes in an effect.

`session/session.test.ts` covers state replacement, paused input, generations, repeated feedback events and completion handling. `session/storage.test.ts` uses in-memory and throwing stores to cover persistence decisions. [Session module](session-module.md) follows the exact call paths and state fields.

## Board directory

`board/Board.tsx` maps the immutable deck into cards. It derives a face-up set and a matched set, obtains the focus hook, exposes a remembered-focus method and supplies the `--grid-size` custom property. It keeps the activation callback stable while calling the latest controller action through a ref.

`board/Card.tsx` is memoized. It renders a native button, obtains its class and accessible description from `describeCard`, renders the face only when supplied and adds a matched mark for matched positions. It delegates navigation and focus to callbacks from the board.

`board/Face.tsx` converts a face design into inline SVG primitives. `board/face-design.ts` maps one of 128 face numbers to shape, marker, color, fill style, coordinates and label. `board/card-state.ts` derives a card's closed, revealed or matched description from supplied presentation data.

`board/keyboard.ts` computes arrow-navigation targets with edge stops and modifier handling. `board/use-board-focus.ts` owns the remembered position and DOM focus calls. `board/nearest-scroll.ts` remembers the viewport scroll position so focus can use nearest alignment after Chromium's automatic focus scrolling.

`board/board.css` owns the scrolling board viewport and grid tracks. `board/card.css` owns the button, concealed back, revealed/matched surfaces, corner mark and card transition. The directory's `keyboard.test.ts`, `card-state.test.ts` and `face-design.test.ts` check the numerical and descriptive helpers. Browser tests observe actual focus, artwork and scrolling.

## Controls and status

`controls/Controls.tsx` renders the grid-size select, New Game and Restart. It reads the size options from `GRID_SIZES`. `controls/control-labels.ts` owns the exact deal and reproduction control strings consumed by components and tests.

`controls/BoardCodeForm.tsx` renders the board-code field and submit button. Its input is keyed by generation, and successful submission moves focus to the load button because the old input is replaced. An invalid submission leaves the draft available for correction.

`controls/Reproduction.tsx` holds the snapshot textarea draft in local state, renders the details disclosure and export/load buttons, and shows the load notice and conditional Resume button outside the disclosure. It uses the game code and generation passed from the parent rather than reconstructing them.

`controls/control.css` defines the common control surface. `controls/controls.css` lays out deal controls. `controls/reproduction.css` lays out the disclosure, form rows, textarea and notice region.

`status/GameStatus.tsx` renders moves, pair progress, elapsed text, best moves and the completion paragraph. `status/status-labels.ts` owns statistic labels. `status/status.css` styles the statistic layout and completion treatment. The component receives the optional best as `number | undefined` and displays `none` when it is absent.

## Scripts directory

`scripts/replay.ts` is the command entry. It passes command-line arguments to `runReplay` and assigns the returned status to `process.exitCode`. `scripts/replay-cli.ts` handles one input path, caller-directory resolution, file reads, human-readable stderr failures and final snapshot output. `scripts/replay.test.ts` exercises the executable in child processes.

`scripts/source-files.ts` recursively reads ordinary files from a supplied folder and rejects links or special entries. `scripts/source-audit.test.ts` inspects packages, compiler settings, source length, unsafe constructs and selected effect boundaries. `scripts/source-conventions.test.ts` checks recognized exports/imports, yes-or-no naming, test names and environment reads.

These scripts are not imported by the application entry. Some import core functions because they are clients of the same public data behavior. That direction lets command-line replay reuse the rules without putting Node APIs into browser modules.

## Application-level test support

`tests/browser-test.ts` adapts Node tests to Playwright, manages browsers and contexts, records runtime versions, gathers page errors and provides artifact paths. `tests/artifact-server.ts` serves the built HTML over loopback. `tests/game-fixture.ts` contains public-control setup helpers, disk URLs and completion helpers.

`tests/deal-oracle.ts` independently computes a specified deal with BigInt. `tests/focus-bounds.ts`, `tests/readability.ts`, `tests/accessibility-tree.ts` and `tests/ui-snapshot.ts` provide focused measurement or comparison support. They are tools used by journeys, not application modules.

The many `.e2e.test.ts` files are grouped by the observable behavior they examine: controls, input, play, persistence, snapshots, lifecycle, clock edges, layout, motion, artwork, status, accessibility, touch and zoom. [Test policy](test-policy.md) explains the runner and evidence boundaries instead of repeating every assertion here.

## Diagnostic entries and configured outputs

`art/index.html` and `art/face-sheet.tsx` form a separate face-sheet build with `art/face-sheet.css`. It imports the real card and design definitions to render every face at a controlled size. `vite.art.config.ts` writes this entry to `.art`.

`art/session/index.html` and `art/session/session-harness.tsx` form a separate lifecycle harness. It mounts `Game` beside an unmount button. `vite.session.config.ts` writes it to `.session`. The harness exists so the browser suite can measure cleanup rather than infer it from a page navigation.

The production build writes `dist/index.html`. Local dependencies live under `node_modules`; browser downloads, temporary browser files, npm cache and test work have separate ignored directories. `test-results` holds browser runtime and journey artifacts. These directories have operational roles but are not additional application source layers.

There is no generated route tree, backend, API client, asset service or utility framework to discover beyond this map. For command ordering see [Build pipeline](build-pipeline.md); for import and runtime separation see [React boundaries](react-boundaries.md).
