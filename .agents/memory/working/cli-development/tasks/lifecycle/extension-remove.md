---
open-forge:
  description: Implement Extension removal with preserved user content and recovery integrity
  tags: [Memory, Working, CLI, Task, Extension, Remove, Lifecycle, Contextual]
---

# Task 18: Extension Remove

## Task State

- State: Acceptance-ready at phase 5 of 5, milestone 8 of 8. The
  streamlined-assured Preflight, Gray, Red, coherent Green, full verification,
  holistic review `T18-R1`, grouped correction `T18-C1`, and final acceptance
  are complete. The accepted feature candidate is commit
  `9326a9214acd58bd7018b021074813f1dccb3f59`, tree
  `623335f16faaa214e001fccd0dd1740c845ab066`. Integration remains pending under
  Overseer ownership; no integration commit exists yet.
- Permanent mapping: Task 18 “Extension Remove” in the
  [project control ledger](../../project-control.md).
- Queue relation: Task 17 “Extension Update” is complete and its completion
  grace is consumed. The active command horizon is Task 18, then Task 19
  “Repair”, then Task 20 “Cleanup”. Workspace Libraries and Extensions
  Evolution follow as queued last-stage improvements.
- Parent: [Lifecycle Commands](_lifecycle.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/extension/remove/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/extension/remove/behavior.md).

The accepted clean activation base is commit
`5cabb10de31cc522e80e82f1ac2ada49e60929c0`, tree
`fb16fed368e82848a9ae79ed11eef2f33012f63a`. The base is immutable. This task
record is working authority and has no self-referential content hash.

## Frozen Execution Capsule

The five phases and eight milestones are fixed:

1. Preflight and activation.
2. Explicit Gray callable and public-shape review.
3. Explicit Red behavior review.
4. One coherent production and focused verification pass.
5. Fresh holistic review, at most one grouped correction, final acceptance,
   and integration.

The milestone ledger is: 1 Preflight and activation; 2 Gray; 3 Red; 4 coherent
production; 5 focused, public, full managed, and supported `linux-x64` Native
AOT verification; 6 fresh holistic review (`T18-R1`); 7 one grouped correction
or documented no-op (`T18-C1`); and 8 acceptance. There is
no council. The task is now acceptance-ready at phase 5/5 and milestone 8/8.
The accepted feature candidate still requires Overseer-owned integration.

The workflow used one Mastermind supervising one Implementer. Gray and Red had
independent owners, and the Implementer owned the coherent production change,
focused evidence, and grouped correction. `T18-R1` was the only holistic
review; `T18-C1` was the only grouped correction slot. No parallel
implementation branch altered the same production authority.

## Accepted Preflight Freeze

- Authority fingerprints: the Extension Remove Interface and Behavior SHA-256
  values are
  `2a355909a828b5272e2f1006f6da1409034ba61539e7e127cc7cab16832b0c28`
  and
  `3c91095c605c0444eaf1c183a538a6ddb3029e7d36de34d5f1e1774106aead57`.
  The C# root, design, and style Directive SHA-256 values are
  `31045ebcb02d5bfeee8ba9f3112d307b1f72a2186cda618fbf7d22e7d1d90b53`,
  `76aa8fc7aaaa79d9535998f5557150f3754e3d80520a7a06864b61659373c1a9`,
  and
  `c3fa9d31575e77fedb103ca397f0ccf10ab7236e6edbef1658c7fe36138457cb`.
- Architecture readiness: reuse the accepted lifecycle package/path ownership,
  Extension identity, dependency, physical-path, generated-navigation,
  workspace-lock, mutation, source-generated serialization, and recovery
  capabilities. Keep request selection, retained-dependent refusal,
  shared/final-owner classification, Keep-as-unmanaged/Delete policy, complete
  planning, result, presentation, and orchestration local to Extension Remove.
  Do not import another command's private `Shared/**` implementation or add a
  generic lifecycle engine.
- Behavior classes: terminal help/version; direct, wizard, automatic, JSON, and
  dry-run requests; exact workspace and stable-ID resolution; trusted or
  unavailable lifecycle; source-independent ownership; repeated-remove proof;
  dependency and route-host blockers; shared, unchanged-final-owner,
  changed-final-owner, missing, and unsafe path classes; same-request prune;
  generated navigation; one complete preflighted plan; lock and volatile-fact
  revalidation; recovery preparation, application, verification, lifecycle
  publication, bundle disposition, and interruption; all seven result statuses;
  and human/JSON presentation from one result.
- Expected production paths:
  `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Remove/**`; the narrow
  Extension binding, composer, root-registration, and help neighborhood; and the
  obsolete Doctor enum/wire mapping described below. Expected tests mirror the
  Remove Unit and Integration paths, add exactly three simple published Remove
  journeys, and retain exactly three Doctor public journeys.
