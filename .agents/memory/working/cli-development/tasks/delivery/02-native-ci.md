---
open-forge:
  description: Prove native CI and reproducible artifact collection for all six accepted platforms
  tags: [Memory, Working, CLI, Task, Distribution, NativeAOT, CI, Contextual]
---

# Task 13: Native CI and Reproducible Artifacts

## Task State

- State: Queued after Task 21, Task 27, and Task 7's new ARM64 package
  expansion. The maintainer approved CI/artifacts for Linux, macOS, and Windows
  on x64 and ARM64 on 2026-09-09.
- Permanent mapping: Task 13 “Native CI and Reproducible Artifacts” in
  the [project control ledger](../../project-control.md).
- Parent: [CLI Delivery](_delivery.md).
- Historical preparation profile: The opt-in
  [Supervised Luna Preparation Trial](../../../../../workflows/supervised-luna-preparation-trial.md).
- Exact preparation base: local `develop` commit
  `c8051478127ab9204ca31b1d86ef358dd982207f`, tree
  `ccc762115238c4317b3b35963be18edd38be2609`.
- Lane: `codex/native-ci-preparation` in
  `open-forge-worktree/native-ci-preparation`.
- Current phase and milestone: phase 1 of 3, milestone 2 of 6.
- Current-state suffix: M2 preparation, review, and integration complete;
  the earlier Linux-only preparation must be refrozen for the six-target
  scope and current source before M3. No new implementation milestone is earned.
- Preparation role: One experimental Luna/max Task Mastermind supervised at
  most three bounded read-only evidence workers, followed by one fresh Sol/xhigh
  whole-task review. That allocation is historical. Later semantic implementation
  and fresh review use Astra/high under current Overseer direction.

The project control ledger defines permanent identity, queue state, worktree
mapping, and integration state. This Task record defines the accepted horizon,
preparation result, evidence, trial measurements, and later revalidation.

## Expected Outcome

CI proves native build and smoke, packed installation and invocation, checksums,
and bounded artifact collection for each accepted target. It records exact
source, SDK, dependency and artifact identities without publishing to a registry.
Byte-for-byte reproducibility requires an independent repeated-build comparison;
one checksum manifest proves identity and integrity only.

## Matrix And Current Authority

| Operating system | Accepted native targets    |
| ---------------- | -------------------------- |
| Linux, glibc     | `linux-x64`, `linux-arm64` |
| macOS            | `osx-x64`, `osx-arm64`     |
| Windows          | `win-x64`, `win-arm64`     |

Use a matching native host for each target. Revalidate current official runner,
SDK/toolchain, action and Native AOT support before freezing concrete jobs.
An emulated execution, cross-build, source property or existing workflow row is
not native proof. Missing matching-host evidence remains visibly incomplete.

Retain the accepted split into a build matrix, a test matrix, and a separate
manual/on-demand artifact publication workflow. The latter stages and packs
accepted artifacts, proves isolated installed-launcher invocation, writes
checksums and collects bounded artifacts; it does not publish npm packages or
create a product release. Routine workflow file names and runner choices within
this accepted graph are implementation decisions for the Overseer.

D1 owns native build/smoke, package installation/invocation, checksums and
bounded artifact receipts for all six targets. Task 7 owns the package graph,
staging/packing implementation and package-owned journeys. Full managed Unit,
Integration and public suites, plus Native AOT Integration/public evidence,
remain Architecture/A1 evidence consumed by D1 and Task 22. The test matrix may
schedule these required gates without collapsing their evidence ownership.

At activation, refreeze the exact candidate and package graph after Task 7,
map every target to its native host and commands, and preserve phase 1/3,
milestone 2/6 as completed historical preparation. Revalidation does not itself
complete M3. Additional RIDs, libc variants, support-floor matrices, signatures,
SBOM, provenance and OIDC remain outside this accepted expansion.

## Historical Preparation Boundary

