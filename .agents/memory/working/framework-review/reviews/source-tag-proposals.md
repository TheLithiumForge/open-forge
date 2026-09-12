---
open-forge:
  description: Recorded tag audit conclusions and candidates requiring a separate decision
  tags: [Memory, Working, Contextual, Framework, Review]
---

# Source tag audit

This is an earlier review snapshot retained for the active Task 28 Git review. Its status and paths describe the recorded stage. Use [Task 28](../../cli-development/tasks/source-framework-review.md) for current decisions, completion, and remaining work.

This audit covers the 44 Markdown and native Skill files under `src/open-forge/**/.agents` and `src/extensions/**/.agents` at baseline `28cac0fc47e43671aa6458d318293594542d9bbf`. It excludes CLI implementation and contracts, runtime/APM agents, stored Memory records, and package README or manifest files.

The complete per-file inventory is in `source-tag-audit.json`. The defined behavior tags are `LoadNow`, `KeepInMind`, `Core`, `Memory`, `Extension`, `Contextual`, `CurrentTruth`, and `Evergreen`. All other tags in this source set are ordinary classification, topic, or search signals. The loader itself has no frontmatter tags because its contract is authored directly.

The source set uses the defined tags consistently:

- `LoadNow` marks baseline category or role entrypoints whose omission would make the Framework harder to enter or use.
- `KeepInMind` marks target-sensitive continuity at the Emerging and Observations boundaries. It does not activate an unselected scope.
- `Core`, `Memory`, and `Extension` identify composition and provenance. They do not create authority.
- `Contextual` marks archived, emerging, working, or otherwise unsettled material. `CurrentTruth` marks accepted Crystallized roles. The shipped source files do not use `Evergreen`, which is appropriate because these are payload contracts rather than synchronized dogfood views.

The 29 generated linked entries are exact: each entry's tags match its target frontmatter tags, and each label matches the target description. Empty generated regions use the `#Empty` sentinel. No generated-entry correction is proposed.

## Proposed changes for discussion

No tag should be added or removed before discussion. Most tags are useful and already express a narrow distinction. The only meaningful discussion candidate is the use of `Contextual` on four Memory Template leaves:

| ID | Files | Question | Effect if changed | Recommendation | Confidence |
| --- | --- | --- | --- | --- | --- |
| ST-001 | `templates/memory/{analysis,handoff,idea,observation}.md` | Does `Contextual` describe the Template source, or only the state of the artifact it helps create? | Removing it would make the Template source less likely to be mistaken for a contextual Memory record, but generated navigation would lose a useful signal about the destination state. Keeping it communicates the intended destination role while `Template` remains the primary source type. | Keep for now and state the convention explicitly in the Templates contract. Reconsider only if dogfood shows agents treating these Templates as active Memory records. | Medium |

The `Extension` tag on packaged files should remain. It communicates package provenance and composition without authority. Removing it would make installed package origin less visible in route indexes. The `Memory` tag on the Work Records Pattern and Memory Templates is an ordinary composition/topic signal and should remain.

The `CurrentView` tag on document Templates is an ordinary topic label for the intended result. It is not `CurrentTruth`, so it does not claim that a copied result is accepted. Keep it unless a clearer established topic vocabulary replaces it across the package.

The absence of Open Forge tags from `experience-design/SKILL.md` and its references is correct. The native Skill contract owns their metadata, activation, resources, and runtime behavior. Adding Open Forge frontmatter or generated entries would create a competing interpretation surface.

The on-demand `templates/_templates.md` category has no loading tag by design. Its `Core` and `Template` tags remain sufficient for route selection after the category is selected. Adding `LoadNow` would increase baseline context without evidence that every task needs Template authoring instructions.

## `KeepInMind` name verdict

Keep the name for now. It accurately signals continuity that should be revisited at task start, resume, restoration, handoff, closeout, and relevant active-scope changes. The loader contract narrows the name operationally: `KeepInMind` follows loaded parent routes and active scopes; it does not mean global recall, permanent authority, or automatic activation.

The name may invite a global-memory interpretation when read without the loader. That is a documentation clarity issue, not enough evidence for a tag rename. Add a short plain-language explanation wherever the tag is introduced if dogfood readers continue to misread it. Renaming would affect the loader, source tags, generated entries, maintenance contracts, and CLI vocabulary, so it requires a separate accepted decision and compatibility plan.

## Verdict

The inventory has no confirmed source-tag defect and no generated-Entry mismatch. There is one medium-confidence convention question, ST-001, which should be discussed before changing tags. All proposed effects and confidence levels are intentionally recorded before any additions or removals.
