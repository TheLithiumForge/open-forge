# ADR 004: Keep play local

This record explains why the playable game has no backend and what that means for records and sharing.

## Decision

Neon Pairs runs its game rules in the browser and needs no application server. There is no account, API key or remote score service. A local development server and the HTTP server used in tests serve files; neither is a gameplay backend. The distinction is practical: opening the built HTML from disk still provides the game.

A memory game needs a deck, a turn state, a clock and a way to display progress. All four can live locally. Adding a backend would create deployment, connectivity and identity questions without improving the required action of revealing and matching cards. The accepted scope therefore uses local preferences for size and best moves and explicit text formats for reproducing a game.

## How information moves

`freshBoardCode` chooses a browser seed. `createGame` reconstructs the deck from the code. `activateCard` and `advanceGame` update the data. The session adapter owns browser time and preference writes. None of those operations needs a request to another process.

A board code can be copied to repeat a starting arrangement. A snapshot can be exported into a textarea and copied manually to preserve progress. Replay files support engineering investigations through the CLI. Those mechanisms supply useful sharing without a hosted document store. They also make the limits clear: a board code has no progress, and local JSON validation establishes structural consistency rather than verified provenance. See [Sharing FAQ](sharing-faq.md) and [Reproducibility](reproducibility.md).

## Records are local evidence

`Preferences` contains a selected size and a map of best move counts. `rememberBest` compares a completed result with the entry for that size. This is a convenience for one browser's player, not an authority about play on other machines. The storage parser checks supported sizes and plausible bounded integer scores; it does not authenticate the person who produced them.

That limitation was acceptable because the product does not depend on trusted remote standings. Treating local data as untrusted also helps ordinary reliability. A malformed preference value falls back to usable defaults instead of preventing a deal. An unavailable storage object leaves the current game playable. [Persistence overview](persistence-overview.md) describes the operational detail.

## Alternatives considered

A thin score service looked superficially small, but it would have needed a definition of identity, abuse handling and trustworthy results. Merely accepting a move count posted by a browser would add networking without adding integrity. A server-generated deck would also complicate opening the game offline and reproducing a deal outside the UI.

A hosted-only static site would avoid backend logic but still make the launch path depend on a reachable origin. The chosen build can be hosted as a file and can also be retained locally. Hosting is a distribution option, not a prerequisite embedded in the rules.

## Consequences for engineering

Debugging relies on documented board codes, snapshots, replay actions and browser artifacts. It does not rely on inspecting a server session. The pure game functions make that a strength: a reported action sequence can run in Node with the same matching behavior. [ADR 015](adr-015-replay-cli.md) records that diagnostic path.

There is no remote recovery promise for an unexported in-memory game. A page reload starts a fresh deal, using a saved supported size when one is available. This follows the current session model rather than attempting to infer a cloud-save feature from local preferences. The absence of a backend also informs [ADR 038](adr-038-no-analytics.md), but that decision has its own reasons about what the game observes.

## Local preference data

The stored model is deliberately small. The adapter interface permits failure tests without turning the game core into a storage client.

```ts
export const DEFAULT_SIZE = GRID_SIZES[0];
export const PREFERENCES_KEY = 'neon-pairs.preferences.v1';
export const PREFERENCES_VERSION = 1;
export interface Preferences {
  readonly size: number;
  readonly bestMoves: Readonly<Record<string, number>>;
}
export interface PreferenceStore {
  getItem(key: string): string | null;
  setItem(key: string, value: string): void;
}
```
