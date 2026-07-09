---
open-forge:
  description: Analysis of how the MVP's date handling failed and what the rebuild must test
  tags: [Extension, Memory, Analysis, Dates, Candidate, Contextual]
---

# Date Handling Failure Modes

## Question

Why did the MVP's reports drop or misplace entries, and what must the rebuild verify?

## Evidence

- Monday `report --since yesterday` returned Sunday-start windows: Friday's work invisible at Monday standup.
- Entries logged after 22:00 local landed on the "wrong day" when local display dates were compared against UTC storage timestamps as strings.
- A DST transition week produced an off-by-one-hour window edge; nobody noticed until an entry at 23:30 vanished from a report.

## Conclusion (candidate)

The failure class is always the same: mixing the storage timeline (UTC instants) with the human timeline (local calendar days) without an explicit conversion boundary. The rebuild's window resolution should be a pure function from (`--since` token, "now" instant, local zone) to a UTC cutoff — injectable "now" and zone so tests can pin Monday/midnight/DST cases exactly.

## Limits

DST cases are approximated in tests by zone injection; a full tz-database sweep is out of scope for a personal tool.
