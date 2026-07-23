---
open-forge:
  description: Unvalidated idea considering a hard-delete shortcut for ledger removal
  tags: [Memory, Idea, Ledger, Removal, Contextual]
---

# Hard-Delete Shortcut Idea

Considered: `ledger remove <id>` could just hard-delete the row directly via `DELETE
/transactions/{id}`, skipping reversal bookkeeping, to save implementation time.

## Status

Not adopted. Raised as a possible shortcut only; it has not been checked against audit,
dispute-resolution, or reporting-reproducibility needs. Do not treat this as settled
removal behavior -- validate it against those needs, and against any accepted decision
on removal, before relying on it.
