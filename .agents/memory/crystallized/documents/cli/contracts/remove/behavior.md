---
open-forge:
  description: Apply one verified removal plan with persistent exclusions, exact recovery inputs and consistent ownership
  tags: [Memory, Document, CLI, Contract, Remove, Behavior, CurrentTruth]
---

# remove Behavior Contract

The [Interface Contract](interface.md) defines target selection, exclusions and
restoration. The [CLI Architecture](../../architecture.md) and shared
[mutation and recovery design](../../technical-designs/mutation-and-recovery.md)
define implementation boundaries. This contract extends the existing domain
removal contracts with persistent intent and explicit managed-path removal.

## Plan Before Effects

Resolve one target from explicit input. Inspect settings and ownership, the exact
physical target, its contained items where applicable, affected generated
navigation and any route-specific incoming references. Complete this observation
before confirmation or effects. Invalid or unavailable required settings or
ownership cannot be interpreted as an empty exclusion or ownership set.

A missing ownership file is known empty for an explicitly selected ordinary path;
it does not supply package or Library deletion authority. Package and Library
operations retain their source-independent ownership checks. Neither matching
payload bytes nor a familiar folder name establishes ownership.

Protect the workspace root, `.agents` itself, authored settings, the ownership
lock, Git metadata and recovery storage. A selected directory cannot contain a
registered Library source. Observe links without following them. A recorded
Library file link requires its exact expected identity and link-specific effects;
unknown, changed or unsupported links are blocked.

## One Complete Change

The plan contains all exclusions, content and generated-navigation changes,
directory effects and final ownership publication. Preserve exact old bytes and
link identities for recovery. Include prior settings and lock bytes, or their
proven absence, before the first persistent effect.

Acquire one workspace lease, rebuild or revalidate the complete observed plan,
and prepare verified recovery. Apply and verify exclusions before deleting
content. Apply guarded content/navigation effects, delete empty directories in
child-before-parent order, and publish changed ownership after content effects
have been verified. Recheck expected state at each effect boundary. Stop after
failure or interruption and report actual effects; no automatic compensation or
restoration is performed.

A settings-only exclusion still uses the mutation, expected-state and recovery
boundaries. A true no-op needs no lease, settings write or recovery preparation.
In a plain workspace, any directory needed for the settings file is an explicit
planned creation, not an unreported side effect of serialization.

## Record The Selected Meaning

- A leaf records each removed physical file, including its removed overwrite.
- A directory records its path in `removedDirectories`; a selected root category
  also records its category name. Future files remain excluded.
- Package removal records selected IDs in `removedExtensions`. It does not turn
  every package file into a global exclusion, since shared files can remain owned
  by another package. Retained dependents block before any exclusion is written.
- Library removal records the ID in `removedLibraries`, releases the registration
  and deletes only its verified owned links.
- Individual Library link removal records the mapped destination in `removedFiles`
  and releases only that source-relative mapping. The source and remaining
  registration are preserved.
- Explicit whole-file removal releases every matching Framework/Extension path
  and region claim. This is different from uninstalling one owner of a shared
  file: the explicit file selection authorizes removal of that file for all
  managers. A region claim by itself never authorizes deleting its host.

Preserve unrelated ownership and package registrations, including registrations
whose selected file set becomes empty. Do not reconstruct ownership from a package
manifest to restore a deliberately released claim.

## Subsequent Operations

Apply exclusion policy to concrete workspace destinations before content changes,
generated-host changes and new ownership are planned. Existing excluded bytes and
claims stay unchanged unless a removal operation explicitly releases them.
Check package ID exclusions against the complete dependency closure, including
dependencies already installed. Library path checks use mapped destinations, not
source-relative paths. Fresh plans under the lease include the observed settings
even when no permission grant is being written.

Do not silently restore an excluded route ancestor to make another selected file
reachable. Report that selection as blocked. Removing an exclusion only permits
a later operation to restore content; it does not restore bytes itself.

## Required Evidence

Cover exact files and companions, binary files, directories with future children,
managed and shared files, native Skills, package dependencies, whole Libraries,
individual owned links and explicit restoration. Verify dry-run parity, no-op
absence, settings preservation, malformed observations, changed observations
under the lease, source protection, no-follow behavior, recovery and partial
failure. Public journeys must remove content, run its relevant installer,
updater or synchronizer, and independently assert that it remains absent.
