---
open-forge:
  description: The frozen implementation plan for Task 86, with the foundation API, the recovery-gate audit, six waves of packets, tests and documentation
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Locking, Recovery, Plan]
---

# Workspace Runtime Lock Plan

The CLI architect of the [lock scope council](record.md) wrote this plan from the chair decisions. It is frozen for [Task 86](../../../archived/cli-development/tasks/task86-workspace-runtime-lock.md). Implementers read this file, not the council.

## Chair amendments

These override the plan below where they differ.

1. **Native interop is `flock` only.** Do not add `UnixLockFileTypeReader` or any native `stat` layout. Before opening, reject links and directories through the BCL (`FileSystemInfo.LinkTarget`, `FileAttributes.Directory` and `FileAttributes.ReparsePoint`). After opening, reject a handle whose `FileStream.CanSeek` is false as `InvalidTarget`, which covers FIFOs, sockets and devices. Windows keeps today's attribute checks.
2. **Temporary files stay in the worktree.** Add `$env:TEMP = $env:TMP = Join-Path (Get-Location) 'artifacts\tmp'` and create that folder in "Build in the sandbox", so sandboxed workers can run integration tests that create temporary workspaces.
3. **PR #7's new F20 case changes meaning.** `UnusableDataHomeBlocksWriterAndNamesTheSetting` expected a relative `OPENFORGE_DATA_HOME` to block `index`. Under D6 the legacy lock is skipped, so `index` must succeed and create `.open-forge-state/`. Packet H rewrites that case. Packet D replaces PR #7's lock wording with the cause-specific messages.
4. **Writing commands warn only about history that signals a problem.** Drafts, invalid or damaged candidates and unavailable coverage are warnings. A verified final bundle from an earlier run is normal retained evidence. Writing commands do not report it, so ordinary successful workflows keep exit code 0. Status, Doctor and Cleanup still report every bundle. Added on 2026-10-23 after the Wave 4 integration, when Extension Update and Remove warned about the bundles their own earlier runs keep on purpose.
5. **The legacy read-only fallback opens without blocking on Unix.** It uses a native `open` with `O_RDONLY | O_NONBLOCK | O_CLOEXEC` next to `flock`, and rejects a handle that cannot seek before locking, so a FIFO at the legacy lock path cannot hang a command that already holds the local lock. This narrows amendment 1. Added on 2026-10-23 from the closeout review.

Baseline: `c2913bab13ffbc57c370182c7794267a0a10ab54`. This plan incorporates the frozen decisions and is sufficient without the council documents. Commands below are execution instructions, not claimed test results.

Keep one lease per selected physical workspace, covering today’s under-lease revalidation, recovery preparation, effects, verification and operation-owned recovery deletion. Planning, prompts, dry runs, refusals and verified no-ops create nothing. Separate worktrees remain the parallelism boundary.

## Build in the sandbox

Paste this section into every packet.

The worker is Codex in a Windows `workspace-write` sandbox. It cannot write outside its worktree, cannot commit, and must leave its diff uncommitted. Load `.agents/loader.md` and the applicable C# and CLI instructions. Do not revert another worker’s changes.

```powershell
$env:APPDATA = Join-Path (Get-Location) 'artifacts\nuget-appdata'
New-Item -ItemType Directory -Force (Join-Path $env:APPDATA 'NuGet')
$env:OPENFORGE_DATA_HOME = Join-Path (Get-Location) 'artifacts\data-home'
dotnet restore OpenForge.Cli.slnx --configfile NuGet.Config

$env:DOTNET_CLI_USE_MSBUILD_SERVER = '0'
dotnet build OpenForge.Cli.slnx -c Release --disable-build-servers -m:1 -p:OpenForgeSkipDevelopmentPublish=true
```

Tests use:

```powershell
dotnet run --project <test project> -c Release --no-build -- --filter-class <class>
```

The existing `TestProcessIsolation.cs` files call `TestDataHome.RedirectCurrentProcess`, which creates an OS temporary directory. The override above does not relocate that fixture. The overseer runs tests that write outside the worktree, including affected integration and end-to-end suites and unchanged test-process initialization. Do not bypass sandbox restrictions.

The overseer must build a fresh development publish before end-to-end execution. A build with `OpenForgeSkipDevelopmentPublish=true` does not refresh that executable.

## Path notation and ownership

Paths below are repository-relative. Braces denote explicit alternatives.

| Prefix | Directory |
|---|---|
| `F` | `src/cli/framework/OpenForge.Cli.Framework/Framework` |
| `O` | `src/cli/operations/OpenForge.Cli.Operations/Commands` |
| `R` | `src/cli/rendering/OpenForge.Cli.Rendering/Presentation` |
| `T` | `src/cli/output-text/OpenForge.Cli.OutputText` |
| `U` | `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests` |
| `I` | `src/cli/tests/integration/OpenForge.Cli.IntegrationTests` |
| `E` | `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests` |
| `S` | `src/cli/tests/support/OpenForge.Cli.TestSupport` |
| `C` | `.agents/memory/crystallized/documents/cli` |

