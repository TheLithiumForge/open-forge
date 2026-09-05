---
open-forge:
  description: Sealed restart snapshot for the active CLI Doctor correction and the ordered replacement-CLI queue
  tags: [LoadNow, Memory, Handoff, Working, CLI, Doctor, Restart, Contextual]
---

# CLI Doctor Restart Handoff — 2026-09-05

## Snapshot Status

This is the sealed restart boundary for the next user-facing Open Forge
Overseer. It is intentionally **uncommitted** and must be treated as sealed
once the PC restarts. Do not edit this snapshot after resumption; record later
state in the active Checkpoint or a new Handoff. The snapshot is contextual
transfer state, not a replacement for the project ledger or Task record.

The exact follow-up packet from Sagan the 3rd is incorporated below. No pending
review packet is missing. The current mutable Doctor correction is still
uncommitted and must be preserved.

## Start Here

Resume from the repository root and then inspect the Doctor worktree:

```text
cd /home/tedy/dev/open-forge
sed -n '1,260p' .agents/loader.md
sed -n '1,260p' .agents/memory/_memory.md
sed -n '1,260p' .agents/memory/working/handoffs/_handoffs.md
sed -n '1,260p' .agents/memory/working/handoffs/2026-09-05_cli-doctor-restart.md
cd /home/tedy/dev/open-forge-worktree/doctor-implementation
git status --short --branch
git rev-parse HEAD
git rev-parse HEAD^{tree}
git diff --stat
git diff --name-status
```

Then read the live sources, in this order:

- [CLI project control ledger](../cli-development/project-control.md), which
  owns permanent IDs, queue state, worktree mapping, integration, and release
  restrictions.
- [CLI development checkpoint](../checkpoints/cli-development.md), which
  records the current project-level state and next action.
- [CLI development plan](../cli-development/plan.md), which records the
  accepted dependency graph and evidence gates.
- [Task 16 Doctor](../cli-development/tasks/operations/doctor.md), which owns
  the Doctor horizon and task-local evidence.
- [Task 5 Route Remove](../cli-development/tasks/route-mutation/route-remove.md),
  which owns the next retained command boundary.
- [Overseer role](../../../../.apm/agents/overseer.agent.md) and [Task
  Mastermind role](../../../../.apm/agents/task-mastermind.agent.md).

The current live Git state, the exact review packet below, and fresh reproduced
evidence outrank older prose that still describes Doctor as merely prepared or
uses the historical 112-kind catalogue.

## Project And Git Boundary

### Integration repository

- Repository: `/home/tedy/dev/open-forge`.
- Branch: `develop`.
- HEAD: `f8e542a96c9f55679a8435f941bdd15ae67c0a78`.
- HEAD tree: `0344b7e5433fb84fbb8d1a767ea213827f6ce972`.
- Worktree: clean at the handoff boundary; current `develop` is ahead of
  `origin/develop` by 109 commits and behind by 0.
- The historical Task 16 integration base remains
  `bb3a03f64fed6e84def7a6d5e0cefe12f9f75f7d`, tree
  `41669500d8e2a49a03412975c8fd00f7635a4b6a`. Current `develop` is that same
  baseline plus the separate prose-only cleanup commit above. The cleanup
  changes exactly three prose files and does not contain the Doctor correction.
- No push, fetch, publication, release, global install, or remote action is
  authorized.

The original `bb3a03f` tip is the exact accepted Status integration base for
Doctor. Current `develop` is the same baseline plus the separately integrated
prose cleanup. It is not safe to reset, restore, clean, or otherwise discard the
Doctor worktree's uncommitted files.

### Active Doctor worktree

- Worktree: `/home/tedy/dev/open-forge-worktree/doctor-implementation`.
- Branch: `codex/doctor-implementation`.
- Base/merge-base with `develop`: `bb3a03f64fed6e84def7a6d5e0cefe12f9f75f7d`.
- Current immutable HEAD: `e602e2c1990930338286f938ffb3eb747f31daaa`.
- Current immutable HEAD tree: `80c076f39519528a08d00f6d829c83f05afa20eb`.
- HEAD parent: reviewed coherent-production candidate
  `b1a49434c6701cc6c0be4b94c915e56d5d5cc177`, tree
  `34d2f9d70b44bf9dd441a99b7c37d47e370144bb`.
- HEAD is the separate prose-only cleanup checkpoint, not a new Doctor
  production candidate or final accepted Doctor result. Its parent is the
  reviewed candidate `b1a49434`; the equivalent isolated cleanup exists as
  `f8e542a9` on current develop. The two cleanup commits are equivalent
  three-prose-file changes; they do not change the 28-path Doctor correction
  inventory.
- Curie was safely interrupted at this boundary. No owned mutating process is
  running; no correction file is staged or committed.
- Index: empty.
- `git diff --check`: clean.
- Dirty target: 28 C# paths, consisting of 20 modified paths and 8 untracked
  paths: 17 Core, 8 Unit, 2 Integration, and 1 EndToEnd.
- This untracked Markdown Handoff is additional transfer state and is not part
  of the 28-path C# correction target; it is expected in `git status`.
- Sorted dirty-path manifest SHA-256:
  `24fde84c837d2aaf0ca471e3247dcb527d805fff040543b01a9b966e673bf065`.
