---
open-forge:
  description: Candidate observations about choices agents may over-prefer on this project
  tags: [Extension, Memory, Observation, AgentBehavior, Candidate, Contextual]
---

# Agent Temptations

Choices that make implementation faster but violate recorded truth or add maintenance weight:

- floats "just for parsing" money input before converting to cents,
- Express, a router package, an ORM, or a validation library despite the frameworks decision,
- computing summary or filtering in the CLI because the data is "right there",
- a DELETE endpoint or `remove`-style hard delete because it seems obviously expected,
- partial CSV imports with a "skipped N bad rows" summary,
- binding `0.0.0.0` for convenience,
- error responses assembled inline instead of through the single helper.

Use as review context, not an absolute ban. If the implementation chooses one of these, look for a concrete stated reason.
