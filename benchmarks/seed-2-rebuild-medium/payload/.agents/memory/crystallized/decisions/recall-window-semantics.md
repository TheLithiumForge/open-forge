---
open-forge:
  description: How since-windows resolve, including laststandup and the Monday-covers-Friday rule
  tags: [Extension, Memory, Decision, Dates, Semantics, CurrentTruth]
---

# Recall Window Semantics

## Decision

Timestamps are stored as UTC ISO-8601. All `--since` window resolution happens in the user's local calendar, then converts to UTC for comparison.

- `yesterday` — start of the previous local calendar day.
- `friday` (or any weekday name) — start of the most recent such local day, looking backward.
- ISO date — start of that local day.
- `laststandup` — start of the previous working day: on Monday that is Friday; on other days, yesterday. Saturday and Sunday entries are included in Monday's window (they fall after Friday's start), so weekend hotfixes are never invisible.

## Rationale

The MVP's naive `yesterday` produced empty Monday reports, and comparing local strings against UTC timestamps dropped late-night entries. Both were real user-facing bugs, not theoretical ones.

## Consequences

- Window math must be tested around week boundaries and near midnight; those tests are not optional (see the test-contracts pattern).
- No timezone configuration: the machine's local zone is the user's zone.
- Rejected alternative: tracking an explicit "last report ran at" marker; more accurate but adds hidden state — revisit only if users ask.
