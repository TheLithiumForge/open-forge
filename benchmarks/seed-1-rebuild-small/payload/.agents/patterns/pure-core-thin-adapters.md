---
open-forge:
  description: Split domain logic from CLI, filesystem, and process edges
  tags: [Extension, Pattern, Architecture, Testability]
---

# Pure Core, Thin Adapters

## Shape

Keep bookmark behavior in pure functions:

- normalize URL
- normalize tags
- add bookmark
- remove bookmark
- list/filter bookmarks
- search bookmarks
- format output lines
- parse and validate persisted JSON shape

Keep side effects in thin adapters:

- CLI argv parsing
- reading environment variables
- reading and writing files
- printing stdout and stderr
- setting exit codes

## Test Implication

Core behavior should be unit-tested without spawning a process or touching the filesystem. Adapter behavior should be covered by focused integration tests.
