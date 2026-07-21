---
open-forge:
  description: Accepted product, persistence, and temporal contract for the standup journal
  tags: [Memory, Document, Product, CurrentTruth, Standup]
---

# Standup Journal Contract

Build a TypeScript CLI with Bun tooling and no runtime dependencies. It helps one developer capture short work notes and produce a standup-ready recall grouped by project.

## Commands

- `standup log <text> [--project p]`
- `standup report [--since <when>]`

Unknown commands, invalid arguments, empty text, and bad date tokens show a useful error and exit 1. Empty reports are successful and friendly. `STANDUP_DATA_FILE` overrides the data location for tests.

## Records And Persistence

- Storage is append-only NDJSON. `log` appends exactly one JSON line; normal operation never rewrites or deletes an existing byte.
- Every entry has an opaque stable id, UTC ISO-8601 `createdAt`, normalized project, and text.
- An unparseable physical line fails with its line number. Reads never skip, repair, or rewrite corrupt data.
- First write creates a missing parent directory. A missing data file is an empty journal. Newlines in note text remain one physical NDJSON record and render as a single readable line.
- Editing, deletion, tags, accounts, syncing, cross-process locking, and compaction are out of scope.

## Projects, Tags, And Time

- Projects are trimmed and lowercased; the default is `general`.
- Created timestamps are UTC ISO-8601 instants. Window tokens resolve against the user's local calendar and convert to a UTC cutoff.
- `yesterday` starts at the previous local calendar day. An ISO date starts at that local date.
- `laststandup` starts on Friday when run on Monday and on the previous local day otherwise. Monday therefore includes Friday, Saturday, and Sunday entries.
- Window resolution is a pure function of the token, an injected current instant, and an injected time zone. Runtime defaults may supply the actual clock and local zone at the edge.

`report` defaults to `laststandup`, groups by project, and orders entries oldest to newest inside each group. Output includes stable ids so stored records remain inspectable.
