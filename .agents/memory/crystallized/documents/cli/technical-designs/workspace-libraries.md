---
open-forge:
  description: Exact local-only Workspace Libraries record, inventory, projection, and recovery design
  responsibility: Define the strict Library record, complete source inventory, relative-link effects, capability gate, and explicit composition boundary
  tags: [Memory, Crystallized, Document, CurrentTruth, Evergreen, CLI, TechnicalDesign, Framework, Library, Workspace, Filesystem, Mutation, Recovery]
---

# Workspace Libraries Technical Design

## Boundary And Ownership

Workspace Libraries project eligible files from a workspace-contained source
root through relative file symlinks. The [CLI Architecture](../architecture.md)
defines project boundaries, BCL filesystem safety, Native AOT and composition.
The [Library contracts](../contracts/library/_library.md) define command behavior.
The [Workspace Permissions contracts](../contracts/shared/workspace-permissions/_workspace-permissions.md)
define consumer grants. Source bytes remain read-only input.

`Framework/Libraries/` owns record, source-root, inventory and mapping facts.
`Framework/Filesystem/` owns neutral no-follow leaf observation and portable
path grammar. `Framework/Mutation/` owns real-directory creation and typed
relative-file-link effects. `Framework/Recovery/` owns their exact identities,
comparison and guarded application. `Framework/Permissions/` owns strict grant
representation, observation, evaluation and proposed ordinary-file changes.
Commands own policy, plans, prompting, findings, results and orchestration.

Library permission policy lives below `Commands/Library/Shared/Permissions`.
It does not import Extension-private policy. `CliLibraryComposer` receives the
existing typed interactive session from root composition and constructs each
Library operation explicitly. Shell remains command-independent. There is no
DI container, runtime registry, reflection, extra project or dependency.

## Physical Source And Destination Boundaries

`sourceRoot` is a non-empty canonical portable workspace-relative directory.
It excludes absolute, empty, dot, parent and backslash segments and portable
aliases. The selected root and every ancestor are real ordinary directories,
lexically and physically contained by the workspace, without linked or reparse
ancestry. No `content/` or `.agents/` child is mandatory.

`destinationRoot` is `.` for the workspace root, or one canonical portable child
directory. A Library-owned `LibraryDestinationRoot` admits `.` without changing
source-root grammar. Every existing destination parent must be a real ordinary
directory. Only explicitly needed missing parents may be created, including
first-level directories. The existing workspace and consumer `.agents` control
roots are not created or replaced by a Library operation.

A destination root may contain a source root, as `.` normally does. Every actual
projection leaf and mutation target must nevertheless remain outside all
selected and registered Library source trees. Check lexical portable identity
and physical containment per target. Whole-root disjointness would incorrectly
reject the default mapping. No grant overrides this source-preservation check.
Detach derives source identity from records without requiring source existence.

## Strict Current Record

The separate ordinary consumer file `.agents/open-forge.libraries.json` has one
strict current schema-v1 shape:

```json
{
  "schemaVersion": 1,
  "libraries": [
    {
      "id": "team",
      "sourceRoot": "shared/team",
      "destinationRoot": ".apm/agents/team",
      "paths": ["checks/security.md", "review.md"]
    }
  ]
}
```

Require exactly `schemaVersion` and `libraries` at the root. Each Library has
exactly `id`, `sourceRoot`, `destinationRoot` and `paths`. The version is exactly
integer `1`. Unknown, duplicate, missing, null or wrongly typed properties are
malformed. There is no earlier-shape reader, migration, alternate spelling or
compatibility branch.

IDs retain the 1–128 lowercase ASCII stable-ID grammar and ordinal uniqueness.
Libraries and source-relative path lists are ordinally ordered. Paths are
unique within a Library. Derived destination leaves are unique across Libraries
under portable identity. Identical source-relative names at distinct mapped
destinations are valid. Empty records and empty path arrays remain valid.

