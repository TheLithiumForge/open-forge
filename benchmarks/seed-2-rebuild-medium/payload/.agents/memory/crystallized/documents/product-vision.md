---
open-forge:
  description: Durable product vision and acceptance criteria for the standup journal CLI
  tags: [Extension, Memory, Document, Vision, Product, CurrentTruth]
---

# Product Vision

Build a small CLI that lets a developer log what they worked on in one short command, and answer "what did I do since last standup?" reliably.

## User Story

As a developer juggling 2–3 projects, I want to log one-line work entries as they happen and get a standup-ready summary the next morning, grouped by project, without ceremony.

## Required Commands

- `standup log <text> [--project p] [--tags a,b]`
- `standup list [--since <when>] [--project p] [--tag t]`
- `standup report [--since <when>]` — standup-ready output, grouped by project
- `standup amend <id> <text>` — correct an entry (see the immutable-corrections decision)
- `standup projects` — list known projects with entry counts
- `standup help`

## Behavior Contract

- Logging must be a single fast command; no prompts, no editors.
- Every entry gets a stable, sortable id (see the entry-ids decision).
- `--project` is optional; entries without one go to project `general`. Project names are trimmed and lowercased.
- Tags are trimmed, lowercased, de-duplicated, and sorted.
- `--since` accepts `yesterday`, `friday`, an ISO date, and `laststandup` — semantics defined in the recall-window decision. Default for `report` is `laststandup`.
- `report` groups by project, newest last within a group, and prints a friendly line when a window has no entries.
- `amend` never edits history in place (see immutable-corrections); `list` and `report` show the corrected text only.
- Invalid commands and arguments show usage and exit 1; empty results are success.
- The data file location can be overridden with an environment variable for tests.

## Persistence Contract

- Storage is append-only NDJSON (see the storage decision).
- Unparseable lines produce a clear user-facing error naming the line number; never silently skip or rewrite them.
- The tool does not need cross-process locking; document this limitation.
