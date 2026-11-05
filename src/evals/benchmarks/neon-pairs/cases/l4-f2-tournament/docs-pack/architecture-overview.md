# Architecture overview

This page is the one-page picture of how the game is built today, with the decisions behind each part.

[Architecture](architecture.md) sets the requirements, and [Source layout](source-layout.md) walks the tree file by file. This page answers a smaller question: what are the parts, and what do they hold?

## Modes

Solo play is the only mode. `src/main.tsx` renders `Game`, and every control acts on the one running game. The game has no accounts and no service behind it, as [ADR 004](adr-004-no-backend.md) records, and it ships as one HTML file, as [ADR 003](adr-003-single-file.md) records.

## Modules

| Module | Holds | Decisions |
| --- | --- | --- |
| `src/game/game.ts`, `src/game/selection.ts` | Game state and card transitions | [ADR 001](adr-001-react.md) |
| `src/game/deck.ts` | Deck generation from a board code | [ADR 009](adr-009-board-codes.md) |
| `src/game/snapshot.ts`, `src/game/snapshot-selection.ts`, `src/game/saved-format.ts` | Progress snapshots and saved formats | [ADR 010](adr-010-snapshot-json.md) |
| `src/game/replay.ts` | Replay from recorded moves | [ADR 002](adr-002-typescript.md) |
| `src/game/elapsed-time.ts` | Active time as a pure calculation | [ADR 013](adr-013-monotonic-clock.md) |
| `src/game/session/` | The browser session: clock, fresh boards and storage | [ADR 013](adr-013-monotonic-clock.md) |
| `src/game/Game.tsx`, `src/game/board/`, `src/game/controls/`, `src/game/status/` | The React view | [ADR 001](adr-001-react.md) |

These are all the modules the game has. A reader who finds behavior outside them has found something this page should list.

## How data moves

A card activation changes game data first. React renders that data, and the session adapter handles time and saved preferences. The same pure functions run in browser tests and in replay, which is why they take their inputs directly.

## Stored and shared data

Preferences are the only data written to the browser. [Storage inventory](storage-inventory.md) lists what that means in keys.

Progress snapshots are the only JSON the game exports. [Export inventory](export-inventory.md) lists the formats.
