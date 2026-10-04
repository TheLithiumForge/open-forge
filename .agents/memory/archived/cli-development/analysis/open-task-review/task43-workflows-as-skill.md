---
open-forge:
  description: "Historical record: Review of Task 43 Workflows As A Skill, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation"
  tags: [Memory, Analysis, TaskReview, Contextual, Archived, Historical]
---

# Task 43 Workflows As A Skill Review

## Archive Status

Archived on 2026-10-04 from `.agents/memory/emerging/analysis/open-task-review/task43-workflows-as-skill.md` after the maintainer selected Memory cleanup. Superseded analytical baseline; preserve original bodies and current residue owners.

This record preserves historical evidence. The [current CLI development route](../../../../working/cli-development/_cli-development.md) and its open Tasks define current work. Local qualification in this record does not establish later integration or publication.

## Question

Is Task 43 still worth doing, what exactly remains, and when should it run?

**Task record:** [Task 43](../../tasks/task43-workflows-as-skill.md)

## Current Conclusion

**Recommendation:** Close as done.

**Size:** Small, because only record keeping remains.

Workflows are now recipes behind the native `use-workflow` Skill from the optional Workflow Support package. Core has no Workflows root. All three acceptance criteria hold in the shipped files. The remaining "historical upgrade evidence" has no users behind it, for the same reason as in the [Task 42 review](task42-minimal-core.md). The old `src/open-forge/.agents/workflows/` root was deleted in `dac3bb53` on 2026-09-21, and the `0.9.0-beta.1` tag, the first npm publication, already ships only the Skill. The one friction left for users, which is that default `index` does not list a new recipe, belongs to Tasks 46 and 47, not here.

## What It Implies

Closing changes nothing in the product. It removes a delivered item from the beta ordering and stops two tasks from appearing to own the same Skill-resource problem.

## State Today

Verified against `815324f9`.

| Acceptance item                                      | State today                                                                                                                                                                                                                                                                                                                                                                                                     |
| ---------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| One sentence separates a Skill from a Workflow       | Done. The [dictionary](../../../../crystallized/documents/maintenance/helpers/dictionary.md#workflow-recipes) defines a Workflow as a repeatable procedure selected through a Skill, not a specialized capability. The site's `concepts/core-categories.md` says the same                                                                                                                                       |
| A user adds a workflow without editing managed files | Met, with friction. A user adds a recipe with `Goal`, `Steps`, and `Completion` under `.agents/skills/use-workflow/references/<scope>/`, using the Workflow template. Only the generated `Entries` region of the package-owned catalogue changes. `extension update` treats that region separately (`ExtensionUpdateGeneratedRegionState`). The user must still run `open-forge index` on the catalogue by name |
| The loader and payload agree                         | Done. The loader lists no Workflows root. The [Workflow Support contract](../../../../crystallized/documents/maintenance/payload/agents/workflows.md) describes the Skill and catalogue                                                                                                                                                                                                                         |

The three open questions in the record are answered:

- A user workflow is a recipe file in a scope under the catalogue, started from the Workflow template.
- The `workflows` root route is gone. It did not become an Extension route either.
- Existing workflows are covered by the Task 42 upgrade reasoning, since no public install had the old root.

The record calls its problem statement historical but still lists these as open, so it reads as more open than it is. The Decisions [routing-surfaces](../../../../crystallized/decisions/framework/routing-surfaces.md) and [workflow-shape](../../../../crystallized/decisions/framework/workflow-shape.md) carry supersession notes, so that coordinator step is done too.

## Dependencies

- **Blocked by:** nothing.
- **Blocks:** nothing.
- **Overlaps with:** [Task 46](../../tasks/task46-routed-skill-resources.md) and the [Task 47 follow-up](../../tasks/task47-default-skill-indexing.md), which own the default Index gap. [Task 61](../../../../working/cli-development/tasks/task61-documentation-accuracy-and-voice.md) found that `extension update` reports the `use-workflow` catalogue's `Entries` as updated when the file does not change. That defect belongs to the CLI output tasks.

## Remaining Work

1. Rewrite the task state to say the three acceptance criteria hold, and answer the open questions in one line each.
2. Point the default Index friction to the Task 47 follow-up.
3. Mark the task complete and archive it with Task 42.

## Pros And Cons

| Pros                                                        | Cons                                                                                             |
| ----------------------------------------------------------- | ------------------------------------------------------------------------------------------------ |
| Removes a delivered item from the active list               | The user-authored workflow journey keeps its manual `index` step until Task 47's follow-up lands |
| Leaves one owner for the Skill-resource navigation question |                                                                                                  |

## Risks And Open Questions

- User recipes live inside a directory another package installs. Removing Workflow Support could affect them. The [Extensions guide](../../../../../../docs/extensions.md#moving-from-the-earlier-package-layout) says removal may refuse to delete a required entrypoint that still has user records beneath it, but I found no test for this recipe directory. Check it when Task 63 or Task 47 adds journeys for this directory.
- The Skill's `description` names vision, architecture, planning, and other methods even when Workflow Support is installed alone with no recipes. Agents may select it and find an empty catalogue. The Skill then tells them to work directly, so the cost is low. Consider this in Task 44's content pass rather than reopening this task.

## Next Check

**Action:** the maintainer confirms closure, then the record is updated and archived together with Task 42.

**Would change the conclusion:** a decision that user-authored recipes must live outside package-owned directories, or evidence that agents do not notice the `use-workflow` Skill in the harnesses that matter.

**Acceptance needed:** the maintainer.
