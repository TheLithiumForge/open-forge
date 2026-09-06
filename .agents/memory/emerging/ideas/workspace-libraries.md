---
open-forge:
  description: Explore Workspace Libraries that project live shared files into ordinary workspace paths without adding Framework roots
  tags: [Memory, Idea, Contextual, Candidate, CLI, Workspace, Library, Sharing, Symlink, Submodule, Security]
---

# Workspace Libraries

## Status

This is a contextual maintainer-review draft. It is not accepted architecture,
an implementation contract, or a permanent task. The current [Extensions MVP
Architecture](../../crystallized/documents/extensions/architecture.md), CLI
contracts, and Framework routes remain authoritative. No ledger, plan, or
control record assigns this idea.

## Purpose And Boundary

Workspace Libraries are a candidate middle layer between project-owned files
and static Extension installation. A `library` would register one contained
source root with one consumer workspace and project its complete eligible
inventory into ordinary consumer paths. The source stays editable at its own
path. The consumer sees the projection through its normal Framework routes and
does not need an imported-content runtime concept.

The maintainer-selected product name is `Workspace Libraries`. The managed
entity and public group are `library` and `open-forge library`. The candidate
surface is:

- `open-forge library list [global flags]` reports consumer-local library records and
  projection health.
- `open-forge library inspect <library-id> [global flags]` explains one record
  and its exact mappings.
- `open-forge library attach <library-id> <source-root> [--dry-run] [global flags]`
  registers one contained source root under a new consumer-local ID after
  preflight and creates its projections.
- `open-forge library sync <library-id> [--dry-run] [global flags]` reconciles
  one complete source inventory with its registered projections.
- `open-forge library detach <library-id> [--dry-run] [global flags]` removes
  one complete consumer projection and its record without deleting source
  files.

A library ID is a caller-supplied unique identity in a separate namespace. It
uses the current portable stable-ID spelling grammar: 1–128 lowercase ASCII
alphanumeric characters in nonempty segments separated by single hyphens. It
is not derived from the source root. Duplicate IDs and IDs outside this grammar
are invalid. A library ID is in a separate namespace from automatic source
IDs. Under current source-reference authority, a projected explicit contained
link has no automatic ID. It is also separate from the Extension `--source`
operand, which remains an Extension-specific source selection and does not
select a library.

`source-root` is a normalized workspace-relative path to a real directory that
is strictly contained both lexically and physically by the selected workspace.
The command rejects absolute, empty, dot-traversal, backslash, escaping,
outside, and aliased values. It uses portable `/` path spelling and does not
normalize an invalid value into an accepted one.

The `attach`, `sync`, and `detach` dry-run forms build the same immutable plan
and perform no projection, record, generated-navigation, recovery, or Git
effect.

This candidate is filesystem composition, not Loader federation. Each consumer
keeps one Loader, one set of root routes, and its own local route structure. A
file projected by a library has the logical meaning of its consumer
destination. A file projected into `.agents/directives/` is a Directive because
of that destination route, not because source-controlled metadata assigns it
that meaning.

## First-Release Projection

The proposed first release has one deliberately narrow shape:

- The source root is one real directory that is lexically and physically
  contained by the selected workspace. A contained Git submodule may supply
  that directory, but the library feature does not operate Git.
- The source inventory includes eligible ordinary files below the source
  root's `.agents/` directory. Each source-relative `.agents/...` path maps to
  the same consumer-relative `.agents/...` path.
- Projections are relative file symlinks. Missing parent directories are real
  directories, and local files may remain beside projected files.
- Recognized entrypoints, adjacent overwrite companions, lifecycle and
  library records, Loader and other manager controls, symlinks, junctions, and
  special files are not eligible source files.
- The first release has no path remapping, globs, dependencies, selector-based
  partial Detach, entrypoint materialization, non-`.agents` destinations, Git
  operations or diagnostics, copy fallback, or write-through mutation.

Reserve `collection` as a possible future name for a named subset. The first
release has no collection selection or collection record field; it projects the
complete eligible inventory.

A portable example is:

```text
workspace/
├── shared/team-knowledge/.agents/
│   └── directives/review.md
└── .agents/
    ├── loader.md
    └── directives/
        ├── _directives.md
        └── review.md -> ../../shared/team-knowledge/.agents/directives/review.md
```

The consumer owns `loader.md`, route entrypoints such as
`.agents/directives/_directives.md`, overwrite companions, lifecycle and
library records, generated navigation, and other manager controls. A
projected leaf can participate in a route only when those consumer-owned route
files establish the required chain. Attaching a library does not invent a route
or rewrite a Loader.

