---
open-forge:
  description: Candidate observations about choices agents may over-prefer on this seed
  tags: [Extension, Memory, Observation, AgentBehavior, Candidate, Contextual]
---

# Agent Temptations

## Observation

Agents may over-prefer choices that make implementation faster but maintenance heavier:

- adding a CLI framework for five commands,
- using a schema package for a tiny JSON shape,
- creating a service/repository abstraction before there is real complexity,
- treating test pass/fail as enough without checking product semantics,
- writing a README that documents commands but not behavior.

## Use

Use this as review context, not as an absolute ban. If the implementation chooses one of these, look for a concrete reason.
