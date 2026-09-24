---
open-forge:
  description: Open Task 54 to trim tags that add no selection, search, or loading value before 1.0, in shipped files first and then in this workspace
  tags: [Memory, Working, Task, Framework, Tags, Release, Contextual, Active]
---

# Task 54 — Tag trimming for 1.0

## Outcome

Recorded at the maintainer's request on 2026-09-25 as 1.0 polish. Every tag on
a shipped file earns its place: it controls loading, classifies status or
Framework part, or helps an agent select or find the file. Tags that only
repeat the path, the description, or another tag are removed.

**Direction:** the maintainer asked for a task, not implementation. This record
authorizes no mutation. Changes to shipped frontmatter are Framework changes
and follow the [deliberate Framework change](../../../../directives/open-forge/framework/_framework.md)
Directive once accepted.

**In scope:**

- Shipped payload and first-party Extensions: 64 distinct tags across 66
  tagged files. 28 tags appear on only one file. Files carry between two and
  eight tags, and most carry three or four.
- This repository's workspace outside Archived: 391 distinct tags, 138 of them
  used once.
- Generated `Entries` lines, which repeat each file's tags and so multiply
  their startup cost wherever the parent entrypoint loads.

**Preserve / out of scope:** the defined tags in the loader (`LoadNow`,
`KeepInMind`, `Core`, `Memory`, `Extension`, `Contextual`, `CurrentTruth`,
`Evergreen`) and their meaning. Loading-tag placement belongs to
[Task 53](task53-loading-and-scoping-audit.md). Archived records keep their
historical tags.

**Done when:**

- [ ] A short tag rule is proposed and accepted: which kinds of tag a shipped
      file should carry, for example one role tag plus subject tags that an
      agent would plausibly search for.
- [ ] Every shipped file's tags are checked against the rule, with a
      keep or remove decision recorded for single-use and redundant tags.
- [ ] Accepted removals are applied to frontmatter and regenerated `Entries`,
      with the Extension pages on the documentation site updated where they
      quote tags.
- [ ] `find --tag` examples in the README, guides, and site still return results.
- [ ] Doctor stays clean and the payload snapshot tests pass.

## Plan

1. **Measure.** List every tag with its file count and its character cost in
   generated `Entries` that load at startup.
2. **Classify.** Mark each tag as defined, role (such as `Template` or
   `Decision`), subject (such as `Testing`), or redundant. Candidates to check
   first: tags that restate the path (`Working`, `Crystallized`, `Emerging`,
   `Archived`), near-synonyms (`Candidate` next to `Contextual`), and
   single-use subject tags (`AgentLearning`, `Convergence`, `Record`).
3. **Propose the rule** and the removal list for the maintainer.
4. **Apply accepted removals** to the payload and Extensions, then to this
   workspace, rebuilding `Entries` and updating documentation.

## Current State

**Now:** recorded, not started. Do after or alongside Task 53, because both
edit the same frontmatter and generated `Entries`.

**Evidence note:** counts come from the frontmatter `tags` lists in
`src/open-forge/`, `src/extensions/`, and `.agents/` excluding Archived, on
2026-09-25. Recount before acting.
