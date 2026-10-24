---
open-forge:
  description: The seven Stage 1 proposals of the lock scope council, kept as evidence
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Council, Locking, Recovery]
---

# Lock Scope Council Proposals

The seven independent Stage 1 proposals, each under its Stage 2 label and lens. Citations point at `develop` commit `c2913bab1`. The [record](record.md) holds the decisions.

## P1: Migration and contract guardian

/root - GPT-6 - high

#### Recommendation

Keep one mutation lock per workspace and place its new coordination file under the fixed `.agents` control directory. Introduce a compatibility mode that holds both the workspace lock and the unchanged legacy lock. Permit workspace-only coordination through an explicit cutover after every writer has upgraded. Default new recovery bundles to reserved, ignored workspace storage while preserving existing v1 bundles in place. Never silently substitute another lock when acquisition fails.

#### Answers

##### 1. Where the lock lives and what it is keyed by

Use `.agents/.open-forge/write.lock`. Every new CLI opening the same physical workspace must open that same ordinary file. Do not include a process ID, CLI version, route, or mount-path hash in its filename.

Retain an exclusive, process-held `FileStream`. The file persists after release. This preserves the existing ownership mechanism, whose implementation uses `FileShare.None`, writes no metadata, and rejects nonordinary targets. [E1]

The directory belongs to the fixed control area. It must remain there when Task 85 moves the knowledge root. Task 85 is currently **deferred**, contrary to the brief’s “next” description. Its accepted design does retain settings and ownership under `.agents`. [E10]

Environment behavior:

| Environment | Proposed behavior |
|---|---|
| Worktree sandbox | Workspace-only mode needs writes inside that worktree |
| Separate worktrees | Independent coordination files |
| Container or CI | Share the coordination file whenever jobs share writable workspace files |
| WSL | Qualify Windows/Linux access to the same filesystem before promising cross-environment exclusion |
| Network drive | Support concurrent writers only on qualified mounts with effective shared locking |
| Unqualified shared filesystem | Use separate checkouts or externally serialized writers |

Do not choose a Git directory. It can sit outside sandbox permissions, and workspace selection currently uses the selected directory without discovering a repository root. [E2]

##### 2. Granularity

Keep one lock across revalidation, recovery preparation, application, verification, ownership publication, and recovery disposal. Planning and consent stay outside it.

Smaller locks would require coordinating overlapping write sets. Index applies a sequence of updates. Route Move combines reference changes, navigation changes, source deletions, and settings changes. These are not folder-local transactions. [E5]

Expected-state checks remain essential, but they do not make a multi-file operation atomic or eliminate the interval between checking and replacing a file. The existing revalidator requires a held same-workspace lease. [E6]

Separate worktrees provide useful concurrency without introducing a lock scheduler. Distinct selected roots whose write sets overlap are not automatically independent. Document that boundary rather than promising safety merely because their lock paths differ.

##### 3. Never stuck

The achievable guarantee is **no persistent busy state caused by an ordinary leftover coordination file**. Unwritable storage, invalid targets, and partial application cannot honestly be guaranteed to require no intervention.

| Situation | User-visible result and action |
|---|---|
| Empty file remains | Acquisition reuses it |
| Holder is killed | OS releases its handle. Retry from current facts |
| Another writer remains active | Acquisition stops before requested effects. Retry after it exits |
| Access denied | Name the denied path and cause. Correct permissions or complete the documented cutover |
| Ambiguous I/O failure | Report uncertainty. Do not assert another writer exists |
| Directory, link, or foreign content occupies the lock path | Preserve it. Report the exact conflict. Inspect and repair it after all writers stop |
| Crash during application | Preserve available recovery evidence. Inspect current files, then replan or perform supported repair |

The existing manager already distinguishes access denial, invalid paths, unsupported operations, and I/O failures internally. Its public wording collapses these into an unsupported contention claim. Index also drops the acquisition cause when forming its finding. [E1, E5, E12]

F20 verifies release after killing a Windows lock-holder process. It does not prove atomic multi-file application or crash durability. [E7]

Never recommend deleting a live mutex or automatically stealing it by age or PID.

##### 4. Recovery bundles

Default new bundles to:

```text
.agents/.open-forge/recovery/v1/<existing-workspace-key>/
```

Keep the existing manifest format, attribution checks, prior bytes, ordered effects, draft verification, final publication, and final readback. Changing storage location does not require changing archive schema. [E8]

This explicitly replaces the accepted requirement that recovery bundles live outside the workspace. It therefore needs a contract decision, not just a path refactor. [E11]

Reserve the entire runtime directory from Index, source discovery, reference scans, package destinations, Remove, and Task 85 migration. Recovery enumeration alone may inspect its bundles.

Establish an internal `.gitignore` before writing payloads. Install also adds the directory to its bounded root ignore section. Preserve user-authored rules and reject incompatible occupants. Ignore rules do not untrack files or prevent forced addition. Existing Install behavior already acknowledges that distinction. [E13]

