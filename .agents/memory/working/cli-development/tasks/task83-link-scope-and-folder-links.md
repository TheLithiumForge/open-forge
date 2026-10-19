---
open-forge:
  description: Open Task 83 to let a workspace ignore links from and to chosen paths, starting with the Memory archive, and to accept links to existing folders
  tags: [Memory, Working, Task, CLI, Doctor, Repair, Links, Settings, Contextual, Active]
---

# Task 83: Link scope and folder links

## Outcome

Doctor and Repair stop reporting links a workspace has chosen not to care about, and accept links to existing folders.

Two authored settings in `.agents/open-forge.json` name workspace-relative paths:

- `ignoreLinksFrom`: links written inside a listed file or folder are not checked or repaired.
- `ignoreLinksTo`: links whose target lies inside a listed file or folder are not checked or repaired.

Install adds `.agents/memory/archived` to `ignoreLinksFrom` in new workspaces that receive the Memory archive, so links into the archive from elsewhere stay checked. An existing folder inside the workspace is a valid link target, and a missing folder is a broken link like a missing file.

The maintainer on 2026-10-16, about the archive: “Maybe we should not care about it's outward links. But if something from outside is referencing something in the arxhive we can care about that.” About folder links: “Those should obviously be valid links, like, they go somewhere to a usable path.” On the proposal: “Let's add boths an ignorelinksfrom and an ingnorelinksto. The rest is xorrect. Proceed.”

Status: Task 83 “Link scope and folder links” (phase 3/3): implemented and reviewed three times with every finding fixed. Final gates and the beta15 release remain.

## Accepted behavior

- Entries are workspace-relative paths, not patterns, read the same way as `allowInstallPaths`: a file entry covers that file and a folder entry covers everything beneath it.
- Doctor and Repair do not check, report or repair an ignored link.
- Route Move updates an entry that names the moved path or a path beneath it. Doctor notes an entry whose path no longer exists.
- Install seeds `ignoreLinksFrom` with `.agents/memory/archived` for new workspaces that receive it.
- A link to an existing folder inside the workspace is valid. Links to other existing local files already pass on existence alone, and web URLs stay unchecked.

## Working assumptions

These are reversible choices made to proceed. The maintainer may correct any of them.

- Route Move and Route Remove do not rewrite or unlink ignored links either, so an ignored file stays exactly as written.
- Route Remove drops an entry that names the removed path or a path beneath it.
- Update leaves the authored settings of existing workspaces unchanged, and the documentation shows how to add the archive entry. This repository adds it by hand.
- References keeps listing every link, including ignored ones, because it answers an explicit question.
- A folder link with a heading fragment is invalid, because a folder has no headings.
- Repair proposes no replacement for a missing folder, so it asks for a manual edit.
- Two spellings of an encoded destination are accepted without a finding: the percent-encoded form Index writes and the readable literal form, with angle brackets around spaces. Only other spellings, such as `%68` for `h`, get an encoding notice, so an upgrade adds no notices for readable hand-written links.

## Approach

1. Two workers implement in parallel, each with its contract, technical design, tests and documentation changes: folder links in the shared link resolution, and the two settings across settings, Doctor, Repair, Route Move, Route Remove and Install.
2. Integrate both, then an independent review, the full Unit, Integration and EndToEnd projects, the Windows and WSL gates, the site build and a manual check with the built CLI.
3. Add `.agents/memory/archived` to this repository's `ignoreLinksFrom`, re-measure Repair and Doctor for [Task 82](task82-cli-in-seconds.md), and release in beta15.

## Progress, 2026-10-16

Folder links and link scope were built in parallel and merged with only Status token snapshots in conflict. The settings are schema version 2 under the [versioned configuration Directive](../../../../directives/software/versioned-configuration-schema.md), because published schemas never change. Version 1 stays readable, and an older CLI ignores keys it does not know, so beta14 ignores both lists.

An independent review found that moving a listed file left its entry behind, that renaming a category wrote the old entrypoint name into the lists, and that an unrecognised settings version was never reported. All three are fixed. A manual walk shortened the folder findings to one instruction each.

The walk also exposed a defect already in beta14: Index writes `old%20guide.md` for a spaced file name, Doctor calls that spelling non-canonical, and Repair then finds nothing to fix. In a managed entrypoint Doctor even calls the valid link broken and advises Update. Task 83 makes the three agree, because a forgiving workspace must reach a clean Doctor after Index.

After the fixes, Unit 5,021, Integration 3,380 and EndToEnd 378 pass. One terminal-timing flake in the F14 consent journey passed on three reruns.

A verification review then found that names containing `#` could not be listed or repaired and that image links lost heading findings. Both are fixed. The same review showed that percent encoding as the only canonical spelling would give readable hand-written links new notices after an upgrade, so both the form Index writes and the readable literal form are now accepted. A final review found that an entity-like name such as `entity&amp;.md` made the literal form name a different file. The literal form now escapes such ampersands, and parsing tests over every ASCII punctuation character found no other case.

This repository now has `.agents/open-forge.json` with the archive in `ignoreLinksFrom`.
