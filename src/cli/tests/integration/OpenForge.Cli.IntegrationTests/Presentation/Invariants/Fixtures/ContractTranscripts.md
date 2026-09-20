# Preserved Interface Transcripts

These authored examples were relocated verbatim from the pre-extraction interface contracts. They remain independent of production factories. An example is not a claim of executable scenario coverage; exact matching reviewed captures are linked from the interface where available. No scenario expectations are newly approved here.

## cleanup-completed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/cleanup/interface.md`.

```text
No recovery data to remove.
```

## cleanup-completed-with-warnings

Original contract: `.agents/memory/crystallized/documents/cli/contracts/cleanup/interface.md`.

```text
Cleanup stopped after removing 1 of 3 items.
  <recovery-bundle-1>    removed
  <recovery-bundle-2>    could not be removed: The recovery candidate disposition is unknown after deletion failed: IOException (0x80070020): The filesystem operation failed
  <recovery-bundle-3>  not started
Next: open-forge cleanup
```

## cleanup-incomplete

Original contract: `.agents/memory/crystallized/documents/cli/contracts/cleanup/interface.md`.

```text
The recovery store could not be read completely. Nothing was removed.
Next: open-forge doctor
```

## cleanup-invalid-input

Original contract: `.agents/memory/crystallized/documents/cli/contracts/cleanup/interface.md`.

```text
Unrecognized command or argument 'unexpected-operand'.
```

## cleanup-blocked

Original contract: `.agents/memory/crystallized/documents/cli/contracts/cleanup/interface.md`.

```text
Cannot clean up: another Open Forge command holds the workspace lock. Nothing was removed.
  <recovery-bundle-1>    not started
  <recovery-bundle-2>    not started
  <recovery-bundle-3>  not started
Next: open-forge cleanup
```

## cleanup-failed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/cleanup/interface.md`.

```text
Cleanup stopped after removing 1 of 3 items.
  <recovery-bundle-1>    removed
  <recovery-bundle-2>    could not be removed: The recovery candidate disposition is unknown after deletion failed: IOException (0x80070020): The filesystem operation failed
  <recovery-bundle-3>  not started
Next: open-forge cleanup
```

## cleanup-cancelled

Original contract: `.agents/memory/crystallized/documents/cli/contracts/cleanup/interface.md`.

```text
Cleanup was cancelled after removing 1 of 3 items.
  <recovery-bundle-1>    removed
  <recovery-bundle-2>    not started
  <recovery-bundle-3>  not started
Next: open-forge cleanup
```

## context-completed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/context/interface.md`.

```text
AGENTS.md
.agents/loader.md
.agents/docs/_docs.md
.agents/docs/guide.md
```

## context-completed-with-warnings

Original contract: `.agents/memory/crystallized/documents/cli/contracts/context/interface.md`.

```text
  Warning  .agents/docs/_docs.md  Section is missing
         .agents/docs/_docs.md has no section named Absent.
  Warning  .agents/docs/guide.md  Section is missing
         .agents/docs/guide.md has no section named Absent.
  Warning  .agents/loader.md  Section is missing
         .agents/loader.md has no section named Absent.
  Warning  AGENTS.md  Section is missing
         AGENTS.md has no section named Absent.
=== AGENTS.md ===

=== .agents/loader.md (loader) ===

=== .agents/docs/_docs.md (docs) ===

=== .agents/docs/guide.md (docs/guide) ===
```

## context-incomplete

Original contract: `.agents/memory/crystallized/documents/cli/contracts/context/interface.md`.