- Direct integration neighborhood: Extension lifecycle readers/writers,
  package/path ownership and identity, generated-navigation formation,
  physical-path containment, workspace locking, mutation validation and
  application, recovery preparation/deletion, source-generated JSON, and
  static Extension/root composition.
- Protected paths and meaning: other commands' private `Shared/**`; Framework
  lifecycle meaning and files; package sources; unknown, unowned,
  another-manager-owned, and user content; public shared result coordinates;
  dependency, platform, project, build, package, release, and generated-source
  authority; legacy CLI and npm/package-manager paths; and every path outside a
  complete accepted plan. A neighboring path changes only when accepted Remove
  meaning directly requires it and the Mastermind records the expansion.
- Historical provisional disposition: provisional commit
  `98b0577d5a5e9a4cdcd382a9d032a91be1eddec5`, tree
  `dcd127b28362b597feef74035c37b4acbe1a28d7`, contains implementation commit
  `446dfad91d5b181fb08ff0a70f74e5a18ca12a71`, tree
  `f09e377e3c7f18f02b0504fa6cb254bdb82cc53b`, on stale base
  `0d269b7a326377edc173036bc56f36e199eb6e5c`, tree
  `8863d17629e0af6665bd8100f55b5b63da41035b`. Its request, dependency,
  ownership, and prune model ideas are evidence only. Its eight-file partial
  shape has no composition, integration, public, recovery, or AOT proof and is
  not transplantable as a whole.
- Baseline correction: intermediate commit
  `083f5ed0a5396dedecde52cd9e4aab8991bccf8e` had tree
  `15fa11eafd3e868f24ef178b3c9fa307ac97de84`; the stale
  `63f5b22c9c73dd9aaf2044c88401e5b91638e143` value is Task 17's accepted
  product tree, not that intermediate tree. The clean activation base above
  supersedes both as Task 18's implementation base.
- Parallelization: read-only discovery and exact mechanical evidence may
  overlap. Gray and Red preparation may overlap only where their files and
  semantic ownership are disjoint. Shared facts, callable/public shape,
  serialization, static composition, semantic Green, correction, and
  integration remain serialized under the one Implementer.
- Resolved task-local decisions: Gray froze the exact callable and public
  shapes, and Red froze the focused test counts and final bounded fixture
  inventory. No unresolved project decision blocks Green.

## Accepted Gray Freeze

- Identity: the cumulative Gray tip is commit
  `eb5169a1583bb10090000db126f4808cb7d3f8f4`, tree
  `fce5f02c9bee8904f6aa293e8132b58cd6ac0919`. It comprises the callable surface
  from commit `8ec3cf6836a804208c044d6730c8b4fd2600e020` and the Doctor Unit
  oracle correction from commit `eb5169a1583bb10090000db126f4808cb7d3f8f4`.
- Surface: exactly 25 production and composition paths are frozen for the
  callable/public shape, plus one Doctor Unit oracle path. The newline-sorted
  25-path manifest SHA-256 is
  `5e3c07f4f21a8691536aaa8473893363377b91b60e7d00a30c0a7036fdbbb4e4`.
- Evidence: locked restore and the full six-project Release build passed with
  zero warnings and errors. Exact Doctor mapping Unit evidence passed `1/1`.
  Formatting, static, protected-path, callable-shape, prohibited-pattern, and
  line-length checks passed. The Doctor catalogue remains at 108 finding kinds
  and 19 Extension kinds, with exactly three Doctor public EndToEnd journeys.
- Red boundary: three disjoint Red authors now freeze Unit, Integration, and
  exactly three simple public EndToEnd journeys. All Red tests are expected to
  fail only at explicit unsupported Extension Remove seams. Red does not change
  the accepted Gray production or Doctor oracle paths.

## Accepted Red Freeze

- Identity: immutable Red commit `51328f51162650655d07fbd55c8c99f35b712e4c`,
  tree `89a194debe07496f9f99a9a660f2683756f328a4`, parent commit
  `44834203d2f7d1b4072a80f03e680f0d17f22bd3`, parent tree
  `61e1b29bd7013132302df3c42a0e4fe4547daf58`.
- Surface: exactly 19 test-only paths are frozen: 11 Unit + 4 Integration + 1
  Remove E2E + 3 required stale Extension group-help oracle neighbors.
  The newline-sorted path manifest SHA-256 is
  `58772eac01cb1e0050b79eaf61c71b554d2192271cfe20dd0726e3632e51959f`; the
  content manifest SHA-256 is
  `c7f747b48026ed05c614165b9514c9bcfe8a95ee5cf01e4cae5f6d552c74fdc0`.
