---
open-forge:
  description: Place unit, integration, complete, and package journey evidence at the scope of the behavior each tier proves
  tags: [Pattern, Testing, Evidence, Unit, Integration, EndToEnd, PackageEndToEnd, Locality, Snapshot]
---

# Evidence Tiers

## Shape

| Tier            | Proves                                                    | Typical boundary                                               | Placement                                                          |
| --------------- | --------------------------------------------------------- | -------------------------------------------------------------- | ------------------------------------------------------------------ |
| Unit            | One cheap focused method, class, function or step         | In-memory inputs and explicit dependencies                     | Beside production subject or in its one-to-one mirrored test scope |
| Integration     | One module or command at a selected real boundary         | Filesystem, process, database, network adapter, package layout | Beside production subject or in its one-to-one mirrored test scope |
| EndToEnd        | Complete critical journey through the delivered interface | Whole built artifact, service, UI or public surface            | System boundary                                                    |
| PackageEndToEnd | Complete journey through packed or wrapped delivery       | Packed artifact, wrapper, launcher or distribution boundary    | System boundary                                                    |

```text
subject/
  production behavior
  focused Unit and Integration evidence
  fixtures and support, when needed
system boundary/
  small intentional set of complete journeys
  packed delivery journeys, when applicable
```

Boundary distinctions and locality are required. Concrete paths and mechanisms are illustrative. A separate test root is optional when materially cleaner and mirrors production paths one-to-one. Classification names apply when the test system supports durable classification.

| Evidence part                                                      | Status                                                          |
| ------------------------------------------------------------------ | --------------------------------------------------------------- |
| Focused stable snapshot projection                                 | Optional when clearer than explicit assertions                  |
| Safety invariants, destructive effects, critical state transitions | Direct assertions when broad diffs could hide them              |
| Delivered-interface snapshot content                               | Stable public evidence only                                     |
| Doubles and normalization                                          | Local evidence-based reason, known environmental variation only |
| Snapshot baseline update                                           | Explicit mutation                                               |

## Applicability And Sources

Most behavior belongs at the lowest tier that proves its contract. Delivered and packed journeys are not the default home for detailed branch logic. Real functions, boundaries and observable resulting state are preferable.

- [Test Evidence Integrity](../../directives/open-forge/testing/evidence-integrity.md) binds evidence boundaries, doubles and snapshots.
- [Source Locality](../../directives/source-locality.md) binds production and mirrored test placement.
- [Nearest Shared Scope](../software/source-locality/nearest-shared-scope.md) shows promotion of reused support.