The following preparation, inventory, feasibility, recipe and review sections
retain the earlier Linux-only D1 and x64-package decisions and receipts. Their
ARM-undecided wording and future execution instructions are historical, not
current authority. The six-target matrix above supersedes those scope limits.
No old command recipe is ready to run without the activation refreeze.

## Preparation Trial Boundary

The preparation lane was authorized to inspect the exact committed CI workflow,
repository-root build and package commands, accepted Task 7 package graph,
artifact paths, checksum requirements, and local evidence already present. It
could update only this Task record and directly required generated navigation.

The lane had to record:

- the current workflow matrix and its difference from accepted D1;
- the exact future native build, direct smoke, packed install and invocation,
  checksum, and bounded artifact steps;
- inputs or paths that remain unstable until command acceptance;
- protected package, product, support-floor, and publication meaning;
- the exact facts and commands that must be revalidated before CI mutation.

Protected surfaces include `.github/workflows/`, CLI production and tests,
package-manager source and manifests, root project and dependency files, public
documentation, release configuration, and generated runtime projections. This
preparation does not edit CI, execute remote jobs, publish, install globally,
or claim D1 acceptance.

## Preparation Result

The exact semantic preparation base is commit
`c8051478127ab9204ca31b1d86ef358dd982207f`, tree
`ccc762115238c4317b3b35963be18edd38be2609`. The activation candidate is commit
`3c904a23ed6735ca91bb3ca2810156b01a25e5b9`, tree
`f2a780b8cb1da6ac680b7d7d7586d9732cb20cf6`, with the semantic base as its
parent. The activation comparison changed only project-control and workflow
records, the Task 13 record, and workflow navigation records. Concretely, the
changed paths are `.agents/memory/working/cli-development/project-control.md`,
this Task record, `.agents/workflows/_workflows.md`, and the added
`.agents/workflows/supervised-luna-preparation-trial.md`. `.github/workflows/cli.yml`,
the root build controls, and the npm package sources remain byte-identical
across those two snapshots. This lane has not changed any protected
implementation or delivery surface.

The accepted preparation candidate is commit
`e76896965e1cce51b7295f899499c5a7d0cce252`, tree
`c204c17b4bdda87bf43a479879d6ebcf87472283`, with activation commit
`3c904a23ed6735ca91bb3ca2810156b01a25e5b9`, tree
`f2a780b8cb1da6ac680b7d7d7586d9732cb20cf6`, as its direct parent. Its direct
parent comparison changes only this Task record. The commit containing the
project-ledger convergence integrates that accepted semantic delta onto local
`develop` parent `de40d550c00e51f55fe7e8b5d39297c450f721e9`, tree
`e6040c4996eb8af5375979b54a9bafc198438a34`.

### Current CI Inventory