- Current-content aggregate SHA-256:
  `2d70e643a9af253b79be5cfbad373c11542156a72a30034877dd55066f7e3e23`.
- Tracked binary diff SHA-256:
  `a4ba257f1cb472f2ab68b6e3914d2226828fa797d4bfcc59b617859a76baa005`.

The correction is one grouped T16-C2 boundary. Preserve all 28 paths until
Curie or an explicitly transferred owner has revalidated every finding against
the current content. Do not use `git reset`, `git restore`, `git clean`, or
similar destructive recovery.

### Current dirty-path inventory

The inventory below is the exact target at the handoff boundary. The first
section contains modified tracked files; the second contains untracked files
that belong to the correction and must not be lost.

```text
src/cli/core/OpenForge.Cli.Core/Commands/Doctor/DoctorOperation.cs
src/cli/core/OpenForge.Cli.Core/Commands/Doctor/Shared/Aggregation/DoctorResultBuilder.cs
src/cli/core/OpenForge.Cli.Core/Commands/Doctor/Shared/Domains/DoctorDomainSupport.cs
src/cli/core/OpenForge.Cli.Core/Commands/Doctor/Shared/Domains/ExtensionLifecycleDoctorInspector.cs
src/cli/core/OpenForge.Cli.Core/Commands/Doctor/Shared/Domains/FrameworkLifecycleDoctorInspector.cs
src/cli/core/OpenForge.Cli.Core/Commands/Doctor/Shared/Domains/FrameworkOwnershipDoctorInspector.cs
src/cli/core/OpenForge.Cli.Core/Commands/Doctor/Shared/Domains/RouteGeneratedEntryDoctorInspector.cs
src/cli/core/OpenForge.Cli.Core/Commands/Doctor/Shared/Domains/WorkspaceEntryDoctorInspector.cs
src/cli/core/OpenForge.Cli.Core/Commands/Doctor/Shared/Rendering/DoctorFindingWireVocabulary.cs
src/cli/core/OpenForge.Cli.Core/Commands/Status/Shared/Aggregation/StatusLifecycleAbsenceResolver.cs
src/cli/core/OpenForge.Cli.Core/Framework/Extensions/Operational/ExtensionLifecycleDoctorReader.cs
src/cli/core/OpenForge.Cli.Core/Framework/Extensions/Operational/Models/ExtensionLifecycleDoctorModels.cs
src/cli/core/OpenForge.Cli.Core/Framework/Sources/Operational/Models/RouteOperationalModels.cs
src/cli/core/OpenForge.Cli.Core/Framework/Sources/Operational/Shared/Routes/RouteDoctorGeneratedNavigationReader.cs
src/cli/core/OpenForge.Cli.Core/Framework/Sources/Operational/Shared/Routes/RouteObservationReader.cs
src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedDoctorProcessTests.cs
src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Sources/Routing/SourceRouteFactsResolverIntegrationTests.cs
src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Doctor/DoctorOperationTestSupport.cs
src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Doctor/DoctorOperationTests.cs
src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Doctor/DoctorWorkspaceRouteMappingTests.cs
```

```text
src/cli/core/OpenForge.Cli.Core/Framework/Extensions/Operational/Models/ExtensionLifecycleDoctorAssessment.cs
src/cli/core/OpenForge.Cli.Core/Framework/OperationalContributors/OperationalLifecycleAbsenceProof.cs
src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Recovery/RecoveryBundleDeletionAttributionIntegrationTests.cs
src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Doctor/DoctorFiniteMappingTests.cs
src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Doctor/DoctorLifecycleAbsenceTests.cs
src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Doctor/DoctorOperationEventTests.cs
src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Doctor/DoctorProjectionParityTests.cs
src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Recovery/Models/RecoveryBundleAttributionContractTests.cs
```

## Task 16 Progress And Ownership

Canonical display at this boundary:

```text
Active
- Task 16 “Doctor” (ID 16; phase 4/5; milestone 4/8) — grouped T16-C2
  correction is uncommitted after a fresh whole-task review.
Recently completed
- none.
Queued
- Task 5 “Route Remove” — next after Doctor acceptance; implementation waits for the accepted Doctor boundary
- Task 6 “Root Update” — after Route Remove
- Task 17 “Extension Update” — after Root Update
- Task 18 “Extension Remove” — after Extension Update
- Task 19 “Repair” — after Extension Remove and the complete contributor inventory
- Task 20 “Cleanup” — after Repair and every artifact producer
- Task 10 “CLI Command Surface Audit” — after retained commands, before delivery
- Task 21 “CLI Command Surface Remediation” — conditional on accepted Task 10 findings
- Task 13 “Native linux-x64 CI and Reproducible Artifacts” — after retained commands and any Task 21 remediation
- Task 22 “Final Documentation, Acceptance, and Release” — final task; publication still needs separate authority
```

No agent process remains active at the handoff boundary. Re-establish exact
ownership after inspecting the live worktree and process state:

- Sagan the 3rd — designated Task Mastermind
  (`.apm/agents/task-mastermind.agent.md`) — `gpt-5.6-sol / xhigh`; completed
  the final safe-boundary packet.
- Curie the 3rd — Brilliant Implementer
  (`.apm/agents/brilliant-implementer.agent.md`) — `gpt-5.6-sol / xhigh`,
  safely interrupted after the grouped correction was prepared; no process
  remains.

