---
open-forge:
  description: CSV imports are all-or-nothing with a per-row rejection report
  tags: [Extension, Memory, Decision, Import, Validation, CurrentTruth]
---

# CSV Import Strictness

## Decision

`import` validates every row before writing anything. If any row fails, the entire import is rejected (API 422 `import-rejected`) with a report listing each bad row's number and problem. Only a fully valid file is appended, atomically from the user's perspective.

## Rationale

A partial MVP import left 147 of 312 rows in and no sane way to reason about which. All-or-nothing plus a fix-and-retry loop is strictly easier for a human.

## Consequences

- Expected columns: `date,amount,category,description` with a header row; amount parsing follows the money decision (string to minor units, no floats).
- The row-error report is the product here — it must name row numbers and concrete problems, not "invalid CSV".
- Duplicate detection inside the file (same derived id) is a row error, not a silent dedupe.