Ordinary `git clean` respects ignored content. `git clean -fdx` and `git clean -fdX` can remove these backups. Require writers to stop before such cleaning and preserve needed bundles elsewhere. Workspace-local recovery cannot protect against workspace deletion. [Git clean documentation](https://git-scm.com/docs/git-clean)

Status, Doctor, and Cleanup inspect both the local store and known legacy stores without creating them. Report coverage per store. An unreadable legacy store must not be described as empty or prevent unrelated local mutations.

A final ZIP proves verified recovery preparation, not successful completion of its operation. Cleanup documentation must retain that distinction.

##### 5. Configuration and reporting

Introduce one persisted coordination choice:

- `compatibility`, the default when absent
- `workspace`, requiring an explicit upgrade cutover

Proposed configuration surface: `install --configure --coordination workspace`, with the same explicit choice available during initial installation. Neither `--automatic` nor `--force` selects it implicitly.

Publish the setting in the next unused settings schema version. Coordinate that allocation with Task 85’s planned v4.

Retain `OPENFORGE_DATA_HOME`:

- Compatibility locking uses its existing resolution semantics.
- An explicit absolute value continues selecting external recovery storage.
- It never relocates the new workspace mutex.
- An explicitly invalid relative value remains an actionable configuration error. [E3]

Status should report the selected mode, workspace lock path, required legacy path, recovery destinations, and observation limitations. It must not claim current ownership or writability from file existence or permission inspection.

Keep existing finding codes and status mappings where their meaning remains valid. Add cause and path facts through command-owned output. Schema 3 permits compatible optional command-local fields, but freezes shared envelope coordinates. [E14]

##### 6. Migration and mixed versions

**An inaccessible legacy mutex and guaranteed exclusion of unchanged old binaries cannot both be bypassed safely.**

Use this transition:

| Writers | Required coordination |
|---|---|
| Old and new CLI | New CLI holds both locks. Both use the same legacy data home and legacy workspace identity |
| New CLIs in different modes | All acquire the workspace lock |
| All writers upgraded, workspace mode selected | Workspace lock alone |
| Old CLI after workspace-only cutover | Unsupported concurrent writing. The new setting cannot fence it out |

Every new writer acquires the local lock first, rereads coordination policy, then acquires the legacy lock when required. Failure or cancellation releases every acquired handle. No requested effect begins until the complete required lease exists. Release in reverse order.

Preserve the legacy filename algorithm exactly, including the friendly prefix, path normalization, Windows casing, and SHA-256 key. “Equivalent” replacement algorithms risk creating a different mutex. [E4]

Compatibility requires one shared legacy namespace. Different users, data homes, or Windows/Linux path representations can already produce different legacy locks. Do not claim that dual acquisition repairs those old limitations.

The cutover procedure is:

1. Stop existing writers and scheduled invocations.
2. Upgrade and pin the CLI used by terminals, agents, CI, and containers.
3. Preview and apply the explicit workspace-mode configuration.
4. Resume writing with supported binaries.

A minimum-version field or schema bump cannot enforce this against old releases. Their settings reader ignores unknown keys and accepts integer schema versions without rejecting newer ones. [E9]

If the legacy store is inaccessible, explicit cutover may use the workspace lock alone after that operational quiescence. This is a declared end to old-writer compatibility. It must never happen automatically after access denial, contention, or timeout.

Existing data:

- Leave `locks/v1` files untouched. Never rename, delete, or populate them with migration metadata.
- Leave `recovery/v1` bundles and drafts untouched. Do not bulk-move, rewrite, or “upgrade” them.
- Discover local recovery plus the effective legacy store and the default legacy store when different.
- Carry store identity and exact path with each catalogue candidate.
- Legacy artifact deletion requires its matching legacy lock as well as the new workspace lock.
- Preserve malformed, unsupported, mismatched, and unattributed artifacts.
- Keep physical-path identity rules. Workspace moves and different mount spellings do not authorize rebinding old recovery evidence. [E4, E8, E11]

For downgrade, stop writers and restore compatibility mode first. An old CLI will not discover new local bundles. Record that limitation in release notes.

##### 7. Implementation plan

Freeze the compatibility matrix, storage policy, bootstrap effects, and public reporting before implementation.

| Packet | Ownership and changes | Estimate |
|---|---|---|
| Coordination foundation | `WorkspaceLockManager`, `WorkspaceLockLease`, path identity, typed failure facts, composite lease lifetime | 2–3 days |
| Runtime storage | New `WorkspaceRuntimePaths` and typed recovery-store locations. Update Store, Catalogue, Reader, and deletion guards | 3–4 days |
| Command integration | Settings and Install configuration, ignore rules, reserved-path exclusions, all acquisition callers, Status and Doctor reporting | 2–3 days |
| Contracts and qualification | Documentation, release notes, mixed-version fixtures, platform journeys, final integration | 3–5 days |

Estimate **10–15 engineering days**, excluding runner delays or filesystem failures discovered during qualification. Parallel ownership is possible after the shared interfaces are settled.

The acquisition search finds 19 caller files, not necessarily 19 public commands. Include shared Include/Exclude application and both Repair paths in the audit.

There is a bootstrap contract change: creating `.agents/.open-forge` before acquiring its lock may create `.agents`. Install currently promises that `.agents` is its first planned directory effect after taking the external lease. Define coordination-directory creation separately, reconcile the plan’s missing-directory expectation, and report any retained infrastructure accurately. Do not say “nothing changed” when bootstrap created directories. [E13]

Update these accepted sources together:

- `shared-operation-contract.md`: local storage, dual leases, no-effect boundaries, truthful failure wording
- `technical-designs/mutation-and-recovery.md`: exact paths, acquisition order, bootstrap, multi-store observation and deletion
- Install Interface and Behavior: configuration, bootstrap, ignore rules, and cutover
- Status, Doctor, Cleanup, and Repair contracts: store coverage, provenance, preservation, and recovery limitations
- Remaining mutation contracts containing external-only storage promises
- Architecture summaries, Task 85’s control-directory plan, settings schemas, `docs/cli.md`, and installed CLI skill guidance

Keep `.agents/open-forge.lock.json` an ownership record. It must not become the mutex.

Required evidence:

- **Unit:** unchanged legacy key vectors, mode resolution, schema tolerance, typed failures, store selection, and result compatibility.
- **Integration:** partial dual-acquisition release, first-creation races, denied directories, invalid lock occupants, real recovery verification, multi-store cleanup, and reserved-path protection.
- **End-to-end:** actual old/new binaries in both acquisition orders, new/new contention, independent worktrees, sandbox cutover, process death, stale plans, partial application, and retry.
- **No-effect regression:** help, reads, previews, verified no-ops, refusal, and cancellation create no infrastructure.
- **Platforms:** Windows, Linux, and macOS, with managed and NativeAOT qualification across the six shipped targets. Replace F20’s Windows-only process fixture with portable test support. Qualify WSL and network mounts separately. [E7, E15]

Release notes must state that compatibility remains the default, workspace-only mode requires upgraded writers, legacy data is retained, local backups can be removed by cleaning, and downgrade loses local recovery discovery.

#### Risks and what I would not build

The principal risk is a false compatibility promise. A fallback mutex, minimum-version marker, or schema bump cannot stop an unchanged old binary.

Local recovery increases exposure to workspace deletion, forced Git addition, broad cleanup, and shared-checkout readers. Accept that tradeoff explicitly and retain external recovery selection.

I would not build folder locks, automatic stale-lock deletion, PID ownership, distributed leases, automatic rollback, archive migration, or a replay journal.

This was source inspection only. I did not reproduce the reported sandbox incident, build, run tests, or execute Doctor. Git rejected the ownership check, so I did not independently confirm the supplied baseline SHA.

#### Evidence

Prefixes below expand repository-relative citations:

- `F` = `src/cli/framework/OpenForge.Cli.Framework/Framework`
- `O` = `src/cli/operations/OpenForge.Cli.Operations/Commands`
- `C` = `.agents/memory/crystallized/documents/cli`
- `T` = `src/cli/tests`

1. **E1:** `F/Mutation/Locking/WorkspaceLockManager.cs:69`, `:102`, `:119`, `:154`, `:198`. Creation, exclusive handle, failures, and target validation.
2. **E2:** `F/Workspace/CliWorkspaceSelector.cs:22`. Workspace selection uses explicit path or current directory.
3. **E3:** `F/Filesystem/PhysicalPaths/CurrentUserDataHome.cs:15`. Existing environment override semantics.
4. **E4:** `F/Mutation/Locking/WorkspaceLockPathIdentity.cs:17`, `:26` and `F/Workspace/WorkspaceIdentity.cs:28`. Exact legacy identity.
5. **E5:** `O/Index/Shared/Operation/IndexApplicationOperation.cs:56`, `:79`, `:142` and `O/Route/Move/Shared/Planning/RouteMoveEffectPlanner.cs:29`, `:56`. Failure projection, revalidation, and write sets.
6. **E6:** `F/Mutation/Validation/MutationRevalidator.cs:13` and `C/technical-designs/mutation-and-recovery.md:129`. Lease-bound expected-state validation.
7. **E7:** `T/end-to-end/OpenForge.Cli.EndToEndTests/Journeys/F20WorkspaceLockJourneyTests.cs:80`, `:305`. Crash-holder journey and Windows restriction.
8. **E8:** `F/Recovery/RecoveryBundleStore.cs:30`, `:128`, `:160`, `:204` and `F/Recovery/RecoveryBundleCatalogue.cs:22`, `:60`. Preparation and observation.
9. **E9:** `F/Settings/Shared/Serialization/WorkspaceSettingsCodec.cs:18`, `:274`. Unknown-key and version handling.
10. **E10:** `.agents/memory/working/cli-development/tasks/task85-configurable-main-root.md:9`, `:19`. Deferred status and fixed controls.
11. **E11:** `C/shared-operation-contract.md:701`, `:713`, `:745`, `:760` and `C/technical-designs/mutation-and-recovery.md:240`. External recovery, interruption, identity, and preservation.
12. **E12:** `src/cli/output-text/OpenForge.Cli.OutputText/Shared/SharedText.cs:115` and `C/shared-operation-contract.md:523`. Incorrect universal contention wording.
13. **E13:** `C/contracts/install/interface.md:330`, `:511`, `:536`. Ignore ownership, external recovery, and directory bootstrap.
14. **E14:** `C/contracts/shared/result-coordinates/interface.md:160`. Shared schema and command-local compatibility.
15. **E15:** `Directory.Build.props:15` and `src/cli/root/OpenForge.Cli/OpenForge.Cli.csproj:9`. Six targets and NativeAOT publication.

## P2: OS locking engineer

/root - GPT-6 - high

#### Recommendation

Use one persistent lock at `<workspace>/.open-forge-state/workspace.lock`, with recovery bundles in the same reserved runtime directory by default. Keep the complete mutation phase serialized and verify that the filesystem actually enforces the lock before issuing a lease. Support ordinary local filesystems and shared Linux bind mounts, with explicit limits for remote filesystems and Windows–WSL interoperability. Transition through dual locking, followed by an explicit cutover after old writers are retired. Promise that an abandoned lock file cannot block progress, not that the CLI can overcome denied permissions or make several file replacements transactional.

#### Answers

##### 1. Where the lock lives and what it is keyed by

The current lock is already workspace-scoped. Its identity combines a per-user store with a hash of the normalized physical workspace path [E1–E3]. Consequently, different users, data-home overrides, hosts and container mount spellings can select different locks for the **same files**. That can fail silently.

Use a fixed workspace-relative pathname. The shared filesystem object becomes the coordination identity. Do not include the username, PID, Git branch, absolute-path hash or protocol version in its name. Separate worktrees get separate objects. Alternate paths coordinate when they reach the same object.

Prefer a reserved `.open-forge-state/` directory outside the knowledge root. `.agents/` is unsuitable as a universal sandbox solution. Current Codex documentation says existing `.agents/` and `.git` paths remain protected inside writable roots. Writes to the actual settings and ownership files still require appropriate host permissions. Relocating runtime state cannot grant those permissions. [OpenAI sandbox documentation](https://learn.chatgpt.com/docs/agent-approvals-security)

| Environment | Actual guarantee and limitation |
|---|---|
| Windows, local NTFS/ReFS | `FileShare.None` uses enforced sharing restrictions on the sentinel. Conflicting read/write/delete opens fail while the handle remains open. This does not lock other workspace files. Metadata-only access is not universally prohibited. [CreateFile semantics](https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-createfilew) |
| Linux, ordinary local filesystem | .NET uses nonblocking exclusive `flock`. Cooperating holders exclude each other. Processes ignoring the advisory lock can still modify or unlink the sentinel and target files. |
| macOS, ordinary local filesystem | The same cooperative design applies through `flock`. It is not Windows-style mandatory sharing protection. [Apple flock documentation](https://developer.apple.com/library/archive/documentation/System/Conceptual/ManPages_iPhoneOS/man2/flock.2.html) |
| Linux containers sharing a real bind mount | Infer coordination when both containers reach the same underlying inode through the same kernel. PID namespaces do not create independent file objects. Separate writable layers or separately mounted runtime directories break that premise. |
| Docker Desktop | Host sharing crosses a VM/filesystem bridge. Do not transfer the native Linux bind-mount guarantee without testing that implementation. [Docker bind-mount documentation](https://docs.docker.com/engine/storage/bind-mounts/) |
| WSL | Linux-only access to the distribution filesystem follows Linux semantics. `/mnt/c` and other host-drive mounts require separate qualification. A pathname beginning `/mnt` alone does not identify its filesystem. Do not promise exclusion between Windows `CreateFile` sharing and Linux `flock`. Microsoft recommends keeping Linux projects in the Linux filesystem. [WSL filesystem guidance](https://learn.microsoft.com/en-us/windows/wsl/filesystems) |
| NFS | Modern Linux commonly translates `flock` into whole-file byte-range locking. Exclusive locking requires write access. Mount options can restrict locking to one client, producing silent cross-host failure. |
| SMB/CIFS | Windows sharing enforcement, Unix byte-range locks and server behavior must interoperate. Linux before 5.5 did not propagate `flock` remotely. Since 5.5 it is commonly translated into SMB byte-range locks, with mandatory effects. Mount options and server implementations still matter. [Linux flock documentation](https://man7.org/linux/man-pages/man2/flock.2.html) |
| Agent sandboxes and CI | Allowed paths and the backing filesystem determine behavior. Claude Code’s shell sandbox normally permits working-directory writes, with protected exceptions. Its current native Windows shell execution is unsandboxed. Other hosts require their own permission qualification. [Claude Code sandbox documentation](https://code.claude.com/docs/en/sandboxing) |

**A successful Unix `FileStream` construction is insufficient evidence.** In the inspected .NET 10 source, failures other than `EWOULDBLOCK`, including unsupported locking and lock permission failures, can be ignored. `DOTNET_SYSTEM_IO_DISABLEFILELOCKING` can also disable locking. NativeAOT does not strengthen these semantics. [Runtime implementation](https://raw.githubusercontent.com/dotnet/runtime/v10.0.0/src/libraries/System.Private.CoreLib/src/Microsoft/Win32/SafeHandles/SafeFileHandle.Unix.cs)

After opening the sentinel, attempt a second independent, non-truncating exclusive open while retaining the first. Issue a lease only when the second receives a recognized contention error. Success means locking is unenforced. Any unrelated error means enforcement is unverified.

This BCL-only check detects important silent failures. It does **not** prove cross-host propagation, Windows–Unix interoperability or survival of server failover. Initially qualify concurrent writers on local filesystems only. Remote and bridged filesystems need topology-specific tests and external single-writer scheduling until qualified.

Reject the data folder as primary coordination because its namespace is user-dependent. Reject the Git directory because it can be outside the worktree, protected, absent or shared across unrelated worktrees.

##### 2. Granularity

Keep one lock per selected workspace, held from final revalidation through recovery preparation, application, verification and recovery cleanup.

Index iterates a multi-file update plan. Route Move combines reference edits, navigation changes, source deletion and possible settings changes [E6–E7]. Shared ownership and settings also create dependencies beyond the named route. Folder or route locks would need read-set tracking, escalation and deterministic multi-lock ordering to remain correct.

Planning and prompting stay outside the lock. Existing no-op and dry-run paths already support that boundary [E6]. Revalidate inputs that determine the write set, not only files eventually written.

Expected-state checks do not replace exclusion. Two writers can validate the same old bytes and then both replace them. Atomic rename protects one replacement, not a multi-file transaction.

Independent worktrees remain parallel. Nested workspaces are independent only when their effects do not overlap. If a parent operation reaches into a child workspace, use a common selected workspace root or reject that overlap. Distinct root strings alone cannot establish independence.

##### 3. Never stuck

The literal requirement is impossible for an unwritable filesystem. The useful guarantee is **no stale lock ownership and no false contention diagnosis**.

| Condition | Behavior and next action |
|---|---|
| Ordinary leftover sentinel | Reopen and reuse it. Existence, age and contents do not establish ownership. Never require deletion. |
| Killed process | Local OS handle cleanup releases exclusion once all owning handles are gone. Retry normally. Do not inherit the handle into children. |
| Live holder | Return `busy`, with the exact lock path. Retry after that writer exits. |
| Half-applied operation | Release the lock, preserve recovery evidence and plan the next invocation from current files. Report actual inconsistencies. Do not create a permanent “dirty workspace” gate or claim automatic rollback. |
| Denied access or read-only mount | Name the inaccessible path and cause. Request the necessary host permission or use a writable checkout. Do not label it another writer. |
| Unenforced or unverified locking | Stop before target effects. Use a qualified filesystem. Never fall back silently to another lock location. |
| Directory or unsafe link occupies the sentinel path | Preserve it and report the obstruction. Repair that exact path while writers are stopped. |

The current manager rejects nonzero sentinel contents [E1]. For the new sentinel, retain ordinary-file validation but make bytes irrelevant and never truncate them.

F20 contains Windows crash-and-retry coverage, but it does not prove crash consistency of a real operation interrupted between effects [E5]. The design explicitly disclaims crash/power-loss durability and replay [E8].

Never unlink a live sentinel. On Unix, another process can recreate its pathname and acquire a different inode while the original writer continues.

##### 4. Recovery bundles

Default to:

`<workspace>/.open-forge-state/recovery/v1/<existing-workspace-key>/`

Retain the existing manifest, verified draft-to-final publication and guarded deletion [E8–E9]. Moving only the lock would leave replacement commands failing against the inaccessible profile recovery store.

Use one resolver for writers and observers. Status, Doctor and Cleanup inspect the local store, the configured external store and accessible legacy locations without creating them. Report unavailable stores separately from empty stores.

Keep v1 identity checks. They are path-derived, so workspace moves and different container mount spellings can leave bundles under other keys. Report those buckets as preserved, unmatched evidence. Do not silently adopt or delete them. Portable recovery identity is a separate change.

Before writing user-file payloads:

- Establish an internal `.gitignore` excluding runtime contents.
- Teach Install’s bounded ignore-section parser about the reserved directory, then add the root exclusion during the compatible rollout.
- Exclude the directory explicitly from Open Forge scans, indexing, route inclusion and ordinary removal.
- Preserve appropriate permissions. Git-ignore is neither confidentiality nor access control.

Install’s current parser accepts a closed set of patterns, so inserting a new pattern without updating it is unsafe [E10].

Ordinary `git clean -fd` respects ignore rules. `git clean -fdx` and `-fdX` can delete these bundles. Force-adding files can also defeat ignore protection. Document an explicit clean exclusion and offer external recovery storage for users requiring survival beyond workspace deletion. Never run destructive cleaning concurrently with writers. [Git clean](https://git-scm.com/docs/git-clean), [Git ignore](https://git-scm.com/docs/gitignore)

##### 5. Configuration and reporting

The primary lock location must not depend on an environment variable.

Retain absolute `OPENFORGE_DATA_HOME` for explicit external recovery storage and the legacy compatibility lock. Preserve rejection of relative values [E3]. Do not silently redirect an explicitly selected but inaccessible store.

Expose these typed facts consistently in text and JSON:

- Selected workspace and exact coordination path.
- Coordination mode, including legacy compatibility.
- `busy`, `access-denied`, `invalid-target`, `locking-unenforced`, `io-error` or `cancelled`.
- Selected recovery location and each observed store’s availability.

For example:

> Cannot open workspace coordination file `<path>`: access denied. No target files changed. Allow writes to this path in the execution environment, then retry.

Read-only Status must not probe writable access by creating a file. Report `not tested` where necessary. The mutation’s actual acquisition establishes capability.

Task 85 retains `.agents/open-forge.json` and `.agents/open-forge.lock.json`, but its record says **deferred**, contrary to the brief’s “next” description [E11]. This change should not depend on its delivery.

##### 6. Migration and mixed versions

There is no transparent universal bridge to an old executable that knows only its own per-user path.

Provide two explicit modes:

1. **Bridge mode during transition.** Acquire the workspace sentinel, then the exact legacy lock, in that order. Hold both through the entire mutation. Release the first if the second fails. Require all old writers to use the same legacy data home and workspace identity.
2. **Workspace mode after cutover.** Once old writers are stopped and executable versions are pinned across agents and CI, persist the explicit selection and use only the workspace sentinel.

Existing workspaces default to bridge mode during the transition. New installations can select workspace mode as part of setup. Never select it automatically because the legacy directory is inaccessible.

An inaccessible legacy store therefore requires a deliberate cutover, or access to the shared legacy store. This is an unavoidable compatibility boundary. “Try both, ignore access denied” would silently permit concurrent old and new writers.

A new settings field or schema number cannot fence old programs that ignore unknown configuration. Neither can a local marker they never read.

Leave existing `locks/v1` files alone. Leave existing recovery bundles in place and observe them through the legacy reader. Delete eligible bundles only through explicit guarded cleanup. Do not bulk-migrate payloads or remove old lock files during ordinary commands.

##### 7. Implementation plan

Freeze the guarantee and migration modes first. Then assign these bounded packets:

| Owner | Scope | Estimate |
|---|---|---:|
| Coordination | `WorkspaceLockManager`, path identity, lease/result models, `WorkspaceRuntimePaths`, enforcement probe, failure classification and dual acquisition | 3–4 days |
| Recovery and integration | Recovery resolver/catalogue/deletion changes, scan exclusions, Install ignore support, configuration and command mappings | 3–4 days |
| Evidence and documentation | Published-process journeys, platform qualification, contracts, reporting and sandbox guidance | 4–6 days |

Allow **10–14 engineering days**, with integration dependencies limiting parallel work. Network-server qualification needs additional environments and time.

Required evidence:

- **Unit:** deterministic path selection, mode selection, error mapping, recovery-store discovery and ignore parsing.
- **Integration:** two independent handles, concurrent first creation, ordinary leftovers, nonzero contents, denied directories, unsafe paths, cancellation and rejection when locking is disabled.
- **End-to-end:** two real processes contending, killed holder and retry, independent worktrees, shared checkout serialization, different data homes and old/new bridge contention.
- **Recovery:** interruption between effects, retained bundles, unavailable stores, no automatic replay, legacy discovery and cleanup guards.
- **Sandbox:** denied profile with allowed workspace, protected `.agents`, read-only workspace and native container bind mounts.
- **Release:** managed and NativeAOT execution on Windows, Linux and macOS for x64 and arm64. Record the actual runtime pack because SDK roll-forward is enabled [E12].

Update the mutation design, shared operation contract, Install, Status, Doctor and Cleanup contracts, affected command findings, public CLI documentation and skill guidance.

#### Risks and what I would not build

A local probe cannot certify distributed locking. Network partition, server restart or lost remote state can invalidate exclusion while a managed handle still appears open. The existing lease checks handle state, not remote lock ownership [E4].

Workspace-local recovery shares the workspace’s deletion and storage failure domain. Moving it inward is an explicit tradeoff for sandbox usability.

I would not build PID expiry, stale-file deletion, directory leases, heartbeat stealing, per-file lock graphs, automatic rollback or a distributed lock service. I would not replace `flock` with byte-range locking merely to obtain a stricter syscall result, since that changes interoperability. No native shim is necessary for the proposed local guarantee.

#### Evidence

Paths below are relative to the repository. Prefixes are:

- `F` = `src/cli/framework/OpenForge.Cli.Framework/Framework`
- `O` = `src/cli/operations/OpenForge.Cli.Operations/Commands`
- `D` = `.agents/memory/crystallized/documents/cli`

| Reference | Code or contract |
|---|---|
| E1 | `F/Mutation/Locking/WorkspaceLockManager.cs:65`, `:102`, `:134`, `:198` — store creation, exclusive open, failure classification and zero-byte rejection |
| E2 | `F/Mutation/Locking/WorkspaceLockPathIdentity.cs:17`, `:26` and `F/Workspace/WorkspaceIdentity.cs:28` — legacy pathname and path hash |
| E3 | `F/Filesystem/PhysicalPaths/CurrentUserDataHome.cs:15` — absolute override and unavailable relative value |
| E4 | `F/Mutation/Locking/Models/WorkspaceLockLease.cs:61`, `:78` — handle-state ownership and disposal |
| E5 | `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/Journeys/F20WorkspaceLockJourneyTests.cs:80`, `:305` — crash journey and Windows-only boundary |
| E6 | `O/Index/IndexOperation.cs:71` and `O/Index/Shared/Operation/IndexApplicationOperation.cs:42`, `:79`, `:142` — no-op boundary, lease, revalidation and multi-file application |
| E7 | `O/Route/Move/Shared/Planning/RouteMoveEffectPlanner.cs:29`, `:56`, `:72` — settings, reference/navigation composition and deletions |
| E8 | `D/technical-designs/mutation-and-recovery.md:129`, `:140`, `:247`, `:427`, `:434` — expectations, preparation, retained state and deletion |
| E9 | `F/Recovery/RecoveryBundleStore.cs:30`, `:128`, `:160` and `F/Recovery/RecoveryBundleCatalogue.cs:22` — storage resolution, verification, publication and observation |
| E10 | `O/Install/Shared/Configuration/InstallIgnoreSection.cs:12`, `:26`, `:42` — owned section and closed pattern parser |
| E11 | `.agents/memory/working/cli-development/tasks/task85-configurable-main-root.md:9`, `:20` — deferral and fixed control files |
| E12 | `global.json:3` and `Directory.Build.props:3`, `:14` — SDK selection, target framework and six NativeAOT RIDs |
| E13 | `src/cli/output-text/OpenForge.Cli.OutputText/Shared/SharedText.cs:115` — misleading contention message |

Read-only inspection found 19 acquisition-bearing operation files. No files were edited, builds or tests run, or Doctor invoked. Platform guarantees above derive from inspected source and linked primary documentation, not new execution evidence.

## P3: Contrarian minimalist

/root - GPT-6 - reasoning level unreported

#### Recommendation

Keep one workspace lock around application, and fix storage placement before changing lock duration. Use one fixed workspace-local coordination file for every new CLI process, with automatic local recovery storage when the default data folder is unwritable. Retain planning outside the lock and expected-state checks inside it. Remove blanket “recovery artifact exists” barriers, while preserving checks on actual workspace state. Support older writers through an explicit compatibility bridge, and acknowledge that sandbox-only writes and unrestricted legacy concurrency cannot both be guaranteed.

#### Answers

##### 1. Where the lock lives and what it is keyed by

Use `<workspace>/.open-forge-state/mutation.lock`. The directory containing that file establishes its scope. Retain the normalized physical workspace key for lease validation and recovery attribution.

Keep the existing zero-byte file and exclusive open-handle mechanism. Its lifetime already follows the handle, rather than file existence. There is no stale-lock cleanup problem to solve. [E1]

Every new writer uses this same pathname, regardless of its account, data-home override, or recovery destination. **Do not implement “try the profile lock, then try a local lock.”** Two processes with different permissions would then acquire different locks successfully.

I prefer a separate runtime directory over `.agents/` because first Install must coordinate before `.agents/` exists. It also avoids coupling coordination to Task 85’s knowledge-root migration. Reserve the directory against route inclusion, source scans, package installation, and ordinary Remove.

- **Sandbox:** coordination stays within the writable workspace.
- **Separate worktrees:** each has its own file. A repository-wide lock would unnecessarily serialize them.
- **Containers and CI:** processes sharing the actual workspace must share its coordination file. Separate checkouts remain independent.
- **WSL and network drives:** a common pathname alone does not prove interoperable locking. Qualify the filesystem and execution combination before supporting concurrent writers. Otherwise use separate checkouts or one writer environment.

The current key hashes the physical path with Windows-specific case normalization. It is not a universal identity across host mount namespaces. [E2]

##### 2. Granularity

Keep one lock per workspace. Do not introduce folder, route, or file locks.

Index already receives a completed plan before acquiring its lease. Under that lease it verifies inputs, revalidates targets, prepares recovery, applies changes, verifies the result, and deletes its recovery bundle. [E3] Route Move rebuilds its entire plan under the lease. Its effects combine reference edits, navigation changes, source deletions, and potentially settings changes. [E4]

Those are poor candidates for folder partitioning. Even a command that does not rewrite ownership can depend on ownership remaining unchanged. Index’s ownership check is an example. [E3]

The attractive alternative is:

1. Plan and prepare immutable bytes without locking.
2. Acquire the workspace lock.
3. Revalidate every relevant input.
4. Publish all effects and release.

That is plausible, but it does not make the publication section cheap automatically. Route Move currently repeats substantial semantic planning under the lock. Replacing that with target hashes alone would omit changes to discovered references and other planning inputs. [E4]

Nor can hashes replace mutual exclusion. Two writers can both validate the same bytes before either overwrites the file. The current applier performs a separate revalidation followed by `File.Move`, rather than an atomic conditional replacement. [E5]

Moving recovery preparation outside the lock adds another problem. Cleanup can delete recognized drafts and verified finals. Its current synchronization assumes the writer holds the workspace lease while preparing them. [E6] Fixing that requires private staging, another lifetime mechanism, or changed cleanup semantics.

Measure contention before paying those costs. The reported failure concerns writable storage, not excessive lock duration.

##### 3. Never stuck

The useful guarantee is **no stale ownership inferred from an artifact**. An unconditional promise that every damaged or unwritable workspace remains writable would be false.

| Condition | Proposed behavior and next action |
| --- | --- |
| Ordinary leftover lock file | Reopen it normally. No deletion or unlock command. |
| Killed writer | The next process acquires after the OS releases the handle, then plans from current files. Existing Windows crash coverage tests this behavior. [E7] |
| Live writer | Report contention only when established. Retry after it finishes. |
| Profile storage denied | Use local recovery storage and the canonical local lock, subject to the legacy transition below. |
| Coordination directory denied or occupied by an unsafe object | Name the exact path and failure. Restore access or resolve the collision. Never silently select another mutex. |
| Half-applied operation | Preserve recovery evidence, inspect current state, and produce a fresh plan. Report affected inconsistent targets. Restore from the bundle manually or use an applicable existing explicit recovery operation. |
| Residual recovery artifact | Report it without treating its mere existence as a workspace-wide veto. |

That last row requires a real behavior change. Route Move currently rejects preparation whenever its recovery catalogue contains **any** candidate. [E8] Changing only the mutex location can therefore leave the user blocked by an unrelated old ZIP.

Replace such blanket checks with current-state validation. Preserve residual bundles and report warnings. Keep operation-specific blocks where current ownership, settings, paths, or an explicitly selected recovery operation cannot be validated. Do not infer successful completion or safe restoration from a bundle’s existence.

The current design explicitly provides no replay journal or automatic rollback. Preserve that boundary. [E9]

##### 4. Recovery bundles

Keep the existing external store as the preferred destination. Add:

`<workspace>/.open-forge-state/recovery/v1/<workspace-key>/`

Use the local destination when the **default** external store cannot be created or written. An explicitly configured invalid or unwritable `OPENFORGE_DATA_HOME` should produce an actionable configuration failure rather than being silently ignored.

Select one destination before creating the operation’s draft. Do not switch destinations after a partial write. Preserve the existing draft verification, same-directory publication, and final verification. [E10]

Status, Doctor, and Cleanup must enumerate both known locations without creating either. Their records must carry the actual store and path. Guarded deletion must use that recorded location rather than resolving a new default. Preserve unknown and malformed artifacts.

For local storage:

- Establish a self-ignore file before writing any recovery payload.
- Add `/.open-forge-state/` to Install’s existing bounded Git-ignore section.
- Exclude the directory explicitly from Open Forge scanning and maintenance.
- Never delete its coordination file through Cleanup.
- Explain that bundles may contain private and uncommitted user files.

Ignoring the directory does **not** protect it from `git clean -fdx` or `git clean -fdX`. Users needing backups to survive those commands must choose external storage or exclude this directory. Concurrent cleaning of the coordination directory is outside the supported writer model. [Git clean documentation](https://git-scm.com/docs/git-clean)

There is another correction to the brief: Cleanup does not prove that a bundle represents a completed operation. It accepts verified finals and recognized incomplete drafts. [E6] Preserve that distinction in reporting.

##### 5. Configuration and reporting

Keep `OPENFORGE_DATA_HOME` for portable external recovery storage and legacy compatibility. It must no longer relocate the new canonical mutex.

Add one explicit workspace coordination mode:

- **Legacy bridge:** acquire both the local mutex and the configured legacy mutex.
- **Workspace:** acquire the local mutex only.

Existing workspaces default to bridge mode until deliberately upgraded. Fresh installations may select workspace mode directly. Expose the choice through Install configuration and persist it in the fixed control settings. Do not add configurable lock paths or lock granularity.

Status should report:

- Coordination mode and canonical lock path.
- Configured legacy bridge location.
- Known recovery locations and unreadable locations.
- Residual bundles.
- Local recovery’s exposure to destructive Git cleaning.

Status remains an observer. It must not claim that a future lock acquisition or write will succeed.

Preserve failure facts through the command mappings. Today Index collapses access denial, input/output failure, and unsupported storage into one finding, while shared wording asserts another command holds the lock. [E11] Say “access denied” when known. For an ambiguous open failure, report that the coordination file could not be opened and contention is unconfirmed.

The current absolute-only override behavior is already implemented, despite the design document still saying there is no fallback or override. Reconcile that documentation. [E12]

##### 6. Migration and mixed versions

Mixed-version support requires a shared synchronization primitive. An older executable knows only its existing data-home lock.

In bridge mode, new writers acquire:

1. The canonical local mutex.
2. The existing legacy mutex at the exact old path.

Hold both through recovery preparation, effects, verification, and recovery deletion. Release the local lease if legacy acquisition fails. Every new process uses the same order. An old process holding only the legacy mutex cannot form a cycle.

Leave existing `locks/v1` files untouched. Continue observing existing `recovery/v1` bundles. Do not rewrite their manifests or relocate them automatically.

**Do not skip the legacy mutex when access is denied.** An unsandboxed older writer could still hold or acquire it.

For a sandbox that cannot access the legacy mutex, the transition is explicit:

1. Stop legacy writers for that workspace.
2. Upgrade its launchers and agents.
3. Select workspace coordination mode.
4. Resume writers using the new CLI.

The configuration change itself uses the canonical local lock. It cannot establish that an inaccessible legacy writer has stopped. That prerequisite belongs to the operator or launcher.

This is a deliberate compatibility boundary. An old binary can ignore the new setting, so mixed execution after opting out of the bridge is unsupported. No marker, timeout, PID check, or expected-state revalidation repairs that gap.

Bridge compatibility also requires agreement on the legacy data home. The old implementation already permits different overrides to produce different mutexes. [E12]

##### 7. Implementation plan

Freeze the location, transition, and residual-artifact policies before implementation. Then use four bounded packets.

| Packet | Ownership and changes | Estimate |
| --- | --- | ---: |
| Coordination | `WorkspaceLockManager`, path identity, lease and result models. Add canonical workspace paths, compound lease ownership, and preserved failure details. | 2 days |
| Recovery | Store resolution, catalogue, deletion guard/session, and operational contributor. Add local fallback, explicit store identity, and dual-location observation. | 2–3 days |
| Command integration | Coordination configuration, reserved runtime directory, ignore handling, lock findings, and replacement of blanket residual-catalogue barriers. | 2–3 days |
| Contracts and qualification | Architecture, mutation/recovery design, affected command contracts, help, schemas, snapshots, and platform journeys. | 2–3 days |

Estimate **8–11 engineering days**, including integration and focused review. Re-estimate if arbitrary cross-host filesystem interoperability becomes a requirement.

Keep new types narrow. A `WorkspaceCoordinationPaths` owner and a typed recovery-location result are sufficient. Do not introduce a transaction framework.

Required evidence:

- **Unit:** mode selection, deterministic locations, override precedence, failure projection, and catalogue merging without invented completion states.
- **Integration:** same-workspace exclusion despite different data homes, independent workspaces, real permission failures, both-store discovery and cleanup, first Install, reserved-path collisions, stale plans, and retained artifacts that no longer impose blanket blocks.
- **End-to-end on Windows, Linux, and macOS:** live holders, killed holders, two concurrent commands, main and linked worktrees, nested selected workspaces, workspace-only writable execution, and old/new bridge exclusion in both acquisition orders.
- **Recovery interruption:** termination during draft creation and between target effects. Verify preserved bytes, honest residuals, and a usable fresh invocation.
- **Packaging:** existing managed and NativeAOT gates across the six declared targets. [E13]

Do not count the Windows-only F20 journey as cross-platform evidence. [E7]

#### Risks and what I would not build

I would not build route locks, lock escalation, expiring leases, stale-file deletion, automatic rollback, a replay journal, or a general lock-free transaction engine.

The largest tradeoff is explicit: safe legacy coexistence requires access to the legacy mutex. Workspace-only execution requires an upgrade boundary.

Local recovery trades independence from profile permissions for exposure to workspace deletion. Neither lock relocation nor shorter lock duration provides multi-file crash atomicity.

Task 85’s fixed control locations fit this proposal. Its current task record says it is deferred, although the brief calls it next. [E14]

This was read-only inspection. No builds or tests ran. The reported sandbox incident and universal worktree behavior were not reproduced. Git refused the revision query because of repository ownership, so the brief’s commit identity was not independently confirmed.

#### Evidence

Paths below are relative to the workspace.

- **E1:** `src/cli/framework/OpenForge.Cli.Framework/Framework/Mutation/Locking/WorkspaceLockManager.cs:99` and `Models/WorkspaceLockLease.cs:78` in the same locking directory. Exclusive handle acquisition and disposal.
- **E2:** `src/cli/framework/OpenForge.Cli.Framework/Framework/Workspace/WorkspaceIdentity.cs:28` and `Framework/Mutation/Locking/WorkspaceLockPathIdentity.cs:26` in that framework project. Physical-path hashing and external pathname construction.
- **E3:** `src/cli/operations/OpenForge.Cli.Operations/Commands/Index/Shared/Operation/IndexApplicationOperation.cs:28`, `:79`, `:111`, and `:244`. Plan boundary, input validation, preparation, and deletion under the lease.
- **E4:** `src/cli/operations/OpenForge.Cli.Operations/Commands/Route/Move/Shared/Application/RouteMovePlanRevalidator.cs:20` and `src/cli/operations/OpenForge.Cli.Operations/Commands/Route/Move/Shared/Planning/RouteMoveEffectPlanner.cs:29`. Complete replanning and combined effects.
- **E5:** `src/cli/framework/OpenForge.Cli.Framework/Framework/Mutation/Application/FileChangeApplier.Staging.cs:42` and `:63`. Separate revalidation and overwrite.
- **E6:** `src/cli/operations/OpenForge.Cli.Operations/Commands/Cleanup/Shared/Planning/CleanupPlanner.cs:109` and `src/cli/operations/OpenForge.Cli.Operations/Commands/Cleanup/Shared/Application/CleanupApplicationOperation.cs:23`. Artifact eligibility and held-lease deletion.
- **E7:** `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/Journeys/F20WorkspaceLockJourneyTests.cs:84` and `:305`. Crash retry and Windows restriction.
- **E8:** `src/cli/operations/OpenForge.Cli.Operations/Commands/Route/Move/Shared/Application/RouteMoveRecoveryLifecycle.Preparation.cs:27`. Any catalogue candidate blocks preparation.
- **E9:** `.agents/memory/crystallized/documents/cli/technical-designs/mutation-and-recovery.md:427`. Residual reporting and absence of replay.
- **E10:** `src/cli/framework/OpenForge.Cli.Framework/Framework/Recovery/RecoveryBundleStore.cs:30`, `:125`, `:160`, and `:201`. Store resolution and verified publication. `RecoveryBundleCatalogue.cs:22` in the same directory supplies non-creating observation.
- **E11:** `src/cli/operations/OpenForge.Cli.Operations/Commands/Index/Shared/Operation/IndexMutationMapper.cs:47` and `src/cli/output-text/OpenForge.Cli.OutputText/Shared/SharedText.cs:115`. Collapsed failures and false contention wording.
- **E12:** `src/cli/framework/OpenForge.Cli.Framework/Framework/Filesystem/PhysicalPaths/CurrentUserDataHome.cs:15` and `.agents/memory/crystallized/documents/cli/technical-designs/mutation-and-recovery.md:25`. Implemented override and outdated documentation.
- **E13:** `Directory.Build.props:15` and `src/cli/root/OpenForge.Cli/OpenForge.Cli.csproj:9`. Six runtime targets and NativeAOT.
- **E14:** `.agents/memory/working/cli-development/tasks/task85-configurable-main-root.md:9` and `:19`. Deferred status and fixed controls.
- **E15:** `.agents/memory/crystallized/documents/cli/contracts/install/interface.md:178`. Existing bounded Install-owned Git-ignore section.

## P4: Crash safety and recovery engineer

/root - GPT-6 - high

#### Recommendation

Keep one operating-system lock per physical workspace and move its stable pathname to `.agents/.open-forge/workspace.lock`. Put recovery bundles in the same reserved control area by default, with an explicit external-storage override. Hold the lock through revalidation, recovery preparation, application and verification. Never treat a leftover lock file, draft or final bundle as evidence that a writer remains active. Promise freedom from persistent software lockouts, while stating that arbitrary power-loss recovery, unwritable filesystems and uncoordinated old binaries cannot satisfy an unconditional “never stuck” guarantee.

#### Answers

##### 1. Where the lock lives and what it is keyed by

Use the fixed control directory, independent of the configurable knowledge root. Task 85 preserves that distinction, although its current record says it is deferred, not next in the implementation queue. **[E1]**

The local lock has one fixed pathname. Its identity is the shared physical file, not a hash of the pathname seen inside each container. Keep the existing physical-workspace hash for recovery attribution and legacy lock compatibility. Today, workspace selection uses the explicit directory or current directory and resolves its physical root without consulting Git. **[E2]**

Retain the open `FileStream` with `FileShare.None`. Never unlink, replace, truncate or use `DeleteOnClose` on the lock. Unlike the current zero-length validation, accept any ordinary local lock file regardless of contents. Bytes cannot prove ownership and should not create another persistent blocker. Reject directories, links and unsafe ancestry. **[E3]**

Expected environments:

| Environment | Behavior |
| --- | --- |
| Worktree-only sandbox | Local locking and recovery require no profile-directory write after the coordinated version cutover in answer 6. |
| Separate worktrees or CI checkouts | Separate locks. |
| Shared checkout or container bind mount | One lock when all writers reach the same backing file and lock domain. |
| Native WSL filesystem | Qualify through Linux process tests. |
| Windows and WSL concurrently accessing one checkout | Unsupported until cross-boundary exclusion is demonstrated. |
| Network drive | Require explicit qualification of the filesystem, mount and client combination. Otherwise use a local checkout or externally enforce one writer. |

This qualification matters. .NET 10’s Unix implementation treats file-sharing locks as advisory and can ignore unsupported-lock errors. Successful opening alone does not establish exclusion on every filesystem. [Runtime source](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Private.CoreLib/src/Microsoft/Win32/SafeHandles/SafeFileHandle.Unix.cs)

##### 2. Granularity

Keep workspace granularity.

Index applies an ordered collection of updates under one lease. Route Move combines reference edits, navigation, settings, destination creation and source deletion. Install publishes shared ownership and settings. Folder or file locks would require a complete dependency-locking protocol to protect these operations. **[E4]**

Plan and prompt before acquisition. Under the lease, revalidate both target expectations and the command’s relevant planning inputs. Then prepare recovery, apply, verify and release. Keep dry runs, refusals and verified no-ops free of lock creation.

Expected-state checks remain essential, but they do not make a multi-file commit atomic. A shorter lock around each replacement would allow another command to plan from an intermediate layout.

Separate workspace locks assume disjoint mutation boundaries. Reject plans crossing a detected nested workspace boundary rather than assuming that different workspace roots imply disjoint files.

##### 3. Never stuck

The provable property is:

> Once the owner exits, no Open Forge artifact alone prevents another invocation from acquiring the workspace lock. Every interrupted operation leaves either unchanged protected targets or inspectable residual state. Continuing may require repairing permissions or explicitly reconciling files.

This is a liveness property with recovery instructions. It is not an all-or-nothing transaction guarantee.

Retain the current preparation barrier: protected replacements and deletions require a verified final bundle. The implementation verifies the draft, publishes it, then verifies the final before returning preparation. **[E5]**

| Interruption or obstruction | Proof and next action |
| --- | --- |
| Old ordinary lock file, including nonempty contents | Acquisition depends on the live handle only. Reopen it. No deletion or stale-age decision is needed. |
| Live or hung writer | Return promptly with the lock path and supported cause. Retry after completion, or stop the known writer. Never steal a lease by timeout or PID age. |
| Process killed before recovery preparation | Its handles are released on qualified local systems. Runtime scaffolding may remain. Retry with a new operation ID. |
| Process killed while writing or verifying the draft | The preparation barrier has not opened, so protected target effects have not begun. Preserve the draft as incomplete. A new operation ID avoids collision. |
| Process killed after final publication but before application | A final bundle may exist with unchanged targets. Its existence does not prove application. Replan normally. |
| Process killed during adjacent staging | The target has not yet been replaced. A stage may remain because `finally` did not run. Ignore it for coordination and preserve unowned lookalikes. **[E6]** |
| Process killed between target effects | Some files can be prior and others intended. Preserve the final bundle. Replan only from current facts. For a conflicted move or lifecycle state, stop writers, preserve current files and explicitly restore or reconcile the affected targets. |
| Settings or ownership left inconsistent | Recovery-store discovery must work without parsing those documents successfully. Status reports their paths and available bundles. Restore or reconcile those exact controls before dependent mutations. Never “repair” this by deleting ownership indiscriminately. |
| Process killed after verification | Retained recovery evidence does not block another command. Do not infer unfinished work merely from its presence. |
| Cleanup killed halfway | Some explicitly selected bundles are gone and others remain. The lock releases. A fresh catalogue supports another cleanup attempt. |
| Recovery destination denied or full | Stop before protected target effects and release the lock. Report the destination. Free space or choose a writable external recovery destination, then retry. |
| Local lock directory denied or occupied by an unsafe object | Report the exact obstruction. After stopping writers, restore permissions or preserve and relocate the conflicting object. Do not silently switch lock locations. |
| Power loss | After restart, validate actual files and surviving bundles. Do not infer durable state from a previous successful readback. Damaged or missing recovery data may require an independent backup. |

The process-kill evidence today is narrower than the brief suggests. F20 kills an external Windows lock holder, then retries Index. It does not kill an applying CLI at every effect boundary, and it skips other platforms. **[E7]**

For power-loss risk, add `Flush(flushToDisk: true)` after closing the ZIP archive and before publishing it, and before publishing staged target bytes. That requests flushing intermediate file buffers. It does not establish a durable multi-file transaction or a portable directory-publication guarantee. [FileStream documentation](https://learn.microsoft.com/en-us/dotnet/api/system.io.filestream.flush?view=net-10.0)

The current design explicitly disclaims crash and power-loss durability. Changing the lock location cannot remove that limitation. **[E8]**

##### 4. Recovery bundles

Default to:

```text
.agents/.open-forge/recovery/v1/<workspace-key>/<operation-id>.zip
```

Keep immutable manifests and the draft-to-final verification sequence. Include newly created file identities in bundles for multi-file operations that already need recovery. Route Move currently excludes creates from its recovery targets, which limits evidence for removing partially created destinations. Existing manifest semantics already represent prior-missing creates. **[E4, E8]**

Retain verified finals until explicit Cleanup, including finals that Index currently deletes after verification. This trades storage for a longer recovery window. Do not add a journal or automatic replay.

Reserve the entire runtime subtree from routing, indexing, reference scans, package ownership, ordinary removal and main-root migration.

Before copying user bytes locally, create and verify an internal `.gitignore` containing `*`. Preserve conflicting user content rather than overwriting it. This avoids needing a recovery bundle merely to modify the root ignore file before recovery storage exists. Install should also add the reserved subtree to its existing bounded ignore section. **[E1]**

Ordinary Git addition and cleaning should exclude the area. Forced addition remains possible. `git clean -x` and `git clean -X` can delete ignored recovery files, so they require stopping writers and exporting wanted bundles first. Ignoring files is not backup protection. [Git documentation](https://git-scm.com/docs/git-clean)

Status, Doctor and Cleanup should observe local and configured legacy storage without creating either. Report unavailable stores separately from empty stores. An inaccessible legacy store must not veto local mutation or independently validated local cleanup.

A final means “verified archive,” not “completed operation.” Cleanup currently selects verified finals without proving operation completion. Its output must explain that it discards recovery evidence. **[E9]**

##### 5. Configuration and reporting

`OPENFORGE_DATA_HOME` remains useful for:

- Explicit external recovery storage.
- The legacy lock in compatibility mode.
- Finding existing legacy recovery bundles.

It must never redirect the new local lock. Otherwise two agents with different environments could acquire different locks for the same checkout.

Keep relative overrides invalid, as the current resolver does. Do not silently replace an explicitly requested but unwritable recovery destination. **[E2]**

Report the selected workspace, lock protocol, exact lock path, recovery destination and unavailable historical stores. Status must distinguish observation from a current ownership assertion.

Preserve typed failure causes through command results. For example:

> Cannot open `.agents/.open-forge/workspace.lock`: access denied. No target files changed. Restore write access to this control directory, then retry.

For ambiguous I/O, say the lock could not be acquired without asserting contention. Today, Index maps several failure kinds to one finding, whose shared text asserts that another command holds the lock. **[E10]**

##### 6. Migration and mixed versions

There is an unavoidable compatibility boundary.

An old process opens only its data-folder lock. A new process opening only the local lock cannot exclude it. Neither a new settings key nor a migration marker can change the old binary’s behavior. Different legacy data homes already create different coordination domains. **[E2, E3]**

Ship an explicit transition protocol:

1. Default the transition release to **bridge mode**.
2. Acquire the local lock first, then the exact legacy lock used by old writers.
3. Hold both for the entire operation.
4. If the second acquisition fails, release the first and apply nothing.
5. Offer a proposed `--lock-protocol workspace` mode for a coordinated cutover.

Bridge-mode exclusion follows directly. Two new writers conflict on the local lock. An old writer and a new writer conflict on the legacy lock. Acquisition is immediate and consistently ordered, so bridge acquisition introduces no lock-order deadlock.

Bridge mode requires the same legacy user, data-home and path-identity domain. It cannot protect an unknown old process using another domain.

If a sandbox cannot access the legacy lock, the safe exit is to stop and exclude old writers, pin the new CLI for that workspace, and use workspace mode. Permission failure must never trigger an automatic downgrade. Workspace mode becomes the default only through that documented version transition.

Leave `locks/v1` files untouched. Do not delete or relocate existing `recovery/v1` bundles. Read them in place and preserve original attribution. Workspace moves and alternate historical data homes require explicit discovery scope, not guessed reassociation.

##### 7. Implementation plan

Use four owned packets, with shared contracts frozen first.

| Packet | Ownership and changes | Estimate |
| --- | --- | ---: |
| Lock foundation | `Framework/Mutation/Locking`, new `WorkspaceRuntimePaths`, protocol selection, composite lease and typed acquisition facts. Reserve the fixed control path. | 2–3 days |
| Recovery | `Framework/Recovery`, `FileChangeApplier.Staging.cs`, store selection, multi-store observation, flushing, retained finals and complete file-effect evidence. Update affected command recovery lifecycles. | 3–4 days |
| Integration and reporting | Mutation callers, Status, Doctor, Cleanup, Install ignore handling, scan exclusions, Rendering and OutputText. Recovery observation must survive malformed controls. | 2–3 days |
| Evidence and documentation | Mirrored unit/integration tests, published-process journeys, contracts, mutation design, CLI help and skill references. | 3–4 days |

Estimate **10–14 engineering days**, excluding qualification of additional network or Windows/WSL configurations.

Required evidence:

- Unit tests for path selection, protocol ordering, failure propagation, store association and reserved-path exclusions.
- Integration tests for denied data home, denied local control directory, recovery failures, nonempty locks, unsafe occupants, corrupt controls, malformed bundles and interrupted cleanup.
- Process tests on Windows, Linux and macOS for live exclusion, killed holders, independent worktrees, shared checkout and bridge interoperability with the old executable.
- Boundary interruption tests before and after draft publication, final verification, each target effect and lifecycle publication.
- Recovery checks comparing every affected file against prior, intended and unrelated user bytes.
- Managed and NativeAOT gates across the existing six-platform matrix. **[E11]**

Update the shared mutation contract, Cleanup semantics, recovery retention rules and Install footprint explicitly. Do not describe a kill test as a power-loss test.

#### Risks and what I would not build

Local recovery shares the workspace’s access and deletion risks. Sensitive or irreplaceable recovery copies belong in a restricted external destination and independent backups.

I would not build folder-lock graphs, timestamp-based stale-lock stealing, PID ownership records, automatic rollback, a replay engine or a distributed lock service.

The strongest achievable guarantee is no persistent lockout caused by Open Forge’s own coordination artifacts. Successful writes despite denied permissions, lossless recovery after arbitrary storage failure and exclusion of unmodified old binaries without a shared lock are impossible promises.

#### Evidence

Paths below are relative to the working directory. This was source inspection only. No build, test or Doctor invocation was performed. Git’s ownership check prevented independent verification of the supplied commit identity.

- **E1:** `.agents/memory/working/cli-development/tasks/task85-configurable-main-root.md:10`, `:20`. `.agents/memory/crystallized/documents/cli/contracts/install/interface.md:177`.
- **E2:** `src/cli/framework/OpenForge.Cli.Framework/Framework/Workspace/CliWorkspaceSelector.cs:22`. `Framework/Workspace/WorkspaceIdentity.cs:28` and `Framework/Filesystem/PhysicalPaths/CurrentUserDataHome.cs:17` under the same project.
- **E3:** `src/cli/framework/OpenForge.Cli.Framework/Framework/Mutation/Locking/WorkspaceLockManager.cs:65`, `:102`, `:198`. Its `Models/WorkspaceLockLease.cs:78` releases the handle.
- **E4:** `src/cli/operations/OpenForge.Cli.Operations/Commands/Index/Shared/Operation/IndexApplicationOperation.cs:79`, `:113`, `:142`, `:246`. Under the same Commands root, `Route/Move/Shared/Planning/RouteMoveEffectPlanner.cs:56`, `:77`, `:182`, and `Install/Shared/Planning/InstallEstablishmentPlanner.cs:298`, `:406`.
- **E5:** `src/cli/framework/OpenForge.Cli.Framework/Framework/Recovery/RecoveryBundleStore.cs:69`, `:128`, `:160`, `:204`. `Framework/Mutation/Application/FileChangeApplier.cs:280` requires preparation for existing targets.
- **E6:** `src/cli/framework/OpenForge.Cli.Framework/Framework/Mutation/Application/FileChangeApplier.Staging.cs:21`, `:63`, `:96`, `:128`.
- **E7:** `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/Journeys/F20WorkspaceLockJourneyTests.cs:84`, `:97`, `:305`.
- **E8:** `.agents/memory/crystallized/documents/cli/technical-designs/mutation-and-recovery.md:140`, `:167`, `:370`, `:427`.
- **E9:** `src/cli/framework/OpenForge.Cli.Framework/Framework/Recovery/RecoveryBundleCatalogue.cs:22`, `:76`. `src/cli/operations/OpenForge.Cli.Operations/Commands/Cleanup/Shared/Planning/CleanupPlanner.cs:109` and `Shared/Application/CleanupApplicationOperation.cs:23`.
- **E10:** `src/cli/operations/OpenForge.Cli.Operations/Commands/Index/Shared/Operation/IndexMutationMapper.cs:47`. `src/cli/output-text/OpenForge.Cli.OutputText/Shared/SharedText.cs:115`.
- **E11:** `Directory.Build.props:15`. `src/cli/root/OpenForge.Cli/OpenForge.Cli.csproj:9`.

## P5: Agent workflow realist

/root - GPT-6 - high

#### Recommendation

Put one persistent OS-handle lock at `.agents/.runtime/workspace.lock`, shared by every new CLI writing to that workspace. Keep one writer per workspace and put recovery bundles in the same local runtime directory by default. This removes the profile-directory dependency for ordinary sandboxed agents without requiring flags. Preserve expected-state checks and report contention, permissions, unsupported locking, and incomplete changes separately. Introduce the change through a dual-lock bridge release, because unrestricted old/new coexistence and inaccessible legacy locks cannot both be supported safely.

#### Answers

##### 1. Where the lock lives and what it is keyed by

Use the selected physical workspace’s fixed control directory:

```text
.agents/.runtime/workspace.lock
.agents/.runtime/recovery/v1/<workspace-key>/
```

The shared lock file itself is the coordination identity. Do not include the user, process, container, branch, or absolute mount spelling in its filename. Keep its pathname stable across future format versions.

Today, workspace selection uses the current directory or explicit `--workspace`. It does not climb to a parent workspace or select the Git common directory. The existing external lock filename hashes that selected physical root. Separate worktrees therefore receive separate identities. This is verified selection behavior, not an executed claim about every command. [E1]

| Agent environment | What it hits today | Proposed behavior |
|---|---|---|
| Separate Git worktrees | Separate lock keys, provided the data home is writable | Independent local locks |
| Shared checkout, same data home | One lock, immediate acquisition failure | One lock, with bounded waiting for proven contention |
| Shared checkout, different homes or overrides | Different lock files can fail to coordinate | Same workspace file coordinates writers |
| Worktree-only sandbox | Profile access can prevent mutation | Runtime storage stays inside the writable worktree |
| Containers and CI | Coordination depends on homes and path spellings | Shared workspace mounts share the lock object. Separate checkout copies remain independent |
| WSL or network storage | Existing code does not establish cross-client locking guarantees | Support depends on the filesystem exposing one effective locking domain |

Do not put the lock in Git administration. Linked-worktree metadata may be outside the writable worktree, and a repository-wide lock would serialize independent worktrees.

Do not promise network or Windows/WSL interoperability from `FileShare.None` alone. The .NET 10 Unix implementation uses advisory locking and ignores some unsupported-lock errors. Add a second-open exclusion check before granting a lease. It detects locally ineffective locking, but cannot prove exclusion across separate clients. Qualify those combinations explicitly. [Runtime source](https://raw.githubusercontent.com/dotnet/runtime/v10.0.0/src/libraries/System.Private.CoreLib/src/Microsoft/Win32/SafeHandles/SafeFileHandle.Unix.cs)

Task 85 is recorded as **deferred**, contrary to the brief’s “next” wording. Its fixed `.agents` control files still make this location appropriate. [E11]

##### 2. Granularity

Keep one workspace lease across revalidation, recovery preparation, application, verification, and operation-owned recovery cleanup.

The real write sets do not support cheap folder isolation. Index can rewrite multiple generated regions. Route Move combines reference rewrites, navigation changes, creates, replacements, deletes, and sometimes settings changes. Index also validates ownership-related inputs under the lease. [E6, E7]

Folder locks would require discovering and locking the complete dependency set, including shared controls. File locks would require ordering, alias handling, and protection against newly discovered references. Those costs do not improve the common agent workflow enough.

Plan and prompt outside the lease. Preserve the existing lock-free dry-run and verified-no-op paths. [E6]

Expected-state checks complement the lease. They cannot make a sequence of file replacements atomic. Releasing the lease between replacements would expose partially applied operations to another cooperating writer.

Separate worktrees remain the straightforward way to run independent writing agents concurrently.

##### 3. Never stuck

Define the promise precisely: **no stale-file unlock procedure and no persistent “workspace read-only” state**. It cannot mean successful writes despite denied filesystem access or unresolved content conflicts.

The current lock already survives ordinary process termination correctly. F20 exercises a killed lock holder on Windows. It does not prove recovery after killing a real writer halfway through application. [E2, E4]

| Situation | What the agent sees and does |
|---|---|
| Ordinary leftover lock file | Acquisition succeeds. No warning or deletion |
| Non-empty local lock file | Ignore its contents. Ownership comes from the handle |
| Live competing writer | Wait up to five seconds, cancellably. Then report the workspace, lock path, and retry instruction |
| Killed holder | The next acquisition succeeds without cleanup |
| Changed planning input | Report `target-changed`, the affected path, and the command to rerun |
| Partial application | Report observed residual state and the recovery path. Require a fresh plan or supported explicit recovery |
| Access denied or read-only mount | Name the failed path and denied operation. Ask for workspace permissions or a writable checkout |
| Directory or symlink at the reserved lock path | Preserve it and report the obstruction. Require inspection after writers stop |

The non-empty-file rule changes current behavior deliberately. Today, the manager rejects nonzero length even after obtaining the handle. That can create a persistent operational obstruction. [E2]

Open the local lock without truncating or deleting it. Handle simultaneous first creation without treating an ordinary creation race as corruption.

A crash can still leave mixed content. The existing design has no replay journal and promises no crash or power-loss durability. A draft is not a verified backup. The CLI must never tell an agent that deleting a lock or recovery ZIP repairs those changes. [E8, E9]

##### 4. Recovery bundles

Default new recovery writes to the local directory above. Otherwise fixing the lock merely exposes the next profile-access failure.

Preserve the existing ZIP format, exact prior bytes, verified draft-to-final publication, and preparation-before-target-effects ordering. Keep the physical-path key for v1 compatibility. [E8]

Use one shared locator for writers, Status, Doctor, Repair, and Cleanup. It should enumerate:

- Local bundles for the current workspace key.
- Other local buckets as foreign-origin retained evidence.
- Recognizable legacy bundles in the selected user data home.

A different container mount spelling must not make retained local archives invisible. Foreign-origin bundles remain preserved. Do not silently reinterpret their v1 workspace identity or grant restoration or deletion authority.

Observers create nothing. An inaccessible legacy store is reported as incomplete coverage, not as an empty store. It should not veto an unrelated new local mutation unless that operation actually requires the missing evidence.

Before writing payload bytes, establish `.agents/.runtime/.gitignore` containing `*`. Also add the runtime exclusion through Install’s owned ignore section. That section currently accepts only known patterns, so its reader and writer must change together. [E10]

Exclude runtime storage explicitly from source inventories, Index, link scans, package projection, root migration, and content removal. Git-ignore rules alone are insufficient for those boundaries.

Recovery copies inherit an appropriately restricted workspace access policy. Do not broaden permissions automatically.

Ordinary `git clean -fd` preserves ignored storage. `-fdx` and `-fdX` can delete it. Document `-e .agents/.runtime/` when preservation is required, and offer external recovery storage for backups that must survive checkout deletion. Never run cleaning concurrently with writers. [Git documentation](https://git-scm.com/docs/git-clean)

##### 5. Configuration and reporting

New installations need no setting. The lock location is fixed and cannot be overridden.

Retain `OPENFORGE_DATA_HOME` for legacy discovery and explicitly selected external recovery storage. It must no longer redirect the new lock. A relative value remains an actionable configuration error when that external store is required. Current resolution already deliberately rejects relative values. [E3]

Preserve each command’s finding code where practical, but carry structured details:

```text
reason, workspace, path, filesystemCause, retryable, nextAction
```

For example:

> Cannot index this workspace. Access was denied opening `<path>`. No workspace content changed. Give this process access to `.agents/.runtime`, then rerun `open-forge index`.

For verified contention:

> Cannot index this workspace after waiting five seconds. Another writer holds `<path>`. Retry when it finishes. Do not delete the lock file.

For ambiguous I/O, name the operation and actual error without asserting another writer exists.

Status should show protocol, lock path, recovery locations, coverage, and retained artifacts. File existence must never be rendered as “locked.”

The brief slightly overstates current failure mapping. There are 19 acquisition-containing operation files, but Index maps invalid lock paths to `workspace-unsafe`. Access, I/O, and unsupported failures collapse to lock-unavailable. [E5]

##### 6. Migration and mixed versions

**There is no safe automatic fallback from an inaccessible legacy lock.** The old process can still acquire that lock while the new process acquires a different local file.

Use two release steps:

1. **Bridge release.** Every writer acquires the legacy lock, then the local lock, and holds both through completion. Any failure releases all acquired handles before returning. All bridge writers use the same acquisition order. Old and bridge binaries remain exclusive when they share the existing legacy coordination domain.
2. **Local release.** After all writers for a shared workspace have reached at least the bridge release, local-only binaries may run alongside bridge binaries. Both acquire the local lock.

The transition cannot cover old binaries using different data homes or different path identities. Those processes already lack a common lock today. [E1, E3]

A sandbox unable to acquire the legacy lock remains blocked **during the bridge stage**. Its error must say:

> Cannot coordinate with older Open Forge versions at `<legacy-path>`. No workspace content changed. Upgrade all writers to the local-lock protocol, or allow access to this legacy lock location.

Once old writers are retired, the local release requires no environment setup.

This rollout condition cannot be enforced by a marker that old binaries never read. If arbitrary pre-bridge binaries must remain free to run concurrently, retain mandatory legacy locking and accept the sandbox limitation. Do not advertise both guarantees.

Leave existing `locks/v1` files alone. They are harmless when unlocked. Do not rename, delete, or repurpose them.

Leave existing `recovery/v1` bundles in place. Discover them through the shared locator and retain existing guarded cleanup semantics. Migration must not erase backups or silently rebind their workspace identities.

##### 7. Implementation plan

Freeze the protocol, migration limitation, bootstrap behavior, and error fields before dividing implementation.

| Packet | Ownership and outcome | Estimate |
|---|---|---:|
| Locking | `WorkspaceLockManager`, path identity, lease, typed failure details, exclusion check, bridge acquisition | 2 days |
| Runtime and recovery | New `WorkspaceRuntimePaths` and `RecoveryStoreLocator`, catalogue and deletion changes, foreign-origin reporting | 2–3 days |
| Command integration | All acquisition consumers, bootstrap handling, reserved-path exclusions, Install ignore rules, Status and wording | 2 days |
| Qualification and documentation | Platform journeys, compatibility fixtures, contracts, public guide and CLI skill references | 2–3 days |

Estimate **8–10 engineering days**, excluding bridge-release adoption time.

Fresh Install needs special attention. Creating `.agents/.runtime` also creates `.agents` when absent. Represent those bootstrap directories as owned setup effects so they do not invalidate Install’s own missing-directory expectations. Bootstrap happens only after real-write admission. Content plans still receive under-lease validation.

Required evidence:

- **Unit:** fixed path selection, reason mapping, store selection, reserved paths, and foreign-origin classification.
- **Integration:** fresh-install bootstrap, concurrent creation, non-empty files, denied access, cancellation, no-op/dry-run purity, recovery publication failure, stale plans, and cleanup exclusion.
- **End-to-end on Windows, Linux, and macOS:** two real writers, holder termination, application interruption, separate worktrees, shared checkout with different data homes, and sandbox-denied profile access.
- **Compatibility:** pre-bridge/bridge and bridge/local pairs. Demonstrate the unsupported pre-bridge/local collision explicitly.
- **NativeAOT:** execute the affected journeys across the existing six-platform matrix. F20’s current Windows-only evidence is insufficient. [E4, E12]

Update mutation/recovery design, shared operation contracts, Install, Cleanup, Status and Doctor contracts, output schemas, public documentation, and both CLI skill copies.

#### Risks and what I would not build

Local recovery trades protection from checkout deletion for reliable access inside sandboxes. Make that visible wherever retained recovery paths are reported.

A local lock cannot make incompatible filesystem locking domains cooperate. Container ownership mismatches remain real permissions failures. Nested workspaces are independently lockable only when mutation targets do not overlap. Reject cross-workspace mutation plans rather than silently assuming independence.

I would not build folder locks, per-file lock graphs, PID expiry, lock stealing, automatic rollback, a daemon, native shims, or a “force unlock” option.

This proposal is based on source inspection. No files were changed, builds or tests run, or Doctor command executed. The historical sandbox incident was not reproduced, and Git’s ownership check prevented confirming the brief’s exact commit identifier.

#### Evidence

Paths below are repository-relative. `F` means `src/cli/framework/OpenForge.Cli.Framework/Framework`, `O` means `src/cli/operations/OpenForge.Cli.Operations/Commands`, and `E` means `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests`.

- **E1:** `F/Workspace/CliWorkspaceSelector.cs:22`, `F/Workspace/WorkspaceIdentity.cs:28`, `F/Mutation/Locking/WorkspaceLockPathIdentity.cs:17` — selected root and external identity.
- **E2:** `F/Mutation/Locking/WorkspaceLockManager.cs:102`, `F/Mutation/Locking/WorkspaceLockManager.cs:198`, `F/Mutation/Locking/Models/WorkspaceLockLease.cs:78` — exclusive handle, length rejection, handle-only disposal.
- **E3:** `F/Filesystem/PhysicalPaths/CurrentUserDataHome.cs:17` — environment override and absolute-path requirement.
- **E4:** `E/Journeys/F20WorkspaceLockJourneyTests.cs:23`, `E/Journeys/F20WorkspaceLockJourneyTests.cs:84` — contention and killed-holder journeys, both Windows-gated.
- **E5:** `O/Index/Shared/Operation/IndexMutationMapper.cs:47`, `src/cli/output-text/OpenForge.Cli.OutputText/Shared/SharedText.cs:115` — failure mapping and misleading contention wording.
- **E6:** `O/Index/IndexOperation.cs:71`, `O/Index/Shared/Planning/IndexPlanBuilder.cs:20`, `O/Index/Shared/Operation/IndexApplicationOperation.cs:64` — no-op boundary, multiple regions, leased validation.
- **E7:** `O/Route/Move/Shared/Planning/RouteMoveEffectPlanner.cs:29`, `O/Route/Move/Shared/Application/RouteMoveApplicationOperation.cs:24` — combined write set and preparation order.
- **E8:** `F/Recovery/RecoveryBundleStore.cs:69`, `F/Recovery/RecoveryBundleStore.cs:128`, `F/Recovery/RecoveryBundleStore.cs:160`, `F/Recovery/RecoveryBundleCatalogue.cs:22` — draft creation, verification, publication, observation.
- **E9:** `.agents/memory/crystallized/documents/cli/technical-designs/mutation-and-recovery.md:427` — residuals, durability limits, no replay, guarded deletion.
- **E10:** `O/Install/Shared/Configuration/InstallIgnoreSection.cs:17`, `.agents/memory/crystallized/documents/cli/contracts/install/interface.md:330` — bounded ignore ownership.
- **E11:** `.agents/memory/working/cli-development/tasks/task85-configurable-main-root.md:9`, `.agents/memory/working/cli-development/tasks/task85-configurable-main-root.md:19` — deferred status and fixed controls.
- **E12:** `Directory.Build.props:15`, `src/cli/root/OpenForge.Cli/OpenForge.Cli.csproj:9` — six targets and NativeAOT publication.

## P6: Fine-grained concurrency advocate

/root - GPT-6 - high

#### Recommendation

Keep one mutation lease per physical workspace, but move its canonical lock file into the workspace’s fixed control directory. Scoped locks have a credible future use for disjoint Index operations, because Index reads shared settings and ownership without rewriting them. They do not yet justify the additional protocol for broad Index runs, Route Move and Install, and they would not solve the sandbox storage failure. Put recovery storage within the writable workspace by default, with explicit external storage available. Introduce this through a dual-lock transition that never silently skips an inaccessible legacy lock.

#### Answers

##### 1. Where the lock lives and what it is keyed by

Use `.agents/.runtime/mutation.lock`, opened exclusively and retained after release. The file’s location identifies the workspace. Keep the normalized physical workspace identity in the lease for validation and reporting. Do not key the mutex filename by an absolute-path hash inside this directory, because different mount paths should still reach the same lock object.

This remains separate from `.agents/open-forge.lock.json`, which records ownership. Keep runtime storage fixed when Task 85 moves the knowledge root. Task 85 explicitly preserves the settings and ownership locations [E11].

The environment contract would be:

| Environment | Behavior |
|---|---|
| Workspace-only sandbox | Coordination and default recovery need only workspace write access after the compatibility cutover. |
| Separate Git worktrees | Each worktree has its own control directory and can mutate independently. |
| Containers sharing a checkout | They must share the runtime directory on the same mounted filesystem. A private overlay for the mutex defeats coordination. |
| WSL and Windows sharing files | The pathname is shared, but cross-environment exclusion needs qualification. Until proved, use separate worktrees or one writer environment. |
| Network filesystem | Support concurrent writers only where cross-client exclusion has been demonstrated. Unsupported locking requires serialized execution or local worktrees. |
| CI | Separate checkout directories remain independent. Jobs sharing a checkout share its runtime directory. |

Do not use the Git common directory. It would serialize otherwise independent worktrees and may lie outside sandbox permissions.

Never fall back between mutex locations based on which directory happens to be writable. Two processes could choose different locks.

##### 2. Granularity

The strongest case for finer locking is narrower than the brief suggests.

| Operation | Actual write set | Concurrency implication |
|---|---|---|
| Index | Generated entrypoint replacements, missing route scaffolding, metadata completion and maintenance replacements. Settings and ownership are read dependencies, not routine output files [E3, E4]. | Two carefully selected, disjoint plans could overlap. Selecting different folders does not prove independence because selection includes parents and siblings. |
| Route Move | Destination creation, source deletion, affected reference documents, navigation files and directory effects. Settings change when link-scope paths need translation. Ownership is inspected and preserved [E5]. | A source-folder lock misses incoming links elsewhere. The broad reference scan also depends on documents that currently need no edit. |
| Install | Payload and generated navigation, host regions, ownership, settings and its bounded `.gitignore` section [E6]. | It changes the shared control state and should remain workspace-exclusive. |

Ownership writers serialize the complete ownership document. Different owners therefore still conflict at one physical file [E7]. Locking only the final ownership publication would be insufficient. Earlier effects could already have used incompatible ownership or settings snapshots.

A defensible future scoped protocol would have:

1. A shared workspace gate for eligible scoped operations and an exclusive gate for Install, Route Move, broad maintenance and Cleanup.
2. Exclusive locks for every output file and shared locks for every relevant read dependency.
3. Directory-range or intention locks where membership matters, including prospective files.
4. One canonical acquisition order, with no in-place upgrades.
5. Complete revalidation after acquisition. If rebuilding changes the resource set, release everything and restart before effects.
6. Locks held through recovery preparation, effects, verification and recovery cleanup.

This could let two independent Index plans proceed together. It also explains why per-file write locks alone are insufficient. A concurrent operation can add a reference or alter directory membership without touching any originally planned output.

Expected-state validation is not an atomic compare-and-replace transaction. Two writers can both validate before either writes. Keep a lease across application. Planning already occurs outside that lease, while Index validates and prepares recovery inside it [E4]. Route Move rebuilds its complete plan under the lease [E5].

**I would not ship finer scoping now.** There is no measured contention evidence here. Measure lock acquisition failures, time held, recovery preparation time and actual read/write overlap in repeated same-checkout workloads. Revisit scoped Index when substantial work repeatedly waits despite disjoint dependencies and outputs. Separate worktrees already provide the more valuable concurrency boundary.

A repository-wide lock earns its place only when an operation actually mutates a resource shared between worktrees. These three write sets do not establish that need.

##### 3. Never stuck

The promise should be **no stale lock state**, not “every mutation always succeeds.”

| Situation | User-visible behavior and action |
|---|---|
| Ordinary leftover mutex file | Reuse it. Existence does not mean ownership. |
| Killed process | The handle is released. Retry with a fresh plan. The existing Windows F20 test exercises this [E2]. |
| Live holder | Report contention when established. Retry after the writer finishes. Never delete or steal its mutex. |
| Half-applied operation | Report current files and retained recovery artifacts. Build a fresh plan. An affected operation may require targeted repair, but do not introduce a workspace-wide “recovery required” latch. |
| Unwritable profile directory | Workspace protocol continues using local storage. Compatibility mode has the explicit limitation in answer 6. |
| Unwritable control directory or targets | Report the exact denied path and operation. Correct permissions or use a writable checkout. Read-only commands remain available. |
| Invalid mutex path, such as a directory or link | Report a path conflict. Repair that exact path after stopping writers. Do not silently relocate the lock. |

The current manager rejects nonzero lock files even though contents do not establish ownership [E1]. For the new mutex, preserve arbitrary bytes in an ordinary file and use only the held handle as authority. Normal creation still produces an empty file. This removes an unnecessary content-based blockage.

Keep existing per-target revalidation and recovery protections. Recovery bundles are not a journal, and a fresh invocation cannot safely infer automatic continuation from them [E9].

No safe design can guarantee writes through denied filesystem permissions, or guarantee consistency after arbitrary external deletion. Those limits must not be mislabeled as another writer holding a lock.

##### 4. Recovery bundles

Default to `.agents/.runtime/recovery/v1/<workspace-key>/`. Retain the existing manifest, prior-byte payloads, verification and draft-to-final publication sequence [E8].

Before writing payloads, create and verify a runtime-owned `.gitignore` containing `*`. Reserve the runtime subtree in Open Forge scanners and mutation destination policies independently of Git. Install and future main-root migration must preserve it.

This avoids making every recovery operation edit the workspace-root `.gitignore`, which already has Install-owned content. It also avoids accidentally routing backup Markdown into context or reference scans.

Local recovery has a real tradeoff:

- Ordinary Git discovery excludes ignored artifacts.
- Force-adding files can still publish them.
- `git clean -fdx` can delete ignored recovery data. Stop writers before destructive cleaning and preserve needed bundles externally. [Git documentation](https://git-scm.com/docs/git-clean)

Offer external recovery storage through `OPENFORGE_DATA_HOME` for users who need it outside checkout deletion. Do not silently switch back to local storage when an explicit external choice fails.

Status, Doctor and Cleanup should use one catalogue that observes local storage and the currently resolved legacy/external store without creating either. Show each artifact’s location and validation state. Cleanup must retain its lease-bound, exact-candidate deletion checks [E10].

Keep historical manifest paths literal. After workspace or mount-path changes, report unmatched recovery buckets without silently rebinding their identity.

##### 5. Configuration and reporting

Normal workspace-protocol operation should need no data-home configuration.

Retain `OPENFORGE_DATA_HOME` for:

- The legacy mutex bridge during transition.
- Explicit external recovery storage.
- Existing isolated test and portable environments.

Preserve rejection of relative values. The code already implements this, although the design document still describes only the special-folder location [E1, E9].

Add one explicit compatibility selection, such as `--lock-protocol workspace-v2`, for a launcher that guarantees all writers have been upgraded. This changes compatibility requirements, not the canonical local mutex path.

Return structured lock failure facts: location, acquisition stage, typed filesystem cause and protocol. Distinguish established contention, access denial, invalid location, unsupported operation and ambiguous I/O. Do not guess that every `IOException` means contention.

For example:

> Cannot acquire the workspace lock at `<path>`: access denied. No target files changed. Make the control directory writable and retry.

Status should show the selected protocol, mutex location and recovery locations. It must not infer a live owner from file existence or claim writability without testing it.

The brief’s finding-code claim needs correction. I found 19 acquisition-bearing operation files, but Install maps acquisition failure to `WorkspaceUnsafe`, while Index maps several failure kinds to `WorkspaceLockUnavailable` [E12]. Update all mappings and their presentation, not just the shared sentence.

##### 6. Migration and mixed versions

During the transition, new writers acquire:

1. The existing legacy data-home mutex.
2. The new workspace mutex.

Acquire both before recovery or target effects. On failure, release any acquired handle. Keep both through verification and cleanup.

This preserves mutual exclusion with an old CLI **when both use the same legacy namespace**. Different user profiles, overrides or incompatible physical-path spellings already prevent that guarantee.

The transition release should default to this bridge. After stopping old writers and upgrading every launcher, explicitly select workspace protocol. All new writers still contend on the same local mutex, including any remaining new bridge-mode processes.

There is an unavoidable boundary: if the legacy mutex is inaccessible and an old writer may still run, the new CLI cannot both proceed safely and promise mutual exclusion. An old binary does not inspect a new protocol marker. An empty legacy directory proves nothing about future writers.

Therefore:

- Never silently skip the legacy mutex after access failure.
- In restricted environments, either establish an upgraded-only writer fleet or provision one legacy namespace accessible to every writer.
- Do not advertise automatic mixed-version compatibility outside those conditions.

Leave existing `locks/v1` files alone. They are reusable handles’ rendezvous points, not stale ownership records. Leave existing `recovery/v1` bundles in place and expose them through the combined catalogue. Do not rewrite manifests or automatically move archives.

##### 7. Implementation plan

Freeze the location, migration and failure contracts first. Then divide ownership into these packets.

| Packet | Files and responsibility | Estimate |
|---|---|---:|
| Lock foundation | `WorkspaceLockManager`, path identity, lease and result models. Add `WorkspaceRuntimePaths` and `WorkspaceLockProtocol`. Implement local acquisition and ordered legacy bridging. | 2 days |
| Recovery and reservation | `RecoveryBundleStore`, catalogue, path identity and deletion sessions. Add explicit storage selection and combined observation. Reserve runtime paths in scanning and lifecycle operations. | 2–3 days |
| Command integration | Acquisition mappings across the 19 callers, structured diagnostics, rendering and OutputText. Preserve no-lock dry runs, refusals and verified no-ops. | 1–2 days |
| Contracts and qualification | Mutation/recovery design, shared operation contract, affected command contracts, CLI documentation and Skill references. Integrate platform evidence. | 2–3 days |

Total: **7–10 engineering days**, excluding a later scoped-lock experiment. Keep one owner for shared lease and storage contracts. Separate implementers can own recovery, command reporting and documentation after those contracts are frozen.

Required verification:

- **Unit:** protocol selection, path identity, error mapping, storage catalogue composition and runtime reservations.
- **Integration:** competing processes, differing data homes, local and legacy recovery, denied directories, invalid paths, cancellation and nonempty mutex reuse.
- **Bootstrap:** fresh Install when `.agents` is absent. Creating coordination infrastructure must not invalidate its planned directory expectations or create install divergence.
- **Command races:** Index against Index, Route Move and Install. Verify changed inputs stop safely and ownership updates are not lost.
- **Recovery failures:** interruption during preparation and between effects. Verify exact residuals and a usable fresh invocation.
- **End-to-end:** genuine Git worktrees, nested worktrees, workspace-only sandbox permissions, old/new contention in both directions and killed holders.
- **Platform:** published-process exclusion and crash-release tests on Windows, Linux and macOS, with the existing x64/arm64 NativeAOT matrix [E13]. F20 currently skips non-Windows platforms.
- **Git and scans:** ignored recovery, ordinary cleanup, destructive-clean limitations and exclusion from Open Forge traversal.

Do not move recovery preparation outside the lease merely to shorten hold time. Cleanup currently relies on that lease to avoid deleting another operation’s active artifacts.

#### Risks and what I would not build

I would not build a general hierarchical lock manager, distributed lock service, PID expiry scheme, lock stealing, automatic rollback or ownership-record sharding for this fix.

The largest compatibility risk is silently treating an inaccessible old lock as absent. The largest local-storage risk is deletion or exposure of recovery copies. Neither is solved by finer granularity.

The scoped-lock opportunity is real, particularly for independent Index plans. It needs measured same-checkout demand and complete dependency accounting before implementation.

This was source inspection only. No files changed, builds or tests ran, or Doctor command executed. Cross-platform mount behavior and the brief’s universal claim about every command in nested worktrees remain unverified. Task 85’s file also currently says “deferred,” rather than “next” [E11].

#### Evidence

Repository-relative prefixes:

- `F` = `src/cli/framework/OpenForge.Cli.Framework/Framework`
- `O` = `src/cli/operations/OpenForge.Cli.Operations/Commands`
- `D` = `.agents/memory/crystallized/documents/cli`

Citations expand these prefixes.

- **E1:** `F/Mutation/Locking/WorkspaceLockManager.cs:102`, `:154`, `:198`. Exclusive handle, path validation and zero-length restriction. `F/Mutation/Locking/WorkspaceLockPathIdentity.cs:17`. Existing location. `F/Workspace/WorkspaceIdentity.cs:28`. Hash identity. `F/Filesystem/PhysicalPaths/CurrentUserDataHome.cs:17`. Override resolution.
- **E2:** `F/Mutation/Locking/Models/WorkspaceLockLease.cs:78`. Handle disposal. `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/Journeys/F20WorkspaceLockJourneyTests.cs:23`, `:84`. Independent workspaces and killed-holder retry, both Windows-only.
- **E3:** `O/Index/Shared/Planning/IndexPlanBuilder.cs:20`. Region and maintenance effects. `O/Shared/RoutedMaintenance/RoutedMaintenanceEffectAssembler.cs:17`, `:25`, `:47`. Scaffolding, metadata and generated replacements.
- **E4:** `O/Index/Shared/Projection/IndexProjectionReader.cs:153`, `:161`, `:178`. Shared read expectations and parent/sibling expansion. `O/Index/Shared/Operation/IndexApplicationOperation.cs:79`, `:113`, `:142`. Validation, recovery and application.
- **E5:** `O/Route/Move/Shared/Planning/RouteMovePlanBuilder.cs:77`. Workspace reference scope. `O/Route/Move/Shared/Planning/RouteMoveEffectPlanner.cs:18`, `:49`. Complete effects. `O/Route/Move/Shared/Planning/RouteMoveLinkScopePlanner.cs:13`. Conditional settings rewrite. `O/Route/Move/Shared/Application/RouteMovePlanRevalidator.cs:25`. Under-lease rebuild.
- **E6:** `O/Install/Shared/Planning/InstallEstablishmentPlanner.cs:300`, `:324`, `:403`. Host-region ownership, ownership publication, settings and ignore effects. `O/Install/Shared/Planning/InstallEffectOrdering.cs:8`. File ordering. `D/contracts/install/interface.md:178`. Bounded ignore ownership.
- **E7:** `F/Ownership/WorkspaceOwnershipStore.cs:27`, `:57`, `:87`. Whole-document Framework, Extension and Library ownership writes.
- **E8:** `F/Recovery/RecoveryBundleStore.cs:30`, `:69`, `:128`, `:160`. Storage resolution, draft creation, verification and publication.
- **E9:** `D/technical-designs/mutation-and-recovery.md:23`, `:62`, `:134`, `:427`. Storage contract, handle ownership, revalidation and non-journal recovery.
- **E10:** `F/Recovery/RecoveryBundleCatalogue.cs:22`. Noncreating observation. `F/Recovery/Operational/RecoveryResidualOperationalContributor.cs:26`, `:34`. Status and Doctor. `O/Cleanup/Shared/Application/CleanupApplicationOperation.cs:23`, `:41`. Lease-bound deletion session.
- **E11:** `.agents/memory/working/cli-development/tasks/task85-configurable-main-root.md:9`, `:19`. Deferred status and fixed controls.
- **E12:** `O/Install/Shared/Operation/InstallApplicationOperation.cs:68`. Install failure mapping. `O/Index/Shared/Operation/IndexMutationMapper.cs:47`. Index mapping. `src/cli/output-text/OpenForge.Cli.OutputText/Shared/SharedText.cs:115`. Misleading contention wording.
- **E13:** `src/cli/root/OpenForge.Cli/OpenForge.Cli.csproj:9`. NativeAOT. `Directory.Build.props:15`. Six runtime identifiers.

## P7: CLI architect

/root - GPT-6 - reasoning level unreported

#### Recommendation

Keep one mutation lease per physical workspace. Move its canonical lock file and default recovery storage into a reserved `.agents/.open-forge/` directory, independent of the configurable knowledge root. Preserve the existing planning, revalidation, recovery preparation and application boundaries. Introduce a deliberate mixed-version transition, because silently falling back from an inaccessible legacy lock would permit concurrent writers. Promise that abandoned lock files never require unlocking, while stating honestly that filesystem permissions and partially applied changes can still require intervention.

#### Answers

##### 1. Where the lock lives and what it is keyed by

Use `<physical workspace>/.agents/.open-forge/write.lock`. Its location identifies the workspace. Do not add a pathname hash to this local filename, since two containers mounting the same directory at different paths should reach the same file.

Retain `WorkspaceIdentity.Key` for legacy lock discovery and existing recovery manifests. The current key hashes the normalized physical path, with Windows case normalization. Workspace selection uses the explicit workspace or current directory, not Git ancestry. Separate worktrees already receive separate identities. [E1–E3]

Continue using an ordinary persistent file and an exclusive `FileStream`. Never unlink it on release. Validate the runtime directory’s physical ancestry and reject redirected or non-directory components.

Expected behavior:

| Environment | Proposed behavior |
| --- | --- |
| Worktree-only sandbox | Workspace mode writes runtime state inside the allowed worktree. |
| Separate Git worktrees | Independent locks. No access to their shared Git directory is required. |
| Containers | Writers sharing the actual runtime directory share the lock, subject to filesystem locking support. |
| CI | Separate checkouts remain independent. Shared checkouts serialize. |
| WSL | Support ordinary Linux-local use. Do not claim Windows and WSL processes coordinate safely on one checkout without qualification. |
| Network drive | No universal guarantee. Use separate local checkouts or external serialization unless the specific filesystem and client combination is qualified. |

This qualification matters. .NET 10’s Unix implementation uses advisory locking and ignores some unsupported-lock failures. A successful `FileStream` open alone cannot prove exclusion on every filesystem. [Runtime source](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Private.CoreLib/src/Microsoft/Win32/SafeHandles/SafeFileHandle.Unix.cs#L312-L327)

Nested workspace identities also do not prove disjoint writes. If a parent operation can modify the child’s files, serialize those operations externally. I would not add hierarchical locking in this change.

##### 2. Granularity

Keep the existing workspace scope and lease duration.

Index forms changes across projected regions and holds its lease through revalidation, recovery preparation, application, verification and recovery deletion. Route Move combines destination creation, reference and navigation rewrites, source deletion and possible settings changes. Those are not folder-local transactions. [E4–E5]

Shared settings and ownership records add conflicts across otherwise unrelated commands. However, the brief’s blanket description is too broad. Index’s inspected application path applies its planned updates rather than routinely rewriting both records. The actual write sets already justify workspace serialization without that claim.

Do not replace the lease with expected-state checks alone. `MutationRevalidator` explicitly requires a live matching lease. Checking expected bytes does not reserve them against another cooperating writer between validation and replacement. [E6]

Keep planning and interaction outside the lease. Preserve lock-free dry runs, refusals and verified no-ops. Index already exits before acquisition for dry runs and no-ops. [E4]

##### 3. Never stuck

The achievable guarantee is **no stale ownership state**, not unconditional write availability.

| Condition | User-visible result and action |
| --- | --- |
| Ordinary leftover lock file | Reuse it. Its existence never means busy. |
| Killed process | The handle is released. Retry normally. F20 already proves this on Windows. |
| Live writer | Acquisition fails without target changes. Retry after the writer finishes. |
| Access denial | Name the exact lock or recovery path and the denied action. Correct the sandbox or directory permissions. |
| Half-applied operation | Report retained recovery evidence and current-state conflicts. Review the files, restore where necessary, then plan again. |
| Invalid runtime path, such as a directory occupying the lock filename | Report the conflicting object. Correct it after stopping writers. Do not delete it automatically. |

For the new local lock, remove the unnecessary zero-length acceptance requirement. Create an empty file, but tolerate an existing ordinary file’s bytes without reading, truncating or changing them. Its exclusive handle supplies ownership. Today both the manager and lease reject nonempty files, so the current “existence means nothing” claim has this important qualification. Preserve legacy validation while interoperating with older binaries. [E2]

Recovery residuals are a separate issue. Route Include blocks on any catalogue candidate. Install filters verified bundles against current target states and blocks unresolved or mixed states. Therefore, a leftover ZIP can already block commands even when locking works perfectly. [E7]

Keep those command safety policies in this change. Give their findings explicit recovery paths and continuation instructions. Do not recommend cleanup as the first response to a partial apply. Cleanup deletes recovery evidence and does not restore workspace files.

No design can safely promise writes when every required directory is unwritable. Likewise, relocating a mutex cannot make a multi-file operation crash-atomic. The current design explicitly provides no replay journal or automatic rollback. [E8]

##### 4. Recovery bundles

In workspace mode, write to:

```text
.agents/.open-forge/recovery/v1/<existing workspace key>/
```

Preserve the existing manifest, attribution and draft-to-final verification protocol. This avoids a recovery-format migration. A changed mount path can produce a different bucket and identity. Do not silently adopt bundles from another identity. [E9]

Add one typed recovery-location resolver, consumed by the writer, catalogue and deletion guards. Returning only a new directory from `RecoveryBundleStore` is insufficient. `RecoveryBundleDeletionValidation` independently reconstructs the expected bucket today. [E10]

Status and Doctor already receive recovery observations through `RecoveryResidualOperationalContributor`. Cleanup directly reads the catalogue and opens a deletion session under the workspace lease. Preserve those ownership boundaries. [E11]

During migration, observers should report active and legacy stores separately, including separate coverage states. An unreadable legacy store must not make a healthy active store appear unavailable. Cleanup should default to the active store, with an explicit legacy-store selection for old artifacts.

Protect local runtime state before writing payloads:

- Create a runtime-local `.gitignore` containing `*` before the first bundle.
- Reserve the subtree against source inclusion, package destinations and ordinary removal.
- Exclude it explicitly from source and Markdown traversal, independently of Git.
- Preserve the directory during framework removal and main-root migration.

Install’s existing bounded ignore section represents route-sharing choices. Its parser rejects arbitrary patterns. Do not insert a runtime rule there without changing that contract. A runtime-local ignore file avoids requiring an Install rerun before another command can safely create a bundle. [E12]

Ignored files remain vulnerable to `git clean -x` and `git clean -X`. Document excluding this subtree when cleaning, and prohibit cleaning it while writers run. Users needing recovery outside disposable worktrees can retain an explicit external data home. [Git documentation](https://git-scm.com/docs/git-clean.html)

##### 5. Configuration and reporting

Keep `OPENFORGE_DATA_HOME` for explicit external recovery storage and legacy lock discovery. Preserve its absolute-path requirement. A relative value must produce an actionable configuration failure rather than silently select another location. [E13]

It must never redirect the new canonical workspace lock. Otherwise two agents with different environments can again acquire different locks for the same files.

For the transition, add one explicit invocation policy, such as:

```text
--lock-protocol compatible|workspace
```

Default the transition release to `compatible`. Workspace mode requires an environment where older writers have been retired. This is a deployment condition, not something the flag can enforce.

Status should show the selected protocol, canonical lock path, active recovery location and legacy observation coverage. It must not create directories, probe write access or infer activity from file existence.

Carry the existing `FilesystemFailure` kind, acquisition stage and attempted path through command results. Render:

- Confirmed contention as a busy workspace.
- Access denial as a permission failure.
- Invalid paths as invalid paths.
- Ambiguous I/O as inability to acquire the lock, without inventing another writer.

The manager already distinguishes several causes. The shared wording currently claims contention unconditionally. Some command mappings also discard detail. [E14]

There are 19 files matching `AcquireAsync(`, not necessarily 19 independent commands or identical finding mappings. Install maps acquisition failure to `WorkspaceUnsafe`, while Index distinguishes invalid paths from unavailable locking. [E14]

##### 6. Migration and mixed versions

In compatible mode:

1. Acquire the canonical local lock.
2. Acquire the exact legacy lock selected by the existing data-home and workspace-key rules.
3. Hold both until the operation finishes.
4. Release the local handle if legacy acquisition fails.
5. Continue writing recovery bundles to the legacy store so old commands can observe them.

Every new writer follows that order. Old writers need only the legacy lock to exclude compatible-mode writers. This guarantee requires the same legacy data home and physical-path identity. Older binaries already lack coordination across different users, overrides or mount-path spellings. [E1–E3, E13]

**Do not skip the legacy lock after access denial.** A sandbox’s inability to open it does not prove that an older host process is absent.

Cutover to workspace mode therefore requires stopping older writers, reviewing legacy residuals and pinning the new CLI in agents and CI. A settings marker would not fence an older executable that ignores it.

This exposes a real incompatibility in the requested guarantees. Transparent sandbox operation with no legacy-store access cannot also guarantee exclusion against arbitrary unmodified legacy writers. If mixed-version safety is mandatory, access to their shared legacy lock remains mandatory.

Leave existing `locks/v1` files untouched. Their presence costs no ownership. Leave existing recovery bundles in place, readable through the legacy catalogue. Legacy cleanup must acquire the legacy lock as well as the local lock. Do not copy, delete or rewrite recovery archives automatically.

##### 7. Implementation plan

Freeze the protocol and store-selection contracts first. Then use these ownership packets.

| Packet | Exact responsibility | Estimate |
| --- | --- | ---: |
| Lock foundation | `WorkspaceLockManager`, `WorkspaceLockPathIdentity`, `WorkspaceLockLease`, `WorkspaceLockResult`. Add `WorkspaceRuntimePaths` and a typed acquisition-stage failure. Support local and composite leases without changing applier contracts. | 2–3 days |
| Recovery locations | `RecoveryBundlePathIdentity`, `RecoveryBundleStore`, `RecoveryBundleCatalogue`, storage helpers and deletion session/validation. Add a typed `RecoveryStoreLocation` carried through observation and deletion. | 2–3 days |
| Runtime protection | Runtime bootstrap and ignore file, `SourceCatalogueReader`, `RouteMarkdownCatalogueReader`, destination policies, inclusion and removal admission. Check bootstrap-created `.agents` against Install’s planned directory expectations. | 1–2 days |
| Reporting and transition | Shared option binding, the 19 acquisition consumers, Status/Doctor observations, Cleanup store selection, Rendering and `SharedText`. | 2–3 days |
| Qualification and documentation | Cross-process journeys, migration tests, platform gates, contracts and operator guidance. | 3–4 days |

**Total: 10–15 engineering days**, including integration and one focused review. With three implementers after the foundation freezes, expect roughly 7–10 elapsed working days. This is not a two-file path substitution.

Required evidence:

- **Unit:** location selection, protocol validation, composite lease lifetime, preserved failure facts and catalogue origin.
- **Integration:** denied profile with writable workspace, denied runtime directory, unchanged nonempty local lock, redirected paths, legacy-store coverage, guarded deletion, scan exclusion and fresh Install bootstrap.
- **Operation regressions:** representative Install, Index, Route Move, Include, Library and Cleanup flows. Verify dry-run/no-op behavior and retained recovery after interrupted application.
- **End-to-end:** extend F20 beyond its Windows-only holder. Prove contention, independent workspaces, process death, old/new exclusion in both directions and different new-process data-home overrides.
- **Platforms:** managed and NativeAOT qualification on Windows, Linux and macOS, across the existing x64 and arm64 matrix. Treat WSL interoperability and network filesystems as separately qualified environments. [E15]

Update the mutation-and-recovery design, architecture storage summary, shared options, affected command findings, Status/Doctor/Cleanup contracts, Install runtime footprint and public sandbox guidance.

Coordinate the reserved control-directory path with Task 85. Its record preserves fixed `.agents` controls, but currently marks the task deferred, contrary to the brief’s “next” wording. [E16]

#### Risks and what I would not build

I would not build folder locks, file-lock ordering, PID ownership, stale-file deletion, leases with timeouts, a lock daemon, automatic rollback or a recovery journal.

The main risks are mixed-version deployment, accidentally admitting runtime files into content operations, deletion guards retaining their old single-store assumptions, and local recovery being removed by external cleanup.

Keep the supported boundary explicit: cooperating writers, qualified filesystems and no external deletion of the live coordination file. Do not claim protection against arbitrary concurrent filesystem tools.

This was read-only inspection at the supplied commit. No builds, tests or Doctor invocation were run. The reported sandbox incident was not reproduced.

#### Evidence

Prefixes below are exact repository paths:

- `F` = `src/cli/framework/OpenForge.Cli.Framework/Framework`
- `O` = `src/cli/operations/OpenForge.Cli.Operations/Commands`
- `D` = `.agents/memory/crystallized/documents/cli`

1. **E1:** `F/Workspace/CliWorkspaceSelector.cs:22`, `F/Workspace/WorkspaceIdentity.cs:28`, `F/Mutation/Locking/WorkspaceLockPathIdentity.cs:17`.
2. **E2:** `F/Mutation/Locking/WorkspaceLockManager.cs:102`, `:154`, `:198`. `F/Mutation/Locking/Models/WorkspaceLockLease.cs:19`, `:65`, `:78`.
3. **E3:** `F/Mutation/Locking/Models/WorkspaceLockStoreRoot.cs:18`.
4. **E4:** `O/Index/IndexOperation.cs:71`, `O/Index/Shared/Planning/IndexPlanBuilder.cs:20`, `O/Index/Shared/Operation/IndexApplicationOperation.cs:42`, `:79`, `:113`, `:142`, `:246`.
5. **E5:** `O/Route/Move/Shared/Planning/RouteMoveEffectPlanner.cs:18`, `:30`, `:72`. `O/Route/Move/Shared/References/RouteMoveReferencePlanner.cs:36`, `:99`.
6. **E6:** `F/Mutation/Validation/MutationRevalidator.cs:13`. `D/technical-designs/mutation-and-recovery.md:129`.
7. **E7:** `O/Route/Shared/Inclusion/Application/RouteInclusionApplication.cs:54`. `O/Install/Shared/Recovery/InstallRecoveryCatalogueReader.cs:27`, `:52`.
8. **E8:** `D/technical-designs/mutation-and-recovery.md:427`. `O/Cleanup/Shared/Planning/CleanupPlanner.cs:109` admits verified finals without proving operation completion.
9. **E9:** `F/Recovery/RecoveryBundleStore.cs:30`, `:128`, `:160`, `:204`. `F/Recovery/Shared/Storage/RecoveryBundlePathIdentity.cs:24`.
10. **E10:** `F/Recovery/Shared/Deletion/RecoveryBundleDeletionValidation.cs:62`. `F/Recovery/Shared/Deletion/RecoveryDeletionStorageBoundary.cs:11`.
11. **E11:** `F/Recovery/Operational/RecoveryResidualOperationalContributor.cs:26`, `:34`. `src/cli/root/OpenForge.Cli/Composition/CliCompositionRoot.cs:55`. `O/Cleanup/CleanupOperation.cs:48`. `O/Cleanup/Shared/Application/CleanupApplicationOperation.cs:23`, `:41`.
12. **E12:** `O/Install/Shared/Configuration/InstallIgnoreSection.cs:17`, `:42`. `F/Sources/Inventory/SourceCatalogueReader.cs:93`. `O/Route/Shared/References/RouteMarkdownCatalogueReader.Traversal.cs:80`. `O/Extension/Shared/Permissions/ExtensionDestinationPolicy.cs:25`. `O/Library/Shared/Planning/LibraryDestinationPolicy.cs:31`.
13. **E13:** `F/Filesystem/PhysicalPaths/CurrentUserDataHome.cs:15`.
14. **E14:** `F/Mutation/Locking/WorkspaceLockManager.cs:71`, `:119`, `:134`. `O/Index/Shared/Operation/IndexMutationMapper.cs:47`. `O/Install/Shared/Operation/InstallApplicationOperation.cs:68`. `src/cli/output-text/OpenForge.Cli.OutputText/Shared/SharedText.cs:115`.
15. **E15:** `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/Journeys/F20WorkspaceLockJourneyTests.cs:23`, `:84`, `:305`. `Directory.Build.props:15`. `src/cli/root/OpenForge.Cli/OpenForge.Cli.csproj:9`.
16. **E16:** `.agents/memory/working/cli-development/tasks/task85-configurable-main-root.md:9`, `:19`.
