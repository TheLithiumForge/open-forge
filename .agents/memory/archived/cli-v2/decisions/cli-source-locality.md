---
open-forge:
  description: "Historical CLI-v2 source: The replacement CLI organizes commands, helpers, and tests by behavioral locality and promotes shared code only to the nearest common scope"
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Source Locality

## Context

The MVP concentrates every concern in one CLI module and most tests in a few distant files. A replacement organized only by technical layers would reduce file size while still forcing a reader to navigate several unrelated folders to understand one command.

## Decision

Organize replacement source by locality of behavior and scope.

Public command paths establish the initial source locality for leaf metadata,
registration, handlers, focused support, and direct tests. A family module
composes its children without becoming a domain handler. Shared behavior moves
only to the nearest common scope of demonstrated consumers; technical-layer
trees, sibling-private imports, generic utility catalogues, and speculative
root services do not replace that locality.

The [CLI Architecture](../../documents/cli/architecture.md#source-organization)
owns the current replacement tree and dependency direction. The [command
slice](../../../../patterns/open-forge/cli/commands/command-slice.md), [focused
module](../../../../patterns/open-forge/typescript/focused-module.md), and [nearest
shared scope](../../../../patterns/software/source-locality/nearest-shared-scope.md)
Patterns own the reusable inspectable shapes. This Decision preserves why
locality was selected rather than copying those structures.

## Rationale

Local slices make the complete behavior of a small command visible from one folder and keep direct handler tests cheap to find. Promotion on demonstrated reuse prevents both duplication and speculative root-level abstractions.

The structure preserves dependency direction without forcing every feature through matching global `commands`, `application`, `domain`, `adapters`, and `tests` trees. Small commands remain small, while complex capabilities can develop focused internal modules locally.

## Consequences

- Readers can inspect one command without traversing parallel parser,
  application, domain, adapter, and test trees.
- Whole-executable journeys remain at CLI scope because they consume more than
  one behavioral slice.
- Direct imports expose dependency direction, and promotion records actual
  reuse rather than anticipated reuse.
- Refactoring may move a capability without changing the public command
  contract, provided the current Architecture and reusable Patterns remain
  satisfied.

## Authoritative Sources

- [Open Forge CLI Architecture](../../documents/cli/architecture.md)
- [TypeScript source structure directive](../../../../directives/open-forge/typescript/typescript-source-structure.md)
- [Focused TypeScript module pattern](../../../../patterns/open-forge/typescript/focused-module.md)
- [Named TypeScript values pattern](../../../../patterns/open-forge/typescript/named-values.md)
- [Nearest shared scope pattern](../../../../patterns/software/source-locality/nearest-shared-scope.md)
- [Planned CLI mutation pattern](../../../../patterns/open-forge/cli/filesystem/planned-mutation.md)
- [CLI command slice pattern](../../../../patterns/open-forge/cli/commands/command-slice.md)

## Decision Relationships

- [CLI command framework](cli-command-framework.md)
- [CLI testing architecture](cli-testing-architecture.md)
- [CLI direct replacement development](cli-direct-replacement-development.md)