Completed relevant agents:

- Bacon the 3rd — Gray Contract Implementer
  (`.apm/agents/gray-contract-implementer.agent.md`) — model/reasoning
  unreported in the final packet; the immutable Gray chain is complete.
- Hegel the 3rd — Reviewer (`.apm/agents/reviewer.agent.md`) —
  `gpt-5.6-luna / max`; completed T16-R2 with `CHANGES_REQUIRED`.

The T16-R2 reviewer inspected the immutable candidate HEAD and tree against the
develop base. The review was read-only and did not authorize a second review
wave. Sagan owns disposition and the one grouped correction; Curie owns the
correction unless an explicit ownership transfer is recorded after confirming
interruption. A quiet or unavailable child is `progress unobserved`, not a
failure and not permission to duplicate or discard work.

## Immutable Task 16 Commit Chain

These commits are immutable evidence and must not be rewritten:

| Boundary | Commit | Tree | Meaning |
| --- | --- | --- | --- |
| Gray | `24a702efa02fb11d25036cd85f3c7816bf207ded` | `25f636883f827db94bf3e7e5c8a5262c6de59f1b` | Restored contract-only Doctor callable/result surface over integrated Status, including plural Extension-source observations. |
| Red | `2ed79f2a078bd09b260c532f05463dd3f74808c4` | `be4ddfd0033852daec9a6f9cc35d6a3a8f982b77` | Froze direct failing owned evidence for six domains, source consumption, mapping, and three simple public journeys. |
| Green | `4db37674f7d5b416e359e99e5143bb6f284df3e7` | `9f2ed3c4d8eb530a9c511a7b9a03e820efcd9077` | Implemented the original deterministic Doctor production boundary. |
| Correction Gray | `1d8624d3435e566ab3413eb91a48106c0eb25d0f` | `a32c2366d5ba01e58ce356af4ff031fccdd34d99` | Constrained lifecycle evidence after the first whole-task review. |
| Recovery-v1 Gray | `074a050c6c60baa67cb12c8ce29693aea5b473c2` | `73d6069ab7c25b3eb3c8fb45d1775acc93bb3272` | Defined strict schema-v1 recovery attribution and bounded Framework partial-state comparison. |
| Extension-deferral Gray | `ce4e98b2166768fafd99974e4edca8263a3f85c2` | `f014287b1402d34532265a53fb3755e96d12db7d` | Deferred two Extension findings until Tasks 17 and 18 provide honest producer facts. |
| 109-kind Gray | `b88588441fc97346bb138ea15c7b6f00d1f06136` | `d708befc38e9a750632aafad3487d12f111eb063` | Reconciled the unreleased schema-v1 catalogue to 109 kinds and recorded the Task 10 ambiguity input. |
| Reviewed candidate | `b1a49434c6701cc6c0be4b94c915e56d5d5cc177` | `34d2f9d70b44bf9dd441a99b7c37d47e370144bb` | Original coherent production plus focused verification; reviewed by Hegel/T16-R2; correction target is now dirty and uncommitted. |
| Prose cleanup checkpoint | `e602e2c1990930338286f938ffb3eb747f31daaa` | `80c076f39519528a08d00f6d829c83f05afa20eb` | Separate three-file prose cleanup on top of the reviewed candidate; no Doctor correction or production behavior change. |

Related historical sources retained by the chain:

- Doctor activation snapshot: `26e245e4116de07d9d922ea055a57848b047c28d`,
  tree `680df0320115a1d8a58a068a90b8c598215c376e`.
- Earlier contract-only Gray source: `ce62759348400a8e6a8094f4a5704950bf9ac0ac`,
  tree `1f07abb7dd447705f8c225953c26aad998a68bc6`.
- Integrated Status base for this task: `bb3a03f64fed6e84def7a6d5e0cefe12f9f75f7d`,
  tree `41669500d8e2a49a03412975c8fd00f7635a4b6a`.

## Accepted First-Release v1 Decisions

### Doctor catalogue

The product has not been released. The public command-local schema is therefore
first-release v1, even where internal notes previously called it “v2”. Do not
create a v2 reader, aliases, migrations, compatibility shims, tombstones, or
fallbacks for this correction.

The accepted Doctor catalogue contains exactly 109 finite kinds:

- 21 workspace;
- 4 recovery;
- 22 route;
- 28 local-reference;
- 14 Framework lifecycle; and
- 20 Extension lifecycle.

The current implementation target is 107 producer-backed emissions plus exactly
two accepted Extension deferrals:

- `extension.bridge-registration` — deferred to Task 17 Extension Update;
- `extension.unmanaged-like-content` — deferred to Task 18 Extension Remove.

The three superseded route kinds are removed from the unreleased v1 catalogue:
`route.child-missing`, `route.cycle`, and `route.overwrite-ambiguous`. Their
wire names, enum members, mappings, and independent literal-oracle rows are
removed together. There are no aliases, migration paths, synthetic producers,
or generated-navigation substitutions. Historical 112-kind and 52-emission
receipts remain historical and must not be presented as current completeness.

The fixed domain order is workspace, recovery, routes, local references,
Framework lifecycle, and Extension lifecycle. Findings are ordered by domain,
stable kind, typed subject, and canonical location. Coverage, severity,
resolution, provenance, candidate/proposal facts, and typed next actions remain
independent facts.

