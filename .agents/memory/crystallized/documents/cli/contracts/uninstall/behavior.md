---
open-forge:
  description: Plan uninstall from the current ownership and payload comparison, then apply it as one verified strip plan
  responsibility: Define how uninstall selects removals, combines them with strip and leaves the workspace without Open Forge state
  tags: [Memory, Crystallized, CLI, Command, Contract, Uninstall, Behavior, Lifecycle, CurrentTruth]
---

# uninstall Behavior Contract

The [Interface Contract](interface.md) defines what uninstall removes and keeps. The [strip Behavior](../strip/behavior.md) defines planning, determinism, revalidation, recovery and verification for the combined plan.

## Select Removals

Read the layout, settings and ownership once. Absent ownership means nothing is owned. Unreadable or unknown ownership stops the run as `uninstall.ownership-unavailable`. A recorded Library stops it as `uninstall.library-attached`.

Read every owned Framework and Extension target through the shared lifecycle target readers, rendered in the workspace's frontmatter form. A current file becomes a deletion. A current managed section becomes a rewrite of its host, or the host's deletion when nothing else remains. An edited target becomes a kept entry, unless `--remove-edited` names it. A missing target adds nothing. An unavailable or blocked target stops the run. Matching bytes never establish ownership for a file the lock does not name.

Then classify unowned entrypoints below the inventory roots against the route scaffold shape. Read their metadata and `Entries` through the shared parsers, never from the generated lines. Add the ownership and settings files when present.

## Combine With Strip

Remove planned deletions from strip's document set before strip plans its edits. Plan flatten moves from the documents that stay. A folder becomes an empty-folder removal only when every file below it is deleted or moved. The main root, outside routes and the flatten folder can go only when they empty.

## Apply

Apply the combined plan under one lease as strip's behavior defines. Delete content before the settings file, and the settings file before the ownership lock, so an interrupted run still records what was installed. After every effect verifies and the recovery bundle is deleted, release the lease. Then remove `.open-forge-state/` only when it holds nothing but the lock file, its ignore file and empty folders. Otherwise leave it and report `uninstall.runtime-retained`.

## Required Evidence

Cover recognized, edited, missing and shared targets, both root hosts with and without user text, legacy host markers, named edited files, route scaffolds with and without user text, absent and malformed ownership, attached Libraries, combined strip and flatten plans, the runtime folder with and without earlier bundles, and a second run that is a verified no-op. Compare one complete scoped workspace with the eval harness's plain setup byte for byte.