- Evidence: Unit executed 51 cases with 50 passes and 1 intentional unsupported
  seam. Integration executed 21 cases with 3 passes and 18 failures at the same
  intentional unsupported seam. Remove EndToEnd selected exactly 3 cases: help
  passed, and the two published-process cases intentionally failed at that same
  unavailable operation. The retained Doctor EndToEnd selection remains exactly
  3 cases. Both required Create help-neighbor selections passed `3/3`.
  The full six-project Release build passed with `0` warnings and `0` errors.
  Formatting, static, protected-path, callable-shape, prohibited-pattern,
  durable-host-path, line-length, and accounting checks passed.
- Fresh specialist `test-evidence` and `csharp-conformance` reviews returned
  `TOPIC_GAP` because their mandated immutable-object tool was unavailable.
  Neither specialist reviewed the snapshot or found defects. Acceptance rests
  on three successful cross-lane content reviews, exact focused
  execution/failure-seam proof, clean build/static/path/accounting checks, and
  Overseer direct immutable Git, path, and key-test inspection.
- Green was owned coherently by Curie as the sole Brilliant Implementer under
  Sagan VI supervision, preserving the exact Red and frozen contracts. No
  parallel Green production owner altered that authority.

## Accepted Green, Review, And Final Verification

- The coherent production implementation was accepted at commit
  `758acc383a5795acfd6a33b65e2094abc7aa7760`. The focused retained-route and
  collision regression evidence was committed separately at
  `7742ecc1d71a9bcf3db5f3afbdd13bcc138f0912`.
- Review correction evidence was committed at
  `9e654119e9bd7ac4f0d6131d3bcf1ae4045af342`. Two reviewer-proposed Unit
  oracles were then withdrawn as false positives and restored to their accepted
  semantics at commit `c2906e71d77aa3184079ea7a174b4c02b5db1d8f`.
- `T18-R1` accepted three material corrections: an invalid or unparseable
  semantic target uses the conservative exact-byte fallback; a malformed or
  unavailable affected generated region blocks removal; and the four reported
  nested or chained conditionals use ordered decisions or exhaustive switches.
  The trusted-absent repeat behavior and unknown deletion residual-path
  behavior were intentionally unchanged after their findings were withdrawn.
  Execution also disproved the proposed invalid-UTF-8 retained-source defect,
  so it required no production change.
- The grouped `T18-C1` correction produced the final accepted feature candidate
  at commit `9326a9214acd58bd7018b021074813f1dccb3f59`, tree
  `623335f16faaa214e001fccd0dd1740c845ab066`. A fresh immutable review passed
  with no material finding.
- The final warning-free six-project Release build passed. Full Unit passed
  `1909/1909`, full Integration passed `1026/1026`, focused Extension Remove
  Unit passed `55/55`, focused Extension Remove Integration passed `27/27`, and
  managed public EndToEnd passed `187/187`. Supported `linux-x64` Native AOT
  Integration passed `1026/1026`, and Native AOT EndToEnd passed `187/187`.
  Every selection had zero failures and skips. The public inventory remains
  exactly three Extension Remove and three Doctor EndToEnd journeys.
- The final `linux-x64` executable SHA-256 values are
  `c7d4f7ca7232a4301520de8e1d82c2531c01a2dce4860af25184028b57da3927`
  for the CLI,
  `766c454e000723ee2397cc7a7ccf0e82ebae700ca1fad7a9a7a74a87e066be27`
  for Integration, and
  `73eab2641aa962948fbae57623b17ad8532c53321f214fed33350de68bf741b4`
  for EndToEnd.
- Formatting, static, protected-path, callable-shape, prohibited-pattern,
  durable-path, line-length, exact-inventory, and clean-Git checks passed. The
  feature is acceptance-ready, but integration remains a separate
  Overseer-owned boundary.

The decisive evidence ladder is focused Unit and Integration evidence, exactly
three Remove public journeys and the retained three Doctor public journeys,
then formatting and static/protected checks, a warning-free full managed gate,
managed EndToEnd against the native root, and supported `linux-x64` Native AOT
Integration and EndToEnd. Focused counts are frozen in the Accepted Red Freeze.
The established full commands are:

