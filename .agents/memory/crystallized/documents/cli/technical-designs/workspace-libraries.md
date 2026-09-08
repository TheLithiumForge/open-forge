---
open-forge:
  description: Exact local-only Workspace Libraries record, inventory, projection, and recovery design
  responsibility: Define the strict Library record, complete source inventory, relative-link effects, capability gate, and explicit composition boundary
  tags: [Memory, Crystallized, Document, CurrentTruth, Evergreen, CLI, TechnicalDesign, Framework, Library, Workspace, Filesystem, Mutation, Recovery]
---

# Workspace Libraries Technical Design

## Boundary And Ownership

This Technical Design realizes the accepted narrow local-only Workspace
Libraries shape. The [CLI Architecture](../architecture.md) remains
authoritative for project boundaries, dependency direction, composition,
filesystem safety, Native AOT, and evidence. The [Shared CLI Operation
Contract](../shared-operation-contract.md) remains authoritative for common
operation stages, statuses, streams, and no-automatic-recovery behavior. This
design adds no Framework root, Loader federation, external destination, Git
operation, copy fallback, or write-through mutation.

The implementation uses the following focused ownership:

- `Framework/Filesystem/` owns the neutral typed no-follow logical-leaf
  observation. It inspects the final component without resolving it, preserves
  exact relative-file-link and generic-link identities, and can be consumed by
  any mutation that addresses a logical file leaf.
- `Framework/Mutation/` owns the typed relative-file-link effect and its
  ordinary parent-directory support. It exposes facts and mechanical effects,
  not Library selection or result policy.
- `Framework/Recovery/` owns relative-file-link recovery identities,
  no-follow comparison, and guarded recovery application.
- `Framework/Libraries/` owns the strict schema-v1 record models, source-root
  facts, complete eligible inventory, and mapping observations.
- `Commands/Library/` owns attach, sync, detach, list, and inspect request
  policy, plans, findings, results, and rendering.

`CliCompositionRoot` constructs these capabilities and registers every Library
leaf explicitly. No dependency injection, service locator, runtime registry,
reflection, assembly scan, or untyped operation catalogue is introduced. The
Library ID is a separate management identity and is never a source-reference
operand or a recovery subject identity.

## Physical Source And Consumer Boundary

Each Library uses one selected workspace and one `sourceRoot`. The normalized
`sourceRoot` is a workspace-relative path using `/`; it is non-empty, has no
`.` or `..` segment, and is lexically and physically contained by the selected
workspace. For Attach, Sync, and Inspect, the source root is a real ordinary
directory. Every directory from the workspace root through the source root has
ordinary directory identity with no symlink, junction, reparse-point, or other
linked ancestry. The source root must contain a real ordinary `.agents/`
directory with the same no-linked or reparse ancestry. For Attach, an absent
source root or mandatory `.agents/` child is invalid input; an inaccessible
source boundary or incomplete inventory is `incomplete`. For an existing
trusted record, later source-root or `.agents/` unavailability or
inaccessibility is unavailable/incomplete read coverage and blocks Sync
mutation because its safe precondition is unsatisfied. Linked, reparse,
aliased, special, or otherwise unsafe boundaries remain `blocked`. List may
report a missing or unavailable boundary without inventing an empty inventory.
Detach is source-independent and does not resolve or require the source root or
its `.agents/` child.

When a source boundary is required, the source root and the consumer destination
namespace are physically disjoint, and the source root is never a destination. A
destination maps into the selected
consumer workspace's `.agents/` namespace. For a mutating projection operation,
the consumer `.agents/` root and every existing destination parent must be real
ordinary directories with no linked or reparse ancestry; the operation does not
create or replace that root. Missing destination parents below that root may be
created as real ordinary directories only for declared mappings. Those parents
do not grant ownership of other files in the directory. A destination final
leaf is never adopted when it already exists as a regular file, directory,
link, reparse point, or special object.

The source root is read-only input. Source file bytes are not copied, rewritten,
deleted, or used as effects. A projection is a relative file symlink whose raw
target names the source file. The consumer sees the projection at its ordinary
destination path and supplies its route meaning through the existing Loader and
route chain. Attaching a Library does not create an entrypoint, rewrite a
Loader, or establish generated navigation that the consumer does not already
own.

## Strict Schema-v1 Record