The test project in each test directory has that directory’s name plus `.csproj`.

Packets A and B have one foundation reasoning owner. Only that owner changes the frozen shared APIs below. Other workers return integration problems to that owner instead of adding overloads, alternate location calculations or command-local locking implementations.

## Frozen foundation API

Names explicitly marked **new** are additions. Existing namespaces retain the `OpenForge.Cli.Core` prefix and follow their physical directories. All signatures below are internal unless already public through disposal interfaces.

### Runtime paths

Add **new** `F/Workspace/Shared/Runtime/WorkspaceRuntimePaths.cs`:

| Member | Signature |
|---|---|
| Runtime directory | `static string DirectoryPath(CliWorkspace workspace)` |
| Sentinel | `static string LockPath(CliWorkspace workspace)` |
| Ignore file | `static string IgnorePath(CliWorkspace workspace)` |
| Recovery root | `static string RecoveryRoot(CliWorkspace workspace)` |
| Reservation | `static bool Contains(CliWorkspace workspace, string absolutePath)` |

Paths derive from `workspace.PhysicalRoot`:

```text
.open-forge-state/
  .gitignore
  workspace.lock
  recovery/v1/<WorkspaceIdentity.Key(workspace.PhysicalRoot)>/
```

`Contains` performs normalized, component-boundary containment with the existing platform path comparison. Callers check both lexical candidates and resolved physical paths, preventing aliases into the reserved subtree.

Add **new** `WorkspaceRuntimeBootstrap.EnsureCreated(CliWorkspace workspace, CancellationToken cancellationToken) → WorkspaceLockFailure?` under `F/Mutation/Locking/Shared/Runtime`.

It creates only an ordinary runtime directory and ensures its ordinary `.gitignore` contains `*\n` before creating any other file there. The runtime ignore file is managed and repairable after interrupted initialization. Unsafe objects are refused. Concurrent bootstrap uses exclusive ordinary-file access and bounded failure, without a contention retry loop. Install’s bounded root `.gitignore` section stays unchanged.

### Lock result, lease and bridge

Under `F/Mutation/Locking/Models` add:

- **New** `WorkspaceLockFailureKind`: `Busy`, `AccessDenied`, `Unsupported`, `InvalidTarget`, `IoError`, `Cancelled`.
- **New** `WorkspaceLockFailure(WorkspaceLockFailureKind Kind, string Path, FilesystemFailure? Detail = null, int? NativeError = null)`.
- **New** `LegacyWorkspaceLockState`: `HeldReadWrite`, `HeldReadOnly`, `SkippedAbsent`, `Unavailable`, `Failed`.
- **New** `LegacyWorkspaceLockResult` with read-only `State`, `WorkspaceLockStoreRoot? StoreRoot`, `string? LockPath`, `FileStream? Handle`, `WorkspaceLockFailure? Failure`. Held states require a live handle. Failed requires a failure. Skipped and unavailable states carry no handle.

Change existing `WorkspaceLockResult`:

- Add `WorkspaceLockFailure? LockFailure`.
- Keep `State`, `Lease`, `Failure` and `Cause`. The latter two project from the typed failure for existing consumers.
- Keep `Acquired(WorkspaceLockLease lease)`.
- Replace `Failed(FilesystemFailure failure)` with `Failed(WorkspaceLockFailure failure)`.
- Replace `Cancelled()` with `Cancelled(string path)`.
- Every non-acquired result carries its typed cause and exact failing path.

Keep these existing manager signatures:

```text
WorkspaceLockManager(WorkspaceLockStoreRoot storeRoot)
static WorkspaceLockManager CreateForCurrentUser()
ValueTask<WorkspaceLockResult> AcquireAsync(
    WorkspaceLockRequest request,
    CancellationToken cancellationToken)
```

The constructor’s store root now controls only the legacy bridge. It never relocates the workspace lock.

Change `WorkspaceLockLease` construction to:

```text
WorkspaceLockLease(
    WorkspaceLockRequest request,
    FileStream handle,
    LegacyWorkspaceLockResult legacy)
```

Keep `Request`, `LockPath`, `IsHeld`, `IsHeldFor(CliWorkspace)`, `Dispose()` and `DisposeAsync()`. `LockPath` is now the fixed workspace sentinel. Remove `StoreRoot` as the lease’s primary identity and expose:

```text
WorkspaceLockStoreRoot? LegacyStoreRoot
string? LegacyLockPath
LegacyWorkspaceLockState LegacyState
bool HoldsLegacyLock(WorkspaceLockStoreRoot storeRoot)
```