```text
  Warning  .agents/docs/guide.md  Source encoding is invalid
         .agents/docs/guide.md is not valid UTF-8, so it was not included.
=== AGENTS.md ===
# Workspace

Read `.agents/loader.md`.

=== .agents/loader.md (loader) ===
# Open Forge Loader

## Entries

- [Documents](docs/_docs.md) - #LoadNow #Docs

=== .agents/docs/_docs.md (docs) ===
---
open-forge:
  description: Documents
  tags: [Docs]
---
# Documents

## Entries

- [Guide](guide.md) - #Docs #Guide
- [Reference](reference.md) - #Docs

=== .agents/docs/guide.md (docs/guide) ===
  sources: The selected Context source layer .agents/docs/guide.md could not be read completely.
  tokens: The selected Context source layer .agents/docs/guide.md could not be read completely.
  bytes: The selected Context source layer .agents/docs/guide.md could not be read completely.
Next: open-forge doctor
```

## context-invalid-input

Original contract: `.agents/memory/crystallized/documents/cli/contracts/context/interface.md`.

```text
Cannot read context: --content invalid is not a known part. Use metadata, paths, frontmatter, headings, body, or section:<name>.
  sources: Context counts were not established.
  tokens: Context counts were not established.
  bytes: Context counts were not established.
  links followed: Context counts were not established.
  links not followed: Context counts were not established.
```

## context-blocked

Original contract: `.agents/memory/crystallized/documents/cli/contracts/context/interface.md`.

```text
Cannot read context: docs/guide matches more than one source. Use the exact path.
Workspace: <workspace>
=== AGENTS.md ===
# Workspace

Read `.agents/loader.md`.

=== .agents/loader.md (loader) ===
# Open Forge Loader

## Entries

- [Documents](docs/_docs.md) - #LoadNow #Docs

=== .agents/docs/_docs.md (docs) ===
---
open-forge:
  description: Documents
  tags: [Docs]
---
# Documents

## Entries

- [Guide](guide.md) - #Docs #Guide
- [Reference](reference.md) - #Docs
  sources: The selected Context sources could not be resolved completely.
  tokens: The selected Context sources could not be resolved completely.
  bytes: The selected Context sources could not be resolved completely.
```

## context-failed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/context/interface.md`.

```text
Context stopped because of an unexpected error: <reason>.
```

## context-cancelled

Original contract: `.agents/memory/crystallized/documents/cli/contracts/context/interface.md`.

```text
Context was cancelled.
```

## doctor-completed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/doctor/interface.md`.

```text
No problems found.
  6 checks complete. 21 links and 20 routes checked.
```

## doctor-completed-with-warnings

Original contract: `.agents/memory/crystallized/documents/cli/contracts/doctor/interface.md`.

```text
No errors. 2 warnings and 3 info findings were recorded.
  To list the warnings: open-forge doctor --detail standard
```

## doctor-incomplete

Original contract: `.agents/memory/crystallized/documents/cli/contracts/doctor/interface.md`.

```text
No problems found. 1 info finding was recorded. 1 check could not finish.
  Extensions were not checked: the package source ./packages/toolkit cannot be read.
  To list the info findings: open-forge doctor --detail full
```

## doctor-invalid-input

Original contract: `.agents/memory/crystallized/documents/cli/contracts/doctor/interface.md`.

```text
Cannot run doctor: The selected workspace path is invalid.
  Workspace checks did not finish: The selected workspace path is invalid.
  Recovery data was not checked: The selected workspace path is invalid.
  Routes were not checked: The selected workspace path is invalid.
  Links were not checked: The selected workspace path is invalid.
  Framework files were not checked: The selected workspace path is invalid.
  Extensions were not checked: The selected workspace path is invalid.
```

## doctor-blocked

Original contract: `.agents/memory/crystallized/documents/cli/contracts/doctor/interface.md`.

```text
Cannot check this workspace: The selected workspace is unavailable.
  Workspace checks did not finish: the workspace does not exist or cannot be read.
  Recovery data was not checked: The selected workspace is unavailable.
  Routes were not checked: The selected workspace is unavailable.
  Links were not checked: The selected workspace is unavailable.
  Framework files were not checked: The selected workspace is unavailable.
  Extensions were not checked: The selected workspace is unavailable.
```

## doctor-failed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/doctor/interface.md`.

```text
Doctor stopped because of an unexpected error: <reason>.
```

