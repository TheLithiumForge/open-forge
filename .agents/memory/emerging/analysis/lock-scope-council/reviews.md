---
open-forge:
  description: The seven Stage 2 peer reviews and rankings of the lock scope council
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Council, Locking, Recovery]
---

# Lock Scope Council Reviews

Each member reviewed all seven proposals, which were labelled P1 to P7 without lenses. The [record](record.md) holds the averaged rankings and the decisions.

## P1: Migration and contract guardian

/root - GPT-6 - high

#### Rankings

Each column ranks all proposals independently, with 1 best.

| Proposal | LOCATION | SCOPE | NEVER-STUCK | MIGRATION | PLAN |
|---|---:|---:|---:|---:|---:|
| P1 | 7 | 5 | 6 | 2 | 4 |
| P2 | 1 | 3 | 2 | 3 | 2 |
| P3 | 2 | 2 | 1 | 4 | 3 |
| P4 | 6 | 7 | 7 | 7 | 7 |
| P5 | 3 | 6 | 3 | 5 | 5 |
| P6 | 5 | 1 | 5 | 6 | 6 |
| P7 | 4 | 4 | 4 | 1 | 1 |

P2 best addresses the actual sandbox boundary and ineffective Unix locking. P6 gives the most precise account of read dependencies and write sets. P3 identifies existing recovery artifacts as another source of persistent blocking. P7 provides the strongest migration design because it preserves recovery visibility for older commands and follows store identity through deletion.

P1 remains strong on explicit contracts and cutover rules, but my original location choice and recovery migration were incomplete. Scope rankings are close because every proposal ultimately retains one workspace lease. P4 adds substantial recovery-policy changes without fully resolving their interaction with existing blockers.

#### Errors

Paths below use these prefixes:

- `F` = `src/cli/framework/OpenForge.Cli.Framework/Framework`
- `O` = `src/cli/operations/OpenForge.Cli.Operations/Commands`
- `D` = `.agents/memory/crystallized/documents/cli`

**P4’s claim that retained recovery evidence does not block another command contradicts current callers.** Route Move rejects any catalogue candidate when recovery is required at `O/Route/Move/Shared/Application/RouteMoveRecoveryLifecycle.Preparation.cs:34`. Include and Exclude do likewise at `O/Route/Shared/Inclusion/Application/RouteInclusionApplication.cs:58`. Retaining every successful final bundle would therefore block subsequent operations unless those policies change. P4 mentions lifecycle updates, but does not specify this necessary semantic change.

**P1’s migration design preserves mutual exclusion but misses legacy recovery visibility.** Its default local bundles would be invisible to unchanged older commands. The catalogue resolves one data-home store at `F/Recovery/RecoveryBundleCatalogue.cs:22`, through `F/Recovery/Shared/Storage/RecoveryBundlePathIdentity.cs:9`. This matters because recovery observations affect mutation admission, not merely reporting. P7 correctly keeps bridge-mode recovery in the legacy store. This is an interoperability omission in P1, not a failure of its dual-lock exclusion proof.

**Local recovery cannot be implemented by changing preparation and catalogue paths alone.** Deletion independently reconstructs the current data-home directory at `F/Recovery/Shared/Deletion/RecoveryBundleDeletionValidation.cs:62`. Any implementation that overlooks this will reject valid local candidates or apply the wrong storage boundary. P7 identifies this dependency explicitly.

**Changing the manager’s nonempty-file check alone is insufficient.** The lease constructor independently rejects nonzero length at `F/Mutation/Locking/Models/WorkspaceLockLease.cs:19`. Proposals adopting content-agnostic sentinels must change both checks. P7 explicitly accounts for both. P1’s retained zero-byte restriction is internally consistent, but unnecessarily requires repair of an otherwise ordinary sentinel.

Two external corrections also affect the rankings:

