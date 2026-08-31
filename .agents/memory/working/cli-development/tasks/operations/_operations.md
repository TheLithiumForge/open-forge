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
plans. Cleanup forms an operand-free default-all catalogue of positively
recognized selected-workspace recovery bundles and drafts without classifying
their activity before lease acquisition. An empty catalogue is a no-lease no-op;
deletion requires holding the same-workspace persistent `WorkspaceLockLease`
through `FileShare.None`, then final catalogue and expected-state revalidation
under that lease. Lease contention causes no deletion. Cleanup adds no independent
activity classifier, marker, PID, journal, or activity-metadata write.

These commands reuse facts from real producers. They must not invent duplicate
scanners, lifecycle interpretations, recovery formats, or generated-artifact
identities. Aggregate findings preserve domain provenance and deterministic order.
They do not discover, report, execute, or mutate repository state. Recovery
catalogues recognize only the strict external bundle schema and never restore or
rebind a target.

Each operational aggregate domain contributes one explicit typed contributor in
deterministic order, registered directly through composition. Do not use
reflection, a service locator, or a dynamic plug-in engine. This freezes the
composition mechanism only and does not select or change the accepted child
command order.

## Child Tasks

- [ ] [Implement complete workspace, lifecycle, managed-source, generated, and recovery-bundle status facts](status.md) — Planned — Implementer: Not assigned
- [ ] [Implement diagnosis domains, severity, recommendations, and complete doctor projections](doctor.md) — Planned — Implementer: Not assigned
- [ ] [Implement explicit repair planning, dry run, application, verification, and recovery](repair.md) — Planned — Implementer: Not assigned
- [ ] [Implement lease-validated cleanup of recognized recovery bundles and drafts](cleanup.md) — Planned — Implementer: Not assigned

## Entries

<!-- open-forge:generated-index:start -->
- [Implement lease-validated cleanup of recognized recovery bundles and drafts](cleanup.md) - #Memory #Working #CLI #Task #Cleanup #Mutation #Contextual
- [Implement diagnosis domains, severity, recommendations, and complete doctor projections](doctor.md) - #Memory #Working #CLI #Task #Doctor #Diagnosis #Contextual
- [Implement explicit repair planning, dry run, application, verification, and recovery](repair.md) - #Memory #Working #CLI #Task #Repair #Mutation #Recovery #Contextual
- [Implement complete workspace, lifecycle, managed-source, generated, and recovery-bundle status facts](status.md) - #Memory #Working #CLI #Task #Status #Observation #Contextual
<!-- open-forge:generated-index:end -->