## doctor-cancelled

Original contract: `.agents/memory/crystallized/documents/cli/contracts/doctor/interface.md`.

```text
Doctor was cancelled.
```

## extension-create-completed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/create/interface.md`.

```text
Created the toolkit Extension scaffold at <extension-source>/toolkit
  <extension-source>/toolkit/extension.json
  <extension-source>/toolkit/content/.agents/
  Edit extension.json, then add files under content/.agents/.
```

## extension-create-incomplete

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/create/interface.md`.

```text
The scaffold could not be created: <extension-source>/toolkit could not be read. Nothing was changed.
  <extension-source>/toolkit/extension.json    not started
  <extension-source>/toolkit/content/.agents/  not started
Next: open-forge extension create --detail debug
```

## extension-create-invalid-input

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/create/interface.md`.

```text
Cannot create the Extension: INVALID ID is not a valid Extension ID. Use lowercase letters, digits and hyphens.
Next: open-forge extension create --help
```

## extension-create-blocked

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/create/interface.md`.

```text
Cannot create toolkit at <extension-source>/toolkit: <extension-source>/toolkit already exists with different content.
  <extension-source>/toolkit/extension.json    not started
  <extension-source>/toolkit/content/.agents/  not started
Next: open-forge extension create --dry-run
```

## extension-create-failed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/create/interface.md`.

```text
Extension create stopped after 1 of 2 files.
  Error  <extension-source>/toolkit/content/.agents/  Writing failed
         Extension create stopped after 1 of 2 files. Created files were left in place.
  <extension-source>/toolkit/extension.json    created
  <extension-source>/toolkit/content/.agents/  not started
Next: open-forge extension create --detail debug
```

## extension-create-cancelled

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/create/interface.md`.

```text
Extension create was cancelled. Nothing was changed.
  Error  <extension-source>  Extension create was cancelled
         Extension create was cancelled. Nothing was changed.
Next: open-forge extension create
```

## extension-inspect-completed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/inspect/interface.md`.

```text
toolkit 1.0.0 is installed and matches the package.
1 file under .agents
1 file unchanged, 1 dependency.
```

## extension-inspect-completed-with-warnings

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/inspect/interface.md`.

```text
toolkit 1.0.0 is installed. 2 files need attention.
  Warning  .agents/retired.md  no longer part of the package
  Warning  .agents/toolkit.md  changed since it was installed
1 file changed, 1 file retired, 1 dependency, 2 warnings.
Next: open-forge extension update toolkit --dry-run
```

## extension-inspect-incomplete

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/inspect/interface.md`.

```text
toolkit is installed, but the comparison could not finish: The explicit Extension source does not exist.
  Warning  <extension-source>/missing-source  Extension source is unavailable
         The source <extension-source>/missing-source could not be read, so the comparison could not finish.
1 dependency, 1 warnings.
```

## extension-inspect-invalid-input

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/inspect/interface.md`.

```text
Unrecognized command or argument 'unexpected-operand'.
```

## extension-inspect-blocked

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/inspect/interface.md`.

```text
Cannot inspect toolkit: The ownership record names toolkit in a way that cannot be matched to one package.
Workspace: <workspace>
1 dependency, 1 errors.
Next: open-forge doctor
```

## extension-inspect-failed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/inspect/interface.md`.

```text
Extension inspect stopped because of an unexpected error: <reason>.
```

## extension-inspect-cancelled

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/inspect/interface.md`.

```text
Extension inspect was cancelled.
```

## extension-install-completed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/install/interface.md`.

```text
Installed the toolkit Extension.
  Created 1 file under .agents
```

## extension-install-completed-with-warnings

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/install/interface.md`.

```text
Nothing was installed from <extension-source>: the package has no content directory.
  Package files belong under <extension-source>/content/.agents/.
```

## extension-install-incomplete

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/install/interface.md`.

```text
The selected Extension could not be installed: The source <extension-source> could not be read. Nothing was changed.
```

