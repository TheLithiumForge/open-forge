---
open-forge:
  description: Keep context recall and source-backed answers read-only
  tags: [LoadNow, Directive, Recall, ReadOnly]
---

# Read-Only Recall

## Axioms

- When the task asks only to recall, summarize, or locate existing truth, do not mutate workspace files, generated state, dependencies, external systems, or source records.
- Use only inspections and checks already known to preserve the observed state; report a limitation when trustworthy recall would require mutation.
