---
open-forge:
  description: Place support through demonstrated semantic reuse or an accepted neutral foundation at the nearest shared scope of its consumers
  tags: [Pattern, Software, Source, Locality, Shared, Reuse, Dependency]
---

# Nearest Shared Scope

## Shape

Organize source around the behavior, capability, subject, or component whose
files change together. Keep its implementation, contracts, unit and integration
tests, fixtures, and focused support in that local scope. Different artifact
types may use separate files, but the file type alone is not a reason to move
them into distant category folders such as one workspace-wide `interfaces`,
`behaviors`, or `tests` tree.

Locality has one structural exception. When a language or build system makes
separate test projects or roots materially cleaner, place tests in a separate
physical tree that mirrors the production feature or capability paths
one-to-one. Keep fixtures and support at the nearest mirrored scope. Place
complete system tests at the system boundary. Do not link test files into
production folders merely to simulate locality.

Begin support beside its only consumer:

```text
commands/
  status/
    status-handler
    format-status
  context/
    context-handler
```

For a separate test root, mirror the production path rather than flattening the
test categories:

```text
src/
  commands/
    status/
      status-handler
tests/
  commands/
    status/
      status-handler-tests
      fixtures/
      support/
  system/
    cli-journeys/
```

When two siblings genuinely consume the same capability, move that capability to their nearest common ancestor:

```text
commands/
  shared/
    terminal-width
  status/
    status-handler
  context/
    context-handler
```

When consumers span the complete CLI, promote only as far as CLI scope:

```text
src/cli/
  shared/
    paths
    process-execution
  commands/
```

A `shared` folder communicates the scope of reuse. Every file inside it still names one capability. A capability-specific folder may replace `shared` when several focused files form one cohesive subsystem.

Do not import a sibling's private support merely because its current implementation is convenient. Promote the shared behavior first, then let both consumers depend on the promoted module.

Do not pre-create shared modules for hypothetical reuse. Duplication is evidence to inspect, not automatic proof that two behaviors have the same meaning.

For a greenfield program with an accepted top-down work graph, do not confuse
later implementation with hypothetical use. If several accepted program outcomes
require one neutral grammar, parser, protocol adapter, serializer, identity model,
or safety primitive, place that mechanical capability at their nearest shared
scope before the first dependent consumer. Keep feature semantics and policy
local, and record the accepted outcomes that justify the foundation.

The replacement CLI has one explicit specialization: supporting implementation
always sits below a `Shared/<Capability>/` child of its narrowest owning CLI,
command-family, or command-leaf boundary. At the leaf level, `Shared` marks the
support boundary rather than proving several consumers. Demonstrated semantic
reuse or an accepted top-down neutral foundation is still required when support
moves to a wider `Shared` parent. Follow the
selected CLI and C# Directives for that physical and namespace shape; do not
apply this spelling to unrelated languages or repositories by analogy.

## Review Checks

- Every support module is at the narrowest scope containing all actual consumers.
- Files that define, implement, or verify one behavior remain easy to discover
  from that behavior's local scope or its one-to-one mirrored test scope.
- A separate test root exists only when the language or build system makes it
  materially cleaner, and its feature or capability paths mirror production
  one-to-one.
- Fixtures and support are at the nearest mirrored scope, while complete system
  tests are at the system boundary.
- No test file is linked into a production folder merely to simulate locality.
- Artifact-type categories do not separate files that change together.
- Promotion follows demonstrated semantic reuse or an accepted top-down neutral
  foundation and preserves one clear owner.
- A top-down foundation names its accepted consumers and contains no consumer
  policy merely to anticipate their delivery.
- Dependency direction does not pass through a sibling's private folder.
- Shared files retain capability-specific names.
- In the replacement CLI, leaf-local support uses the required explicit
  `Shared/<Capability>/` path without implying wider reuse.
- Moving a module upward does not turn it into an unrelated utility collection.
