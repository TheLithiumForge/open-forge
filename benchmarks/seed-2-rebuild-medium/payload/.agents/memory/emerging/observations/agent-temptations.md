---
open-forge:
  description: Candidate observations about choices agents may over-prefer on this project
  tags: [Extension, Memory, Observation, AgentBehavior, Candidate, Contextual]
---

# Agent Temptations

Choices that make implementation faster but violate recorded truth or add maintenance weight:

- reaching for a date library when the recall-window decision needs only a small pure function,
- "improving" storage to a rewritten JSON array or SQLite despite the append-only decision,
- implementing `amend` as an in-place edit because it is simpler,
- adding a `delete` command nobody asked for,
- treating window math as too trivial to test around Monday/midnight boundaries.

Use as review context, not an absolute ban. If the implementation chooses one of these, look for a concrete stated reason.
