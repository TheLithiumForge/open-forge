---
open-forge:
  description: Implement aggregate status, diagnosis, repair, and cleanup after every state producer exists
  tags: [Memory, Working, CLI, Task, Status, Doctor, Repair, Cleanup, Contextual]
---

# Operational Commands

## Task State

- State: Planned.
- Parent: [Complete The Replacement CLI](../00-cli-development.md).
- Prerequisites: Every read-only and mutating command that produces observable
  state, lifecycle, recovery, or generated artifacts.

## Shared Boundary

Status reports typed observed state without diagnosis. Doctor evaluates complete
diagnostic domains and recommendations. Repair consumes explicit accepted repair
plans. Cleanup removes only recognized artifacts under exact ownership and safety
rules.

These commands reuse facts from real producers. They must not invent duplicate
scanners, lifecycle interpretations, recovery formats, or generated-artifact
identities. Aggregate findings preserve domain provenance and deterministic order.

## Child Tasks

- [ ] [Implement complete workspace, lifecycle, managed-source, generated, recovery, and Git status facts](status.md) — Planned — Implementer: Not assigned
- [ ] [Implement diagnosis domains, severity, recommendations, and complete doctor projections](doctor.md) — Planned — Implementer: Not assigned
- [ ] [Implement explicit repair planning, dry run, application, verification, and recovery](repair.md) — Planned — Implementer: Not assigned
- [ ] [Implement bounded cleanup of recognized disposable and recovery artifacts](cleanup.md) — Planned — Implementer: Not assigned

## Entries

<!-- open-forge:generated-index:start -->

- [Implement complete workspace, lifecycle, managed-source, generated, recovery, and Git status facts](status.md) - #Memory #Working #CLI #Task #Status #Observation #Contextual
- [Implement diagnosis domains, severity, recommendations, and complete doctor projections](doctor.md) - #Memory #Working #CLI #Task #Doctor #Diagnosis #Contextual
- [Implement explicit repair planning, dry run, application, verification, and recovery](repair.md) - #Memory #Working #CLI #Task #Repair #Mutation #Recovery #Contextual
- [Implement bounded cleanup of recognized disposable and recovery artifacts](cleanup.md) - #Memory #Working #CLI #Task #Cleanup #Mutation #Contextual

<!-- open-forge:generated-index:end -->
