---
open-forge:
  description: Accepted immutable-ledger decision forbidding hard deletion and requiring append-only reversal
  tags: [Memory, Decision, Storage, CurrentTruth, Ledger, Removal]
---

# Immutable Ledger

## Decision

Transactions are immutable and append-only. A mistake is corrected by appending a reversal with the opposite amount, category `correction`, and a reference to the original transaction. No API endpoint, CLI command, or storage operation may hard-delete or rewrite a transaction.

## Rationale

Historical summaries must remain reproducible. A prior delete feature silently changed reports that users had already reviewed, while reversals preserve both the correction and its cause.

## Consequences

- The API uses `POST /transactions/{id}/reverse`; it has no `DELETE` route.
- List output shows reversals and their original references.
- Summaries include reversal amounts so the original and reversal net to zero.
- Reversing a missing transaction or an existing reversal is a user-facing error.
