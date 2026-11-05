# Module layout policy

This page explains how Neon Pairs places behavior in the source tree and how to extend that structure without obscuring the game rules.

The current tree is small enough to read directly. It has no application framework around the game, no route hierarchy and no general service container. `src/main.tsx` mounts `Game`; the application then divides into rule functions, a session adapter and components for the board, controls and status. [Source layout](source-layout.md) is the file inventory. This page explains the choices behind it.

## Follow behavior before file type

The board directory contains `Board.tsx`, `Card.tsx`, `Face.tsx`, navigation helpers, card descriptions, face design, styles and focused tests. Its contents answer a coherent set of questions: how a position is presented, how focus moves and how a face becomes visible. A separate top-level directory for every hook, type or stylesheet would make those questions harder to answer.

The session directory has a different responsibility. It connects rules to React state, browser observations and saved preferences. A feature lives in src/game/FEATURE/, and its rules live in src/game/FEATURE/rules.ts, which imports neither React nor any browser global. That placement gives a reader a predictable starting point for a feature's decisions, with the integration code close enough to inspect alongside them.

This convention applies to feature work; existing behaviors also use other filenames. The current core is in `game.ts`, `deck.ts`, `selection.ts`, `snapshot.ts`, `snapshot-selection.ts`, `saved-format.ts`, `elapsed-time.ts` and `replay.ts` directly under `src/game`. The board already uses focused names such as `keyboard.ts` and `card-state.ts`.

## The dependency direction

The central rules know about game data. `game.ts` imports the deck generator and the selection union. It does not import `Game.tsx`, `Board.tsx` or the session hook. `snapshot.ts` rebuilds and validates a game using those same definitions. `replay.ts` applies logical advances and activations to a game without rendering it.

The session layer points inward. `session.ts` calls `activateCard`, `advanceGame`, `createGame` and `isWon`, then adds session feedback, import pause state and best-score decisions. `use-game.ts` provides callable actions to components. The UI receives values and actions; it does not become the only place where a legal turn can be computed.

There are intentional lateral imports. `storage.ts` uses `GRID_SIZES` and `MOVES_MAX` to validate preferences against actual game limits. `card-state.ts` uses `faceDesign` to describe a visible face. These relationships connect meanings that genuinely belong together. They do not form a general helpers layer with unrelated operations.

A useful way to inspect an unfamiliar change is to start at its public behavior and follow direct imports toward the core. A card click leads through the controller to `activateSession` and `activateCard`. A loaded snapshot leads through `decodeSnapshot`, `readMatched` and `readSelection`. The direction remains visible because there are no barrel files hiding which definition is used.

## State is divided by meaning

`GameState` extends the board description with matched positions, the current selection, moves, elapsed milliseconds and a start flag. It does not store a focus position or a form draft. The board description includes its code, size, seed and deck. `generateBoard` freezes the description and deck at runtime, while the TypeScript interfaces expose readonly fields.

`Session` wraps that game with `paused`, `generation`, `feedback`, `loadNotice`, `announcement`, `celebration` and `preferences`. A generation identifies a replacement deal. It is not a move count and does not belong in a snapshot. Feedback belongs here because turn results and imports need session-level presentation, while replay needs only the final game state.

The board hook owns its remembered focused index. `Reproduction` owns the textarea draft. `BoardCodeForm` uses an uncontrolled input initialized from the current code, with its generation key causing a fresh input on replacement. These values have useful local lifetimes. Moving them into `GameState` would make serialization and rule transitions depend on presentation details.

## Splitting a file

Technology specification T06 sets an aim of 100 lines and a ceiling of 200 authored TypeScript lines per file. The source audit checks the ceiling across `src`, `scripts`, `tests` and `art`. The aim is a reading aid, not a reason to scatter every function into a separate module.

The current snapshot split is instructive. `snapshot.ts` owns the outer format, byte bound, numeric fields, serialization and consistency checks. `snapshot-selection.ts` owns the nested selection and matched-position interpretation. Both operate on the same domain, but their validation tasks are distinct enough to read independently. `saved-format.ts` holds the small operations actually shared by snapshot, replay and preferences.

