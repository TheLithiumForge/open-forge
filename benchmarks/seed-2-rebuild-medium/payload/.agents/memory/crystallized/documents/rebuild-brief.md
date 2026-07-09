---
open-forge:
  description: Post-MVP rebuild context for the standup journal CLI
  tags: [Extension, Memory, Document, Rebuild, Product, CurrentTruth]
---

# Rebuild Brief

This is a clean restart after a medium-sized MVP that people actually used for six weeks, not a blank idea.

## What We Learned From The MVP

- The core loop (log fast, report at standup) is right and sticky. Everything else was decoration.
- Dates destroyed us twice: a naive "yesterday" made Monday reports empty (Friday's work invisible), and mixing local timestamps with UTC comparisons dropped entries logged late at night. The recall-window decision exists because of this.
- A JSON-array store was rewritten on every log; one crash mid-rewrite lost a week of entries. Append-only NDJSON is now an accepted decision, not a preference.
- In-place edits silently destroyed the audit trail once; a user wanted to know what an entry said before "fixing" it. Corrections are now append-only too.
- Array indexes as ids broke every reference after a deletion. Ids are now content-independent and sortable.
- Free-text project names diverged (`Web`, `web`, `website`); normalization is required, aliasing was judged not worth it.
- The MVP's `report` dumped raw entries; users wanted grouping by project and a copy-pasteable shape.

## Rebuild Goal

Rebuild the same product with the recorded semantics made precise. Same command surface plus `amend` and `projects`, which the MVP proved necessary.

## Non-Goals

- No time tracking or duration math.
- No git integration, calendar integration, reminders, or team sharing.
- No TUI, colors, or interactive mode.
- No sync; single machine, single user.
