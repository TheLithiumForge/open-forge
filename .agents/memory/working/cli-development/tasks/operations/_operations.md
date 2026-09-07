---
open-forge:
  description: Implement incremental aggregate status and diagnosis, then repair and cleanup after the complete producer inventory
  tags: [Memory, Working, CLI, Task, Status, Doctor, Repair, Cleanup, Contextual]
---

# Operational Commands

## Task State

- State: Active through Task 19 “Repair” at phase 4/5, milestone 3/8. Repair
  Preflight, Gray, and Red are accepted. Task 18 integration was merged at
  `f0437512`; refrozen transition `eae8eb36` opened Green, which remains a
  draft pending focused verification and acceptance.
- Parent: [Complete The Replacement CLI](../00-cli-development.md).
- Initial prerequisites: Task 14 “Extension Install” for Status, then Status for
  Doctor. Status, Doctor, and Task 18 “Extension Remove” are complete. Repair is
  active, and Cleanup retains its command-local dependency on Repair.

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
- [ ] [Task 19: implement explicit repair planning, dry run, application, verification, and recovery](repair.md) — Active at phase 4/5, milestone 3/8; Green draft after accepted merge/refreeze `eae8eb36` — Task Mastermind allocated by the Overseer
- [ ] [Task 20: implement lease-validated cleanup of recognized recovery bundles and drafts](cleanup.md) — Task-locally active at phase 4/5, milestone 3/8 with accepted Red; queued after Task 19 for Green — Green owner assigned after Task 19 integration

## Entries

<!-- open-forge:generated-index:start -->
- [Implement lease-validated cleanup of recognized recovery bundles and drafts](cleanup.md) - #Memory #Working #CLI #Task #Cleanup #Mutation #Contextual
- [Implement diagnosis domains, severity, recommendations, and complete doctor projections](doctor.md) - #Memory #Working #CLI #Task #Doctor #Diagnosis #Contextual
- [Implement explicit repair planning, dry run, application, verification, and recovery](repair.md) - #Memory #Working #CLI #Task #Repair #Mutation #Recovery #Contextual
- [Implement complete workspace, lifecycle, managed-source, generated, and recovery-bundle status facts](status.md) - #Memory #Working #CLI #Task #Status #Observation #Contextual
<!-- open-forge:generated-index:end -->
