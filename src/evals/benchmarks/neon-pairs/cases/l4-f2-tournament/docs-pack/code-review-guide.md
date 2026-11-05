# Code review guide

This page explains how to assess a Neon Pairs change using the behavior boundaries and evidence already present in the project.

A useful review starts with the observable change, follows the responsible transition or adapter and then evaluates the evidence at that boundary. The architecture specification calls for adding a rule at its game behavior and updating its component, tests and specification. The source is small enough that direct import tracing usually gives a clearer answer than a broad architectural summary.

## Start with one concrete before and after

A change description is easiest to assess when it identifies a trigger and an outcome. For example, an overdue mismatch could block a click until a delayed callback runs; the current controller synchronizes before activation so the click sees the logically closed selection. That example names a behavior, an ordering decision and the relevant regression evidence.

“Refactor timing” does not identify which part changed. The project has numerical clock arithmetic, browser observation, session gating, logical game advancement, display formatting and snapshot rounding. A review can be precise about one of those without reopening all the others.

The same applies to “fix import.” A parser correction, a paused-session correction and a focus correction belong to different files even when the user reaches all three through the same control. The review follows the actual changed contract.

## Find the owner

| Observable concern | First implementation to inspect | Evidence that reaches beyond it |
| --- | --- | --- |
| A move is counted incorrectly | `game.ts` activation | Session result, completion and browser play |
| A code reconstructs a different deal | `deck.ts` | Literal vectors and independent deal oracle |
| A snapshot accepts inconsistent progress | `snapshot.ts`, `snapshot-selection.ts` | Load controls and paused import journey |
| A notice or celebration is wrong | `session/session.ts` | Status and announcement journeys |
| A timer affects a replacement | Clock hook and generation checks | Restart and unmount journeys |
| Focus moves to the wrong card | `keyboard.ts` and board focus hook | Actual keyboard and scroll journeys |
| A best score is not retained | `storage.ts` and saving hook | Served-origin persistence journeys |
| A card looks different in diagnostics | Card styles and inherited values | Face-sheet/game computed-style comparison |
| A copied game requests another file | Vite config and built HTML | Direct-disk delivery checks |

The table is a starting point, not a routing system that replaces judgment. A change can cross several boundaries. [Source layout](source-layout.md) provides the inventory once the owner is identified.

## Core transition review

For a core change, the useful questions are its accepted inputs, ignored inputs, changed fields and retained references. `activateCard` returns its input for illegal attempts. A match creates a new matched array, while a first reveal changes selection without increasing moves. `advanceGame` treats active elapsed accumulation and mismatch expiry as related but distinct decisions.

A reviewer can compare those cases directly with [Rules](rules.md), [Selection state](selection-state.md) and [Turn resolution](turn-resolution.md). An apparently harmless mutation can matter because memoized board derivations rely on array and selection references. TypeScript readonly annotations help, but the actual returned collections remain part of the reasoning.

The existing caps have edge evidence. Matching and victory continue at the move cap, and mismatch resolution continues at the elapsed cap. A change that merely clamps a displayed value may leave serialized state inconsistent; a change that stops transitions at a numeric cap may alter gameplay. The rule owner is the place to assess those consequences.

## Format compatibility review

Board codes are inputs to deterministic generation, so shuffle implementation details can be externally observable. The literal example deck and independent rejection vectors are relevant when recurrence, traversal, starting order or sampling comparisons change. A board that “looks shuffled” is insufficient evidence of compatibility.

Snapshot review follows both shape and consistency: byte bound, exact outer fields, numeric bounds, reconstructed deck, matched pairs, nested selection and start-state relationships. Replay adds action bounds and ordering. A successful round trip alone cannot establish that invalid input is rejected correctly.

The current decoder does not replay an unknown past to prove state reachability. It validates the constraints represented by the snapshot. A review therefore evaluates the actual format contract instead of demanding an action history that the format does not contain. [Snapshot format](snapshot-format.md) and [Replay format](replay-format.md) carry the external definitions.

## Session and lifecycle review

`Session` owns pause, replacement generation, feedback events, celebration and preferences around a game. Replacing a game increments generation even for Restart of the same code. This lifetime is shared with board remounting and timer invalidation but is not serialized progress.

The clock hook checks generation before changing its observation baseline, and `tickSession` checks again before changing game data. Cleanup also marks the effect inactive, clears its timeout and removes its visibility listener. Each defense has a different purpose. Removing one because another exists needs an explanation at the corresponding lifecycle boundary.

Visibility ordering deserves particular attention. The handler synchronizes under the old stored visibility state, changes that state and reschedules. That accounts for the interval ending at the event. [Clock design](clock-design.md) gives concrete timelines, including hidden return and overdue click cases.

## Rendering and native interaction

Components consume data and actions. `Card` uses the native click path; navigation is a separate helper and DOM operation. The board owns remembered focus, and the reproduction component owns its draft. These presentation values do not belong in the serialized core merely because React can store them.

The board passes null face data for concealed positions and mounts a face only when visible. A DOM change can accidentally expose identity even if the picture remains hidden, so accessible names and attributes are relevant to review as well as pixels.

For large boards, elapsed-only updates should retain the values on which memoized card props depend. The existing stable activation wrapper and memoized sets explain the current rendering cost. A performance claim can be checked against those actual references rather than against the presence of `memo` alone.

Native interaction evidence includes context menu behavior, keyboard activation, Tab exit and touch scrolling. A synthetic handler that appears equivalent for a mouse can change those browser behaviors. [Input](input.md) is the contract; the end-to-end suite supplies the browser observations.

## Style and delivery review

Global CSS, SVG variables, scrollbar geometry and generated output can cross a local file's apparent boundary. [CSS organization](css-organization.md) traces the visual dependencies; [Offline delivery](offline-delivery.md) connects source, artifact inspection and disk journeys. Those pages identify the existing evidence relevant to such changes.

## Interpret checks at their scope

`typecheck` covers three compiler projects. `npm test` covers the Vitest patterns for core and scripts. `test:e2e` builds production and diagnostic output before the Node/Playwright journeys. `format:check` checks formatting. No one command listed here silently includes every other command.

The source audits enforce specific patterns for packages, file size, type escapes, exports, naming, effects and environment reads. They are useful mechanical evidence with known textual limits. [Source audit notes](source-audit-notes.md) documents those limits so a passing regex scan is not mistaken for a complete dependency or purity proof.

Browser runtime records identify the actual Node, npm, Playwright and Chromium versions. A supported runtime and the named tested combination are different claims. The README records the original named result; evidence from another execution belongs to its observed combination.

A precise review distinguishes source inspection, an attempted check, a blocked check and an actual passing result. [Source audit notes](source-audit-notes.md) describes the audit inputs and coverage.

## Keep the explanation proportional

A local wording correction can be explained by the changed string and its visible context. A transition correction benefits from a concrete state sequence and the regression that distinguishes old from new behavior. A lifecycle correction needs the event ordering and stale-work case that motivated it.

The architecture specification discourages a global store, router, event bus or generic utility folder. Existing direct imports and behavior-focused files usually make a smaller explanation possible. Shared support earns its place when real behaviors share the same meaning, as the JSON helpers already do.

A final review account is most useful when it states what behavior changed, where that decision now lives, what evidence observes it and what limitations remain. That lets the next engineer continue from a concrete result rather than from an unexplained green command or a long list of files.
