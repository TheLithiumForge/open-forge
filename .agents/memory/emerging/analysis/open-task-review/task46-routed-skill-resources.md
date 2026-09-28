---
open-forge:
  description: Review of Task 46 Routed Skill Resources, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation
  tags: [Memory, Analysis, TaskReview, Contextual, Candidate]
---

# Task 46 Routed Skill Resources Review

## Question

Is Task 46 still worth doing, what exactly remains, and when should it run?

**Task record:** [Task 46](../../../archived/cli-development/tasks/task46-routed-skill-resources.md)

## Current Conclusion

**Recommendation:** Fold into Task 47, specifically its [default Skill indexing follow-up](../../../working/cli-development/tasks/task47-default-skill-indexing.md).

**Size:** Small, because the fold is only a record merge. The design work is sized in the [Task 47 review](task47-entrypoint-reachability.md).

Task 46 posed two decisions. One is done in practice and the other is exactly the question the Task 47 follow-up already owns. Its headline problem, that installing any first-party Extension makes `doctor` fail with thirteen warnings, no longer reproduces. Keeping two records for one open design question invites two answers.

## What It Implies

Folding changes nothing for users. It leaves one owner for "can a native Skill host routed resources, and does default navigation reach them". For the Framework, whichever model the follow-up picks becomes the single contract that the Skills and Workflow Support maintenance contracts both follow.

## State Today

Verified against `815324f9`. The Index resolver and route topology builder have not changed since `dac3bb53` on 2026-09-21.

- **Doctor on a fresh install:** fixed. The 2026-09-21 [navigation recheck](../../../working/cli-development/tasks/beta-follow-ups/task46-47-navigation.md) installed Core and every bundled Extension. It found no Doctor findings across 58 sources. `RouteDoctorFactReader.cs` accepts valid native Skill metadata and readable Skill resources. The record's body still describes the failure as current, and only its header marks it superseded.
- **How a Skill's metadata is read:** answered in practice. `SourceFormClassifier.cs` classifies `SKILL.md` as its own source form. Generated Skills `Entries` use the native `description`, as the Core [Skills entrypoint](../../../../../src/open-forge/.agents/skills/_skills.md) shows for `open-forge-cli`. No `open-forge:` block is required. The [Skills maintenance contract](../../../crystallized/documents/maintenance/payload/agents/skills.md) says Open Forge imposes no internal schema on a Skill package. What remains is making this an explicit contract rule rather than an implementation fact.
- **Whether Skill resources are routes:** still open. Topology admits native Skills as sources, but only recognized entrypoints provide parent relationships. So `references/_references.md` under `use-workflow` stays detached from the loader. The Task 47 follow-up owns this.

There is also a wording tension to settle in the same model. The [Workflow Support contract](../../../crystallized/documents/maintenance/payload/agents/workflows.md) says the Skill mechanism does not make its resources loader-reachable routes. The Skills contract's verification line mentions "loader-to-resource inheritance". Both cannot be the target state.

## Dependencies

- **Blocked by:** nothing.
- **Blocks:** nothing on its own. [Task 60](../../../archived/cli-development/tasks/task60-cli-skill.md) mentions using routed resources for a CLI command reference. That idea waits on the Task 47 model, not on this record.
- **Overlaps with:** Task 47 and its follow-up (same root cause), and [Task 43](../../../archived/cli-development/tasks/task43-workflows-as-skill.md), whose recipe layout created the case.

## Remaining Work

1. Move the surviving question, "can a native Skill host routed resources, and on what terms", and the metadata rule into the Task 47 follow-up's "Decisions to freeze" list.
2. Record that the thirteen-warning report and the metadata question are resolved, with the recheck packet as evidence.
3. Close Task 46 as folded and archive it with a link to the follow-up.

## Pros And Cons

| Pros                                                     | Cons                                                                                   |
| -------------------------------------------------------- | -------------------------------------------------------------------------------------- |
| One owner and one answer for one design question         | Loses a standalone record of the original metadata argument, though archiving keeps it |
| Removes a record whose main symptom no longer reproduces |                                                                                        |

## Risks And Open Questions

- The record's boundary says "do not add an `open-forge:` block to a native `SKILL.md`" and "do not move recipes back under a routed root". Carry both constraints into the follow-up so they are not lost in the fold. The first matters more now. The Task 62 analysis reports, not yet confirmed, that several other tools read Skills natively from `.agents/skills/`.
- [Task 62](../../../working/cli-development/tasks/task62-glob-scoped-loading.md) notes that Open Forge already reads a root-level `description` for `SKILL.md`. If Task 62 accepts root-level `description` for ordinary files, the Skill metadata rule becomes a case of that general rule. Decide the follow-up's metadata wording with that in view.

## Next Check

**Action:** merge the open question and constraints into the Task 47 follow-up, then close Task 46.

**Would change the conclusion:** a new fresh-install Doctor failure involving Skill resources, which would make this a live defect again rather than a design question.

**Acceptance needed:** the maintainer, for closing and merging records.