- P1, P4, P5, P6 and P7 cannot treat placement beneath `.agents` as a general Codex sandbox solution. Existing `.agents` directories are recursively protected under the documented default workspace-write policy. P2’s root-level runtime directory avoids that coordination obstacle. Actual settings and ownership writes still require permission. [OpenAI sandbox documentation](https://learn.chatgpt.com/docs/agent-approvals-security)
- Successful `FileStream` construction does not establish effective exclusion on every Unix filesystem. The inspected .NET implementation ignores several locking errors. P2 and P5 correctly require additional evidence. This is stronger than merely checking whether the handle remains open. [Pinned .NET runtime implementation](https://raw.githubusercontent.com/dotnet/runtime/v10.0.0/src/libraries/System.Private.CoreLib/src/Microsoft/Win32/SafeHandles/SafeFileHandle.Unix.cs)

These findings come from source review. I did not execute tests or qualify filesystem combinations.

#### Best combination

**Location and ownership from P2, with P7’s complete validation changes.** Use a fixed `.open-forge-state/workspace.lock` beneath the selected workspace root. Keep it independent of the configurable knowledge root, Git administration, usernames and mount-path hashes. Ownership comes from the held handle. Preserve ordinary sentinel contents without interpreting or truncating them. Reject unsafe filesystem objects.

Use P2’s second independent exclusive-open check before granting a lease. Only a recognized contention result establishes local enforcement. An unrelated error remains unverified. This does not qualify cross-host locking, Windows/WSL interoperability or network-server failover.

**Scope from P6 and P3.** Hold one workspace lease through complete revalidation, recovery preparation, application, verification and operation-owned recovery deletion. Plan and prompt beforehand. Revalidate inputs that determine the write set, including reference discovery and control documents. Keep Cleanup’s catalogue validation and deletion under the lease.

Separate worktrees remain independent when their effects do not overlap. Distinct selected roots do not make overlapping parent and child operations safe. Document that restriction and require common coordination or external serialization. Do not introduce folder-lock scheduling now.

**Migration from P7, governed by P1’s explicit workspace cutover.**

| Workspace mode | Required locks | New recovery destination |
|---|---|---|
| Compatibility, default when absent | Local, then exact legacy lock | Existing legacy store |
| Workspace, explicitly selected after cutover | Local lock | Local store by default, explicit external override allowed |

All new binaries use one acquisition order and release every acquired handle on failure. Compatibility requires the same legacy data-home and workspace-identity domain as old writers. Preserve the legacy filename algorithm exactly.

An inaccessible legacy lock must block compatibility-mode mutation. Never interpret denial, timeout or contention as permission to skip it. Cutover requires stopping old writers, reviewing residuals, pinning supported binaries and explicitly selecting workspace mode. A settings field cannot fence old executables. Their reader accepts unfamiliar integer schema versions at `F/Settings/Shared/Serialization/WorkspaceSettingsCodec.cs:274`.

Leave existing `locks/v1` and `recovery/v1` data in place. Preserve archive schema and attribution. Do not bulk-migrate, rewrite or delete historical artifacts. Returning to older binaries requires quiescence and explicit reconciliation of local recovery evidence that they cannot discover.

**Recovery liveness from P3, bounded by P7’s current-state safeguards.** Audit every recovery-dependent mutation gate. Replace blanket candidate-presence vetoes with explicit rules based on relevant current state. An unrelated draft or historical bundle must not become a workspace-wide prohibition.

Do not simply remove every recovery guard. Install already distinguishes mixed prior/intended state from historical snapshots at `O/Install/Shared/Recovery/InstallRecoveryCatalogueReader.cs:52`. Preserve necessary guards and provide exact restoration or reconciliation guidance. Deleting the only recovery evidence must not be the default instruction for unblocking work.

Use P7’s explicit recovery-store identity throughout preparation, observation, revalidation and deletion. Status and Doctor should inspect local and known legacy stores without creating them, even when settings or ownership cannot be parsed. Report unavailable coverage separately from an empty store. Cleanup should default to the active store and require explicit legacy selection. Legacy deletion requires both locks.

**Contract discipline from P1 and implementation boundaries from P7.** Before implementation, revise:

- Shared mutation contracts for local coordination, compatibility mode, recovery location and residual admission rules.
- The mutation-and-recovery design for composite leases, enforcement checks, typed store identity and deletion boundaries.
- Install contracts for runtime scaffolding, ignore handling and bootstrap ordering.
- Cleanup, Status and Doctor contracts for store selection, incomplete observation and recovery guidance.
- Settings and output specifications, preserving existing machine-readable coordinates where their meanings remain valid.

Local bundles explicitly change the outside-workspace requirement at `D/shared-operation-contract.md:713`. Bootstrap changes must address `D/contracts/install/interface.md:536`. Adding runtime patterns to Install’s managed ignore section requires changing its closed parser at `O/Install/Shared/Configuration/InstallIgnoreSection.cs:26`.

Keep existing successful-operation bundle deletion for this change. P4’s indefinite retention, expanded recovery coverage and forced flushing deserve separate decisions. Retention currently conflicts with `D/shared-operation-contract.md:736`.

Reserve runtime storage from scans, package destinations, removal and main-root migration. Establish ignore protection before storing user bytes. Document that forced Git cleaning and workspace deletion can still destroy local recovery.

Use five implementation packets: contracts and protocol, lock infrastructure, recovery stores and gates, command/reporting integration, then qualification and documentation. Budget roughly **10–15 engineering days**, excluding qualification of additional shared-filesystem topologies.

Release gates should cover actual old/new binaries in both acquisition directions, killed applying processes at recovery and effect boundaries, denied legacy and local paths, disabled locking, nonempty sentinels, malformed controls, legacy cleanup and preserved user bytes. Run process evidence across supported operating systems and published NativeAOT artifacts.

Release notes must explain compatibility defaults, the remaining legacy-access requirement, explicit cutover, unsupported old writers afterward, downgrade limitations, local-backup exposure and the distinction between lock release and crash recovery.

#### What changed my mind

I would move runtime state outside `.agents`. I now consider recovery visibility part of mixed-version compatibility, alongside mutual exclusion. I also recognize that leftover recovery artifacts already block commands, so relocating storage alone cannot deliver the intended liveness improvement.

P2 changed my confidence in successful Unix opens. P7 changed my migration design. P3 changed the minimum scope of the recovery audit.

## P2: OS locking engineer

/root - GPT-6 - high

#### Rankings

Each column ranks all seven proposals independently. **1 is best.** Lower ranks often reflect missing guarantees rather than disagreement about workspace granularity.

| Proposal | LOCATION | SCOPE | NEVER-STUCK | MIGRATION | PLAN |
|---|---:|---:|---:|---:|---:|
| P1 | 7 | 5 | 6 | 2 | 3 |
| **P2, mine** | **1** | **4** | **3** | **4** | **4** |
| P3 | 2 | 2 | 1 | 3 | 2 |
| P4 | 5 | 6 | 7 | 6 | 7 |
| P5 | 3 | 7 | 4 | 7 | 6 |
| P6 | 6 | 1 | 5 | 5 | 5 |
| P7 | 4 | 3 | 2 | 1 | 1 |

**LOCATION:** P2 combines a root-level runtime directory with the strongest platform analysis. P3 also avoids `.agents`. That matters because Codex’s default workspace-write policy recursively protects an existing `.agents` directory. P5’s general sandbox claim is consequently too broad. Moving coordination outside `.agents` still cannot authorize protected target writes. [Codex security documentation](https://learn.chatgpt.com/docs/agent-approvals-security)

**SCOPE:** P6 best distinguishes shared read dependencies from writes. P3 best explains why shortening the lease also changes recovery and Cleanup synchronization. All seven reasonably retain workspace serialization.

**NEVER-STUCK:** P3 identifies and proposes changing an existing artifact-based veto. P7 identifies it but retains it. P2 addresses stale sentinel contents and silent locking failures, but misses the recovery veto. P4’s additional retention policy would aggravate that veto without corresponding changes.

**MIGRATION:** P7 preserves both exclusion and old-reader recovery visibility. P1 provides the clearest explicit cutover and historical-cleanup rules.

**PLAN:** P7 identifies the necessary storage, deletion, command and platform integration points. P3 explicitly budgets removal of blanket recovery barriers.

#### Errors

Paths below use these prefixes:

- `F` = `src/cli/framework/OpenForge.Cli.Framework/Framework`
- `O` = `src/cli/operations/OpenForge.Cli.Operations/Commands`

Some findings are omissions that invalidate promised behavior, rather than incorrect descriptions of current code.

1. **P4’s retained-bundle behavior conflicts with unchanged command gates.** P4 says retained recovery evidence does not block another command and proposes retaining every verified final. Route Include and Exclude currently stop on any catalogue candidate at `O/Route/Shared/Inclusion/Application/RouteInclusionApplication.cs:58`. Route Move does likewise when recovery is required at `O/Route/Move/Shared/Application/RouteMoveRecoveryLifecycle.Preparation.cs:34`. P4 must change these gates before making that promise. P2 and P6 also miss these existing barriers when discussing fresh planning without a persistent recovery latch. P3 correctly identifies the required change.

2. **P3’s “no stale-lock cleanup problem” needs qualification.** P3 retains the existing zero-byte mechanism. The manager rejects nonempty files at `F/Mutation/Locking/WorkspaceLockManager.cs:198`, and the lease independently rejects them at `F/Mutation/Locking/Models/WorkspaceLockLease.cs:19`. An ordinary abandoned file containing bytes therefore remains a permanent obstruction until someone intervenes. P1 explicitly accepts this obstruction. P2, P4, P5, P6 and P7 correctly propose removing it for the new sentinel.

3. **Dual locking alone does not preserve recovery compatibility.** P1 through P6 do not require legacy-visible recovery storage throughout bridge mode. The old catalogue resolves only the current external store and its workspace bucket at `F/Recovery/RecoveryBundleCatalogue.cs:22` and `:29`. A new writer can hold both locks, leave an interrupted operation’s bundle locally, and then release both locks. An old invocation subsequently sees no such bundle. Mutual exclusion remains correct, but existing recovery checks lose evidence. P7’s explicit legacy recovery destination fixes this gap.

4. **P2 and P5 overstate what the second-open probe establishes.** It detects important persistent failures, but does not prove that the first handle owns the lock. Inference from the runtime: the first `flock` could fail with an ignored error, another writer could acquire the lock, and the probe could then receive contention. P2 would incorrectly issue a lease. The current manager already treats a successful managed open as acquisition at `F/Mutation/Locking/WorkspaceLockManager.cs:102` and `:146`. The [.NET 10 implementation](https://raw.githubusercontent.com/dotnet/runtime/v10.0.0/src/libraries/System.Private.CoreLib/src/Microsoft/Win32/SafeHandles/SafeFileHandle.Unix.cs) ignores locking errors other than `EWOULDBLOCK`. The probe is useful qualification evidence, not an authoritative acquisition result.

Two corrections in other proposals survived verification. P6 and P7 correctly reject the brief’s uniform failure-mapping claim. Install maps acquisition failure to `WorkspaceUnsafe` at `O/Install/Shared/Operation/InstallApplicationOperation.cs:68`. Index distinguishes invalid paths at `O/Index/Shared/Operation/IndexMutationMapper.cs:47`. P6 also correctly identifies settings and ownership as Index read dependencies at `O/Index/Shared/Projection/IndexProjectionReader.cs:153` and `:161`.

#### Best combination

**Location from P2 and P3, with stronger Unix acquisition.** Ship one fixed `<workspace>/.open-forge-state/workspace.lock`. Never relocate it through configuration, hash its filename by mount spelling, truncate it, replace it or delete it during normal operation. Accept existing ordinary-file contents. Remove both zero-length checks.

On Windows, retain `FileShare.None`. On Linux and macOS, add a small checked `flock` adapter and grant the lease only after successful exclusive, nonblocking acquisition. Handle interruption and classify every other failure. Do not infer ownership from a second open. This uses system libraries through supported [NativeAOT interop](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/interop), without a daemon or separately installed runtime.

**Platform boundaries from P2.** Windows sharing restrictions protect the sentinel, not workspace targets. Local Unix locking remains advisory. Native Linux bind mounts coordinate when they expose the same backing file and lock domain. A private runtime overlay defeats that guarantee. Network mounts, Docker Desktop bridges and WSL `/mnt` access require separate qualification.

A checked syscall still cannot establish remote fencing. NFS can be configured for client-local locking. Linux SMB propagation and mandatory-lock effects depend on kernel version, mount options and server behavior. Two clients can therefore proceed despite a successful local acquisition. [Linux locking semantics](https://man7.org/linux/man-pages/man2/flock.2.html)

Agent names are not permission guarantees. Codex has protected workspace subtrees. Claude Code’s shell sandbox has configurable write boundaries and protected paths. Other sandboxes must be assessed from their actual mounts and permissions. [Claude Code sandbox documentation](https://code.claude.com/docs/en/sandboxing)

**Scope from P6 and P3.** Keep one workspace lease across input revalidation, recovery preparation, all effects, verification and recovery deletion. Preserve complete Route Move replanning under the lease. Do not substitute output hashes for reference-discovery dependencies or directory membership.

Keep planning and consent outside the lease. Measure contention before introducing scoped Index execution. Independent worktrees provide concurrency only when their mutation targets are disjoint.

**Recovery and liveness from P3, P7 and P5.** Replace blanket candidate-existence vetoes with operation-specific validation of relevant current state. Preserve genuine safety boundaries. Install already distinguishes mixed prior/intended states from historical bundles at `O/Install/Shared/Recovery/InstallRecoveryCatalogueReader.cs:52`. Removing every recovery check would discard useful protection.

Use P7’s typed recovery location throughout writing, observation and deletion. The deletion guard currently reconstructs the external bucket at `F/Recovery/Shared/Deletion/RecoveryBundleDeletionValidation.cs:66`, so changing only the writer’s path is insufficient.

After cutover, default recovery to the reserved local directory. Preserve v1 attribution and report foreign workspace identities without silently adopting them. Keep explicit external storage through `OPENFORGE_DATA_HOME`, with actionable errors for invalid selections.

Create the runtime-local ignore file before payloads, exclude runtime state from all content scans, and explain that forced Git cleaning can still delete recovery data. Cleanup must say that it deletes evidence, not repairs files. Defer P4’s broader retention and durability changes.

**Migration from P7 and P1.** Default existing workspaces to compatibility mode. Acquire local then exact legacy lock, hold both throughout application, and write recovery bundles where old readers can see them. Failure to acquire either releases the other before target effects.

Persist workspace-only cutover explicitly after old writers are stopped and launchers are pinned. Read that policy under the local lease. A marker cannot fence an old binary that ignores it. Never infer cutover from access denial.

Leave legacy files in place. Explicit legacy cleanup acquires both relevant locks. Observers report active and legacy coverage separately. Default Cleanup to the active store rather than blindly deleting across every discovered location.

**Delivery from P7, supplemented by P1 and P3.** Use five packets: lock foundation, recovery locations, runtime protection, command/reporting integration, and qualification/contracts. Add the checked Unix adapter and residual-gate changes explicitly. Preserve truthful reporting when bootstrap creates infrastructure.

Qualification must include real competing processes, interrupted application, legacy-reader visibility, failed Unix locking, protected `.agents`, shared bind mounts, exact deletion guards and all six NativeAOT targets. Keep remote filesystems outside the supported concurrency claim until tested. I would provision approximately **12–17 engineering days**, subject to the residual-policy changes.

#### What changed my mind

P3 showed that existing recovery artifacts already create the lockout I only warned against introducing.

P7 showed that mixed-version safety includes recovery visibility, not just mutex overlap.

P6 made the future case for scoped Index execution more credible, while strengthening the case against implementing it now.

I also withdraw P2’s claim that a second-open probe proves ownership. A checked Unix acquisition result is required.

## P3: Contrarian minimalist

/root - GPT-6 - reasoning level unreported

#### Rankings

Each column ranks all seven proposals independently, with **1 best**. SCOPE is close because everyone ultimately retains the workspace lease.

| Proposal | LOCATION | SCOPE | NEVER-STUCK | MIGRATION | PLAN |
|---|---:|---:|---:|---:|---:|
| P1 | 4 | 5 | 6 | 2 | 2 |
| P2 | 1 | 4 | 1 | 4 | 3 |
| **P3, mine** | **2** | **2** | **7** | **3** | **6** |
| P4 | 7 | 7 | 3 | 7 | 7 |
| P5 | 6 | 6 | 5 | 6 | 4 |
| P6 | 5 | 1 | 4 | 5 | 5 |
| P7 | 3 | 3 | 2 | 1 | 1 |

- **LOCATION:** P2 gives the strongest reason for placing runtime state outside `.agents`. Codex’s documented default protection covers existing `.agents` directories recursively. Workspace write access therefore does not necessarily permit the locations proposed by P1, P4, P5, P6, and P7. Moving runtime state also cannot grant permission to mutate protected settings or content. [Official OpenAI documentation](https://learn.chatgpt.com/docs/agent-approvals-security#protected-paths-in-writable-roots)
- **SCOPE:** P6 most carefully distinguishes shared reads from shared writes. Index records settings and ownership expectations, then expands maintenance selection into parents and siblings. That supports keeping serialization without pretending every command rewrites both controls. `src/cli/operations/OpenForge.Cli.Operations/Commands/Index/Shared/Projection/IndexProjectionReader.cs:153`, `:161`, `:178`.
- **NEVER-STUCK:** P2 addresses ineffective locking as well as leftover files. P7 preserves the distinction between mutex ownership and recovery conflicts. My P3 adds broad residual-policy changes without enough command-specific analysis.
- **MIGRATION:** P7 uniquely makes bridge-mode recovery storage explicit. Old binaries must see recovery evidence, not merely contend on a mutex.
- **PLAN:** P7 identifies independently reconstructed recovery paths and deletion guards. P1 handles bootstrap and contract changes thoroughly. P4 combines the location fix with retention, durability, and recovery-coverage expansion, making it the least minimal delivery.

#### Errors

For compact citations, these prefixes expand exactly:

- `F` = `src/cli/framework/OpenForge.Cli.Framework/Framework`
- `O` = `src/cli/operations/OpenForge.Cli.Operations/Commands`

The principal problems are compatibility gaps and promises requiring additional changes, rather than widespread factual misreading.

**R1 — P3’s ordinary-file reuse promise is too broad.**  
I retained zero-byte validation while saying an ordinary leftover file would reopen normally. The manager rejects nonempty ordinary files, and the lease constructor independently rejects them. P2, P4, P5, P6, and P7 explicitly address this. The smallest correction is to remove content-based rejection for the new local mutex while preserving legacy behavior.

`F/Mutation/Locking/WorkspaceLockManager.cs:198`  
`F/Mutation/Locking/Models/WorkspaceLockLease.cs:19`

**R2 — P1/P2/P3/P4/P5 miss an old-reader consequence of extending Install’s ignore section.**  
Adding a runtime exclusion to that section can break older Install invocations. Its parser accepts only recognized route patterns and throws on anything else. Updating the new reader does not update an old executable. The proposed dual-lock bridge therefore does not provide unrestricted behavioral compatibility if it also writes this new pattern.

Use the runtime-local ignore file during the transition. Defer changing the older owned section.

`O/Install/Shared/Configuration/InstallIgnoreSection.cs:26`  
`O/Install/Shared/Configuration/InstallIgnoreSection.cs:46`

**R3 — Local recovery during bridge mode leaves old binaries blind.**  
P1, P2, P4, P5, and P6 default new recovery to local storage without explicitly preserving legacy recovery visibility throughout bridging. P3’s permission-triggered fallback can produce the same problem. Dual locking still provides exclusion, but it does not preserve recovery-dependent behavior.

The existing catalogue reads one data-home-derived store. Install then uses that catalogue to recognize unresolved recovery states. A locally retained bundle is absent from that old decision. P7 supplies the missing rule: bridge-mode recovery stays in the legacy store.

`F/Recovery/RecoveryBundleCatalogue.cs:22`  
`F/Recovery/Shared/Storage/RecoveryBundlePathIdentity.cs:9`  
`O/Install/Shared/Recovery/InstallRecoveryCatalogueReader.cs:17`, `:52`

**R4 — P4’s retain-all-finals policy needs explicit changes to recovery-conflict guards.**  
Retaining every final cannot by itself deliver its promise that retained evidence does not block another command. Route Move rejects any nonempty catalogue. Include/Exclude does likewise on its mutation path. P4 acknowledges changes to recovery lifecycles, but does not define the replacement admission policy.

This is an unresolved implementation obligation, not evidence that retaining backups is inherently wrong.

`O/Route/Move/Shared/Application/RouteMoveRecoveryLifecycle.Preparation.cs:34`  
`O/Route/Shared/Inclusion/Application/RouteInclusionApplication.cs:58`

I verified P4’s separate claim that Route Move excludes creates from recovery targets. That claim is correct. `O/Route/Move/Shared/Planning/RouteMoveEffectPlanner.cs:182`.

#### Best combination

I would ship **P2’s location and enforcement check, P6’s scope analysis, and P7’s transition and recovery-location design**, with a narrower scope than my original P3.

1. **One fixed root-level mutex, from P2/P3.**  
   Use `.open-forge-state/workspace.lock`. Keep it independent of data-home overrides, usernames, mount-path hashes, and the knowledge root. Validate its ancestry and ordinary-file type. Preserve its bytes and never unlink it during normal operation. This avoids both `.agents` bootstrap interference and a documented sandbox protection boundary.

2. **One application lease, from P6/P3.**  
   Keep planning and consent outside it. Keep semantic revalidation, recovery preparation, effects, verification, and operation-owned recovery deletion inside it. Do not shorten the section until measurements identify a material bottleneck. Moving recovery preparation outside it would require changing Cleanup’s protection of active artifacts. No folder-lock graph or optimistic transaction engine.

3. **A bounded exclusion check, from P2.**  
   A successful Unix `FileStream` open is insufficient. The inspected runtime deliberately ignores some lock failures and permits locking to be disabled. After opening the mutex, attempt an independent non-truncating exclusive open. Grant the lease only after recognized contention confirms local exclusion. Other failures remain unverified capability, not success. This check does not certify remote clients or Windows–WSL interoperability. [Inspected .NET implementation](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Private.CoreLib/src/Microsoft/Win32/SafeHandles/SafeFileHandle.Unix.cs#L312-L327)

4. **Protocol-dependent recovery storage, from P7.**  
   In compatible mode, acquire local then legacy locks and continue using legacy recovery storage. After an explicit upgraded-writer cutover, use local recovery by default. Honor an explicit absolute external destination. Do not silently replace an explicitly selected destination after failure.

   This is simpler than P3’s permission-dependent default fallback. Storage behavior follows a declared mode rather than changing with process permissions.

5. **Minimal configuration and complete location propagation, from P7.**  
   Start with one explicit invocation policy, pinned by agent and CI launchers. Avoid a new persisted settings field in this fix. Carry typed recovery locations through catalogue candidates and deletion sessions. The current deletion validator reconstructs its expected directory independently, so changing only the writer is insufficient.

   `F/Recovery/Shared/Deletion/RecoveryBundleDeletionValidation.cs:62`

   Observe active and legacy stores separately. Legacy cleanup requires the legacy lock. Keep existing archives and their attribution unchanged.

6. **Preserve current recovery safety decisions initially, from P7.**  
   P3 correctly identified presence-only barriers, but my proposed replacement was too broad. Install already distinguishes historical bundles from mixed or unobservable states. A blanket “warn and continue” rule could erase meaningful command policy.

   `O/Install/Shared/Recovery/InstallRecoveryCatalogueReader.cs:52`

   Ship exact recovery paths and continuation instructions. Address presence-only barriers later through focused command contracts and tests. Never suggest that deleting a ZIP restores workspace consistency.

7. **Protect runtime storage without expanding the old ignore format, from P7/P6.**  
   Create and verify its internal ignore file before payload bytes. Reserve the directory from scans, inclusion, package destinations, ordinary removal, and root migration. State that forced Git cleaning can delete local backups. Keep existing retention behavior. Defer P4’s forced flushing, retained-all-finals policy, and additional create evidence to separately justified recovery work.

Use P7’s implementation packets and P1’s bootstrap and no-effect checks. Add P2’s disabled-locking case, genuine old/new binaries, bridge recovery visibility, old-reader ignore compatibility, and real interruption between effects. Qualify managed and NativeAOT behavior on all shipped platforms. No builds or tests were run for this review.

#### What changed my mind

P2 changed my assessment of successful acquisition. The runtime source demonstrates a real gap, so a small enforcement check earns its complexity.

P7 changed my migration model. Sharing a mutex is insufficient when older commands cannot discover the new writer’s recovery evidence.

I now prefer explicit local recovery after cutover over P3’s automatic fallback. I also withdraw my broad residual-policy change from this fix and accept nonempty ordinary local mutex files.

Nothing changed my view on granularity. The evidence still supports one workspace application lease. The reported incident does not justify finer locking or a shorter critical section.

## P4: Crash safety and recovery engineer

/root - GPT-6 - high

#### Rankings

Rank 1 is best within each column. These rank the proposals as written, before the corrections below.

| Proposal | LOCATION | SCOPE | NEVER-STUCK | MIGRATION | PLAN |
|---|---:|---:|---:|---:|---:|
| P1 | 6 | 5 | 7 | 2 | 3 |
| P2 | 1 | 3 | 2 | 4 | 4 |
| P3 | 2 | 2 | 1 | 3 | 1 |
| **P4, mine** | **7** | **6** | **3** | **6** | **7** |
| P5 | 4 | 7 | 4 | 7 | 6 |
| P6 | 5 | 1 | 6 | 5 | 5 |
| P7 | 3 | 4 | 5 | 1 | 2 |

P2 best addresses actual sandbox placement and ineffective locking. P6 gives the most accurate dependency analysis. P3 identifies the recovery-artifact barriers that directly threaten continued operation. P7 best preserves recovery visibility across versions.

P4 provides useful interruption cases, but misses existing blockers and adds retention changes that would make those blockers more common. Its implementation plan therefore ranks last.

#### Errors

Here, `F` means `src/cli/framework/OpenForge.Cli.Framework/Framework` and `O` means `src/cli/operations/OpenForge.Cli.Operations/Commands`.

**P4’s retained-evidence guarantee needs additional command changes. P7 explicitly preserves the conflicting behavior.** P4 says retained evidence does not block another command, while proposing retention of successful Index bundles. Route Move rejects any catalogue candidate at `O/Route/Move/Shared/Application/RouteMoveRecoveryLifecycle.Preparation.cs:34`. Include and Exclude do likewise at `O/Route/Shared/Inclusion/Application/RouteInclusionApplication.cs:58`. Create, Init and Update have similar barriers. P4 does not explicitly remove them. P7 acknowledges and retains them. Neither design as written establishes freedom from artifact-driven command lockouts.

**P1–P5 overlook an old-reader compatibility problem in the root ignore section.** Updating the new parser does not update old executables. The current reader rejects every pattern outside its known route patterns at `O/Install/Shared/Configuration/InstallIgnoreSection.cs:26`. Rewriting calls that reader first at line 46. Adding a runtime pattern during the bridge can therefore break older Install invocations despite correct dual locking. P2 explicitly proposes this during the compatible rollout.

**Dual locking alone does not preserve recovery compatibility.** P1, P2, P4, P5 and P6 propose local recovery without P7’s explicit bridge-mode exception. Old readers resolve only the data-home store at `F/Recovery/Shared/Storage/RecoveryBundlePathIdentity.cs:9`. After a new writer crashes and releases both locks, an old writer cannot inspect its local bundle. Mutual exclusion survives, but the old command’s recovery admission policy loses evidence it previously depended on.

**P4 misses the fresh-install bootstrap conflict.** Install records `.agents` as missing at `O/Install/Shared/Planning/InstallPlanningBasisBuilder.cs:75`, then plans its creation at `O/Install/Shared/Planning/InstallEstablishmentPlanner.cs:447`. Creating `.agents/.open-forge` for lock acquisition changes that expectation. P4’s plan lacks the reconciliation identified by P1, P5 and P7.

**P3’s unchanged zero-byte rule leaves an avoidable obstruction.** A nonempty ordinary file remains unacceptable in both `F/Mutation/Locking/WorkspaceLockManager.cs:198` and `F/Mutation/Locking/Models/WorkspaceLockLease.cs:19`. Its leftover-file guarantee must be narrowed or those checks changed for the new lock.

#### Best combination

**Placement and ownership from P2, with P4’s nonempty-file tolerance.** Use one fixed `<workspace>/.open-forge-state/workspace.lock`. Its physical file supplies coordination identity. Never relocate it through an environment override, truncate it, replace it or delete it.

P2’s sandbox objection is verified. Existing `.agents` directories are recursively protected under the documented default Codex workspace-write policy. A root runtime directory avoids imposing that restriction on commands whose actual outputs are elsewhere. Commands modifying protected controls still need appropriate permissions. [OpenAI security documentation](https://learn.chatgpt.com/docs/agent-approvals-security)

Use P2’s second independent exclusive-open check before granting a lease. Treat unexpected success as unenforced locking and unrelated failures as unverified enforcement. This detects local failures, not cross-host reliability. Qualify supported filesystems and execution environments explicitly.

**Scope from P6 and P3.** Hold one workspace lease through fresh validation, recovery preparation, effects, verification and operation-owned cleanup. Index reads settings and ownership without routinely rewriting them. Route Move can rewrite references beyond its source route and already rebuilds its plan under the lease. Preserve those dependencies. Reject overlapping nested-workspace effects unless one coordination boundary covers them. Do not introduce finer locks now.

**Recovery admission from P3, informed by P7’s distinction between evidence and actual inconsistency.** Audit every catalogue-based admission gate. Replace blanket candidate-existence checks with validation of the current operation’s dependencies and affected state.

Do not simply delete every recovery check. Install already distinguishes historical bundles from some mixed states at `O/Install/Shared/Recovery/InstallRecoveryCatalogueReader.cs:52`. Preserve justified stops where required state cannot be validated. An unrelated draft must not demand evidence deletion before another safe command can run.

**Storage and deletion from P2 and P7.** After cutover, default recovery to the reserved runtime directory, with explicit external storage available. Select the destination before preparation and never switch after effects begin. Preserve invalid explicit-override errors.

Carry a typed store origin through writing, observation and guarded deletion. Changing only the writer’s directory is insufficient because deletion independently reconstructs the old location at `F/Recovery/Shared/Deletion/RecoveryBundleDeletionValidation.cs:66`.

Establish the internal ignore file atomically before storing user bytes. Reserve the subtree independently of Git in scans and mutation policies. Use P7’s internal ignore approach during migration, without adding incompatible root-section patterns. Report each store’s coverage separately. Preserve unmatched identity buckets without adopting them.

Ignored backups remain vulnerable to `git clean -x` and `-X`. Writers must stop before destructive cleaning, and wanted recovery evidence must be preserved elsewhere. [Git clean documentation](https://git-scm.com/docs/git-clean)

**Migration from P7 and P1.** Default existing workspaces to an explicit compatibility mode. Acquire local then legacy, release both on failure, and hold both through the operation. Crucially, continue writing legacy-visible bundles while old writers remain supported. Legacy cleanup also requires both locks.

Persist the explicit cutover choice as P1 proposes, but require operational retirement and version pinning of old writers. Configuration cannot fence binaries that ignore it. Never infer cutover from an inaccessible legacy directory. Leave existing locks and bundles in place.

**The bounded never-stuck proof combines P3 and P4:**

| Case | Result and continuation |
|---|---|
| Killed holder or ordinary leftover lock | OS handle release ends ownership on qualified systems. Reopen the same file, regardless of its bytes. |
| Kill during draft creation | Protected replacements and deletions have not begun. Preserve the draft, report it, and allow independently validated work. |
| Final published before application | A final proves preparation, not application. Revalidate current files without an existence-only veto. |
| Kill during staging or between effects | Preserve stages and recovery evidence. Replan from actual files. Reconcile affected inconsistencies before dependent work. |
| Malformed settings or ownership | Recovery discovery remains available without successful control-document parsing. Report exact damaged controls and evidence paths. |
| Kill during cleanup | Catalogue the survivors again. Exact-candidate guards make another cleanup attempt possible. |
| Denied path, full disk or unsafe occupant | Stop with the exact cause and release acquired handles. Correct the obstruction. Never substitute another mutex silently. |
| Power loss | Reinspect files and verify surviving archives. No claim of durable multi-file atomicity or guaranteed recovery from lost storage. |

This proves absence of persistent software ownership and blanket residual-artifact vetoes under the stated filesystem assumptions. It does not prove unattended repair of damaged files or writes through denied permissions.

**Delivery combines P3’s gate audit with P7’s integration plan.** Budget 10–15 engineering days across coordination, recovery and admission, command integration, and qualification. Require real applying-process interruption tests at draft, final, staging and target-effect boundaries, plus old/new recovery visibility, cleanup interruption, denied storage and first-install cases. Run supported Windows, Linux and macOS managed and NativeAOT gates during implementation.

Defer P4’s universal final-retention policy and durability expansion. Neither is necessary for this scoped fix, and both require separate lifecycle acceptance.

#### What changed my mind

I no longer favor placing runtime state under `.agents`.

P3 showed that stale ownership is only half the problem. Existing recovery gates can block healthy operations.

P7 showed that mixed-version recovery visibility matters alongside mutual exclusion.

I would withdraw P4’s retain-all-finals change from this work. It expands scope and aggravates existing blockers. Its interruption matrix remains useful, but it is an acceptance plan, not evidence that those cases already pass.

## P5: Agent workflow realist

/root - GPT-6 - high

#### Rankings

Ranks are relative within each column. **1 is best.** LOCATION includes configuration and reporting.

| Proposal | LOCATION | SCOPE | NEVER-STUCK | MIGRATION | PLAN |
|---|---:|---:|---:|---:|---:|
| P1 | 6 | 6 | 7 | 2 | 3 |
| P2 | 1 | 4 | 2 | 3 | 2 |
| P3 | 2 | 2 | 1 | 4 | 4 |
| P4 | 7 | 7 | 6 | 6 | 7 |
| **P5, mine** | **3** | **5** | **4** | **7** | **6** |
| P6 | 5 | 1 | 5 | 5 | 5 |
| P7 | 4 | 3 | 3 | 1 | 1 |

P2 wins LOCATION because it checks the sandbox’s actual protected paths. P3 loses some ground for permission-dependent recovery placement, which makes two agents’ storage behavior harder to predict.

P6 wins SCOPE by distinguishing writes from read dependencies. Index need not rewrite ownership to require protection against concurrent ownership changes. P3 adds the strongest explanation of why recovery preparation must remain protected from Cleanup.

P3 wins NEVER-STUCK because it explicitly addresses existing recovery-artifact vetoes. P7 recognizes them but retains them. P4 provides useful crash analysis, but retaining every final bundle without explicitly changing those vetoes creates a serious integration gap.

P7 wins MIGRATION and PLAN because it preserves old readers’ recovery visibility and follows storage identity through guarded deletion. My two-release rollout delays the sandbox benefit and leaves more deployment work to agent launchers.

#### Errors

The following distinguish unsupported claims from implementation gaps. A proposed behavior change is not itself an error.

- **P1, P4, P5 and P6 overstate the sandbox benefit of storage under `.agents`. P7’s chosen location has the same limitation.** Existing `.agents` directories are recursively protected under default Codex workspace-write policy. P2 correctly identifies this. This is an environment correction, verified against [official sandbox documentation](https://learn.chatgpt.com/docs/agent-approvals-security), rather than repository code. The repository also confirms that actual settings remain under `.agents`, so moving runtime storage alone cannot authorize every command: `src/cli/framework/OpenForge.Cli.Framework/Framework/Settings/WorkspaceSettingsDefinitions.cs:12`.

- **P3’s ordinary-leftover guarantee needs a zero-byte qualification.** Keeping the existing mechanism unchanged does not allow every ordinary leftover file to be reopened successfully. The manager rejects nonempty files, and the lease independently requires zero length. Normal crash residue is reusable, but nonempty residue remains an obstruction. Both checks need changing to adopt P2/P5/P7’s stronger guarantee: `src/cli/framework/OpenForge.Cli.Framework/Framework/Mutation/Locking/WorkspaceLockManager.cs:198` and `src/cli/framework/OpenForge.Cli.Framework/Framework/Mutation/Locking/Models/WorkspaceLockLease.cs:22`.

- **P4’s claim that retained recovery evidence does not block another command requires additional changes.** Route Move blocks on any catalogue candidate. Include and Exclude do likewise. Retaining Index’s successful-operation bundles would therefore obstruct later unrelated operations under current policies. P4’s plan must explicitly change those gates. My P5 also overlooked them: `src/cli/operations/OpenForge.Cli.Operations/Commands/Route/Move/Shared/Application/RouteMoveRecoveryLifecycle.Preparation.cs:34` and `src/cli/operations/OpenForge.Cli.Operations/Commands/Route/Shared/Inclusion/Application/RouteInclusionApplication.cs:58`.

- **Updating the new Install ignore parser does not preserve compatibility with old Install readers.** P2 explicitly proposes adding the runtime pattern during the compatible rollout. P1, P3, P4 and P5 also propose using Install’s bounded section without resolving that older-reader problem. The current reader rejects patterns outside its known route patterns, and setup invokes that reader. P7’s runtime-local ignore file avoids this dependency: `src/cli/operations/OpenForge.Cli.Operations/Commands/Install/Shared/Configuration/InstallIgnoreSection.cs:26` and `src/cli/operations/OpenForge.Cli.Operations/Commands/Install/Shared/Configuration/InstallSetupResolver.cs:54`.

Two important claims survived verification. P6 correctly rejects the brief’s implication that Index routinely rewrites shared controls. P7 correctly identifies that deletion validation independently reconstructs the external recovery location, so changing catalogue discovery alone is insufficient: `src/cli/framework/OpenForge.Cli.Framework/Framework/Recovery/Shared/Deletion/RecoveryBundleDeletionValidation.cs:66`.

#### Best combination

**Ship P2’s location, P6’s scope analysis, P3’s recovery-gate audit, P7’s migration mechanics and P5’s diagnostic requirements.**

**Location and environment behavior — P2, with P5 reporting.** Use one fixed `<workspace>/.open-forge-state/workspace.lock`. Default new recovery storage to that reserved directory after cutover. Do not include an absolute-path hash in the mutex filename or let an environment variable relocate it.

Separate worktrees and independent CI checkouts get independent locks. Agents sharing a checkout coordinate through the same filesystem object. Containers must share that object, including the runtime directory. Different mount spellings should not create separate mutexes.

Use P2’s second-open exclusion probe before granting a lease. Treat it as evidence about local enforcement, not proof of cross-host interoperability. Qualify network storage, Docker Desktop and Windows/WSL combinations separately. A real permission failure must remain an explicit failure.

**Scope — P6 and P3.** Keep one workspace lease across under-lock validation, recovery preparation, application, verification and recovery deletion. Preserve Route Move’s complete replanning until a replacement can demonstrate equivalent dependency coverage. The current implementation rebuilds the plan under the lease: `src/cli/operations/OpenForge.Cli.Operations/Commands/Route/Move/Shared/Application/RouteMovePlanRevalidator.cs:28`.

Do not introduce folder locks, a publication-only lock or a second recovery-lifetime protocol in this change. Those would require protecting read dependencies, parent and sibling entrypoints, discovered references and active recovery artifacts.

**Continuation after interruption — P3, bounded by P7’s safety analysis.** Tolerate nonempty ordinary sentinel files without truncating them. Never infer ownership from file existence or delete a lock to recover.

Audit every recovery-candidate veto. Replace mere-existence barriers where current inputs and the proposed operation can be independently validated. Preserve meaningful blocks for inconsistent controls, unresolved affected targets and explicit recovery operations. Install already distinguishes current target states rather than blocking every verified historical bundle: `src/cli/operations/OpenForge.Cli.Operations/Commands/Install/Shared/Recovery/InstallRecoveryCatalogueReader.cs:52`.

Retain evidence while making the next action specific. Cleanup must never be presented as repairing partially applied files. Defer P4’s universal retention policy and broader durability changes to separately justified work.

**Migration — P7 plus P1.** Persist a workspace coordination mode. Existing installations initially use compatibility mode. Acquire local, then legacy, and hold both through completion. Continue writing recovery bundles to the legacy store during this mode, as P7 proposes. Dual locks alone do not give old commands visibility into local archives. The current catalogue resolves only the external store: `src/cli/framework/OpenForge.Cli.Framework/Framework/Recovery/RecoveryBundleCatalogue.cs:22`.

After stopping old writers and pinning upgraded launchers, perform one explicit cutover to workspace mode. Thereafter agents need no per-invocation flag or data-home setup. Fresh installations can establish workspace mode directly.

Never silently skip an inaccessible legacy lock. A persisted setting cannot fence an old executable. The unavoidable migration prerequisite should be owned by the operator or launcher, not rediscovered by every coding agent.

**Recovery discovery and protection — P7, P2 and P5.** Carry the observed store identity through catalogue records and guarded deletion. Observe active and known legacy locations without creating them. Report unreadable locations separately from empty ones. Preserve foreign-origin bundles without silently rebinding their workspace identity.

Create a runtime-local ignore file before copying user bytes. Reserve the directory in scanners and mutation destination policies. Keep new runtime patterns out of the legacy Install-owned ignore section during compatibility. Explicit external recovery selection remains available and must fail clearly if unusable.

**One-error usability — P5, corrected by P2 and P6.** Every failure should identify the workspace, failed path, phase, supported cause, whether target effects occurred and one next action.

Busy means retry after the writer finishes. Access denied means identify the denied directory and required host permission. Unverified locking means use qualified storage or a single writer. Changed inputs mean rerun planning. Recovery conflicts mean identify the affected targets and retained bundle.

Do not claim another process holds the lock when that is unproven. Do not claim nothing changed if runtime scaffolding was created. Preserve command-specific status mappings, including Install’s existing `WorkspaceUnsafe` mapping.

**Delivery — P7’s packets, strengthened by P2, P3 and P4.** Separate coordination, recovery locations and deletion, runtime protection, command diagnostics, migration, and qualification. Include real sandbox tests, old/new executable contention, retained-artifact continuation, and process termination at application boundaries. Budget roughly 10–15 engineering days. These checks are proposed work, not tests run during this read-only review.

#### What changed my mind

P2 overturned my assumption that `.agents/.runtime` solves the default agent sandbox problem.

P3 and P7 showed that lock placement alone cannot deliver the requested continuation behavior. Recovery catalogues already create independent command barriers.

P7 also changed my migration recommendation. Sharing a mutex is insufficient if older commands cannot discover new recovery evidence. I now prefer legacy recovery writes during compatibility and one explicit workspace cutover over my two-release rollout.

I would also drop P5’s arbitrary five-second wait until observed contention justifies it. An agent benefits more from a promptly classified failure with a precise next action.

## P6: Fine-grained concurrency advocate

/root - GPT-6 - high

#### Rankings

Each column ranks all seven proposals independently. Lower is better.

| Proposal | LOCATION | SCOPE | NEVER-STUCK | MIGRATION | PLAN |
|---|---:|---:|---:|---:|---:|
| P1 | 6 | 6 | 7 | 2 | 3 |
| P2 | 1 | 4 | 2 | 3 | 2 |
| P3 | 2 | 2 | 1 | 4 | 4 |
| P4 | 7 | 5 | 6 | 6 | 5 |
| P5 | 4 | 7 | 4 | 7 | 6 |
| **P6, mine** | **5** | **1** | **5** | **5** | **7** |
| P7 | 3 | 3 | 3 | 1 | 1 |

P2 best addresses the actual sandbox boundary. P6 makes the strongest conditional case for finer concurrency without misrepresenting shared control reads as writes. P3 identifies an existing recovery-artifact veto that relocation alone cannot fix. P7 provides the most complete migration and implementation plan.

The scope rankings are close because all seven retain a workspace lease. P4’s broader retention policy needs additional changes to avoid increasing recovery-related blockage. P5’s release-wide cutover is less useful than an explicit deployment boundary. My original estimate omitted enough integration work to deserve last place for PLAN.

This review used source inspection. No builds or tests were run.

#### Errors

Paths below use:

- `F` = `src/cli/framework/OpenForge.Cli.Framework/Framework`
- `O` = `src/cli/operations/OpenForge.Cli.Operations/Commands`

**P4’s claim that retained recovery evidence does not block another command contradicts current behavior unless its plan explicitly changes the admission checks.** Route Move rejects any existing candidate at `O/Route/Move/Shared/Application/RouteMoveRecoveryLifecycle.Preparation.cs:34`. Include does the same at `O/Route/Shared/Inclusion/Application/RouteInclusionApplication.cs:58`. Retaining successful Index bundles until Cleanup would therefore introduce routine blockers under those policies.

**P6 understates the same problem.** I said not to introduce a workspace-wide recovery latch, but such blanket gates already exist. My plan did not remove them. P3 correctly treats this as necessary behavioral work. P7 identifies the problem but deliberately retains the existing policies, which limits its never-stuck result.

**P1, P2, P3, P4 and P5 miss a mixed-version consequence of expanding Install’s owned ignore section.** The current reader accepts only recognized route patterns and throws on anything else at `O/Install/Shared/Configuration/InstallIgnoreSection.cs:26`. Configuration invokes that reader at `O/Install/Shared/Configuration/InstallConfigurationIntendedStateBuilder.cs:85`. Updating the new reader and writer does not update older binaries. Adding a runtime pattern during a compatible rollout can therefore break old Install configuration operations even when dual locking preserves exclusion. P2’s explicit proposal to add the rule during that rollout is particularly problematic.

**P3’s “no stale-lock cleanup problem” needs qualification.** Keeping the existing zero-byte acceptance rule means an ordinary nonempty sentinel remains a persistent obstruction. Both `F/Mutation/Locking/WorkspaceLockManager.cs:198` and `F/Mutation/Locking/Models/WorkspaceLockLease.cs:22` reject it. A normally abandoned empty sentinel is harmless. The broader claim about arbitrary ordinary leftovers is false.

#### Best combination

**1. Use P2’s root-level location, with P6/P7’s internal ignore policy.**

Ship one fixed `.open-forge-state/workspace.lock`, independent of the knowledge root, Git metadata, account and recovery destination.

P2’s sandbox objection changed the location decision. Current Codex documentation says an existing `.agents` directory is recursively protected under default workspace-write policy. Being inside the worktree does not establish writability. [Codex security documentation](https://learn.chatgpt.com/docs/agent-approvals-security)

Root placement also avoids creating `.agents` before first Install’s planned effects, as P3 observes. It does not grant permission to modify protected content or control files. Those remain real target-access requirements.

Use a runtime-local `.gitignore`. Reserve the directory separately against source discovery, Markdown scanning, package projection, inclusion, moves and removal. Do not extend Install’s root ignore section during this migration. Ignored recovery remains vulnerable to destructive cleaning and workspace deletion.

**2. Keep P6’s workspace lease, strengthened by P3’s dependency analysis.**

The actual operations support this decision:

- **Index** reads ownership and settings expectations at `O/Index/Shared/Projection/IndexProjectionReader.cs:155`. Its maintenance selection expands beyond the requested source to related navigation at `:178`. Shared control reads do not inherently serialize independent Index operations, but apparent folder separation does not establish independence.
- **Route Move** combines navigation, reference replacements, creates and deletes, with an optional settings change at `O/Route/Move/Shared/Planning/RouteMoveEffectPlanner.cs:30` and `:56`. Its Markdown catalogue starts from the workspace root at `O/Route/Move/Shared/Planning/RouteMovePlanBuilder.cs:88`.
- **Install** publishes ownership and configuration effects at `O/Install/Shared/Planning/InstallEstablishmentPlanner.cs:324` and `:403`. Ownership updates serialize the complete record at `F/Ownership/WorkspaceOwnershipStore.cs:27`.

Hold the lease through revalidation, recovery preparation, application, verification and recovery disposal. Separate worktrees remain independent. Add no repository-wide lock without an identified repository-wide mutable resource. Reject operations crossing another workspace boundary unless their overlapping targets participate in the same coordination domain.

Fine scoping becomes worthwhile if measurements show frequent, materially delayed, independent scoped Index operations. Then use a shared workspace gate for scoped Index, exclusive admission for broad writers, shared locks for observed inputs, exclusive locks for effects, and hierarchical protection for directory membership. Acquire resources in a canonical order and revalidate the complete dependency set. Locking only changed files misses newly discovered sources and references.

P3 also correctly identifies that moving recovery preparation outside the lease changes Cleanup’s safety assumptions. Shortening the lease is separate architectural work.

**3. Combine P2/P5’s enforcement check with P6/P7’s sentinel semantics.**

Accept any ordinary local sentinel without reading, truncating or replacing its contents. Reject unsafe objects and ancestry. Never unlink a live coordination file or infer ownership from age or PID.

A successful open alone is insufficient on every supported environment. The inspected .NET Unix implementation can ignore some locking failures and can disable file locking through configuration. [Runtime source](https://raw.githubusercontent.com/dotnet/runtime/v10.0.0/src/libraries/System.Private.CoreLib/src/Microsoft/Win32/SafeHandles/SafeFileHandle.Unix.cs)

Before granting a lease, perform P2/P5’s independent second-open exclusion check. Require a recognized contention result. Qualify the exact published runtime with cross-process tests. A local probe does not prove interoperability between network clients, Windows and WSL.

Report confirmed contention, permission denial, unsafe paths and uncertain I/O distinctly. Include the acquisition stage and path. Status should observe without creating directories or probing write access.

**4. Adopt P7’s migration and typed recovery locations.**

During compatible mode, every new writer acquires the local lock, then the unchanged legacy lock. Hold both throughout application. Continue writing recovery to the legacy store so older commands can discover it.

Offer P7’s explicit `--lock-protocol compatible|workspace`. Workspace mode requires retiring old writers and reviewing legacy residuals. Never infer that condition from access denial, a marker or an empty directory.

After cutover, default recovery to `.open-forge-state/recovery/v1/<workspace-key>/`. Retain an explicit absolute `OPENFORGE_DATA_HOME` for external recovery. Do not silently redirect an invalid explicit destination.

Carry P7’s typed `RecoveryStoreLocation` through writers, observers and deletion guards. This is necessary because deletion currently reconstructs the default store independently at `F/Recovery/Shared/Deletion/RecoveryBundleDeletionValidation.cs:66`.

Observe active and legacy stores separately. Unavailable observation is not an empty store. Cleanup defaults to the active store. Explicit legacy cleanup requires both locks. Leave existing lock files and archives in place. Preserve foreign-origin attribution rather than adopting archives merely because they are nearby.

**5. Adopt P3’s removal of blanket recovery vetoes, with targeted safety checks.**

A retained archive alone should not disable unrelated operations. Replace the Route Move and Include existence checks with current-state admission that establishes the requested operation’s safety.

Preserve genuine unresolved-control and partial-application checks. Install already distinguishes these cases. Its classifier even treats a verified bundle containing a `Third` target state as history at `O/Install/Shared/Recovery/InstallRecoveryCatalogueReader.cs:63`. Reuse the principle, not an assumption that every command has identical recovery semantics.

Use P4’s clear interruption diagnostics and recovery discovery that survives malformed controls. Do not make broader retention, automatic rollback or durability changes part of this fix. Cleanup deletes evidence. It does not repair partially applied files.

**6. Use P7’s implementation packets, adding P3’s admission changes and P2’s qualification.**

Budget **10–15 engineering days**, with further qualification potentially extending that estimate. Freeze protocol, location, bootstrap and reporting contracts first. Divide ownership into locking, recovery locations and deletion, command admission and runtime exclusions, then reporting and qualification.

Require old/new exclusion in both directions, first Install, denied profile access, protected `.agents`, nonempty sentinels, retained unrelated archives, interrupted application, Cleanup races and independent worktrees. Exercise published NativeAOT processes across the supported platform matrix. Update affected command contracts and the mutation/recovery design.

#### What changed my mind

P2 changed my preferred location from `.agents/.runtime` to a reserved workspace-root directory.

P3 showed that residual recovery blockage is existing behavior requiring explicit changes. P7 showed that migration must preserve recovery visibility as well as mutual exclusion, and that store identity must reach deletion guards.

My scope conclusion is unchanged. Fine scoping has a credible future for independent Index workloads. The present evidence does not justify its complexity, and it does not solve the reported storage-access failure.

## P7: CLI architect

/root - GPT-6 - reasoning level unreported

#### Rankings

Each column ranks all seven proposals independently, with 1 best.

| Proposal | LOCATION | SCOPE | NEVER-STUCK | MIGRATION | PLAN |
|---|---:|---:|---:|---:|---:|
| P1 | 6 | 5 | 7 | 4 | 3 |
| P2 | 1 | 4 | 2 | 6 | 2 |
| P3 | 2 | 2 | 1 | 3 | 5 |
| P4 | 7 | 6 | 3 | 7 | 7 |
| P5 | 5 | 7 | 4 | 5 | 6 |
| P6 | 4 | 1 | 6 | 2 | 4 |
| P7, mine | 3 | 3 | 5 | 1 | 1 |

P2 wins location because it addresses the actual sandbox boundary. Writable workspace access does not imply writable `.agents` access. Codex’s default workspace-write policy protects existing `.agents` directories recursively. P2 correctly separates coordination permissions from permissions needed to modify actual control files. [Official sandbox documentation](https://learn.chatgpt.com/docs/agent-approvals-security)

P6 gives the strongest granularity argument through actual read dependencies and write sets. P3 identifies the most consequential remaining liveness problem, the recovery-candidate vetoes. P7 has the strongest migration and implementation seams, particularly legacy recovery visibility, deletion authority and per-store observation failures. Its decision to preserve blanket recovery barriers weakens its never-stuck answer.

P4’s interruption analysis is useful, but retaining all finals, extending recovery coverage and adding flushing make its implementation substantially broader than the reported coordination problem.

#### Errors

For compact citations, `F` means `src/cli/framework/OpenForge.Cli.Framework/Framework` and `O` means `src/cli/operations/OpenForge.Cli.Operations/Commands`.

1. **P2’s “compatible rollout” cannot include the proposed managed ignore pattern without affecting old Install behavior.** P1, P3, P4 and P5 propose the same addition. Updating the new parser does nothing for old executables. The existing parser rejects every pattern outside its route choices, and setup converts that rejection into `TargetUnsafe`. Dual locking preserves exclusion, but does not preserve this command behavior. Use the runtime-local ignore file during coexistence.

   Evidence: `O/Install/Shared/Configuration/InstallIgnoreSection.cs:26`, `O/Install/Shared/Configuration/InstallSetupResolver.cs:54`.

2. **Local recovery defaults leave a compatibility gap in P1, P2, P4, P5 and P6.** Their dual locks do not make new local archives visible to old recovery guards. P3 has the same gap whenever its fallback selects local storage. The existing catalogue resolves one data-home store and enumerates only the selected workspace bucket. After an interrupted new write, an old command can miss evidence that would otherwise affect its recovery checks. P7 explicitly avoids this by keeping recovery writes in the legacy store during compatible mode.

   Evidence: `F/Recovery/RecoveryBundleCatalogue.cs:22`, `F/Recovery/RecoveryBundleCatalogue.cs:51`, `F/Recovery/Shared/Storage/RecoveryBundlePathIdentity.cs:9`, `O/Install/Shared/Recovery/InstallRecoveryCatalogueReader.cs:17`.

3. **P4’s “replan normally” and “retained recovery evidence does not block another command” require an additional behavioral change.** A new operation ID prevents filename collision, but does not bypass the existing catalogue gates. Route Move and shared Include/Exclude application reject any candidate. Retaining successful Index bundles would increase those obstructions unless those gates change explicitly. P4’s implementation packet does not identify that dependency precisely enough.

   Evidence: `O/Route/Move/Shared/Application/RouteMoveRecoveryLifecycle.Preparation.cs:34`, `O/Route/Shared/Inclusion/Application/RouteInclusionApplication.cs:58`.

4. **P3’s ordinary-leftover guarantee is too broad while retaining zero-byte validation.** An ordinary nonempty file remains a persistent obstruction even after exclusive opening succeeds. Both the manager and lease constructor reject it. This is not live ownership, and P3 needs the explicit contents-irrelevant change proposed elsewhere.

   Evidence: `F/Mutation/Locking/WorkspaceLockManager.cs:198`, `F/Mutation/Locking/Models/WorkspaceLockLease.cs:19`.

These findings distinguish false compatibility consequences from intentional changes. Moving storage or changing retention is not itself a factual error.

#### Best combination

**1. Use P2’s root-level runtime directory with P7’s bounded configuration.**

Ship a fixed `<workspace>/.open-forge-state/workspace.lock`. Its location identifies the shared coordination object. Keep the existing physical-path key for legacy discovery and recovery attribution.

Use an explicit `--lock-protocol compatible|workspace`, with compatible mode as the transition default. Do not make the new mutex location configurable. This avoids both the protected `.agents` directory and first-Install interference with planned creation of `.agents`. Actual writes to settings or ownership still require access to those files.

**2. Keep P6’s workspace scope and P3’s full lease lifetime.**

Planning and prompting remain outside acquisition. Hold the lease through input revalidation, recovery preparation, application, verification and recovery cleanup.

Index reads ownership and settings and expands selection to parents and siblings. Those dependencies matter even when it does not write the controls. Route Move rebuilds its complete plan under the lease. Cleanup can delete preparation artifacts, so moving recovery preparation outside the lease requires another synchronization design.

Evidence: `O/Index/Shared/Projection/IndexProjectionReader.cs:161`, `O/Index/Shared/Projection/IndexProjectionReader.cs:178`, `O/Route/Move/Shared/Application/RouteMovePlanRevalidator.cs:25`, `O/Cleanup/Shared/Application/CleanupApplicationOperation.cs:23`.

I would not ship finer locks or nested-workspace overlap guarantees based merely on different selected roots.

**3. Combine P2’s lock diagnostics with P3’s recovery-barrier correction.**

Accept existing ordinary local lock files regardless of contents. Never truncate, unlink, expire or steal them. Preserve legacy validation while interoperating with old binaries.

Carry acquisition stage, exact path and supported failure facts through `WorkspaceLockResult` and command mappings. Distinguish established contention from denied access and ambiguous I/O. Read-only Status must not create files to test writability.

Use P2’s second-open check to detect locally unenforced locking, subject to platform verification. It cannot establish cross-host or Windows/WSL interoperability.

Replace the presence-only checks in Include/Exclude and Move with operation-specific current-state decisions. Retained evidence remains visible. Unsafe paths, unreadable controls and unresolved relevant partial changes still stop application. Preserve Install’s existing state-based recovery checks rather than indiscriminately removing recovery protection.

A killed writer releases ownership. A partial apply may still require reconciliation. No automatic rollback or persistent workspace-wide “dirty” flag is needed.

**4. Use P7’s recovery location contract, extended with P2/P5’s foreign-bucket reporting.**

In workspace mode, default recovery to `.open-forge-state/recovery/v1/<workspace-key>/`. Preserve the existing archive format and verified publication sequence. An explicit absolute `OPENFORGE_DATA_HOME` may select external recovery storage.

Introduce a typed location shared by `RecoveryBundleStore`, `RecoveryBundleCatalogue`, storage boundaries and deletion validation. This is necessary because deletion currently independently reconstructs the expected bucket.

Evidence: `F/Recovery/Shared/Deletion/RecoveryBundleDeletionValidation.cs:66`.

Status and Doctor continue through `RecoveryResidualOperationalContributor`. Cleanup keeps its lease-bound deletion session. Report active, historical and unavailable stores separately. Preserve foreign-identity buckets as unmatched evidence without granting deletion authority.

Create and verify a runtime-local ignore file before copying user bytes. Explicitly exclude the subtree from scans, destination admission and ordinary removal. Keep Install’s managed ignore section unchanged during compatibility. Document that destructive Git cleaning can remove local recovery.

**5. Keep P7’s migration semantics and P1’s operational cutover discipline.**

Compatible mode acquires local first, then the exact legacy lock, and writes recovery into the legacy store. Failure releases all acquired handles before target effects.

Workspace-only mode requires stopping old writers and pinning upgraded launchers. Never infer that cutover from access denial. Legacy cleanup requires both locks. Leave existing lock files and recovery archives in place.

This preserves the honest boundary that transparent sandbox operation and unrestricted old-binary coexistence cannot both be guaranteed.

**6. Start with P7’s work packets, adding explicit ownership of P3’s barrier changes.**

The exact seams are:

- Lock manager, path identity, lease and result models
- Recovery location, catalogue, storage validation and deletion sessions
- Runtime bootstrap, scan exclusions and destination policies
- Acquisition callers, shared option binding, Status/Doctor reporting and Cleanup selection
- Include/Exclude and Move recovery gates, associated contracts and regression evidence

Allow **12–18 engineering days**, including integration and review. P7’s 10–15 days is a reasonable base, with another 2–3 days for the recovery-gate changes and their failure scenarios.

Qualification must cover real old/new binaries, killed holders, interruption between effects, cleanup races, nonempty sentinels, denied stores, observer purity and separate worktrees on Windows, Linux and macOS. Network and cross-environment qualification needs additional environments.

Defer P4’s retention expansion and flushing changes to separately scoped recovery work.

#### What changed my mind

I would move P7’s runtime directory out of `.agents`. P2 supplied a concrete sandbox reason, while P3 identified the cleaner first-Install boundary.

I would also revise P7’s decision to preserve blanket recovery barriers. The code confirms that an unrelated residual archive can block an otherwise valid operation. P3 addresses that directly.

Finally, mixed-version compatibility needs both storage visibility and parser compatibility. Sharing a mutex alone is insufficient.