`LibrariesRecordDocument` is the source-generated wire model. The strict codec
validates the entire object before producing immutable `LibrariesRecord` and
`LibraryRecord` facts. The record has no lifecycle authority and stores no
expected target, source bytes, hashes, timestamps, Git facts, dependencies,
globs or per-file remapping. Grants remain in the separate permission document.

## Inventory And Mapping Facts

`LibrarySourceRootObservation` exposes lexical/physical source-root and
containment facts. It has no mandatory-child or whole-root-disjointness fact.
`LibraryInventory` records the selected root, physical source root, complete
ordered eligible ordinary files and completeness. Each file has its
source-relative path and contained ordinary-file identity, not source bytes.

Inventory recursively enumerates the selected root. Exclude Git metadata,
linked/reparse directories and files, special entries, recognized manager
controls, and Open Forge source controls at their original `.agents` source
coordinates. Entry/overwrite/Loader classification does not turn external
`_name.md`, `*.overwrite.md` or README files into Framework metadata. Outside
recognized control scope these are ordinary opaque Library content. Never
follow an excluded link. An inaccessible directory, failed enumeration,
unavailable eligible ordinary file or unsafe required boundary prevents a
complete inventory. A readable prefix never permits retirement.

`LibraryMapping` is the sole derivation of three facts: source-relative path,
workspace-relative destination path and exact raw relative link target. For
source suffix `p`, derive source `sourceRoot/p` and destination `p` when the root
is `.`, otherwise `destinationRoot/p`. Derive the raw `/` target from the
actual destination parent to the source. A workspace-root leaf has the workspace
root as its parent. No source bytes or target resolution establish link identity.

`LibraryMappingSetRequest` carries one workspace, both roots and the complete
source-relative path set to observation. All ownership, collision, operational,
inspection and recovery consumers use derived destinations. Source suffixes
remain the membership keys for current-versus-registered reconciliation.

Final destination admission separately protects Git metadata, known manager
controls, Framework-owned paths/regions, `.agents` Loader/entrypoint/overwrite
controls, lifecycle/Library/permission/lock controls, operation temporary and
recovery storage, and source trees. Library external Markdown remains opaque.
Only mapped `.agents/**` leaves can affect existing generated navigation under
the Index contract. No new route chain, Loader or source entrypoint is created.

## Library Permission Planning

Library grants bind ID and source root. Their required `paths` and `directories`
arrays contain exact external files and recursive external folder scopes.
Extension grant shape and exact-file semantics remain unchanged. Directory
matching uses portable segment-prefix identity and covers strict descendant
leaves, never the directory itself. Workspace-root grants are invalid.

The command forms complete required/missing concrete external leaves. Live
missing leaves propose their immediate parent directory; root leaves and
retirement-only/Detach leaves propose exact files. Deduplicate scopes and remove
child proposals covered by another proposed directory. Top-level directories
are valid; do not invent a minimum depth. Prompts describe future descendants
explicitly and show the source, ID, exact current effects and scopes.

When the same ID has grants for a different source root, show old and new roots
and require explicit replacement approval. Replace only that Library subject
with the newly approved scopes. Preserve same-source existing grants and all
unrelated subjects. An old-source grant never admits a new-source operation.

`LibraryPermissionRequest`, `LibraryPermissionApproval` and
`LibraryPermissionStage` keep structural target facts, immutable approval,
observation, proposed file change, recovery target and failure separate.
`LibraryPermissionView` adds proposed/approved scopes and source-rebinding facts
to concrete required/missing leaves and the standard permission receipt fields.
A not-evaluated stage never claims no permission is required.

## Mutation And Recovery Lifecycle

Attach requires a new ID and complete inventory. Sync compares complete current
source suffixes with the recorded set, preserving both recorded roots. Detach
requires exact registered links without source enumeration. Missing, changed,
unsafe or unverifiable retired/Detach links block the complete request.
Unregistered occupants are never adopted. Multiple Libraries can share real
parents but cannot own one leaf.

