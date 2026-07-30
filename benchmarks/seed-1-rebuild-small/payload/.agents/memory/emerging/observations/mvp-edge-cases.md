---
open-forge:
  description: Candidate observations about edge cases that made the MVP less reliable
  tags: [Extension, Memory, Observation, MVP, Candidate, Contextual]
---

# MVP Edge Cases

## Observation

The earlier MVP shape was useful, but edge behavior needed more explicit contracts:

- equivalent URL input should not create avoidable duplicates,
- repeated add should merge tags rather than replace them unexpectedly,
- tags should normalize consistently across add, list, and search,
- corrupt JSON should not be overwritten,
- empty results should be friendly and successful,
- missing remove targets should be clear user errors.

These cases are small individually, but together they decide whether the CLI feels dependable.
