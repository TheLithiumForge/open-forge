# ADR 011: Version local keys by data family

This historical record explains the original dotted storage keys and the reasoning behind the preferences key.

Status: Superseded by [ADR 058: storage namespace](adr-058-storage-namespace.md)

## Original setting

The first persistent data was small: a chosen grid size and local best move counts. It lived beside other applications on a browser origin, so a generic key such as `settings` would have been easy to collide with. The storage discussion treated key naming as a way to identify ownership and make saved data recognizable during debugging.

The decision at that time was that stored data uses keys of the form `neon-pairs.THING.v1`. The prefix identified the game, the middle segment identified the data family and the suffix identified the stored format generation. The existing preferences key, `neon-pairs.preferences.v1`, follows that decision. Its name was intended to be understandable in browser storage tools without requiring a separate registry.

## Why that structure seemed sufficient

Combining size and best moves in one preferences value made the initial storage surface easy to inspect. A reload could read one JSON document and either accept its supported values or return a default size with no best records. The document also carried `schemaVersion: 1`, so the reader could check the structure after locating the key.

The duplication between a versioned key and a version field was deliberate. The key selected a family of stored data; the field protected the parser from an unrelated or damaged value placed there. A recognizable name did not establish that the JSON was valid. [Preferences store](preferences-store.md) describes the actual parser.

## Alternatives considered at the time

One unqualified key for the whole application would have been shorter. It would also have hidden ownership and made unrelated applications sharing an origin more likely to reuse the name. A separate key for each score would have made individual entries easy to inspect but spread validation and writes over several values.

The chosen envelope kept the selected size and all per-size bests together. It did not store active progress. That distinction kept routine preference writes from becoming an implicit saved-game mechanism. Exported snapshots already had their own explicit representation and did not need to be mixed into settings merely because they were JSON.

## Failure behavior was part of the reasoning

The storage adapter exposed `getItem` and `setItem`, allowing a fake store to throw during tests. Parsing, access and writing could all fail without preventing the in-memory game from proceeding. Reading a versioned key therefore did not imply that persistence was always available. The browser origin still controlled access, and direct disk opening had browser-dependent storage behavior.

The reader checked the saved side length against the supported catalogue and checked score entries against the move limits. A key name was not used as a substitute for these checks. Similarly, ownership of one key gave no reason to clear other origin data when a value was rejected.

## Historical consequence

This record explains why a dotted versioned preferences key exists in the current source. It is not the naming authority for additional stored families. The replacement decision is [ADR 058](adr-058-storage-namespace.md); it addresses new names without requiring the current preferences value to be renamed.

That distinction prevents a maintenance error in which a developer reads an old record, treats its original rationale as current policy and spreads the retired naming scheme. It also preserves the useful part of the history: the preferences name was a considered design choice, not an arbitrary leftover. [Persistence envelope](persistence-envelope.md) covers the current value and [Corrupt data policy](corrupt-data-policy.md) covers rejection behavior.

## The surviving preferences identifier

This is present-day evidence of the original decision’s effect. It is not evidence that every future data family follows the old scheme.

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