### Contributor architecture

Doctor consumes the explicit immutable application-scoped
`OperationalContributorCatalogue` built by `CliCompositionRoot`. It invokes the
six producer-owned typed views once per invocation in fixed order. This is
explicit composition, not dependency injection, a service locator, reflection,
assembly scanning, an enumerable/runtime registry, a generic operational engine,
ambient registration, or a context/options/service bag. Doctor does not parse
Status output, reread producers, or import another command's private `Shared/**`.

The catalogue remains the accepted pluggable seam: future producers extend the
typed contributor inventory and affected Status/Doctor evidence at their own
task boundary. Do not replace it with runtime registration or DI merely to make
the future scale.

### Recovery attribution

Recovery schema v1 keeps discriminator `1`. Each final bundle needs verified
finite producer, finite operation, and typed subject `{kind, identity}`
attribution. The current writers supply the selected normalized physical
workspace identity and `WorkspaceIdentity.Key(...)`. Unknown, null, malformed,
cross-combined, or cross-workspace values fail closed. Existing schema-v1 finals
without valid attribution remain preserved but are malformed/unattributed; they
are not migrated, rewritten, adopted, inferred, or deleted. Drafts remain
exact-name/path-only incomplete facts and are not used to infer attribution.

Deletion revalidation includes attribution equality as well as existing
same-workspace lease, re-enumeration, and final semantic checks. Attribution is
not deletion authority. Framework partial recovery compares one verified
same-workspace Framework-attributed final's exact prior and intended state per
bundle; no payload bytes, cross-bundle winner, third state, or historical
inference enters Doctor.

### Absence and Extension boundaries

Missing lifecycle documents or payload-derived empty collections do not prove
that managed Framework or Extension state is absent. Framework may claim safe
absence only from explicit, complete, producer-owned absence proof covering the
managed boundary. Extension makes no absent/NotApplicable/install-state claim
until Task 18 supplies its independent installed-manifest observation. The two
Extension horizon limitations remain honest incomplete coverage, not evidence
of absence.

Unavailable generated navigation must not be compared to an empty placeholder;
it cannot fabricate `Extra` drift or an `index` action. Loader duplicate-root
facts must remain visible through the existing workspace-loader-malformed
mapping. Route diagnosis is based on authored/producer-owned facts, never on
derived generated entries as authored topology.

## T16-R2 Whole-Task Review And Correction

Hegel the 3rd reviewed immutable candidate
`b1a49434c6701cc6c0be4b94c915e56d5d5cc177` (tree
`34d2f9d70b44bf9dd441a99b7c37d47e370144bb`) against the accepted develop base
`bb3a03f64fed6e84def7a6d5e0cefe12f9f75f7d` (tree
`41669500d8e2a49a03412975c8fd00f7635a4b6`). The reviewer personally read the
complete current C# directive files. The review result was `CHANGES_REQUIRED`.
It was independent, read-only, and bounded; no second full review wave was
authorized.

The exact grouped correction packet is:

### T16-R2-F1 — High: Framework safe absence is not independently proved

- Evidence: `FrameworkLifecycleDoctorInspector.cs` previously treated
  `DocumentMissing + Targets.Count == 0 + empty complete recovery` as safe
  absence, while `FrameworkLifecycleDoctorReader.cs` derived empty targets from
  missing lifecycle/payload.
- Consequence: a missing lifecycle could become a complete Framework-absent
  diagnosis and an Install action without proof of managed state, ownership,
  routes, source inventory, or installation boundary.
- Required correction: use one neutral immutable absence proof only when complete
  producer-owned absence is explicit; otherwise retain incomplete/missing state.
  Status delegates remain unchanged. Doctor route inventory must not reread.
  Add positive, route-negative, and recovery-negative owned Unit evidence.

### T16-R2-F2 — High: Extension safe absence is not independently proved

- Evidence: the prior Extension inspector inferred an absent installed set from
  missing lifecycle state and lifecycle-derived empty collections, although the
  lifecycle reader marks missing lifecycle trust incomplete.
- Consequence: manifests or packages could exist while Doctor claimed safe
  Extension absence; horizon limitations did not make that claim true.
- Required correction: remove the unsafe absence shortcut, use the lifecycle/source
  assessment, and keep missing state incomplete without `Absent`, NotApplicable,
  or an Install action. Task 18 remains the owner of the independent
  installed-manifest observation.

### T16-R2-F3 — High: unavailable generated navigation can fabricate drift

- Evidence: `RouteDoctorGeneratedNavigationReader.Project` compared entries
  before checking unavailable region state; unavailable regions supplied empty
  entries, and comparison then marked current entries `Extra`.
- Consequence: invalid/missing authored metadata could fabricate generated
  navigation drift and an Index action.
- Required correction: suppress comparisons whenever the region state is
  unavailable, with a focused owned Unit fact.

### T16-R2-F4 — High: precise Loader duplicate-root fact was dropped

- Evidence: `SourceRouteFactsResolver` emits `LoaderDuplicateRoot`, but the
  Doctor workspace inspector and route mapping did not preserve it. The direct
  producer Integration expectation still expected `LoaderMalformed`.
- Consequence: a producer-backed Loader defect disappeared from Doctor evidence,
  and the prior direct Integration receipt was stale or false for this candidate.