`IsHeldFor` validates physical workspace identity and the fixed local path. A held legacy handle must also remain live. Disposal releases legacy first and local in `finally`, including failed acquisition and cancellation paths.

Add **new** `F/Mutation/Locking/Shared/Legacy/LegacyWorkspaceLockBridge.cs`:

```text
static LegacyWorkspaceLockResult Acquire(
    WorkspaceLockRequest request,
    WorkspaceLockStoreRoot? storeRoot,
    CancellationToken cancellationToken)
```

Keep `WorkspaceLockPathIdentity` and `WorkspaceLockStoreRoot` as the exact legacy naming and data-home rules. Do not change their hash, friendly-name normalization or `locks/v1` layout.

Acquisition order is fixed:

1. Bootstrap runtime and acquire local sentinel.
2. Resolve legacy root once and attempt read-write `OpenOrCreate`, `FileShare.None`.
3. On access denied, attempt read-only exclusive open when the legacy file exists.
4. Skip only when access was denied and absence was established, or data-home resolution is unavailable, including a relative `OPENFORGE_DATA_HOME`.
5. Legacy contention releases local and returns `Busy`. Other actual locking or filesystem failures remain typed failures.
6. Return a composite lease only after these steps.

`File.Exists == false` is insufficient evidence of absence because it suppresses access errors. Use typed observation. Preserve legacy files and bundles. Keep the legacy zero-length compatibility validation, while removing all length and content checks from the new sentinel.

### Checked Unix ownership

Add **new** `F/Mutation/Locking/Shared/Interop/UnixWorkspaceLock.cs`:

```text
static WorkspaceLockFailure? TryAcquire(
    SafeFileHandle handle,
    string path,
    CancellationToken cancellationToken)

private static partial int Flock(int fd, int operation)
```

Use source-generated `LibraryImport`, entry point `flock`, `SetLastError = true`, and platform libc resolution. Enable unsafe generated interop only in `OpenForge.Cli.Framework.csproj`.

Hold a safe-handle reference across descriptor extraction and the call. Pass `LOCK_EX | LOCK_NB`, numerically `2 | 4`. Read errno immediately. Success alone grants ownership. Retry `EINTR` with cancellation checks. Map platform `EWOULDBLOCK` to `Busy`, locking-unavailable errors to `Unsupported`, permission errors to `AccessDenied`, and remaining errors to `IoError`. Preserve errno. Never inspect exception message text to infer contention.

