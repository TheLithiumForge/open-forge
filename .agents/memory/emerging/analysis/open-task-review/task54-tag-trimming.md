---
open-forge:
  description: Review of Task 54 Tag Trimming, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation
  tags: [Memory, Analysis, TaskReview, Contextual, Candidate]
---

# Task 54 Tag Trimming Review

## Question

Is Task 54 still worth doing, what exactly remains, and when should it run?

**Task record:** [Task 54](../../../working/cli-development/tasks/task54-tag-trimming.md)

## Current Conclusion

**Recommendation:** Do before 1.0, limited to shipped files. Run it after Task 53 and after the maintainer picks Task 62's entry-line form. Leave the workspace part for after 1.0.

**Size:** Small for shipped files: one tag rule and a decision on about 28 single-use tags across 66 files. It becomes Medium if the workspace's 390 tags are included.

The value is a coherent public vocabulary, not tokens. Tag suffixes in the payload's startup `Entries` total about 365 characters, roughly 90 tokens. Trimming them saves little. What matters is that shipped tags become the words users search with `find --tag` and copy into their own files. Changing them after 1.0 is a visible change. Every changed shipped byte also sends beta users through `update`, and edited files through recovery bundles. So batch it with Tasks 53 and 44 in one release. The workspace's own tags are this repository's business and can wait.

## What It Implies

- **Users** get fewer, more predictable tags on installed files, and a written rule for tagging their own.
- **Framework:** a short tag rule in the Markdown or loader contracts. Removals in 66 shipped frontmatter blocks and their generated `Entries`.
- **CLI:** no code change, as long as load-bearing tags stay. Snapshot tests change with the bytes.
- **Maintenance:** Extension pages on the site quote tags and need updating.

## State Today

Verified against `815324f9`. Not started.

- The recorded counts still hold for shipped files: 64 distinct tags across 66 tagged files, 28 of them on one file only.
- The workspace outside Archived now has 390 distinct tags across 574 tagged files, 135 used once. The record says 391 and 138.
- Some tags are load-bearing and must survive any rule:
  - `Template`: `RouteTemplateResolver.cs` refuses a `route create --template` source without it.
  - `Skill`: `GeneratedNavigationRegionPlanner.cs` writes it for native Skill entries.
  - `Decision` and `Memory`: the documentation uses them in `find --tag` examples.
  - The loader's defined tags: `Core`, `Memory`, `Extension`, `Contextual`, `CurrentTruth`, and `Evergreen`, plus the two loading tags.

## Dependencies

- **Blocked by:** [Task 53](../../../working/cli-development/tasks/task53-loading-and-scoping-audit.md), which edits the same frontmatter and decides loading-tag placement. [Task 62](../../../working/cli-development/tasks/task62-glob-scoped-loading.md)'s entry-line choice, because the [analysis](../glob-scoped-loading.md) offers two forms. A labeled `applies to` segment after the tags keeps tags bare. The alternative `#AppliesTo` marker tag followed by code spans would give tags arguments, which changes the grammar a tag rule describes.
- **Blocks:** Task 44's content pass, which should read files in their final frontmatter.
- **Overlaps with:** [Task 55](../../../archived/cli-development/tasks/task55-alternative-root.md), because APM frontmatter may bring its own keys. Task 62's root-level frontmatter proposal keeps `tags` scoped only, so it does not conflict.

## Remaining Work

1. Recount, and measure the tag cost in startup `Entries` for a fresh install and for an install with all Extensions.
2. Propose the rule. For example: defined tags where they apply, one role tag such as `Decision` or `Template`, and at most two subject tags an agent would plausibly search for.
3. Classify each shipped tag as defined, role, subject, or redundant. Check first the tags that restate the path (`Working`, `Crystallized`, `Emerging`, `Archived`), `Candidate` next to `Contextual`, and single-use tags such as `AgentLearning` and `Convergence`. Protect the load-bearing tags listed above.
4. After acceptance, apply removals to source and dogfood copies, rebuild `Entries`, update the site's Extension pages, and check the documented `find --tag` examples.
5. Record the rule for the workspace and leave its cleanup as an optional later task.

## Pros And Cons

| Pros                                                              | Cons                                                               |
| ----------------------------------------------------------------- | ------------------------------------------------------------------ |
| Fixes the shipped vocabulary before 1.0 makes it harder to change | Almost no startup token saving for the payload                     |
| Gives users a rule for their own tags                             | Another shipped-byte change for beta users to update through       |
| Easier `find --tag` results                                       | Removing a tag someone searches for silently shrinks their results |

## Risks And Open Questions

- Tags that restate the path look redundant, but `find --tag=Working` is a cheap query. The rule should say whether search convenience justifies a tag.
- Should the tag rule live in the loader, which costs startup tokens, or in the Markdown syntax contract, which costs nothing? The contract is the natural place.
- If the maintainer picks the marker-tag form for Task 62, the rule must define a reserved tag with arguments.

## Next Check

**Action:** after the Task 53 inventory, draft the tag rule and the shipped removal list for one review.

**Would change the conclusion:** evidence that agents select files noticeably better with richer tags, or a decision to freeze shipped frontmatter until after 1.0.

**Acceptance needed:** the maintainer, since shipped frontmatter changes are Framework changes.
