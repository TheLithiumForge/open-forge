---
open-forge:
  description: "Historical record: Review of Task 44 Template And Core File Content, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation"
  tags: [Memory, Analysis, TaskReview, Contextual, Archived, Historical]
---

# Task 44 Template And Core File Content Review

## Archive Status

Archived on 2026-10-04 from `.agents/memory/emerging/analysis/open-task-review/task44-template-content.md` after the maintainer selected Memory cleanup. Superseded analytical baseline; preserve original bodies and current residue owners.

This record preserves historical evidence. The [current CLI development route](../../../../working/cli-development/_cli-development.md) and its open Tasks define current work. Local qualification in this record does not establish later integration or publication.

## Question

Is Task 44 still worth doing, what exactly remains, and when should it run?

**Task record:** [Task 44](../../../../working/cli-development/tasks/task44-template-content.md)

## Current Conclusion

**Recommendation:** Do before 1.0, as the last pass over shipped content after Tasks 53 and 54 and the Task 60 and Task 62 decisions.

**Size:** Medium, because it reads 15 Core files and 56 Extension content files as a new user would, but earlier passes already cleaned most of the wording.

The goal still matters. These files are what every agent loads and what a new user opens first. The record says "Open, not started", which is stale. Two beta passes already did much of the work. What they could not do is the part that makes this task distinct: deciding whether a file or section earns its place, and cutting or restructuring it. Running that now would collide with Tasks 53, 54, and 62, which edit the same loader, entrypoints, and frontmatter. It should run once those settle, so each file is judged in its 1.0 shape.

## What It Implies

- **Users** get shorter, clearer startup files and starters, and a smaller token cost if sections are cut.
- **Framework:** some Core Axioms may move, merge, or go. Each removal is a Framework change under the [deliberate Framework change](../../../../../directives/open-forge/framework/deliberate-framework-change.md) Directive.
- **CLI:** no code change. The payload snapshot tests, and the Install and Status integration snapshots that record exact file, character, and token counts, need refreshing.
- **Maintenance:** the payload maintenance contracts must follow any cut.

## State Today

Done since the task was raised on 2026-09-17:

- The [lossless wording pass](../../../beta-preparation/src-wording-proposal.md) read all 71 Markdown files under `src`, then applied eight changes W01 to W08 on 2026-09-22. It was lossless by design. It could not remove a rule or a file.
- The [beta templates work](../../../beta-preparation/beta-templates.md), approved 2026-09-23 and merged in `a887e730`, created Core Templates. It aligned all 27 starters, turned the instructions in 15 older starters into removable prompts, and removed repeated inherited rules from Core (R3 to R7).
- The `open-forge-cli` Skill was added on 2026-09-26, and Task 61 corrected its status wording.

Still open:

- No pass has asked, file by file, "does this earn its place for a new user?"
- [Task 61](../../../../working/cli-development/tasks/task61-documentation-accuracy-and-voice.md) explicitly excluded shipped Framework and Extension content.
- The authored loader has 92 non-empty lines before `Entries`. Its [maintenance contract](../../../../crystallized/documents/maintenance/payload/agents/loader.md) sets a review budget of 35 to 80, so it has been over budget since at least `dac3bb53`.
- `memory/_memory.md` is 3.4 KB, about three times any other Core entrypoint, and loads at startup.

## Dependencies

- **Blocked by:** [Task 53](../../../../working/cli-development/tasks/task53-loading-and-scoping-audit.md), because it decides which entrypoints load. [Task 54](../../tasks/task54-tag-trimming.md), because it rewrites every file's tags. [Task 62](../../../../working/cli-development/tasks/task62-glob-scoped-loading.md), because it may add a frontmatter field and loader wording. The [Task 60](../../tasks/task60-cli-skill.md) loader decision. [Task 55](../../tasks/task55-alternative-root.md), because a root change would move every file.
- **Blocks:** nothing directly. It should finish before the 1.0 release.
- **Overlaps with:** [Task 37](../../../../working/cli-development/tasks/task37-wording-review-against-proposals.md), which reviews CLI wording, not installed files.

## Remaining Work

1. Rewrite the record: list what the two beta passes covered, and narrow the scope to the remaining questions of place, size, and new-user clarity.
2. After 53, 54, 60, and 62 settle, read each Core file as a new user. Record keep, cut, or rewrite for each section, starting with the loader and `memory/_memory.md`.
3. Read each Extension starter and entrypoint the same way. Check that each `description` matches the body. Include the `use-workflow` description, which names methods even when no recipe is installed.
4. Bring the loader within its 80-line budget, or have the maintainer revise the budget.
5. Apply accepted changes to source and dogfood copies, the maintenance contracts, and the site's Framework pages. Refresh snapshots and measure startup again.

## Pros And Cons

| Pros                                                | Cons                                                   |
| --------------------------------------------------- | ------------------------------------------------------ |
| Improves the text every agent and user reads first  | A third pass over the same files, with falling returns |
| Can shrink startup tokens by cutting whole sections | Cuts to Axioms can change behavior and need review     |
| Brings the loader back within its own contract      | Snapshot and contract churn with every shipped change  |

## Risks And Open Questions

- Without a clear rule for what earns its place, the pass turns into another rewording round. The maintainer should approve the keep-or-cut criterion first. One option is Task 53's rule: keep text whose omission costs more than reading it.
- Should Task 44 merge with Task 53 into one 1.0 content pass? They share files and verification. Keeping them separate is simpler to review. Merging saves one snapshot cycle.

## Next Check

**Action:** update the Task 44 record to reflect the completed beta passes and the narrowed scope, and order it after Tasks 53 and 54 in the 1.0 list.

**Would change the conclusion:** a decision to freeze shipped content for 1.0 after Tasks 53 and 54, or a Task 55 decision to move the root, which would make a later pass necessary anyway.

**Acceptance needed:** the maintainer, for the scope and for each Framework cut.
