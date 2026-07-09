---
open-forge:
  description: Durable product vision and acceptance criteria for the bookmarks CLI seed
  tags: [Extension, Memory, Document, Vision, Product, CurrentTruth]
---

# Product Vision

Build a small CLI that makes saving and finding personal bookmarks predictable without requiring a service.

## User Story

As a developer working in a terminal, I want to save URLs with a few tags so I can list, filter, search, and remove them later from the same machine.

## Required Commands

- `add <url> [--tags a,b,c]`
- `list [--tag x]`
- `remove <url>`
- `search <query>`
- `help`

## Behavior Contract

- Validate URLs with the platform URL parser.
- Store normalized URL strings so equivalent URL input does not create avoidable duplicates.
- Accept only `http://` and `https://` URLs.
- Tags are trimmed, lowercased, de-duplicated, and sorted for stable output.
- Adding an existing normalized URL updates the bookmark by merging new tags with existing tags.
- `list --tag x` uses exact tag matching after tag normalization.
- `search` is case-insensitive over normalized URLs and tags.
- Empty list and empty search results are successful commands with friendly output.
- Removing a missing bookmark is a user error.
- Invalid commands and invalid arguments show usage.
- The data file location can be overridden with an environment variable for tests and isolated runs.

## Persistence Contract

- Store bookmarks as human-readable JSON.
- Do not silently overwrite corrupt or wrong-shaped data.
- Writes should avoid truncating the data file if the process is interrupted.
- The tool does not need cross-process locking; document this limitation if relevant.