- `dotnet restore OpenForge.Cli.slnx --locked-mode --nologo`
- `dotnet build OpenForge.Cli.slnx -c Release --no-restore --nologo -p:OpenForgeSkipDevelopmentPublish=true`
- `artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests`
- `artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests`
- `artifacts/bin/OpenForge.Cli.EndToEndTests/release/OpenForge.Cli.EndToEndTests`
- `dotnet publish src/cli/root/OpenForge.Cli/OpenForge.Cli.csproj --configuration Release --runtime linux-x64 --no-restore --output artifacts/publish/linux-x64/open-forge -p:OpenForgeSkipDevelopmentPublish=true --nologo`
- `dotnet publish src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --runtime linux-x64 --no-restore --output artifacts/publish/linux-x64/integration -p:OpenForgeSkipDevelopmentPublish=true --nologo`
- `dotnet publish src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --configuration Release --runtime linux-x64 --no-restore --output artifacts/publish/linux-x64/end-to-end -p:OpenForgeSkipDevelopmentPublish=true --nologo`,
  followed by direct execution of
  `artifacts/publish/linux-x64/integration/OpenForge.Cli.IntegrationTests` and
  `artifacts/publish/linux-x64/end-to-end/OpenForge.Cli.EndToEndTests`
- `dotnet build src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj -c Release --no-restore --nologo -p:OpenForgeSkipDevelopmentPublish=true -p:OpenForgeEndToEndTargetRuntimeIdentifier=linux-x64`,
  followed by managed EndToEnd execution against the native root.

Focused selection uses `dotnet test --project` against the Unit and Integration
projects with `-c Release --no-build --filter-trait
"Feature=extension-remove" --minimum-expected-tests <frozen-Red-count>`.
Public selection uses the EndToEnd project with `-c Release --no-build
--filter-class "*PublishedExtensionRemoveProcessTests"
--minimum-expected-tests 3`; the retained Doctor selection replaces the class
with `*PublishedDoctorProcessTests` and keeps the exact minimum `3`. Zero-test,
stale `--no-build`, skipped, warning-bearing, partially loaded, or wrong-scope
negative receipts prove no gate.

## Expected Outcome

`extension remove` removes only lifecycle-managed content for selected packages,
preserves user and other package content, updates generated navigation and
lifecycle state, and exposes exact recovery.

## Architecture

- Observe package dependencies, managed identities, user divergence, shared
  files/regions, generated projections, and lifecycle before planning.
- `ExtensionRemovePlan` names exact package order and file/region effects.
- Reuse lifecycle, ownership, atomic, and external recovery-bundle primitives.
  Keep selection, dependency refusal, removal policy, findings, and result local.

## Doctor Boundary

Task 17 already closed the accepted producer-owned
`extension.bridge-registration` observation. Extension Remove has no Doctor
producer obligation and does not own an installed-manifest observation universe.
It must not add an installed-manifest scan, a new Doctor finding, or a coverage
horizon. The current Doctor catalogue and Extension lifecycle diagnosis remain
authoritative.

The clean activation base still carries the now-unreleased
`ExtensionUnmanagedLikeContent` enum member, its wire mapping, and the associated
109-count mapping expectation. Gray and Green must remove only that orphaned
executable shape so the implementation conforms to the accepted 108-kind / 19-
Extension catalogue. This narrow reconciliation creates no finding, producer,
scan, compatibility path, or lifecycle meaning.

The command does not inspect or adopt package-source manifests or legacy
`open-forge.extensions.json` as removal authority. No broad `.agents` recursion,
payload/path/byte resemblance, Framework bridge, static CLI composition,
dependency injection, runtime registry, compatibility machinery, lifecycle
schema change, or storage/scanner behavior may be introduced.

No dependency change, JavaScript/MJS/CJS implementation, native bridge,
reflection path, unsafe code, fake filesystem, remote action, publication, or
release action belongs to this task.

## Public Evidence

Retain exactly three simple public EndToEnd journeys:

1. Terminal help succeeds without inspecting the workspace or writing anything.
2. Default removal is source-independent, preserves unowned content, and a
   trusted repeat is an exact no-op.
3. JSON preview and apply use the same request shape, with explicit prune
   preview/apply semantics and recovery-safe output.

Focused unit and integration evidence may cover the accepted selection,
dependency, ownership, shared-target, divergence, lifecycle, dry-run, lock,
recovery, generated-navigation, cancellation, and no-write rules. It must test
Open Forge-owned behavior only. Retain exactly three Doctor public journeys;
Doctor remains read-only and is not expanded by this task.

## Evidence

Cover one/many packages, dependency blockers, shared targets, user-modified
content, missing managed content, lifecycle errors, dry run, lock race, bundle
preparation and retention after partial failure/cancellation, generated
navigation, package isolation, second run, process, and AOT. Run formatting,
static, protected-path, callable-shape, prohibited-pattern, and line-length
checks before acceptance. Run fresh full managed/public and supported
`linux-x64` Native AOT gates after focused evidence and again as required by
the final acceptance workflow.

## Stop Conditions

Stop before deleting unowned content, cascading dependency removal without
contract authorization, changing Framework lifecycle, or removing recovery
evidence before verified completion.