## Consumer Record

The proposed separate management record is
`.agents/open-forge.libraries.json`. Under current authority it is ordinary
workspace content, not part of `.agents/open-forge.lifecycle.json`. Status and
Doctor may read it only after explicit accepted contract extensions define it
as a new typed input. Those extensions must not alter the lifecycle document.

The proposed record contains only a schema discriminator and library records.
Each record contains a library ID, a workspace-relative source root, and exact
source-to-destination mapping pairs with the expected relative-link
relationship:

```json
{
  "schema": 1,
  "libraries": [
    {
      "id": "team-knowledge",
      "sourceRoot": "shared/team-knowledge",
      "mappings": [
        {
          "sourcePath": ".agents/directives/review.md",
          "destinationPath": ".agents/directives/review.md",
          "expectedRelativeLink": "../../shared/team-knowledge/.agents/directives/review.md"
        }
      ]
    }
  ]
}
```

The example is a proposed shape, not a wire contract. The `sourceRoot` field
retains the source-origin meaning. The record contains no workstation-absolute
path, timestamp, Git revision, source-granted permission, glob, dependency,
remapping, exclusion, collection, or source metadata. The consumer record is
the only ownership and authorization evidence for a library. Source content
cannot grant itself a destination. Directory creation is mechanical support
and does not grant ownership of a broad directory tree.

## Projection Safety And Lifecycle

`attach` must validate the new library ID, establish the contained source root,
complete the eligible-file inventory, form exact mappings, and detect every
collision before applying any projection. It may create only declared relative
file symlinks and their real parent directories. It never adopts an existing
regular file, directory, or different link.

`sync` must enumerate the complete current eligible-file inventory before it
removes anything. It may remove an exact registered projection only when that
complete inventory proves the source mapping is gone and the destination still
is the expected relative symlink. If the source is unavailable or its
inventory is incomplete, Sync performs no deletion. A missing link is drift
that can be recreated after a complete preflight. A regular file, directory,
different link, or unsafe target remains in place and blocks that mapping.

`detach` is whole-library detachment in the first release. Its preflight is
all-or-nothing: any blocked mapping causes no projection or record effect. It
removes only exact registered symlinks whose expected relationship is still
proven, and publishes the updated record only after every planned link effect
verifies. It never removes source files. A changed occupant or unverifiable
link is preserved and blocks the complete request. Detach has no
selector-based partial form. If an unexpected application failure occurs,
retain the prior record and exact recovery evidence rather than claiming a
transactional rollback.

Sync may remove individual retired mappings, but only after a complete source
inventory and exact expected-link verification. A partial Detach is never
inferred from that Sync behavior.

Library mutations use the normal workspace lock, expected-state revalidation,
recovery preparation for existing-target effects, bounded local application,
verification, and final record publication order. Recovery state must retain
enough exact path and byte evidence to recover the consumer-side record and
projection effects. Status, Doctor, and Cleanup observe residuals; they do not
acquire mutation authority or remove them implicitly.

## Existing CLI Interaction

Read-only commands should consume a projected file at its logical destination
and retain library origin as a separate typed fact. Under the accepted
source-reference contract, an explicit contained local link keeps its
canonical `.agents/...` destination path but has no automatic ID: human output
uses `ID: none` and structured output uses `null`. A library ID remains a
separate management identity and is not accepted by source-reference operands.
Commands must not create a second source-ID grammar or treat a library ID as a
route identity.

The current code has a mandatory pre-dogfood gap. A contained single-file
symlink is currently followed, so Route Update and Index write through to its
physical target. Route Move and Route Remove delete the physical target and
then fail verification, leaving the projection link dangling. A link-aware
guard and a real-filesystem regression are therefore mandatory before
dogfooding this feature. Until an accepted mutation slice defines safe
write-through behavior, Route Update, Index, Route Move, and Route Remove must
recognize a library projection and block rather than follow or delete its
source target.

Consumer-owned entrypoints keep local and library-projected children
composable. The first release does not attach entrypoints, materialize them, or
rewrite their authored content or the Loader. Attach, Sync, and Detach may
update only an existing consumer-owned generated `Entries` region as part of
the same immutable plan, and only after the ordinary route chain already
exists. They must not create that chain or invent route semantics. Index may
update an existing consumer-owned generated region when its ordinary route
contract permits that effect; it must not write through a projected link.

Extension lifecycle operations must treat a library projection as a collision
occupant owned separately by the library. An Extension cannot overwrite, adopt,
update, or remove a library projection. Library attach and sync must likewise
block against Extension-owned paths, lifecycle records, and other manager
controls. Two libraries mapping to one destination are a collision with no
implicit winner or rename.

