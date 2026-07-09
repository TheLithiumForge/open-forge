---
open-forge:
  description: Expected test shape for the bookmarks CLI implementation
  tags: [Extension, Pattern, Testing, Verification]
---

# Test Contracts

## Minimum Useful Tests

Test pure behavior:

- URL validation and normalization.
- Tag normalization.
- duplicate add merges tags.
- list filtering.
- search matching.
- remove hit and miss.
- persisted JSON parse errors.

Test real CLI behavior:

- add/list/list by tag/search/remove happy path.
- invalid URL.
- missing or unknown command.
- isolated data file through environment override.
- at least one smoke path through the executable entrypoint, not only direct function calls.
- test and smoke data stored under the workspace, not the operating system temp directory.

## Verification

One command should run the full test suite. The final report must name that command and its result.
