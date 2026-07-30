---
open-forge:
  description: Accepted compatibility, safety, non-goal, and verification facts for the archive export
  tags: [Memory, Document, Product, CurrentTruth, Archive, Export, Compatibility]
---

# Archive Export Contract

The public archive export remains UTF-8 NDJSON with exactly one record per physical line. Records are ordered by `createdAt`, then `id`; the stable fields are `id`, `kind`, `createdAt`, and `body` in that order.

An optional `--since <ISO-date>` filter may be added. Without it, output must remain byte-compatible with the existing full-export fixture. The boundary is inclusive at the start of the requested UTC date.

Export is read-only. It never rewrites source records, generated indexes, or workspace memory. A malformed source record fails with its source path and physical line number, and a failed export must not leave a partial destination.

Compression, cloud upload, a second output format, schema migration, source repair, and mutation of archived records are non-goals.

Verification must cover repeated-run byte determinism, the unchanged default fixture, the inclusive `--since` boundary, stable ordering ties, malformed-line diagnostics, absence of a partial destination, and unchanged source bytes after both success and failure.
