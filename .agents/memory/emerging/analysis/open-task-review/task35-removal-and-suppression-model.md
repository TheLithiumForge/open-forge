---
open-forge:
  description: Review of Task 35 Removal and Suppression Model, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation
  tags: [Memory, Analysis, TaskReview, Contextual, Candidate]
---

# Task 35 Removal And Suppression Model Review

## Question

Is Task 35 still worth doing, what exactly remains, and when should it run?

**Task record:** [Task 35](../../../archived/cli-development/tasks/task35-removal-and-suppression-model.md)

## Current Conclusion

**Recommendation:** Close as done.

**Size:** Small, because the only useful remainder is one decision record.

Task 35 was an exploration that should end in a recorded design and a follow-up implementation task. [Task 50](../../../archived/cli-development/tasks/task50-unified-remove.md) was that design and that implementation. It chose a single root `remove` that delegates to the existing managers, one authored exclusion list, and a fixed set of consumers. The one gap is that the choice lives only in an archived task and a contract. No Crystallized Decision explains the options that were not chosen.

## What It Implies

Nothing changes for users or the CLI. Closing the task and writing the decision makes the removal model findable for later work, especially [Task 63](../../../working/cli-development/tasks/task63-keeping-edits-through-updates.md) and [Task 48](../../../working/cli-development/tasks/task48-scoping-for-extension-routes.md), which both need to agree with it.

## State Today

Each question in the record now has an answer in the [remove interface](../../../crystallized/documents/cli/contracts/remove/interface.md) and [behavior](../../../crystallized/documents/cli/contracts/remove/behavior.md) contracts:

- **Shape.** `open-forge remove <target> [--kind path|route|extension|library]`. `CliRootRemoveComposer` delegates to `route remove`, `extension remove`, and `library detach`, which stay available. This is the "delegate rather than reimplement" option.
- **Unit.** Exact files, directories including future descendants, root categories, package IDs, and Library IDs. Path exclusions are global, the "blacklist entirely" option. Suppression scoped by source was not chosen.
- **Storage.** The authored `.agents/open-forge.json`, not the lock. Task 33 had guessed the lock.
- **Consultation.** `WorkspaceRemovals` is called from 21 production files, covering Framework install and update, Extension install, update, and remove, and Library attach, sync, and detach. `route init` does not consult it, which fits "users may create files regardless of exclusions" but matters for Task 48.
- **Reporting.** `remove` releases matching claims, so a removed path never looks like a claimed file that went missing. The third-state vocabulary is only needed for exclusions written by hand, covered in the [Task 33 review](task33-managed-content-removal.md).
- **Lifecycle.** Exclusions survive `detach` then `attach` and `extension remove` then `install`. Selecting an excluded package or Library explicitly is blocked. Clearing the entry restores.
- **The `registered-link-restored` ruling.** Split exactly as the record predicted. An excluded destination stays gone ("Detach records exclusion without ownership and later Sync honors it"). An unexcluded missing link is still restored with a warning.
- **The remembered Library removal verb.** Answered by `remove --kind library` and by removing one Library link by path.

The six characterization tests exist across suites, though not as one named set. Library sync restoration has the `registered-link-gone` snapshots. Extension update restoration has "Extension Update replaces changed and restores missing current targets". Route remove on managed leaves and categories is in `RouteRemoveApplicationIntegrationTests`. The user-authored sibling case is "Root route removal releases a managed source and removes an unmanaged sibling independently". Detach then attach relies on `IsLibraryRemoved` in the attach planner. I did not find a test named for the attach block.

Stale material: the record's state line, its "Verified starting facts" table, and the [Workspace State Files decision](../../../crystallized/decisions/framework/workspace-state-files.md), which still says the settings file holds only `allowInstallPaths` and `removedCategories`. The doc comment in `src/cli/framework/OpenForge.Cli.Framework/Framework/Settings/Shared/Planning/WorkspaceRemovals.cs` points at Task 50's old Working path.

## Dependencies

- **Blocked by:** nothing.
- **Blocks:** nothing in code. Task 63 and Task 48 should cite the decision once it exists.
- **Overlaps with:** Task 33, which the same delivery closed, and Task 63, since a kept-file list would be a second kind of exclusion.

## Remaining Work

1. Write one Crystallized Decision for persistent removal intent, or extend Workspace State Files. Record the shape, the units, the storage, the consumers, and the options not chosen: per-manager verbs, suppression scoped by source, and storage in the lock.
2. Record the `registered-link-restored` split in that decision.
3. Mark Task 35 complete and archive it with Task 33.
4. Fix the stale path in the `WorkspaceRemovals` doc comment when that file is next touched.

## Pros And Cons

| Pros                                                                                  | Cons                                                                                         |
| ------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------- |
| The removal model gets one findable rationale instead of an archived execution record | Writing the decision after the fact risks inventing rationale the implementers did not weigh |
| Later tasks can cite one source for how exclusions work                               | A small amount of record work for no user-visible change                                     |
| Clears a misleading "Exploration, not implementation" item                            |                                                                                              |

## Risks And Open Questions

- Global path exclusions mean one removal affects every manager. That is the simpler model. Suppression scoped by source would matter only if two managers ever ship the same path on purpose, which the [Extensions Architecture](../../../crystallized/documents/extensions/architecture.md) currently forbids.
- The maintainer should confirm that the Task 50 shape is accepted as the Task 35 outcome rather than as a delegated delivery only.

## Next Check

**Action:** Draft the removal decision from the remove interface and Task 50, then close Tasks 33 and 35 together.

**Would change the conclusion:** A maintainer wish for suppression scoped by source, or a use case where a global path exclusion removes content the user wanted from another package.

**Acceptance needed:** The maintainer.