- Required correction: map duplicate-root precisely to the existing
  `workspace.loader-malformed` boundary and repair the stale producer
  expectation/mapping Unit. Do not invent a new public kind.

### T16-R2-F5 — High: published E2E expectation is stale

- Evidence: the unchanged `PublishedDoctorProcessTests` JSON journey asserted
  exit `0` and `"complete"`, while the current two Extension horizon
  limitations make the result `incomplete` with exit `3`.
- Consequence: the historical published E2E green receipt is not candidate
  evidence.
- Required correction: update only that simple journey's expected exit/status and
  assert the two Extension limitations. Keep exactly three E2E journeys and
  test only owned public reachability/streams/exits/no-write behavior. Do not
  test the CLI framework, serializer, runtime, OS, ZIP, or other third-party
  behavior.

### T16-R2-F6 — High: failure, interruption, and projection parity lack evidence

- Evidence: `DoctorOperation` has distinct cancellation/interruption and failure
  paths, but current tests covered only contributor order/domain retention and
  plural Extension-source consumption. No owned Unit fact checked compact,
  expanded, and JSON parity.
- Consequence: corrected behavior could regress while existing Unit,
  Integration, and E2E receipts stayed green.
- Required correction: add narrow owned Unit facts for injected failure and
  pre-cancelled operation, asserting failed/interrupted status, all six domains,
  limitations, and no action; add one typed-result projection-parity fact across
  compact, expanded, and JSON. Current correction reports those Unit tests green.

### T16-R2-F7 — Medium: strict attribution/deletion evidence is incomplete

- Evidence: production validators cover strict attribution and deletion
  revalidation, but tests lacked missing/unknown producer-operation-subject,
  invalid tuple, subject/workspace mismatch, and changed-attribution deletion
  evidence.
- Consequence: production-critical attribution and cleanup authority lacked
  falsifiable owned evidence.
- Required correction: add focused contract tests for codec/manifest rejection
  and one integration deletion-guard journey that changes only attribution and
  asserts rejection/retention. Do not test STJ, ZIP, BCL, or test-platform
  internals. Current attribution Unit is green; deletion correction is compiled
  but its final targeted integration execution is pending.

### T16-R2-F8 — Medium: wire maps and coverage merge use ordinal/params plumbing

- Evidence: `DoctorFindingWireVocabulary.Read<T>(T, params string[])` created a
  params array per call and selected stable wire values by enum ordinal without
  an explicit undefined-value arm. `DoctorDomainSupport.Combine(params
  DoctorCoverageState[])` was called 17 times with exactly two values.
- Consequence: enum reordering could silently change stable wire output,
  undefined values could escape the closed vocabulary, and coverage merges paid
  avoidable allocations. This violates the C# finite-map and redundant-
  allocation directives.
- Required correction: use exhaustive switches or nearest-scope immutable maps
  with explicit discard/throw handling, and a two-argument nine-combination
  coverage merge. The correction removes params/ordinal behavior.

The original writer must revalidate each finding against current content before
accepting or repairing it. The packet above is the current correction scope;
there is no permission to broaden it into a general refactor or redesign.

## Receipt Register

### Current at the handoff boundary

These receipts were produced against the current correction content, but their
scope and invalidation status matter:

- Integration Release build: exit `0`, 0 warnings, 0 errors, 14.92 seconds.
- Unit Release build: exit `0`, 0 warnings, 0 errors, 8.44 seconds.
- Doctor Unit: `20/20`, 0 failures, 0 skips, no warnings.
- Recovery attribution Unit: `8/8`, 0 failures, 0 skips, no warnings.
- Precise route producer evidence: `12/12`, 0 failures, 0 skips, no warnings.
- Doctor Integration: `3/3` green on identical production, but before the two
  expectation-only changes; rerun is required.
- Status/deletion/duplicate targeted selection: `4/4` green before final
  refreeze; rerun is required.
- `git diff --check`: clean; index empty; no staging or commit occurred.

These are not final Task 16 acceptance receipts until all listed invalidations
are rerun against a refrozen current tree.

### Historical, preserved, or rejected evidence

- The immutable commit-chain receipts above are historical freeze points. The
  original Green focused receipts on `b1a49434` were valid for that immutable
  candidate, but the dirty correction invalidates any claim that they prove the
  current worktree.
- Historical rejected failures: `16/17` stale LoaderUnavailable evidence is
  resolved by current precise route producer `12/12`; Doctor `1/3`
  missing-ownership evidence is resolved by later Doctor `3/3` evidence; two
  bad filters selected zero tests and are non-evidence.
- The historical original Doctor focused receipt was: Core/Unit/Integration
  Release builds warning-free; Doctor Unit `3/3`; Doctor Integration `3/3`;
  published Doctor E2E `3/3`; operational-catalogue Unit `1/1`; Status
  composition Integration `4/4`; Extension lifecycle contributor Integration
  `1/1`. It remains evidence for the immutable candidate only and does not
  close the current correction.
- Task 15 Status remains accepted and integrated at the develop base with final
  warning-free managed and `linux-x64` Native AOT gates. Later producers must
  extend its explicit contributor inventory and affected evidence.

### Pending acceptance evidence

Run these exact commands after the correction is refrozen. Do not silently
replace them with a broad or stale `--no-build` command:

1. ```text
   dotnet test --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj -c Release --no-build --filter-trait "Feature=doctor-command" --minimum-expected-tests 3
   ```
2. ```text
   dotnet test --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj -c Release --no-build --filter-method "*DeletionGuardRejectsChangedAttributionAndRetainsFinal" --filter-method "*SafelyUninstalledWorkspaceReportsNotApplicableStatusFactsAndPreservesAllBytes" --filter-method "*RecoveryResidualPreventsUninstalledLifecycleAbsenceProof" --minimum-expected-tests 3
   ```
3. ```text
   dotnet build src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj -c Release --no-restore
   ```
4. ```text
   dotnet test --project src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj -c Release --no-build --filter-class "*PublishedDoctorProcessTests" --minimum-expected-tests 3
   ```

Then run the task's exact format, static, protected-path, prohibited-pattern,
callable-shape, and line-length scans. Recompute the path/content/binary
receipts and inspect every changed/untracked path before staging. No production
file is currently known to exceed 200 lines, but recompute that fact after the
correction; any test file over 200 lines needs a bounded review. No commit
message has been finalized.

If any source, test, or generated byte changes during correction, invalidate
the affected receipts and refreeze/rebuild/rerun all current Core, Unit,
Integration, and EndToEnd receipts. Do not count Task 16 milestone 5 until the
corrected current tree has fresh focused, public, full managed, and supported
`linux-x64` Native AOT evidence.

### Later full gates

After the grouped correction is accepted, run the deferred full managed/public
and supported `linux-x64` Native AOT gates, including managed-on-native public
Doctor journeys, with exact selected/discovered/executed counts, failures,
skips, warnings, exit codes, artifact hashes, and source-tree identity. Full
Doctor evidence remains pending and milestone 5 remains uncounted until those
gates are fresh.

## C# And Testing Directives

Every C# author or reviewer must independently read these complete current
files before acting and record their current fingerprints:

- `.agents/directives/csharp/_csharp.md` —
  `31045ebcb02d5bfeee8ba9f3112d307b1f72a2186cda618fbf7d22e7d1d90b53`.
- `.agents/directives/csharp/design.md` —
  `76aa8fc7aaaa79d9535998f5557150f3754e3d80520a7a06864b61659373c1a9`.
- `.agents/directives/csharp/style.md` —
  `c3fa9d31575e77fedb103ca397f0ccf10ab7236e6edbef1658c7fe36138457cb`.

The important rules for this boundary are:

- prefer one or two explicit behavioral parameters; four or five is the normal
  maximum; use cohesive immutable records for real data shapes;
- no Args/options/context/service bags, DI, service locator, reflection,
  runtime registry, generic operational engine, or speculative abstraction;
- use explicit finite maps/switches with a discard arm that throws for unnamed
  enum numeric values; do not rely on enum ordinals;
- avoid redundant extraction, allocation, rendering, and dispatch; no `params`
  arrays for fixed two-value coverage merges;
- keep truthful nullable state and warning-clean C#; no null-forgiving `!` as a
  warning workaround;
- keep command-local behavior under the narrowest owning `Shared/<Capability>/`
  scope, with namespaces matching folders; and
- honor formatting and the 200-character guideline, treating 200 lines as a
  review trigger rather than a hard universal law.

Tests cover only Open Forge-owned request formation, diagnosis,
classification, projection, output, state, safety, placement, and public
reachability. Do not test third-party libraries or runtime/OS/tool behavior,
including System.CommandLine, System.Text.Json, YamlDotNet, Markdig, ZIP, BCL,
xUnit, or the test platform. Doctor must retain exactly three simple E2E
journeys; those journeys check owned reachability, streams, exits, JSON result
shape as our output, repeatability/no-write facts, and the two accepted
Extension limitations. Do not add broad CLI behavior tests or third-party
contract tests.

## Standing User Directives

The next Overseer must preserve these accepted operating rules:

- The user-facing Overseer owns project architecture, priority, cross-task
  contracts, worktrees, integration, and final acceptance. Every Task
  Mastermind supervises its own children; children report upward only.
- Use the streamlined assured lane for Tasks 4–6 and 14–20: Mastermind
  Preflight, explicit Gray, explicit Red, one Brilliant Implementer through
  Green/focused verification, then one fresh Mastermind review that absorbs
  ordinary Blue and Purple concerns, followed by one grouped correction.
- Use Luna/max workers liberally for exact shell/build/test execution, menial
  work, artifact inspection, and output summarization when commands and scope
  are literal and cheap to verify. They must not choose commands, product
  meaning, architecture, test meaning, or mutate the same live semantic
  boundary concurrently.
- Progress is dynamic and project-level. Every progress-bearing update must
  render `Active`, `Recently completed`, and `Queued`, with permanent task ID,
  actual name, phase `A/B`, completed milestone `C/D`, current state, and every
  active responsible agent as `name — type (role file) — model / reasoning`.
  Completed tasks remain for exactly two subsequent progress updates, then are
  removed. Mastermind checkpoints use exactly `Done`, `Now`, `Next`, and
  `Blocker`; terse reports should use caveman wording.
- Append a compact progress bar to every progress update, for example
  `Task 16 “Doctor” (phase 4/5; milestone 4/8) — ▰▰▰▰▱▱▱▱`. Bar cells represent
  completed milestones only; disclose internal subphases separately and never
  move the bar backward because an internal phase changed.
