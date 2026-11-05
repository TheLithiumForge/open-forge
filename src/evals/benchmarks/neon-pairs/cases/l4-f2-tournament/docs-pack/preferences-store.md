# Preferences store

This page is a code guide to `storage.ts` and `use-saved-preferences.ts`, including the session transitions that feed their values.

The store is deliberately small. It reads one aggregate at initialization and writes the current preference object through a React effect. It does not subscribe to browser changes, own the active game or provide a general repository abstraction. Its public functions are direct enough that most persistence bugs can be localized to a reader, a session transition or a writer.

## Files and ownership

The browser-independent value operations live in `src/game/session/storage.ts`. That module exports `DEFAULT_SIZE`, `PREFERENCES_KEY`, `PREFERENCES_VERSION`, the `Preferences` and `PreferenceStore` interfaces, and three functions: `readPreferences`, `writePreferences` and `rememberBest`.

The browser adapter and save effect live in `src/game/session/use-saved-preferences.ts`. It exports `browserStore` and `useSavedPreferences`. `src/game/session/use-game.ts` ties those exports to the mounted session. `src/game/session/session.ts` decides when the selected size or best map changes.

`PreferenceStore` is a structural interface:

```ts
export interface PreferenceStore {
  getItem(key: string): string | null;
  setItem(key: string, value: string): void;
}
```

A native `Storage` object satisfies those methods, while a unit test can supply a map-backed object. The interface does not expose deletion, enumeration, transactions or event subscriptions. The implementation consequently does not need a browser to test its value logic.

## Constants and in-memory data

`PREFERENCES_KEY` is `neon-pairs.preferences.v1`. `PREFERENCES_VERSION` is 1. These are separate constants used for key selection and the JSON discriminator respectively. `DEFAULT_SIZE` is `GRID_SIZES[0]`, currently 4, so the fallback follows the size catalogue's first entry rather than repeating an independent literal inside the reader.

The in-memory shape is:

```ts
export interface Preferences {
  readonly size: number;
  readonly bestMoves: Readonly<Record<string, number>>;
}
```

`readonly` expresses the intended TypeScript use. It does not validate strings read from the browser and does not freeze every object at runtime. The reader constructs trusted values after checking unknown parsed input. Session transformations create replacement objects rather than modifying the existing map.

A size field can be any TypeScript number at this type level. A score map can also be constructed with arbitrary string keys by a caller. The runtime checks in `readPreferences` are what establish supported sizes and plausible completed counts for browser input. `writePreferences` and `rememberBest` rely on their callers; they do not repeat the whole decoder.

## Reading the aggregate

`readPreferences(store)` accepts either a store or `null`. It prepares the fallback, then reads `PREFERENCES_KEY` inside a catch boundary. No text returns the fallback. Otherwise it parses JSON into `unknown` through `parseJson` and narrows the result before reading expected fields.

The selected size must be numeric and supported. `bestMoves` must be a non-null non-array object. Every entry must have a canonical supported size key and a nonnegative safe integer move count between that size's pair count and `MOVES_MAX`, inclusive. One rejected entry rejects the complete aggregate. The successful result contains the accepted selected size and a newly built map.

The module imports `isValidSize` from `deck.ts`, `MOVES_MAX` from `game.ts`, and JSON and integer helpers from `saved-format.ts`. This ties the preference constraints to actual game values. The score maximum is 999999; the minimum is computed from the entry's size, independently of the currently selected size.

## Writing the aggregate

`writePreferences` calls the optional store's `setItem` with the key and a JSON string built from `{ schemaVersion: PREFERENCES_VERSION, ...preferences }`. For the actual `Preferences` objects supplied by the session, the stored properties are `schemaVersion`, `size` and `bestMoves`. The writer does not pretty print, attach dates or serialize the rest of the session.

The `try` block catches both serialization errors and store errors. A null store skips `setItem`; a thrown write is swallowed. The function returns no status in either case. Callers therefore cannot infer durability from a completed call. The returned type is `void`, not a saved-value receipt.

