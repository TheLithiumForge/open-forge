---
open-forge:
  description: Customize selected guidance through its adjacent overwrite without editing the base
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F25: Customize through an overwrite

**Selection:** Recommended addition for maintainer validation.

## Who And Goal

A maintainer wants a local adjustment while preserving the original guidance.

## Starting Point

An installed workspace with an ordinary `guidance/team-note` source. Keep its original bytes for comparison.

## Flow

1. Write `.agents/guidance/team-note.overwrite.md` with a distinctive local adjustment.
2. Run `open-forge index`, then list the guidance route.
3. Run `open-forge context guidance/team-note`.
4. Run index again and compare both authored files byte-for-byte.

## Expected Result

The overwrite is not a separately indexed source. Context supplies the base and then its companion, preserving their relationship. Both authored files remain unchanged and repeated indexing is a no-op. This checks retrieval and preservation, not whether an agent obeys the prose.

## Verification

Count the route entries independently and inspect context source order. Preserve separate actual output and state checks in the run record. This flow adds a missing everyday customization case to the supplied collection.