The consumer record is the ordinary workspace file
`.agents/open-forge.libraries.json`. It is separate from
`.agents/open-forge.lifecycle.json` and has no lifecycle authority. Its strict
schema-v1 shape is:

```json
{
  "schemaVersion": 1,
  "libraries": [
    {
      "id": "team-knowledge",
      "sourceRoot": "shared/team-knowledge",
      "paths": [
        ".agents/directives/review.md"
      ]
    }
  ]
}
```

The root object has exactly `schemaVersion` and `libraries`.
`schemaVersion` is the integer `1`. Each Library object has exactly `id`,
`sourceRoot`, and `paths`. Each `paths` item is a string. Unknown, missing,
null, duplicate, or differently typed properties are malformed. No v2 schema,
compatibility reader, dual reader, migration, or alternate field spelling
exists.

`id` is one to 128 lowercase ASCII alphanumeric characters in non-empty
segments separated by single hyphens. It is unique within the selected
consumer record and is not derived from `sourceRoot`. `sourceRoot` uses the
physical and lexical boundary above. Each `paths` item is relative to
`sourceRoot`, names an eligible `.agents/...` file, and is also the identical
canonical `/` destination path relative to the consumer workspace.

The expected link is derived as the exact raw `/`-separated relative target
from the destination's parent directory to `sourceRoot/path`. It is not stored
in the record and is not resolved to form link identity. When the source is
available, it must resolve to the same contained source file named by the path.
It is never an absolute path, contains no backslash, and cannot escape the
selected workspace. A raw target with `..` segments is valid when its
normalized destination remains the exact contained source path.

The writer emits Libraries in ordinal `id` order and each `paths` array in
ordinal path order. The reader requires unique IDs, unique paths across
Libraries, canonical source/destination interpretation, and those same orders.
The record contains no expected link target, source bytes, byte hashes,
timestamps, Git revisions, dependencies, globs, remapping, collection,
exclusion, source permission, workstation-absolute path, or destination
ownership inferred from content. A Library record is the consumer's management
evidence; source content cannot grant itself a projection.

## Typed Library Facts

`Framework/Libraries/` retains immutable typed facts with these shapes. The
names describe the ownership boundary and are not a public C# API:

```text
LibrariesRecord
  SchemaVersion: 1
  Libraries: ordered LibraryRecord[]

LibraryRecord
  Id: LibraryId
  SourceRoot: WorkspaceRelativeDirectory
  Paths: ordered SourceRelativeEligiblePath[]

LibraryMapping
  SourcePath: SourceRelativeEligiblePath
  DestinationPath: WorkspaceRelativeEligiblePath
  ExpectedRelativeLink: RawRelativeLinkTarget

LibraryInventory
  SourceRoot: contained real ordinary directory
  SourceAgentsDirectory: contained real ordinary directory
  Entries: complete ordered EligibleSourceFile[]
  Completeness: complete | incomplete | blocked

LibraryMappingObservation
  LogicalDestinationPath: canonical workspace-relative path
  Leaf: NoFollowLeafObservation
  ExpectedLink: RelativeFileLinkIdentity
  State: current | missing | changed | blocked | unavailable
```

`LibraryMapping` and `LibraryMappingObservation` are derived immutable facts for
planning and inspection; neither widens the strict record beyond its `paths`
array.

`EligibleSourceFile` records the source-relative path, ordinary-file
classification, and contained physical identity needed to prove the inventory.
It does not carry source bytes. `RelativeFileLinkIdentity` contains the exact
raw relative target and its link kind. A safely observed symbolic link that is
not that exact relative file-link identity is a generic `Link`; its neutral
identity retains the raw target and target form when safely observable. An
absolute, unsupported, or otherwise non-relative link never satisfies a
`RelativeFileLinkIdentity`. Reparse points and special entries remain distinct
no-follow states. Link identity never substitutes target bytes or a resolved
target path.

The complete eligible inventory recursively inspects the source `.agents/`
directory. It admits only ordinary regular files whose complete path and
physical identity remain contained and safe. Recognized entrypoints, adjacent
overwrite companions, the Loader, lifecycle and Library records, other
recognized manager controls, all symlinks, junctions, reparse points, and
special files are excluded from projection. An excluded control is still
inspected sufficiently to establish its no-follow classification and safe
logical containment; its target is never traversed. An inaccessible, unreadable,
ambiguous, externally resolving, aliased, or otherwise unsafe required boundary
or eligible ordinary item prevents a complete inventory; a safe prefix is never
reported as complete. Directory ancestry in this Library boundary is always real
and ordinary even though the ordinary shared path contract permits stable
contained directory-link ancestry for other operations.