- Keep progress values truthful and non-regressing. The stable repository-global
  task horizon is currently none, so do not append `/Y` to task IDs.
- Commit smartly at useful coherent boundaries. Use natural past-tense commit
  subjects in the user's tone and a material body covering Why, candidate
  history, and evidence. Do not use `docs()` or other conventional-prefix
  labels merely as ceremony. Do not rewrite immutable commits.
- No authored `.js`, `.mjs`, or `.cjs` files. TypeScript only for package-manager
  helper scripts.
- The npm/package-manager intent is one main package in
  `src/cli/package-managers/npm/` plus one peer dependency package per target
  platform. Root linking may build the .NET CLI and then link from `src/cli`;
  only small TypeScript helpers are expected. The rejected random
  `/scripts` artifact and `src/cli/root/development-link` are not the intended
  design. No publication or live link/unlink has been authorized.
- Only `.agents` is currently accepted as the payload scope. The future `.apm`
  or similar payload idea is contextual, not current authority.
- Doctor and Status are pluggable through the explicit immutable contributor
  catalogue, not DI or a runtime registry.
- The product is unreleased first-release v1. Internal “v2” wording does not
  create a compatibility obligation. Accept good, non-destructive behavior;
  do not add migration machinery without a new decision.
- Do not push, publish, deploy, globally install, contact remotes, or perform
  destructive recovery. Queue order and acceptance gates remain authoritative.
- The full CLI command-surface architecture/design/test audit is Task 10 after
  Tasks 5, 6, 17, 18, 19, and 20. It may activate conditional Task 21 only
  after the maintainer accepts concrete findings. The user's requested audit is
  PR-level smell detection across the retained `development` CLI branch, not
  broad deep scrubbing or behavior redesign.

## Task 5 Provisional Preparation

Canonical provisional status is Task 5 “Route Remove” (ID 5; phase 0/5;
milestone 0/8). The read-only preflight was performed from the clean original
Task 16 base `bb3a03f64fed6e84def7a6d5e0cefe12f9f75f7d`; it made no source,
contract, branch, worktree, or artifact changes. One `gpt-5.6-sol / xhigh`
Task Mastermind supervised two `gpt-5.6-luna / high` Explorers. This is a
fresh lane plan, not a continuation of the historical Route Remove lane, and
it has no activation artifact. Activation waits for final Doctor integration.
Before any mutation, Gray must freeze the exact first-release-v1 JSON graph,
finding codes, and `next` content.

The durable boundary remains positive-unmanaged-only removal of one ordinary
routed leaf or one complete routed category. Neutral discovery, reference,
generated-navigation, and mutation mechanics may be reused at their nearest
accepted shared scope; Route Remove owns classification, refusal policy,
ordering, result, and lifecycle behavior. It must not import sibling-private
surfaces.

Before activation from a fresh post-Doctor `develop` tip, revalidate Framework
and Extension ownership, the accepted Status/Doctor catalogue and typed views,
root composition/help/serialization, lifecycle/recovery facts, generated
navigation, and affected Status/Doctor evidence. Managed scoped removal remains
out of scope: current Route Remove does not release Framework-managed targets,
generated regions, or lifecycle ownership. `sourceAssetPath` is provenance,
not release authority.

Task 6 may perform read-only preparation only after Task 5 Gray; its dependent
implementation waits for Task 5 acceptance.

## Remaining Queue And Integration Order

After Doctor is accepted and squash-integrated into `develop`, continue exactly:

1. Task 5 “Route Remove”.
2. Task 6 “Root Update”.
3. Task 17 “Extension Update”.
4. Task 18 “Extension Remove”.
5. Task 19 “Repair”.
6. Task 20 “Cleanup”.
7. Task 10 “CLI Command Surface Audit”.
8. Conditional Task 21 “CLI Command Surface Remediation”, only for a concrete
   maintainer-accepted Task 10 finding set.
9. Task 13 “Native linux-x64 CI and Reproducible Artifacts”. Its accepted
   preparation is already integrated; implementation waits for the retained
   command queue, Task 10, and any Task 21 remediation.
10. Task 22 “Final Documentation, Acceptance, and Release”. Queueing it does
    not authorize publication.

Do not activate a dependent implementation on a stale branch. For each future
task, freeze the exact current `develop` commit/tree, use a distinct worktree
and branch, preserve the same streamlined flow, and integrate only after fresh
evidence and accepted review. No safe mutable parallelism remains among the
retained mutation tasks; read-only preparation can be considered only where
its protected boundary is explicit.

## Exact Resume Procedure And Stop Conditions

1. Confirm whether the PC restart left Curie's owned process stopped. It is
   already recorded stopped at this boundary. If a process unexpectedly exists,
   inspect it and stop only the exact owned process before transferring the
   mutable boundary.
2. Verify the Doctor worktree identity, branch, HEAD, tree, clean index, and
   exact 28-path dirty inventory. Preserve the dirty files. Recompute the three
   boundary hashes if any bytes changed during restart.
3. Read the current Task 16 record and compare it to this snapshot. Treat this
   handoff as the sealed transfer map; use the live Task record for any later
   accepted state, not for erasing this boundary.
