---
open-forge:
  description: Entry ids are short, sortable, timestamp-prefixed, and hand-rolled
  tags: [Extension, Memory, Decision, Identifiers, CurrentTruth]
---

# Entry Ids

## Decision

Every entry gets an id of the form `<base36 millisecond timestamp>-<4 random base36 chars>`, generated at log time. Ids are stable forever and lexicographically sortable by creation time.

## Rationale

Array indexes broke after deletions in the MVP. Timestamp-prefixed ids sort naturally, are short enough to type for `amend`, and need no dependency — a ULID package was rejected under the zero-dependency stance.

## Consequences

- Users reference ids in `amend`; `list` must display them.
- Collisions within the same millisecond are tolerable at personal-tool scale given the random suffix; do not add coordination machinery.
