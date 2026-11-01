# Architecture
Architecture is the code structure for Neon Pairs that keeps game rules separate from browser effects.

Status: spec version 2.1.

A card activation changes game data first. React renders that data, then the session adapter handles time and saved preferences. The same rules run in browser tests and replay without a browser.

## Game core

- A01 - Keep deck generation, card transitions, logical time, snapshot validation and replay as pure functions under `src/game`. They receive their inputs directly. They don't read the DOM, storage, external random sources or browser clocks.

- A02 - Keep the board description and deck immutable. Keep matched positions, current selection, moves, active elapsed milliseconds and whether play started in game state. Derive pair count and victory. Serialize progress using the [reproducibility formats](reproducibility.md).

- A03 - Use direct calls and exhaustive branches for the closed, one-card and mismatch states. Keep named states, format versions, timing values and size limits beside the behavior they control.

Proposed game functions:

| Call | Result |
| --- | --- |
| `createGame(boardCode)` | Initial state for a valid board code |
| `activateCard(state, index)` | State after one card activation |
| `advanceGame(state, milliseconds)` | State after logical time advances |
| `isWon(state)` | Whether every position is matched |
| `encodeSnapshot(state)` | Version 1 snapshot JSON |
| `decodeSnapshot(text)` | Valid restored state or `null` |
| `replayGame(text)` | Final replay state or `null` |

## Browser session

- A04 - Keep React state, timer lifecycle and browser visibility handling in `src/game/session`. Use a monotonic browser clock. Clear pending work on unmount and invalidate callbacks when the deal changes. A callback from an earlier deal can't change the current one.

- A05 - Keep preference and best-score reads in the session storage module. Treat saved JSON as unknown input. Validate grid sizes and finite integer scores. Catch read, parse and write failures. Keep unrelated storage keys untouched. A failure leaves the game playable.

- A06 - Keep active game progress in memory until you export or import a snapshot. Importing a completed snapshot doesn't update best scores or play a celebration. Completing an unfinished imported game records a local best under R10.

## Components

- A07 - `Game.tsx` composes status, the board, game controls and reproduction controls. Keep board navigation in `src/game/board`, controls in `src/game/controls` and session behavior in `src/game/session`. Keep each component's props and styles beside it.

- A08 - A card mounts its face SVG only while revealed or matched. Hidden card identity stays out of DOM attributes, accessible names and diagnostic globals. Tests can know a deck from its documented board code. The app gives them no solver endpoint.

- A09 - Use native browser input and the shared card activation function. Keep focus position in the board component. Focus movement changes presentation, never the deck or move count. [Input](input.md) defines the keys and focus rules.

## Evidence

- A10 - Keep focused tests beside their behavior. Complete browser journeys live at the application test level. Check generation against independent literal vectors. Check game transitions against stated rules. Test built HTML from disk as well as served storage behavior.

- A11 - Keep development fixtures and diagnostic helpers out of the shipped game. Tests can use the documented board code, snapshot and replay formats. The shipped game exposes only its documented controls.

## Changes

- A12 - Add a new rule at its game behavior, then update its component, tests and specification. Add shared support only when two actual behaviors need the same meaning. Don't add a global store, event bus, router or generic utility folder.

The [game specification](README.md) defines the behavior this structure implements.
