# Export inventory

This page lists every piece of JSON a person can copy out of the game and load back in today.

The game exports one kind of JSON, the progress snapshot.

| What | Made by | Loaded by | Defined in |
| --- | --- | --- | --- |
| Progress snapshot | `src/game/snapshot.ts` | `src/game/snapshot.ts`, through the Load control | [Reproducibility](reproducibility.md) |

Board codes are shared too, but they are short text and not JSON. [ADR 010](adr-010-snapshot-json.md) records why snapshots are readable JSON.
