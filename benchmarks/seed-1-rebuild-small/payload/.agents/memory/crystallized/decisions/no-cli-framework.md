---
open-forge:
  description: Prefer handwritten CLI parsing over a command framework for this seed
  tags: [Extension, Memory, Decision, CLI, Dependency, CurrentTruth]
---

# No CLI Framework

## Decision

Use handwritten command parsing for this project instead of a CLI framework such as Commander, Yargs, or Oclif.

## Rationale

A framework would make argument parsing faster to write, and an agent may naturally prefer it. For this project, the maintenance cost is not worth it:

- the command set is tiny,
- the option grammar is simple,
- handwritten parsing is easy to test directly,
- no runtime dependency keeps the tool easier to audit,
- framework-generated help text can hide behavior that should be explicit in tests and README.

## Consequence

If an implementation adds a CLI framework, it must explain why the seed's command surface has become too complex for handwritten parsing.
