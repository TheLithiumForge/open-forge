---
open-forge:
  description: "Historical record: Review of Task 36 Extension Merge and Guards, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation"
  tags: [Memory, Analysis, TaskReview, Contextual, Archived, Historical]
---

# Task 36 Extension Merge And Guards Review

## Archive Status

Archived on 2026-10-04 from `.agents/memory/emerging/analysis/open-task-review/task36-extension-merge-and-guards.md` after the maintainer selected Memory cleanup. Superseded analytical baseline; preserve original bodies and current residue owners.

This record preserves historical evidence. The [current CLI development route](../../../../working/cli-development/_cli-development.md) and its open Tasks define current work. Local qualification in this record does not establish later integration or publication.

## Question

Is Task 36 still worth doing, what exactly remains, and when should it run?

**Task record:** [Task 36](../../../../working/cli-development/tasks/task36-extension-merge-and-guards.md)

## Current Conclusion

**Recommendation:** Needs a maintainer decision first.

**Size:** Medium for the guard question, because it changes a file format every workspace carries and needs a migration. Partial merging would be Large.

Split the task. Question 1, partial merging by Extensions, contradicts accepted direction and has no concrete package that needs it. Close it or park it in the [Extensions Evolution idea](../../../../emerging/ideas/extensions-overhaul.md). Question 2 is now much narrower than the record says. Only the `<!-- open-forge:start -->` and `<!-- open-forge:end -->` pair in `AGENTS.md` and `CLAUDE.md` remains. A second record, the [Task 30 structural subtask](../../tasks/task30/phase-4a-structural.md#managed-host-heading-direction), already holds a competing "`# Open Forge` heading" direction for the same boundary. Merge the two into one record. If the format changes, change it before 1.0 so 1.0 does not freeze a format the maintainer wants gone.

## What It Implies

For Question 1, Extensions could write part of a shared file. That reverses invariant 6 of the [Extensions Architecture](../../../../crystallized/documents/extensions/architecture.md) ("Packages contribute whole files"), weakens invariant 9 (manual installation by copying files), and reopens the [Extension Package Boundary decision](../../../../crystallized/decisions/extensions/extension-package-boundary.md), which rejected inline mutations because they "obscure authorship, complicate conflicts, and make manual installation and removal less reliable."

For Question 2, users would see a visible section boundary in `AGENTS.md` and `CLAUDE.md` instead of HTML comments. The CLI needs a new locator, an automatic migration of existing comment pairs, and updated install and update contracts. The `open-forge` region claim in the lock stays.

## State Today

- **Entries guards are gone.** B1 moved `Entries` to heading-based lookup. `MarkdownEntriesSectionReader` still recognizes the old `generated-index` markers only to remove them.
- **Root host guards remain.** `FrameworkContentIdentity.ReadManagedBlock` finds the pair with `string.IndexOf`, not Markdig. That is a hand-rolled scanner, which the task's own boundary forbids for the replacement.
- **The Prettier acceptance item is moot.** `.prettierignore` holds no guard containment, and its history since `768bd51a` never did. Whether Prettier changes the root host comments is unverified, because `npm run verify` runs only the delivery format check.
- **A heading-only boundary fails on this repository.** The root `AGENTS.md` has `## Exact Mechanical Execution Exception` after the end marker. A `# Open Forge` section that runs to the next top-level heading or end of file would absorb it, and the next update would replace it.
- **A `---` pair has a first-line risk.** Install creates `CLAUDE.md` with the block on line 1. A leading `---` makes many tools read the block as YAML frontmatter, and `@AGENTS.md` is not valid YAML. How Claude Code and other harnesses treat that is unverified.
- **A hybrid avoids both problems.** Start at a `# Open Forge` heading and end at a thematic break. Markdig exposes both as blocks, the end is explicit, and nothing starts with `---`.
- **The agent-reading experiment only matters for fences.** Heading and thematic-break forms keep the instructions as plain Markdown. Dropping the fenced-block candidate removes the most expensive step in the record.

## Dependencies

- **Blocked by:** the maintainer's choice of boundary and the disposition of Question 1.
- **Blocks:** nothing hard. [Task 44](../../../../working/cli-development/tasks/task44-template-content.md) edits the same shipped `AGENTS.md` text.
- **Overlaps with:** the Task 30 structural subtask (a duplicate of Question 2), [Task 63](../../../../working/cli-development/tasks/task63-keeping-edits-through-updates.md) (listing `AGENTS.md` in `removedFiles` is how a user keeps an edited block today), and [Task 55](../../tasks/task55-alternative-root.md) (another root may change the host files).

## Remaining Work

1. Decide Question 1: close it, or move it to the Extensions Evolution idea with the accepted invariants it would change.
2. Merge the Task 30 managed-host heading direction into Task 36, or the reverse, so one record owns the boundary.
3. Choose among keeping the comments, heading plus closing `---`, and a `---` pair. Check each against Prettier, Markdig, GitHub rendering, and how Claude Code, Codex, and Copilot read a host whose block starts on line 1.
4. Specify the boundary and the migration in the [install](../../../../crystallized/documents/cli/contracts/install/behavior.md) and [update](../../../../crystallized/documents/cli/contracts/update/behavior.md) contracts. The migration must keep text that follows an old end marker outside the block.
5. Replace `ReadManagedBlock` with a Markdig locator. Update `src/open-forge/AGENTS.md`, `src/open-forge/CLAUDE.md`, journey F01, `UpdateManagedHostIntegrationTests`, `InstallPreservationIntegrationTests`, and `docs/cli.md`.
6. Add the record's test that appends prose in every position around the boundary.

## Pros And Cons

| Pros                                                                      | Cons                                                                                  |
| ------------------------------------------------------------------------- | ------------------------------------------------------------------------------------- |
| Removes the last comment guards, as the maintainer directed on 2026-09-12 | The comments work today, are tested, and are hidden in rendered previews              |
| Replaces a string scanner with the parser the rest of the CLI uses        | Every installed workspace needs a migration of two files                              |
| A visible boundary reads well in raw text and previews                    | A wrong end rule silently absorbs user text, as this repository's `AGENTS.md` shows   |
| Closing Question 1 keeps the whole-file package model intact              | Question 1's use case, an Extension adding a section to a shared file, stays unserved |

## Risks And Open Questions

- Should partial merging stay rejected? If the maintainer wants it, it needs a decision that changes the Extensions Architecture first, not a task.
- Is changing the host format worth a migration at all, given the comments already work?
- Harness behavior for a host that begins with `---` must be tested before that option is chosen.

## Next Check

**Action:** Ask the maintainer to close Question 1 and pick one owner record for the host boundary.

**Would change the conclusion:** A concrete Extension that cannot work without editing a shared file, or evidence that Prettier or a harness already mishandles the comment pair.

**Acceptance needed:** The maintainer, since both questions change accepted Framework or Extension direction.