Windows uses `FileShare.None`. Unix also performs the explicit checked call after opening. A successful managed open is insufficient, as the pinned runtime can ignore unsupported implicit locking. See [.NET’s Unix handle implementation](https://raw.githubusercontent.com/dotnet/runtime/v10.0.0/src/libraries/System.Private.CoreLib/src/Microsoft/Win32/SafeHandles/SafeFileHandle.Unix.cs).

Add **new** `UnixLockFileTypeReader` in the same interop directory:

```text
static WorkspaceLockFailure? Validate(
    string path,
    bool directory,
    bool allowMissing)
```

Use checked, no-follow native metadata before opening. Ordinary runtime directories are allowed. Sentinel directories, links, FIFOs, sockets and devices are refused without opening or removing them. BCL attributes alone do not establish ordinary Unix file type.

Keep native metadata layouts private and bounded to supported x64 and arm64 targets. Use Linux `lstat` with the platform stat layout, and Darwin’s correct inode64 symbol and layout. Verify against [glibc](https://raw.githubusercontent.com/bminor/glibc/master/sysdeps/unix/sysv/linux/bits/struct_stat.h), [Darwin stat](https://raw.githubusercontent.com/apple-oss-distributions/xnu/main/bsd/sys/stat.h) and [Darwin symbol aliases](https://raw.githubusercontent.com/apple-oss-distributions/xnu/main/bsd/sys/cdefs.h). Unknown platforms return `Unsupported`. This is part of the bounded D2 native exception, not a general filesystem replacement.

### Typed recovery location

Add **new** files under `F/Recovery/Models/Storage`:

```text
enum RecoveryStoreKind
    Workspace
    Legacy

sealed record RecoveryStoreLocation
    CliWorkspace Workspace
    RecoveryStoreKind Kind
    string RootPath
    string WorkspaceDirectory
    WorkspaceLockStoreRoot? LegacyLockStoreRoot

sealed record RecoveryStoreObservation(
    RecoveryStoreKind Kind,
    RecoveryStoreLocation? Location,
    RecoveryBundleCatalogueResult Catalogue)
```

Locations are immutable, normalized and constructed only by **new** `F/Recovery/Shared/Storage/RecoveryStoreLocator.cs`:

```text
static RecoveryStoreLocation Local(CliWorkspace workspace)
static RecoveryStoreLocation? Legacy(CliWorkspace workspace)
```

Resolution never creates storage. The legacy location records the matching lock root. Null legacy resolution becomes an unavailable observation, not an empty successful store.

Freeze these changes:

| Existing owner | Final signature or addition |
|---|---|
| `RecoveryBundleStore` | Keep `PrepareAsync(RecoveryBundleInput, CancellationToken)`, always select `Local` |
| `RecoveryBundleCatalogue` | Keep `ReadAsync(CliWorkspace, CancellationToken)`, explicitly local-only |
| `RecoveryBundleCatalogue` | Add `ReadAsync(RecoveryStoreLocation, CancellationToken) → ValueTask<RecoveryBundleCatalogueResult>` |
| `RecoveryBundleCatalogue` | Add `ReadStoresAsync(CliWorkspace, CancellationToken) → ValueTask<ImmutableArray<RecoveryStoreObservation>>`, local then legacy |
| `RecoveryBundleCandidateSnapshot` | Required `RecoveryStoreLocation Location`, including draft and invalid candidates |
| `RecoveryBundleVerifiedRead` | Required `RecoveryStoreLocation Location` |
| `RecoveryBundlePreparation` | Read-only `Location` delegated from `Verified` |
| `RecoveryBundleReader` | `ReadFinalAsync(RecoveryStoreLocation, string bundlePath, CancellationToken)` |
| `RecoveryBundleReader` | `VerifyExpectedAsync(RecoveryBundleInput, RecoveryStoreLocation, string bundlePath, RecoveryBundleCandidateKind, CancellationToken)` |
| `RecoveryBundleReader` | `ReadExpectedFinalAsync(RecoveryBundleInput, RecoveryStoreLocation, string bundlePath, CancellationToken)` |
| `RecoveryBundleReader` | Keep `ReadSelectedFinalAsync(CliWorkspace, RecoveryBundleCandidateSnapshot, CancellationToken)`, use candidate location |
| `RecoveryBundleDeletionGuard` | `OpenSessionAsync(WorkspaceLockLease, RecoveryStoreObservation, CancellationToken)` |
| `RecoveryBundleDeletionGuard` | Keep `DeleteAsync(WorkspaceLockLease, RecoveryBundleCandidateSnapshot, CancellationToken)` |
| `RecoveryDeletionStorageBoundary` | Replace `ReadWorkspace` with `ReadLocation(RecoveryStoreLocation) → FilesystemFailure?` |

Preserve existing return types where not restated. Candidate factories receive location explicitly, except `VerifiedFinal`, which obtains it from the verified read.

Reader identity checks, `RecoveryBundleDeletionValidation`, deletion sessions and `RecoveryApplicationEvidence` use recorded locations. They never re-resolve the environment during deletion. Legacy deletion additionally requires `lease.HoldsLegacyLock(location.LegacyLockStoreRoot)`.

In `RecoveryResidualOperationalModels.cs`, add store-specific status and doctor views carrying kind, nullable location, coverage, candidates and cause. Add `Stores` collections to existing aggregate views, preserving their aggregate properties. The contributor returns available candidates even when the other store is unavailable.

### Presentation boundary

Rendering cannot reference Framework directly. Add **new** Operations-owned `WorkspaceLockFailureInfo` and `WorkspaceLockFailureReason` under `O/Shared/Models/Mutation`, with the same six causes and exact path. Add `WorkspaceLockFailureProjection.From(WorkspaceLockFailure)` under `O/Shared/Mutation`.

Writing findings gain an optional `LockFailure` property of that Operations type. Rendering selects OutputText wording from it. Never put the only path in `InstallFinding.Cause`, which truncates text.

Freeze common wording entry points:

```text
CliFindingWording.WorkspaceLockUnavailable(WorkspaceLockFailureInfo failure)
CliFindingWording.RecoveryHistory(string path)
CliFindingWording.RecoveryCoverageUnavailable(string? path, string cause)
```

Preserve command finding codes, semantic status, stream and exit mappings, including Install’s `WorkspaceUnsafe`. Only established contention says another writer exists. No lock or recovery deletion advice.

## D5 audit

Locations are snapshot line numbers under `O/`. Historical warnings name every observed artifact. Unavailable historical coverage warns without vetoing independent current-state validation. Cancellation still stops. Newly required recovery preparation must still succeed.

| Gate | Protection today | Replacement |
|---|---|---|
| `Route/Shared/Inclusion/Application/RouteInclusionApplication.cs:58` | Any candidate vetoes Include or Exclude before recovery preparation | Keep replanning, observed-input comparison and mutation revalidation already preceding it |
| `Route/Move/Shared/Application/RouteMoveRecoveryLifecycle.Preparation.cs:34` | Any candidate vetoes a move requiring recovery | Keep `RouteMovePlanRevalidator` and exact source, destination and reference expectations |
| `Route/Create/Shared/Application/RouteCreateRecoveryLifecycle.cs:69` | Any candidate vetoes Create recovery preparation | Keep `RouteCreatePlanRevalidator` and `MutationRevalidator` before effects |
| `Route/Init/Shared/Planning/RouteInitPlanFinalizer.cs:99` | Nonempty catalogue prevents finalizing a plan requiring recovery | Keep ownership, prospective topology and mutation preflight checks |
| `Route/Init/Shared/Application/RouteInitRecoveryLifecycle.cs:82` | Repeats that veto during apply | Keep `RouteInitPlanRevalidator` and exact file and directory expectations |
| `Route/Update/Shared/Application/RouteUpdateRecoveryPreparer.cs:55` | Any candidate vetoes Update recovery preparation | Keep `RouteUpdateApplicationPreparer.PrepareAsync`, including whole-plan and per-expectation validation |
| `Route/Remove/Shared/Application/RouteRemoveRecoveryLifecycle.Preparation.cs:34` | Any candidate that is not a verified final vetoes removal | Keep `RouteRemovePlanRevalidator`, inventory and deletion expectations |
| `Extension/Install/Shared/Planning/ExtensionInstallFoundationReader.cs:101` | Draft, invalid candidate or unavailable catalogue vetoes foundation planning | Keep ownership, topology, permissions, destination checks and under-lease replan |
| `Extension/Update/Shared/Planning/ExtensionUpdatePlanner.cs:815` | `IsExpectedRecovery` requires every candidate to be a verified final | Remove historical predicate, retain current plan validation and exact `allowed` preparation proof at line 825 |
| `Extension/Remove/Shared/Planning/ExtensionRemovePlanner.cs:619` | Same historical predicate in normal and no-op planning | Remove historical predicate, retain current ownership and target validation and exact `allowed` proof at line 629 |

No gate’s immediate effect is unknown. The blanket predicates do not establish a target-specific dependency on the historical artifact.

Explicit exclusions from removal:

- `Install/Shared/Recovery/InstallRecoveryCatalogueReader.cs:30`, `:55`, `:63`, `:68` retain unverified, unavailable-comparison and mixed-state checks, while treating third-state evidence as history.
- `Install/Shared/Planning/InstallPlanningInspector.cs:136` and `Install/Shared/Operation/InstallApplicationPreconditionValidator.cs:92` consume that filtered blocking catalogue. Keep them.
- Exact current-operation identity checks in Index, Repair, top-level Update, Library and Extension remain.
- Cleanup’s candidate integrity, frozen membership and deletion-authority checks remain.
- Draft filename collisions and final semantic readback remain failures.

## Waves and packets

Every packet owns its corresponding tests and snapshots unless explicitly assigned elsewhere. Each handoff includes changed paths, build result, focused execution receipts or explicit overseer handoff, and remaining risks. Workers do not expand ownership silently.

### Wave 1 — A: Runtime and composite lease

**Owner:** foundation owner. **Size:** 2–3 engineer-days.

**Files:** `F/Mutation/Locking/**`, new runtime paths file, Framework project’s interop setting, new Operations failure projection files, new recovery location model and locator files. Tests under `U/Framework/Mutation/Locking`, `I/Framework/Mutation/Locking`, and `I/TestSupport/WorkspaceLockTestStore.cs`.

Implement the frozen path, bootstrap, typed errors, composite lease, bridge and native validation. Keep existing writer call signatures usable through result projections.

**Focused commands:** run U `*WorkspaceLockContractTests`, I `*WorkspaceLockManagerIntegrationTests` and `*WorkspaceLockContractIntegrationTests`. Add focused `*LegacyWorkspaceLockBridgeIntegrationTests`.

**Acceptance:** same physical workspace contends despite different data homes, distinct worktrees do not. Nonempty local sentinel survives byte-for-byte. Cancellation and every bridge failure release local. Both legacy access directions exclude each other. Test denied-existing, denied-absent, unavailable resolution, busy, unsafe targets and concurrent first initialization. Linux and macOS checked-lock and special-file tests gate foundation acceptance.

### Wave 2 — B and C may run in parallel

#### B: Local recovery and recorded deletion authority

**Owner:** same foundation owner. **Size:** 2–3 days.

**Files:** `F/Recovery/**`, `S/Isolation/TestDataHome.cs`, recovery fixtures and tests. Mechanical consumer changes only in `O/Cleanup/CleanupOperation.cs`, `Cleanup/Shared/Application/CleanupApplicationOperation.cs`, and `Repair/Shared/Application/RepairLibraryRecoveryApplication.cs`. Also own `Install/Shared/Recovery/InstallRecoveryCatalogueReader.cs`.

Implement all frozen recovery signatures. Preserve the format, attribution, draft-to-final verification and deletion guards. Extend the shared test oracle with separate local and legacy paths rather than silently redefining legacy helpers.

Install reads both available stores through its existing state filter. Legacy coverage failure does not veto local work. Preserve all actual state-based boundaries.

**Focused commands:** I `*RecoveryBundle*IntegrationTests`, `*RecoveryDeletionStorageBoundaryIntegrationTests`, `*LibrarySelectedFinalReadIntegrationTests`, and affected Install recovery tests.

**Acceptance:** every producer writes locally. Draft and final identity validates in both stores. Identical operation IDs in different stores remain distinct. Environment changes after observation cannot redirect deletion. Legacy deletion without matching legacy authority refuses only that deletion. Absent-store reads create nothing. No new archive schema or retention policy.

#### C: Reserve the runtime subtree

**Owner:** reservation worker. **Size:** 1.5–2 days.

**Files:**

- `F/Sources/Inventory/SourceCatalogueReader.cs`
- `F/Sources/Reading/SourceDocumentReader.cs`
- `O/Route/Shared/References/RouteMarkdownCatalogueReader.Traversal.cs`
- `O/Route/Shared/Inclusion/Planning/RouteInclusionTargetResolver.cs`
- `O/Extension/Shared/Permissions/ExtensionDestinationPolicy.cs`
- `O/Library/Shared/Planning/LibraryDestinationPolicy.cs`
- `O/Remove/Shared/Selection/RemoveTargetResolver.cs`
- `O/Remove/Shared/Planning/RemovePathInventory.cs`
- Route Create’s `RouteCreateTargetInspector.cs`, Init’s `RouteInitTargetPlanner.cs`
- Route Move’s `RouteMoveSubjectSelector.Boundaries.cs`, `RouteMoveDestinationResolver.cs`, `RouteMoveCategoryInventoryReader.cs`
- Route Remove’s `RouteRemoveSubjectSelector` partial files and `RouteRemoveCategoryInventoryReader.cs`

Reject explicit reserved targets and skip traversal before reading content. Apply lexical and physical checks. Keep recovery observers as deliberate consumers of their own store.

**Focused commands:** U `*ExtensionDestinationPolicyTests`, `*LibraryDestinationPolicyTests`, I `*SourceCatalogueReaderIntegrationTests`. Add I `*WorkspaceRuntimeReservationIntegrationTests`.

**Acceptance:** Index, source inclusion and Markdown reference scans ignore runtime content even with missing ignore rules. Direct and linked aliases cannot admit it. Package destinations and removal selectors refuse it. Framework Remove preserves runtime recovery.

### Wave 3 — D: Typed command failures

**Size:** 1.5–2 days.

**Files:** the existing `AcquireAsync` consumers:

- `Index/Shared/Operation/IndexApplicationOperation.cs`
- `Install/Shared/Operation/InstallApplicationOperation.cs`
- `Library/{Attach/LibraryAttachOperation,Sync/LibrarySyncOperation,Detach/LibraryDetachOperation}.cs`
- `Repair/Shared/Application/{RepairApplicationOperation,RepairLibraryRecoveryApplication}.cs`
- `Update/Shared/Application/UpdateApplicationOperation.cs`
- `Remove/Shared/Application/RemovePathApplication.cs`
- `Cleanup/Shared/Application/CleanupApplicationOperation.cs`
- `Route/Shared/Inclusion/Application/RouteInclusionApplication.cs`
- `Route/{Create,Init,Update,Remove}/Shared/Application/Route{Create,Init,Update,Remove}ApplicationOperation.cs`
- `Route/Move/Shared/Application/RouteMoveApplicationOperation.Lease.cs`
- `Extension/{Install,Update,Remove}/Shared/Application/Extension{Install,Update,Remove}ApplicationOperation.cs`

Also own their finding models and lock-related rendering branches, `R/Shared/Wording/CliFindingWording.cs`, `T/Shared/{SharedText,CliFindingWording}.cs`, and Cleanup’s separate lock wording.

Implement common history wording now for the next wave. Later packets consume it without editing shared wording.

**Focused commands:** new U `*WorkspaceLockFailurePresentationTests`, plus affected I `*BeforeOutputSnapshotTests`.

**Acceptance:** a six-cause matrix across command families preserves existing code/status/exit mappings. Exact paths survive every detail level and JSON projection. Access denial and unsupported locking never claim contention. Extension Create remains outside workspace mutation locking.

### Wave 4 — E, F and G may run in parallel

#### E: Route history becomes warnings

**Size:** 1.5–2.5 days.

**Files:** the seven Route audit files, their command-local preparation/result models and result builders, and corresponding Route wording, report selectors and serialization projections. No shared Framework or common wording edits.

Remove historical vetoes, preserve the replacement checks listed in the audit, and keep historical observations separate from the current operation’s recovery outcome.

**Focused commands:** I `*Route*RecoveryConflictCoordinateIntegrationTests`, `*RouteRemovePriorRecoveryIntegrationTests`, new `*RouteInclusionRecoveryHistoryIntegrationTests`. U `*Route*RecoveryConflictPresentationTests`.

**Acceptance:** each command proceeds with unrelated final, draft and damaged historical artifacts. Changed current targets still stop before effects. Prior artifacts remain byte-identical. Warnings identify all retained paths, including on successful no-op results.

#### F: Extension history becomes warnings

**Size:** 1–1.5 days.

**Files:** the three Extension audit files, their local finding/result models, planners’ result construction, and corresponding Extension rendering and serialization projections.

Remove historical integrity predicates and historical coverage vetoes. Keep exact own-preparation validation, ownership, dependency, target and plan checks.

**Focused commands:** I `*ExtensionInstallRecoveryHistoryIntegrationTests`, `*ExtensionUpdateRecoveryHistoryIntegrationTests`, `*ExtensionRemovePriorRecoveryIntegrationTests`.

**Acceptance:** drafts and damaged history no longer block unrelated install, update or remove. A missing or changed current-operation preparation still fails. Verified no-ops create no runtime state and can report history.

#### G: Per-store observers and Cleanup

**Size:** 1.5–2.5 days.

**Files:** recovery-facing files under `O/{Status,Doctor,Cleanup}`, their `R` presentation directories and `T` wording directories. In particular, own `StatusRecoveryModels.cs`, `StatusRecoveryAggregator.cs`, `StatusRecoveryFindingAggregator.cs`, `CleanupOperation.cs`, `CleanupPlan.cs`, `CleanupPlanner.cs`, `CleanupApplicationOperation.cs` and `CleanupResultBuilder.cs`. Foundation contributor APIs remain frozen.

Status and Doctor consume both store observations. Expose ordered store records with kind, path, coverage, candidates and cause. Preserve existing aggregate fields, retaining unknown totals where coverage is incomplete.

Cleanup forms and validates one frozen plan per store. Replace the current global all-safe veto with store-specific safety. Local eligible deletion can complete while legacy coverage or authority is unavailable. Report partial results truthfully.

**Focused commands:** I `*StatusBeforeOutputSnapshotTests`, `*StatusOperationLifecycleSnapshotIntegrationTests`, `*CleanupBeforeOutputSnapshotTests`, U `*DoctorBeforeOutputSnapshotTests`. Add I `*RecoveryStoreCoverageIntegrationTests`.

**Acceptance:** observers create neither store and never probe lock writability. Both readable stores are reported and cleaned. Unreadable legacy never disables local cleanup. Legacy deletion requires the bridge handle. Unsafe items remain retained. Doctor keeps its existing producer-specific comparison scope.

### Wave 5 — H: Process journeys and platform qualification

**Owner:** overseer with a bounded test implementer. **Size:** 2–3 days.

**Files:** `E/Journeys/F20WorkspaceLockJourneyTests.cs`, `F23RecoveryCleanupJourneyTests.cs`, `RootRemoveRecoveryConsumerJourneyTests.cs`, `E/Published{Status,Doctor,Cleanup}ProcessTests.cs`, `E/Shared/PublishedProcess/PublishedWorkspaceLockStore.cs`, `E/Shared/Journeys/PublishedLockHolder.cs`, and new journey files below.

Replace the Windows-only holder with a test-owned child protocol in the existing test executable. Own `E/TestProcessIsolation.cs` and its project settings if needed for that entry seam. Keep exactly three runnable test projects. Add no production test flag or runtime dependency.

**Focused commands:** E class filters for these existing journeys and:

- `*WorkspaceLockBridgeJourneyTests`
- `*WorkspaceRuntimeIsolationJourneyTests`
- `*RecoveryResidualContinuationJourneyTests`

**Acceptance journeys on Windows, Linux and macOS:**

1. F20 live holder blocks its workspace, permits another worktree, releases normally, and releases after process kill.
2. Local contention works across different legacy data homes.
3. Legacy read-write and read-only holders exclude new writers in both directions. Legacy busy releases local.
4. Nonempty local sentinel is preserved.
5. First Install creates runtime ignore before recovery or sentinel files and does not disturb `.agents` planning.
6. Dry runs, prompts, refusals, verified no-ops and observers create nothing.
7. Local and legacy residuals appear separately, local cleanup works with unavailable legacy, and root Remove retains recovery.
8. Route and Extension continuation succeeds with unrelated leftovers while current-state corruption still stops.

Add Windows sandbox qualification for denied-existing and denied-absent legacy cases. On Unix, include FIFO, socket, symlink and unsupported-lock outcomes. Cross-check the legacy protocol with a pinned beta.17 executable in overseer qualification.

Run complete managed acceptance and the repository’s supported Native AOT gate after integration. Publish and execute supported x64 and arm64 targets across Windows, Linux and macOS. Record actual platform coverage and skips separately.

### Wave 6 — I: Contracts and documentation

**Size:** 1–1.5 days. One final documentation owner.

Update these existing files, using brace expansion for the named alternatives:

- `C/architecture.md`, `C/layers/{framework,operations}.md`
- `C/technical-designs/mutation-and-recovery.md`
- `C/shared-operation-contract.md`
- `C/contracts/{status,doctor,cleanup,install,update,repair,remove}/{interface,behavior}.md`
- `C/contracts/index-candidate/{interface,behavior,technical-design}.md`
- `C/contracts/route/{include,exclude,create,init,update,move,remove}/{interface,behavior}.md`
- `C/contracts/route/update/technical-design.md`
- `C/contracts/extension/{install,update,remove}/{interface,behavior}.md`
- `C/contracts/extension/create/behavior.md`, preserving its lock-free destination semantics
- `C/contracts/library/{attach,sync,detach}/{interface,behavior}.md`
- `C/contracts/shared/{source-universe-filters,source-references,workspace-permissions}/{interface,behavior}.md`
- `C/contracts/{find,context,references-candidate}/behavior.md`
- `C/contracts/help/interface.md`
- `C/experience/flows/f20-run-concurrent-work-without-misleading-lock-advice.md`
- `C/experience/flows/f23-review-and-remove-recovery-leftovers.md`
- `C/experience/scenarios/experience.md` and affected command scenarios for Index, Repair, Cleanup, Route, Extension and Library
- `docs/cli.md`
- `src/docusaurus/docs/cli/{index,flows}.md`
- `src/docusaurus/docs/glossary.md`
- `src/docusaurus/docs/concepts/customizing.md`
- Both installed and shipped `open-forge-cli/SKILL.md` and `references/{common,routes,packages}.md` under `.agents/skills` and `src/open-forge/.agents/skills`

Update affected command help literals through their existing OutputText owners, especially Cleanup and Status. Review generated help snapshots explicitly. Rewrite PR #7’s data-folder explanation to the final design rather than applying its historical wording wholesale.

Document fixed local paths, independent worktrees, sandbox behavior, persistent sentinel ownership, local recovery, per-store coverage, `git clean -x` and `-X`, legacy-only meaning of `OPENFORGE_DATA_HOME`, and bridge removal at 1.0. State the accepted first-write mixed-version gap and that older readers may miss new local evidence.

**Focused checks:** documentation link checks and E help-process tests, plus affected help snapshots. Protected `.agents` edits that the worker cannot perform are handed to the overseer as a concrete patch.

## Snapshot changes

Update only reviewed affected cases:

- Lock failures in command `BeforeOutputSnapshotTests`, plus `InstallChangeOutputSnapshotTests` and Route Include/Exclude output snapshots.
- Route recovery-conflict presentation and coordinate groups become historical-warning cases. Preserve separate current-operation conflict coverage.
- Extension recovery-history and prior-recovery groups gain warning outcomes.
- Status lifecycle/recovery, Doctor recovery and Cleanup groups gain per-store coverage and partial-cleanup cases.
- Published journeys and help snapshots reflect local runtime paths.

Normalize only owned temporary roots. Never normalize away store identity, exact artifact paths, causes, warning severity or result coordinates.

## Integration risks and early detection

- **Native ownership incorrectly granted.** Gate packet A on real cross-process exclusion, special-file rejection and Native AOT execution before command work depends on it.
- **Location reconstructed during cleanup.** Test environment changes after observation and same-ID candidates in both stores. Search deletion code for residual `ResolveStoreRoot` calls.
- **Legacy failure still globally blocks work.** Exercise inaccessible legacy storage through Install, each D5 command and Cleanup, not just the catalogue.
- **History confused with current recovery.** Assert current preparation identity and historical byte preservation separately.
- **Reservation bypass through explicit roots or aliases.** Test direct reads, included entries, linked destinations and removal inventories, with `.gitignore` absent.
- **Partial cleanup rendered as no changes.** Assert filesystem effects alongside status, counts, text and JSON.
- **Fixture drift hides the migration.** Keep distinct local and legacy test oracles. Remove obsolete local zero-byte assumptions without weakening legacy fixtures.
- **Stale publish or skipped Unix tests produces false confidence.** Bind receipts to the integrated tree and fresh executable, with nonzero executed counts and explicit platform results.

Budget approximately **14–22 engineer-days**, plus overseer platform and sandbox qualification. Parallelism shortens the calendar path, but the foundation, failure propagation and final qualification remain sequential dependencies.
