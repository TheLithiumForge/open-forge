---
open-forge:
  description: Place support through demonstrated semantic reuse or an accepted neutral foundation at the nearest shared scope of its consumers
  tags: [Pattern, Software, Source, Locality, Shared, Reuse, Dependency]
---

# Nearest Shared Scope

## Shape

```text
commands/
  status/
    status-handler                  behavior and composition
    format-status                   support beside its only consumer
  context/
    context-handler
```

After demonstrated identical meaning in both consumers:

```text
commands/
  shared/
    terminal-width                  promoted to nearest common ancestor
  status/
    status-handler                  -> shared/terminal-width
  context/
    context-handler                 -> shared/terminal-width
```

Optional separate test root when materially cleaner for the language or build:

```text
src/commands/status/status-handler
tests/
  commands/status/                   required one-to-one mirrored scope
    status-handler-tests
    fixtures/                       nearest mirrored scope, when needed
    support/                        nearest mirrored scope, when needed
  system/cli-journeys/               complete system boundary, when needed
```

| Part                                                   | Status                                                               |
| ------------------------------------------------------ | -------------------------------------------------------------------- |
| Narrowest scope of actual consumers                    | Required                                                             |
| Accepted neutral foundation before dependent consumers | Allowed for accepted top-down outcomes, with those outcomes recorded |
| Feature semantics and policy local                     | Required                                                             |
| Capability-specific filenames                          | Required, even under a shared folder                                 |
| `shared` folder                                        | Illustrative, a cohesive capability folder may replace it            |
| `Shared/<Capability>/`                                 | Fixed only in the replacement CLI, including leaf-local support      |
| `commands`, `status`, `context`, capabilities          | Illustrative names, not fixed order                                  |

Support spanning the complete CLI has its nearest shared scope at CLI level. A leaf-local CLI `Shared` folder marks a support boundary and does not prove wider reuse.

## Applicability And Sources

For source, contracts, tests, fixtures and support that change together. Duplication is evidence to inspect, not automatic proof of shared meaning.

- [Source Locality](../../../directives/source-locality.md) binds locality, promotion and dependency direction.
- [C# Style](../../../directives/csharp/style.md) and the selected [CLI Implementation](../../../directives/open-forge/cli/implementation.md) bind the replacement CLI's physical and namespace specialization.
- [Evidence Tiers](../../testing/evidence-tiers.md) shows complete journeys at the system boundary.
