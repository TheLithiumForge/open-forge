---
open-forge:
  description: Open Task 53 to audit every LoadNow and KeepInMind entry and the default scoping before 1.0, so startup context holds only what omission would cost more than reading
  tags: [Memory, Working, Task, Framework, Loading, Scope, Release, Contextual, Active]
---

# Task 53 — Loading and scoping audit for 1.0

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
   - Guidance, Patterns, Maps, and Skills start empty in a fresh install. Does
     reading their entrypoints at startup earn its cost before they have entries?
   - Crystallized starts empty. Does it need `#LoadNow` before Extensions add
     Decisions or Documents?
   - Does Emerging need `#KeepInMind` in a base install without the Observations
     or Ideas categories?
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
