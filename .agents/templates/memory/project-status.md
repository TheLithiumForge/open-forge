---
open-forge:
  description: Project status template is used when one bounded Working record must preserve where active work stands, what matters now, what blocks it, and how to resume
  tags: [Template, Memory, Working, Status, Contextual]
---

# {Subject} Status

{
Template selection:

- Need: One expiring coordination record for the current position of active work.
- Primary question: Where does the work stand, what matters now, what blocks it, and what is needed to resume or coordinate it?

Instantiate under the appropriate Working route.
This record is expected to expire. Extract durable results, archive useful history, and prune obsolete operational detail when the active need ends.
Replace this template's frontmatter, title, and placeholders, then remove this braced source guidance.
}

## Scope And Freshness

{State what this status covers, when it was last made current, and which external systems define live details. Link the project control ledger for permanent task identity, queue state, and completion grace, and link Task records for accepted horizons, current phase ordinals, completed milestone counts, and current-state suffixes. This Status only derives and presents those facts.}

## Current Phase

{Name the present operating or development phase in ordinary domain language. The numeric phase value is the current active ordinal, starting at one.}

## Completed

{Record only completed outcomes needed to understand the current position. If this Status projects `Recently completed`, derive membership and remaining grace from the project control ledger, show the final Task-owned phase and full milestone count, and do not extend grace. Link to durable authoritative sources and evidence.}

## In Progress

{State active work and its responsible person or role without copying full task systems. Derive each permanent task ID and actual name from the project control ledger. Derive the current phase ordinal, completed milestone count, and separate current-state suffix from the linked Task record.}

## Current Priorities

{State the ordered outcomes receiving attention now. When showing queued tasks, preserve the ledger's priority and dependency order rather than sorting by task ID. Do not invent phase or milestone horizons.}

## Gaps And Blockers

{State unresolved conditions that materially prevent or threaten progress.}

## Next Actions

{State concrete next actions sufficient to resume or coordinate the work.}

## Exit

{State when this record should be refreshed, extracted, archived, or pruned.}