The clock split follows a different seam. `browser-clock.ts` performs arithmetic on supplied observations. `use-session-clock.ts` owns observation, scheduling, visibility and cleanup. Splitting those by arbitrary line count would lose the reason for the boundary. [Clock design](clock-design.md) follows the interaction in detail.

A file can stay short while still carrying several unrelated concerns. Conversely, a dense type and a straightforward exhaustive branch may reasonably live together. The existing architecture asks for splitting by behavior, so a review considers what a reader would need to understand together before considering the final line count.

## Imports expose the owner

The source uses static ESM imports with `.ts` and `.tsx` suffixes. Types are imported with `import type` or an inline `type` marker. `use-game.ts` imports the snapshot decoder directly from `../snapshot.ts`; it does not import an application-wide registry. `Controls.tsx` imports `GRID_SIZES` from `../deck.ts`, where supported sizes are defined.

That directness matters when a symbol changes. `SelectionKind` is the owner of the selection discriminants; both rules and validators use it. `MISMATCH_MS` is defined beside turn resolution and imported where an interval bound is needed. A second constant with the same current number would hide a semantic disagreement until a later change.

The source convention tests also inspect exported values. They look for an importer for every recognized export and a single owner for each recognized exported constant name. This is a lightweight check, not a complete module system. Its practical effect is to discourage speculative public helpers and ambiguous exported constants. [Source audit notes](source-audit-notes.md) describes the exact text patterns and their limitations.

## Components and their adjacent styles

Component imports make stylesheet ownership explicit. `Game.tsx` imports `game.css`; `Board.tsx` imports `board.css`; `Card.tsx` imports `card.css`; the controls share `control.css` and add their own layout sheets. These are global CSS files, not generated CSS modules. Adjacency helps locate a selector, but it does not automatically scope one.

A board change can therefore involve both a local sheet and root styling. `board.css` defines `.board-viewport`, whose geometry is consumed by focus scrolling. `game.css` defines the global focus outline and the page width. Reading only the nearest stylesheet is sometimes insufficient. [CSS organization](css-organization.md) traces these dependencies without turning visual requirements into module rules.

## Test support and application support

`game.fixture.ts` supplies a documented example code, a literal deck, its pairs and specification vectors. Application modules do not import it. Browser support in `tests/game-fixture.ts` loads a fixed board through the reproduction controls. `tests/deal-oracle.ts` implements the documented shuffle with BigInt arithmetic so the browser suite can reason about deals independently.

The separate `art` entries reuse application components but remain separate builds. `art/face-sheet.tsx` renders the real card component for each face. `art/session/session-harness.tsx` mounts the game beside an unmount button so teardown can be measured. Neither entry is imported by `src/main.tsx`.

That distinction explains why a helper can be useful without belonging in the shipped game. A fixture has permission to know the complete fixed deck. A hidden card in the application receives `null` instead of its face until revealed. Moving diagnostic knowledge into runtime code would bypass that deliberate presentation boundary.

## Examples of placement decisions

A correction to arrow-key edge behavior belongs first in `board/keyboard.ts`, which computes a target index from an index, size, key and modifiers. Its React caller translates a target into DOM focus. A change to the scroll behavior belongs in the hook or `nearest-scroll.ts`, because it concerns the browser's focus and scrolling behavior. Neither change calls for a new game-state field.

A correction to imported mismatch validity belongs in `snapshot-selection.ts`; a change to the loading pause belongs in `session.ts` or `use-game.ts`. They may share a user journey, but they answer different questions: whether the submitted data describes a valid game and how the browser enters that valid game.

A turn-count defect belongs in `activateCard`. The displayed statistic and exported snapshot already consume its result. Fixing the number in `GameStatus` would leave replay, completion and storage observing the old value. These examples are consequences of the existing structure, not extra parallel implementations.

For review, [Code review guide](code-review-guide.md) starts from the behavior changed and follows these seams. For the larger rationale, [Architecture](architecture.md) remains the specification of the core, browser session and component boundaries.