4. Verify the `e602e2c1` Doctor-branch cleanup and `f8e542a9` develop cleanup
   are equivalent isolated prose changes with different parents/tips, and that
   neither changes the Doctor correction inventory. Do not rebase the dirty
   Doctor tree. Have Sagan revalidate F1–F8 against current content. Curie
   should continue the one grouped correction if continuity is available;
   otherwise record an explicit ownership transfer with the inspected partial
   artifacts before a replacement owner acts.
5. Run the four exact pending commands, then the required format/static/path/
   callable/line scans. Record selected, discovered, executed, failed, skipped,
   warning, exit, artifact, and source-tree facts.
6. Stage only after current content is refrozen and every formerly untracked
   correction file is accounted for. Finish the T16-C2 evidence first, then
   create one natural coherent correction commit; do not amend `b1a49434` or
   `e602e2c1`, and do not manufacture a phase/status commit.
7. Run the deferred full managed/public and supported `linux-x64` Native AOT
   gates. Only then advance milestone 5, finish review/correction/acceptance,
   and integrate into `develop` with exact tree comparison.
8. Only after the T16-C2 commit is accepted, squash/integrate the Doctor result
   onto current `develop` `f8e542a9` carefully, excluding the already-integrated
   equivalent prose cleanup. Start Task 5 only from that accepted integration
   tip.

Stop and return upward before changing public or cross-task meaning, producer or
catalogue ownership, recovery schema strength, lifecycle release authority,
shared serialization, dependency/platform authority, protected paths, or test
meaning. Stop before any remote, destructive, publishing, deployment, global
installation, or dependency-download effect. Stop on any unexpected workspace
change, baseline mismatch, protected-path violation, unresolved maintainer
decision, false-green receipt, zero-test/stale-`--no-build` result, or need for
an unaccepted seam/workaround.

## Copy-Paste Next-Chat Prompt

```text
You are the user-facing Open Forge Overseer. Read and assume
/home/tedy/dev/open-forge/.apm/agents/overseer.agent.md, then read
/home/tedy/dev/open-forge/.agents/loader.md and load the required routes.

The PC was restarted. Resume from the sealed handoff
/home/tedy/dev/open-forge-worktree/doctor-implementation/.agents/memory/working/handoffs/2026-09-05_cli-doctor-restart.md.
Do not edit that sealed handoff. Read the current CLI project-control ledger,
checkpoint, plan, Task 16 Doctor record, Task 5 Route Remove record, and the
complete current C# directive files. Treat the handoff's exact Git state,
immutable commit chain, accepted first-release schema-v1 decisions, T16-R2 F1–F8
packet, receipt classifications, and 28-path dirty inventory as the restart
boundary. Verify live Git/process state before acting.

Continue Task 16 “Doctor” at phase 4/5, milestone 4/8 in
/home/tedy/dev/open-forge-worktree/doctor-implementation on branch
codex/doctor-implementation. Curie was safely interrupted with no owned process,
no staging, and no correction commit; no agent process remains active at the
handoff boundary. Re-establish Sagan as the designated Task Mastermind and
Curie as the Brilliant Implementer only after live inspection. Preserve all
dirty C# files. Verify that `e602e2c1` and `f8e542a9` are equivalent isolated
prose cleanups, then do not rebase the dirty Doctor tree. Sagan owns
revalidation and Curie owns the one grouped T16-C2 correction when continuity
remains; do not duplicate or discard work because of silence. Revalidate F1–F8,
run the four exact pending commands in the handoff, rerun invalidated evidence,
run format/static/protected-path/callable/line scans, refreeze, and finish the
T16-C2 evidence before creating one natural coherent commit only after all
formerly untracked files are accounted for.
Do not amend immutable b1a49434. Then run full managed/public and supported
linux-x64 Native AOT gates before counting milestone 5. Only after acceptance,
squash/integrate the Doctor result onto current develop `f8e542a9`, excluding
the equivalent isolated prose cleanup already present there. No second full
review wave is authorized unless a distinct named risk requires it.

Always report dynamic Active/Recently completed/Queued progress with permanent
task IDs, actual task names, phase A/B, completed milestone C/D, and every
active agent name/type/file, model, and reasoning. Append a compact milestone
progress bar to every progress update; name internal subphases separately and
never regress the bar. Mastermind reports use
exactly Done/Now/Next/Blocker and caveman wording. Keep the streamlined flow:
Mastermind preflight, Gray, Red, one Brilliant Implementer, one whole-task
review, one grouped correction. Use Luna/max for literal menial/build/test/
summarization work only. C# authors and reviewers must read and fingerprint
_csharp.md, design.md, and style.md.
Tests cover only Open Forge-owned behavior; keep exactly three simple Doctor E2E
journeys; never test third-party/runtime/OS/ZIP/serializer/framework behavior.
No JS/MJS/CJS, no DI/runtime registry, no v2/compatibility machinery, no remote
or destructive actions, and no publication. After accepted Doctor integration,
continue the queue Task 5 -> 6 -> 17 -> 18 -> 19 -> 20 -> 10 -> conditional 21
-> 13 -> 22.
```

## Exit

This Handoff ends when the current Doctor correction has been revalidated,
freshly evidenced, coherently committed, accepted, and integrated into
`develop`, or when an explicit new Handoff records a later transfer. Keep this
file for historical restart provenance; never rewrite it to make later state
look current.
