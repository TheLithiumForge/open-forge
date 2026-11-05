# Persistence envelope

This page describes the feature storage envelope and distinguishes it from the preference format already implemented in Neon Pairs.

The source currently serializes a preference aggregate, not a collection of feature records. Its key is `neon-pairs.preferences.v1`, and its discriminator is `schemaVersion: 1`. The existing reader and writer continue to use that preference format; values already stored in browsers require a code migration to change shape.

## The feature value boundary

A feature's stored collection has an outer representation and a collection of domain records inside it. The envelope carries the format identity at that outer boundary, before any individual record can be interpreted. Every value a feature adds to browser storage is a JSON object with exactly two keys: "v", the number 3, and "items", the array of records. The records carry the feature's data, while the outer object identifies the representation used to hold that collection.

This makes a feature value inspectable without relying on the name of a variable in the UI. A reader can distinguish an outer format mismatch from a problem in a particular record. The envelope is separate from the record schema, storage key, ordering field and identity scheme. The current preferences implementation stores a selected size and a best-score map rather than feature records.

The envelope also does not change board-code, snapshot or replay formats. Those are interchange representations with their own version identifiers and validation. A snapshot copied by the player is not a preference save, and a replay read by the developer command is not a browser collection. See [Format versioning guide](format-versioning-guide.md) and [Data exchange policy](data-exchange-policy.md) when following a value across those boundaries.

## The existing aggregate, precisely

The current storage writer does this:

```ts
JSON.stringify({ schemaVersion: PREFERENCES_VERSION, ...preferences })
```

`PREFERENCES_VERSION` is 1. With the ordinary session value, this produces `schemaVersion`, `size` and `bestMoves`. For example:

```json
{"schemaVersion":1,"size":8,"bestMoves":{"4":8,"8":40}}
```

This object remembers a selected 8x8 size, a 4x4 best of 8 moves and an 8x8 best of 40. The score map is keyed by board size rather than represented as a sequence of attempts. It has no recency information. Its version number is not inferred from the suffix on the storage key: the reader checks the numeric discriminator inside the value as well.

There is no feature-envelope serializer or decoder in `src/game/session/storage.ts`. `readPreferences` expects the existing aggregate, and feeding it a feature-shaped collection returns the default preference state. Conversely, no current browser code loads feature records and turns them into the preference map. The two representations have different jobs and are not interchangeable encodings of the same object.

[Preferences store](preferences-store.md) documents the complete aggregate reader. [Persistence overview](persistence-overview.md) explains why active progress is absent from that value.

## What the preference reader actually checks

`readPreferences` starts with `{ size: DEFAULT_SIZE, bestMoves: {} }` as its fallback. It obtains the stored string, passes it through `parseJson`, requires a non-null non-array object, checks the version and selected size, and then checks the score map. A single rejected map entry returns the fallback for the whole preference value.

The object checks are structural, not class checks. `isJsonObject` in `src/game/saved-format.ts` accepts an object if it is non-null and not an array. That helper alone says nothing about which fields are present. The reader supplies the field-specific checks after narrowing the parsed value.

There is a useful asymmetry here. Snapshot validation has an exact-key helper available in the shared format module, but the preference reader does not call it. Extra top-level preference properties are ignored if the fields the reader uses are valid. A successful read returns a freshly constructed `Preferences` value containing only `size` and the validated score map. The current preference format accepts extra top-level properties.

The feature envelope's boundary should not be inferred from that lenience. Its stated outer shape is its own contract. A convenience helper existing nearby does not decide which parser uses it or what a particular version accepts.

## Version labels answer different questions

The project has a product specification version, a preference schema version and explicit interchange format versions. They are not one counter. The current specification identifies itself as 2.2 while the preference discriminator remains 1. A feature envelope uses the numeric version described in its own boundary. A board code's prefix describes its generation algorithm rather than the layout of browser preference JSON.

Keeping those meanings distinct avoids a common support mistake: assuming a newer app release must reject every older saved object. The preferences are accepted according to `PREFERENCES_VERSION` and field validation. The product release number is not read from storage, compared to storage or written into the preference object.

There is no migration chain in the preference store. An unsupported discriminator produces the same fallback as a malformed value. No code attempts to guess an older shape, rename its fields or convert a saved sequence into a best map. A migration requires a code change with observable effects.

## A value moves through several representations

It helps to follow one ordinary preference update from the UI rather than treating storage as a single operation. Selecting a different grid size creates a new game. `replaceGame` copies the session preferences and changes `size` to the new board's side length. React renders using that state. The save effect then calls `writePreferences`, which constructs the versioned object, serializes it and passes the string to the supplied store.

The domain state is therefore not the same thing as the serialized object. `Preferences` has no `schemaVersion` field. The writer adds it at the boundary. The reader removes that boundary concern by returning a `Preferences` value with known fields. This is why callers can use `preferences.size` without repeatedly checking a schema version in components.

An envelope serves a similar boundary role for its own record collection, but there is no existing generic persistence framework to extend. The source uses direct imports and a two-method store interface. There is no registry, adapter hierarchy or global storage coordinator.

## Empty data and invalid data

An empty preference score map is valid. It means no supported size has a saved best in that aggregate. The selected size is still meaningful. By contrast, an absent score-map field, an array in its place, or a map with an impossible score invalidates the preference value. Missing storage and invalid storage reach the same fallback state through different branches.

For a feature collection, an empty array describes a collection with no records. [Corrupt data policy](corrupt-data-policy.md) deals with rejection and replacement; it is the place to examine how a failed read interacts with later saving.

Empty data should also be distinguished from unavailable storage. A reader can return an empty state after a denied read, but that return value alone does not prove that the browser is ready to accept a future write. In the preferences API, failure information is intentionally not returned to the caller. The feature failure presentation described in [Storage failure policy](storage-failure-policy.md) consequently cannot be obtained just by inspecting an empty map.

## Inspecting a captured value

A useful investigation begins with the exact key and original string. A JavaScript object shown after parsing is already one transformation removed from the browser value. Parse failures depend on the text, while field failures depend on the parsed structure. Retaining the original string in an isolated reproduction makes these stages separable.

For preferences, compare the inner `schemaVersion` with `PREFERENCES_VERSION`, then the selected size with `GRID_SIZES`, then each score key and value with the reader's checks. A key spelled `04` is rejected even though converting it to a number yields 4. A score of 7 for a 4x4 board is rejected because eight pairs require at least eight moves. A value can be syntactically valid JSON while failing either of those semantic checks.

For a feature collection, inspect the outer representation before speculating about domain records. A test result for preference JSON is not evidence about feature-record acceptance. Stored values can share a storage mechanism while using different validators.