## extension-install-invalid-input

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/install/interface.md`.

```text
Cannot install: Extension install needs to know which packages. Pass their IDs or --all. This session cannot ask.
Next: open-forge extension list
```

## extension-install-blocked

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/install/interface.md`.

```text
Cannot install toolkit: Another Open Forge command holds the workspace lock. Nothing was changed.
Workspace: <workspace>
```

## extension-install-failed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/install/interface.md`.

```text
Extension install stopped after 1 of 2 changes.
Workspace: <workspace>
  .agents/aaa.md      created (toolkit)
  .agents/toolkit.md  not started
  Created 1 file under .agents
```

## extension-install-cancelled

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/install/interface.md`.

```text
Extension install was cancelled. Nothing was changed.
Workspace: <workspace>
```

## extension-list-completed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/list/interface.md`.

```text
Installed
  development          0.1.0
  development-toolkit  0.1.0
  memory-starters      0.1.0
  planning             0.1.0
  project-documents    0.1.0

Available (bundled with this CLI)
  development          0.1.0  Optional Development, Debugging, and Review Workflows that use project context                                  installed
  development-toolkit  0.1.0  An optional bundle of project documents, Memory starters, planning, and development packages                    (5 packages)  installed
  memory-starters      0.1.0  Copy-ready Memory Templates for decisions, ideas, analyses, observations, and handoffs                          installed
  orchestration        0.1.0  Coordinate dependent tasks through one optional managed-delivery workflow                                       (3 packages)
  planning             0.1.0  An optional planning Workflow, Work Records Pattern, and Templates for tasks, plans, backlogs, and checkpoints  installed
  project-documents    0.1.0  Optional Vision and Architecture Workflows with document Templates for a project's direction and structure      installed
Next: open-forge extension install <id>
```

## extension-list-completed-with-warnings

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/list/interface.md`.

```text
Extension list completed with warnings.
  Warning  <extension-source>/missing-source  Installed Extension source is missing
         The missing-package Extension's source <extension-source>/missing-source cannot be read.
         open-forge extension inspect missing-package
Installed
  missing-package  1.0.0  source missing

Available (from <extension-source>)
  toolkit  1.0.0  Extension package toolkit.
Next: open-forge extension inspect missing-package
```

## extension-list-incomplete

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/list/interface.md`.

```text
Extension list could not be read completely.
  Warning  <extension-source>  Extension source is unavailable
         <extension-source> could not be read, so available packages are not listed.
Installed  (no ownership record, so installed packages cannot be listed)

Available (from <extension-source>)  unavailable
  available packages: The process cannot access the file '<extension-source>/toolkit/extension.json' because it is being used by another process.
```

## extension-list-invalid-input

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/list/interface.md`.

```text
Unrecognized command or argument 'unexpected-operand'.
```

## extension-list-blocked

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/list/interface.md`.

```text
Cannot list Extensions: The explicit Extension source overlaps the selected workspace.
Workspace: <workspace>
  Error  <workspace>  Extension source is blocked
         <workspace> cannot be used as a source: it is inside the workspace.
Installed  (no ownership record, so installed packages cannot be listed)

Available (from <workspace>)  unavailable
  available packages: The explicit Extension source overlaps the selected workspace.
```

## extension-list-failed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/list/interface.md`.

```text
Extension list stopped because of an unexpected error: <reason>.
```

## extension-list-cancelled

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/list/interface.md`.

```text
Extension list was cancelled.
```

## extension-remove-completed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/remove/interface.md`.

```text
Removed the toolkit Extension.
  .agents/toolkit.md  deleted
  The deleted files are kept in a recovery bundle at <recovery-bundle>.
Next: open-forge cleanup  (after reviewing the bundle)
```

## extension-remove-completed-with-warnings

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/remove/interface.md`.

```text
Removed the toolkit Extension. base remains installed and is no longer needed by it.
  Warning  base  Dependency remains installed
         base remains installed and is no longer needed by toolkit.
         open-forge extension remove base
  .agents/toolkit.md  deleted
  The deleted files are kept in a recovery bundle at <recovery-bundle>.
