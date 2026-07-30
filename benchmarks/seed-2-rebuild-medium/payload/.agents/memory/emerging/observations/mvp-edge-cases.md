---
open-forge:
  description: Candidate edge cases the MVP hit that the rebuild should cover
  tags: [Extension, Memory, Observation, EdgeCase, Candidate, Contextual]
---

# MVP Edge Cases

Grounded cases observed during six weeks of MVP use:

- Logging with an empty message (`standup log ""`) created ghost entries; should be a user error.
- A project name with surrounding spaces created a duplicate project group.
- `amend` targeting an already-amended entry: users expect to amend the *resolved* entry, chaining corrections.
- The data file's parent directory did not exist on a fresh machine; first `log` must create it.
- An entry containing a newline broke one-record-per-line storage; text must be stored escaped (JSON string handles this) and rendered flat in reports.
- `report` on a fresh install (no data file) should read as a friendly empty result, not an error.
