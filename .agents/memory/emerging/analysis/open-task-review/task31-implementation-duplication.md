---
open-forge:
  description: Review of Task 31 Implementation duplication, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation
  tags: [Memory, Analysis, TaskReview, Contextual, Candidate]
---

# Task 31 Implementation Duplication Review

## Question

Is Task 31 still worth doing, what exactly remains, and when should it run?

**Task record:** [Task 31](../../../archived/cli-development/tasks/task31-implementation-duplication.md)

## Current Conclusion

**Recommendation:** Close as done.

**Size:** Small, because what remains is record correction and one optional file split.

M5's wait condition is met, and M5 has already been applied. Task 30 G4 finished the presentation structure, and [Task 38](../../../archived/cli-development/tasks/task38/_task38.md) settled the five-library project topology. The M5 moves landed in `dac3bb53` on 2026-09-21, the same commit as the project split. M1 to M4 were complete before that. The only unapplied item is an optional split of `CleanupResultFacts.cs`, which no user sees and no other Task needs. Record the application, rule on that split, and archive the Task.

## What It Implies

Closing changes nothing for users, the Framework, or CLI behavior. Every Task 31 milestone was behavior-preserving by definition.

For maintenance, closing removes an active Task whose `phase-3-5.md` subtask is tagged `KeepInMind`. That subtask stops being refreshed whenever the Task scope is active. The Route Move and Route Remove stop boundary stays in force as recorded history, not as open work.

## State Today

Verified against the repository:

- The Extension List collapse is applied. `Commands/Extension/List/Models/` in `src/cli/operations/OpenForge.Cli.Operations` holds the flattened model files.
- `RouteListResult.cs` now sits in `Commands/Route/List/Models/Result/`.
- `WorkspaceSelectionWireVocabulary.cs` and its test are gone. Git shows the deletion in `dac3bb53`.
- `CleanupResultFacts.cs` is unsplit. It has 1,023 lines and now lives in the Operations library.
- A count over tracked files, using the inventory's leaf-folder rule, finds about 296 one-file folders across the five libraries. The branch-point figure was 294, so no new folder sprawl appeared.

The records disagree with each other and with the code:

- The Task record's "Current next step" still asks for a ruling on two `Needs a decision` folders and then the collapse. The [M5 inventory](../../../archived/cli-development/tasks/task31/phase-structure-inventory.md) records both gates as resolved and applied, with Unit 3,176 passed and Integration 2,219 run with 0 failed.
- The Task's milestone map says M5 application has not started.
- The [ledger](../../../working/cli-development/project-control.md) says "M5 waits for stable structure".
- The inventory's paths use `src/cli/core/OpenForge.Cli.Core`, which no longer exists. Task 38 split it into Framework, Shell, Operations, Rendering, and OutputText.

## Dependencies

- **Blocked by:** nothing. G4 and Task 38 are complete.
- **Blocks:** nothing. No open Task waits on Task 31.
- **Overlaps with:** Task 30 slice 53, the single path normalizer. Repair still normalizes through its own `RepairRelinkNormalizer`. That is the last duplication-shaped work in view, and Task 30 already carries it.

## Remaining Work

1. Update the Task record, its milestone map, and the ledger row to say M5 was applied on 2026-09-21 in `dac3bb53`.
2. Rule on the `CleanupResultFacts.cs` split. Either accept it as one separate behavior-preserving batch, or decline it.
3. If it is accepted, re-derive the consumer list for the new layout first. For example, `CleanupReportSelector.cs` now lives in the Rendering library. Then run the Cleanup contract tests, `CleanupBeforeOutputSnapshotTests`, `LayerBoundaryTests`, and `CliReportInvariantsTests`. The last two moved to the integration project.
4. Archive Task 31 and its subtasks. Keep the Route Move and Route Remove boundary in the archived record, so a later reader doesn't lift those selectors.

## Pros And Cons

| Pros                                                                                 | Cons                                                                                          |
| ------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------- |
| Removes a stale active Task and a `KeepInMind` subtask from routine context          | Leaves one 1,023-line file whole if the split is declined                                     |
| Makes the records match the code                                                     | The 103-group duplication survey was measured before the project split and is not re-measured |
| Keeps optional cleanup behind user-visible 1.0 work, as the beta follow-up list asks |                                                                                               |

## Risks And Open Questions

- Task 38 moved code across five libraries. That move could have created new duplication, for example between OutputText wording and Rendering. No measurement since the split checks this. A fresh scan would be a new Task, not a reason to keep this one open.
- The maintainer must decide whether the Cleanup split is worth a batch. It separates distinct fact families, but no reader or contributor has reported the file as a problem.
- Behavior Tasks still to come, such as 33, 35, 36, 48, and 63, will change command code. They don't change the project topology, so they don't reopen M5's wait condition.

## Next Check

**Action:** correct the three stale statements and ask the maintainer for a yes or no on the Cleanup split.

**Would change the conclusion:** a duplication scan on the five-library layout that finds substantial new duplicated bodies, or a planned Cleanup change that the split would make easier.

**Acceptance needed:** the maintainer decides the Cleanup split and the Task's closure.
