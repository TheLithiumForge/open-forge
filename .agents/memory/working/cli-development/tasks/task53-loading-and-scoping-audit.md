---
open-forge:
  description: Open Task 53 to audit every LoadNow and KeepInMind entry and the default scoping before 1.0, so startup context holds only what omission would cost more than reading
  tags: [Memory, Working, Task, Framework, Loading, Scope, Release, Contextual, Active]
---

# Task 53 — Loading and scoping audit for 1.0

**Reviewed on 2026-09-28:** [review](../../../emerging/analysis/open-task-review/task53-loading-and-scoping-audit.md). Recommendation:
Do next. The review names any details in this record that are out of date.

## Outcome

Recorded at the maintainer's request on 2026-09-25 as 1.0 polish. Every
`#LoadNow` and `#KeepInMind` entry is justified under the loader's
[Choosing Loading Tags](../../../../loader.md) rules, and default scoping puts
specialized material behind a scope instead of at startup. The result is a
smaller, deliberate startup context for fresh installs and for this workspace.

**Direction:** the maintainer asked for a task, not implementation. This record
selects the work for specification. It authorizes no Framework, payload, or
workspace mutation. Proposed changes to shipped loading behavior are Framework
changes and follow the [deliberate Framework change](../../../../directives/open-forge/framework/_framework.md)
Directive once accepted.

**In scope:**

- The shipped payload in `src/open-forge/`: 8 `#LoadNow` and 1 `#KeepInMind`
  entry lines (six root entrypoints, Memory, Working, Crystallized, and Emerging).
- First-party Extensions in `src/extensions/`: none carry a loading tag today.
  Confirm that on-demand is still right for each Memory category they add.
- This repository's workspace `.agents/`: 58 `#LoadNow` and 15 `#KeepInMind`
  entry lines. The nine root Directive files total about 44 KB, and
  `hierarchical-orchestration.md` alone is about 13 KB.

**Preserve / out of scope:** the loader's tag semantics themselves, unless the
audit finds a rule that cannot be applied consistently. Tag vocabulary beyond
the two loading tags belongs to [Task 54](task54-tag-trimming.md).

**Done when:**

- [ ] One audit table lists every loading-tagged entry with its current cost,
      the cost of omitting it, and a keep, scope, or remove recommendation.
- [ ] The maintainer accepts or rejects each recommendation.
- [ ] Accepted payload changes ship with updated maintenance contracts, the
      README and development-guide token figures, and the documentation site's
      [loading table](../../../../../src/docusaurus/docs/concepts/loading-and-tags.md).
- [ ] Before-and-after startup measurements use the method in the
      [development guide](../../../../../docs/development.md#measure-context-size).
- [ ] Doctor stays clean and the payload snapshot tests pass.

## Plan

1. **Inventory.** List every entry line carrying `#LoadNow` or `#KeepInMind`
   in the payload, Extensions, and workspace, with byte and token size.
2. **Question the payload defaults.** For each root entrypoint, decide whether
   its Axioms must be visible at startup or whether the loader's one-line entry
   is enough. Candidate questions:
   - Guidance, Patterns, and Maps start empty in a fresh install, and Skills
     lists only the `open-forge-cli` Skill. Does reading their entrypoints at
     startup earn its cost before they have entries?
   - The loader's CLI section overlaps the `open-forge-cli` Skill. Should it
     shrink now that the Skill exists? See [Task 60](../../../archived/cli-development/tasks/task60-cli-skill.md).
   - Crystallized starts empty. Does it need `#LoadNow` before Extensions add
     Decisions or Documents?
   - Does Emerging need `#KeepInMind` in a base install without the Observations
     or Ideas categories?
   - Planning's Checkpoints category tells agents to tag an active Checkpoint
     `#KeepInMind`, but the category's own entry in Working carries no loading
     tag. Tags act only through loaded parents, so the Checkpoint refreshes only
     after the category has been opened. Found during
     [Task 61](task61-documentation-accuracy-and-voice.md). Decide whether the
     category entry needs a tag or the Checkpoint guidance needs rewording.
3. **Question the workspace.** Identify root Directives that apply only to a
   kind of work, such as orchestration, review, or C#, and propose scopes for
   them. Retire stale `#KeepInMind` Working records left from beta preparation.
4. **Recommend.** Produce the audit table with one recommendation per entry and
   the startup delta of the whole proposal.
5. **Apply accepted changes** in payload, workspace, contracts, docs, and site,
   then measure and verify.

## Current State

**Now:** recorded, not started.

**Provenance:** the [Framework Context And Authoring candidate](potential/framework-context-and-authoring.md)
already asks whether only actionable leaf Directives should carry `#LoadNow`
and what startup budget applies. Promote its loading questions into this task
rather than keeping two answers. The [Authors' Findings](../../../emerging/authors-findings/_authors-findings.md)
on active-context size are related evidence.

## Folded in on 2026-09-28

- **From [Task 60](../../../archived/cli-development/tasks/task60-cli-skill.md): should the loader's CLI section shrink
  to a pointer?** The [review](../../../emerging/analysis/open-task-review/task60-cli-skill.md) proposes one
  sentence saying the CLI is optional and the files stay complete without it,
  pointing to the `open-forge-cli` Skill and `open-forge --help`. The Skills
  entrypoint loads at startup, so the Skill stays reachable, and the change
  saves about 225 tokens. After acceptance, update the loader in both copies,
  both maintenance contracts, the README, and `cli/index.md`, then refresh
  snapshots and measure startup again. A Task 58 demo in a harness without
  native Skills could confirm the pointer is enough.
- **From [Task 42](../../../archived/cli-development/tasks/task42-minimal-core.md): is each startup Core entrypoint
  worth its startup cost?** Add it to the audit table where the existing
  questions don't already cover it.
- **The shipped loader is over its budget.** It has 92 authored lines against
  its maintenance contract's 35 to 80. Settle the budget or the length here or
  in [Task 44](task44-template-content.md).
