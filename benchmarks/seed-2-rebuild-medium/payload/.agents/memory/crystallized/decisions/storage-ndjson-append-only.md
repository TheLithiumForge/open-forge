---
open-forge:
  description: Storage is append-only NDJSON, one record per line; never rewrite the whole file on log
  tags: [Extension, Memory, Decision, Storage, CurrentTruth]
---

# Storage: Append-Only NDJSON

## Decision

The data file is newline-delimited JSON: one record per line, appended only. Normal operation never rewrites existing lines.

## Rationale

The MVP's JSON-array store rewrote the entire file on every log; a crash mid-rewrite lost data. Appending a single line is effectively atomic at this scale, keeps history intact, and stays human-inspectable with standard line tools.

## Consequences

- `log` and `amend` append one line each; nothing else touches existing lines.
- Reading replays the file in order; later records may supersede earlier ones (see immutable-corrections).
- A compaction command is explicitly out of scope for this rebuild.
- Rejected alternative: JSON array with temp-file-and-rename. Safer than the MVP but still rewrites everything and loses the append-only audit property.
