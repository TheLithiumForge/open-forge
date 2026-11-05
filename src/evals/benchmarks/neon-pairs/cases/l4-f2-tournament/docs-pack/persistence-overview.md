# Persistence overview

This page explains the small amount of browser data Neon Pairs saves and the boundary between remembered preferences and an active game.

The game has one application storage key, `neon-pairs.preferences.v1`. Its value remembers the selected board size and the lowest completed move count for each size with a record. It does not contain a saved game. Reloading the page reads those preferences and creates a fresh deal at the remembered size. This is the most useful distinction to establish before investigating a report that something disappeared.

## The durable part of a visit

`src/game/session/storage.ts` defines the persistence data and its reader and writer. The in-memory `Preferences` interface has two properties: `size`, a number, and `bestMoves`, a read-only mapping from size strings to move counts. The writer adds `schemaVersion: 1` when serializing it. A representative stored value is:

```json
{"schemaVersion":1,"size":6,"bestMoves":{"4":9,"6":20}}
```

Here the next page load starts a new 6x6 game. The best displayed while playing that size is 20. Choosing 4x4 exposes the independent best of 9. There is no record for 8x8, so the status displays `none` for that size. The selected size does not have to be the only size represented in the score map.

The list of supported side lengths comes from `GRID_SIZES` in `src/game/deck.ts`: 4, 6, 8, 10, 12, 14 and 16. `DEFAULT_SIZE` uses the first entry, currently 4. A missing or rejected preference value therefore produces a playable 4x4 start with an empty score map. Storage is useful for continuity between visits, but the game does not depend on it to construct a board or resolve a turn.

See [Preferences store](preferences-store.md) for the exact validation and React effect. [Best score records](best-score-records.md) covers the gameplay meaning of the numbers rather than the browser mechanism that holds them.

## What remains in this page's memory

The active session includes the game, pause state, session generation, feedback, load notice, announcement counter, celebration flag and preferences. Only the preference portion reaches `writePreferences`. The session's game contains the board description, deck, matched positions, selection, moves, active elapsed time and started flag. None of that game progress is automatically written to browser storage.

This has several visible consequences:

- Reloading during a mismatch produces a fresh board rather than restoring the two exposed cards.
- Reloading after a win retains an eligible best, but it does not restore the completed board or its banner.
- Restart repeats the current deal only within the active session. Reload is a separate operation and starts a fresh deal.
- A loaded snapshot can change the remembered size even though its matched cards and elapsed time will disappear on reload.
- A draft in the reproduction controls is not a browser save. Typing text there does not itself create a durable preference change.

These are ordinary consequences of the data boundary, rather than separate recovery modes. A screenshot showing a nonzero move count says nothing about whether that count was stored. A storage dump showing a best says nothing about the last board code. For progress transfer, the explicit snapshot controls are the relevant tool; see [Snapshot format](snapshot-format.md) and [Reproduction panel](reproduction-panel.md).

## When preferences change

`useGame` obtains a browser store once through `useState(browserStore)`. Its session initializer calls `readPreferences(store)` and then `initialSession(freshBoardCode(preferences.size), preferences)`. The size affects board creation, while the score map is retained alongside the new game. `useSavedPreferences(store, session.preferences)` connects that object to the writer after rendering.

Replacing a game goes through `replaceGame` in `src/game/session/session.ts`. It makes a new preference object with `size: game.size`, preserving the existing score map. This path is shared by changing size, New Game, Restart, a valid board-code load and a valid snapshot load. Consequently, replacing a game can cause a preference write even when the selected size has not numerically changed. The effect follows object identity; it does not compare serialized strings.

Invalid board-code and snapshot loads take an earlier exit. They update feedback through `notice`, leaving preferences alone. A rejected draft therefore does not alter the last accepted size. The browser test `valid board loads save their size while invalid loads keep that preference` exercises this distinction with a valid 6x6 board followed by invalid text and a reload.

During ordinary turns, the session continues carrying the same preferences object. On completion through card activation, `activateSession` calls `rememberBest`. A first result for a size or a strictly better result makes a new score map and preference object. Equal and worse counts return the original preference object. Clock ticks and intermediate reveals change game state without scheduling preference writes merely because time or moves changed.

## Why the stored value stays small

The preferences answer two recurring questions: which size did this browser use, and what is its best completed move count for this size? They can answer both without retaining every attempt. There is no attempt log, record timestamp, saved deck, player identity, sound option or theme preference in the current schema. There is also no automatic snapshot written on every move or on page exit.

This keeps browser effects out of game rules. `rememberBest` is a pure transformation; the session decides when it applies, and the hook performs the browser write later. Storage denial cannot stop a match, because matching does not ask storage for permission or wait for a successful write. A page can remain fully playable with the store represented by `null`.

The small payload also limits synchronous work. `localStorage` uses a synchronous API, but this implementation serializes a compact aggregate instead of the whole active board repeatedly. It does not store artwork, assets or the distribution HTML there. Offline delivery comes from the built file, not from browser preference storage. [Offline delivery](offline-delivery.md) describes that separate concern.

## Locality and lifetime

The storage belongs to the browser context and origin that opened the game. A second browser profile has its own data. A different HTTP port can put an otherwise identical build in another origin. Moving between direct disk delivery and a local HTTP address is therefore not a reliable way to carry preferences over. The technology specification uses a local HTTP origin for saved-preference checks because disk URL storage behavior varies.

The app does not synchronize preference changes between open tabs. Each tab reads its initial value and then writes its own in-memory aggregate. One tab can overwrite another tab's changes; the specification explicitly leaves several simultaneous game windows outside the supported journeys.

Browser policy, explicit site-data deletion and private-session cleanup can also end a stored value's lifetime. The app has no server copy from which to recover it. [Private browsing notes](private-browsing-notes.md) and [Storage quota notes](storage-quota-notes.md) separate these environmental issues from the application's own replacement behavior.

## Reading this group of pages

The existing preference representation is older than the feature-level storage conventions described in [Persistence envelope](persistence-envelope.md). The existing writer continues to emit the preference aggregate. Likewise, [Retention policy](retention-policy.md) concerns saved lists; the current best-score map is an aggregate, not an attempt list.

For failures, begin with the operation that failed. Accessing `window.localStorage`, reading a key, parsing its string and writing a replacement are distinct stages. [Storage failure policy](storage-failure-policy.md) covers unavailable storage and the feature behavior built around it. [Corrupt data policy](corrupt-data-policy.md) covers values that can be read but cannot be accepted. Both cases lead to usable in-memory state, but their evidence and timing differ.

[Storage testing guide](storage-testing-guide.md) maps the unit and browser evidence to the boundary being investigated. It also explains how a served origin, isolated context and operation-specific store double make a persistence report reproducible.