## Status, Doctor, And Recovery Facts

If this candidate is accepted, Status should expose typed library management
facts without making the record Framework runtime authority: record state, source
availability, exact mapping count, and per-mapping `current`, `missing`,
`changed`, `blocked`, or `unavailable` observations. It should not mutate,
repair, or turn a matching link into ownership without the consumer record.

Doctor should report typed findings for malformed or missing library state,
unavailable or incomplete source inventory, source containment or physical
alias failures, dangling or retargeted links, projection drift, unsupported
real-link capability, and collisions with local or Extension ownership. These
are candidate observations only. Doctor should not add a new root, adopt a
projection, invoke a library command, or delete recovery evidence.

Library attach, sync, and detach should use the existing recovery boundary for
consumer-side effects. Recovery must distinguish the library record and exact
link effects from the source files and must never delete or restore a source
file. An incomplete source inventory is not evidence that a source file was
deleted, and therefore cannot authorize Sync deletion.

## Platform And Git Boundary

The first release requires a capability-gated real file symlink. It must prove
that the created destination is a symlink with the expected relative target,
and it has no copy fallback. The managed APIs for creating and inspecting file
symlinks are [`.NET File.CreateSymbolicLink`](https://learn.microsoft.com/en-us/dotnet/api/system.io.file.createsymboliclink?view=net-10.0)
and [`.NET FileSystemInfo.LinkTarget`](https://learn.microsoft.com/en-us/dotnet/api/system.io.filesysteminfo.linktarget?view=net-10.0).
If the current platform cannot create and verify a real link, the operation
blocks and leaves the workspace unchanged.

A Git submodule is a possible contained source root, but first-release library
records do not duplicate Git revision authority. Attach, Sync, and Detach
perform no fetch, pull, checkout, switch, stage, commit, or Git diagnostic.
Git may materialize symlinks according to its `core.symlinks` configuration;
that behavior is outside the first release and a materialized file fails the
expected-link check. See the official [Git submodule
documentation](https://git-scm.com/book/en/v2/Git-Tools-Submodules) and
[`core.symlinks` configuration](https://git-scm.com/docs/git-config#Documentation/git-config.txt-coresymlinks)
for those independent Git behaviors.

## Maintainer Decisions Before Promotion

The maintainer has selected the product name `Workspace Libraries`, managed
entity `library`, and public group `open-forge library`. Three other decisions
remain for review:

1. **First-release boundary.** Confirm one contained source root, eligible
   ordinary files under its `.agents/**`, same consumer-relative destinations,
   relative file symlinks, real parent directories, consumer-owned controls,
   no first-release `collection` selection, and the listed exclusions.
2. **Record and complete-tree Sync deletion.** Confirm
   `.agents/open-forge.libraries.json`, its minimal exact mapping shape,
   whole-library detachment, and deletion during Sync only after a complete
   source inventory and an exact expected-link check.
3. **Link capability and Git scope.** Require real symlinks with no copy
   fallback, and defer all initial Git diagnostics and operations even when the
   source root is a contained submodule.

## Queue Proposal

If the three remaining decisions are accepted, create candidate Task 23, Workspace
Libraries, after Task 18 and before Tasks 19 and 20. Its first prerequisite
must be the link-aware guard for Route Update, Index, Route Move, and Route
Remove plus a real-filesystem regression. That prerequisite must pass before
any Attach dogfood. The task would then freeze the record and lifecycle
contracts, implement the smallest library list/inspect/attach/sync/detach
slices, and extend Status, Doctor, recovery, and Extension collision evidence.
This is a queue proposal only; it does not assign Task 23 or change the current
CLI ledger.

## Evidence Before Promotion

Promotion should wait for evidence that:

1. Real filesystem tests prove capability gating, relative-link identity,
   containment, aliases, dangling links, and no copy fallback on each supported
   platform.
2. Library Attach, complete-tree Sync, whole-library Detach, local sibling
   files, source additions and deletions, missing sources, incomplete
   inventories, occupied destinations, and two-library collisions preserve
   every occupant and record exact effects.
3. Route Update, Index, Route Move, and Route Remove regressions prove that no
   operation follows a library link or deletes its physical source target.
4. Status, Doctor, recovery, and Extension lifecycle evidence reports typed
   library facts without adopting, mutating, or confusing ownership.
5. Maintainer review accepts the three remaining decisions before any promotion to
   Architecture, contracts, planning, or a permanent task.