The current [CLI workflow](../../../../../../.github/workflows/cli.yml) runs for
relevant pull requests and manual dispatch. One `native` job expands to six
RIDs: `win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `osx-x64`, and
`osx-arm64`. It selects `ubuntu-latest` for `linux-x64`, installs the floating
`10.0.x` SDK line, restores the solution with `NuGet.Config`, verifies format,
builds Release, runs managed Unit and Integration evidence, publishes the root
CLI, runs managed EndToEnd evidence, publishes and executes native Integration
and EndToEnd projects, and uploads the native root executable for each RID.

That workflow currently differs from accepted D1 in four material ways:

- D1 owns one native `linux-x64` delivery lane. The other five matrix entries
  are inventory only and are outside the current support and D1 evidence
  boundary. Their presence does not establish ARM support or package release
  readiness.
- The workflow has no accepted package staging, `npm pack`, isolated offline
  install, or installed-launcher invocation step. Task 7's one package journey
  proves package reachability and executable placement, but deliberately does
  not invoke the CLI.
- The workflow uploads native executables but emits no checksum manifest for
  the native binary, packed packages, or a deterministic source archive.
- The workflow records neither the resolved SDK/toolchain and dependency graph
  nor one bounded artifact manifest tied to the candidate commit and
  informational version. Its `10.0.x` setup input must be reconciled with the
  repository's `global.json` (`10.0.100` with feature-band roll-forward).

The existing managed and native test publishes are separately recorded
Architecture/A1 evidence. They do not become D1 requirements merely because
the shared workflow schedules them, and no unsupported RID is a current
shipping claim.

The accepted future CI shape is minimal: a build matrix, a test matrix, and a
separate manual or on-demand publish workflow. The current combined `native`
job is only the recorded starting point. Its matrix contents, native runner
labels, package steps, and artifact evidence must be revalidated when M3 is
authorized; a current ARM entry cannot be carried forward as a support
decision.

### Accepted x64 Expansion And ARM Feasibility

The permanent Task 7 “npm Package Manager Release and Local Linking” record is
already reopened with a separate platform-expansion horizon. That current
record, rather than the earlier candidate recommendation preserved in the review
receipt below, defines phase 1 of 4 with 0 of 7 milestones complete; its
preparation is complete. It keeps the historical 8/8 closeout intact, consumes
and revalidates this Task's target evidence in M1 and M2, applies the coherent
x64 package expansion in M3 and M4, runs focused owned-package evidence in M5,
and closes through one review and integration record in M6 and M7. The package
evidence asserts only Open Forge package placement, launcher reachability,
argument/process forwarding, and completion; it does not test npm, Node, the
operating system, third-party libraries, or CLI command behavior.

The ARM candidates were assessed as feasibility only:

| Candidate     | Native AOT buildability                                                                                                                                                                            | Native runner and testability                                                                                                                                                          | npm release shape and incremental cost                                                                                                                                                               | Disposition                                                                                                        |
| ------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------ |
| `linux-arm64` | .NET Native AOT supports the RID. Cross-architecture x64-to-Arm64 publication needs the target linker, runtime objects, and compatible native dependencies; a cross-build is not sufficient proof. | GitHub lists `ubuntu-24.04-arm`, so a native Linux run is available in principle. The glibc floor, SDK toolchain, publish, and smoke still need a real runner receipt.                 | One thin package with `os: ["linux"]`, `cpu: ["arm64"]`, and `libc: "glibc"`; plus one model/staging row, native artifact, package tarball, checksum, install/reachability journey, and runner lane. | Technically feasible candidate; not simple or acceptance-ready, and not accepted.                                  |
| `osx-arm64`   | .NET Native AOT supports the RID. Cross-architecture publication can use the macOS/Xcode toolchain, but cross-OS publication remains unsupported.                                                  | GitHub lists `macos-latest`/current macOS Arm64 labels. Native execution is possible, but hosted Arm64 limitations and action compatibility require image/toolchain revalidation.      | One thin package with `os: ["darwin"]` and `cpu: ["arm64"]`, with the same additional artifact, checksum, and owned install/reachability evidence.                                                   | Technically feasible candidate; the native path is plausible but not yet simple or proven, and not accepted.       |
| `win-arm64`   | .NET Native AOT supports the RID. Publication requires the appropriate Visual Studio C++ Arm64 tools; cross-architecture publication must use a matching Windows target toolchain.                 | GitHub lists `windows-11-arm` and `windows-11-vs2026-arm`. Native execution is possible in principle, but the installed C++ toolchain and image identity require a spike-time receipt. | One thin package with `os: ["win32"]` and `cpu: ["arm64"]`, plus one model/staging row, native artifact, checksum, install/reachability journey, and runner lane.                                    | Technically feasible candidate; toolchain/image verification makes it non-baseline-simple, and it is not accepted. |

These are not three metadata-only additions. Together they would add three
platform package templates/manifests, three main-package optional dependency
edges, three runtime/staging mappings, three native publication and smoke
lanes, three package tarballs, and corresponding checksums and owned
installation evidence. The macOS x64 package is a separate accepted baseline
gap and must be solved even if every ARM candidate is declined. No candidate
may be released merely because its RID is present in `Directory.Build.props` or
the current workflow.

The feasibility assessment uses the current official sources for the later
spike: [Native AOT cross-compilation](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/cross-compile),
[Native AOT deployment and supported targets](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/),
the [.NET RID catalog](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog),
[GitHub-hosted runner labels and limitations](https://docs.github.com/en/actions/reference/runners/github-hosted-runners),
and [npm platform package fields](https://docs.npmjs.com/cli/v11/configuring-npm/package-json/).
They were read on 2026-09-03 and must be revalidated before an ARM scope
decision or any runner/package mutation. These sources establish feasibility
constraints, not repository acceptance or successful build evidence.

### Future Execution Capsule

The later implementation owner must revalidate every input below on the then
current clean candidate. These commands are a preparation recipe, not evidence
run by this task and not permission to mutate the protected workflow now.

| Boundary                         | Future command or change                                                                                                                                                                                                                                                                                                                                                            | Required result                                                                                                                                                                                                                                               |
| -------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Candidate and host identity      | `git status --short --branch`; `git rev-parse --verify HEAD`; `git rev-parse HEAD^{tree}`; `uname -m`; `ldd --version`; `dotnet --version`; `node --version`; `npm --version`                                                                                                                                                                                                       | Clean native Linux x64 runner, exact commit/tree, resolved toolchains, and glibc facts are recorded.                                                                                                                                                          |
| Audited restore and dependencies | `dotnet restore OpenForge.Cli.slnx --configfile NuGet.Config`; `dotnet package list --project OpenForge.Cli.slnx --include-transitive --format json --no-restore`                                                                                                                                                                                                                   | Restore uses the repository source and audit settings. The complete resolved graph is captured with no warning-bearing or partial result.                                                                                                                     |
| Native publication               | `gitSha="$(git rev-parse --verify HEAD)"`; `version="0.0.0-dev.sha-${gitSha}"`; `dotnet publish src/cli/root/OpenForge.Cli/OpenForge.Cli.csproj --configuration Release --runtime linux-x64 --self-contained true --no-restore --output artifacts/publish/linux-x64/open-forge -p:OpenForgeSkipDevelopmentPublish=true -p:OpenForgeCliInformationalVersion="${version}"`            | A fresh `artifacts/publish/linux-x64/open-forge/OpenForge.Cli` is produced from the exact clean candidate, with its version marker and informational version recorded.                                                                                        |
| Native smoke                     | `nativeArtifact="artifacts/publish/linux-x64/open-forge/OpenForge.Cli"`; `test -x "${nativeArtifact}"`; `"${nativeArtifact}" --version`                                                                                                                                                                                                                                             | The Open Forge executable starts natively and returns the expected candidate version. This is the owned smoke, not a test of the runtime or SDK.                                                                                                              |
| Accepted package staging         | `node src/cli/package-managers/npm/manage.ts stage artifacts linux-x64 artifacts/publish/linux-x64/open-forge/OpenForge.Cli local "${gitSha}"`                                                                                                                                                                                                                                      | Task 7's main package and Linux x64 optional package are staged under ignored artifacts with synchronized `0.0.0-dev.sha-<full lowercase Git SHA>` versions. The Windows package remains part of the accepted graph but is not staged for this Linux journey. |
| Fresh bounded output roots       | `runRoot="artifacts/task13/linux-x64/${gitSha}"`; `test ! -e "${runRoot}"`; `test ! -e artifacts/publish/linux-x64/open-forge`; `test ! -e artifacts/npm/stage/linux-x64`; `mkdir -p "${runRoot}/pack/main" "${runRoot}/pack/linux" "${runRoot}/delivery"`                                                                                                                          | The candidate refuses stale task-owned output roots instead of deleting them, then creates the exact pack and delivery directories required by later redirections. The publish and stage commands must create their own guarded output paths.                 |
| Packed install                   | `npm pack artifacts/npm/stage/linux-x64/open-forge --pack-destination "${runRoot}/pack/main" --offline --ignore-scripts --no-save --no-package-lock --no-audit --no-fund`; repeat for `open-forge-linux-x64` with `"${runRoot}/pack/linux"`                                                                                                                                         | Exactly the staged main and Linux platform packages are packed. Capture each path only after asserting that its destination contains exactly one `.tgz`.                                                                                                      |
| Tarball path capture             | `test "$(find "${runRoot}/pack/main" -maxdepth 1 -type f -name '*.tgz' \| wc -l)" -eq 1`; `test "$(find "${runRoot}/pack/linux" -maxdepth 1 -type f -name '*.tgz' \| wc -l)" -eq 1`; `mainTarball="$(find "${runRoot}/pack/main" -maxdepth 1 -type f -name '*.tgz' -print -quit)"`; `linuxTarball="$(find "${runRoot}/pack/linux" -maxdepth 1 -type f -name '*.tgz' -print -quit)"` | Fresh absolute-to-workspace tarball paths are captured and no stale or duplicate package is accepted.                                                                                                                                                         |
| Isolated invocation              | `installRoot="$(mktemp -d)"`; `npm install --offline --ignore-scripts --no-save --no-package-lock --no-audit --no-fund --prefix "${installRoot}" "${mainTarball}" "${linuxTarball}"`; `"${installRoot}/node_modules/.bin/open-forge" --version`                                                                                                                                     | Only the Open Forge package boundary is asserted: both packages install, the launcher resolves the installed Linux payload, and the installed command returns the expected version. Do not test npm internals or third-party behavior.                        |
| Source and checksum identity     | `git archive --format=tar --prefix="open-forge-${gitSha}/" "${gitSha}" > "${runRoot}/delivery/open-forge-${gitSha}.tar"`; `sha256sum "${nativeArtifact}" "${mainTarball}" "${linuxTarball}" "${runRoot}/delivery/open-forge-${gitSha}.tar" > "${runRoot}/delivery/SHA256SUMS"`; `sha256sum --check "${runRoot}/delivery/SHA256SUMS"`                                                | The manifest binds the exact native binary, packed artifacts, and source archive. The check proves traceable integrity and identity; it is not byte-for-byte reproducibility evidence.                                                                        |
| Bounded receipt                  | Record runner identity, SDK and dependency facts, commit/tree, informational and package versions, exact artifact paths and hashes, command statuses, selected/discovered/executed counts where a test runner is used, failures, skips, warnings, and limits                                                                                                                        | A reviewable receipt distinguishes D1-owned package/native proof from separately consumed A1 evidence. No publication or credentials are involved.                                                                                                            |

The `mainTarball`, `linuxTarball`, and `nativeArtifact` shell names in the
recipe are resolved from the fresh outputs of the immediately preceding steps.
The implementation must not reuse an older ignored artifact or use
`--no-build` against an artifact whose input identity is not recorded.

This Linux D1 journey intentionally does not stage the accepted `osx-x64` or
`win-x64` packages; those package journeys belong to the reopened Task 7
horizon. It also does not stage any ARM package.

### Change Matrix And Ownership

The future D1 mutation is limited to the accepted CI responsibilities and
their bounded evidence paths. The exact workflow file names are not frozen by
this preparation record; M3 must return a project change request if the
retained authority does not identify them. The three responsibilities are:

- build matrix: compile the accepted candidate and collect exact SDK, runtime,
  dependency, commit, and bounded build-artifact identity;
- test matrix: run only the accepted focused managed/native evidence selected
  by the final task authority, with no third-party or broad CLI-contract
  assertions; and
- manual/on-demand publish workflow: perform the Linux D1 native/package
  stage, pack, isolated install/reachability, archive, checksum, and bounded
  artifact upload steps only when explicitly dispatched.

These responsibilities must not be collapsed back into the current combined
`native` job, must not make publication automatic, and must not use the
existing ARM rows as implicit release support. Any matrix or workflow-path
change needs a fresh support-boundary decision.

The accepted `src/cli/package-managers/npm/` source, package manifests, root
`package.json`, `global.json`, `Directory.Build.props`, `Directory.Packages.props`,
and `NuGet.Config` are inputs to revalidate, not preparation mutation targets.
No production source, test project, package contract, public documentation,
release configuration, generated runtime projection, or project-control file
is needed for M3. A mismatch in those sources returns to the Overseer as a
project change request rather than receiving an improvised local substitute.

### Accepted Authority Split

The accepted x64 direction is aligned across the current Distribution, Delivery,
Task 7, Task 13, and Task 22 records. Task 7 owns the accepted x64 package graph,
platform-package expansion, and applicable-host package journeys. Task 13
retains only the later Linux D1 native build and smoke, checksums, and bounded
artifact collection after retained commands, Task 10, and accepted Task 21
remediation. Task 22 owns final acceptance and separately authorized atomic
publication of the complete x64 graph. ARM remains undecided and outside all
three task scopes.

### Stale Risks And Revalidation Triggers

- The preparation base predates Route Move integration and the later command,
  architecture-remediation, and release prerequisites. Every source fact and
  command path must be reread after those inputs settle; this record does not
  authorize starting M3 from the preparation commit.
- Task 7's package contract is accepted, but later package-source, launcher,
  manifest, or version changes invalidate staging and packed-install evidence.
  The installed journey must use a fresh native artifact and the current full
  candidate SHA.
- `ubuntu-latest` and `10.0.x` are labels rather than complete reproducibility
  identities. Revalidate native `x86_64`, glibc, resolved SDK, Node/npm, action
  versions, and package graph on the actual runner. Do not present an emulated
  or cross-compiled run as native execution proof.
- Ignored `artifacts/` output is disposable and can outlive its inputs. Start
  the D1 output directories fresh, record the producing commit/tree, and reject
  stale or warning-bearing binaries, tarballs, manifests, and receipts.
- The current package EndToEnd test uses an inert owned payload and checks
  installation reachability only. D1 must add one real native packed-launcher
  invocation while asserting only Open Forge's resulting behavior; it must not
  grow into npm, Node, or CLI command-contract coverage.
- Restore may contact NuGet and CI may upload bounded artifacts, but this
  preparation made no such external effects. Publication, registry contact,
  credentials, signatures, SBOM, provenance, OIDC, and support-floor expansion
  remain outside D1.

Before M3, revalidate the prerequisite ledger state, exact clean ancestry,
workflow and package-source identities, runner facts, restore/audit result, fresh
native version smoke, package graph and synchronized versions, offline packed
invocation, and checksum verification. Any changed input invalidates the
dependent downstream evidence from that earliest boundary.

## Whole-Task Review And Disposition

A fresh Sol/xhigh whole-task review inspected this record, the complete routed
Open Forge context, the activation parent/tree, local links, protected paths,
and the official feasibility sources. It found no blocking issue. One grouped
correction accepted all eight findings:

| Review ID | Finding                                                                                          | Disposition                                                                                                                                                                    |
| --------- | ------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| R1        | The Task record could not own queue state while saying `Queued` against an `ACTIVE` ledger row.  | Accepted and fixed: the record remains active for review; the Overseer must perform any ledger handback.                                                                       |
| R2        | M2 could not be complete while whole-task review and correction were pending.                    | Accepted and fixed: review is now dispositioned and the record is at phase 1/3, milestone 2/6.                                                                                 |
| R3        | The accepted x64 expansion needed an explicit authority and ownership handoff beyond Task 13.    | Accepted and recorded as the deferred project-change request naming Architecture, release, Delivery, Task 7, Task 13, and the ledger; the current authority split resolves it. |
| R4        | The future change matrix contradicted the accepted build/test/manual-publish workflow split.     | Accepted and fixed: the three responsibilities are now separate and exact future file paths remain an authorization boundary.                                                  |
| R5        | The recommended Task 7 reopening named phases but not six milestones.                            | Accepted and fixed: the recommendation now names M1 through M6 and remains non-authoritative until Task 7 is reopened.                                                         |
| R6        | The future recipe did not guard fresh pack/delivery directories or capture unique tarball paths. | Accepted and fixed: guarded output-root initialization and one-tarball path capture are now explicit.                                                                          |
| R7        | One checksum pass proved identity, not byte-for-byte reproducibility.                            | Accepted and fixed: the current outcome is traceable/checksummed collection; stronger reproducibility requires an independent repeat comparison.                               |
| R8        | Activation provenance omitted the Task record from its changed-record description.               | Accepted and fixed: Task, project-control, and workflow navigation records are named.                                                                                          |

No finding was rejected, duplicated, or treated as preference. The reviewer
verified that only the owned Task record is modified, protected-path diff is
empty, all local links resolve, and `git diff --check` passes.

Integration completed R1's ledger handback. It also converged R5's earlier
non-authoritative recommendation with the already accepted current Task 7
phase 1/4 and 0/7 horizon. The other review dispositions remain unchanged.

## Execution Horizon

1. Phase 1, preparation.
   - M1 inventories the current CI, package, artifact, checksum, and accepted
     authority boundary on the exact base.
   - M2 freezes a future execution capsule, stale-data risks, revalidation
     triggers, and the experimental workflow evidence. Task 13 then returns to
     the queue until its implementation prerequisite is satisfied.
2. Phase 2, implementation after Task 21, Task 27, Task 7 ARM64 expansion,
   and refreeze of the six-target source, package, runner and evidence boundary.
   - M3 applies the native CI and artifact collection change for all six targets.
   - M4 passes focused local/static validation and the available CI-equivalent
     journey without remote publication.
3. Phase 3, acceptance.
   - M5 passes matching native-runner, packed install/invocation, checksum, and
     bounded artifact evidence for every accepted target on the final candidate.
   - M6 records the final evidence, residual limits, integration identity, and
     trial comparison.

The phase and milestone counts do not regress. Preparation evidence is not
implementation or acceptance evidence, and every prepared fact is revalidated
before M3.

## Reproducibility And Artifacts

For each accepted target, record native host, SDK, RID, commit, informational
version, dependency graph, source
archive identity, binary/package hashes, package inventory, and smoke/packed
results. This preparation defines traceable and checksummed artifact collection;
it does not call one production run byte-for-byte reproducible. A later release
gate must independently repeat production from the same clean candidate and
compare the resulting identities before using that stronger claim. Upload only
bounded release-candidate artifacts. Run publishes sequentially when output is
shared.

## Stop Conditions

Stop on emulated evidence presented as native, warning-bearing publish, skipped
required behavior, runner-specific uncommitted patch, root-path C# assumption,
automatic publication, a reproducibility claim without an independent repeat
comparison, or any broader RID/support-floor claim without explicit acceptance.

## Historical Trial Measurements

Record elapsed preparation time, child count, handoffs, missing-context or model
fallbacks, accepted and rejected Sol/xhigh review findings, grouped correction
count, facts invalidated before M3, later revalidation cost, gate failures,
integration conflict, and any defect found after acceptance. The initial
read-only candidate comparison required a fallback evidence child because its
requested Luna runtime was unavailable; that child result is useful input but
does not count as successful Luna-only execution evidence.

This preparation now has three bounded read-only evidence-child packets: the
current CI/workflow inventory, the package/artifact/checksum authority
inventory, and the ARM feasibility inventory. The requested Luna/max route was
attempted once and rejected by the runtime, so the packets are fallback-model
evidence and none counts as Luna-only evidence. The ARM packet independently
confirmed that all three candidates are possible only subject to native
toolchain/runner receipts, while the accepted release boundary remains the
three x64 targets above. The whole-task review accepted R1–R8, one grouped
correction was applied, and no finding was rejected or deferred. The accepted
authority split is now recorded in the current delivery records.