Next: open-forge extension remove base
```

## extension-remove-incomplete

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/remove/interface.md`.

```text
The <id> Extension could not be removed: <limitation>. Nothing was changed.
```

## extension-remove-invalid-input

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/remove/interface.md`.

```text
No files are recorded for toolkit, so there is nothing to remove.
```

## extension-remove-blocked

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/remove/interface.md`.

```text
Cannot remove toolkit: IOException (0x80070020): The filesystem operation failed.
```

## extension-remove-failed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/remove/interface.md`.

```text
Extension remove stopped after 1 of 2 changes.
  .agents/aaa.md      deleted
  .agents/toolkit.md  deleted
  The deleted files are kept in a recovery bundle at <recovery-bundle>.
Next: open-forge cleanup  (after reviewing the bundle)
```

## extension-remove-cancelled

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/remove/interface.md`.

```text
Extension remove was cancelled. Nothing was changed.
```

## extension-update-completed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/update/interface.md`.

```text
The toolkit Extension is up to date. Nothing to do.
```

## extension-update-completed-with-warnings

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/update/interface.md`.

```text
The toolkit Extension is up to date, but 1 file from an earlier version was kept.
  Warning  .agents/toolkit.md  Retired file was kept
         A retired managed Extension target was preserved without --prune.
  .agents/toolkit.md  kept; no longer part of the package
Next: open-forge extension update toolkit --prune --dry-run  (preview deleting the kept file)
```

## extension-update-incomplete

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/update/interface.md`.

```text
The toolkit Extension could not be updated: The source <extension-source> could not be read. Nothing was changed.
```

## extension-update-invalid-input

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/update/interface.md`.

```text
Cannot update: Extension update needs to know which packages. Pass their IDs or --all.
Next: open-forge extension list --installed
```

## extension-update-blocked

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/update/interface.md`.

```text
Cannot update toolkit: Another Open Forge command holds the workspace lock. Nothing was changed.
```

## extension-update-failed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/update/interface.md`.

```text
Extension update stopped after 1 of 2 changes.
  .agents/aaa.md      replaced (you had changed it and this version changes it)
  .agents/toolkit.md  replaced (you had changed it and this version changes it)
  Previous content: recovery bundle at <recovery-bundle>
Next: open-forge doctor
```

## extension-update-cancelled

Original contract: `.agents/memory/crystallized/documents/cli/contracts/extension/update/interface.md`.

```text
Extension update was cancelled. Nothing was changed.
```

## find-completed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/find/interface.md`.

```text
No sources match <query>.
```

## find-completed-with-warnings

Original contract: `.agents/memory/crystallized/documents/cli/contracts/find/interface.md`.

```text
Find completed with warnings.
```

## find-incomplete

Original contract: `.agents/memory/crystallized/documents/cli/contracts/find/interface.md`.

```text
The search is incomplete.
```

## find-invalid-input

Original contract: `.agents/memory/crystallized/documents/cli/contracts/find/interface.md`.

```text
Cannot search: <problem>.
```

## find-blocked

Original contract: `.agents/memory/crystallized/documents/cli/contracts/find/interface.md`.

```text
Cannot search: <reason>.
```

## find-failed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/find/interface.md`.

```text
Find stopped because of an unexpected error: <reason>.
```

## find-cancelled

Original contract: `.agents/memory/crystallized/documents/cli/contracts/find/interface.md`.

```text
Find was cancelled.
```

## index-candidate-completed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/index-candidate/interface.md`.

```text
Entries sections are current in all 2 files. Nothing to do.
Workspace: <workspace>
```

## index-candidate-completed-with-warnings

Original contract: `.agents/memory/crystallized/documents/cli/contracts/index-candidate/interface.md`.

```text
Updated the Entries section in 2 of 2 files.
Workspace: <workspace>
  .agents/loader.md      1 -> 1 entries
  .agents/root/_root.md  unknown -> 1 entries
```