Every mutator follows one complete plan:

1. Read records, source/mapping facts, protected targets, ownership and generated
   navigation. Finish structural preflight before asking for permission.
2. Derive required external leaves and scopes. Human prompt-capable application
   asks once; JSON, redirected execution and dry-run never prompt. Missing or
   declined permission blocks without effects. Cancellation is interrupted.
3. Acquire the existing same-workspace lease and reobserve all volatile facts,
   including exact permission bytes or absence. A changed plan or permission
   observation blocks; approval cannot transfer to a wider recomputed plan.
4. Prepare and verify one existing recovery bundle covering links, permitted
   generated-region changes, permission create/replace and record changes.
5. Apply and verify permission first. Then create required ordinary parents,
   create/delete exact relative links, and update permitted generated regions.
6. Publish and verify the Library record last. Remove only the positively
   recognized command-owned bundle after complete operation verification.

Before every effect, repeat no-follow leaf, ordinary ancestry and expected-state
checks. Relative-file-link Create accepts missing and produces the exact raw
symbolic link. Delete accepts that exact link and produces missing. Ordinary
file effects never operate through links. The source target is never written,
removed or used as recovery payload. Failure/interruption stops new effects;
verified effects and actual permission outcome remain visible. There is no
rollback or copy fallback. Failed cleanup retains explicit residual evidence.

Recovery retains exact prior ordinary bytes or prior absence, and raw relative
link kind/target. It needs no new bundle version or entry kind. Recreating an
exact dangling link is permitted. Library attribution remains
`library`/`attach|sync|detach`/`workspace`, with normalized workspace subject
identity, never the Library ID.

`LibraryResidualAttributionReader` compares recovery target paths against mapped
destinations, using recorded roots and raw link identity. It excludes the
permission control-file entry from automatic Library repair attribution.
`LibraryRecoveryPermissionReader` checks current grants under the same lease,
using the attributed recorded Library/source identity. Revoked or unavailable
permission blocks explicit recovery; recovery never prompts, restores grants,
widens them or reads source bytes. Permission restoration is manual from the
retained evidence. Cleanup only deletes recognized bundles through its existing
explicit guarded path and never applies entries.

## Read-Only And Other Consumers

List observes records, bounded source availability and registered mapped leaves
without a source inventory. Inspect inventories the selected root completely
and compares the mapped union. Both expose the recorded destination root and
source-relative/destination facts; automatic source IDs apply only to eligible
`.agents` destinations and remain null for opaque external files. Neither
command prompts, acquires a lease or writes persistent state.

Status remains bounded; Doctor attempts complete inventories of registered
sources only. Missing record means complete empty Library coverage. Incomplete
or unavailable inventory never becomes empty or grants permission to retire.
Their typed record/mapping facts use the central mapping, as do Extension
Install/Update/Remove Library-ownership guards and Repair attribution. Extension
content and permission semantics are otherwise unchanged.

## Capability And Evidence

Use ordinary managed BCL file-link APIs and no-follow inspection. Unsupported
real-relative-link capability is blocked without effects; there is no P/Invoke,
helper process, native bridge or copy fallback. The supported failure boundary
covers ordinary defects, interruption and cooperating processes, not malicious
same-user namespace races. No Git action or diagnostic is added.

Prove portable root grammar, exact schema, mapped ownership and grant scope at
Unit tier. Real-filesystem Integration proves recursive root inventory,
individual links, root leaves, ordinary parents, collisions, source preservation,
future-descendant grants, explicit rebinding, revocation, lease drift, partial
permission outcomes and exact recovery. Preserve three public journeys per
Library command and unchanged Extension journeys. Shared schema, composition and
filesystem changes require complete managed and supported linux-x64 Native AOT
evidence under the active Task.