The writer does not compare with the string already in storage and does not read before writing. It serializes the whole aggregate that the tab currently knows. It neither merges another tab's score map nor preserves arbitrary extra properties from a previously accepted value. Only this one application key is touched.

Because the writer is not a validator, a direct unit call can serialize an invalid `Preferences` object. Reading that string afterward may reject it. That is not a contradiction: one function encodes trusted application state, and the other validates untrusted saved input. Meaningful round-trip tests use a valid baseline and separate rejection tests exercise hostile or accidental edits.

## Calculating a best

`rememberBest(preferences, size, moves)` looks up `preferences.bestMoves[String(size)]`. If an earlier value exists and is no greater than the submitted count, it returns the original preferences object. A first or strictly better result returns a new object whose map spreads the previous entries and replaces the submitted size's count.

For a baseline `{ size: 6, bestMoves: { "4": 10, "6": 20 } }`, submitting `(4, 9)` yields a 4x4 best of 9 while leaving the selected size at 6 and its best at 20. Submitting `(4, 10)` or `(4, 11)` returns the baseline itself. The function does not select the size it updates; the session's board replacement path owns selected size.

Object identity has a practical consequence for the hook. Equal and worse submissions do not create a changed preference dependency merely by finishing a game. A better result does. The unit test explicitly checks identity for equal and worse counts, not just deep equality of their contents.

The function performs no browser operations and has no knowledge of completion. `activateSession` calls it when an activation produces a win. Calling it directly in a test is useful for minimum-selection logic but does not prove that the session calls it at the correct transition. That behavior has separate session and browser evidence.

## Acquiring the browser store

`browserStore` attempts to return `window.localStorage` and catches property-access errors, returning `null`. This catch is distinct from the catches around `getItem` and `setItem`, because the getter itself can throw before either method is available.

`useGame` uses `const [store] = useState(browserStore)`. React invokes that initializer to obtain the store value, rather than storing the function as the value. The mounted hook then uses that captured object or null. It does not reacquire storage on every clock tick or card activation.

Session initialization reads preferences from that store and creates a fresh board at their size. A `useRef` carries the latest session for clock and input coordination; it is not a durable storage cache. Replacing the board changes session state, while browser persistence remains a later effect on the preferences portion.

## The effect's actual cadence

`useSavedPreferences` invokes `writePreferences` in a `useEffect` dependent on `[preferences, store]`. Effects run after rendering, including the initial mount. There is no debounce interval, end-of-page listener or pending-save object. The hook has no cleanup because it schedules no continuing background operation.

Ordinary elapsed-time updates preserve preference identity. A card reveal before completion does too. The effect therefore does not serialize the running game at every tick or turn. It can still run after every accepted board replacement, since `replaceGame` constructs a new preferences object even if the selected numeric size is unchanged.

For invalid load attempts, `notice` changes feedback and announcement state while preserving preferences. [New game and restart](new-game-and-restart.md) and [Snapshot selection notes](snapshot-selection-notes.md) discuss the accepted replacement paths and their session behavior.

The effect follows normal React dependency semantics rather than promising a fixed global write count. A test can assert that ordinary ticking does not alter preference identity, but broad expectations about exactly one write across every development mounting pattern are more brittle than the product behavior being checked.

## Limits worth preserving in an investigation

There is no `storage` event listener in this adapter. A tab reads at initialization, keeps its own preference snapshot and writes that aggregate later. Multiple-tab reconciliation is outside the current supported specification. A second tab's newer value is not automatically reflected in the first tab's displayed best.

There is also no availability state, error callback or queue of failed writes. A browser object with readable old data and denied writes can produce a current in-memory best that reverts to the older value after reload. A null store simply makes later writes no-ops for that mounted session.

The source's compact API makes these limits visible rather than hiding them behind a large persistence layer. [Storage failure policy](storage-failure-policy.md) covers unavailable-storage behavior, and [Storage testing guide](storage-testing-guide.md) maps each exported operation to tests that can isolate it.
