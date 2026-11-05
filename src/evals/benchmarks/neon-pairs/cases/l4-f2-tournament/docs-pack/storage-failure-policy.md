# Storage failure policy

This page describes how unavailable browser storage affects play and how saved-record features present that limitation.

The current game isolates storage failures from the active game. It catches errors while obtaining the browser store, reading the preference key and writing its replacement. The session can hold its selected size and best results in memory even when no durable write succeeds. There is no current preference-storage warning in the rendered game.

## Features with saved records

For a feature that presents saved records, the unavailable-storage state belongs near the data it affects. The active interaction can still use its current in-memory state even though later visits cannot rely on finding that state again. When browser storage is blocked, a feature keeps working in memory and shows "Progress won't be saved on this device." where its saved records would appear. This ties the explanation to the missing persistence rather than turning it into a failure of the whole game.

The preference adapter does not yet expose a blocked-status result. `readPreferences` returns a `Preferences` value in all cases, and `writePreferences` returns `void`. Neither return type carries an availability flag or failure reason. `Game.tsx` renders no preference-storage warning; saved-record features have separate failure feedback.

The existing status display remains driven by session state. A best achieved during a visit can appear there after a failed write because it is already in `session.preferences`. A visible best is evidence of the in-memory result; persistence across a later reload needs separate evidence. [Persistence overview](persistence-overview.md) describes the data boundary.

## Three browser operations can fail independently

The first operation is acquiring the storage object. `browserStore` in `src/game/session/use-saved-preferences.ts` returns `window.localStorage` inside a `try` block. If the property getter throws, the function catches the exception and returns `null`. A denied property getter is tested in `tests/browser-environment.e2e.test.ts` by defining a getter that throws a `SecurityError` before the game initializes.

The second operation is reading the key. A browser can expose an object whose `getItem` later throws. `readPreferences` protects that call and returns `{ size: DEFAULT_SIZE, bestMoves: {} }` on failure. Its optional call also accommodates a `null` store. A successful read can return no value, a malformed string or valid preferences; those are separate outcomes from a thrown read.

The third operation is writing a serialized replacement. `writePreferences` encloses both serialization and `setItem` in its `try` block. A thrown error is swallowed so it cannot interrupt play. A browser may accept reads and refuse writes, so successful startup does not establish successful saving. Quota exhaustion is one possible write failure; browser policy is another.

These stages explain why a single boolean based on the existence of the storage property would be a weak diagnostic. The game doesn't use such a probe. It attempts the real operation at the boundary and catches failure there.

## What the current fallback preserves

Storage failure does not bypass the game's own rules. Card eligibility, move counting, matching, elapsed time and completion still use their normal state transitions. The fallback affects the initial preference value and the durability of subsequent changes. It does not substitute a different deck algorithm or change how many moves a result takes.

After a denied startup read, the game starts at the default size with no saved bests. Choosing another supported size updates the active session. Completing a game can produce a best for that size in memory. New Game and Restart preserve those in-memory preferences because `replaceGame` copies the session preference object rather than rereading the browser store.

If storage remains unavailable, a page reload starts another session from the fallback. The earlier in-memory best is gone with the old page. This is the expected consequence of unsuccessful saving, even though play in the earlier page was complete and valid. A report that a score survived Restart but disappeared on reload is consistent with a write failure.

An imported unfinished snapshot remains usable under the same conditions. The snapshot text is decoded into game state, then the session handles pause and resume. That path does not require a successful preference write. The accepted board size may be retained only in the current session if the browser refuses the later save.

## No implicit retry service

The hook is a small effect:

```ts
useEffect(() => {
  writePreferences(store, preferences);
}, [preferences, store]);
```

There is no queue, timer-driven retry, backoff or unload handler. Another dependency change can cause another attempt, but the code does not poll for a browser setting to change. A returned `null` store is captured in state for the lifetime of that mounted game; enabling storage later does not replace that state value automatically.

If the captured store object exists and its methods change behavior, a later effect can succeed using the same object. For example, a test double can reject its first write and accept a later one. That is different from reacquiring a previously inaccessible property. Both cases can be modeled without making the game rules aware of browser error names.

The writer's lack of a success result also means the existing UI cannot distinguish a rejected write from a successful one. This is a deliberate limit of the current adapter, not a hidden confirmation mechanism. Any investigation that needs that distinction can observe the supplied store in a test.

## Read failure and corrupt data have different evidence

A denied read and an invalid JSON value both lead to fallback preferences. Only the former establishes that an operation could not be completed. A parse failure establishes that the browser returned text which the application could not use. Confusing the two can lead to a misleading message about browser settings when the actual issue is an edited value.

Similarly, a quota error does not mean the existing value is invalid. The old string may remain perfectly readable while the new result exists only in memory. A person may see a better current best and then see the older best after reload. That sequence is a useful way to reproduce a failed update without damaging a valid baseline.

[Corrupt data policy](corrupt-data-policy.md) follows the rejection branches in detail. [Storage quota notes](storage-quota-notes.md) explains why the small size of this app's own payload cannot establish that a shared origin has available space.

## Errors do not authorize deleting other data

The source only calls `getItem` and `setItem` for `PREFERENCES_KEY`. It has no storage-wide clear call, key enumeration or cleanup routine. `PreferenceStore` exposes only those two methods. The storage unit test explicitly seeds an unrelated key and checks that a preference write leaves its value intact.

This matters in shared local environments, where another page can occupy the same origin. A rejected preference write does not cause Neon Pairs to free space by deleting that page's values. The current fallback is simply to continue without a successful save. The app also does not switch to cookies, a server endpoint or another browser database after failure.

When reproducing capacity problems, a disposable origin or isolated browser context keeps artificial fill data away from a person's real preferences. That is a testing technique rather than an extra product recovery path. The test can remove the keys it created afterward; the shipped game contains no such test cleanup behavior.

## Evidence in the test suite

`storage.test.ts` uses a denied store whose read and write methods both throw. It checks that reading yields the default size and that writing does not throw. It also covers `readPreferences(null)`. This verifies the API boundary directly, without needing to configure an actual browser's privacy settings.

The browser environment test covers a different boundary: acquisition of `window.localStorage`. It installs the throwing getter before loading the game, loads a fixed board and matches a known pair. The harness gathers uncaught page errors and checks that none occurred. That demonstrates continued play after an acquisition failure, not persistence in that denied context.

These tests don't establish every browser policy or a physical disk limit. They also don't assert the saved-record feature notice described earlier. Reading the test title alone can obscure those limits; the actual injection point and assertions show what each test proves. [Storage testing guide](storage-testing-guide.md) extends that evidence into an investigation workflow.

