---
open-forge:
  description: Analysis of tempting tooling and dependency choices for the rebuild
  tags: [Extension, Memory, Analysis, Tooling, Dependency, Contextual]
---

# Tooling And Dependency Tradeoffs

## Bun Versus Node

Node with npm is the most universal runtime choice. It may be the safer default for unknown machines.

Bun is still the accepted choice for this seed because the maintainers want to dogfood a Bun-first TypeScript workflow and compare agent behavior under that constraint. The implementation may use Node-compatible APIs where they keep the code portable, but the project scripts should prefer Bun when available.

## Commander Or Yargs Versus Handwritten Parsing

Commander or Yargs would reduce boilerplate and produce polished help output quickly. They are not selected because the command surface is intentionally small and the dependency would be larger than the parsing problem.

Handwritten parsing makes the behavior obvious in code review and forces the implementation to own error messages and tests.

## Zod Or Valibot Versus Handwritten JSON Validation

Schema libraries would make persisted data validation concise. They are not selected because the persisted shape is tiny and handwritten validation keeps runtime dependencies at zero.

The tradeoff is that tests must cover wrong-shaped JSON so validation does not drift.

## Build Step Versus Direct TypeScript Execution

Direct TypeScript execution can be convenient in modern runtimes. A build step can make the installed CLI behavior clearer and expose type errors before smoke tests.

Either is acceptable if the README and scripts are clear. Do not let tooling cleverness become the project.
