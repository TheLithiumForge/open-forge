---
open-forge:
  description: Exact local-only Workspace Libraries record, inventory, projection, and recovery design
  responsibility: Define typed Library ownership, complete source inventory, relative-link effects, capability gate, and explicit composition boundary
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
comparison and guarded application. `Framework/Settings/` owns authored settings observation, shared allow-list
evaluation and proposed explicit settings changes. No Library source binding
exists in permission state.
Commands own policy, plans, prompting, findings, results and orchestration.

Library permission policy lives below `Commands/Library/Shared/Permissions`.
It does not import Extension-private policy. `CliLibraryComposer` receives the
neutral `CliInteractionComposition` with `CliTerminal` and typed `CliPrompts`
from root composition and constructs each Library operation explicitly. Shell
remains command-independent. There is no
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

## Current Ownership And Publication

`LibraryRegistrationReader` projects Library claims from the shared forgiving
`.agents/open-forge.lock.json` read into immutable typed registrations. IDs retain
the 1–128 lowercase ASCII stable-ID grammar. Roots and source-relative paths
retain portable validation; mapped destinations must be unambiguous. Projection
canonicalizes ordinal ID and path order. Unknown members or schema versions do
not gate an operation. An absent, unreadable, nonordinary, malformed or
uninterpretable lock supplies no usable registrations and an informational
ownership observation. Actual source and mapped destination checks remain
mandatory before effects.

There is one ownership publication and one public state-file outcome, naming
the lock. Its exact snapshot supplies prior recovery bytes and revalidation.
Publication follows verified effects, preserves unrelated owners, and is
best-effort. Final detach removes the selected registration while retaining
other owners. No old Library record is read, written, converted or deleted.

Library claims contain no expected link target, source bytes, hashes, timestamps,
Git facts, dependencies, globs, or per-file remapping. Exact link identity derives
from source root, destination root, and source-relative suffix. Permission stays
in the authored workspace settings.

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
controls, authored settings and generated ownership controls, operation temporary and
recovery storage, and source trees. Library external Markdown remains opaque.
Only mapped `.agents/**` leaves can affect existing generated navigation under
the Index contract. No new route chain, Loader or source entrypoint is created.

## Library Permission Planning

Library admission uses the same `allowInstallPaths` as Extension admission.
An entry admits itself and descendants through the shared portable path
comparison. There is no persisted ID, source root, file/directory discriminator
or rebinding graph in permission state. Existing reserved, source-tree,
ownership and physical checks remain independent.

The command forms complete required/missing external destination strings. Live
missing leaves propose their immediate parent directory; root leaves and
retirement-only/Detach leaves propose exact files. Deduplicate scopes and remove
child proposals covered by another proposed directory. Top-level directories
are valid. Prompts describe future descendants and the operation's source, ID,
exact effects and scopes. Those operation coordinates do not become grant keys.

Always approves the displayed scopes and stages a settings update using the sole
settings codec. Prospective grants are evaluated in memory; publication waits
for confirmed application under the existing lease, revalidation and recovery
rules. Once approves this operation without changing settings; cancel or refusal
before application applies nothing. Explicit `--allow-path` uses the same
staged permission plan and publishes only during application. Revalidation
compares the actual settings observation; changing a Library source never
requires grant rebinding. Unknown authored keys remain preserved.

`LibraryPermissionRequest`, `LibraryPermissionApproval` and
`LibraryPermissionStage` keep structural target facts, immutable approval,
observation, proposed file change, recovery target and failure separate.
`LibraryPermissionView` adds proposed/approved `{kind,path}` scopes to destination
strings and the standard decision/action/outcome fields. A not-evaluated stage
never claims no permission is required. Interactive settings publication requires
verified recovery before either Create or Replace, before content effects.

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
   declined permission blocks without effects. Cancellation has the `cancelled`
   status.
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
removed or used as recovery payload. Failure/cancellation stops new effects;
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
for the actual attributed destination strings. Recorded Library/source identity
still establishes mapping and source protection, not a grant binding. Revoked
or unavailable external permission blocks explicit recovery; recovery never prompts, restores grants,
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
sources only. Unavailable ownership means a complete information observation with no inferred registrations. Incomplete
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
filesystem changes require complete managed and supported Windows Native AOT
evidence under the active Task. That gate is host-only: it requires `vswhere` on
`PATH` and cannot run inside a sandboxed worker, so an overseer or other
unsandboxed host must run it.