## index-candidate-incomplete

Original contract: `.agents/memory/crystallized/documents/cli/contracts/index-candidate/interface.md`.

```text
The Entries sections could not be rebuilt completely. Nothing was changed.
Workspace: <workspace>
  Warning  .agents/root/child.md  Frontmatter could not be read
         The frontmatter of .agents/root/child.md could not be read completely.
Next: open-forge doctor
```

## index-candidate-invalid-input

Original contract: `.agents/memory/crystallized/documents/cli/contracts/index-candidate/interface.md`.

```text
Cannot index .agents/root: it is a folder, not a source.
Workspace: <workspace>
Next: open-forge index root
```

## index-candidate-blocked

Original contract: `.agents/memory/crystallized/documents/cli/contracts/index-candidate/interface.md`.

```text
Cannot rebuild the Entries section of .agents/root/_root.md.
Workspace: <workspace>
  Error  .agents/root/child.md  Frontmatter is invalid
         The frontmatter of .agents/root/child.md cannot be used: A direct routed child document cannot be safely decoded for authored metadata.
Next: open-forge doctor
```

## index-candidate-failed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/index-candidate/interface.md`.

```text
Index stopped after 1 of 2 files.
Workspace: <workspace>
  Error  .agents/beta/_beta.md  Write failed
         Writing .agents/beta/_beta.md failed. Stopped after 1 of 2 changes. Recovery data: <recovery-bundle>.
  .agents/alpha/_alpha.md  unknown -> 1 entries
  .agents/beta/_beta.md    not started
Next: open-forge doctor
```

## index-candidate-cancelled

Original contract: `.agents/memory/crystallized/documents/cli/contracts/index-candidate/interface.md`.

```text
Index was cancelled. Nothing was changed.
Workspace: <workspace>
  Error  <workspace>  Index was cancelled
         Index was cancelled. Nothing was changed.
Next: open-forge index
```

## install-completed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/install/interface.md`.

```text
Installed the Open Forge Framework into <workspace>.
Workspace: <workspace>
  Created 21 files and 20 directories under .agents (listed in .agents/open-forge.lock.json).
  Created AGENTS.md and CLAUDE.md with an Open Forge section.
```

## install-completed-with-warnings

Original contract: `.agents/memory/crystallized/documents/cli/contracts/install/interface.md`.

```text
Cannot install: 1 Framework file has changed since they were installed.
Workspace: <workspace>
  Error  <workspace>  Framework files have changed
         <workspace> has changed since it was installed. Install does not replace changed files.
         open-forge update
Next: open-forge update
```

## install-incomplete

Original contract: `.agents/memory/crystallized/documents/cli/contracts/install/interface.md`.

```text
Install could not start: The recovery workspace storage path is not a directory. Nothing was changed.
Workspace: <workspace>
  Warning  <workspace>  Recovery data is unavailable
         Recovery data could not be prepared at <workspace>. Nothing was changed.
         open-forge doctor
Next: open-forge doctor
```

## install-invalid-input

Original contract: `.agents/memory/crystallized/documents/cli/contracts/install/interface.md`.

```text
Unrecognized command or argument 'unexpected-operand'.
```

## install-blocked

Original contract: `.agents/memory/crystallized/documents/cli/contracts/install/interface.md`.

```text
Cannot install: 1 file already exists where the Framework would write.
Workspace: <workspace>
  .agents/memory/_memory.md
Next: open-forge install --force --dry-run
```

## install-failed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/install/interface.md`.

```text
Extension install stopped after 1 of 2 changes.
Workspace: <workspace>
  .agents/aaa.md      created (toolkit)
  .agents/toolkit.md  not started
  Created 1 file under .agents
```

## install-cancelled

Original contract: `.agents/memory/crystallized/documents/cli/contracts/install/interface.md`.

```text
Extension install was cancelled. Nothing was changed.
Workspace: <workspace>
```

## references-candidate-completed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/references-candidate/interface.md`.

