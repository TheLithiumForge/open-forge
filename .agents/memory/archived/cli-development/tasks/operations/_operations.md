---
open-forge:
  description: Implement incremental aggregate status and diagnosis, then repair and cleanup after the complete producer inventory
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Status, Doctor, Repair, Cleanup]
---

# Operational Commands

## Task State

- State: Complete. Task 20 “Cleanup” is root-accepted at phase 5/5,
  milestone 8/8 and squash-integrated at `148d378d`, exact candidate tree
  `ab7e488192b435fdefa0b8d30bf1dc853a6b2327`. Task 19 “Repair” is complete
  at phase 5/5, milestone 8/8 and integrated at `11e7a5ed`, exact accepted
  tree `b2bd951fde4e3afdad87f64a692597e3d167f609`.
- Parent: [Complete The Replacement CLI](../00-cli-development.md).
- Initial prerequisites: Task 14 “Extension Install” for Status, then Status for
  Doctor. Status, Doctor, Extension Remove, Repair, and Cleanup are complete.
  Task 23 Workspace Libraries follows at the program boundary.

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

Task 15 froze an initial complete contributor inventory for every producer in its
exact baseline. Task 16 diagnoses that complete declared inventory, and Task 17
closed its accepted Extension bridge-registration observation. Task 18 has no
Doctor producer obligation. Repair and Cleanup consume the accepted inventory
and their command-local dependencies; no absent future domain is called
healthy.

## Child Tasks

- [x] [Task 15: implement complete baseline workspace, lifecycle, managed-source, generated, and recovery-bundle status facts](status.md) — Complete
- [x] [Task 16: implement diagnosis domains, severity, recommendations, and complete doctor projections](doctor.md) — Complete; no remaining Extension observation horizon
- [x] [Task 19: implement explicit repair planning, dry run, application, verification, and recovery](repair.md) — Complete at phase 5/5, milestone 8/8; integrated at `11e7a5ed`
- [x] [Task 20: implement lease-validated cleanup of recognized recovery bundles and drafts](cleanup.md) — Complete at phase 5/5, milestone 8/8; root-accepted and integrated at `148d378d`

## Entries

- [Implement lease-validated cleanup of recognized recovery bundles and drafts](cleanup.md) - #Memory #Archived #Contextual #Historical #CLI #Task #Cleanup #Mutation
- [Implement diagnosis domains, severity, recommendations, and complete doctor projections](doctor.md) - #Memory #Archived #Contextual #Historical #CLI #Task #Doctor #Diagnosis
- [Implement explicit repair planning, dry run, application, verification, and recovery](repair.md) - #Memory #Archived #Contextual #Historical #CLI #Task #Repair #Mutation #Recovery
- [Implement complete workspace, lifecycle, managed-source, generated, and recovery-bundle status facts](status.md) - #Memory #Archived #Contextual #Historical #CLI #Task #Status #Observation
