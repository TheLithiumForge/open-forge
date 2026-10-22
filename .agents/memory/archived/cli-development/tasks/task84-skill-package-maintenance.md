---
open-forge:
  description: "Historical record: Index fills missing metadata in user-owned Skill packages and Repair maintains their links, released in beta17"
  tags: [Memory, Task, CLI, Index, Repair, Doctor, Skill, Contextual, Complete, Archived, Historical]
---

# Task 84: Skill package maintenance

## Outcome

Index and Repair maintain a Skill package the user owns the way they maintain any other user-owned Markdown. A `SKILL.md` or a package reference without frontmatter gets it from Index, and a renamed or moved package file gets its links repaired.

The maintainer reported on 2026-10-21: “Skills are not auto edited to add the frontmatter if it's missing, same for their references. We should not have such randim exclusions. Since they are more breaking than having them.”

Status: Task 84 “Skill package maintenance” is complete and shipped in beta17.

## Current behavior (beta15, reproduced 2026-10-21)

In a Full Core workspace, a new `.agents/skills/release-notes/SKILL.md` and `references/style.md`, both without frontmatter:

- Index stops for the whole workspace: “The Entries sections could not be rebuilt completely. Nothing was changed.”, because the `SKILL.md` frontmatter “could not be read completely”.
- Doctor says the Skill “needs its package metadata. Complete its native Skill metadata by hand.” and that the reference needs a description and tags because “Index does not edit files inside a Skill package, so add them by hand.” It also warns that the reference route “cannot be reached”.
- With the same frontmatter written by hand, Index adds the Skill entry and Doctor reports no problems, so no entrypoint inside the package is needed.

The exclusion lives in one shared protection rule: `RoutedMetadataCompletionPolicy` treats a `SKILL.md` and every file beneath its folder as `NativePackage`, and the Repair protection reader refuses links inside a native Skill package.

## Accepted behavior

- A `SKILL.md` without native metadata gets `name` and `description` at the frontmatter root, the native Skill format, whatever the workspace frontmatter form. A present key is kept.
- Other Markdown files inside the package get Open Forge `description` and `tags` like any routed file, in the workspace frontmatter form.
- Index never stops the whole run because a user-owned `SKILL.md` lacks metadata, and the new Skill gets its Skills entry in the same run.
- Doctor's next step for missing metadata in a Skill package is Index, not a manual edit.
- Repair, Route Move and Route Remove maintain links inside user-owned Skill packages like other user-owned files.
- Skills installed by the Framework or an Extension stay managed content owned by their Update, as every managed file is.

## Working assumptions

- `name` is the Skill folder name, as Install adoption already writes it.
- `description` is the first heading, or the file path when the Skill has no heading, as Install adoption already derives it.
- `SKILL.md` gets no Open Forge `tags`, matching the shipped Skills.
- Index still creates no entrypoint inside a Skill package, because the package references need none.

## Approach

1. Contracts first: the Skills payload maintenance contract, the Skills primitive document, the Index, Doctor and Repair contracts and the forgiving maintenance design.
2. Implement with failing tests first at Unit, Integration and EndToEnd level, including one journey that indexes a new Skill without frontmatter and reaches a clean Doctor.
3. Review, full Windows and WSL gates, site build, then release with the next beta.

## Progress, 2026-10-21

One shared protection rule excluded Skill packages, so removing it covers Index, Doctor and Repair together. `RoutedMetadataCompletionPolicy` no longer reports `NativePackage`. A `SKILL.md` goes to `RoutedSkillMetadataCompleter`, which reuses the workspace adoption planner that already completes native Skill metadata safely for Install, so `name` comes from the folder, `description` from the first heading, and values are double-quoted the way Install writes them. Other package Markdown takes the ordinary path with `[Skill]` tags. Rename evidence admits package files and `SKILL.md`, and Repair no longer reserves `SKILL.md` boundaries during revalidation. Managed Skills stay protected as managed content.

Doctor sends a `SKILL.md` missing native fields to Index with “`<path>` needs its Skill name or description. Index can fill them from the folder name and heading and keep your content.” The by-hand texts for Skill packages and the native-package Repair wording are gone, and the known Doctor issue about Skill resources is removed from `docs/cli.md`.

Tests that placed a `SKILL.md` under Guidance were removed at the maintainer's direction, and the Install and Update adoption tests now expect Index to complete the reference metadata afterwards. Unit 5,073, Integration 3,392 and EndToEnd 380 pass. A manual walk indexed a bare Skill and its reference, then repaired the `SKILL.md` link after a reference rename.

Moving or removing a whole Skill package through Route Move and Route Remove is not part of this Task.

Final gates on `6bcd3645d` passed: Windows `build:native` and all six `test:built` modes, WSL with its declared platform skips, `ci:checks` and the site build. The WSL gate first needed one online restore in its clone, because WSL's .NET SDK had moved to 10.0.113. Index on this repository then completed the two Worker Watch Skill files without frontmatter.
