---
open-forge:
  description: Place unit, integration, complete, and package journey evidence at the scope of the behavior each tier proves
  tags: [Pattern, Testing, Evidence, Unit, Integration, EndToEnd, PackageEndToEnd, Locality, Snapshot]
---

# Evidence Tiers

## Shape

Use increasing evidence depth without moving focused tests away from the behavior
they prove. Use the following evidence terms when the test system supports
durable test classification.

| Tier            | Proves                                                      | Typical boundary                                                                        |
| --------------- | ----------------------------------------------------------- | --------------------------------------------------------------------------------------- |
| Unit            | One cheap, focused method, class, function, or step         | In-memory inputs and explicit dependencies                                              |
| Integration     | One module or command at a selected real system boundary    | Filesystem, process, database, network adapter, package layout, or equivalent           |
| EndToEnd        | A complete critical journey through the delivered interface | Whole built artifact, deployed service, user interface, or another public surface       |
| PackageEndToEnd | A complete journey through a packed or wrapped delivery     | Packed artifact, package-manager wrapper, launcher, or equivalent distribution boundary |

Most behavior belongs in the lowest tier that proves the relevant contract.
`EndToEnd` evidence proves the delivered composition, and `PackageEndToEnd`
evidence proves the packed delivery path. Neither is the default home for
detailed branch logic.

Keep unit and integration evidence beside its production subject until
demonstrated reuse needs a nearer shared test-support scope. When a language or
build system makes separate test projects or roots materially cleaner, use a
physical test tree that mirrors production feature or capability paths one-to-one.
Keep fixtures and support at the nearest mirrored scope. Place complete-system
and package journeys at the system boundary because they prove dispatch,
packaging, rendering, completion, and several capabilities together. Do not link
test files into production folders merely to simulate locality.

Prefer real functions and boundaries plus observable resulting state. A test double closes only a boundary that cannot be exercised safely and deterministically with the real implementation.

## Snapshots

Snapshot a focused stable projection only when it communicates the complete reviewed result better than explicit assertions. Keep safety invariants, destructive effects, and critical state transitions as direct assertions when they could disappear inside a broad snapshot diff.

Snapshots of a delivered interface contain only stable public evidence. Normalize only known environmental variation; do not broadly scrub output until it ceases to prove the contract.

## Review Checks

- Each evidence tier proves a distinct boundary without duplicating lower-tier branch coverage.
- The system boundary has a small, intentional set of complete journeys.
- Package journeys prove the packed or wrapped delivery boundary rather than only the unpacked executable.
- Test support sits at the nearest common scope of real consumers, including a nearest mirrored scope when test roots are separate.
- A separate test root mirrors production paths one-to-one and is materially cleaner for its language or build system.
- No test file is linked into a production folder merely to simulate locality.
- Doubles and normalizations have a local, evidence-based reason.
- Snapshot updates are explicit mutations rather than an ordinary test side effect.
