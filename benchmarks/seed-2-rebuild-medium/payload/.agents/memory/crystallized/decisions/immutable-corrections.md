---
open-forge:
  description: Entries are immutable; amend appends a correction record instead of editing history
  tags: [Extension, Memory, Decision, Storage, Semantics, CurrentTruth]
---

# Immutable Corrections

## Decision

Logged entries are never modified or deleted in place. `amend <id> <text>` appends a correction record referencing the original id. Reads resolve to the latest correction; the original stays on disk.

## Rationale

An in-place edit in the MVP silently destroyed what an entry originally said, and the user missed it. Append-only corrections preserve the audit trail, keep the storage decision honest (no rewrites), and make `amend` crash-safe for free.

## Consequences

- The record shape needs a way to distinguish original entries from corrections and to reference the corrected id.
- `list`/`report` show only the resolved (latest) text; a raw view of history is out of scope.
- Amending a nonexistent id is a user error.
- There is deliberately no `delete` command in this rebuild; if an entry was a mistake, amend it to say so.
