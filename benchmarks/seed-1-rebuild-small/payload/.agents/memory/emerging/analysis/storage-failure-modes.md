---
open-forge:
  description: Analysis of local JSON storage failure modes from the MVP
  tags: [Extension, Memory, Analysis, Storage, Reliability, Contextual]
---

# Storage Failure Modes

The local JSON file is the user's data. Treat it as important even though the project is small.

## Failure Modes To Consider

- Missing file: should mean empty store.
- Malformed JSON: should be a user-facing error, not silent reset.
- Wrong shape: should be a user-facing error, not silent reset.
- Interrupted write: should not leave an empty or partial main data file when avoidable.
- Concurrent writes: can lose updates without locking; document this limitation rather than overbuilding.
- Path override: tests should use an environment override to avoid touching a real user file.

## Review Note

An implementation that passes happy-path CLI tests but silently overwrites corrupt data should not be considered high quality.