List reads the strict consumer record and performs lightweight checks of the
recorded source root and destination leaves. It reports record identity, source
availability, path count, and bounded mapping states; it does not enumerate the
source tree or read source bytes. Inspect reads the full
recorded path set and forms a complete `LibraryInventory` plus every exact
mapping observation. Attach and Sync require that complete inventory before
planning; Detach needs the strict record and complete exact mapping observations
but does not need source bytes or a source inventory because it never removes a
source file. None of these read-only fact formations acquires a lease or creates
state.

## Relative-File-Link Effects

`Framework/Mutation/` owns a typed `RelativeFileLinkEffect` with this exact
shape:

```text
RelativeFileLinkEffect
  Kind: create | delete
  LogicalPath: canonical workspace-relative destination path
  LinkKind: relative-file-symbolic-link
  RawRelativeTarget: exact `/`-separated target text
  Expected:
    missing                         when Kind is create
    relative-file-link(identity)    when Kind is delete
  Intended:
    relative-file-link(identity)    when Kind is create
    missing                         when Kind is delete
```

Create accepts only an exact missing final leaf and creates the link object with
the declared raw target. Delete accepts only the exact expected relative file
link and deletes that link object. Both inspect the final leaf without following
it immediately before their effect and verify the exact intended leaf state
afterward. They never resolve into, write, or delete the source target. A
changed occupant, generic `Link`, different raw target, different link kind,
reparse point, special object, unsafe parent, or unavailable observation blocks
the effect.

Only this typed Library relative-file-link effect may create or delete a link
object. Ordinary file effects reject a link final leaf and cannot become a
projection operation by consulting the Library record.

Real parent directories are separate ordinary Create-only directory effects.
They are planned only for missing parents below the consumer workspace and are
never used to adopt an existing directory tree. Source files and source
directories are never effect targets. A mapping whose projection is already the
exact expected link is an observation and receives no effect.

## Library Operation Lifecycle

All Library mutators form one complete source or mapping fact set, one ordered
plan, and one preflight before any effect. The sequence is:

1. Validate the strict record shape, Library ID or source root, capability, and
   physical boundaries.
2. For Attach and Sync, complete the source inventory. For Detach, complete the
   exact recorded mapping observations.
3. Form every relative-file-link and real-parent-directory effect, every
   permitted bounded consumer-generated-region effect, every collision result,
   and the consumer-record effect. Do not include source bytes as an effect.
4. Run initial no-follow final-leaf observations and all expected-state and
   containment checks for the complete plan.
5. Acquire the same-workspace lock and repeat every volatile observation under
   the lease. A stale or unsafe fact blocks the whole request.
6. Prepare and verify one schema-v1 recovery bundle for every non-no-op effect
   that needs reversal, including link creates or deletes and the prior-missing
   ordinary Create of a new Library record.
7. Apply declared link, parent-directory, and permitted generated-region effects
   in deterministic ordinal order. Before each effect, repeat its no-follow
   final-leaf and expected-state checks. Application is monotonic: a failure or
   interruption stops new effects and preserves verified effects and residual
   recovery evidence.
8. Verify every link and the complete projection. Write or delete the consumer
   Library record last, through its ordinary file effect, and verify its exact
   bytes and ordinary-file identity.
9. After whole-operation verification, delete only the positively recognized
   command-owned recovery bundle. A failed cleanup leaves the verified effects
   and exact residual path for explicit Repair or Cleanup handling.

Attach requires a unique ID, a complete source inventory, no existing mapping
collision, and one record Create after all link effects. Sync compares the
complete current eligible inventory with the record. It may create links for
new source paths and delete retired projections only when each retired
destination is still the exact registered relative link. A source-unavailable
or incomplete inventory produces no deletion, link effect, or record update.
Changed occupants and unsafe mappings block the complete Sync request.

Detach is whole-library detachment. It plans deletion only for exact registered
links and deletes the existing record last. Any blocked, changed, missing, or
unverifiable mapping prevents all projection and record effects. Detach never
deletes or rewrites source files and has no selector-based partial form.

