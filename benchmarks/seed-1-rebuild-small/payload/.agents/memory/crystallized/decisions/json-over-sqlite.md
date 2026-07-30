---
open-forge:
  description: Prefer a local JSON file over SQLite or other database storage
  tags: [Extension, Memory, Decision, Storage, Data, CurrentTruth]
---

# JSON Over SQLite

## Decision

Persist bookmarks in a local JSON file, not SQLite, IndexedDB, or another database.

## Rationale

SQLite would be technically stronger for querying, concurrent access, and future growth. It is still the wrong default here because maintainers want a tiny, inspectable CLI where the data file can be opened, diffed, copied, and understood without tooling.

The project has no need for relational queries, migrations, or large datasets.

## Consequence

Make JSON handling robust instead of replacing it with a database:

- validate shape,
- handle corrupt files clearly,
- write safely,
- document the lack of cross-process locking.
