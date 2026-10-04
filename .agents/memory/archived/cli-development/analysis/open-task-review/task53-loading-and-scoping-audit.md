---
open-forge:
  description: "Historical record: Review of Task 53 Loading And Scoping Audit, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation"
  tags: [Memory, Analysis, TaskReview, Contextual, Archived, Historical]
---

# Task 53 Loading And Scoping Audit Review

## Archive Status

Archived on 2026-10-04 from `.agents/memory/emerging/analysis/open-task-review/task53-loading-and-scoping-audit.md` after the maintainer selected Memory cleanup. Superseded analytical baseline; preserve original bodies and current residue owners.

This record preserves historical evidence. The [current CLI development route](../../../../working/cli-development/_cli-development.md) and its open Tasks define current work. Local qualification in this record does not establish later integration or publication.

## Question

Is Task 53 still worth doing, what exactly remains, and when should it run?

**Task record:** [Task 53](../../../../working/cli-development/tasks/task53-loading-and-scoping-audit.md)

## Current Conclusion

**Recommendation:** Do next. Start with the payload audit and the Checkpoints gap. Hold scoping decisions for file-specific workspace rules until the maintainer answers Task 62's semantics.

**Size:** Medium. The inventory is mechanical, but each change to shipped loading is a Framework decision and needs contracts, documentation, site, and snapshot updates.

The task sits at the head of a chain. Tasks 44 and 54 edit the same entrypoints and should follow it, and the Task 60 loader question is explicitly deferred to it. It also carries the one shipped loading defect in this group: an active Checkpoint cannot refresh until its on-demand category is opened. The payload part is small, with nine loading-tagged lines. The workspace part is where the tokens are.

## What It Implies

- **Users:** startup context stays about the same size or shrinks. For the payload the saving is modest. Guidance, Patterns, and Maps together are about 3.2 KB, and the loader alone is 11.5 KB of a startup set the guide estimates at about 5.8k tokens.
- **Framework:** each removed `#LoadNow` changes what every workspace sees at startup. Decide each one against the rule in the loader's "Choosing Loading Tags".
- **CLI:** no code change. Install and Status snapshots record token counts and must be refreshed.
- **This workspace:** the larger effect. The eight root Directive files are about 37 KB, and `hierarchical-orchestration.md` alone is 12.9 KB.

## State Today

Verified against `815324f9`. Not started.

- The payload has 8 `#LoadNow` and 1 `#KeepInMind` entry lines, as recorded. No Extension frontmatter carries a loading tag.
- Guidance, Patterns, Maps, Crystallized, Emerging, and Working all ship with `- none` Entries.
- This workspace has 58 `#LoadNow` entry lines, as recorded. The record's 15 `#KeepInMind` lines are stale. Task 56 archived the beta preparation records. Five tagged entry lines remain: Emerging, Authors' Findings, `cli-development`, the Task 30 conventions, and the Task 31 phase packet. A few more matches are example text inside idea files.
- The Checkpoints gap is real. The Planning [Checkpoints entrypoint](../../../../../../src/extensions/planning/content/.agents/memory/working/checkpoints/_checkpoints.md) tells agents to tag an active Checkpoint `#KeepInMind`. The category's own entry in Working has no loading tag, so the child refreshes only after someone opens the category.
- The loader has 92 non-empty authored lines, above the 35 to 80 budget in its [maintenance contract](../../../../crystallized/documents/maintenance/payload/agents/loader.md).
- The [Framework Context And Authoring candidate](../../../../working/cli-development/tasks/potential/framework-context-and-authoring.md) still holds the "only leaf Directives carry `#LoadNow`" question. The record's plan to promote it into this task has not been done.

## Dependencies

- **Blocked by:** nothing for the payload audit. For workspace rules tied to file types, such as the C# and TypeScript scopes, the [Task 62 analysis](../glob-scoped-loading.md) proposes `applyTo` as a third narrowing option next to scopes and loading tags. The audit table needs a "glob" column if that is accepted. Payload verification also needs the Install and Status snapshots that [Task 60](../../tasks/task60-cli-skill.md) left unrefreshed.
- **Blocks:** Task 54, which should run after or alongside it because they edit the same frontmatter and `Entries`. It also blocks Task 44's content pass and the Task 60 loader decision.
- **Overlaps with:** [Task 62](../../../../working/cli-development/tasks/task62-glob-scoped-loading.md). Both would edit the loader's "Tags And Loading" section and the site's loading table. Batch those edits.

## Remaining Work

1. Recount and build the inventory table with bytes and estimated tokens.
2. Fix the Checkpoints gap. The simplest option is `#KeepInMind` on the Planning Checkpoints entrypoint's own frontmatter, about 1.5 KB per refresh in workspaces with Planning. The other option is to reword the Axiom so resuming agents open the category.
3. For each payload entrypoint, decide keep or on demand. Empty categories are the question. Crystallized and Emerging carry capture rules that matter even when empty. Guidance, Patterns, and Maps mostly say "check `Entries`".
4. Decide the loader CLI section with the Task 60 review.
5. For the workspace, propose scopes for work-specific root Directives, starting with `hierarchical-orchestration.md`. Mark file-type rules as glob candidates pending Task 62. Question the `#KeepInMind` on `cli-development`, which refreshes CLI task state in every task.
6. Promote the candidate's loading questions, apply accepted changes, update contracts, the README and guide figures, and the site table, then measure again.

## Pros And Cons

| Pros                                                 | Cons                                                                            |
| ---------------------------------------------------- | ------------------------------------------------------------------------------- |
| Fixes a real Checkpoint refresh defect               | Payload token savings are small                                                 |
| Gives each startup file a recorded reason before 1.0 | Removing `#LoadNow` from populated categories can hide Entries agents need      |
| Unblocks Tasks 44, 54, and the Task 60 decision      | Workspace changes need care, because this repository's own rules depend on them |

## Risks And Open Questions

- Making an empty category on demand saves tokens only while it stays empty. In a populated workspace, the agent then sees only a one-line entry. The maintainer should decide whether Core optimizes for fresh installs or for grown workspaces.
- A glob-gated `#LoadNow`, as Task 62 proposes, could replace some scoping moves. Deciding the workspace table before Task 62's semantics risks redoing it.

## Next Check

**Action:** build the inventory table and propose the Checkpoints fix. Both are independent of Task 62.

**Would change the conclusion:** a Task 55 decision to change the root before 1.0, which would move every audited file, or a maintainer decision to leave the payload's loading unchanged for 1.0.

**Acceptance needed:** the maintainer, for each loading change, as Framework changes.