List and Inspect are read-only. They do not acquire the lock, create recovery or
record state, update generated navigation, repair projections, or invoke a
Library mutator. A matching projection does not gain ownership without the
consumer record.

## Recovery Relationship

The current schema-v1 recovery manifest records each covered target's logical
path, typed effect kind, and state-specific prior and intended identity. The
admissible Library attribution tuples are `library`/`attach`/`workspace`,
`library`/`sync`/`workspace`, and `library`/`detach`/`workspace`. The workspace
subject identity remains the trusted normalized physical workspace key. The
Library ID is a command-local management identity and is not substituted for
that subject or used as a source-reference operand.

Ordinary existing-file effects retain exact prior bytes and ordinary-file
identity. A prior-missing ordinary Create, such as a new Library record, has no
prior payload but records the intended ordinary-file identity. Relative link
effects retain no target bytes. Their identity is exactly the raw relative
target plus `relative-file-symbolic-link` kind. A link may therefore be
recreated as an exact dangling link when explicit recovery requires it.

Strong recovery comparison and application use the no-follow leaf observation:

- Restore a prior-missing target only by deleting the exact intended current
  object. For a Library record or link Create, a changed, third, or unsafe
  current object blocks rather than being adopted or followed.
- Restore prior ordinary bytes only when the current object is the exact
  intended ordinary file and the final leaf passes the ordinary no-follow
  guard. The restore uses the ordinary guarded atomic-file path.
- Restore a prior relative file link only from an exact missing final leaf by
  creating the recorded raw relative target and link kind. The target is not
  resolved and its bytes are never read for link identity.
- A third, mismatched, link, reparse, special, unsafe, or unavailable state
  blocks recovery. No recovery step writes, deletes, or restores a source
  target.

This capability supports explicit Repair evidence; it is not automatic rollback
or compensation. Residual evidence remains available when a Library operation
stops after one or more verified effects. Cleanup validates and deletes
recognized recovery bundles through its generic guarded-artifact path and does
not apply their entries.

## Capability And Platform Boundary

The supported implementation uses ordinary managed BCL file-link APIs and
no-follow inspection. It requires a real file symlink with the declared raw
relative target and link kind. If the platform cannot create and inspect that
real link, the Library operation is capability-gated `blocked` and leaves the
workspace, source files, projections, and record unchanged. There is no copy
fallback, native interop, P/Invoke, helper process, platform production project,
or custom filesystem.

Linux x64 is the initial executable evidence target. Evidence must prove real
relative-link creation and inspection, contained source and destination paths,
source/consumer physical disjointness, complete inventory, local sibling files,
missing and dangling links, collision blocking, record-last publication, and
no copy fallback. Other platform behavior is capability-gated and nonshipping
until equivalent real-link evidence exists. The design adds no Git action or Git
diagnostic, including fetch, pull, checkout, switch, stage, commit, or revision
inspection, even when a contained source directory happens to be a submodule.

## Status And Doctor Relationship

The Library producer supplies typed observations through the explicit
application-scoped composition already defined by the Architecture. Status
remains a lightweight, bounded observer: it consumes the record and registered
mapping view without acquiring the lease, mutating a projection, creating a
record, or inferring ownership from a matching link.

Doctor reads the strict consumer record under the existing `workspace and entry`
domain. A safely proven absent record means zero Libraries and complete Library
coverage, produces no Library finding, grants no ownership, and does not infer
any mapping. For a readable strict record, Doctor attempts a complete eligible
inventory for every Library source root named by that record. Those registered
source roots are the complete declared Library coverage. When every inventory
and registered mapping fact is complete, Doctor reports the complete facts. An
unavailable or incomplete fact emits `library.inventory-incomplete` and marks
Library coverage incomplete; Doctor never treats a readable prefix as a complete
inventory and never enumerates an unregistered source root. Neither Status nor
Doctor invokes Attach, Sync, or Detach, and neither uses Library record authority
as a substitute for the neutral no-follow guard.

## Related Current Sources

- [CLI Architecture](../architecture.md)
- [Shared CLI Operation Contract](../shared-operation-contract.md)
- [Mutation And Recovery Technical Design](mutation-and-recovery.md)
- [CLI Source References Interface Contract](../contracts/shared/source-references/interface.md)
- [CLI Source References Behavior Contract](../contracts/shared/source-references/behavior.md)
