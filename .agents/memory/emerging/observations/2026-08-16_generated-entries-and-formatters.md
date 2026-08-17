---
open-forge:
  description: Formatting routed entrypoints changed generated Entries whitespace until targeted indexing restored the manager-owned representation
  tags: [Memory, Observation, AgentLearning, Contextual, Candidate, Framework, Routing, Generated, Formatting, Validation]
---

# Observation: Formatters And Generated Entries Need Separate Validation

Date: 2026-08-16. Scope: CLI release review and program entrypoints. This is a
concrete tooling occurrence, not an accepted formatter or indexing rule.

## Occurrence

During Queue 29 validation, Prettier reported style differences in the review and
CLI-release entrypoints. Running Prettier on those complete files inserted blank
lines inside their generated `Entries` regions. A subsequent targeted
`open-forge-old index` restored the generated representation.

The final validation therefore:

- used targeted indexing for manager-owned `Entries` interiors;
- limited Prettier claims to authored non-generated files; and
- reran Doctor, body loading, link checks, and `git diff --check` after indexing.

No command meaning, route metadata, or authored prose required the generated
whitespace change. The conflict was between whole-file formatting and the
current generated representation.

## Potential Reuse

Future formatting or validation should distinguish authored prose from generated
interiors instead of treating a whole routed entrypoint as formatter-owned. A
future tool may need one of these bounded solutions:

- formatter exclusion for generated interiors;
- formatter output that preserves the exact manager representation; or
- a validation sequence that formats authored sources and indexes generated
  regions afterward without claiming that the complete entrypoint independently
  satisfies both byte representations.

Do not change the canonical generated format or add a formatter exception from
this single occurrence alone. Record another matching occurrence or resolve it
through the future CLI's accepted Index and formatting architecture.
