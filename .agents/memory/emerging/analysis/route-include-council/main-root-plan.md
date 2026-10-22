---
open-forge:
  description: The implementation plan for a configurable main root, with the inventory of every .agents assumption, the layout type, migration and ordered work packets
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Layout, Install, Settings, Plan]
---

# Configurable main root implementation plan

A read-only planner wrote this plan on 2026-10-21 against the route include branch, before the merged review. Names and line numbers may have moved since. [Task 85](../../../working/cli-development/tasks/task85-configurable-main-root.md) owns the work.

This plan implements the accepted design in the [route include council record](record.md#the-configurable-main-root-7). It proposes implementation contracts where the council leaves mechanics unspecified.

Read-only investigation completed. No files changed, builds or tests run, or Doctor command executed.

**Baseline caveat:** this snapshot contains settings v3 and `roots`, but `SourceReferenceParser.cs:11` still recognizes only `.agents/` exact paths. Include, Exclude and Help command definitions are absent. Complete those PR #6 changes before integrating #7.

## 1. Inventory

Paths below use these exact repository prefixes:

| Prefix | Repository path |
| --- | --- |
| `F` | `src/cli/framework/OpenForge.Cli.Framework/Framework` |
| `O` | `src/cli/operations/OpenForge.Cli.Operations/Commands` |
| `R` | `src/cli/rendering/OpenForge.Cli.Rendering/Presentation` |
| `T` | `src/cli/output-text/OpenForge.Cli.OutputText` |
| `H` | `src/cli/root/OpenForge.Cli/Composition` |

The source census contains **322 C# files**, plus `T/Help/concepts.md`. Counts include direct literals, calls to helpers that currently enforce `.agents`, pass-through models and comments. They are audit counts, not predicted edit counts. Files appear in one group only.

Reproduce the C# census with:

```powershell
rg -l --glob '*.cs' --glob '!**/tests/**' 'SourceLogicalPath\.|SourceIdentity\.|SourceFormClassifier\.|SourceGeneratedDestinationResolver\.|AgentsRoot|LoaderPath|HumanGuidePath|AuthoringGuidePath|WorkspaceSettingsDefinitions\.(DirectoryName|ImplicitPathPrefix|MemoryArchivePath)|\.agents' src/cli
```

**Meaning:** `K` = knowledge root, `C` = fixed control directory, `S` = native Skills location. Package `.agents/` paths are portable payload coordinates that require mapping, rather than installed filesystem coordinates.

| Consumer area | Files | Locations and dependency to separate |
| --- | ---: | --- |
| Source identity and discovery | 12 | **K/S.** `F/Sources/Identity/SourceLogicalPath.cs:8` defines the root, loader and guides. Its validation and parent arithmetic enforce `.agents/` at lines 35–55. `SourceIdentity.cs:14` strips a fixed prefix and reconstructs it at line 24. `SourceFormClassifier.cs:16` recognizes the loader and treats its parent specially at line 86. Audit all of `Identity`, `Inventory/SourceCatalogueReader.cs` and `Models/Inventory`, including candidate, layer, identity and selection-scope validation. |
| Loading and context | 32 | **K/S.** `F/Sources/Reading/SourceReadSessionReader.cs:34` chooses the scan root and selection scope. `Sources/Routing/SourceLoaderDestinationParser.cs:66` manufactures `.agents/{destination}`. `Sources/Loading/SourceGeneratedDestinationResolver.cs:23` chooses the loader’s parent and line 113 assumes a single root segment. Audit Reading, Routing, Loading, Selection, their models, and `O/{Context,Find,References}`. `O/Context/Shared/Projection/ContextProjectionBuilder.cs:196` also excludes guides through the hard-coded helper. |
| Generated navigation and Index | 21 | **K/S.** `F/GeneratedNavigation/GeneratedNavigationFormationBuilder.Derivation.cs:52` finds the fixed loader. `GeneratedNavigationFormationBuilder.cs:163` identifies direct children by their parent. `F/Sources/Sharing/SourceSharing.cs:23` restricts sharing records to `.agents`. `O/Index/Shared/Selection/IndexSkillCatalogueTraversal.cs:11` derives Skills from the knowledge root. Include generated-region formation, metadata completion and `O/Shared/RoutedMaintenance`. |
| Route operations | 74 | **K/S**, with **C** persistence. `O/Route/Create/Shared/Planning/RouteCreateTargetPlanner.cs:23` builds destinations from `AgentsRoot`. Move does likewise in `RouteMoveDestinationProjector.cs:72`. Init recognizes literal prefixes in `RouteInitTargetPlanner.cs:211` and checks the first segment in `RouteInitBinding.cs:216`. Remove scans `.agents` in `RouteRemoveSubjectSelector.cs:79`. Audit every List, Inspect, Create, Init, Update, Move and Remove leaf, shared scaffolding, source models, reference scanning and navigation projection. Include/Exclude must use the same layout once #6 supplies them. |
| Framework lifecycle | 49 | **K/C/S.** `O/Install/Shared/Planning/InstallPlanningBasisBuilder.cs:26` has one “Agents directory” expectation. `InstallEstablishmentPlanner.cs:440` creates parents beneath it. `Install/Shared/Configuration/InstallConfigurationChoices.cs:49` maps route IDs to `.agents/{id}`. `InstallConfigurationSettings.cs:34` hard-codes Memory paths. `O/Update/Shared/Planning/UpdateIntendedStateBuilder.cs:117` partitions payload files by prefix. `O/Remove/Shared/Planning/RemovePathSafety.cs:18` combines the root with reserved historical paths. Repair and Git rename evidence use fixed inventories and prefix stripping. **Cleanup has no direct census hit**, but must retain physical-workspace recovery identity and understand migration residuals. |
| Settings and exclusions | 11 | **C** for settings and ownership filenames, **K/S** for category expansion and defaults. `F/Settings/WorkspaceSettingsDefinitions.cs:12` defines `DirectoryName`, while line 13 incorrectly doubles it as `ImplicitPathPrefix`. Line 42 fixes the archive location. `WorkspaceSettingsCodec.Roots.cs:16` excludes all `.agents` paths. `WorkspaceRemovals.cs:16` expands categories through that prefix. Settings Reader/Writer and Ownership Reader must keep their current fixed locations. |
| Permissions | 5 | **K/S** implicit content destinations, **C** reserved controls. `O/Extension/Shared/Permissions/ExtensionDestinationPolicy.cs:16` grants implicit admission to `.agents/`. `O/Library/Shared/Permissions/LibraryPermissionScopePlanner.cs:41` and `F/Libraries/Shared/Permissions/LibraryRecoveryPermissionReader.cs:34` repeat that assumption. Keep `WorkspaceAllowList`’s explicit literal grants separate from layout-derived implicit admission. |
| Extensions | 22 | **K/S** installed destinations, **C** settings and receipts. `O/Extension/Install/Shared/Planning/ExtensionInstallPayloadNormalizer.cs:45` selects package targets before admission. Its line 28 expands categories through `.agents/`. Install and Update parent loops stop at `.agents`, including `ExtensionInstallEffectPlanner.cs:41`. Foundation checks use the fixed directory. Create’s `content/.agents/` scaffold remains the package format. Installed ownership must contain mapped destinations. |
| Libraries | 10 | **K/S** consumer roots and projections, **C** records. `O/Library/Shared/Planning/LibraryConsumerBoundaryReader.cs:26` and `LibraryMutationPlanningPolicy.cs:32` choose `.agents`. `LibraryDestinationPolicy.cs:70` combines controls, root protection and loader protection. `F/Libraries/Shared/Paths/LibraryEligiblePathPolicy.cs:30` searches for an `.agents` segment in source paths. Generated-navigation reading and List/Inspect/Detach completion also inherit root assumptions. Source locations never migrate implicitly. |
| Host pointers and payload | 8 | **K** selected loader, **S** payload exception, **C** reserved paths. `F/Distribution/Models/FrameworkPayloadAsset.cs:9` keeps root host filenames but aliases the runtime loader constant at line 11. `EmbeddedFrameworkSourceProjector.cs:26` treats package paths as installed paths. `Distribution/Shared/Content/WorkspacePayloadRenderer.cs:29` gates rendering by `.agents/`. Audit payload selection, source projection and alignment. Managed host handling also depends indirectly on `InstallIntendedStateBuilder.cs:482` and `InstallConfigurationIntendedStateBuilder.cs:200`. |
| Status and doctor | 16 | **K/C/S** reported separately. `F/Workspace/Operational/WorkspaceEntryOperationalContributor.cs:39` observes `.agents` and the fixed loader together. `F/Sources/Operational/RouteDoctorFactReader.cs:145` identifies root routes by parent depth. Audit workspace-entry facts, route facts, startup estimates and Doctor inspectors. Settings and ownership findings continue naming `.agents/open-forge*.json`. |
| Rendering and output text | 63 | **K/C/S**, selected by the producing operation. `R/Install/Shared/Selection/InstallReportSelector.cs:25` groups all effects beneath `.agents`. `R/Route/Move/Shared/Selection/RouteMoveReportSelector.cs:764` strips its prefix. `R/Repair/Shared/Selection/RepairReportSelector.cs:963` compares fixed path segments. `T/Context/ContextText.cs:14`, `T/Doctor/DoctorText.cs:490` and permission wording hard-code runtime locations. Package examples and fixed control-file messages remain literal. Includes `T/Help/concepts.md:5`. |

Three mandatory separations:

- Retain `WorkspaceSettingsDefinitions.DirectoryName` solely as **control-directory identity**.
- Retire `ImplicitPathPrefix` as a global permission or category rule.
- Replace runtime `AgentsRoot`, `LoaderPath`, `HumanGuidePath` and `AuthoringGuidePath` constants with layout members. Keep separately named constants for default package coordinates.

Parent, filename and canonical-path helpers must become root-neutral. Membership, routability and permission checks must then explicitly consult the layout. Merely relaxing validation would widen mutation authority.

## 2. The layout type

Place the new capability under `F/Workspace/Layout/`. One foundation owner owns this folder, settings shapes and ownership publication throughout #7.

The following is the frozen callable surface. Constructors validate lexical invariants. The resolver additionally validates physical observations.

```csharp
internal sealed record WorkspaceSourceMount(
    string Directory,
    string IdPrefix);

internal sealed class WorkspaceLayout
{
    internal WorkspaceLayout(
        CliWorkspace workspace,
        string canonicalMainRoot);

    internal CliWorkspace Workspace { get; }
    internal string MainRoot { get; }
    internal string ControlDirectory { get; }
    internal string SettingsPath { get; }
    internal string OwnershipPath { get; }
    internal string NativeSkillsRoot { get; }
    internal string NativeSkillsCataloguePath { get; }
    internal string LoaderPath { get; }
    internal string HumanGuidePath { get; }
    internal string AuthoringGuidePath { get; }
    internal string MemoryArchivePath { get; }
    internal string RootAgentPath { get; }
    internal string RootClaudePath { get; }
    internal ImmutableArray<WorkspaceSourceMount> SourceMounts { get; }

    internal bool IsMainRootPath(string path);
    internal bool IsNativeSkillPath(string path);
    internal bool IsUnroutedGuide(string path);
    internal bool IsImplicitContentDestination(string path);
    internal string CategoryDirectory(string categoryId);
    internal string MapPayloadPath(string packagePath);
    internal bool TryGetSourceRelativePath(
        string path, out string sourceRelativePath);
}

internal enum WorkspaceLayoutState
{
    Ready,
    Uninstalled,
    Mismatch,
    Invalid,
    Unavailable
}

internal sealed record WorkspaceLayoutRead(
    WorkspaceSettingsRead Settings,
    WorkspaceLayout? Layout,
    WorkspaceLayoutState State,
    ImmutableArray<WorkspaceLayoutIssue> Issues);

internal sealed class WorkspaceLayoutResolver(
    PhysicalPathResolver paths)
{
    internal ValueTask<WorkspaceLayoutRead> ReadAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken);
}
```

`WorkspaceLayoutIssue` has `Kind`, `Path`, `RelatedPaths` and `Cause`. Freeze kinds as `InvalidMainRoot`, `UnsafeMainRoot`, `LoaderMissing`, `LegacyLoaderPresent`, `RecordedLayoutMismatch` and `SettingsUnavailable`.

Member values:

| Member | Value |
| --- | --- |
| Controls | `.agents`, `.agents/open-forge.json`, `.agents/open-forge.lock.json` |
| Native Skills | `.agents/skills`, `.agents/skills/_skills.md` |
| Knowledge files | `<mainRoot>/{loader.md,README.md,authoring.md}` |
| Archive | `<mainRoot>/memory/archived` |
| Host files | `AGENTS.md`, `CLAUDE.md` |
| Default mounts | One mount `(".agents", "")` |
| Custom mounts | `("<mainRoot>", "")`, then `(".agents/skills", "skills")` |

Deduplicate overlapping default mounts. Discovery does not activate either mount.

`MapPayloadPath` maps `.agents/<suffix>` to `<mainRoot>/<suffix>`, except native Skills remain fixed. Controls and historical reserved control paths remain recognizable and prohibited package destinations before mapping. Non-`.agents` package paths remain literal.

Propagation:

```csharp
SourceCatalogueRequest(
    WorkspaceLayout layout,
    IEnumerable<string> logicalRoots);

SourceReadSessionReader.ReadAsync(
    WorkspaceLayout layout,
    CancellationToken cancellationToken);

SourceIdentity.DeriveId(
    string? canonicalPath,
    WorkspaceLayout layout);

SourceFormClassifier.TryClassify(
    string? canonicalPath,
    WorkspaceLayout layout,
    out SourceDocumentForm form);

SourceGeneratedDestinationResolver.Resolve(
    WorkspaceLayout layout,
    string parentCanonicalPath,
    bool isLoader,
    string destination);

GeneratedNavigationFormationBuilder.Build(
    SourceCatalogue observedCatalogue,
    IReadOnlyList<SourceLogicalSource> intendedSources);
```

`SourceCatalogue` and `SourceReadSession` carry their immutable `Layout`. Formation reads it from the catalogue.

- Operations receive the concrete resolver through existing explicit composition. Resolve once at operation entry, then pass the result through command-local planning facts.
- Source readers receive `WorkspaceLayout`, never rediscover settings.
- Install additionally receives a requested destination root through its request and constructs current and intended layouts.
- Status/Doctor aggregation resolves once and supplies the same observation to contributors.
- Rendering receives concrete paths and grouping facts through command results. It performs no settings reads.
- Revalidate the settings snapshot, layout observations and physical expectations under the existing mutation lease.

Do not add layout state to `CliWorkspace`. Its physical identity remains independent, as defined in `F/Workspace/Models/CliWorkspace.cs:16`.

**Default compatibility:** absent `mainRoot` resolves to `.agents`. Preserve source order, selected bytes, destinations, loader rows and existing result output. Intentional differences are new help/options and newly created settings declaring v4. Do not insert `"mainRoot": ".agents"` during unrelated rewrites.

## 3. Source identity

`F/Sources/Identity/SourceIdentity.cs:7` becomes layout-relative:

| Physical workspace path | Source ID |
| --- | --- |
| `.agents/memory/crystallized/_crystallized.md` with default layout | `memory/crystallized` |
| `docs/forge/memory/crystallized/_crystallized.md` with custom layout | `memory/crystallized` |
| `.agents/skills/example/SKILL.md` under either layout | `skills/example` |
| Included `CONTRIBUTING.md` | `null` |

Retain entrypoint recognition, overwrite pairing, exact case and collision behavior.

Freeze operand classification:

1. `./<workspace-path>` means an exact literal workspace path.
2. `.agents/<path>` remains a legacy exact literal workspace path.
3. Every other operand means a source ID.
4. Never redirect `.agents/foo` to `<mainRoot>/foo`.
5. Use `./docs/forge/...` for an unambiguous custom-root exact path. A bare `docs/forge/...` remains an ID.
6. Old `.agents` content outside native Skills has no short ID under a custom layout. Commands may admit it as an exact external file according to their existing contracts.

Update both shared contracts:

- `.agents/memory/crystallized/documents/cli/contracts/shared/source-references/interface.md:35`
- `.agents/memory/crystallized/documents/cli/contracts/shared/source-references/behavior.md`

They must add layout-relative identity, native Skills identity, generic exact paths, literal legacy operands, mismatch behavior and the distinction between resolution and mutation permission.

Reserve the short ID namespace `skills` for the native mount. A conflicting custom-root Skills category is a structural error, not a second native location.

## 4. Settings v4 and validation

Publish `schemas/v4/open-forge.schema.json`. Leave v1–v3 unchanged.

```json
{
  "$schema": "https://raw.githubusercontent.com/TheLithiumForge/open-forge/main/schemas/v4/open-forge.schema.json",
  "schemaVersion": 4,
  "mainRoot": "docs/forge"
}
```

Schema delta:

- Optional string `mainRoot`, default `.agents`.
- Retain unknown-key acceptance.
- Describe categories relative to the selected root, with the native Skills exception.
- Describe `roots` as literal external workspace files.
- Keep the existing lock schema unless ownership fields change.

Codec rules:

- Add `DeclaredMainRoot` and effective `MainRoot` to `WorkspaceSettingsDocument`.
- Absence means `.agents`. Explicit null, wrong type and empty values are invalid.
- Persist canonical workspace-relative directory paths with `/`, no leading `./` or trailing slash.
- Reject absolute paths, drives, UNC paths, traversal, empty segments, control characters and unsupported path syntax.
- Reject the workspace root, Git metadata and control locations. Permit `.agents` as the explicit default exception. Recommend rejecting its descendants as custom roots.
- CLI input may normalize accepted separator and leading-`./` forms. Authored settings must already be canonical.
- Physical validation rejects unsafe ancestry, non-directory roots and escaping or ambiguous aliases.
- Validate `roots` against the resolved main root and fixed controls, rather than rejecting every `.agents/...` path.
- Preserve existing `$schema`, `schemaVersion`, unknown values and property order on rewrites. Read absent, older and newer versions without fetching schemas.

This follows `.agents/directives/software/versioned-configuration-schema.md:13` and the existing preserving writer in `WorkspaceSettingsCodec.Roots.cs:94`.

Mismatch behavior:

| Observation | Read commands | Writing commands |
| --- | --- | --- |
| Selected loader exists and records agree | Normal | Normal |
| Selected loader missing in an established workspace | Report selected path and mismatch. Never fall back | Block |
| Custom root selected but `.agents/loader.md` remains | Read selected loader with a mismatch finding | Block |
| Ownership or managed pointers identify another root | Report disagreement | Block |
| Fresh workspace without a loader or prior Framework ownership | Report uninstalled | Install may establish the requested layout |
| Invalid or unreadable settings | Report actual settings failure | Block, never operate using fallback defaults |

`install --configure --main-root ...` may resolve a mismatch through an explicit plan. It must not infer a source by scanning for arbitrary loaders.

## 5. Migration

Add `O/Install/Shared/MainRootMigration/`. Do not extend the existing `InstallMigrationPlan` by changing its meaning. That type currently describes adoption changes at `Models/Planning/InstallMigrationPlan.cs:6`.

Freeze:

```csharp
internal sealed record InstallMainRootMigrationRequest(
    WorkspaceLayoutRead Current,
    string DestinationMainRoot);

internal sealed record InstallMainRootFileMove(
    string SourcePath,
    string DestinationPath,
    FileStateSnapshot Source,
    ImmutableArray<byte> DestinationBytes);

internal sealed class InstallMainRootMigrationPlanner
{
    internal ValueTask<InstallMainRootMigrationPlan> BuildAsync(
        InstallMainRootMigrationRequest request,
        CancellationToken cancellationToken);
}

WorkspaceOwnershipStore.PlanOwnership(
    WorkspaceOwnershipRead current,
    WorkspaceOwnershipDocument intended);
```

The plan carries current/intended layouts, complete inventory, file moves, typed Library link effects, directory effects, one settings change, one combined ownership change, host/ignore changes and recovery targets.

### Inventory and preflight

1. Enumerate actual filesystem entries, including ignored, untracked, binary, hidden, empty-directory, overwrite and private Working content. Do not use Git’s visible-file list.
2. Pin controls, native Skills and registered Library source trees. Nothing pinned moves.
3. Snapshot bytes and hashes, directory structure, supported link identities, ownership, settings, loader policy, host regions and ignore sections.
4. Resolve the current source from the configured layout and exact recorded evidence. Ambiguous competing roots block.
5. Reject collisions even when destination bytes happen to match. `--force` does not authorize merging roots.
6. Require a missing or empty destination, except the fixed pinned entries when returning to `.agents`.
7. Reject nested source/destination roots, case-only aliases, nested Git repositories, unsafe links, incomplete inventory and unknown ownership.
8. Accept only recorded, verified Library relative-file projections as movable link objects. Other link objects block.
9. Inspect recovery with `RecoveryBundleCatalogue` and `RecoveryBundleTargetStateReader`. Unresolved or unreadable relevant operations block. Completed historical bundles do not.
10. Verify moved private files retain their intended ignore protection. If authored rules would stop protecting them, block with exact affected paths.

### Translation rules

| Surface | Required translation |
| --- | --- |
| Framework/Extension ownership | Remap moved `Paths` and `OwnedRegion.Path`. Preserve owner IDs, versions and sharing relationships. Copying user files creates no ownership claim. |
| Exclusions | Keep category IDs stable. Remap `removedFiles` and `removedDirectories` beneath the moved namespace, including absent excluded paths. Exempt pinned paths. Keep removed Extension/Library IDs. |
| Link-ignore lists | Remap `ignoreLinksFrom` and `ignoreLinksTo` through the same component-aware mapping. Preserve order and absence-versus-explicit-empty meaning. |
| Explicit permission grants | Keep `allowInstallPaths` literal. Do not widen grants. New main-root and native Skills content use layout-derived implicit admission. |
| Install ignore section | Rewrite only the section recognized by `InstallIgnoreSection.cs:13`, together with `FrameworkOwnership.GitIgnoredRoutes`. Preserve all authored bytes outside it. |
| Library ignore rules | Rewrite only rules proven owned by the affected Library projections. Preserve unrelated authored rules. |
| Library projections | Preserve source roots and source bytes. Move only destinations inside the moved folder. Recompute raw relative link targets and verify them without following destination leaves. |
| `roots` | Preserve registered workspace paths, order and off registrations. Rebase existing loader destinations. Never recreate a missing row. |
| Loader/navigation | Preserve order, tags, conditions and disabled routes. Rebase links to stationary Skills, includes and pinned sources. |
| Authored links | Use parsed destination spans to rebase links in moved documents when their physical target remains fixed. Preserve labels, fragments and unrelated bytes. Report uneditable supported incoming references. |
| Host pointers | Rewrite only recognized Open Forge regions in root `AGENTS.md` and `CLAUDE.md`. Preserve surrounding user text. |
| Recovery history | Never rewrite old manifest paths, attribution or payloads. |

Library constraint: `LibraryPathIdentity.cs:11` derives every projection from one destination root. A registration straddling the moved folder cannot always preserve that shape. Recommend blocking that migration with an instruction to reattach using narrower destinations, rather than silently changing unrelated projections.

### Effects and continuation

Reuse `WorkspaceLockManager`, `MutationPreflight`, `MutationRevalidator`, `FileExpectation`, `PlannedFileChange`, `FileChangeApplier`, directory appliers, `RelativeFileLinkEffect`, `RecoveryBundleStore`, `RecoveryBundleAttribution` and `WorkspaceOwnershipStore`.

Under one physical-workspace lease:

1. Revalidate the complete plan.
2. Prepare one recovery bundle before target effects, attributed as Framework Install.
3. Create destination directories and files. Derive any rebased bytes from verified source snapshots. Verify every destination’s intended bytes before deleting its source.
4. Create and verify new Library projections.
5. Publish reviewed ignore changes, combined ownership, host pointers and settings. Publish selected `mainRoot` only after its loader and content are verified.
6. Delete verified originals and old projection links. Delete the old loader last. Remove only proven empty directories.
7. Verify final files, controls, links, pointers and preserved off states.

Retain the migration recovery bundle after success.

Existing recovery manifests contain prior bytes and intended identities, **not a replay journal**, as specified in `technical-designs/mutation-and-recovery.md:167`.

- Interruption returns partial effects, exact residual paths and the recovery bundle.
- Do not automatically roll back, resume from equal bytes or claim completion.
- A blind retry encountering partial migration blocks.
- Continuation requires restoring the recorded prior layout, or a newly reviewed plan whose expectations match every residual.
- Supply recovery instructions mapping original paths to bundle payloads. Never overwrite subsequent user edits during restoration.
- No new generic Repair rollback mode is included.

## 6. Packets

**Ownership rule:** owner A retains layout, settings and ownership shapes throughout. Other workers must not introduce alternative root resolvers or persistence fields. Every worker independently reads the C# Directives and preserves others’ work.

A folder below means one cohesive packet. Brace lists mean **separate packets**, one per named folder. Test work belongs to separate mirrored test packets, avoiding concurrent edits to production scopes.

Acceptance labels refer to section 7. Estimates are engineering days, including focused verification.

| Wave | Packet ownership | Frozen dependencies | Acceptance | Estimate |
| --- | --- | --- | --- | ---: |
| 0 | A1: new `F/Workspace/Layout/` | Section 2 | U-layout | 0.75 |
| 0 | A2: `F/Settings/` | A1 lexical layout contract | U-settings, I-settings | 0.75 |
| 0 | A3: `WorkspaceOwnershipStore.cs`, its existing path-release partial, ownership document and codec | Combined `PlanOwnership`, unchanged wire fields | U-ownership | 0.5 |
| 0 | A4: `schemas/v4/open-forge.schema.json`, architecture, new workspace-layout design, mutation/recovery design, shared source-reference behavior contract | Sections 2–5 | Contract/schema review | 0.5 |
| 1 | S1: `F/Sources/Identity/` | A1, source signatures | U-identity | 0.5 |
| 1 | S2: `F/Sources/Models/Inventory/` | Catalogue carries Layout, root-neutral path syntax | U-inventory | 0.25 |
| 1 | S3: `F/Sources/Inventory/` | S1–S2 | I-discovery | 0.5 |
| 1 | S4–S7: `F/Sources/{Reading,Routing,Loading,Selection}/`, separately | Layout-bearing catalogue/session | U-loading, I-loading | 0.25 each |
| 1 | S8–S9: `F/Sources/Models/{Routing,Loading}/`, separately | Existing result semantics, layout-neutral validation | U-loading | 0.25 each |
| 1 | C1–C3: `O/{Context,Find,References}/`, separately | A1 and source signatures | I-reading | 0.5 each |
| 2 | N1: `F/GeneratedNavigation/` | Layout-bearing catalogue | U-navigation | 0.5 |
| 2 | N2: `F/Sources/Sharing/` | Layout category mapping | U-privacy | 0.25 |
| 2 | N3: `O/Shared/RoutedMaintenance/` | N1–N2 | I-navigation | 0.5 |
| 2 | N4: `O/Index/` | N1–N3, fixed Skills mount | I-index | 0.5 |
| 2 | D1: `F/Distribution/` | `MapPayloadPath`, separate package coordinates | U-payload | 0.5 |
| 3 | R1–R9: `O/Route/{List,Inspect,Create,Init,Update,Move,Remove,Include,Exclude}/`, separately | Source contract, navigation contract | I-routes | 0.25–0.5 each |
| 3 | R10: `O/Shared/RouteScaffolding/` | Category mapping | I-routes | 0.25 |
| 3 | R11: `O/Route/Shared/` | Root-neutral source models, explicit layout membership | I-routes | 0.25 |
| 3 | E1–E4: `O/Extension/{Install,Update,Remove,Create}/`, separately | D1, mapped actual destinations | I-extensions | 0.25 each |
| 3 | E5: `O/Extension/Shared/Permissions/` | Implicit content admission and fixed reserved controls | U-permissions | 0.25 |
| 3 | L1: `F/Libraries/` | Fixed sources, exact projection identity | U-libraries | 0.5 |
| 3 | L2: `O/Library/Shared/` | L1, navigation and permission contracts | I-libraries | 0.5 |
| 3 | L3–L6: `O/Library/{Attach,Sync,Detach,Inspect}/`, separately | L1–L2 | I-libraries | 0.25 each |
| 4 | F1: `O/Install/Shared/Planning/` | D1, A3, current/intended layouts | I-install | 0.5 |
| 4 | F2: `O/Install/Shared/Configuration/` | Layout-aware choices, ignore/sharing translation | I-configure | 0.5 |
| 4 | F3–F5: `O/{Update,Remove,Repair}/`, separately | All prior source/package contracts | I-lifecycle | 0.5 each |
| 4 | F6: `O/Cleanup/` | Physical recovery identity, mismatch policy | I-cleanup | 0.25 |
| 4 | M1: new `O/Install/Shared/MainRootMigration/` | Section 5 and combined ownership publication | I-migration | 1 |
| 4 | M2: `O/Install/Shared/Operation/` | M1 plan, existing mutation/recovery effects | I-interruption | 0.75 |
| 4 | M3: Install definitions, binder, request, setup-input model and binding-input reader | `--main-root`, no-prompt rules | U-binding | 0.25 |
| 5 | Q1: `F/Workspace/Operational/` | Shared layout observation | I-observation | 0.25 |
| 5 | Q2–Q3: `O/{Status,Doctor}/`, separately | Q1, actual paths and migration residuals | I-observation | 0.25 each |
| 5 | Q4: `F/Sources/Operational/Shared/RenameEvidence/` | Layout-neutral paths and stable IDs | I-repair | 0.25 |
| 5 | Q5: `F/Sources/Operational/Shared/Routes/` | Layout-bearing sessions | I-observation | 0.25 |
| 5 | Q6: remaining direct files in `F/Sources/Operational/` | New shared contracts | I-observation | 0.25 |
| 5 | H1: `CliCompositionRoot.cs`, `CliStandaloneComposer.cs`, `CliCoreApplication.cs`, both `H/Shared/Git/` readers | Explicit resolver injection | Architecture checks | 0.25 |
| 5 | P-* | One command leaf under `R`, or one matching leaf under `T`, per packet | Concrete result paths, no filesystem access | Output snapshots | 0.5 total |
| 6 | V-U, V-I, V-E | Separate new test files grouped by section 7 scenario, each packet ≤5 files | Integrated production contracts | All section 7 gates | 1.5 |
| 6 | DOC-* | Separate ≤5-file batches listed below | Final behavior and examples | Link/example/parity checks | 0.75 |

Parallel execution:

- Wave 0 is owner A’s sequential foundation.
- Within subsequent waves, disjoint packets may run in parallel after their stated dependencies.
- Source, Extension and Library owners may prepare against frozen signatures concurrently. Integrate their executable checks after source/navigation foundations.
- Complete one custom-root Install → Context → Index slice before multiplying lifecycle changes.
- Owner A alone integrates shared-shape corrections.

Documentation batches:

1. Shared source-reference and permission contracts.
2. Install, Update, Remove, Repair and Cleanup contracts.
3. Route command contracts in batches of five.
4. Extension and Library contracts in separate batches.
5. Payload and dogfood loader, authoring and host files, each batch ≤5.
6. `docs/cli.md`, root README, Extension guide, Library design and concepts text.
7. CLI Skill references and documentation-site pages in separate batches.

**Estimate:** 22–25 engineering days, with roughly 9–12 elapsed working days using three implementation lanes. Re-estimate if generalized continuation or mixed Library projection support is selected.

## 7. Tests that must exist

Use the existing three runnable projects. Do not add a fourth test project.

### `OpenForge.Cli.Core.UnitTests`

- **U-layout:** default/custom members, multi-segment roots, controls, Skills, payload mapping, reserved destinations, case/Unicode and mount deduplication.
- **U-settings:** v4 schema, absent/older/newer versions, unknown keys, explicit null, malformed roots, declaration preservation and no-op rewrites.
- **U-identity/inventory/loading:** stable IDs, exact-path classification, literal `.agents` operands, overwrite pairing, collisions, custom loader parent arithmetic and Skills identity.
- **U-navigation/privacy:** preserved row order, off rows, required loading, conditions, private-route sharing and guide exclusion.
- **U-payload/permissions:** package versus installed paths, fixed controls, implicit main-root/Skills admission and literal explicit grants.
- **U-ownership/libraries:** combined ownership translation, no new claims on user files, pinned sources, relative targets and straddling-registration rejection.
- **U-binding:** option forms, repeat policy, existing-workspace Configure requirement and no prompts for preview, JSON or unattended execution.

### `OpenForge.Cli.IntegrationTests`

- **I-settings/discovery/loading/reading:** real custom-root filesystem, safe containment, missing loader and competing-root observations.
- **I-navigation/index/routes:** all source-reference forms, native Skills catalogue maintenance, custom-root CRUD, Include/Exclude and no writes to foreign included files.
- **I-install/configure/migration:** ignored/untracked/binary/private files, overwrites, empty folders, unknown user files, absent excluded paths and authored ignore bytes.
- **I-extensions/libraries:** Install/Update/Remove against mapped destinations, shared claims, fixed sources, moved projections and ignore ownership.
- **I-lifecycle/repair/cleanup/observation:** Update, Remove, Repair, Status and planned Doctor findings use selected paths. Historical bundles retain original paths and workspace keys.
- **I-interruption:** fail or cancel after copies, projection creation, ownership, pointers, settings and individual deletions. Assert recoverable bytes, honest residual reporting and blocked blind retry.
- Exercise stale plans, concurrent cooperating mutations and source edits between copy and deletion.

### `OpenForge.Cli.EndToEndTests`

Add published-process journeys beside the existing `PublishedInstallProcessTests.cs:28` and `PublishedCleanupProcessTests.cs`:

1. **Custom-root install:** `install --main-root docs/forge`, preview then unattended apply. Verify fixed controls/Skills and root host pointers.
2. **Migration:** default → custom → another custom → default, preserving hashes, source IDs, overwrites, private files and off registrations.
3. **Update:** custom-root payload refresh without recreating `.agents/loader.md`.
4. **Remove:** mapped content and ownership are removed according to selection, preserving controls, Skills and unrelated content.
5. **Cleanup:** migration and historical bundles remain associated with the same physical workspace.
6. **Offline startup:** with the CLI unavailable, independently follow `AGENTS.md` and `CLAUDE.md` to the selected loader, native Skills and included files. Repeat with README deleted.
7. **Reading parity:** loader-only, Context-only and concepts-led readers reach the same ordered required files.
8. **No reactivation:** remove a loader row, then run Index, Update, Configure and Extension Install. It stays off.
9. **Default compatibility:** existing default-root output and payload bytes remain unchanged apart from explicit v4/help changes.

Run the required managed and supported Native AOT gates across the established platform matrix before shipping. These are future acceptance requirements, not checks performed during this planning task.

## 8. Risks and open questions

| Risk or maintainer question | Recommendation |
| --- | --- |
| Snapshot has only part of #6 | Require completed #6 source-reference, Include/Exclude and Help behavior before #7 integration. |
| “Byte-identical default” could conflict with v4 | Preserve existing workspace bytes and output. Explicitly allow new settings declarations and advertised help changes. |
| Partial migration lacks replay state | Keep existing recovery semantics and block blind retries. Generalized continuation needs a separately accepted durable-state design. |
| Authored ignore rules may expose private files after moving | Preserve authored bytes and block migration until reviewed rules protect the destination. |
| Library registrations can straddle the moved folder | Block these plans with narrower reattachment instructions. Supporting them requires a new projection mapping contract and possibly lock schema v2. |
| Library sources may live inside the old root | Pin their subtrees. Block overlaps that prevent an unambiguous inventory or projection plan. |
| Custom-root `skills` could collide with native IDs | Reserve `skills` for `.agents/skills`. Never create a second native category. |
| Older CLIs ignore `mainRoot` | Document the minimum CLI version in layout help and managed host guidance. Preserve fully functional offline startup. |
| Competing loaders or hand-edited settings can look recoverable | Never select a fallback silently. Configure requires one explicit, evidence-backed source layout. |
| Arbitrary prose may contain old paths | Rewrite parsed supported links and managed pointer regions only. Report remaining literal references for maintainer review. |