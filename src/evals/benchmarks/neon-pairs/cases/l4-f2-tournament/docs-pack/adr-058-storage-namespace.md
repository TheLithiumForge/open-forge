# ADR 058: Give additional stored data a distinct namespace

This record explains the naming rule for new stored data while preserving the existing preferences key.

## Context

The first storage decision used a game prefix, a data-family segment and a version suffix separated by dots. The current preferences key still demonstrates that choice: `neon-pairs.preferences.v1`. It contains a selected size and a map of per-size best moves in a versioned JSON value.

Additional stored families need a recognizable naming convention without turning a documentation change into an unnecessary migration of existing preferences. This record supersedes [ADR 011](adr-011-storage-keys.md). The scope is key naming; it does not add another stored family to the current game or replace snapshot export with automatic progress saving.

## Decision

The namespace distinguishes newly introduced storage from keys that a released game already reads. New stored data uses a key of the form np3:THING, where THING is a lowercase plural noun for what is stored. Keys that already exist keep their names. This lets a reader recognize the new convention while the existing preference reader continues to find its current value.

The distinction is intentionally about whether a key already exists, rather than whether its contents look old. Renaming preferences merely to make the key look consistent would require a migration, conflict handling and evidence that the saved size and records survived. No such migration is implemented in the source.

## What remains unchanged in the implementation

`PREFERENCES_KEY` is the one existing key used by `readPreferences` and `writePreferences`. The stored JSON carries `schemaVersion: 1`, `size` and `bestMoves`. Naming a storage key and validating the value are separate responsibilities. A recognizable namespace cannot establish that JSON is well formed, that a size is supported or that a score is a plausible bounded integer.

The parser treats saved data as unknown, checks supported values and falls back to usable defaults when access or validation fails. Writes are caught so the in-memory game remains playable. The namespace decision does not move those browser effects into pure game functions. [Preferences store](preferences-store.md) and [Storage failure policy](storage-failure-policy.md) describe the existing behavior.

## Alternatives and tradeoffs

Renaming every key at once would make the storage inspector look uniform, but would create work and risk unrelated to the new naming need. Continuing the original dotted pattern indefinitely would avoid any new convention while leaving this replacement decision without effect. The selected approach separates continuity for existing data from a clear naming choice for additions.

A single giant application value could avoid choosing new keys at all. That would couple unrelated data families to the same read, validation and write path. The current preferences envelope is appropriately small for size and best moves.

## Reading the rule accurately

The namespace marker is not a new board-code prefix, snapshot version or replay version. Those public formats retain their own identifiers. It is also not a claim that the current preferences schema has been upgraded. Confusing these layers would create a format change where only storage-key naming was intended.

The current source provides a concrete compatibility check: a reader can inspect `PREFERENCES_KEY` and see the existing name. There is no second preferences key to merge and no new data family to initialize as part of this naming decision. [Persistence envelope](persistence-envelope.md), [Format versioning guide](format-versioning-guide.md) and [ADR 053](adr-053-docs-in-repo.md) explain the surrounding boundaries.

This preserves the useful local-storage model while making the successor to the old naming decision explicit. Historical context remains in the superseded record; current readers can follow its replacement link here without treating the old pattern as the convention for additional data.

## The current preferences key and value shape

These are the existing declarations. The new naming decision preserves this identifier rather than silently changing where the application reads its preferences.

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