```text
.agents/docs/guide.md
  out  :10:10   reference.md#details
```

## references-candidate-completed-with-warnings

Original contract: `.agents/memory/crystallized/documents/cli/contracts/references-candidate/interface.md`.

```text
.agents/docs/guide.md
  out  :8:1   missing.md   missing
Next: open-forge doctor
```

## references-candidate-incomplete

Original contract: `.agents/memory/crystallized/documents/cli/contracts/references-candidate/interface.md`.

```text
.agents/docs/guide.md
  Warning  .agents/docs/guide.md  Link encoding cannot be resolved
         The link at .agents/docs/guide.md has an encoding that cannot be resolved.
Next: open-forge doctor
```

## references-candidate-invalid-input

Original contract: `.agents/memory/crystallized/documents/cli/contracts/references-candidate/interface.md`.

```text
Cannot list references: Direction 'invalid' is not one of in, out, or both.
Next: open-forge references --help
```

## references-candidate-blocked

Original contract: `.agents/memory/crystallized/documents/cli/contracts/references-candidate/interface.md`.

```text
Cannot list references: The source ID resolves to more than one logical source.
Workspace: <workspace>
  Warning  .agents/docs/guide.md  Two sources share an identity
         Cannot list references: The source catalogue retained an unresolved boundary.
Next: open-forge doctor
```

## references-candidate-failed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/references-candidate/interface.md`.

```text
References stopped because of an unexpected error: <reason>.
```

## references-candidate-cancelled

Original contract: `.agents/memory/crystallized/documents/cli/contracts/references-candidate/interface.md`.

```text
References was cancelled.
```

## repair-completed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/repair/interface.md`.

```text
Repaired 2 links.
  .agents/docs/source.md:9:8    ./guide.md -> guide.md
  .agents/docs/source.md:10:16  ./guide.md -> guide.md
```

## repair-completed-with-warnings

Original contract: `.agents/memory/crystallized/documents/cli/contracts/repair/interface.md`.

```text
Nothing could be repaired automatically. 2 problems need a choice.
  Warning  .agents/docs/source.md:9:10  Broken link
         Broken link; 4 possible targets
  Warning  .agents/docs/source.md:10:18  Broken link
         Broken link; 4 possible targets
Next: open-forge repair
```

## repair-incomplete

Original contract: `.agents/memory/crystallized/documents/cli/contracts/repair/interface.md`.

```text
Repaired 1 link.
  .agents/directives/review.md  restored from recovery
```

## repair-invalid-input

Original contract: `.agents/memory/crystallized/documents/cli/contracts/repair/interface.md`.

```text
Unrecognized command or argument 'unexpected-operand'.
```

## repair-blocked

Original contract: `.agents/memory/crystallized/documents/cli/contracts/repair/interface.md`.

```text
Cannot repair: another Open Forge command holds the workspace lock. Nothing was changed.
  Warning  .agents/docs/source.md:10:10  Broken link
         Broken link; 4 possible targets
  .agents/docs/source.md:9:8  ./guide.md -> guide.md
Next: open-forge doctor
```

## repair-failed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/repair/interface.md`.

```text
Repair stopped after 1 of 2 links were rewritten.
  .agents/docs/aaa.md:8:8     rewritten
  .agents/docs/source.md:9:8  not started
  Recovery data: <recovery-bundle>
Next: open-forge repair --detail debug
```

## repair-cancelled

Original contract: `.agents/memory/crystallized/documents/cli/contracts/repair/interface.md`.

```text
Repair was cancelled. Nothing was changed.
Next: open-forge repair
```

## status-completed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/status/interface.md`.

```text
Open Forge is installed and current.
  Startup reads 19 of 22 routed files, about 8.0k tokens.
```

## status-completed-with-warnings

Original contract: `.agents/memory/crystallized/documents/cli/contracts/status/interface.md`.

