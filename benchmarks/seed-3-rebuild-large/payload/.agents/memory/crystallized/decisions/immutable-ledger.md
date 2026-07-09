---
open-forge:
  description: The ledger is append-only; mistakes are corrected by reversing entries, and no command hard-deletes a transaction
  tags: [Extension, Memory, Decision, Storage, Semantics, CurrentTruth]
---

# Immutable Ledger

## Decision

The ledger is an append-only record. Transactions are never modified or deleted once written. A mistaken transaction is corrected by appending a reversing entry (same amount, opposite sign, reference to the original id, category `correction`). No API endpoint or CLI command performs a hard delete.

## Rationale

Three months of MVP use showed that "just delete it" quietly falsified history: a summary someone had already read could no longer be reproduced. Accounting solved this centuries ago — you reverse, you don't erase.

## Consequences

- The API exposes `POST /transactions/{id}/reverse` rather than any DELETE route.
- Summaries include reversals naturally (they net to zero against the original).
- `list` shows reversals as ordinary entries referencing the original.
- The storage decision (append-only NDJSON) and this decision reinforce each other; neither works without the other.
