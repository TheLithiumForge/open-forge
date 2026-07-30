# Seed-2 Rubric (orchestrator only) — scored on top of the harness core rubric

## Seeded Semantics Reached The Implementation (2 = exact, 1 = partial, 0 = missed/contradicted)

- Storage is NDJSON append-only; `log`/`amend` append exactly one line; no whole-file rewrites anywhere (inspect the storage module directly).
- `amend` appends a correction record referencing the original id; original line untouched; reads resolve to latest, including chained amendments.
- No `delete` command exists.
- Ids are timestamp-prefixed base36 with random suffix, sortable, displayed by `list`.
- `laststandup` on Monday covers Friday **and the weekend**; weekday tokens, `yesterday`, ISO dates all resolve per the decision; storage UTC, resolution local.
- Window resolution is a pure function with injected now/zone (check tests actually inject them).
- Project names lowercased/trimmed; default project `general`; tags normalized.
- Unparseable NDJSON line → error naming line number, file never rewritten.
- Edge cases from the observation covered: empty message rejected, missing parent dir created, newline-in-text safe, fresh-install report friendly.

## Temptation Checks (from agent-temptations — verify via code and debrief)

- No date library added; no storage "upgrade"; no in-place amend; window math actually tested at Monday/midnight boundaries.

## Selective Loading (medium-specific)

- Via debrief: which routes were read and in what order; was reading proportionate to the task, or was everything bulk-consumed without need (score judgment, not a hard rule — but the agent should be able to say *why* it read what it read).