```text
Open Forge is installed, but 1 file needs attention.
  Warning  .agents/guidance/adaptive-collaboration.md  Changed since it was installed
         changed since it was installed
         open-forge update
  Startup reads 19 of 22 routed files, about 8.0k tokens.
Next: open-forge update
```

## status-incomplete

Original contract: `.agents/memory/crystallized/documents/cli/contracts/status/interface.md`.

```text
Open Forge is installed, but 1 file needs attention and some checks could not finish.
  Warning  .agents/loader.md  Changed since it was installed
         changed since it was installed
         open-forge update
  Warning  .agents/loader.md  Entries section is unavailable
         The Entries section of .agents/loader.md could not be read.
  Warning  .agents/memory/_memory.md  Startup context is incomplete
         Startup context could not be measured completely: .agents/memory/_memory.md could not be read completely.
         open-forge doctor
  Warning  .agents/memory/_memory.md  Managed file is unavailable
         could not be read
         open-forge doctor
  Warning  .agents/memory/_memory.md  Entries section is unavailable
         The Entries section of .agents/memory/_memory.md could not be read.
  Warning  <workspace>  May-load-again context is unavailable
         The may-load-again context could not be measured.
         open-forge doctor
  Warning  <workspace>  Framework files could not be checked completely
         .agents/open-forge.lock.json cannot be used for Framework files: The Framework lifecycle observation is incomplete.
         open-forge doctor
  Warning  <workspace>  Root categories are unavailable
         The root categories could not be read from .agents/loader.md.
         open-forge doctor
  Warning  <workspace>  Startup context is unavailable
         The current startup context could not be measured.
         open-forge doctor
  routed files: Filesystem access was denied.
  startup files: Filesystem access was denied.
  startup tokens: Filesystem access was denied.
  may-load-again files: The continuity context measurement is unavailable.
  may-load-again tokens: The continuity context measurement is unavailable.
  all routed tokens: Filesystem access was denied.
  startup share: Filesystem access was denied.
  root categories: The current root-category observation is unavailable.
  current Entries sections: Filesystem access was denied.
  stale Entries sections: Filesystem access was denied.
  missing Entries sections: Filesystem access was denied.
Next: open-forge update
```

## status-invalid-input

Original contract: `.agents/memory/crystallized/documents/cli/contracts/status/interface.md`.

```text
Unrecognized command or argument 'unexpected-operand'.
```

## status-blocked

Original contract: `.agents/memory/crystallized/documents/cli/contracts/status/interface.md`.

```text
Cannot check this workspace: The selected workspace is missing.
Next: open-forge doctor
```

## status-failed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/status/interface.md`.

```text
Status stopped because of an unexpected error: <reason>.
```

## status-cancelled

Original contract: `.agents/memory/crystallized/documents/cli/contracts/status/interface.md`.

```text
Status was cancelled.
```

## update-completed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/update/interface.md`.

```text
The Framework is up to date. Nothing to do.
Workspace: <workspace>
```

## update-completed-with-warnings

Original contract: `.agents/memory/crystallized/documents/cli/contracts/update/interface.md`.

```text
The Framework is up to date, but 1 retired file was kept.
  Warning  <path>  Retired content kept
Next: open-forge cleanup
```

## update-incomplete

Original contract: `.agents/memory/crystallized/documents/cli/contracts/update/interface.md`.

```text
The Framework could not be updated completely. Nothing was changed.
```

## update-invalid-input

Original contract: `.agents/memory/crystallized/documents/cli/contracts/update/interface.md`.

```text
Cannot update Open Forge: <problem>.
```

## update-blocked

Original contract: `.agents/memory/crystallized/documents/cli/contracts/update/interface.md`.

```text
Cannot update Open Forge: <reason>.
```

## update-failed

Original contract: `.agents/memory/crystallized/documents/cli/contracts/update/interface.md`.

```text
Update stopped after <n> of <m> files.
```

## update-cancelled

Original contract: `.agents/memory/crystallized/documents/cli/contracts/update/interface.md`.

```text
Update was cancelled. Nothing was changed.
```
